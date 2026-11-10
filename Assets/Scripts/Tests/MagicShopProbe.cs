#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.UI;
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
        const string Output = "docs/ProductionReports/MagicShop/";
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
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Önce açık sahneyi kaydet.");
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("Magic Shop QA").AddComponent<MagicShopProbe>();
            Size(1600);
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
            data.presentation.coins = 700;
            data.presentation.gems = 17;
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
            stack.Push(Run());
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

        IEnumerator Until(Func<bool> condition, string name)
        {
            float limit = Time.realtimeSinceStartup + 20;
            while (!condition() && Time.realtimeSinceStartup < limit) yield return null;
            Check(condition(), name);
        }

        static Button Button(string name) => GameObject.Find(name)?.GetComponent<Button>();

        void Click(string name, bool doubleTap = false)
        {
            var button = Button(name);
            Check(button != null && button.IsInteractable(), name + " etkileşimli");
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
                name + " gerçek raycast hedefi" + (hits.Count > 0 ? " · " + hits[0].gameObject.name : ""));
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            if (doubleTap) ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Shot(string file)
        {
            yield return new WaitForSecondsRealtime(.20f);
            ScreenCapture.CaptureScreenshot(Output + file + ".png");
            yield return new WaitForSecondsRealtime(.25f);
            Check(File.Exists(Output + file + ".png"), file + " Game View kaydedildi");
        }

        void Models()
        {
            var data = new ProgressData();
            data.presentation.coins = 500;
            var failed = new ProgressLedger(data, _ => throw new IOException("QA"));
            var item = ShopItem.CrystalCat;
            int price = Config.Price(item);
            Check(failed.Purchase(item, price, 1) == ShopPurchaseResult.SaveFailed && failed.Presentation.coins == 500 &&
                !failed.Owns(item) && failed.NextShopPurchase == 1, "Başarısız kayıt altın, sahiplik ve işlem numarasını değiştirmedi");
            var ledger = new ProgressLedger(data, _ => { });
            Check(ledger.Purchase(item, -1, 1) == ShopPurchaseResult.PriceChanged && ledger.Presentation.coins == 500,
                "Negatif fiyat reddedildi");
            Check(!ledger.SetResources(-1, 0, 5), "Negatif altın engellendi");
            Check(!ledger.Equip(item), "Sahip olunmayan görünüm kuşanılamadı");
            Check(ledger.Purchase(item, price, 1) == ShopPurchaseResult.Purchased, "Yeterli altınla kozmetik alındı");
            Check(ledger.Purchase(item, price, 1) == ShopPurchaseResult.Duplicate &&
                ledger.Purchase(item, price, 2) == ShopPurchaseResult.AlreadyOwned, "Tekrarlanan işlem ve tekil ürün ikinci kez ödenmedi");
            Check(ledger.Purchase(ShopItem.GoldCat, Config.Price(ShopItem.GoldCat), 2) == ShopPurchaseResult.InsufficientGold &&
                ledger.Presentation.coins == 500 - price, "Yetersiz altın bakiye ve sahipliği değiştirmedi");
            var consumed = new ProgressLedger(data, _ => { });
            int stock = consumed.Remaining(BoosterKind.Hint);
            Check(consumed.Purchase(ShopItem.Hint, Config.hintPrice, 1) == ShopPurchaseResult.Purchased &&
                consumed.Purchase(ShopItem.Hint, Config.hintPrice, 1) == ShopPurchaseResult.Duplicate &&
                consumed.Remaining(BoosterKind.Hint) == stock + Config.boosterQuantity, "Aynı tekrar alınabilir güçlendirici isteği tek kez uygulandı");

            var rewards = new ProgressLedger(new ProgressData(), _ => { }, () => new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero));
            Check(rewards.Claim(1, 3, true, out _, "shop-victory-1", 3, false, true) && rewards.LastCompletionReward.Base == Config.levelBase,
                "İlk kazanış temel altını yapılandırmadan aldı");
            Check(rewards.TryBoostVictory(1, "shop-victory-1", out int bonus) &&
                rewards.LastCoinReward + bonus == rewards.LastCoinReward * ProgressLedger.VictoryAdMultiplier, "Reklam ödülü yapılandırılmış toplama ulaştı");
            for (int i = 2; i <= 6; i++)
                Check(rewards.Claim(i, 3, true, out _, "shop-victory-" + i, 3), "Etkinlik kazanışı " + i);
            long coins = rewards.Presentation.coins;
            Check(rewards.ClaimDailyTask(DailyTaskKind.Wins) && !rewards.ClaimDailyTask(DailyTaskKind.Wins) &&
                rewards.Presentation.coins == coins + Config.taskWinsGold, "Günlük görev değişken altını bir kez verdi");
            coins = rewards.Presentation.coins;
            Check(rewards.ClaimWeekly() && rewards.Owns(ShopItem.RoseEffect) && !rewards.ClaimWeekly() &&
                rewards.Presentation.coins == coins + Config.weeklyGold, "Haftalık etkinlik altın ve kozmetik verdi");
            coins = rewards.Presentation.coins;
            Check(rewards.ClaimSeason(2) && rewards.Owns(ShopItem.GoldBottle) && !rewards.ClaimSeason(2) &&
                rewards.Presentation.coins == coins + Config.SeasonGold(2), "Sezon altını ve cam ödülünü bir kez verdi");
        }

        IEnumerator Buy(string button)
        {
            Click(button);
            yield return null;
            Click("ShopConfirm", true);
            yield return new WaitForSecondsRealtime(.31f);
        }

        void CheckPortrait(int height)
        {
            Canvas.ForceUpdateCanvases();
            var view = GameObject.Find("ShopViewport").GetComponent<RectTransform>();
            Check(view.rect.width > 300 && view.rect.height > 500, height + " mağaza görünüm alanı");
            foreach (var text in GameObject.Find("MagicShop").GetComponentsInChildren<TMP_Text>())
            {
                text.ForceMeshUpdate();
                Check(!text.isTextOverflowing, height + " " + text.name + " taşmıyor");
            }
        }

        IEnumerator Run()
        {
            Models();
            yield return Until(() => L.Ready && Button("NavInventory") != null, "Home ve yerelleştirme hazır");
            L.Select("tr", false);
            Check(Ledger.Presentation.coins == 700 && Ledger.Presentation.gems == 17 && Ledger.Presentation.energy == 4 &&
                Ledger.Equipped(ShopCategory.Cats) == ShopItem.ClassicCat, "Eski profilsiz mağaza verisi altın, mücevher ve enerjiyi korudu");
            if (Button("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            Click("NavInventory");
            yield return null;
            yield return Shot("01-Boosters");
            CheckPortrait(1600);
            Click("BuyHint");
            yield return null;
            yield return Shot("02-Confirm");
            Click("ShopCancel");
            Check(Ledger.Presentation.coins == 700, "Vazgeç altın harcamadı");
            int hints = Ledger.Remaining(BoosterKind.Hint);
            yield return Buy("BuyHint");
            Check(Ledger.Presentation.coins == 700 - Config.hintPrice &&
                Ledger.Remaining(BoosterKind.Hint) == hints + Config.boosterQuantity, "Raycast ile onaylanan çift dokunuş bir kez satın aldı");

            Click("ShopTab_Cats");
            yield return null;
            yield return Shot("03-Costumes");
            yield return Buy("ShopAction_CrystalCat");
            Click("ShopAction_CrystalCat");
            yield return null;
            Check(Ledger.Equipped(ShopCategory.Cats) == ShopItem.CrystalCat, "Kristal kostüm kuşanıldı");
            Click("ShopTab_Bottles");
            yield return null;
            yield return Buy("ShopAction_CrystalBottle");
            Click("ShopAction_CrystalBottle");
            yield return null;
            yield return Shot("04-Bottles");
            Click("ShopTab_Effects");
            yield return null;
            yield return Buy("ShopAction_RoseEffect");
            Click("ShopAction_RoseEffect");
            yield return null;
            yield return Shot("05-Effects");

            Click("ShopTab_Cats");
            yield return null;
            Click("ShopAction_GoldCat");
            yield return null;
            Click("ShopConfirm");
            long remaining = Ledger.Presentation.coins;
            Check(!Ledger.Owns(ShopItem.GoldCat) && remaining == 700 - Config.hintPrice - Config.crystalCatPrice -
                Config.crystalBottlePrice - Config.roseEffectPrice, "Yetersiz bakiye gerçek mağazada harcanmadı");
            yield return Shot("06-Insufficient");
            Click("ShopCancel");
            yield return null;
            Click("ShopOwned");
            yield return null;
            Check(GameObject.Find("ShopProduct_GoldCat") == null && GameObject.Find("ShopProduct_CrystalCat") != null,
                "Sahip Olduklarım yalnız sahip olunanları listeliyor");
            yield return Shot("07-Owned");
            Click("ShopTab_Packs");
            yield return null;
            Check(GameObject.Find("ShopSpecialPacks") != null && Button("ShopConfirm") == null &&
                Button("BuyHint") == null, "Özel paketler ödeme eylemi açmadı");
            yield return Shot("08-Packs");
            foreach (int height in new[] { 1950, 2000, 1600 })
            {
                Size(height);
                yield return Until(() => Screen.height == height, height + " Game View hazır");
                yield return null;
                if (height == 2000)
                {
                    var safe = (RectTransform)GameObject.Find("MagicShop").transform.parent;
                    safe.anchorMin = new Vector2(.03f, .045f);
                    safe.anchorMax = new Vector2(.97f, .945f);
                }
                Click("ShopTab_Boosters");
                yield return null;
                CheckPortrait(height);
                if (height == 2000) yield return Shot("09-Shop-SafeArea");
            }
            Click("ShopClose");
            yield return null;
            var liveCat = FindFirstObjectByType<HomeCharacterPresenter>().cat;
            Check(liveCat.GetComponentsInChildren<Image>().Any(i => i.name == "Body" && i.material.shader.name == "Mewgic/ShopCostume"),
                "Ana menüde asıl maskotun kostüm malzemesi değişti");
            yield return Shot("10-EquippedHome");

            PlayerPrefs.SetString(key, "{corrupt");
            PlayerPrefs.Save();
            var reopened = GameplayProgress.Open(key);
            Check(PlayerPrefs.GetString(key + ".corrupt") == "{corrupt" && reopened.Presentation.coins == remaining &&
                reopened.Presentation.gems == 17 && reopened.Equipped(ShopCategory.Cats) == ShopItem.CrystalCat &&
                reopened.Equipped(ShopCategory.Bottles) == ShopItem.CrystalBottle &&
                reopened.Equipped(ShopCategory.Effects) == ShopItem.RoseEffect, "Bozuk ana kayıt aynadan bakiye ve kuşanılanları kurtardı");
            Check(reopened.Purchase(ShopItem.Hint, Config.hintPrice, 1) == ShopPurchaseResult.Duplicate,
                "Yeniden açılan kayıtta eski satın alma yeniden ödenmedi");
            Current.SetValue(null, reopened);
            SceneManager.LoadScene("Boot");
            yield return null;
            yield return Until(() => SceneManager.GetActiveScene().name == "Home" && Button("Play") != null &&
                BootLoadingController.Active == null, "Kayıt yeniden açıldı ve gerçek Boot → Home tamamlandı");
            yield return null;
            if (Button("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            Click("Play");
            yield return Until(() => SceneManager.GetActiveScene().name == "Gameplay" && Button("Hint") != null, "Gerçek Gameplay açıldı");
            yield return null;
            FindFirstObjectByType<PawGuide>()?.Close();
            var glass = GameObject.Find("Bottle 1").GetComponentsInChildren<Image>().First(i => i.name == "GlassShell");
            Check(glass.color == ShopCosmetics.BottleTint(ShopItem.CrystalBottle), "Kuşanılan şişe camı Gameplay’de etkin");
            yield return Shot("11-EquippedGameplay");
            int hintBefore = Ledger.Remaining(BoosterKind.Hint);
            Click("Hint");
            Check(Ledger.Remaining(BoosterKind.Hint) == hintBefore - 1, "Alınan güçlendirici gerçek oyunda harcandı");
            int selected = FindFirstObjectByType<PrototypeApp>().GuideSelectedBottle;
            if (selected >= 0) Button("Bottle " + (selected + 1)).onClick.Invoke();
            int pours = 0;
            foreach (var move in new[] { (1,3), (2,1), (3,2) })
            {
                Button("Bottle " + move.Item1).onClick.Invoke();
                Button("Bottle " + move.Item2).onClick.Invoke();
                yield return new WaitForSecondsRealtime(.22f);
                if (++pours == 2)
                {
                    var sparkle = GameObject.Find("Bottle 1").GetComponentInChildren<BottleCompletionGraphic>();
                    var tint = (Color)typeof(BottleCompletionGraphic).GetField("tint", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sparkle);
                    Check(sparkle.Playing && tint == ShopCosmetics.EffectTint(ShopItem.RoseEffect),
                        "Kuşanılan Gül Parıltısı gerçek şişe tamamlanışında çalıştı");
                    ScreenCapture.CaptureScreenshot(Output + "12-EquippedSparkle.png");
                    yield return null;
                }
            }
            yield return Until(() => GameObject.Find("Win panel") != null, "Bulmaca gerçek döküşlerle kazanıldı");
            Check(Ledger.Completed(1) && Ledger.BestStars(1) == 3 && Ledger.Presentation.coins == remaining + Ledger.LastCoinReward &&
                Ledger.LastCompletionReward.Base == Config.levelBase, "Kazanış mağaza bakiyesine eklendi ve ilerleme korundu");
        }
    }
}
#endif
