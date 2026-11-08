#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.UI;
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
    public sealed class VictoryReferenceProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/VictoryReference/";
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo Profile = typeof(GameplayProgress).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo PuzzleField = typeof(PrototypeApp).GetField("puzzle", BindingFlags.Instance | BindingFlags.NonPublic);
        static VictoryReferenceProbe active;
        readonly List<string> checks = new(), errors = new();
        string key;
        bool background;
        Dictionary<string, string> original;
        ProgressLedger Ledger => GameplayProgress.Current;
        Puzzle Board => (Puzzle)PuzzleField.GetValue(FindFirstObjectByType<PrototypeApp>());

        [MenuItem("Mewgic/QA/Run Victory Reference")]
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Açık sahnede kaydedilmemiş değişiklik var.");
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
            new GameObject("Victory Reference QA").AddComponent<VictoryReferenceProbe>();
            Size(1600);
            EditorApplication.isPlaying = true;
        }

        static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor")
            .GetMethod("Size").Invoke(null, new object[] { height });

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            background = Application.runInBackground;
            Application.runInBackground = true;
            original = PlayerSave();
            Application.logMessageReceived += Log;
            Seed();
        }

        static Dictionary<string, string> PlayerSave() => Suffixes.ToDictionary(
            s => s, s => PlayerPrefs.HasKey(GameplayProgress.StorageKey + s)
                ? PlayerPrefs.GetString(GameplayProgress.StorageKey + s) : null);

        void Clear()
        {
            if (key == null) return;
            foreach (string suffix in Suffixes) PlayerPrefs.DeleteKey(key + suffix);
            PlayerPrefs.Save();
        }

        void Seed()
        {
            Clear();
            key = "Mewgic.Tests.Victory." + Guid.NewGuid().ToString("N");
            var data = new ProgressData();
            data.presentation.manualLocale = "tr";
            data.presentation.motionPreferenceSet = true;
            PlayerPrefs.SetString(key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            GameplayProgress.UseTestProfile();
            Profile.SetValue(null, GameplayProgress.Open(key));
            GameplayLevelCatalog.ResetSelection();
        }

        void Log(string message, string stack, LogType type)
        {
            if (type is LogType.Error or LogType.Assert or LogType.Exception) errors.Add(message);
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.runInBackground = background;
            Application.logMessageReceived -= Log;
            RewardedVictoryAds.Provider = null;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
            Clear();
        }

        void Check(bool valid, string detail)
        {
            if (!valid) throw new InvalidOperationException(detail);
            checks.Add(detail);
        }

        IEnumerator Start()
        {
            var steps = new Stack<IEnumerator>();
            steps.Push(Run());
            string failure = null;
            while (steps.Count > 0)
            {
                bool more;
                object item;
                try { more = steps.Peek().MoveNext(); item = more ? steps.Peek().Current : null; }
                catch (Exception e) { failure = e.ToString(); break; }
                if (!more) steps.Pop();
                else if (item is IEnumerator child && item is not CustomYieldInstruction) steps.Push(child);
                else yield return item;
            }
            var current = PlayerSave();
            bool intact = original.All(pair => current[pair.Key] == pair.Value);
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "PLAY_MODE.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = failure == null && errors.Count == 0 && intact,
                editor = Application.unityVersion, count = checks.Count, checks, failure, errors, playerSaveIntact = intact
            }, Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        IEnumerator Until(Func<bool> predicate, string detail)
        {
            float deadline = Time.realtimeSinceStartup + 18;
            while (!predicate() && Time.realtimeSinceStartup < deadline) yield return null;
            Check(predicate(), detail);
        }

        IEnumerator Ready(int level = 1)
        {
            yield return Until(() => L.Ready && FindFirstObjectByType<GameplayHud>()?.State.Level == level &&
                GameObject.Find("Bottle 1") != null, $"Gameplay bölüm {level} hazır");
            FindFirstObjectByType<PawGuide>()?.Close();
            Check(L.Select("tr", false), "Türkçe sonuç metinleri etkin");
        }

        void Click(string name, bool raycast = false)
        {
            var button = GameObject.Find(name)?.GetComponent<Button>();
            Check(button != null && button.IsInteractable(), name + " etkileşimli");
            if (!raycast) { button.onClick.Invoke(); return; }
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            var hit = hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
            Check(hit == button.gameObject, name + " raycast doğru hedefe ulaştı");
            ExecuteEvents.Execute(hit, pointer, ExecuteEvents.pointerClickHandler);
        }

        static bool Route(int[][] data, int remaining, List<PuzzleMove> moves, HashSet<string> visited)
        {
            var current = new Puzzle(data);
            if (remaining == 0 || current.Won) return remaining == 0 && current.Won;
            if (!visited.Add(remaining + ":" + PuzzleSolver.StateKey(data))) return false;
            for (int source = 0; source < data.Length; source++)
                for (int target = 0; target < data.Length; target++)
                {
                    var next = new Puzzle(data);
                    if (next.Pour(source, target) == 0) continue;
                    moves.Add(new PuzzleMove(source, target));
                    if (Route(next.Export(), remaining - 1, moves, visited)) return true;
                    moves.RemoveAt(moves.Count - 1);
                }
            return false;
        }

        IEnumerator Win(int moves, int stars)
        {
            var route = new List<PuzzleMove>();
            Check(Route(Board.Export(), moves, route, new HashSet<string>()), $"{moves} geçerli döküşlük çözüm bulundu");
            foreach (var move in route)
            {
                int next = Board.Moves + 1;
                Click("Bottle " + (move.Source + 1));
                yield return null;
                Click("Bottle " + (move.Target + 1));
                yield return Until(() => Board.Moves == next && (Board.Won ||
                    FindFirstObjectByType<PrototypeApp>().GuideCanInteract), $"Döküş {next}/{moves}");
            }
            yield return Until(() => GameObject.Find("Win panel") != null, "Gerçek kazanış sonucu açıldı");
            Check(Board.Won && Ledger.BestStars(1) == stars &&
                FindFirstObjectByType<GameplayHud>().State.AwardedStars == stars, $"{stars} yıldız hesaplandı ve kaydedildi");
        }

        static RawImage Star(int index) => GameObject.Find("AwardStar" + index).GetComponent<RawImage>();
        static Button Watch => GameObject.Find("WatchRewardedAd").GetComponent<Button>();
        static string Label(string name) => GameObject.Find(name)?.GetComponent<TMP_Text>().text;

        IEnumerator Reveal(int awarded)
        {
            for (int i = 1; i <= awarded; i++)
            {
                int slot = i;
                yield return Until(() => Star(slot).color.a > .4f, $"Yıldız {slot} açıldı");
                for (int pending = slot + 1; pending <= awarded; pending++)
                    Check(Star(pending).color.a < .1f, $"Yıldız {pending}, yıldız {slot} açıldığında henüz görünmedi");
            }
            yield return new WaitForSecondsRealtime(1.7f);
            for (int i = 1; i <= 3; i++)
                Check(i <= awarded ? Star(i).color.a > .99f : Star(i).color.a < .5f,
                    $"{i}. yıldız, {awarded} yıldızlık sonuçta doğru parlaklıkta");
        }

        IEnumerator Capture(string name)
        {
            ScreenCapture.CaptureScreenshot(Output + name + ".png");
            yield return new WaitForSecondsRealtime(.35f);
            Check(File.Exists(Output + name + ".png"), name + " Game View kaydedildi");
        }

        void Layout(string label)
        {
            Canvas.ForceUpdateCanvases();
            var root = (RectTransform)GameObject.Find("Win panel").transform;
            var safe = (RectTransform)GameObject.Find("Safe area").transform;
            var corners = new Vector3[4];
            root.GetWorldCorners(corners);
            var box = safe.rect;
            foreach (var point in corners.Select(safe.InverseTransformPoint))
                Check(point.x >= box.xMin - .5f && point.x <= box.xMax + .5f &&
                    point.y >= box.yMin - .5f && point.y <= box.yMax + .5f, label + " panel Safe Area içinde");
            foreach (string name in new[] { "NextLevel", "Replay", "ResultHome", "WatchRewardedAd" })
            {
                var rect = (RectTransform)GameObject.Find(name).transform;
                rect.GetWorldCorners(corners);
                Check(corners[2].y - corners[0].y >= 44, label + " " + name + " dokunma yüksekliği ≥44 px");
            }
            foreach (var text in root.GetComponentsInChildren<TMP_Text>())
            {
                text.ForceMeshUpdate();
                Check(!text.isTextOverflowing, label + " " + text.name + " taşmıyor");
            }
        }

        sealed class Ad : IRewardedVictoryAd
        {
            public TaskCompletionSource<RewardedVictoryAdResult> Pending = new();
            public int Calls;
            public Task<RewardedVictoryAdResult> ShowAsync() { Calls++; return Pending.Task; }
        }

        void RewardModel()
        {
            var fixture = new ProgressData();
            fixture.presentation.coins = 20;
            fixture.levels.Add(new LevelRecord
            {
                level = 1, rewardClaimed = true, bestStars = 1, rewardedVictoryBase = 20, rewardedVictoryReceipt = "victory"
            });
            ProgressData disk = null;
            bool fail = true;
            var ledger = new ProgressLedger(fixture, data =>
            {
                if (fail) throw new IOException("İzole disk hatası");
                disk = data;
            });
            Check(!ledger.TryBoostVictory(1, "başka", out _), "Başka kazanış kimliği ödül alamaz");
            Check(!ledger.TryBoostVictory(1, "victory", out _) && ledger.Presentation.coins == 20 &&
                ledger.CanBoostVictory(1, "victory"), "Yazma hatası altın ve hak kaybına yol açmaz");
            fail = false;
            Check(ledger.TryBoostVictory(1, "victory", out int bonus) && bonus == 80 &&
                ledger.Presentation.coins == 100, "20 + 80 = 100, ödül ikinci kez eklenmedi");
            var reopened = new ProgressLedger(JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(disk)), _ => { });
            Check(!reopened.TryBoostVictory(1, "victory", out _) && reopened.Presentation.coins == 100,
                "Yeniden açılan kayıt aynı reklam ödülünü tekrar vermez");
        }

        IEnumerator Run()
        {
            Directory.CreateDirectory(Output);
            RewardModel();
            yield return Ready();
            yield return Win(6, 1);
            yield return Reveal(1);
            yield return Capture("01-OneStar");
            Click("ResultHome", true);
            yield return Until(() => SceneManager.GetActiveScene().name == "LevelMap", "Haritaya Dön LevelMap sahnesini açtı");

            Seed();
            SceneManager.LoadScene("Gameplay");
            yield return null;
            yield return Ready();
            yield return Win(4, 2);
            yield return Reveal(2);
            yield return Capture("02-TwoStars");
            Click("Replay", true);
            Check(Board.Moves == 0 && !Board.Won && GameObject.Find("Win panel") == null,
                "Tekrar Oyna aynı bölümü sıfırladı");

            Seed();
            SceneManager.LoadScene("Gameplay");
            yield return null;
            yield return Ready();
            yield return Win(3, 3);
            yield return Reveal(3);
            Check(Star(2).rectTransform.rect.width > Star(1).rectTransform.rect.width * 1.2f, "Orta yıldız daha büyük");
            Layout("9:16 TR");
            yield return Capture("03-ThreeStars-900x1600");
            foreach (int height in new[] { 1950, 2000 })
            {
                Size(height);
                yield return Until(() => Screen.height == height, "Game View " + height);
                yield return null;
                if (height == 2000)
                {
                    var safe = (RectTransform)GameObject.Find("Safe area").transform;
                    safe.anchorMin = new Vector2(.03f, .055f);
                    safe.anchorMax = new Vector2(.97f, .945f);
                    yield return null;
                }
                Layout("900x" + height);
                yield return Capture("04-ThreeStars-900x" + height);
            }
            foreach (string locale in L.Codes)
            {
                Check(L.Select(locale, false), locale + " dili seçildi");
                Layout("9:20 " + locale);
            }
            Check(L.Select("tr", false), "Türkçe geri yüklendi");

            long baseCoins = Ledger.Presentation.coins;
            Check(baseCoins == 50, "Gerçek ilk kazanış 50 altın kaydetti");
            Click("WatchRewardedAd", true);
            Check(Ledger.Presentation.coins == baseCoins && Label("RewardBreakdown") == L.Resolve(L.V("victory.ad_unavailable")),
                "Sağlayıcı yokken mesaj gösterildi ve altın verilmedi");
            var provider = new Ad();
            RewardedVictoryAds.Provider = provider;
            Click("WatchRewardedAd", true);
            Check(provider.Calls == 1 && !Watch.IsInteractable() &&
                !GameObject.Find("NextLevel").GetComponent<Button>().IsInteractable(), "Reklam sırasında yeniden tıklama ve navigasyon kilitli");
            provider.Pending.SetResult(RewardedVictoryAdResult.Closed);
            yield return Until(() => Watch.IsInteractable(), "Erken kapanan reklam sonrası tekrar denenebilir");
            Check(Ledger.Presentation.coins == baseCoins, "Erken kapanan reklam altın vermedi");
            provider.Pending = new TaskCompletionSource<RewardedVictoryAdResult>();
            Click("WatchRewardedAd", true);
            provider.Pending.SetResult(RewardedVictoryAdResult.Rewarded);
            yield return Until(() => Ledger.Presentation.coins == 100, "Onaylı reklam farkı ekledi ve toplam 100 oldu");
            var disk = GameplayProgress.Open(key);
            var record = disk.Snapshot.levels.Single(x => x.level == 1);
            Check(!Watch.IsInteractable() && provider.Calls == 2 && disk.Presentation.coins == 100 &&
                !disk.TryBoostVictory(1, record.lastCoinReceipt, out _), "Ödül kalıcı ve aynı reklam yeniden ödenmiyor");
            yield return Capture("05-AdRewardClaimed");
            Click("NextLevel", true);
            yield return Ready(2);
        }
    }
}
#endif
