#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mewgic.Clans;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.UI;
using Stopwatch = System.Diagnostics.Stopwatch;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class ReleaseIntegrationProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/ReleaseReadiness/Integrated/";
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo Profile = typeof(GameplayProgress).GetField("current",BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo PuzzleField = typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance | BindingFlags.NonPublic);
        static ReleaseIntegrationProbe active;
        string key;
        bool passed;
        readonly List<string> checks = new(), errors = new();
        Dictionary<string,string> original;
        bool background;
        Puzzle Board => (Puzzle)PuzzleField.GetValue(FindFirstObjectByType<PrototypeApp>());

        [MenuItem("Mewgic/QA/Run Release Integration")]
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(FixturePath)) throw new InvalidOperationException("Run tools/ClanServer/release_unity_qa.py first.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Önce açık sahneyi kaydet.");
            EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
            new GameObject("Release Integration QA").AddComponent<ReleaseIntegrationProbe>();
            Size(1600);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Mewgic/QA/Validate Release Generator")]
        public static void ValidateGenerator()
        {
            const int count = 1024;
            var pool = Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratedLevel[]>(
                Resources.Load<TextAsset>("Levels/GeneratedFallbacksV2").text);
            foreach (var fallback in pool) fallback.Validate();
            var watch = Stopwatch.StartNew();
            var timings = new List<double>();
            var signatures = new HashSet<string>();
            int fallbackCount = 0, determinismChecks = 0;
            string previous = null;
            for (int id = 3; id < count+3; id++)
            {
                var request = WorldMapRules.Request(id);
                long start = watch.ElapsedTicks;
                var level = LevelGenerator.Resolve(request,null,pool).level;
                timings.Add((watch.ElapsedTicks-start)*1000.0/Stopwatch.Frequency);
                if (level == null || level.level != id || level.signature == previous || level.difficulty.measured > request.target)
                    throw new InvalidOperationException("Üretim, kimlik, zorluk veya komşu tekrar: "+id);
                level.Validate();
                signatures.Add(level.signature);
                previous = level.signature;
                if (level.fallback) fallbackCount++;
                if (id%32 != 0 && id is not (3 or 9 or 10 or 11 or 99 or 100 or 101)) continue;
                var repeated = LevelGenerator.Resolve(WorldMapRules.Request(id),null,pool).level;
                if (repeated == null || !LevelGenerator.Exact(level.Export(),repeated.Export()))
                    throw new InvalidOperationException("Arşivsiz yeniden üretim değişti: "+id);
                determinismChecks++;
            }
            timings.Sort();
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output+"Generator.json",Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success=true, count, adjacentRepeats=0, uniqueLayouts=signatures.Count, fallbackCount, determinismChecks,
                editor=Application.unityVersion, resolveP95Ms=timings[(int)(timings.Count*.95)], resolveMaxMs=timings.Last()
            },Newtonsoft.Json.Formatting.Indented));
        }

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            original = PlayerSave();
            background = Application.runInBackground;
            Application.runInBackground = true;
            fixture = JsonUtility.FromJson<Fixture>(File.ReadAllText(FixturePath));
            key = fixture.key;
            if (fixture.phase is not (1 or 2) || key == null || !key.StartsWith("Mewgic.QA.Release.",StringComparison.Ordinal) ||
                !Guid.TryParseExact(key[18..],"N",out _))
                throw new InvalidOperationException("Invalid isolated test profile.");
            if (fixture.phase == 1)
            {
                var data = new ProgressData();
                data.presentation.manualLocale = "tr";
                PlayerPrefs.SetString(key,JsonUtility.ToJson(data));
                PlayerPrefs.Save();
            }
            GameplayProgress.UseTestProfile();
            Profile.SetValue(null,GameplayProgress.Open(key));
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived += Log;
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.runInBackground = background;
            Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
            if (fixture != null && (fixture.phase == 2 || !passed))
            {
                foreach (string suffix in Suffixes.Concat(new[] { ".qaCoins", ".qaHints" }))
                    PlayerPrefs.DeleteKey(key + suffix);
                PlayerPrefs.Save();
            }
        }

        void Log(string message,string trace,LogType type)
        {
            if (type is LogType.Error or LogType.Assert or LogType.Exception) errors.Add(message);
        }

        static Dictionary<string,string> PlayerSave() => Suffixes.ToDictionary(s => s, s =>
            PlayerPrefs.HasKey(GameplayProgress.StorageKey+s) ? PlayerPrefs.GetString(GameplayProgress.StorageKey+s) : null);
        static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size")
            .Invoke(null,new object[] { height });
        static GameObject Find(string name) => GameObject.Find(name);
        static string Text(string name) => Find(name)?.GetComponent<TMP_Text>()?.text;
        static void Enter(string name,string value) => Find(name).GetComponent<TMP_InputField>().text = value;

        void Check(bool condition,string text)
        {
            if (!condition) throw new InvalidOperationException(text);
            checks.Add(text);
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
                catch (Exception error) { failure = error.ToString(); break; }
                if (!more) stack.Pop();
                else if (item is IEnumerator child && item is not CustomYieldInstruction) stack.Push(child);
                else yield return item;
            }
            var after = PlayerSave();
            bool intact = original.All(p => after[p.Key] == p.Value);
            passed = intact && failure == null && errors.Count == 0;
            File.WriteAllText(Output+"Phase-"+fixture.phase+".json",Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = passed, fixture.phase, editor = Application.unityVersion,
                transport = "UnityWebRequest → live HTTP API → SQLite", count = checks.Count, checks, errors, failure, playerSaveIntact = intact
            },Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        IEnumerator Until(Func<bool> condition,string description)
        {
            float deadline = Time.realtimeSinceStartup + 24;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Check(condition(),description + (condition() ? "" : " · " + Text("EventNotice")));
        }

        IEnumerator Await(Task task,string name)
        {
            yield return Until(() => task.IsCompleted,name);
            task.GetAwaiter().GetResult();
        }

        void Click(string name,bool raycast = true)
        {
            var button = Find(name)?.GetComponent<Button>();
            Check(button != null && button.IsInteractable(),name + " etkileşimli");
            if (!raycast) { button.onClick.Invoke(); return; }
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var point = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(point,hits);
            var receiver = hits.Count == 0 ? null : ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Check(receiver == button.gameObject,name + " gerçek raycast" + (hits.Count == 0 ? " · isabet yok" : " → " + hits[0].gameObject.name));
            ExecuteEvents.Execute(receiver,point,ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Shot(string file)
        {
            yield return new WaitForSecondsRealtime(.25f);
            var path = Output + file + ".png";
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(.35f);
            Check(File.Exists(path),file + " gerçek Game View");
        }

        IEnumerator Sign(bool register,string handle,string password)
        {
            yield return Until(() => Find("ClanHandle") != null,"Hesap alanları açıldı");
            yield return new WaitForSecondsRealtime(.20f);
            Enter("ClanHandle",handle);
            Enter("ClanPassword",password);
            Click(register ? "ClanRegister" : "ClanSignIn");
            yield return Until(() => Find("ClanHall") != null && ClanClient.SignedIn,"Gerçek HTTP oturumu açıldı");
            yield return new WaitForSecondsRealtime(.20f);
        }

        IEnumerator Win(int level)
        {
            yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == level && Find("Bottle 1") != null,
                "Gameplay " + level + " açıldı");
            FindFirstObjectByType<PawGuide>()?.Close();
            var solution = PuzzleSolver.Solve(Board.Export());
            Check(solution.Status == SearchStatus.Solved && solution.Moves.Count > 0,"Bölüm " + level + " gerçek çözüm");
            foreach (var move in solution.Moves)
            {
                int next = Board.Moves + 1;
                Click("Bottle " + (move.Source + 1),false);
                yield return null;
                Click("Bottle " + (move.Target + 1),false);
                yield return Until(() => Board.Moves == next && (Board.Won || FindFirstObjectByType<PrototypeApp>().GuideCanInteract),"Döküş uygulandı");
            }
            yield return Until(() => Find("Win panel") != null,"Sonuç açıldı");
            yield return new WaitForSecondsRealtime(.45f);
            yield return Await(ClanClient.Flush(),"Bölüm kanıtı HTTP üzerinden gönderildi");
        }

        [Serializable] sealed class Fixture { public string endpoint,handle,password,key; public int phase; }
        Fixture fixture;
        const string FixturePath = "Library/ReleaseQaFixture.json";
        ProgressLedger Ledger => GameplayProgress.Current;

        IEnumerator ReadyHome()
        {
            float started = Time.realtimeSinceStartup;
            string origin = SceneManager.GetActiveScene().name;
            yield return Until(() => L.Ready && SceneManager.GetActiveScene().name == "Home" &&
                Find("NavEvents") != null && BootLoadingController.Active == null,origin+" → Home tamamlandı");
            checks.Add($"Editor {origin} → Home: {Time.realtimeSinceStartup-started:0.00} sn");
            yield return new WaitForSecondsRealtime(.5f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
        }

        IEnumerator Account(bool register)
        {
            Click("NavTeam");
            yield return Sign(register,fixture.handle,fixture.password);
            if (register)
            {
                Click("ClanCreate");
                yield return null;
                Enter("ClanNameInput","Ay Patisi QA");
                Enter("ClanDescriptionInput","Yalıtılmış yayın denetimi");
                Click("ClanSave");
            }
            yield return Until(() => Find("ClanBanner") != null,"Klan üyeliği ekranda");
            yield return new WaitForSecondsRealtime(.3f);
            yield return Shot(fixture.phase == 1 ? "04-Clan" : "07-Restored-Clan");
            Click("ClanClose");
            yield return new WaitForSecondsRealtime(.2f);
        }

        IEnumerator BackFromResult()
        {
            Click("ResultHome");
            yield return Until(() => Find("MapHome") != null && SceneManager.GetActiveScene().name == "LevelMap","Harita açıldı");
            FindFirstObjectByType<Mewgic.Map.MapWorld>()?.SkipJourney();
            yield return new WaitForSecondsRealtime(.4f);
            if (Find("ChapterClose") != null)
            {
                Click("ChapterClose");
                yield return new WaitForSecondsRealtime(.25f);
            }
            Click("MapHome");
            yield return ReadyHome();
        }

        IEnumerator Run()
        {
            ClanClient.TestEndpoint(fixture.endpoint);
            yield return ReadyHome();
            var config = EconomyConfig.Current;
            if (fixture.phase == 2)
            {
                Check(Ledger.Completed(1) && Ledger.Completed(2) && Ledger.Completed(3) &&
                    Ledger.BestStars(1) == 3 && Ledger.CurrentUnlockedLevel == 4,"Yeni Play oturumunda üç bölüm, yıldız ve açık 4 korundu");
                Check(Ledger.Presentation.coins >= 0 && Ledger.Presentation.coins == PlayerPrefs.GetInt(key+".qaCoins") &&
                    Ledger.Remaining(BoosterKind.Hint) == PlayerPrefs.GetInt(key+".qaHints") &&
                    Ledger.DailySnapshot.claimed[(int)DailyTaskKind.Wins],"Bakiye, güçlendirici ve günlük makbuzu yeni oturumda aynı");
                yield return Account(false);
                var board = ClanClient.Events();
                yield return Await(board,"Yeni oturumda gerçek etkinlik kaydı okundu");
                var daily = board.Result.cards.First(c => c.kind == "daily_spell" && c.status == 1);
                Check(daily.points == 100 && daily.milestones[0].claimed && Ledger.HasLeagueReward(daily.milestones[0].receipt),
                    "Etkinlik puanı ve aynı teslim makbuzu korunuyor");
                var league = ClanClient.League(false,null);
                yield return Await(league,"Yeni oturumda gerçek lig kaydı okundu");
                Check(league.Result.self?.wins == 1 && league.Result.self.points > 0,"Önceki lig puanı geri geldi");
                Click("Play");
                yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == 4,"Oyuncu açık 4. bölümden devam etti");
                yield return Shot("08-Restored-Level4");
                yield break;
            }
            Check(Ledger.Presentation.coins == config.initialGold && Ledger.CurrentUnlockedLevel == 1,"Taze oyuncu kaydı ve 1. bölüm");
            Click("Play");
            yield return Until(() => FindFirstObjectByType<PawGuide>()?.OnboardingVisible == true,"İlk bölümün pati öğreticisi göründü");
            yield return Shot("01-First-Tutorial");
            yield return Win(1);
            Check(Ledger.BestStars(1) == 3 && Ledger.Presentation.coins > 0,"İlk kazanışta yıldız ve altın");
            yield return new WaitForSecondsRealtime(1);
            yield return Shot("02-First-Victory");
            Click("NextLevel");
            yield return Win(2);
            Check(Ledger.CurrentUnlockedLevel == 3,"Sonraki Bölüm 2'yi oynattı, 3 açıldı");
            yield return BackFromResult();

            Click("NavInventory");
            yield return new WaitForSecondsRealtime(.25f);
            long coins = Ledger.Presentation.coins;
            int hints = Ledger.Remaining(BoosterKind.Hint);
            Click("BuyHint");
            yield return new WaitForSecondsRealtime(.2f);
            Click("ShopConfirm");
            yield return Until(() => Ledger.Presentation.coins == coins-config.hintPrice &&
                Ledger.Remaining(BoosterKind.Hint) == hints+config.boosterQuantity,"Kazanılan altınla ipucu satın alındı");
            yield return Shot("03-Purchased");
            Click("ShopClose");
            yield return new WaitForSecondsRealtime(.25f);

            Click("MissionsButton");
            yield return new WaitForSecondsRealtime(.25f);
            coins = Ledger.Presentation.coins;
            Click("DailyAction_1");
            yield return Until(() => Ledger.DailySnapshot.claimed[(int)DailyTaskKind.Wins] &&
                Ledger.Presentation.coins == coins+config.taskWinsGold,"İki kazanışın günlük ödülü yazıldı");
            Click("DailyClose");
            yield return new WaitForSecondsRealtime(.25f);

            Click("NavEvents");
            yield return Until(() => Find("EventOpen_daily_spell") != null,"Misafir etkinlik takvimini görüyor");
            Click("EventClose");
            yield return new WaitForSecondsRealtime(.2f);
            yield return Account(true);

            Click("Play");
            yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == 3 && Find("Hint") != null,"Üçüncü bölüm açıldı");
            FindFirstObjectByType<PawGuide>()?.Close();
            yield return new WaitForSecondsRealtime(.35f);
            hints = Ledger.Remaining(BoosterKind.Hint);
            Click("Hint");
            Check(Ledger.Remaining(BoosterKind.Hint) == hints-1,"Satın alınan ipucu gerçek oyunda harcandı");
            int selected = FindFirstObjectByType<PrototypeApp>().GuideSelectedBottle;
            if (selected >= 0) Find("Bottle "+(selected+1)).GetComponent<Button>().onClick.Invoke();
            yield return Win(3);
            yield return BackFromResult();
            Click("NavEvents");
            yield return Until(() => Find("EventOpen_daily_spell") != null,"Üyelik sonrası etkinlik merkezi açıldı");
            yield return new WaitForSecondsRealtime(.25f);
            Click("EventOpen_daily_spell");
            yield return new WaitForSecondsRealtime(.2f);
            coins = Ledger.Presentation.coins;
            Click("EventClaim_0");
            yield return Until(() => Ledger.Presentation.coins == coins+50,"Doğrulanmış etkinlik ödülü cüzdana eklendi");
            yield return Shot("05-Event-Claimed");
            Click("EventClose");
            yield return new WaitForSecondsRealtime(.25f);
            Click("NavTeam");
            yield return Until(() => Find("ClanLeague") != null,"Klan salonu lig girişini gösterdi");
            yield return new WaitForSecondsRealtime(.25f);
            Click("ClanLeague");
            yield return Until(() => Find("LeagueSelf") != null,"Gerçek kazanıştan lig sırası oluştu");
            var leagueBefore = ClanClient.League(false,null);
            yield return Await(leagueBefore,"Lig puanı sunucudan geldi");
            Check(leagueBefore.Result.self?.wins == 1 && leagueBefore.Result.self.points > 0,"Lig ilk çevrimiçi kazanışı saydı");
            yield return Shot("06-League");
            PlayerPrefs.SetInt(key+".qaCoins",checked((int)Ledger.Presentation.coins));
            PlayerPrefs.SetInt(key+".qaHints",Ledger.Remaining(BoosterKind.Hint));
            PlayerPrefs.Save();
        }
    }
}
#endif
