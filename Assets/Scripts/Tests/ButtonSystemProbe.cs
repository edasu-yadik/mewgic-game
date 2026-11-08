#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
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
    public sealed class ButtonSystemProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/PremiumButtons/";
        static ButtonSystemProbe active;
        static readonly string[] SaveSuffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        [SerializeField] bool before;
        readonly List<string> checks = new(), errors = new();
        Dictionary<string, string> saved;
        bool background;
        string Folder => Output + (before ? "Before/" : "After/");

        [MenuItem("Mewgic/QA/Run Premium Buttons")]
        public static void After() => Begin(false);
        public static void Before() => Begin(true);

        static void Begin(bool baseline)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Açık sahnede kaydedilmemiş değişiklik var.");
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            var probe = new GameObject("Premium Buttons QA").AddComponent<ButtonSystemProbe>();
            probe.before = baseline;
            Size(1600);
            EditorApplication.isPlaying = true;
        }

        static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor")
            .GetMethod("Size").Invoke(null, new object[] { height });

        static Dictionary<string, string> PlayerSave() => SaveSuffixes.ToDictionary(
            s => s, s => PlayerPrefs.HasKey(GameplayProgress.StorageKey + s)
                ? PlayerPrefs.GetString(GameplayProgress.StorageKey + s) : null);

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            background = Application.runInBackground;
            Application.runInBackground = true;
            saved = PlayerSave();
            var data = new ProgressData();
            data.presentation.manualLocale = "tr";
            data.presentation.coins = 200;
            GameplayProgress.UseTestProfile(data);
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived += Log;
        }

        void Log(string text, string stack, LogType type)
        {
            if (type is LogType.Error or LogType.Exception or LogType.Assert) errors.Add(text);
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.logMessageReceived -= Log;
            Application.runInBackground = background;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
        }

        void Check(bool okay, string note)
        {
            if (!okay) throw new InvalidOperationException(note);
            checks.Add(note);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(Folder);
            string failure = null;
            var stack = new Stack<IEnumerator>();
            stack.Push(Run());
            while (stack.Count > 0)
            {
                bool more;
                object item;
                try { more = stack.Peek().MoveNext(); item = more ? stack.Peek().Current : null; }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!more) stack.Pop();
                else if (item is IEnumerator child && item is not CustomYieldInstruction) stack.Push(child);
                else yield return item;
            }
            var final = PlayerSave();
            bool intact = saved.All(pair => final[pair.Key] == pair.Value);
            File.WriteAllText(Folder + "PLAY_MODE.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = intact && failure == null && errors.Count == 0,
                editor = Application.unityVersion, count = checks.Count, checks, errors, failure, playerSaveIntact = intact
            }, Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        IEnumerator Until(Func<bool> ready, string note)
        {
            float end = Time.realtimeSinceStartup + 18;
            while (!ready() && Time.realtimeSinceStartup < end) yield return null;
            Check(ready(), note);
        }

        static Button Find(string name) => GameObject.Find(name)?.GetComponent<Button>();

        PointerEventData Pointer(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            return new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
        }

        void Click(string name, bool twice = false)
        {
            var button = Find(name);
            Check(button != null && button.IsInteractable(), name + " etkileşimli");
            var pointer = Pointer(button);
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Check(hits.Count > 0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) == button.gameObject,
                name + " raycast hedefi");
            Dispatch(button, pointer);
            if (twice) Dispatch(button, pointer);
        }

        static void Dispatch(Button button, PointerEventData pointer)
        {
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForSecondsRealtime(.35f);
            ScreenCapture.CaptureScreenshot(Folder + name + ".png");
            yield return new WaitForSecondsRealtime(.25f);
            Check(File.Exists(Folder + name + ".png"), name + " Game View karesi");
        }

        void CheckLayout(string label, params string[] names)
        {
            Canvas.ForceUpdateCanvases();
            foreach (var name in names)
            {
                var button = Find(name);
                Check(button != null, label + " " + name);
                var text = button.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.ForceMeshUpdate();
                    Check(!text.isTextOverflowing, label + " " + name + " metni taşmıyor");
                }
                var icon = button.transform.Find("PremiumIcon") as RectTransform ??
                    button.transform.Find("Icon") as RectTransform ?? button.transform.Find("QuestIcon") as RectTransform;
                var corners = new Vector3[4];
                if (icon != null)
                {
                    var host = (RectTransform)button.transform;
                    icon.GetWorldCorners(corners);
                    var min = host.InverseTransformPoint(corners[0]);
                    var max = host.InverseTransformPoint(corners[2]);
                    Check(min.x >= host.rect.xMin - 1 && min.y >= host.rect.yMin - 1 &&
                        max.x <= host.rect.xMax + 1 && max.y <= host.rect.yMax + 1, label + " " + name + " ikonu düğme içinde");
                }
                ((RectTransform)button.transform).GetWorldCorners(corners);
                var safe = SafeRect();
                var safeCorners = new Vector3[4];
                safe.GetWorldCorners(safeCorners);
                Check(corners[0].x >= safeCorners[0].x - 1 && corners[0].y >= safeCorners[0].y - 1 &&
                    corners[2].x <= safeCorners[2].x + 1 && corners[2].y <= safeCorners[2].y + 1,
                    label + " " + name + " güvenli alan içinde");
            }
        }

        static RectTransform SafeRect() =>
            FindFirstObjectByType<HomeCharacterPresenter>()?.transform.Find("UI") as RectTransform ??
            (GameObject.Find("Safe area") ?? GameObject.Find("MapSafeArea"))?.GetComponent<RectTransform>();

        static void InsetSafeArea()
        {
            var safe = SafeRect();
            safe.anchorMin = new Vector2(.025f, .034f);
            safe.anchorMax = new Vector2(.975f, .95f);
        }

        IEnumerator Ratios(params string[] names)
        {
            if (before) yield break;
            CheckLayout("900x1600", names);
            foreach (int height in new[] { 1950, 2000, 1600 })
            {
                Size(height);
                yield return Until(() => Screen.height == height, "Game View " + height);
                yield return null;
                if (height == 2000)
                {
                    InsetSafeArea();
                    yield return null;
                }
                if (height != 1600) CheckLayout("900x" + height, names);
            }
        }

        IEnumerator Run()
        {
            yield return Until(() => L.Ready && Find("Play") != null, "Home hazır");
            L.Select("tr", false);
            if (Find("ArrivalDismiss") != null)
            {
                yield return Shot("00-Gift");
                Click("ArrivalDismiss");
                yield return null;
            }

            yield return Shot("01-Home");
            if (!before)
            {
                foreach (int height in new[] { 1950, 2000 })
                {
                    Size(height);
                    yield return Until(() => Screen.height == height, "Game View " + height);
                    yield return null;
                    if (height == 2000)
                    {
                        InsetSafeArea();
                        yield return null;
                    }
                    foreach (var code in L.Codes)
                    {
                        L.Select(code, false);
                        CheckLayout(code + "/" + height, "Play", "NavHome", "NavMap", "NavTeam", "NavInventory", "NavEvents", "MissionsButton", "SettingsButton");
                    }
                    L.Select("tr", false);
                    yield return Shot("01-Home-" + height);
                }
                Size(1600);
                yield return Until(() => Screen.height == 1600, "Game View 1600");
            }

            Click("SettingsButton");
            yield return Shot("02-Settings");
            yield return Ratios("settings.music", "LanguageButton", "TutorialReplay", "ResourceInfoClose");
            if (!before)
            {
                var button = Find("settings.music");
                var pointer = Pointer(button);
                var face = button.transform.Find("PremiumFace");
                Check(face != null, "Ayar düğmesi ortak prefab yüzünü kullandı");
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                yield return new WaitForSecondsRealtime(.08f);
                Check(face.localScale.x < .995f, "Basışta yüz küçüldü");
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                yield return new WaitForSecondsRealtime(.22f);
                Check(face.localScale.x > .999f, "Bırakınca yüz eski boyutuna döndü");
                bool music = GameplayProgress.Current.Presentation.MusicEnabled;
                Click("settings.music");
                yield return null;
                Click("settings.music");
                Check(GameplayProgress.Current.Presentation.MusicEnabled != music, "Çift dokunuş ayarı iki kez çevirmedi");
            }
            Click("ResourceInfoClose");
            yield return null;
            Click("CoinsPlus");
            yield return Shot("03-Shop");
            yield return Ratios("BuyHint", "BuyUndo", "BuyExtraBottle", "BuyShuffle", "ShopClose");
            if (!before)
            {
                var ledger = GameplayProgress.Current;
                long coins = ledger.Presentation.coins;
                int hints = ledger.Remaining(BoosterKind.Hint);
                Click("BuyHint");
                yield return null;
                Click("ShopConfirm", true);
                Check(ledger.Presentation.coins == coins - ProgressLedger.BoosterPrice(BoosterKind.Hint) &&
                    ledger.Remaining(BoosterKind.Hint) == hints + 1, "Çift dokunuşta tek satın alma");
            }
            Click("ShopClose");
            yield return null;
            Click("MissionsButton");
            yield return Shot("04-Missions");
            yield return Ratios("DailyClose", "DailyTasksTab", "DailyGiftTab", "DailyAction_0", "DailyAction_3");
            Click("DailyGiftTab");
            yield return Shot("05-DailyReward");
            yield return Ratios("ClaimDailyGift", "DailyClose");
            if (!before)
            {
                var ledger = GameplayProgress.Current;
                long coins = ledger.Presentation.coins;
                Click("ClaimDailyGift");
                yield return Until(() => Find("ArrivalOpen") != null, "Günlük ödül mevcut sandık penceresini açtı");
                yield return null;
                Click("ArrivalOpen");
                yield return Until(() => Find("ArrivalContinue") != null, "Sandık animasyonu tamamlandı");
                yield return null;
                Click("ArrivalContinue");
                yield return null;
                Check(ledger.Presentation.coins == coins + ProgressLedger.DailyGiftCoins && !Find("ClaimDailyGift").IsInteractable(),
                    "Günlük ödül gerçek düğmeden alındı ve yeniden kilitlendi");
            }
            Click("DailyClose");
            yield return null;
            Click("NavEvents");
            yield return Shot("06-Events");
            yield return Ratios("RewardHubClose", "RewardTab_0", "RewardTab_1", "RewardTab_2", "RewardTab_3", "HubOpenDaily");
            Click("RewardTab_1");
            yield return Shot("07-Weekly");
            if (!before)
            {
                var locked = Find("ClaimWeekly");
                Check(locked != null && !locked.IsInteractable(), "Haftalık ödül başlangıçta kilitli");
                int calls = 0;
                locked.onClick.AddListener(() => calls++);
                Dispatch(locked, Pointer(locked));
                Check(calls == 0, "Kilitli dokunuş işlem başlatmadı");
                yield return new WaitForSecondsRealtime(.08f);
                Check(locked.transform.Find("PremiumFace").localScale.x < 1, "Kilitli dokunuş görsel geri bildirim verdi");
            }
            Click("RewardHubClose");
            yield return null;
            if (!before)
            {
                Click("NavTeam");
                yield return Until(() => Find("ClanClose") != null, "Klan düğmesi klan salonunu açtı");
                yield return null;
                Click("ClanClose");
                yield return null;
                Click("NavInventory");
                yield return Until(() => Find("ShopClose") != null, "Eski Envanter girişi mağazayı açtı");
                yield return null;
                Click("ShopClose");
                yield return null;
            }
            Click("NavMap");
            yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true, "LevelMap hazır");
            yield return Shot("08-Map");
            yield return Ratios("ChapterClose", "JourneyFocus", "JourneyMotion");
            if (!before)
            {
                var lockedNode = FindObjectsByType<MapPoint>(FindObjectsSortMode.None).First(p =>
                    p.kind == MapPointKind.LevelSlot && p.regionIndex == 0 && p.slotIndex == 1);
                Click(lockedNode.name);
                yield return Until(() => Find("MapInfoClose") != null, "Kilitli taş gerçek harita bilgisini açtı");
                yield return null;
                Click("MapInfoClose");
                yield return null;
            }
            if (Find("ChapterClose") != null) { Click("ChapterClose"); yield return null; }
            Click("MapHome");
            yield return Until(() => SceneManager.GetActiveScene().name == "Home" && Find("Play") != null, "Haritadan ana menüye dönüldü");
            yield return null;
            Click("Play", true);
            if (!before) Check(Find("Play").GetComponent<PremiumPlayButton>().AcceptedClicks == 1, "Oyna çift dokunuşta bir geçiş hazırladı");
            yield return Until(() => SceneManager.GetActiveScene().name == "Gameplay" && Find("Pause") != null, "Oyna Gameplay açtı");
            FindFirstObjectByType<UI.PawGuide>()?.Close();
            yield return Shot("09-Gameplay");
            yield return Ratios("Pause", "Hint", "ExtraBottle", "Shuffle", "Restart", "Home");
            if (!before)
            {
                int hints = GameplayProgress.Current.Remaining(BoosterKind.Hint);
                Click("Hint");
                Check(GameplayProgress.Current.Remaining(BoosterKind.Hint) == hints - 1, "İpucu düğmesi mevcut yardımcı gücü kullandı");
            }
            Click("Pause");
            yield return Shot("10-Pause");
            yield return Ratios("Resume", "PauseRestart", "PauseHome");
            Click("Resume");
            yield return Until(() => Find("Resume") == null, "Devam Et oyunu sürdürdü");
        }
    }
}
#endif
