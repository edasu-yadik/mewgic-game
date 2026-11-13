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
        public readonly string StoreOrderId;
        internal readonly string JournalTransactionId;
        public readonly int Quantity;
        public readonly bool Permanent;

        public StoreProof(string platform, string sku, string transactionId, string receipt, int quantity, bool permanent,
            string storeOrderId = null, string journalTransactionId = null)
        {
            Platform = platform;
            Sku = sku;
            TransactionId = transactionId;
            Receipt = receipt;
            Quantity = quantity;
            Permanent = permanent;
            StoreOrderId = storeOrderId;
            JournalTransactionId = journalTransactionId ?? StorePayments.JournalId(platform, transactionId);
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
        readonly Dictionary<string, double> confirming = new(StringComparer.Ordinal);
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
                !ValidTransactionId(proof.JournalTransactionId) ||
                string.IsNullOrWhiteSpace(proof.Sku) || proof.Sku != package.PlatformId(platform) ||
                proof.Permanent != package.Permanent || proof.Quantity < 1 || proof.Quantity > 100 ||
                (restoration && !package.Permanent)) return;
            string flight = proof.JournalTransactionId;
            if ((confirming.TryGetValue(flight, out double retryAfter) &&
                retryAfter > Time.realtimeSinceStartupAsDouble) || !processing.Add(flight)) return;
            try
            {
                var entry = ledger.StorePurchase(platform, flight) ?? Entry(proof.Sku, flight, proof.Quantity);
                if (entry.productId != proof.Sku || entry.quantity != proof.Quantity) return;
                if (!string.IsNullOrEmpty(proof.StoreOrderId)) entry.storeOrderId = proof.StoreOrderId;
                if (entry.confirmation == StoreConfirmationStatus.Confirmed)
                {
                    SetState(id, StorePaymentState.Success);
                    return;
                }
                SetState(id, StorePaymentState.Processing);
                if (entry.reward == StoreRewardStatus.Delivered)
                {
                    Confirm(id, entry, confirm, waitForConfirmation);
                    return;
                }
                entry.purchase = StorePurchaseStatus.Pending;
                if (!Save(entry) || string.IsNullOrWhiteSpace(proof.Receipt))
                {
                    SetState(id, StorePaymentState.Waiting);
                    return;
                }
                var result = await verifier.VerifyAsync(proof);
                if (result.State != StoreVerificationState.Verified || !result.Sandbox ||
                    string.IsNullOrWhiteSpace(result.CanonicalId) || result.CanonicalId.Length > 512)
                {
                    entry.purchase = result.State == StoreVerificationState.Pending
                        ? StorePurchaseStatus.Pending : StorePurchaseStatus.Rejected;
                    Save(entry);
                    SetState(id, result.State == StoreVerificationState.Pending ? StorePaymentState.Waiting : StorePaymentState.Failed);
                    return;
                }
                // A verifier must return the same canonical ID for all replays of this platform transaction.
                string key = Hash(platform + ":sandbox:" + result.CanonicalId);
                entry.purchase = StorePurchaseStatus.Verified;
                entry.reward = StoreRewardStatus.Delivered;
                entry.updatedUtc = DateTimeOffset.UtcNow.ToString("O");
                var delivered = ledger.DeliverStorePackage(package, key, proof.Quantity, entry);
                if (delivered is StoreDeliveryResult.Applied or StoreDeliveryResult.AlreadyApplied)
                    Confirm(id, entry, confirm, waitForConfirmation);
                else
                {
                    entry.reward = StoreRewardStatus.Retry;
                    Save(entry);
                    SetState(id, delivered == StoreDeliveryResult.Retry ? StorePaymentState.Waiting : StorePaymentState.Failed);
                }
            }
            catch (Exception) { SetState(id, StorePaymentState.Waiting); }
            finally { processing.Remove(flight); }
        }

        StorePurchaseEntry Entry(string sku, string transactionId, int quantity) =>
            new() { platform = platform, transactionId = transactionId, productId = sku, quantity = quantity };

        bool Save(StorePurchaseEntry entry)
        {
            entry.updatedUtc = DateTimeOffset.UtcNow.ToString("O");
            return ledger.SaveStorePurchase(entry);
        }

        static bool ValidTransactionId(string id) =>
            !string.IsNullOrWhiteSpace(id) && id.Length <= 512 && !id.Any(char.IsControl);

        // Google IAP uses the purchase token as TransactionID; never persist that token.
        internal static string JournalId(string platform, string sdkId) =>
            !ValidTransactionId(sdkId) ? null : platform == "google"
                ? "sha256:" + Hash(sdkId) : sdkId;

        void Confirm(string id, StorePurchaseEntry entry, Action confirm, bool waitForConfirmation)
        {
            entry.confirmation = StoreConfirmationStatus.Waiting;
            if (!Save(entry)) { SetState(id, StorePaymentState.Waiting); return; }
            SetState(id, StorePaymentState.Waiting);
            if (waitForConfirmation) confirming[entry.transactionId] = Time.realtimeSinceStartupAsDouble + 30;
            try { confirm?.Invoke(); }
            catch (Exception)
            {
                Confirmation(id, entry.transactionId, false);
                return;
            }
            if (!waitForConfirmation) Confirmation(id, entry.transactionId, true);
        }

        internal void Confirmation(string id, string transactionId, bool success)
        {
            if (!packages.TryGetValue(id ?? "", out var package) || !ValidTransactionId(transactionId)) return;
            var entry = ledger.StorePurchase(platform, transactionId);
            if (entry == null || entry.productId != package.PlatformId(platform) ||
                entry.reward != StoreRewardStatus.Delivered) return;
            confirming.Remove(transactionId);
            if (entry.confirmation == StoreConfirmationStatus.Confirmed) return;
            entry.confirmation = success ? StoreConfirmationStatus.Confirmed : StoreConfirmationStatus.Retry;
            SetState(id, Save(entry) && success ? StorePaymentState.Success : StorePaymentState.Waiting);
        }

        void Observe(string id, string transactionId, int quantity, StorePurchaseStatus status)
        {
            if (!packages.TryGetValue(id ?? "", out var package) || !ValidTransactionId(transactionId) ||
                ledger.StorePurchase(platform, transactionId) != null) return;
            var entry = Entry(package.PlatformId(platform), transactionId, quantity);
            entry.purchase = status;
            Save(entry);
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

        // Do not open a native billing sheet until store validation and account delivery are integrated.
        public static Task<bool> ConfigureNativeSandbox(IStorePurchaseVerifier verifier, Guid sandboxAccount) =>
            Task.FromResult(false);

#if UNITY_EDITOR
        internal static async Task<bool> ConnectSandboxStore(StoreController controller, StorePackage[] mapped,
            ProgressLedger ledger, string platform, IStorePurchaseVerifier verifier, Guid sandboxAccount)
        {
            if (!GameplayProgress.IsTestProfile) return false;
            var definitions = ProductDefinitions(mapped, platform);
            var retryOrders = new Dictionary<string, PendingOrder>(StringComparer.Ordinal);
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
            Action retry = null;
            var session = new StorePayments(mapped, ledger, platform, verifier, controller.PurchaseProduct,
                () =>
                {
                    if (controller.GetConnectionState() == ConnectionState.Connected)
                    {
                        retry?.Invoke();
                        FetchProducts();
                    }
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
            string ProductId(Order order) => order?.CartOrdered?.Items().FirstOrDefault()?.Product.definition.id;
            string TransactionId(Order order) => JournalId(platform, order?.Info?.TransactionID);
            async void Deliver(Order order, bool restore)
            {
                var items = order.CartOrdered.Items();
                if (items.Count != 1) return;
                var item = items[0];
                if (!session.packages.TryGetValue(item.Product.definition.id, out var package)) return;
                string receipt = platform == "apple" ? order.Info.Apple?.jwsRepresentation : order.Info.Receipt;
                string transaction = TransactionId(order);
                if (transaction == null) { session.SetState(package.id, StorePaymentState.Waiting); return; }
                if (order is PendingOrder pendingOrder) retryOrders[transaction] = pendingOrder;
                var proof = new StoreProof(platform, item.Product.definition.storeSpecificId, order.Info.TransactionID,
                    receipt, item.Quantity, package.Permanent, order.Info.Google?.OrderId, transaction);
                Action confirm = order is PendingOrder pending ? () => controller.ConfirmPurchase(pending) : null;
                await session.Accept(package.id, proof, confirm, restore, waitForConfirmation: confirm != null);
                if (ledger.StorePurchase(platform, transaction)?.confirmation == StoreConfirmationStatus.Confirmed)
                    retryOrders.Remove(transaction);
            }
            retry = () =>
            {
                foreach (var order in retryOrders.Values.ToArray()) Deliver(order, false);
            };
            void Observe(Order order, StorePurchaseStatus purchase, StorePaymentState state)
            {
                string id = ProductId(order);
                session.Observe(id, TransactionId(order), order.CartOrdered.Items().FirstOrDefault()?.Quantity ?? 0, purchase);
                session.SetState(id, state);
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
                session.confirming.Clear();
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
                retryOrders.Clear();
                session.confirming.Clear();
                FetchPurchases();
            };
            controller.OnPurchaseDeferred += order => Observe(order, StorePurchaseStatus.Deferred, StorePaymentState.Waiting);
            controller.OnPurchaseFailed += order =>
            {
                bool cancelled = order.FailureReason == PurchaseFailureReason.UserCancelled;
                Observe(order, cancelled ? StorePurchaseStatus.Cancelled : StorePurchaseStatus.Failed,
                    cancelled ? StorePaymentState.Cancelled : StorePaymentState.Failed);
            };
            controller.OnPurchaseConfirmed += order =>
            {
                bool finished = order is ConfirmedOrder || platform == "apple" &&
                    order is FailedOrder failed && failed.FailureReason == PurchaseFailureReason.DuplicateTransaction;
                session.Confirmation(ProductId(order), TransactionId(order), finished);
            };
            controller.OnPurchasePending += order => Deliver(order, false);
            controller.OnPurchasesFetched += orders =>
            {
                fetchingPurchases = false;
                if (controller.GetConnectionState() != ConnectionState.Connected) return;
                foreach (var order in orders.DeferredOrders) Observe(order, StorePurchaseStatus.Deferred, StorePaymentState.Waiting);
                foreach (var order in orders.PendingOrders) Deliver(order, false);
                foreach (var order in orders.ConfirmedOrders)
                {
                    session.Confirmation(ProductId(order), TransactionId(order), true);
                    Deliver(order, true);
                }
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
#endif
    }
}
