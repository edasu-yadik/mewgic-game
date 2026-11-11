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
    public sealed partial class FullGamePreview : MonoBehaviour
    {
        public const string SessionKey = "Mewgic.VisualQA.Active";
        public const string VerifyKey = "Mewgic.VisualQA.Verify";
        public const string ReviewKey = "Mewgic.VisualQA.FinalReview";
        const string Evidence = "Library/MewgicVisualQA/";
        static readonly string[] SaveSuffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly string[] Markers =
        {
            "NavEvents","Bottle 1","Win panel","ContinuousRoadContent","MagicShop","ClanBanner","LeagueSelf",
            "EventOpen_daily_spell","DailyClose","ContinuousRoadContent","ContinuousRoadContent","ContinuousRoadContent"
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
            if (SessionState.GetBool(SingleMapKey,false)) StartCoroutine(Drive(VerifySingleMap(),true));
            else if (SessionState.GetBool(ReviewKey,false)) StartCoroutine(Drive(Review(),true));
            else if (SessionState.GetBool(VerifyKey,false)) StartCoroutine(Drive(Verify(),true));
            else StartCoroutine(Drive(Warmup(),false));
        }

        IEnumerator Warmup()
        {
            yield return Until(() => GameObject.Find("NavEvents") != null && L.Ready,"Ana menü");
            yield return null;
            if (GameObject.Find("ArrivalDismiss") != null) Press("ArrivalDismiss");
            yield return null;
            Press("NavInventory");
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
            bool verifying = SessionState.GetBool(VerifyKey, false);
            data.presentation.coins = verifying ? 550 : 4200;
            data.presentation.gems = verifying ? 0 : 3600;
            if (completed >= EconomyConfig.Current.colorSwapUnlock - 1) data.colorSwap = 1;
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
            bool review = SessionState.GetBool(ReviewKey, false);
            string output = SessionState.GetBool(SingleMapKey,false) ? SingleMapOutput : review ? ReviewOutput : Evidence;
            Directory.CreateDirectory(output);
            File.WriteAllText(output+"RESULT.json",Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success=failure == null && errors.Count == 0 && intact, failure, errors, checks, playerSaveIntact=intact,
                performance = review ? reviewPerformance : null, overflow = review ? reviewOverflow : null
            },Newtonsoft.Json.Formatting.Indented));
        }

        IEnumerator OpenPage(int page,int level,int stars,int? completed = null)
        {
            Status = "Ekran açılıyor…";
            if (page is 1 or 2)
            {
                GameplayProgress.UseTestProfile(Fixture(completed ?? level-1));
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
            else if (page is 9 or 10 or 11)
            {
                if (page == 11) GameplayProgress.UseTestProfile(Fixture(8));
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
                    case 3:
                        Press("NavMap",true);
                        yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true,"Harita");
                        break;
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
            string dll = Path.GetFullPath(Evidence + "LocalBackend/ClanServer.dll");
            if (!File.Exists(dll)) dll = Path.GetFullPath("tools/ClanServer/bin/Release/net10.0/ClanServer.dll");
            if (!File.Exists(dll)) throw new InvalidOperationException(
                "Yerel API gerekli. Repo kökünde dotnet build tools/ClanServer -c Release -o Library/MewgicVisualQA/LocalBackend çalıştır.");
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
            yield return VerifyRewardFlow();
            GameplayProgress.UseTestProfile(Fixture(0));
            yield return VerifySingleMap();
            GameplayProgress.UseTestProfile(Fixture(0));
            foreach (int page in Enumerable.Range(0,10))
            {
                yield return OpenPage(page,1,3);
                checks.Add(Markers[page]);
                if (page is 2 or 7 or 9)
                {
                    yield return new WaitForSecondsRealtime(3.85f);
                    ScreenCapture.CaptureScreenshot(Evidence+$"Page-{page}.png");
                    yield return null;
                }
            }
            foreach (int stars in new[] {1,2,3})
            {
                yield return OpenPage(2,2,stars);
                yield return new WaitForSecondsRealtime(3.85f);
                if (GameplayProgress.Current.BestStars(2) != stars)
                    throw new InvalidOperationException("Yıldız örneği eşleşmedi.");
                ScreenCapture.CaptureScreenshot(Evidence+$"Stars-{stars}.png");
                checks.Add(stars+" yıldız örneği");
                yield return null;
            }
            yield return OpenPage(2,2,3);
            Press("SkipReward",true);
            if (GameObject.Find("VictorySecondaryReveal").GetComponent<CanvasGroup>().alpha < .99f)
                throw new InvalidOperationException("Animasyonu hızlandır düğmesi son duruma geçmedi.");
            checks.Add("Zafer animasyonu gerçek düğmeyle hızlandırıldı");
            var ledger = GameplayProgress.Current;
            long gold = ledger.Presentation.coins;
            int bonus = ledger.LastCoinReward*(ProgressLedger.VictoryAdMultiplier-1);
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

        IEnumerator VerifyRewardFlow()
        {
            GameplayProgress.UseTestProfile(Fixture(0));
            yield return OpenPage(0,1,3);
            var mission = GameObject.Find("MissionsButton");
            var card = (RectTransform)mission.transform.parent;
            var screenCenter = RectTransformUtility.WorldToScreenPoint(null,
                mission.transform.TransformPoint(((RectTransform)mission.transform).rect.center));
            var drag = new PointerEventData(EventSystem.current)
            {
                position=screenCenter, button=PointerEventData.InputButton.Left,
                eligibleForClick=true, pointerId=-1
            };
            ExecuteEvents.Execute(mission,drag,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(mission,drag,ExecuteEvents.beginDragHandler);
            drag.position += Vector2.right*(Screen.width*.70f);
            ExecuteEvents.Execute(mission,drag,ExecuteEvents.dragHandler);
            yield return null;
            ExecuteEvents.Execute(mission,drag,ExecuteEvents.endDragHandler);
            ExecuteEvents.Execute(mission,drag,ExecuteEvents.pointerUpHandler);
            yield return Until(() => card.anchoredPosition.x > card.rect.width*3.62f,"Görevler kartı sağa yerleşti");
            if (drag.eligibleForClick || GameObject.Find("DailyClose") != null)
                throw new InvalidOperationException("Sürükleme dokunma olarak işlendi.");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Drag.png");
            checks.Add("Görevler kartı yatay sürüklendi, tıklama tetiklenmedi");
            yield return null;
            Press("MissionsButton",true);
            yield return null;
            Press("DailyGiftTab",true);
            yield return null;
            Press("ClaimDailyGift",true);
            yield return null;
            var ledger = GameplayProgress.Current;
            long before = ledger.Presentation.coins;
            long target = before + ProgressLedger.DailyGiftCoins;
            Press("ArrivalOpen",true);
            var giftText = GameObject.Find("ArrivalReward").GetComponent<TMPro.TMP_Text>();
            if (giftText.alpha > .01f)
                throw new InvalidOperationException("Sandık ödülü giriş animasyonundan önce göründü.");
            if (ledger.Presentation.coins != target || Balance() != HomeTopBar.FormatAmount(before))
                throw new InvalidOperationException("Sandık kaydı veya animasyon başlangıç bakiyesi hatalı.");
            yield return new WaitForSecondsRealtime(.29f);
            var lid = GameObject.Find("ArrivalChestLid");
            if (lid == null || Quaternion.Angle(lid.transform.localRotation,Quaternion.identity) < 1 ||
                lid.GetComponent<CanvasRenderer>().GetMesh()?.vertexCount is not > 0)
                throw new InvalidOperationException("Sandık kapağı hareket etmedi.");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Chest-Lid.png");
            yield return Until(() => GameObject.Find("ChestRewardPiece0") != null,"Sandık parçacıkları");
            yield return new WaitForSecondsRealtime(.27f);
            if (giftText.alpha <= .25f)
                throw new InvalidOperationException("Sandık ödül metni açılmadı.");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Chest-Burst.png");
            yield return Until(() => GameObject.Find("FlyingCoin2") != null,"Sandık coin uçuşu");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Chest-Coins.png");
            yield return CheckCountUp(before,target,"Chest");
            yield return Until(() => GameObject.Find("ArrivalContinue") != null,"Sandık tamamlandı");
            yield return null;
            Press("ArrivalContinue",true);
            yield return null;
            if (ledger.Presentation.coins != target || ledger.GiftReady ||
                GameObject.Find("CoinsDisplay/Value").GetComponent<TMPro.TMP_Text>().text != HomeTopBar.FormatAmount(target))
                throw new InvalidOperationException("Günlük hediye barı veya tek ödül kaydı hatalı.");
            checks.Add("Günlük sandık kapak, saçılma, sayaç ara değerleri ve tek kayıt");

            yield return OpenPage(1,5,3);
            ledger = GameplayProgress.Current;
            before = ledger.Presentation.coins;
            yield return SolveThroughGameView();
            StartCoroutine(RecordVictoryFrames());
            var end = FindFirstObjectByType<GameplayEndScreen>();
            var panel = GameObject.Find("VictoryPanelEntrance").GetComponent<CanvasGroup>();
            var stars = Enumerable.Range(1,3).Select(i => GameObject.Find("AwardStar"+i).GetComponent<RawImage>()).ToArray();
            yield return Until(() => GameObject.Find("CelebrationSpark1") != null,"Zafer kutlaması");
            if (panel.alpha > .01f || stars[0].color.a > .01f || !end.IsShowing)
                throw new InvalidOperationException("Zafer, kutlama bitmeden paneli veya yıldızı gösterdi.");
            yield return new WaitForSecondsRealtime(.20f);
            if (GameObject.Find("VictoryFireworkBloom0")?.GetComponent<CanvasRenderer>()?.GetMesh()?.vertexCount is not > 0)
                throw new InvalidOperationException("Zafer patlama ışığı oluşmadı.");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Victory-Burst.png");
            var frame=GameObject.Find("GoldFrame").GetComponent<CanvasRenderer>();
            yield return Until(() => frame.GetMesh()?.vertexCount is > 0,"Altın çerçeve oluşuyor");
            if (panel.alpha>.03f) throw new InvalidOperationException("Mor panel çerçeveden önce açıldı.");
            ScreenCapture.CaptureScreenshot(Evidence+"ApprovedVictory-Frame.png");
            var title=GameObject.Find("VictoryTitleReveal").GetComponent<CanvasGroup>();
            yield return Until(() => title.alpha>.2f,"Başlık açıldı");
            if (stars[0].color.a>.05f) throw new InvalidOperationException("Yıldız başlıktan önce açıldı.");
            ScreenCapture.CaptureScreenshot(Evidence+"ApprovedVictory-Title.png");
            for (int i=0; i<stars.Length; i++)
            {
                int index = i;
                yield return Until(() => stars[index].color.a > .16f,"Zafer yıldızı "+(index+1));
                if (i+1 < stars.Length && stars[i+1].color.a > .01f)
                    throw new InvalidOperationException("Yıldızlar sırayla gelmedi.");
                if (i==0) ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Victory-FirstStar.png");
            }
            yield return Until(() => panel.alpha >= .99f,"Sonuç paneli açıldı");
            var adStage=GameObject.Find("VictoryAdReveal").GetComponent<CanvasGroup>();
            var goldStage=GameObject.Find("VictoryGoldReveal").GetComponent<CanvasGroup>();
            var nextStage=GameObject.Find("VictoryNextReveal").GetComponent<CanvasGroup>();
            yield return Until(() => goldStage.alpha>.2f,"Altın ödülü açıldı");
            if (adStage.alpha>.05f) throw new InvalidOperationException("Reklam altın ödülünden önce açıldı.");
            yield return Until(() => adStage.alpha>.2f,"Reklam paneli açıldı");
            if (nextStage.alpha>.05f) throw new InvalidOperationException("Sonraki Bölüm reklamdan önce açıldı.");
            target = ledger.Presentation.coins;
            if (target <= before || ledger.BestStars(5) != 3)
                throw new InvalidOperationException("Gerçek bölüm ödülü kaydedilmedi.");
            yield return Until(() => GameObject.Find("FlyingCoin2") != null,"Zafer coin uçuşu");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Victory-Coins.png");
            yield return CheckCountUp(before,target,"Victory");
            yield return new WaitForSecondsRealtime(.23f);
            ScreenCapture.CaptureScreenshot(Evidence+"ApprovedVictory-Settled.png");
            yield return null;
            checks.Add("Gerçek bölüm 5, kutlama → çerçeve → panel ve başlık → üç yıldız → ödül ve CTA");
            foreach (int height in new[] {1950,2000})
            {
                Size(height);
                yield return Until(() => Screen.width==900 && Screen.height==height,"Sonuç ekranı "+height);
                yield return null;
                Canvas.ForceUpdateCanvases();
                foreach (string name in new[] {"RewardCoinBalance","Success","NextLabel","MapLabel","WatchTotal"})
                {
                    var label=GameObject.Find(name).GetComponent<TMPro.TMP_Text>();
                    label.ForceMeshUpdate();
                    if (label.isTextOverflowing) throw new InvalidOperationException("Sonuç metni taştı: "+name);
                }
                checks.Add("Sonuç paneli ve coin bar 900 × "+height);
            }
            Size(1600);
            yield return Until(() => Screen.height==1600,"Sonuç ekranı 900 × 1600");
            yield return null;
            Press("ResultHome",true);
            yield return null;
            if (SceneManager.GetActiveScene().name!="Gameplay" || GameObject.Find("MagicVeil")==null)
                throw new InvalidOperationException("Haritaya geçiş perdeyi beklemedi.");
            yield return new WaitForSecondsRealtime(.14f);
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Map-Fade.png");
            yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true,"Sonuçtan LevelMap");
            var map = FindFirstObjectByType<MapWorld>();
            yield return Until(() => !map.JourneyRunning && GameObject.Find("MagicVeil")==null,"Harita geçişi ve maskot");
            if (map.SelectedChapter != 0 || map.Progression.CurrentId != 6 ||
                GameObject.Find("ContinuousRoadContent")==null)
                throw new InvalidOperationException("Sonuçtan doğru Büyülü Köy terası açılmadı.");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Map-Village.png");
            checks.Add("Sonuçtan fade ile LevelMap Büyülü Köy 1–10 ve güncel bölüm 6");
            yield return null;
            yield return OpenPage(2,91,2,100);
            yield return new WaitForSecondsRealtime(3.85f);
            if(GameObject.Find("WatchRewardedAd").GetComponent<Button>().interactable ||
                GameObject.Find("Reward").GetComponent<TMPro.TMP_Text>().text != L.Resolve(L.V("victory.no_gold")))
                throw new InvalidOperationException("Tekrar oynama sıfır altın veya reklam vaadi gösterdi.");
            Press("ResultHome",true);
            yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true,"Eski bölümden LevelMap");
            map = FindFirstObjectByType<MapWorld>();
            yield return Until(() => GameObject.Find("MagicVeil")==null,"Kristal harita geçişi");
            if (map.SelectedChapter != 9 || map.Progression.CurrentId != 101 ||
                GameObject.Find("ContinuousRoadContent")==null)
                throw new InvalidOperationException("91 tekrarı Kristal 91–100 yerine mevcut ilerleme adasına döndü.");
            ScreenCapture.CaptureScreenshot(Evidence+"RewardFlow-Map-CrystalReplay.png");
            yield return null;
            checks.Add("91 tekrarından Kristal 91–100 açıldı, mevcut ilerleme 101 kaldı");
        }

        static string Balance() => GameObject.Find("RewardCoinBalance")?.GetComponent<TMPro.TMP_Text>().text;

        static IEnumerator CheckCountUp(long before,long after,string tag)
        {
            string first = HomeTopBar.FormatAmount(before), last = HomeTopBar.FormatAmount(after);
            yield return Until(() => Balance() != null && Balance() != first && Balance() != last,"Coin bar ara bakiye");
            ScreenCapture.CaptureScreenshot(Evidence+$"RewardFlow-{tag}-CountUp.png");
            yield return Until(() => Balance() == last,"Coin bar son bakiye");
        }

        static IEnumerator RecordVictoryFrames()
        {
            string directory=Evidence+"ApprovedVictoryFrames/";
            Directory.CreateDirectory(directory);
            var pixels=new Texture2D(360,640,TextureFormat.RGB24,false);
            var small=RenderTexture.GetTemporary(360,640,0);
            var full=RenderTexture.GetTemporary(Screen.width,Screen.height,0);
            try
            {
                for(int frame=0;frame<30;frame++)
                {
                    yield return new WaitForEndOfFrame();
                    ScreenCapture.CaptureScreenshotIntoRenderTexture(full);
                    Graphics.Blit(full,small,new Vector2(1,-1),new Vector2(0,1));
                    var previous=RenderTexture.active;
                    RenderTexture.active=small;
                    pixels.ReadPixels(new Rect(0,0,360,640),0,0);
                    RenderTexture.active=previous;
                    File.WriteAllBytes(directory+frame.ToString("D2")+".jpg",pixels.EncodeToJPG(87));
                    yield return new WaitForSecondsRealtime(.145f);
                }
            }
            finally
            {
                RenderTexture.ReleaseTemporary(full);
                RenderTexture.ReleaseTemporary(small);
                Destroy(pixels);
            }
        }

        static IEnumerator SolveThroughGameView()
        {
            var app = FindFirstObjectByType<PrototypeApp>();
            var puzzle = (Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(app);
            var solution = PuzzleSolver.Solve(puzzle.Export());
            if (solution.Status != SearchStatus.Solved) throw new InvalidOperationException("Test bulmacası çözülemedi.");
            for (int i=0; i<solution.Moves.Count; i++)
            {
                var move = solution.Moves[i];
                Press("Bottle "+(move.Source+1),true);
                yield return null;
                Press("Bottle "+(move.Target+1),true);
                if (i < solution.Moves.Count-1) yield return new WaitForSecondsRealtime(1.1f);
            }
            yield return Until(() => FindFirstObjectByType<GameplayEndScreen>().IsShowing,"Gerçek kazanma");
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
                throw new InvalidOperationException("Düğme raycast hedefi değil: "+name+" / "+(hits.Count == 0 ? "—" : hits[0].gameObject.name));
            pointer.eligibleForClick = true;
            pointer.pointerCurrentRaycast = hits[0];
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
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
