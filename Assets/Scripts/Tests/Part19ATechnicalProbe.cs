#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.Map;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class Part19ATechnicalProbe : MonoBehaviour
    {
        const string Output="docs/ProductionReports/Part19A";
        static Part19ATechnicalProbe active;
        readonly List<object> samples=new List<object>();
        readonly List<string> errors=new List<string>(), warnings=new List<string>(), checks=new List<string>();
        readonly Dictionary<string,string> player=new Dictionary<string,string>();
        readonly string[] suffixes={"",".backup",".corrupt",".previous",".pending"};
        readonly DateTimeOffset now=new DateTimeOffset(2026,11,4,10,0,0,TimeSpan.Zero);
        string run;

        public static void Begin(string label)
        {
            SessionState.SetString("Part19A.Run",label);
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("PART19A QA").AddComponent<Part19ATechnicalProbe>();
            EditorApplication.isPlaying=true;
        }

        void Awake()
        {
            if(active!=null){Destroy(gameObject);return;}
            active=this;DontDestroyOnLoad(gameObject);
            foreach(string suffix in suffixes)
            {
                string k=GameplayProgress.StorageKey+suffix;
                player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;
            }
            run=SessionState.GetString("Part19A.Run","AFTER");
            var data=new ProgressData();
            data.presentation.manualLocale="tr";
            data.completedTutorials.Add("paw.basic.v1");
            for(int i=1;i<=92;i++)data.levels.Add(new LevelRecord{level=i,bestStars=3,rewardClaimed=true});
            GameplayProgress.UseTestProfile(data,()=>now);
            GameplayProgress.Current.ClaimDailyGift();
            Application.logMessageReceived+=Log;
        }

        void Log(string message,string stack,LogType type)
        {
            if(type==LogType.Warning)warnings.Add(message);
            if(type==LogType.Error||type==LogType.Assert||type==LogType.Exception)errors.Add(message+"\n"+stack);
        }

        void Check(bool pass,string name){if(!pass)throw new InvalidOperationException(name);checks.Add(name);}

        IEnumerator Start()
        {
            var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            view.Show();view.maximized=true;view.Focus();
            yield return null;
            Size(1600);
            yield return new WaitForSecondsRealtime(.75f);
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
            bool intact=player.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.GetString(p.Key)==p.Value);
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,run+".json"),Newtonsoft.Json.JsonConvert.SerializeObject(
                new{success=failure==null&&intact&&errors.Count==0,unity=Application.unityVersion,
                    graphics=SystemInfo.graphicsDeviceName, api=SystemInfo.graphicsDeviceType.ToString(),
                    processor=SystemInfo.processorType, playerSaveIntact=intact,checks,warnings,errors,failure,samples},
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying=false;
        }

        IEnumerator Run()
        {
            yield return Until(()=>L.Ready&&GameObject.Find("Play")!=null,"Home");
            yield return Measure("Home",120);
            SceneManager.LoadScene("LevelMap");
            yield return Until(()=>FindFirstObjectByType<MapWorld>()?.Mounted==true,"Map");
            yield return Measure("World map",120);
            FindFirstObjectByType<MapWorld>().OpenChapter(1,4);
            yield return Measure("Crystal Falls",120);
            if(run!="BEFORE")yield return Shot("Kristal");
            GameplayLevelCatalog.ResetSelection();
            SceneManager.LoadScene("Gameplay");
            yield return Until(()=>GameObject.Find("Bottle 1")!=null,"Gameplay");
            yield return Measure("Gameplay idle",120);
            var app=FindFirstObjectByType<PrototypeApp>();
            var field=typeof(PrototypeApp).GetField("puzzle",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var board=(Puzzle)field.GetValue(app);
            var move=PuzzleSolver.Solve(board.Export()).Moves[0];
            Click("Bottle "+(move.Source+1));Click("Bottle "+(move.Target+1));
            yield return Measure("Pour",32,false);
            if(run=="BEFORE")yield break;
            yield return Until(()=>GameObject.Find("Pour stream")!=null,"İlk döküş akışı");
            int streamId=GameObject.Find("Pour stream").GetInstanceID();
            yield return Until(()=>app.GuideCanInteract,"İlk döküş bitti");
            var second=PuzzleSolver.Solve(board.Export()).Moves[0];
            Click("Bottle "+(second.Source+1));Click("Bottle "+(second.Target+1));
            yield return Until(()=>GameObject.Find("Pour stream")!=null,"İkinci döküş akışı");
            Check(GameObject.Find("Pour stream").GetInstanceID()==streamId,"İkinci döküş aynı akış nesnesini kullandı");
            var pause=typeof(PrototypeApp).GetMethod("OnApplicationPause",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            int count=board.Moves;
            pause.Invoke(app,new object[]{true});
            pause.Invoke(app,new object[]{false});
            app.ResumeGame();
            Check(board.Moves==count&&app.GuideCanInteract&&GameObject.Find("Pour stream")==null,"Arka plan ve devam döküşü iki kez uygulamadı");
            foreach(int height in new[]{1600,1950,2000})
            {
                Size(height);yield return Until(()=>Screen.width==900&&Screen.height==height,"Ekran oranı "+height);
                var bottle=(RectTransform)GameObject.Find("Bottle 1").transform;
                var corners=new Vector3[4];bottle.GetWorldCorners(corners);
                Check(corners[2].x-corners[0].x>=44&&corners[2].y-corners[0].y>=44,"Şişe dokunma alanı "+height);
            }
            yield return Shot("Oynanis");
            Size(1600);
            for(int i=0;i<8;i++)
            {
                SceneManager.LoadScene("Home");yield return Until(()=>GameObject.Find("Play")!=null,"Döngü Home");
                SceneManager.LoadScene("LevelMap");yield return Until(()=>FindFirstObjectByType<MapWorld>()?.Mounted==true,"Döngü Map");
                if(i==0)
                {
                    var map=FindFirstObjectByType<MapWorld>();
                    int region=WorldMapRules.Island(1000000);
                    map.FocusRegion(region,false);
                    Check(map.ActiveRegion==region,"1.000.000 düzeyi dünya haritasında odaklandı");
                    map.FocusRegion((int.MaxValue-50)/50,false);
                    Check(map.ActiveRegion==(int.MaxValue-50)/50,"Son tam 50 bölüm paketi haritada taşmadı");
                }
                SceneManager.LoadScene("Gameplay");yield return Until(()=>GameObject.Find("Bottle 1")!=null,"Döngü Gameplay");
                yield return Resources.UnloadUnusedAssets();GC.Collect();
                yield return Measure("Cycle "+(i+1),30);
            }
            var finalApp=FindFirstObjectByType<PrototypeApp>();
            var finalBoard=(Puzzle)field.GetValue(finalApp);
            var exitMove=PuzzleSolver.Solve(finalBoard.Export()).Moves[0];
            Click("Bottle "+(exitMove.Source+1));Click("Bottle "+(exitMove.Target+1));
            yield return Until(()=>GameObject.Find("Pour stream")!=null,"Çıkış öncesi akış");
            SceneManager.LoadScene("Home");
            yield return Until(()=>GameObject.Find("Play")!=null,"Döküşten çıkış");
            Check(GameObject.Find("Pour stream")==null&&GameplayProgress.Current.BestStars(1)==3,"Döküşte sahneden ayrılma yarım akış veya yeni ödül bırakmadı");
            yield return new WaitForSecondsRealtime(45);
            yield return Measure("Soak Home",120);
            Check(FindObjectsByType<GameFeel>(FindObjectsSortMode.None).Length==1,"Sahne döngüsünde bir ses yöneticisi");
        }

        IEnumerator Measure(string name,int frames,bool warm=true)
        {
            if(warm)yield return new WaitForSecondsRealtime(.4f);
            using var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1);
            using var render=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Render Thread",1);
            using var alloc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1);
            using var memory=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"Total Used Memory",1);
            using var batches=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Batches Count",1);
            using var draws=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count",1);
            var frame=new double[frames];var main=new double[frames];var rt=new double[frames];var gc=new double[frames];
            var gpu=new List<double>(frames);
            var timing=new FrameTiming[1];
            for(int i=0;i<frames;i++)
            {
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
                frame[i]=Time.unscaledDeltaTime*1000;main[i]=cpu.LastValue/1e6;rt[i]=render.LastValue/1e6;gc[i]=alloc.LastValue;
                if(FrameTimingManager.GetLatestTimings(1,timing)>0&&timing[0].gpuFrameTime>0)gpu.Add(timing[0].gpuFrameTime);
            }
            samples.Add(new{name,frames,frameMs=Stats(frame),mainMs=cpu.Valid?Stats(main):null,renderMs=render.Valid?Stats(rt):null,
                gcBytes=alloc.Valid?Stats(gc):null,gpuMs=gpu.Count>0?Stats(gpu.ToArray()):null,gpuSamples=gpu.Count,
                draws=draws.Valid?draws.LastValue:(long?)null,batches=batches.Valid?batches.LastValue:(long?)null,
                usedBytes=memory.Valid?memory.LastValue:(long?)null,objects=FindObjectsByType<Transform>(FindObjectsSortMode.None).Length});
        }

        static object Stats(double[] values)
        {
            Array.Sort(values);
            return new{median=Math.Round(values[values.Length/2],3),p95=Math.Round(values[Math.Min(values.Length-1,(int)Math.Ceiling(values.Length*.95)-1)],3),max=Math.Round(values[^1],3)};
        }

        static void Click(string name)=>GameObject.Find(name).GetComponent<Button>().onClick.Invoke();
        static void Size(int height)=>Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null,new object[]{height});
        static IEnumerator Until(Func<bool> ready,string label)
        {
            float deadline=Time.realtimeSinceStartup+20;
            while(!ready()){if(Time.realtimeSinceStartup>deadline)throw new TimeoutException(label);yield return null;}
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Directory.CreateDirectory(Output);
            var raw=RenderTexture.GetTemporary(Screen.width,Screen.height,0);
            var flipped=RenderTexture.GetTemporary(Screen.width,Screen.height,0);
            var tex=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;
            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(raw);
                Graphics.Blit(raw,flipped,new Vector2(1,-1),new Vector2(0,1));
                RenderTexture.active=flipped;
                tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);tex.Apply();
                File.WriteAllBytes(Path.Combine(Output,name+".jpg"),tex.EncodeToJPG(90));
            }
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(raw);RenderTexture.ReleaseTemporary(flipped);Destroy(tex);}
        }

        void OnDestroy()
        {
            if(active!=this)return;
            active=null;Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
        }
    }
}
#endif
