#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Mewgic.Clans;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace Mewgic
{
    public sealed class FullGamePreview : MonoBehaviour
    {
        public const string SessionKey = "Mewgic.VisualQA.Active";
        public const string VerifyKey = "Mewgic.VisualQA.Verify";
        const string Evidence = "Library/MewgicVisualQA/";
        static readonly string[] SaveSuffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly string[] Markers =
        {
            "NavEvents","Bottle 1","Win panel","MapHome","MagicShop","ClanBanner","LeagueSelf",
            "EventOpen_daily_spell","DailyClose","ChapterClose","ChapterClose"
        };
        public static FullGamePreview Active { get; private set; }
        public bool Busy { get; private set; }
        public string Status { get; private set; } = "Ana menü";
        Process server;
        string database;
        bool background;
        readonly List<string> errors = new(), checks = new();
        Dictionary<string,string> playerBefore;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void StartIsolated()
        {
            if (!SessionState.GetBool(SessionKey,false)) return;
            var host = new GameObject("MeWGic Visual QA");
            DontDestroyOnLoad(host);
            Active = host.AddComponent<FullGamePreview>();
            Active.background = Application.runInBackground;
            Application.runInBackground = true;
            Active.playerBefore = PlayerSave();
            Application.logMessageReceived += Active.Log;
            GameplayProgress.UseTestProfile(Fixture(0));
            GameplayLevelCatalog.ResetSelection();
            ClanClient.TestEndpoint("http://127.0.0.1:1");
            RewardedVictoryAds.Provider = new PreviewAd();
        }

        void Start()
        {
            Size(1600);
            if (SessionState.GetBool(VerifyKey,false)) StartCoroutine(Drive(Verify(),true));
            else StartCoroutine(Drive(Warmup(),false));
        }

        IEnumerator Warmup()
        {
            yield return Until(() => GameObject.Find("NavEvents") != null && L.Ready,"Ana menü");
            yield return null;
            if (GameObject.Find("ArrivalDismiss") != null) Press("ArrivalDismiss");
            yield return EnsureBackend();
        }

        void OnDestroy()
        {
            if (Active != this) return;
            Application.logMessageReceived -= Log;
            Application.runInBackground = background;
            Active = null;
            GameplayLevelCatalog.ResetSelection();
            GameplayProgress.RestorePlayerProfile();
            RewardedVictoryAds.Provider = null;
            if (server == null) return;
            try
            {
                if (!server.HasExited) server.Kill();
                server.WaitForExit(2000);
                if (server.HasExited)
                    foreach (string suffix in new[] { "", "-wal", "-shm" })
                        File.Delete(database+suffix);
            }
            catch (IOException) { }
            catch (InvalidOperationException) { }
            finally { server.Dispose(); }
        }

        static ProgressData Fixture(int completed)
        {
            var data = new ProgressData();
            data.presentation.manualLocale = "tr";
            data.presentation.coins = 550;
            data.presentation.motionPreferenceSet = true;
            for (int i=1;i<=completed;i++)
                data.levels.Add(new LevelRecord { level=i, bestStars=(i%3)+1, rewardClaimed=true });
            return data;
        }

        public static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size")
            .Invoke(null,new object[] { height });

        public void ResetPreview()
        {
            if (Busy) return;
            GameplayProgress.UseTestProfile(Fixture(0));
            GameplayLevelCatalog.ResetSelection();
            Navigate(0,1,3);
        }

        public void Navigate(int page,int level,int stars)
        {
            if (!Busy) StartCoroutine(Drive(OpenPage(page,level,stars),false));
        }

        IEnumerator Drive(IEnumerator operation,bool verifying)
        {
            Busy = true;
            string failure = null;
            var stack = new Stack<IEnumerator>();
            stack.Push(operation);
            while (stack.Count > 0)
            {
                bool moved;
                object current;
                try { moved=stack.Peek().MoveNext(); current=moved ? stack.Peek().Current : null; }
                catch (Exception error) { failure=error.GetBaseException().Message; break; }
                if (!moved) stack.Pop();
                else if (current is IEnumerator nested && current is not CustomYieldInstruction) stack.Push(nested);
                else yield return current;
            }
            Busy = false;
            Status = failure ?? "Hazır · İzole önizleme";
            if (!verifying) yield break;
            var after = PlayerSave();
            bool intact = playerBefore.All(pair => after[pair.Key] == pair.Value);
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Evidence+"RESULT.json",Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success=failure == null && errors.Count == 0 && intact, failure, errors, checks, playerSaveIntact=intact
            },Newtonsoft.Json.Formatting.Indented));
        }

        IEnumerator OpenPage(int page,int level,int stars)
        {
            Status = "Ekran açılıyor…";
            if (page is 1 or 2)
            {
                GameplayProgress.UseTestProfile(Fixture(level-1));
                yield return Await(GameplayLevelCatalog.PrepareAsync(level));
                if (!GameplayLevelCatalog.Select(level,GameplayProgress.Current))
                    throw new InvalidOperationException("Test bölümü seçilemedi.");
                yield return SceneManager.LoadSceneAsync("Gameplay");
                yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == level && L.Ready,"Oyun sahnesi");
                if (page == 2)
                {
                    var app = FindFirstObjectByType<PrototypeApp>();
                    const BindingFlags hidden = BindingFlags.Instance|BindingFlags.NonPublic;
                    var puzzle = (Puzzle)typeof(PrototypeApp).GetField("puzzle",hidden).GetValue(app);
                    var solution = PuzzleSolver.Solve(puzzle.Export());
                    if (solution.Status != SearchStatus.Solved) throw new InvalidOperationException("Önizleme bulmacası çözülemedi.");
                    foreach (var move in solution.Moves) puzzle.Pour(move.Source,move.Target);
                    if (!puzzle.Won) throw new InvalidOperationException("Önizleme çözümü tamamlanmadı.");
                    typeof(PrototypeApp).GetField("completionHandled",hidden).SetValue(app,true);
                    var ledger = GameplayProgress.Current;
                    string receipt = Guid.NewGuid().ToString("N");
                    if (!ledger.Claim(level,stars,true,out _,receipt))
                        throw new InvalidOperationException("Önizleme sonucu hazırlanamadı.");
                    FindFirstObjectByType<UI.PawGuide>()?.Close();
                    FindFirstObjectByType<GameplayEndScreen>().Show(level,stars,true,true,ledger.LastCompletionReward,ledger,receipt);
                    typeof(PrototypeApp).GetMethod("Refresh",hidden).Invoke(app,new object[] { false });
                }
            }
            else if (page is 3 or 9 or 10)
            {
                if (page == 9) GameplayProgress.UseTestProfile(Fixture(99));
                if (page == 10)
                {
                    GameplayProgress.UseTestProfile(Fixture(98));
                    if (!GameplayProgress.Current.Claim(99,3,true,out _))
                        throw new InvalidOperationException("Harita yürüyüşü hazırlanamadı.");
                }
                yield return SceneManager.LoadSceneAsync("LevelMap");
                yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true && L.Ready,"Harita");
                if (page == 9) FindFirstObjectByType<MapWorld>().FocusCurrent();
            }
            else
            {
                if (page is >= 5 and <= 7) yield return EnsureBackend();
                yield return SceneManager.LoadSceneAsync("Home");
                yield return Until(() => GameObject.Find("NavEvents") != null && L.Ready,"Ana menü");
                yield return null;
                if (GameObject.Find("ArrivalDismiss") != null) Press("ArrivalDismiss");
                yield return null;
                switch (page)
                {
                    case 4: Press("NavInventory"); break;
                    case 5: Press("NavTeam"); break;
                    case 6:
                        Press("NavTeam");
                        yield return Until(() => GameObject.Find("ClanLeague") != null,"Lig girişi");
                        yield return null;
                        Press("ClanLeague");
                        break;
                    case 7: Press("NavEvents"); break;
                    case 8: Press("MissionsButton"); break;
                }
            }
            yield return Until(() => GameObject.Find(Markers[page]) != null,Markers[page]);
            yield return null;
        }

        IEnumerator EnsureBackend()
        {
            if (server != null && !server.HasExited && ClanClient.SignedIn) yield break;
            string dll = Path.GetFullPath("tools/ClanServer/bin/Release/net10.0/ClanServer.dll");
            if (!File.Exists(dll)) throw new InvalidOperationException("Yerel API gerekli. Repo kökünde dotnet build tools/ClanServer -c Release çalıştır.");
            if (server != null)
            {
                if (!server.HasExited) server.Kill();
                server.WaitForExit(2000);
                server.Dispose();
                foreach (string suffix in new[] { "", "-wal", "-shm" }) File.Delete(database+suffix);
            }
            var probe = new TcpListener(IPAddress.Loopback,0);
            probe.Start();
            int port = ((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();
            string address = $"http://127.0.0.1:{port}";
            Directory.CreateDirectory(Evidence);
            database = Path.GetFullPath(Evidence+Guid.NewGuid().ToString("N")+".db");
            var start = new ProcessStartInfo("dotnet") { UseShellExecute=false, CreateNoWindow=true };
            foreach (string arg in new[] { dll,"--urls",address,"--Database",database,"--environment","Development","--Postgres:ConnectionString","" })
                start.ArgumentList.Add(arg);
            // Isolate the helper from the workstation's possible cloud or service configuration.
            start.Environment.Remove("Postgres__ConnectionString");
            start.Environment.Remove("Render__EdgeTls");
            start.Environment.Remove("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES");
            start.Environment["DOTNET_ENVIRONMENT"] = "Development";
            server = Process.Start(start);
            bool ready = false;
            for (int i=0;i<24 && !ready && !server.HasExited;i++)
            {
                using var request = UnityWebRequest.Get(address+"/health");
                request.timeout=1;
                yield return request.SendWebRequest();
                ready = request.result == UnityWebRequest.Result.Success;
                if (!ready) yield return new WaitForSecondsRealtime(.2f);
            }
            if (!ready) throw new InvalidOperationException("Yalıtılmış yerel API başlatılamadı.");
            ClanClient.TestEndpoint(address);
            yield return Await(ClanClient.SignIn(true,"preview_"+Guid.NewGuid().ToString("N")[..8],Guid.NewGuid().ToString("N")));
            yield return Await(ClanClient.Save(null,new ClanDraft
                { name="Görsel QA Patileri", description="Yalıtılmış yerel önizleme", announcement="Görsel test verisi", open=true }));
            yield return Await(ClanClient.Me());
            ClanClient.Submit(1,new[] { 0,2,1,0,1,2 });
            ClanClient.Submit(2,new[] { 0,3,2,0,1,2,1,3 });
            yield return Await(ClanClient.Flush());
        }

        IEnumerator Verify()
        {
            Directory.CreateDirectory(Evidence);
            foreach (int page in Enumerable.Range(0,10))
            {
                yield return OpenPage(page,1,3);
                checks.Add(Markers[page]);
                if (page is 2 or 7 or 9)
                {
                    yield return new WaitForSecondsRealtime(1.8f);
                    ScreenCapture.CaptureScreenshot(Evidence+$"Page-{page}.png");
                    yield return null;
                }
            }
            foreach (int stars in new[] {1,2,3})
            {
                yield return OpenPage(2,2,stars);
                yield return new WaitForSecondsRealtime(1.8f);
                if (GameplayProgress.Current.BestStars(2) != stars)
                    throw new InvalidOperationException("Yıldız örneği eşleşmedi.");
                ScreenCapture.CaptureScreenshot(Evidence+$"Stars-{stars}.png");
                checks.Add(stars+" yıldız örneği");
                yield return null;
            }
            var ledger = GameplayProgress.Current;
            long gold = ledger.Presentation.coins;
            int bonus = ProgressLedger.RewardedVictoryTotal-ledger.LastCoinReward;
            Press("WatchRewardedAd",true);
            yield return Until(() => ledger.Presentation.coins == gold+bonus,"Yerel örnek reklam ödülü");
            checks.Add("Yerel örnek reklam ödülü");
            Press("NextLevel",true);
            yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == 3,"Sonraki Bölüm 2 → 3");
            checks.Add("Sonraki Bölüm 2 → 3");
            foreach (int level in new[] {1,2,3,4})
            {
                yield return OpenPage(1,level,3);
                yield return Until(() => FindFirstObjectByType<UI.PawGuide>()?.OnboardingVisible == true,"Pati "+level);
                checks.Add("Pati "+level);
            }
            yield return OpenPage(10,100,3);
            var map = FindFirstObjectByType<MapWorld>();
            if (!map.JourneyRunning) throw new InvalidOperationException("Kristal Şelaleler yürüyüşü başlamadı.");
            checks.Add("Kristal Şelaleler · Kedi 99 → 100");
            yield return Until(() => !map.JourneyRunning,"Kedi yürüyüşü tamamlandı");
            foreach (int height in new[] {1600,1950,2000})
            {
                Size(height);
                yield return Until(() => Screen.height == height && Screen.width == 900,"900×"+height);
                yield return OpenPage(4,1,3);
                Canvas.ForceUpdateCanvases();
                foreach (var label in GameObject.Find("MagicShop").GetComponentsInChildren<TMPro.TMP_Text>())
                {
                    label.ForceMeshUpdate();
                    if (label.isTextOverflowing) throw new InvalidOperationException("Mağaza metni taştı: "+label.name);
                }
                checks.Add("Mağaza 900×"+height);
            }
            Size(1600);
            yield return OpenPage(0,1,3);
        }

        static void Press(string name,bool raycast = false)
        {
            var button = GameObject.Find(name)?.GetComponent<Button>();
            if (button == null || !button.IsInteractable()) throw new InvalidOperationException("Düğme hazır değil: "+name);
            if (!raycast) { button.onClick.Invoke(); return; }
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button=PointerEventData.InputButton.Left,
                position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer,hits);
            if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
                throw new InvalidOperationException("Düğme raycast hedefi değil: "+name);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }

        static IEnumerator Until(Func<bool> ready,string label)
        {
            double end = Time.realtimeSinceStartupAsDouble+15;
            while (!ready() && Time.realtimeSinceStartupAsDouble < end) yield return null;
            if (!ready()) throw new TimeoutException("Önizleme bekleme süresi: "+label);
        }

        static IEnumerator Await(Task task)
        {
            yield return Until(() => task.IsCompleted,"Yerel görev");
            task.GetAwaiter().GetResult();
        }

        static Dictionary<string,string> PlayerSave() => SaveSuffixes.ToDictionary(s => s,
            s => PlayerPrefs.HasKey(GameplayProgress.StorageKey+s) ? PlayerPrefs.GetString(GameplayProgress.StorageKey+s) : null);

        void Log(string message,string stack,LogType type)
        {
            if (type is LogType.Error or LogType.Assert or LogType.Exception) errors.Add(message);
        }

        sealed class PreviewAd : IRewardedVictoryAd
        {
            public RewardedVictoryAdState State => RewardedVictoryAdState.Ready;
            public bool IsPresenting => false;
            public Task<RewardedVictoryAdResult> ShowAsync(string receipt) => Task.FromResult(RewardedVictoryAdResult.Rewarded);
        }
    }
}
#endif
