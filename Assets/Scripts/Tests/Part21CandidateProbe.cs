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
using Mewgic.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class Part21CandidateProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/Part21/UNITY_REGRESSION.json";
        static Part21CandidateProbe active;
        static bool finished, passed;
        readonly string key = "Mewgic.Tests.Part21." + Guid.NewGuid().ToString("N");
        static readonly string[] Suffixes = {"",".pending",".backup",".previous",".corrupt"};
        readonly Dictionary<string,string> player = new Dictionary<string,string>();
        readonly List<string> checks = new List<string>(), errors = new List<string>(), warnings = new List<string>();
        readonly DateTimeOffset now = new DateTimeOffset(2026,11,4,10,0,0,TimeSpan.Zero);
        static FieldInfo Current => typeof(GameplayProgress).GetField("current",BindingFlags.Static|BindingFlags.NonPublic);
        PrototypeApp App => FindFirstObjectByType<PrototypeApp>();
        Puzzle Board => (Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(App);
        ProgressLedger Ledger => GameplayProgress.Current;

        public static void Begin()
        {
            double readyAt=EditorApplication.timeSinceStartup+2;
            EditorApplication.update+=WaitForEditor;
            void WaitForEditor()
            {
                if(EditorApplication.timeSinceStartup<readyAt||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
                EditorApplication.update-=WaitForEditor;
                StartPlay();
            }
        }

        static void StartPlay()
        {
            finished=passed=false;
            EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
            new GameObject("PART21 isolated QA").AddComponent<Part21CandidateProbe>();
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.isPlaying=true;
        }

        static void OnPlayMode(PlayModeStateChange change)
        {
            if(change!=PlayModeStateChange.EnteredEditMode)return;
            EditorApplication.playModeStateChanged-=OnPlayMode;
            if(Application.isBatchMode)EditorApplication.Exit(finished&&passed?0:1);
        }

        void Awake()
        {
            if(active!=null){Destroy(gameObject);return;}
            active=this;DontDestroyOnLoad(gameObject);
            foreach(string suffix in Suffixes)
            {
                string name=GameplayProgress.StorageKey+suffix;
                player[name]=PlayerPrefs.HasKey(name)?PlayerPrefs.GetString(name):null;
            }
            var data=new ProgressData();
            data.presentation.manualLocale="en";
            PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();
            GameplayProgress.UseTestProfile();
            Current.SetValue(null,Open());
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived+=Log;
        }

        ProgressLedger Open()=>GameplayProgress.Open(key,()=>now);
        void Log(string message,string stack,LogType type)
        {
            if(type==LogType.Warning)warnings.Add(message);
            if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+stack);
        }
        void Check(bool pass,string name){if(!pass)throw new InvalidOperationException(name);checks.Add(name);}

        IEnumerator Start()
        {
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
            passed=failure==null&&intact&&errors.Count==0;
            Directory.CreateDirectory(Path.GetDirectoryName(Output));
            File.WriteAllText(Output,Newtonsoft.Json.JsonConvert.SerializeObject(
                new{success=passed,checks,errors,warnings,failure,playerSaveIntact=intact},
                Newtonsoft.Json.Formatting.Indented));
            finished=true;
            if(Application.isBatchMode)EditorApplication.Exit(passed?0:1);
            else EditorApplication.isPlaying=false;
        }

        IEnumerator Run()
        {
            yield return Until(()=>L.Ready&&SceneManager.GetActiveScene().name=="Home"&&GameObject.Find("Play")!=null,"Gerçek Boot → Home");
            Check(L.Code=="en"&&Ledger.Presentation.coins==0,"Boot, dil ve yeni oyuncu başlangıcı");
            var definition=MapProgressionCatalog.Definition();
            Check(definition.regions.Length==12&&definition.regions.All(i=>i.levelCount==50),"12 ada, her adada 50 bölüm");
            Click("ArrivalOpen");
            Check(Open().Presentation.coins==100&&Open().Remaining(BoosterKind.Hint)==4&&!Open().GiftReady,"Sandık ödülü animasyondan önce kaydedildi ve tekrar kilitlendi");
            yield return Until(()=>GameObject.Find("ArrivalContinue")!=null,"Sandık animasyonu");
            Click("ArrivalContinue");
            Click("Play");
            yield return Until(()=>FindFirstObjectByType<PawGuide>()?.ThoughtVisible==true,"Pati kaynak, hedef ve açıklama");
            Check(Board.Moves==1,"Pati tek geçerli döküş yaptı");
            Click("PawThoughtClose");
            Check(Open().TutorialCompleted(PawGuide.BasicId),"Pati tamamlanması diskte");
            yield return Solve();
            Check(Ledger.Completed(1)&&Ledger.LastCoinReward==50&&Open().Presentation.coins==150,"İlk kazanış ve atomik 50 coin");
            Click("ResultHome");
            yield return Until(()=>FindFirstObjectByType<MapWorld>()?.Mounted==true,"Sonuç → dünya haritası");
            var map=FindFirstObjectByType<MapWorld>();
            Check(map.Progression.CurrentId==2&&map.Progression.Stars(1)==3,"Harita yıldızı ve sonraki bölüm");
            Check(map.JourneyRunning&&map.JourneyTarget==2,"Kedi yolculuğu gerçek ilerlemeden başladı");
            yield return Until(()=>!map.JourneyRunning,"Kedi yolculuğunun bitişi");
            Check(map.JourneyTarget==0,"İlk bölüm sonrası rota tamamlandı");
            Check(Open().SetReducedMotion(true),"İzole kayıt tekrar açılıp yazılabildi");
            Current.SetValue(null,Open());
            Check(Ledger.Presentation.coins==150&&Ledger.BestStars(1)==3&&Ledger.ReducedMotion,"Güncel nesil yeniden açılışta korundu");

            var frontier=new ProgressData();
            frontier.presentation.manualLocale="en";
            frontier.presentation.reduceMotion=true;
            for(int level=1;level<=599;level++)
                frontier.levels.Add(new LevelRecord{level=level,bestStars=3,rewardClaimed=true});
            GameplayProgress.UseTestProfile(frontier,()=>now);
            var task=GameplayLevelCatalog.PrepareAsync(600);
            yield return Until(()=>task.IsCompleted,"600 hazırlığı");
            Check(task.IsCompletedSuccessfully&&GameplayLevelCatalog.Select(600,Ledger),"600 doğrulanmış tahtası");
            SceneManager.LoadScene("Gameplay");
            yield return Until(()=>SceneManager.GetActiveScene().name=="Gameplay"&&GameObject.Find("Bottle 1")!=null,"600 Gameplay");
            yield return Solve();
            Check(Ledger.Completed(600),"600 gerçek döküşlerle kazanıldı");
            Click("ResultHome");
            yield return Until(()=>FindFirstObjectByType<MapWorld>()?.Mounted==true,"600 sonrası harita");
            map=FindFirstObjectByType<MapWorld>();
            Check(map.Progression.CurrentId==601&&map.Progression.CurrentRegion==12&&map.Progression.CanEnter(601),"Yeni ada sınırı ve 601 açıldı");
            task=GameplayLevelCatalog.PrepareAsync(601);
            yield return Until(()=>task.IsCompleted,"601 hazırlığı");
            Check(task.IsCompletedSuccessfully&&task.Result.Info.id==601&&GameplayLevelCatalog.Select(601,Ledger),"601 doğrulanmış prosedürel tahta");
            SceneManager.LoadScene("Gameplay");
            yield return Until(()=>SceneManager.GetActiveScene().name=="Gameplay"&&GameObject.Find("Bottle 1")!=null,"601 Gameplay");
            yield return null;
            var app=App;
            var board=Board;
            var step=PuzzleSolver.Solve(board.Export()).Moves[0];
            Click("Bottle "+(step.Source+1));Click("Bottle "+(step.Target+1));
            var pause=typeof(PrototypeApp).GetMethod("OnApplicationPause",BindingFlags.Instance|BindingFlags.NonPublic);
            pause.Invoke(app,new object[]{true});pause.Invoke(app,new object[]{false});
            app.ResumeGame();
            Check(board.Moves==1&&app.GuideCanInteract,"Döküş arka plan simülasyonunda tekrar uygulanmadı");
        }

        IEnumerator Solve()
        {
            var board=Board;
            var plan=PuzzleSolver.Solve(board.Export());
            Check(plan.Status==SearchStatus.Solved,"Çözücü gerçek tahtayı doğruladı");
            foreach(var step in plan.Moves)
            {
                Click("Bottle "+(step.Source+1));Click("Bottle "+(step.Target+1));
                yield return Until(()=>App.GuideCanInteract||FindFirstObjectByType<GameplayEndScreen>().IsShowing,"Döküş tamamlandı");
            }
            Check(board.Won&&FindFirstObjectByType<GameplayEndScreen>().IsShowing,"Kazanış ekranı");
        }

        static void Click(string name)
        {
            var button=GameObject.Find(name)?.GetComponent<Button>();
            if(button==null||!button.interactable)throw new InvalidOperationException("Düğme kullanılamıyor: "+name);
            button.onClick.Invoke();
        }

        static IEnumerator Until(Func<bool> condition,string stage)
        {
            float deadline=Time.realtimeSinceStartup+35;
            while(!condition())
            {
                if(Time.realtimeSinceStartup>deadline)throw new TimeoutException(stage);
                yield return null;
            }
        }

        void OnDestroy()
        {
            if(active!=this)return;
            active=null;
            Application.logMessageReceived-=Log;
            GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            foreach(string suffix in Suffixes)PlayerPrefs.DeleteKey(key+suffix);
            PlayerPrefs.Save();
        }
    }
}
#endif
