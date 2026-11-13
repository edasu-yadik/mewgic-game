using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mewgic.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Mewgic
{
    public enum StorePaymentState { Ready, Processing, Waiting, Success, Cancelled, Failed }
    public enum StoreVerificationState { Verified, Pending, Rejected }

    public readonly struct StoreProof
    {
        public readonly string Platform, Sku, TransactionId, Receipt;
        public readonly int Quantity;
        public readonly bool Permanent;

        public StoreProof(string platform, string sku, string transactionId, string receipt, int quantity, bool permanent)
        {
            Platform = platform;
            Sku = sku;
            TransactionId = transactionId;
            Receipt = receipt;
            Quantity = quantity;
            Permanent = permanent;
        }
    }

    public readonly struct StoreVerification
    {
        public readonly StoreVerificationState State;
        public readonly string CanonicalId;
        public readonly bool Sandbox;

        public StoreVerification(StoreVerificationState state, string canonicalId, bool sandbox)
        {
            State = state;
            CanonicalId = canonicalId;
            Sandbox = sandbox;
        }
    }

    // Implementations authenticate the current player and validate SKU, quantity, payment state and ownership.
    public interface IStorePurchaseVerifier
    {
        Task<StoreVerification> VerifyAsync(StoreProof proof);
    }

    public sealed class StorePayments
    {
        readonly Dictionary<string, StorePackage> packages;
        readonly Dictionary<string, ProductMetadata> metadata = new(StringComparer.Ordinal);
        readonly Dictionary<string, double> offerEnds = new(StringComparer.Ordinal);
        readonly Dictionary<string, StorePaymentState> states = new(StringComparer.Ordinal);
        readonly HashSet<string> processing = new(StringComparer.Ordinal);
        readonly IStorePurchaseVerifier verifier;
        readonly ProgressLedger ledger;
        readonly string platform;
        readonly Action<string> start;
        readonly Action refresh;
        readonly Action<Action<bool>> restorePermanent;
        bool restored;
        public event Action<string> Changed;
        public static StorePayments Current { get; private set; }
        public ProductMetadata Metadata(string id) => id != null && metadata.TryGetValue(id, out var value) ? value : null;
        public string Price(string id) => Metadata(id)?.localizedPriceString;
        public StorePaymentState State(string id) => states.TryGetValue(id, out var state) ? state : StorePaymentState.Ready;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Current = null;

        internal StorePayments(IEnumerable<StorePackage> packages, ProgressLedger ledger, string platform,
            IStorePurchaseVerifier verifier, Action<string> start, Action refresh = null, Action<Action<bool>> restorePermanent = null)
        {
            this.packages = packages.ToDictionary(package => package.id, StringComparer.Ordinal);
            this.ledger = ledger;
            this.platform = platform;
            this.verifier = verifier;
            this.start = start;
            this.refresh = refresh;
            this.restorePermanent = restorePermanent;
            if (platform is not ("apple" or "google") || this.packages.Values.Any(package => !package.Valid) ||
                this.packages.Values.GroupBy(package => package.PlatformId(platform), StringComparer.Ordinal)
                    .Any(group => !ValidPlatformId(platform, group.Key) || group.Count() > 1))
                throw new ArgumentException("Mağaza ürün eşlemesi geçersiz.");
        }

        static bool ValidPlatformId(string platform, string id) => id != null &&
            Regex.IsMatch(id, platform == "apple" ? @"\A[A-Za-z0-9._]{1,100}\z" : @"\A[a-z0-9][a-z0-9._]{0,149}\z");

        internal static List<ProductDefinition> ProductDefinitions(IEnumerable<StorePackage> packages, string platform) =>
            packages.Where(package => !string.IsNullOrWhiteSpace(package.PlatformId(platform)))
                .Select(package => new ProductDefinition(package.id, package.PlatformId(platform),
                    package.Permanent ? ProductType.NonConsumable : ProductType.Consumable)).ToList();

        internal void SetMetadata(string id, ProductMetadata value)
        {
            if (id == null || !packages.ContainsKey(id)) return;
            if (string.IsNullOrWhiteSpace(value?.localizedPriceString) ||
                string.IsNullOrWhiteSpace(value.isoCurrencyCode) || value.localizedPrice <= 0)
                metadata.Remove(id);
            else metadata[id] = value;
            Changed?.Invoke(id);
        }

        internal void SetState(string id, StorePaymentState state)
        {
            if (id == null || !packages.ContainsKey(id)) return;
            states[id] = state;
            Changed?.Invoke(id);
        }

        // Only a store or authenticated offer service should supply an observed server time and a real deadline.
        internal void SetVerifiedOfferEnd(string id, DateTimeOffset observedUtc, DateTimeOffset endsUtc)
        {
            if (!packages.ContainsKey(id)) return;
            offerEnds[id] = Time.realtimeSinceStartupAsDouble + (endsUtc - observedUtc).TotalSeconds;
            Changed?.Invoke(id);
        }

        internal bool HasOfferDeadlines => offerEnds.Count > 0;

        internal TimeSpan? OfferRemaining(string id) => offerEnds.TryGetValue(id, out double end)
            ? TimeSpan.FromSeconds(Math.Max(0, end - Time.realtimeSinceStartupAsDouble)) : null;

        internal bool Buy(string id)
        {
            if (!restored || Price(id) == null || OfferRemaining(id) == TimeSpan.Zero || State(id) is StorePaymentState.Processing or StorePaymentState.Waiting ||
                states.Values.Contains(StorePaymentState.Processing)) return false;
            SetState(id, StorePaymentState.Processing);
            try { start(id); return true; }
            catch (Exception) { SetState(id, StorePaymentState.Failed); return false; }
        }

        internal void RefreshPurchases() => refresh?.Invoke();
        public bool RestorePermanentPurchases(Action<bool> completed)
        {
            if (restorePermanent == null || !packages.Values.Any(package => package.Permanent)) return false;
            restorePermanent(completed);
            return true;
        }
        internal void Ready() => restored = true;

        internal async Task Accept(string id, StoreProof proof, Action confirm, bool restoration = false, bool waitForConfirmation = false)
        {
            if (!packages.TryGetValue(id, out var package) || proof.Platform != platform ||
                (!restoration && (string.IsNullOrWhiteSpace(proof.TransactionId) || string.IsNullOrWhiteSpace(proof.Receipt))) ||
                string.IsNullOrWhiteSpace(proof.Sku) || proof.Sku != package.PlatformId(platform) ||
                proof.Permanent != package.Permanent || proof.Quantity < 1 || proof.Quantity > 100 ||
                (restoration && !package.Permanent)) return;
            string flight = id + ":" + proof.TransactionId;
            if (!processing.Add(flight)) return;
            SetState(id, StorePaymentState.Processing);
            try
            {
                var result = await verifier.VerifyAsync(proof);
                if (result.State != StoreVerificationState.Verified || !result.Sandbox ||
                    string.IsNullOrWhiteSpace(result.CanonicalId) || result.CanonicalId.Length > 512)
                {
                    SetState(id, result.State == StoreVerificationState.Pending ? StorePaymentState.Waiting : StorePaymentState.Failed);
                    return;
                }
                // A verifier must return the same canonical ID for all replays of this platform transaction.
                string key = Hash(platform + ":sandbox:" + result.CanonicalId);
                var delivered = ledger.DeliverStorePackage(package, key, proof.Quantity);
                if (delivered is StoreDeliveryResult.Applied or StoreDeliveryResult.AlreadyApplied)
                {
                    if (waitForConfirmation) SetState(id, StorePaymentState.Waiting);
                    confirm?.Invoke();
                    if (!waitForConfirmation) SetState(id, StorePaymentState.Success);
                }
                else SetState(id, delivered == StoreDeliveryResult.Retry ? StorePaymentState.Waiting : StorePaymentState.Failed);
            }
            catch (Exception) { SetState(id, StorePaymentState.Waiting); }
            finally { processing.Remove(flight); }
        }

        static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }

#if UNITY_EDITOR
        internal static void UseLocalTest(StorePayments session)
        {
            if (!GameplayProgress.IsTestProfile) throw new InvalidOperationException("İzole QA profili gerekli.");
            Current = session;
        }
#endif

        public static Task<bool> ConfigureNativeSandbox(IStorePurchaseVerifier verifier, Guid sandboxAccount)
        {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID) && DEVELOPMENT_BUILD
            return ConnectNativeSandbox(verifier, sandboxAccount);
#else
            return Task.FromResult(false);
#endif
        }

        static async Task<bool> ConnectNativeSandbox(IStorePurchaseVerifier verifier, Guid sandboxAccount)
        {
            if (verifier == null || sandboxAccount == Guid.Empty || Current != null) return false;
            var catalog = MagicShopPanel.LoadCatalogue();
            string platform = Application.platform == RuntimePlatform.IPhonePlayer ? "apple" : "google";
            var mapped = catalog.Packages.Where(package => !string.IsNullOrWhiteSpace(package.PlatformId(platform))).ToArray();
            if (mapped.Length == 0) return false;
            // Platform sandbox receipts stay in a dedicated save even on a device with real progress.
            ProgressLedger ledger;
            try { ledger = GameplayProgress.Open("Mewgic.IapSandbox.v1." + sandboxAccount.ToString("N")); }
            catch (Exception) { return false; }
            var controller = UnityIAPServices.StoreController(platform == "apple" ? AppleAppStore.Name : GooglePlay.Name);
            return await ConnectSandboxStore(controller, mapped, ledger, platform, verifier, sandboxAccount);
        }

        internal static async Task<bool> ConnectSandboxStore(StoreController controller, StorePackage[] mapped,
            ProgressLedger ledger, string platform, IStorePurchaseVerifier verifier, Guid sandboxAccount)
        {
            var definitions = ProductDefinitions(mapped, platform);
            bool fetchingProducts = false, fetchingPurchases = false;
            void FetchProducts()
            {
                if (fetchingProducts || controller.GetConnectionState() != ConnectionState.Connected) return;
                fetchingProducts = true;
                controller.FetchProducts(definitions, new NoRetriesPolicy());
            }
            void FetchPurchases()
            {
                if (fetchingPurchases || controller.GetConnectionState() != ConnectionState.Connected) return;
                fetchingPurchases = true;
                controller.FetchPurchases();
            }
            var session = new StorePayments(mapped, ledger, platform, verifier, controller.PurchaseProduct,
                () =>
                {
                    if (controller.GetConnectionState() == ConnectionState.Connected)
                        FetchProducts();
                    else if (controller.GetConnectionState() is ConnectionState.Disconnected or ConnectionState.Unavailable)
                        _ = controller.Connect();
                }, completed =>
                {
                    if (platform == "apple")
                        controller.RestoreTransactions((ok, _) =>
                        {
                            if (ok) FetchPurchases();
                            completed?.Invoke(ok);
                        });
                    else
                    {
                        FetchPurchases();
                        completed?.Invoke(true);
                    }
                });
            Current = session;
            string ProductId(Order order) => order.CartOrdered.Items().FirstOrDefault()?.Product.definition.id;
            async void Deliver(Order order, bool restore)
            {
                var items = order.CartOrdered.Items();
                if (items.Count != 1) return;
                var item = items[0];
                if (!session.packages.TryGetValue(item.Product.definition.id, out var package)) return;
                string receipt = platform == "apple" ? order.Info.Apple?.jwsRepresentation : order.Info.Receipt;
                var proof = new StoreProof(platform, item.Product.definition.storeSpecificId, order.Info.TransactionID,
                    receipt, item.Quantity, package.Permanent);
                Action confirm = order is PendingOrder pending ? () => controller.ConfirmPurchase(pending) : null;
                await session.Accept(package.id, proof, confirm, restore, waitForConfirmation: confirm != null);
            }
            controller.ProcessPendingOrdersOnPurchasesFetched(false);
            controller.OnProductsFetched += products =>
            {
                if (controller.GetConnectionState() != ConnectionState.Connected) return;
                fetchingProducts = false;
                foreach (var package in mapped)
                {
                    var product = products.Find(candidate => candidate.availableToPurchase &&
                        candidate.definition.id == package.id &&
                        candidate.definition.storeSpecificId == package.PlatformId(platform));
                    session.SetMetadata(package.id, product?.metadata);
                }
                FetchPurchases();
            };
            controller.OnProductsFetchFailed += failure =>
            {
                fetchingProducts = false;
                foreach (var product in failure.FailedFetchProducts)
                    session.SetMetadata(product.id, null);
            };
            controller.OnStoreDisconnected += _ =>
            {
                session.restored = false;
                fetchingProducts = fetchingPurchases = false;
                foreach (var package in mapped)
                {
                    if (session.State(package.id) == StorePaymentState.Processing)
                        session.SetState(package.id, StorePaymentState.Waiting);
                    session.SetMetadata(package.id, null);
                }
            };
            controller.OnStoreConnected += () =>
            {
                if (platform == "apple") controller.AppleStoreExtendedService?.SetAppAccountToken(sandboxAccount);
                else controller.GooglePlayStoreExtendedService?.SetObfuscatedAccountId(sandboxAccount.ToString("N"));
                FetchProducts();
            };
            controller.OnAuthAccountChanged += () =>
            {
                session.restored = false;
                FetchPurchases();
            };
            controller.OnPurchaseDeferred += order => session.SetState(ProductId(order), StorePaymentState.Waiting);
            controller.OnPurchaseFailed += order => session.SetState(ProductId(order),
                order.FailureReason == PurchaseFailureReason.UserCancelled ? StorePaymentState.Cancelled : StorePaymentState.Failed);
            controller.OnPurchaseConfirmed += order =>
            {
                session.SetState(ProductId(order), order is FailedOrder ? StorePaymentState.Waiting : StorePaymentState.Success);
            };
            controller.OnPurchasePending += order => Deliver(order, false);
            controller.OnPurchasesFetched += orders =>
            {
                fetchingPurchases = false;
                if (controller.GetConnectionState() != ConnectionState.Connected) return;
                foreach (var order in orders.DeferredOrders) session.SetState(ProductId(order), StorePaymentState.Waiting);
                foreach (var order in orders.PendingOrders) Deliver(order, false);
                foreach (var order in orders.ConfirmedOrders) Deliver(order, true);
                session.Ready();
            };
            controller.OnPurchasesFetchFailed += _ =>
            {
                fetchingPurchases = false;
                session.restored = false;
            };
            try
            {
                await controller.Connect();
                return controller.GetConnectionState() == ConnectionState.Connected;
            }
            catch (Exception) { return false; }
        }
    }
}
