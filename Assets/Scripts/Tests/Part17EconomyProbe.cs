#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Daily;
using Mewgic.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class Part17EconomyProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/Part17";
        readonly List<string> checks = new List<string>(), errors = new List<string>();
        readonly Dictionary<string,string> original = new Dictionary<string,string>();
        readonly string key = "Mewgic.Tests.Part17." + Guid.NewGuid().ToString("N");
        static readonly string[] Suffixes = {"", ".backup", ".corrupt"};
        DateTimeOffset now = new DateTimeOffset(2026,11,4,9,0,0,TimeSpan.Zero);
        ProgressLedger Ledger => GameplayProgress.Current;
        static FieldInfo Current => typeof(GameplayProgress).GetField("current",BindingFlags.Static|BindingFlags.NonPublic);

        public static void Begin()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("PART17 isolated QA").AddComponent<Part17EconomyProbe>();
            EditorApplication.isPlaying=true;
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            foreach(var suffix in Suffixes)
            {
                var name=GameplayProgress.StorageKey+suffix;
                original[name]=PlayerPrefs.HasKey(name)?PlayerPrefs.GetString(name):null;
            }
            var data=new ProgressData();
            data.presentation.manualLocale="tr";
            data.completedTutorials.Add("paw.basic.v1");
            PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();
            GameplayProgress.UseTestProfile();
            Current.SetValue(null,GameplayProgress.Open(key,()=>now));
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived+=Log;
        }

        void Log(string message,string stack,LogType type)
        {
            if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert||type==LogType.Warning)
                errors.Add(message);
        }

        void Check(bool valid,string label)
        {
            if(!valid)throw new InvalidOperationException(label);
            checks.Add(label);
        }

        IEnumerator Start()
        {
            var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            view.Show();view.maximized=true;view.Focus();
            yield return null;
            Size(1600);
            yield return new WaitForSecondsRealtime(.7f);
            var stack=new Stack<IEnumerator>();
            stack.Push(Run());
            string failure=null;
            while(stack.Count>0)
            {
                bool more=false;object next=null;
                try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}
                catch(Exception e){failure=e.ToString();break;}
                if(!more){stack.Pop();continue;}
                if(next is IEnumerator iterator&&!(next is CustomYieldInstruction)){stack.Push(iterator);continue;}
                yield return next;
            }
            bool intact=original.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.GetString(p.Key)==p.Value);
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"UNITY_TESTS.json"),Newtonsoft.Json.JsonConvert.SerializeObject(
                new{success=failure==null&&errors.Count==0&&intact,checks,errors,failure,playerSaveIntact=intact},
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying=false;
        }

        void ModelChecks()
        {
            var tick=now;
            var ledger=new ProgressLedger(new ProgressData(),_=>{},()=>tick);
            for(int stars=1;stars<=3;stars++)
            {
                Check(ledger.BeginFirstAttempt(stars),"İlk deneme işaretlendi "+stars);
                Check(ledger.Claim(stars,stars,true,out var first,"initial-"+stars,4,true,false)&&first&&ledger.LastCoinReward==35+stars*5,"İlk galibiyet tablosu "+stars);
            }
            Check(ledger.Claim(1,3,true,out _,"initial-1",4)&&ledger.LastCoinReward==0&&ledger.BestStars(1)==1,"Aynı galibiyet kimliği yeniden işlenmedi");
            Check(ledger.Claim(1,1,true,out _,"same-stars",4)&&ledger.LastCoinReward==0,"Tekrar galibiyette coin yok");
            Check(ledger.Claim(1,2,true,out _,"second-star",4)&&ledger.LastCoinReward==5,"1 → 2 yıldız farkı +5");
            Check(ledger.Claim(1,3,true,out _,"third-star",4)&&ledger.LastCoinReward==5,"2 → 3 yıldız farkı +5");
            Check(ledger.Claim(1,2,true,out _,"lower-stars",4)&&ledger.LastCoinReward==0&&ledger.BestStars(1)==3,"Düşük yıldız en iyi sonucu düşürmedi");
            Check(ledger.BeginFirstAttempt(4),"Kapatmadan önce deneme yazıldı");
            var reopened=new ProgressLedger(ledger.Snapshot,_=>{},()=>tick);
            Check(!reopened.BeginFirstAttempt(4)&&reopened.Claim(4,3,true,out _,"after-restart",4,false,true)&&reopened.LastCoinReward==35,"Yeniden açılışta ilk deneme ve ipucusuz bonusu verilmedi");
            bool fail=true;
            var failing=new ProgressLedger(new ProgressData(),_=>{if(fail)throw new IOException("QA");},()=>tick);
            Check(!failing.Claim(1,3,true,out _,"win",4)&&failing.LastCoinReward==0&&failing.Presentation.coins==0&&!failing.Completed(1),"Hatalı galibiyet kaydı ödül ve yıldızı değiştirmedi");
            Check(!failing.ClaimDailyGift()&&failing.GiftReady&&failing.Presentation.coins==0,"Hatalı sandık kaydı hakkı tüketmedi");
            fail=false;
            Check(failing.Claim(1,3,true,out _,"win",4)&&failing.LastCoinReward==35,"Başarısız galibiyet aynı kimlikle güvenle yeniden kaydedildi");
            Check(failing.ClaimDailyGift(0)&&!failing.ClaimDailyGift(0),"Sandık peş peşe açılamıyor");
            tick=tick.AddHours(23).AddMinutes(59);
            Check(!failing.GiftReady&&!failing.ClaimDailyGift(),"24 saatten önce yeni sandık yok");
            tick=tick.AddMinutes(1);
            Check(failing.GiftReady&&!failing.ClaimDailyGift(0)&&failing.ClaimDailyGift(failing.LastGiftUtc),"24 saat sınırı ve eski popup kimliği");
            tick=tick.AddDays(-1);
            Check(failing.GiftClockBlocked&&!failing.ClaimDailyGift(),"Saat geri alındığında sandık kapalı");
            tick=now;
            TimeSpan elapsed=TimeSpan.Zero;
            var guarded=new ProgressLedger(new ProgressData(),_=>{},()=>tick,()=>elapsed);
            tick=tick.AddDays(2);
            Check(guarded.GiftClockBlocked&&!guarded.ClaimDailyGift(),"Aynı oturumda saat ileri alma engellendi");
            elapsed=TimeSpan.FromDays(2);
            Check(guarded.ClaimDailyGift(),"Gerçek geçen süre ile çevrimdışı sandık açık");
            var buyer=new ProgressLedger(failing.Snapshot,_=>throw new IOException("QA"),()=>tick);
            int hint=buyer.Remaining(BoosterKind.Hint);
            long balance=buyer.Presentation.coins;
            Check(!buyer.BuyBooster(BoosterKind.Hint)&&buyer.Remaining(BoosterKind.Hint)==hint&&buyer.Presentation.coins==balance,"Satın alma kaydı başarısızsa coin ve stok korunuyor");

            string legacy=key+".legacy";
            try
            {
                PlayerPrefs.SetString(legacy,"{\"version\":1,\"undo\":3,\"hint\":7,\"extra\":1,\"shuffle\":1,\"levels\":[{\"level\":1,\"bestStars\":2,\"rewardClaimed\":true}],\"presentation\":{\"version\":1,\"coins\":123}}");
                var old=GameplayProgress.Open(legacy,()=>now);
                Check(old.Completed(1)&&old.Presentation.coins==123&&old.Remaining(BoosterKind.Hint)==7,"Eski JSON yıldız, bakiye ve stoku korudu");
                Check(old.Claim(1,3,true,out _,"legacy-upgrade",4)&&old.LastCoinReward==5&&old.Presentation.coins==128,"Eski JSON tekrar taban ödülü almadı");
            }
            finally{foreach(var suffix in Suffixes)PlayerPrefs.DeleteKey(legacy+suffix);}
        }

        IEnumerator Run()
        {
            ModelChecks();
            yield return Until(()=>GameObject.Find("ArrivalOpen")!=null);
            yield return new WaitForSecondsRealtime(.7f);
            yield return Shot("Gunluk_Kapali",1600);
            Tap("ArrivalOpen");
            Check(GameplayProgress.Open(key,()=>now).Presentation.coins==100&&GameplayProgress.Open(key,()=>now).Remaining(BoosterKind.Hint)==4,"Sandık animasyonu sürerken diskten coin ve ipucu yeniden yüklendi");
            Check(!Ledger.ClaimDailyGift(),"Animasyon sırasında ikinci sandık ödülü yok");
            yield return Until(()=>GameObject.Find("ArrivalContinue")!=null);
            yield return Shot("Gunluk_Acik",1600);
            Tap("ArrivalContinue");
            yield return null;
            Tap("CoinsPlus");yield return null;
            Tap("BuyHint");yield return null;
            Tap("BuyExtraBottle");yield return null;
            Tap("BuyHint");yield return null;
            Check(Ledger.Presentation.coins==0&&Ledger.Remaining(BoosterKind.Hint)==5&&Ledger.Remaining(BoosterKind.ExtraBottle)==2,"Mağaza satın alma ve yetersiz bakiye, negatif coin yok");
            yield return new WaitForSecondsRealtime(1.1f);
            yield return Shot("Yardimci_Gucler",1600);
            Tap("ShopClose");yield return null;
            Tap("Play");
            yield return Until(()=>GameObject.Find("Bottle 1")!=null);
            yield return new WaitForSecondsRealtime(.75f);
            yield return Solve();
            Check(Ledger.LastCoinReward==50&&Ledger.BestStars(1)==3&&Ledger.Presentation.coins==50,"Gerçek ilk kazanış: 3 yıldız, +50 coin");
            Tap("SkipReward");
            Check(GameObject.Find("SkipReward")==null,"Ödül animasyonu atlandı");
            foreach(int height in new[]{1600,1950,2000})
            {
                Size(height);yield return new WaitForSecondsRealtime(.6f);
                var next=FindButton("NextLevel").transform as RectTransform;
                var map=FindButton("ResultHome").transform as RectTransform;
                Check(!Bounds(next).Overlaps(Bounds(map))&&Hit("NextLevel")&&Hit("ResultHome"),"Continue ve Map ayrı ve seçilebilir "+height);
                yield return Shot("Bolum_Odulu_"+height,height);
            }
            Tap("Replay");yield return null;
            yield return Solve();
            Check(Ledger.Presentation.coins==50&&Ledger.LastCoinReward==0,"Gerçek tekrar kazanış bakiye artırmadı");
            Tap("ResultHome");
            yield return Until(()=>SceneManager.GetActiveScene().name=="LevelMap"&&FindFirstObjectByType<MapWorld>()?.Mounted==true);
            var loaded=GameplayProgress.Open(key,()=>now);
            Check(loaded.Presentation.coins==50&&loaded.BestStars(1)==3&&!loaded.GiftReady,"Bölüm, satın alma ve sandık yeni yüklemede birlikte korundu");
        }

        IEnumerator Solve()
        {
            var app=FindFirstObjectByType<PrototypeApp>();
            var puzzle=(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(app);
            foreach(var move in PuzzleSolver.Solve(puzzle.Export()).Moves)
            {
                Tap("Bottle "+(move.Source+1));Tap("Bottle "+(move.Target+1));
                yield return new WaitForSecondsRealtime(.95f);
            }
            Check(puzzle.Won&&FindFirstObjectByType<GameplayEndScreen>().IsShowing,"Gerçek döküşlerle sonuç ekranı");
        }

        static Button FindButton(string name)=>GameObject.Find(name).GetComponent<Button>();
        static Rect Bounds(RectTransform r)
        {
            var corners=new Vector3[4];r.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
        }

        static bool Hit(string name)=>HitTarget(FindButton(name))?.transform.IsChildOf(FindButton(name).transform)==true;

        static GameObject HitTarget(Button button)
        {
            Canvas.ForceUpdateCanvases();
            var rect=(RectTransform)button.transform;
            var hit=new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))},hit);
            return hit.Count>0?hit[0].gameObject:null;
        }

        void Tap(string name)
        {
            var button=FindButton(name);
            Check(button.interactable&&Hit(name),"Etkileşim "+name+" / "+HitTarget(button)?.name+(button.interactable?"":" / devre dışı"));
            button.onClick.Invoke();
        }

        IEnumerator Until(Func<bool> condition)
        {
            float deadline=Time.realtimeSinceStartup+15;
            while(!condition()){if(Time.realtimeSinceStartup>deadline)throw new TimeoutException(SceneManager.GetActiveScene().name);yield return null;}
        }

        static void Size(int height)=>Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null,new object[]{height});

        IEnumerator Shot(string file,int height)
        {
            yield return new WaitForEndOfFrame();
            Check(Screen.width==900&&Screen.height==height,"Game View "+file);
            Directory.CreateDirectory(Output);
            var raw=RenderTexture.GetTemporary(900,height,0);
            var flipped=RenderTexture.GetTemporary(900,height,0);
            var image=new Texture2D(900,height,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;
            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(raw);
                Graphics.Blit(raw,flipped,new Vector2(1,-1),new Vector2(0,1));
                RenderTexture.active=flipped;
                image.ReadPixels(new Rect(0,0,900,height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Output,file+".png"),image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previous;RenderTexture.ReleaseTemporary(raw);RenderTexture.ReleaseTemporary(flipped);Destroy(image);
            }
        }

        void OnDestroy()
        {
            Application.logMessageReceived-=Log;
            GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            foreach(var suffix in Suffixes)PlayerPrefs.DeleteKey(key+suffix);
            PlayerPrefs.Save();
        }
    }
}
#endif
