#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
using Mewgic.Rewards;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class Part18ReleaseProbe : MonoBehaviour
    {
        static Part18ReleaseProbe active;
        const string Output="docs/ProductionReports/Part18";
        readonly string key="Mewgic.Tests.Part18."+Guid.NewGuid().ToString("N");
        readonly string[] suffixes={"",".backup",".corrupt"};
        readonly Dictionary<string,string> original=new Dictionary<string,string>();
        readonly List<string> checks=new List<string>(), errors=new List<string>(), warnings=new List<string>();
        readonly HashSet<string> overflows=new HashSet<string>();
        readonly DateTimeOffset now=new DateTimeOffset(2026,11,4,9,0,0,TimeSpan.Zero);
        AudioSource Source(string name)=>GameObject.Find("Mewgic Game Feel/"+name).GetComponent<AudioSource>();

        public static void Begin()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("PART18 QA").AddComponent<Part18ReleaseProbe>();
            EditorApplication.isPlaying=true;
        }

        void Awake()
        {
            if(active!=null){Destroy(gameObject);return;}
            active=this;
            DontDestroyOnLoad(gameObject);
            foreach(string suffix in suffixes)
            {
                string id=GameplayProgress.StorageKey+suffix;
                original[id]=PlayerPrefs.HasKey(id)?PlayerPrefs.GetString(id):null;
            }
            var data=new ProgressData();
            data.presentation.manualLocale="en";
            data.completedTutorials.Add("paw.basic.v1");
            PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();
            var profile=GameplayProgress.Open(key,()=>now);
            profile.ClaimDailyGift();
            GameplayProgress.UseTestProfile();
            typeof(GameplayProgress).GetField("current",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,profile);
            Application.logMessageReceived+=Log;
        }

        void Log(string message,string stack,LogType type)
        {
            if(type==LogType.Warning)warnings.Add(message);
            if(type==LogType.Error||type==LogType.Assert||type==LogType.Exception)errors.Add(message);
        }

        void Check(bool pass,string label)
        {
            if(!pass)throw new InvalidOperationException(label);
            checks.Add(label);
        }

        IEnumerator Start()
        {
            var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            view.Show();view.maximized=true;view.Focus();
            yield return null;
            Size(1600);
            yield return new WaitForSecondsRealtime(.65f);
            var stack=new Stack<IEnumerator>();stack.Push(Run());
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
                new{success=failure==null&&errors.Count==0&&intact&&L.Missing.Count==0,
                    checks,errors,warnings,failure,playerSaveIntact=intact,overflowCandidates=overflows.OrderBy(s=>s),missing=L.Missing},
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying=false;
        }

        IEnumerator Run()
        {
            yield return Until(()=>L.Ready&&GameObject.Find("SettingsButton")!=null,"Home");
            foreach(var name in new[]{"HomeMusic","PuzzleMusic","Waterfall","Pour","Valid","Invalid","Victory","Coin","Chest","Map"})
                Check(Resources.Load<AudioClip>("Audio/Part18/"+name)?.length>0,"Ses dosyası "+name);
            Check(Source("Music").isPlaying&&Source("Music").clip.name=="HomeMusic"&&Source("Waterfall").isPlaying,"Ana menü müziği ve şelale kanalları");
            Check(!new PlayerPresentationData{soundEnabled=false}.MusicEnabled&&!new PlayerPresentationData{soundEnabled=false}.EffectsEnabled,"Eski sessiz tercih iki ses kanalını da kapatır");
            Check(L.Choose(null,new[]{"fr-CA"})=="fr"&&L.Choose(null,new[]{"ja-JP"})=="en","Cihaz dili ve İngilizce geri dönüş");
            Click("SettingsButton");
            Click("settings.music");
            Check(!Source("Music").isPlaying&&Source("Waterfall").isPlaying,"Müzik kapalı, efekt açık");
            Click("settings.effects");
            Click("settings.vibration");
            Click("settings.motion");
            var saved=GameplayProgress.Open(key,()=>now).Presentation;
            Check(!saved.MusicEnabled&&!saved.EffectsEnabled&&saved.vibrationEnabled&&saved.reduceMotion&&!Source("Waterfall").isPlaying,"Dört ayrı tercih diskten geri okundu");
            GameFeel.Play(GameCue.Coin);
            Check(!Source("Effects").isPlaying,"Efekt kapalıyken one-shot susturuldu");
            Click("settings.music");Click("settings.effects");Click("settings.vibration");Click("settings.motion");
            Click("LanguageButton");Click("Locale_de");
            Check(L.Code=="de"&&GameplayProgress.Open(key,()=>now).Presentation.manualLocale=="de","Ayarlar üzerinden seçilen dil diskte");
            Click("ResourceInfoClose");

            int index=0;
            foreach(string code in L.Codes)
            {
                int height=new[]{1600,1950,2000}[index++%3];
                Size(height);
                yield return Until(()=>Screen.width==900&&Screen.height==height,"Game View yeniden boyutlama "+Screen.width+"x"+Screen.height+" → "+height);
                Check(L.Select(code,false),"Dil "+code);
                yield return new WaitForSecondsRealtime(.48f);
                yield return Shot(code,"Home",height);
                Click("SettingsButton");yield return null;
                foreach(var button in new[]{"settings.music","settings.effects","settings.vibration","settings.motion","LanguageButton","TutorialReplay","ResourceInfoClose"})
                {
                    var rect=(RectTransform)GameObject.Find(button).transform;
                    var corners=new Vector3[4];rect.GetWorldCorners(corners);
                    Check(corners[2].y-corners[0].y>=44,"Ayar dokunma alanı "+code+"/"+button);
                }
                yield return Shot(code,"Settings",height);
                Click("ResourceInfoClose");
                var rewards=FindFirstObjectByType<RewardHubPanel>();rewards.Open();
                yield return new WaitForSecondsRealtime(.12f);
                yield return Shot(code,"Rewards",height);
                rewards.Close();
                SceneManager.LoadScene("LevelMap");
                yield return Until(()=>FindFirstObjectByType<MapWorld>()?.Mounted==true,"LevelMap");
                yield return new WaitForSecondsRealtime(.25f);
                yield return Shot(code,"Map",height);
                if(code=="en")
                {
                    FindFirstObjectByType<MapWorld>().OpenChapter(0,0);
                    yield return new WaitForSecondsRealtime(.25f);
                    yield return Shot(code,"Island",height);
                }
                GameplayLevelCatalog.ResetSelection();
                SceneManager.LoadScene("Gameplay");
                yield return Until(()=>GameObject.Find("Bottle 1")!=null,"Gameplay");
                yield return new WaitForSecondsRealtime(.30f);
                yield return Shot(code,"Gameplay",height);
                if(code=="en")
                {
                    Check(Source("Music").isPlaying&&Source("Music").clip.name=="PuzzleMusic","Bulmaca müziği geçişi");
                    var ledger=GameplayProgress.Current;
                    var app=FindFirstObjectByType<PrototypeApp>();
                    var puzzle=(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(app);
                    int empty=Enumerable.Range(0,puzzle.Count).First(i=>puzzle.Bottle(i).Count==0);
                    Click("Bottle "+(empty+1));
                    Check(puzzle.Moves==0&&Source("Effects").isPlaying,"Gerçek geçersiz hamle sesi");
                    foreach(var step in PuzzleSolver.Solve(puzzle.Export()).Moves)
                    {
                        Click("Bottle "+(step.Source+1));Click("Bottle "+(step.Target+1));
                        yield return Until(()=>GameObject.Find("Pour stream")!=null,"Sıvı akışı");
                        Check(Source("Effects").isPlaying,"Gerçek döküşte efekt ve akış eşzamanlı");
                        yield return Until(()=>app.GuideCanInteract||FindFirstObjectByType<GameplayEndScreen>().IsShowing,"Döküş tamamlandı");
                    }
                    Check(puzzle.Won&&ledger.LastCoinReward>0&&Source("Effects").isPlaying,"Kazanış ve kaydedilen coin sesi");
                    yield return new WaitForSecondsRealtime(1.3f);
                    yield return Shot(code,"Victory",height);
                    Click("Replay");
                    ledger.SetReducedMotion(true);
                    Check(GameFeel.Reduced,"Hareket azaltma oynanışta etkin");
                    var move=PuzzleSolver.Solve(puzzle.Export()).Moves[0];
                    Click("Bottle "+(move.Source+1));Click("Bottle "+(move.Target+1));
                    yield return new WaitForSecondsRealtime(.22f);
                    Check(puzzle.Moves==1&&GameObject.Find("Pour stream")==null&&app.GuideCanInteract,"Az hareketle geçerli döküş ve giriş kilidinin açılması");
                    ledger.SetReducedMotion(false);
                }
                SceneManager.LoadScene("Home");
                yield return Until(()=>GameObject.Find("SettingsButton")!=null,"Home dönüşü");
                yield return new WaitForSecondsRealtime(.25f);
            }
            Check(L.Missing.Count==0,"Görüntülenen ekranlarda eksik anahtar yok");
        }

        static void Click(string name)=>GameObject.Find(name).GetComponent<Button>().onClick.Invoke();

        static IEnumerator Until(Func<bool> condition,string scene)
        {
            float deadline=Time.realtimeSinceStartup+15;
            while(!condition())
            {
                if(Time.realtimeSinceStartup>deadline)throw new TimeoutException(scene);
                yield return null;
            }
        }

        static void Size(int height)=>Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null,new object[]{height});

        IEnumerator Shot(string code,string screen,int height)
        {
            Canvas.ForceUpdateCanvases();
            yield return new WaitForEndOfFrame();
            Check(Screen.width==900&&Screen.height==height&&L.Code==code,"Game View "+code+"/"+screen+"/"+height+" / gerçek "+Screen.width+"x"+Screen.height+" "+L.Code);
            foreach(var text in FindObjectsByType<LocalizedTMP>(FindObjectsSortMode.None))
            {
                if(string.IsNullOrWhiteSpace(text.Request)||!text.Request.StartsWith("\u001e")||
                    text.canvasRenderer.cull||string.IsNullOrWhiteSpace(text.text))continue;
                var p=RectTransformUtility.WorldToScreenPoint(null,text.rectTransform.TransformPoint(text.rectTransform.rect.center));
                if(p.x<0||p.x>900||p.y<0||p.y>height)continue;
                if(text.isTextOverflowing||text.isTextTruncated)
                    overflows.Add(code+"/"+screen+"/"+text.gameObject.name+"/"+text.Request.Split('\u001f')[0].TrimStart('\u001e'));
            }
            string folder=Path.Combine(Output,code);
            Directory.CreateDirectory(folder);
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
                File.WriteAllBytes(Path.Combine(folder,screen+".jpg"),image.EncodeToJPG(90));
            }
            finally
            {
                RenderTexture.active=previous;RenderTexture.ReleaseTemporary(raw);RenderTexture.ReleaseTemporary(flipped);Destroy(image);
            }
        }

        void OnDestroy()
        {
            if(active!=this)return;
            active=null;
            Application.logMessageReceived-=Log;
            GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            foreach(string suffix in suffixes)PlayerPrefs.DeleteKey(key+suffix);
            PlayerPrefs.Save();
        }
    }
}
#endif
