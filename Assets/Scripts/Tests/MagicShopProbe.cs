#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Characters;
using Mewgic.Map;
using Mewgic.Localization;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class MagicShopProbe : MonoBehaviour
    {
        [SerializeField] string output;
        [SerializeField] bool costumes;
        string Output => string.IsNullOrEmpty(output) ? "docs/ProductionReports/ShopVisualAudit/" : output;
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo Current = typeof(GameplayProgress).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        static MagicShopProbe active;
        readonly List<string> checks = new(), errors = new();
        readonly string key = "Mewgic.Tests.Shop." + Guid.NewGuid().ToString("N");
        Dictionary<string, string> original;
        bool runInBackground;
        ProgressLedger Ledger => GameplayProgress.Current;
        EconomyConfig Config => EconomyConfig.Current;

        [MenuItem("Mewgic/QA/Run Magic Shop")]
        public static void Begin() => Begin(null);

        public static void Begin(string output) => Begin(output, false);

        [MenuItem("Mewgic/QA/Run Costume Integration")]
        public static void BeginCostumes() => Begin("Library/CostumeIntegration/", true);

        static void Begin(string output, bool costumes)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Önce açık sahneyi kaydet.");
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            var probe = new GameObject("Magic Shop QA").AddComponent<MagicShopProbe>();
            probe.output = output;
            probe.costumes = costumes;
            Size(1600);
            EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
            EditorApplication.isPlaying = true;
        }

        static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor")
            .GetMethod("Size").Invoke(null, new object[] { height });
        static Dictionary<string, string> PlayerSave() => Suffixes.ToDictionary(s => s,
            s => PlayerPrefs.HasKey(GameplayProgress.StorageKey + s)
                ? PlayerPrefs.GetString(GameplayProgress.StorageKey + s) : null);

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            runInBackground = Application.runInBackground;
            Application.runInBackground = true;
            original = PlayerSave();
            var data = new ProgressData();
            data.presentation.coins = 600;
            data.presentation.gems = costumes ? 15000 : 2700;
            data.presentation.energy = 4;
            data.presentation.manualLocale = "tr";
            data.presentation.reduceMotion = true;
            data.presentation.motionPreferenceSet = true;
            var old = JObject.Parse(JsonUtility.ToJson(data));
            old.Remove("shop");
            PlayerPrefs.SetString(key, old.ToString());
            PlayerPrefs.Save();
            GameplayProgress.UseTestProfile();
            Current.SetValue(null, GameplayProgress.Open(key));
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived += Log;
        }

        void Log(string message, string stack, LogType type)
        {
            if (type is LogType.Error or LogType.Exception or LogType.Assert) errors.Add(message);
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.runInBackground = runInBackground;
            Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
            foreach (var suffix in Suffixes) PlayerPrefs.DeleteKey(key + suffix);
            PlayerPrefs.Save();
        }

        void Check(bool okay, string name)
        {
            if (!okay) throw new InvalidOperationException(name);
            checks.Add(name);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(Output);
            string failure = null;
            var stack = new Stack<IEnumerator>();
            stack.Push(costumes ? RunCostumes() : Run());
            while (stack.Count > 0)
            {
                bool more; object item;
                try { more = stack.Peek().MoveNext(); item = more ? stack.Peek().Current : null; }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!more) stack.Pop();
                else if (item is IEnumerator child && item is not CustomYieldInstruction) stack.Push(child);
                else yield return item;
            }
            var end = PlayerSave();
            bool intact = original.All(pair => end[pair.Key] == pair.Value);
            File.WriteAllText(Output + "PLAY_MODE.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = intact && failure == null && errors.Count == 0, editor = Application.unityVersion,
                count = checks.Count, checks, errors, failure, playerSaveIntact = intact
            }, Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        static Button Button(string name) => GameObject.Find(name)?.GetComponent<Button>();

        void Press(string name)
        {
            var button = Button(name);
            Check(button != null && button.IsInteractable(), name + " etkin");
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            var hit = hits.Count == 0 ? null : ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Check(hit == button.gameObject, name + " tıklama alanı: " + (hits.Count == 0 ? "-" : hits[0].gameObject.name));
            if (button is MapPointButton or BottleTapButton)
            {
                pointer.eligibleForClick = true;
                pointer.pointerCurrentRaycast = hits[0];
                ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerUpHandler);
            }
            ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Until(Func<bool> condition, string description)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Check(condition(), description);
        }

        void CheckText()
        {
            foreach (var label in GameObject.Find("MagicShop").GetComponentsInChildren<TMP_Text>())
            {
                label.ForceMeshUpdate();
                Check(!label.isTextOverflowing && label.firstOverflowCharacterIndex == -1,
                    label.transform.parent.name + "/" + label.name + " taşmıyor");
                if (label.name is "Price" or "PreviousPrice")
                {
                    Check(label.textInfo.lineCount == 1, label.transform.parent.name + " fiyat tek satır");
                    if (label.text.Contains("₺")) Check(label.font.HasCharacter('₺'), "TL simgesi fontta var");
                }
            }
        }

        IEnumerator Shot(string name)
        {
            ScreenCapture.CaptureScreenshot(Output + name + ".png");
            yield return new WaitForSecondsRealtime(.33f);
        }

        IEnumerator RunCostumes()
        {
            var items = new[]{ ShopItem.PrincessCat, ShopItem.ExplorerCat, ShopItem.CrystalCat,
                ShopItem.NightCat, ShopItem.FairyCat, ShopItem.RoyalCat };
            yield return Until(() => L.Ready && Button("NavInventory") != null, "Ana Menü hazır");
            if (Button("ArrivalDismiss") != null) Press("ArrivalDismiss");
            yield return null;
            Press("NavInventory");
            yield return null;
            Press("ShopTab_Costumes");
            yield return null;
            long initialGold = Ledger.Presentation.coins;
            foreach (var item in items)
            {
                foreach (CostumePose pose in Enum.GetValues(typeof(CostumePose)))
                {
                    var sprite = CostumeArt.Get(item, pose);
                    Check(sprite != null && sprite.rect.width > 200 && sprite.rect.height > 200,
                        item + " ayrı sprite: " + pose);
                }
                long gems = Ledger.Presentation.gems;
                long request = Ledger.NextShopPurchase;
                string button = "ShopAction_" + item;
                Press(button);
                yield return null;
                Press("CostumeSide");
                yield return null;
                var art = GameObject.Find("ConfirmArt").GetComponent<Image>();
                Check(art.sprite == CostumeArt.Get(item, CostumePose.StepA) && !Ledger.Owns(item) && Ledger.Presentation.gems == gems,
                    item + " ücret olmadan yan açı önizlendi");
                if (item == ShopItem.FairyCat) yield return Shot("SidePreview");
                Press("CostumeFront");
                yield return null;
                Check(art.sprite == CostumeArt.Get(item, CostumePose.Front), item + " ön açı");
                Press("ShopConfirm");
                yield return null;
                Check(Ledger.Owns(item) && Ledger.Presentation.gems == gems - Config.Price(item) &&
                    Ledger.Presentation.coins == initialGold, item + " yalnızca bir kez elmasla alındı");
                Check(Ledger.Purchase(item, Config.Price(item), request) == ShopPurchaseResult.Duplicate &&
                    Ledger.Purchase(item, Config.Price(item), Ledger.NextShopPurchase) == ShopPurchaseResult.AlreadyOwned,
                    item + " eski ve yeni istekle ikinci ücret reddedildi");
                Press(button);
                yield return null;
                var home = FindObjectsByType<WizardCatPresentation>(FindObjectsSortMode.None)
                    .FirstOrDefault(actor => actor.WornCostume == item && actor.WearingCostumeArt);
                Check(home != null && Ledger.Equipped(ShopCategory.Cats) == item, item + " Ana Menü prefabı anında güncellendi");
            }
            var reopened = GameplayProgress.Open(key);
            Check(items.All(reopened.Owns) && reopened.Equipped(ShopCategory.Cats) == ShopItem.RoyalCat &&
                reopened.Presentation.gems == 15000 - items.Sum(Config.Price), "Altı sahiplik ve son kuşanma kayıt yeniden açıldığında doğru");
            Press("ShopEquipClassic");
            yield return null;
            Check(Ledger.Equipped(ShopCategory.Cats) == ShopItem.ClassicCat &&
                GameplayProgress.Open(key).Equipped(ShopCategory.Cats) == ShopItem.ClassicCat,
                "Varsayılan kostüme dönüş kaydedildi");
            Press("ShopClose");
            yield return null;
            var purchased = Ledger.Snapshot;

            foreach (var item in new[]{ShopItem.ClassicCat}.Concat(items))
            {
                bool classic = item == ShopItem.ClassicCat;
                var fixture = purchased.Copy();
                fixture.levels = MapFlowProbe.Data(98).levels;
                fixture.worldMap = new WorldMapData { highestUnlockedIsland = 1, lastCelebratedIsland = 1 };
                fixture.presentation.reduceMotion = false;
                GameplayProgress.UseTestProfile(fixture);
                Check(Ledger.Equip(item) && Ledger.Claim(99, 3, true, out _) && Ledger.SaveMapFocus(99),
                    item + " izole Kristal Şelaleler yürüyüşü hazır");
                SceneManager.LoadScene("Home");
                yield return null;
                yield return Until(() => SceneManager.GetActiveScene().name == "Home" && Button("NavMap") != null, "Home hazır");
                if (Button("ArrivalDismiss") != null) Press("ArrivalDismiss");
                yield return null;
                var home = FindObjectsByType<WizardCatPresentation>(FindObjectsSortMode.None)
                    .First(actor => actor.WornCostume == item);
                Check(home.WearingCostumeArt != classic, item + " Ana Menü kostüm sprite durumu doğru");
                yield return Shot(item + "-Home");
                Check(home.TriggerMagic() || home.MagicBusy, item + " Ana Menü kutlaması çalıştı");
                yield return new WaitForSecondsRealtime(.3f);
                if (!classic)
                    Check(home.transform.Find("BreathingRig/Body").GetComponent<Image>().sprite ==
                        CostumeArt.Get(item, CostumePose.Victory), item + " kutlama pozu rig üzerinde");
                Press("NavMap");
                yield return Until(() => SceneManager.GetActiveScene().name == "LevelMap" &&
                    FindFirstObjectByType<MapWorld>()?.Mounted == true, "Dünya haritası açıldı");
                var map = FindFirstObjectByType<MapWorld>();
                var overview = map.Canvas.GetComponentsInChildren<RawImage>().Single(img =>
                    img.name == "WorldCostumeMascot" && img.enabled);
                Check(classic || overview.texture == CostumeArt.Get(item, CostumePose.Sitting).texture,
                    item + " dünya haritasındaki gerçek kostüm atlası");
                yield return Shot(item + "-World");
                var island = map.Points.Single(point => point.gameObject.activeInHierarchy &&
                    point.kind == MapPointKind.LevelGroup && point.regionIndex == 1);
                Press(island.name);
                yield return Until(() => map.JourneyRunning, "99 → 100 animasyonu başladı");
                var journeyCat = GameObject.Find("WizardCatGuide").GetComponent<JourneyCatImage>();
                var states = new HashSet<JourneyCatPose>();
                float end = Time.realtimeSinceStartup + 12;
                while (map.JourneyRunning && Time.realtimeSinceStartup < end)
                {
                    var pose = journeyCat.Pose;
                    if (states.Add(pose))
                    {
                        Check(classic || journeyCat.texture == CostumeArt.Get(item, (CostumePose)((int)pose + 1)).texture &&
                            journeyCat.uvRect == CostumeArt.UV(CostumeArt.Get(item, (CostumePose)((int)pose + 1))),
                            item + " bölüm haritası gerçek poz: " + pose);
                        ScreenCapture.CaptureScreenshot(Output + item + "-" + pose + ".png");
                    }
                    yield return null;
                }
                Check(!map.JourneyRunning && states.Contains(JourneyCatPose.Curled) &&
                    states.Contains(JourneyCatPose.Blink) && states.Contains(JourneyCatPose.Stretch) &&
                    states.Contains(JourneyCatPose.Standing) && states.Contains(JourneyCatPose.StepA) &&
                    states.Contains(JourneyCatPose.StepB), item + " uyku, uyanış, gerinme, kalkış ve iki adım oynadı");

                var prepared = GameplayLevelCatalog.PrepareAsync(100);
                yield return Until(() => prepared.IsCompletedSuccessfully, "Bölüm 100 hazır");
                Check(GameplayLevelCatalog.Select(100, Ledger), "Bölüm 100 seçildi");
                SceneManager.LoadScene("Gameplay");
                yield return null;
                yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == 100, "Gameplay hazır");
                var actor = GameObject.Find("GameplayWizardCat").GetComponent<WizardCatPresentation>();
                Check(actor.WornCostume == item && actor.WearingCostumeArt != classic, item + " oyun içi bekleme");
                yield return Shot(item + "-Gameplay");
                var app = FindFirstObjectByType<PrototypeApp>();
                const BindingFlags hidden = BindingFlags.Instance | BindingFlags.NonPublic;
                var board = (Puzzle)typeof(PrototypeApp).GetField("puzzle", hidden).GetValue(app);
                var solution = PuzzleSolver.Solve(board.Export());
                foreach (var move in solution.Moves)
                    if (board.Pour(move.Source, move.Target) == 0) throw new InvalidOperationException("Geçersiz QA döküşü");
                Check(board.Won, "Gerçek kurallarla 100. bölüm çözüldü");
                typeof(PrototypeApp).GetMethod("Refresh", hidden).Invoke(app, new object[] { false });
                yield return Until(() => GameObject.Find("VictoryMascotReveal")?.GetComponent<CanvasGroup>().alpha > .99f,
                    item + " bölüm sonu maskot animasyonu tamamlandı");
                var victory = GameObject.Find("VictoryCostume").GetComponent<Image>();
                Check(classic ? !victory.enabled : victory.enabled && victory.sprite == CostumeArt.Get(item, CostumePose.Victory),
                    item + " bölüm sonu doğru kostüm");
                var backdrop = GameObject.Find("CostumeBackdrop").GetComponent<RawImage>();
                Check(classic ? !backdrop.enabled : backdrop.enabled && backdrop.texture != null,
                    item + " bölüm sonu maskot fonu doğru");
                yield return Shot(item + "-Victory");
            }
        }

        IEnumerator Run()
        {
            yield return Until(() => L.Ready && Button("NavInventory") != null, "Ana Menü hazır");
            if (Button("ArrivalDismiss") != null) Press("ArrivalDismiss");
            Check(Ledger.Presentation.coins == 600 && Ledger.Presentation.gems == 2700 &&
                Ledger.Presentation.energy == 4, "Eski şema bakiyeleri korundu");
            Press("NavInventory");
            yield return null;
            string[] pages = { "Offers", "Gold", "Powers", "Costumes" };
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                Size(height);
                yield return Until(() => Screen.height == height, $"900 × {height} Game View");
                foreach (string page in pages)
                {
                    var header = (RectTransform)GameObject.Find("ShopTitleFrame").transform;
                    Vector3 headerBefore = header.position;
                    var tabScroll = GameObject.Find("ShopViewport").GetComponent<ScrollRect>();
                    Press("ShopTab_" + page);
                    Check(Mathf.Abs(tabScroll.content.anchoredPosition.y) < .1f &&
                        (header.position - headerBefore).sqrMagnitude < .01f, page + " ilk karede üstte, başlık sabit");
                    yield return null;
                    Check(Mathf.Abs(tabScroll.content.anchoredPosition.y) < .1f &&
                        (header.position - headerBefore).sqrMagnitude < .01f, page + " sonraki karede sıçrama yok");
                    Check(GameObject.Find("ShopPage_" + page) != null && Button("ShopTab_Effects") == null,
                        page + " açık, Efektler sekmesi yok");
                    CheckText();
                    if (height == 1600) yield return Shot(page);
                    else
                    {
                        Directory.CreateDirectory("Library/ShopVisualAudit/Portrait/");
                        ScreenCapture.CaptureScreenshot($"Library/ShopVisualAudit/Portrait/{page}-{height}.png");
                        yield return null;
                    }
                    float scrollRoom = tabScroll.content.rect.height - tabScroll.viewport.rect.height;
                    var wheel = new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, -18) };
                    ExecuteEvents.Execute(tabScroll.gameObject, wheel, ExecuteEvents.scrollHandler);
                    yield return null;
                    float offset = tabScroll.content.anchoredPosition.y;
                    Check(scrollRoom > .1f ? offset > .1f && offset <= scrollRoom + .1f : Mathf.Abs(offset) < .1f,
                        page + " " + height + " kaydırma içerik sınırına uyuyor");
                }
            }

            Size(1600);
            yield return Until(() => Screen.height == 1600, "9:16 işlem senaryoları");
            Press("ShopTab_Offers");
            yield return null;
            var catalog = JObject.Parse(Resources.Load<TextAsset>("ShopReference/Offers").text);
            foreach (var offer in catalog["offers"])
            {
                string id = (string)offer["id"];
                var card = GameObject.Find("Offer_" + id).transform;
                Check(card.Find("OfferTitle").GetComponent<TMP_Text>().text == (string)offer["name"] &&
                    card.Find("OfferPrice/Price").GetComponent<TMP_Text>().text == "₺" + (string)offer["price"] &&
                    card.Find("OfferPrice/PreviousPrice").GetComponent<TMP_Text>().text == "₺" + (string)offer["previous"] &&
                    card.Find("ShopBuy_" + id + "/Label").GetComponent<TMP_Text>().text.Contains("Önizle"),
                    id + " teklif fiyatları ve önizleme durumu");
                var items = card.Find("OfferContents").GetComponentsInChildren<TMP_Text>().Select(label => label.text);
                Check(items.SequenceEqual(offer["items"].Values<string>()), id + " paket içeriği katalogla aynı");
            }
            Press("ShopBuy_powers");
            yield return null;
            Check(GameObject.Find("ConfirmMessage").GetComponent<TMP_Text>().text.Contains("kapalı"), "Gerçek para teklifi açıklaması");
            Press("ShopConfirm");
            yield return null;
            Check(Ledger.Presentation.coins == 600 && Ledger.NextShopPurchase == 1, "Özel paket ödeme/teslimat başlatmadı");

            Press("ShopTab_Gold");
            yield return null;
            var featured = catalog["gold"].Last;
            foreach (string name in new[]{"Featured10000Gold", "Repeat10000Gold"})
            {
                var card = GameObject.Find(name).transform;
                Check(card.Find("GoldAmount").GetComponent<TMP_Text>().text == "10.000 Altın" &&
                    card.Find("PricePlate/Price").GetComponent<TMP_Text>().text == "₺" + (string)featured["price"] &&
                    card.Find("Bonus/Label").GetComponent<TMP_Text>().text == "%30 BONUS", name + " katalogyla aynı");
            }
            Press("ShopBuy_gold_featured");
            yield return null;
            Press("ShopConfirm");
            yield return null;
            Check(Ledger.Presentation.coins == 600 && Ledger.NextShopPurchase == 1, "Altın paketi önizlemesi bakiye yazmadı");

            Press("ShopTab_Powers");
            yield return null;
            var powerPrices = new (string icon, int price)[]
            {
                ("move", (int)catalog["previewPowers"][0]["price"]), ("extra", Config.Price(ShopItem.ExtraBottle)),
                ("hint", Config.Price(ShopItem.Hint)), ("shuffle", Config.Price(ShopItem.Shuffle)),
                ("swap", Config.Price(ShopItem.ColorSwap)), ("hammer", Config.Price(ShopItem.BottleHammer))
            };
            foreach (var (icon, price) in powerPrices)
                Check(GameObject.Find("Power_" + icon).transform.Find("PricePlate/Price").GetComponent<TMP_Text>().text ==
                    price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")), icon + " güç fiyatı modelle aynı");
            Press("ShopPreview_move");
            yield return null;
            Press("ShopConfirm");
            yield return null;
            Press("BuyBottleHammer");
            yield return null;
            Press("ShopConfirm");
            yield return null;
            Check(Ledger.NextShopPurchase == 1, "Ekstra Hamle önizleme ve erken bölümde Çekiç satın alma kilidi");
            Press("BuyExtraBottle");
            yield return null;
            Press("ShopConfirm");
            yield return null;
            Check(GameObject.Find("ConfirmMessage").GetComponent<TMP_Text>().text.Contains("altının"), "Yetersiz altın bildirimi");
            Press("ShopCancel");
            yield return null;
            Check(Ledger.SetResources(3000, 2700, 4), "İzole profil işlemler için dolduruldu");
            int extra = Ledger.Remaining(BoosterKind.ExtraBottle);
            Press("BuyExtraBottle");
            yield return null;
            long request = Ledger.NextShopPurchase;
            Press("ShopConfirm");
            yield return null;
            Check(Ledger.Presentation.coins == 3000 - Config.extraPrice && Ledger.Remaining(BoosterKind.ExtraBottle) == extra + 1 &&
                Ledger.Purchase(ShopItem.ExtraBottle, Config.extraPrice, request) == ShopPurchaseResult.Duplicate,
                "Güç stoğu canlı, altın doğru, aynı işlem numarası yeniden yazılmadı");
            Check(GameObject.Find("Power_extra").transform.Find("ProductInfo").GetComponent<TMP_Text>().text.EndsWith((extra + 1).ToString()),
                "Sahip Olduğun satırı satın almadan sonra yenilendi");

            Press("ShopTab_Costumes");
            yield return null;
            var wardrobe = GameObject.Find("ShopPage_Costumes").transform;
            var expectedOrder = new[]{ ShopItem.PrincessCat, ShopItem.ExplorerCat, ShopItem.CrystalCat,
                ShopItem.NightCat, ShopItem.FairyCat, ShopItem.RoyalCat };
            Check(wardrobe.Cast<Transform>().Where(card => card.name.StartsWith("ShopProduct_")).Select(card => card.name)
                .SequenceEqual(expectedOrder.Select(item => "ShopProduct_" + item)), "Altı kostüm referans sırasıyla");
            foreach (var item in expectedOrder)
            {
                var price = wardrobe.Find("ShopProduct_" + item + "/PricePlate/Price").GetComponent<TMP_Text>().text;
                Check(price == Config.Price(item).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),
                    item + " fiyatı ekonomi modelinden");
            }
            Press("ShopAction_PrincessCat");
            yield return null;
            Press("ShopConfirm");
            yield return null;
            Check(Ledger.Owns(ShopItem.PrincessCat) && Ledger.Presentation.gems == 2700 - Config.princessCatPrice &&
                Ledger.Presentation.coins == 3000 - Config.extraPrice, "Prenses Kedi yalnızca elmasla alındı");
            Press("ShopAction_PrincessCat");
            yield return null;
            Check(Ledger.Equipped(ShopCategory.Cats) == ShopItem.PrincessCat &&
                GameObject.Find("EquippedCostumeTitle").GetComponent<TMP_Text>().text == "Prenses Kedi", "Kuşan ve seçili kostüm eşleşti");
            Press("ShopEquipClassic");
            yield return null;
            Check(Ledger.Equipped(ShopCategory.Cats) == ShopItem.ClassicCat, "Büyücü Kedi yeniden kuşanıldı");
            Press("ShopAction_RoyalCat");
            yield return null;
            Press("ShopConfirm");
            yield return null;
            Check(GameObject.Find("ConfirmMessage").GetComponent<TMP_Text>().text.Contains("elmasın") &&
                !Ledger.Owns(ShopItem.RoyalCat), "Yetersiz elmasta kostüm verilmedi");
            Press("ShopCancel");
            yield return null;

            var scroll = GameObject.Find("ShopViewport").GetComponent<ScrollRect>();
            float before = scroll.content.anchoredPosition.y;
            var mouse = new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, -12) };
            ExecuteEvents.Execute(scroll.gameObject, mouse, ExecuteEvents.scrollHandler);
            yield return null;
            Check(scroll.content.anchoredPosition.y > before + 10, "Unity scroll girdisi koleksiyon alanına kaydırdı");
            Press("ShopTab_Offers");
            Check(Mathf.Abs(scroll.content.anchoredPosition.y) < .1f && scroll.velocity.sqrMagnitude < .01f,
                "Kaydırılan Kostümler'den Özel Teklifler'e geçiş ilk karede sıfır konum ve hız");
            yield return null;
            Check(Mathf.Abs(scroll.content.anchoredPosition.y) < .1f, "Kaydırma sonraki sekmeye taşınmadı");
            Press("ShopTab_Costumes");
            yield return null;
            scroll.verticalNormalizedPosition = 0;
            yield return null;
            Press("ShopOwned");
            yield return null;
            Press("ShopCollectionClose");
            yield return null;

            var reopened = GameplayProgress.Open(key);
            Check(reopened.Owns(ShopItem.PrincessCat) && reopened.Presentation.gems == 2700 - Config.princessCatPrice &&
                reopened.Remaining(BoosterKind.ExtraBottle) == extra + 1, "Yeniden açılan profilde kostüm, elmas ve stok korundu");
            Press("ShopClose");
            yield return null;
            Check(GameObject.Find("MagicShop") == null, "Mağaza kapandı ve Ana Menü açığa çıktı");
        }
    }
}
#endif
