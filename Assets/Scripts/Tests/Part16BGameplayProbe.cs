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
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class Part16BGameplayProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/Part16B";
        static Part16BGameplayProbe active;
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly Dictionary<string,string> player = new Dictionary<string,string>();
        readonly List<string> checks = new List<string>(), errors = new List<string>();
        Touchscreen touch;
        int touchId;
        string key;
        PrototypeApp App => FindFirstObjectByType<PrototypeApp>();
        PawGuide Guide => FindFirstObjectByType<PawGuide>();
        Puzzle Board => (Puzzle)typeof(PrototypeApp).GetField("puzzle",Private).GetValue(App);
        int Selected => (int)typeof(PrototypeApp).GetField("selected",Private).GetValue(App);

        public static void Begin()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
            new GameObject("PART16B isolated QA").AddComponent<Part16BGameplayProbe>();
            EditorApplication.isPlaying=true;
        }

        void Awake()
        {
            if(active!=null){Destroy(gameObject);return;}
            active=this;DontDestroyOnLoad(gameObject);Application.runInBackground=true;
            Directory.CreateDirectory(Output);
            foreach(string suffix in new[]{"",".backup",".corrupt"})
            {
                string k=GameplayProgress.StorageKey+suffix;
                player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;
            }
            key="Mewgic.Tests.Part16B."+Guid.NewGuid().ToString("N");
            var data=new ProgressData();data.presentation.manualLocale="tr";
            PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();
            GameplayProgress.UseTestProfile();
            typeof(GameplayProgress).GetField("current",BindingFlags.Static|BindingFlags.NonPublic)
                .SetValue(null,GameplayProgress.Open(key));
            GameplayLevelCatalog.ResetSelection();

            Application.logMessageReceived+=Log;
        }

        void Log(string message,string stack,LogType type)
        {
            if(type is LogType.Error or LogType.Assert or LogType.Warning or LogType.Exception)errors.Add(message);
        }

        void Check(bool condition,string name)
        {
            if(!condition)throw new InvalidOperationException(name);
            checks.Add(name);
        }

        IEnumerator Start()
        {
            var window=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            window.Show();window.maximized=true;window.Focus();
            yield return null;
            Size(1600);
            yield return new WaitForSecondsRealtime(.6f);
            touch=InputSystem.AddDevice<Touchscreen>();
            var stack=new Stack<IEnumerator>();stack.Push(Run());
            string failure=null;
            while(stack.Count>0)
            {
                bool more=false;object next=null;
                try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}
                catch(Exception error){failure=error.ToString();break;}
                if(!more){stack.Pop();continue;}
                if(next is IEnumerator iterator&&!(next is CustomYieldInstruction)){stack.Push(iterator);continue;}
                yield return next;
            }
            bool intact=player.All(x=>x.Value==null?!PlayerPrefs.HasKey(x.Key):PlayerPrefs.GetString(x.Key)==x.Value);
            File.WriteAllText(Path.Combine(Output,"UNITY_TESTS.json"),Newtonsoft.Json.JsonConvert.SerializeObject(
                new{success=failure==null&&errors.Count==0&&intact,failure,checks,errors,playerSaveIntact=intact,touchDevice="InputSystem.Touchscreen"},
                Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying=false;
        }

        static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null,new object[]{height});

        IEnumerator Until(Func<bool> condition,string label,float seconds=12)
        {
            float deadline=Time.unscaledTime+seconds;
            while(!condition()&&Time.unscaledTime<deadline)yield return null;
            Check(condition(),label);
        }

        static Vector2 Center(GameObject target)
        {
            var rect=(RectTransform)target.transform;
            return RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        }

        static GameObject Hit(Vector2 point)
        {
            var data=new PointerEventData(EventSystem.current){position=point};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            return hits.Count==0?null:hits[0].gameObject;
        }

        void QueueTouch(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 position) =>
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=position});

        IEnumerator Tap(string name)
        {
            Canvas.ForceUpdateCanvases();
            var target=GameObject.Find(name);
            Check(target!=null,"Dokunma hedefi var "+name);
            Vector2 point=Center(target);
            Check(Hit(point)==target,"Dokunmatik raycast "+name+" / "+Hit(point)?.name);
            int id=++touchId;
            QueueTouch(id,UnityEngine.InputSystem.TouchPhase.Began,point);
            yield return null;yield return null;
            QueueTouch(id,UnityEngine.InputSystem.TouchPhase.Ended,point);
            yield return null;yield return null;
        }

        IEnumerator Run()
        {
            foreach(var size in new[]{new Vector2(630,960),new Vector2(570,1060),new Vector2(525,980)})
                foreach(int count in new[]{2,3,4,5,6,7,8,12})
                {
                    var slots=new List<Rect>();
                    for(int i=0;i<count;i++)
                    {
                        Rect slot=GameplayPresentation.Slot(i,count,size);
                        Check(slot.xMin>=0&&slot.xMax<=1.001f&&slot.yMin>=0&&slot.yMax<=1.001f&&
                            slot.width*size.x>=44&&slots.All(x=>!x.Overlaps(slot)),"Şişe düzeni "+count);
                        slots.Add(slot);
                    }
                }
            yield return Until(()=>L.Ready&&FindFirstObjectByType<MapWorld>()?.Mounted==true,"Harita hazır");
            FindFirstObjectByType<MapWorld>().EnterLevel(1);
            yield return Until(()=>Guide!=null&&GameObject.Find("Bottle 1")!=null,"İlk bölüm hazır");
            yield return Until(()=>Guide.State==PawGuide.Phase.SourceClick,"Pati kaynak şişeye ulaştı");
            yield return Shot("Pati_Kaynak",1600);
            yield return Until(()=>Guide.State==PawGuide.Phase.TargetClick,"Pati hedef şişeye ulaştı");
            yield return Shot("Pati_Hedef",1600);
            yield return Until(()=>Guide.ThoughtVisible,"Kedi düşünce balonunu açtı");
            Check(Board.Moves==1&&GameObject.Find("SpecialGoal")==null,"Pati tek gerçek döküş yaptı, gereksiz hedef kutusu yok");
            yield return new WaitForSecondsRealtime(.32f);
            yield return Shot("Pati_Balon",1600);
            yield return Tap("PawThoughtClose");
            Check(!Guide.DemoRunning&&GameplayProgress.Open(key).TutorialCompleted(PawGuide.BasicId),"Erken kapatma öğreticiyi bir kez kaydetti");
            yield return Tap("Restart");
            Check(Board.Moves==0&&!Guide.DemoRunning,"Tekrar denemede eğitim zorunlu açılmadı");

            var move=PuzzleSolver.Solve(Board.Export()).Moves[0];
            Vector2 sourcePoint=Center(GameObject.Find("Bottle "+(move.Source+1)));
            Vector2 targetPoint=Center(GameObject.Find("Bottle "+(move.Target+1)));
            int firstTouch=++touchId,secondTouch=++touchId;
            QueueTouch(firstTouch,UnityEngine.InputSystem.TouchPhase.Began,sourcePoint);
            QueueTouch(secondTouch,UnityEngine.InputSystem.TouchPhase.Began,targetPoint);
            yield return null;yield return null;
            QueueTouch(firstTouch,UnityEngine.InputSystem.TouchPhase.Ended,sourcePoint);
            QueueTouch(secondTouch,UnityEngine.InputSystem.TouchPhase.Ended,targetPoint);
            yield return null;yield return null;
            Check(Board.Moves==0,"Aynı karede iki parmak otomatik döküş yapmadı");
            yield return Tap("Restart");
            yield return new WaitForSecondsRealtime(.19f);
            int full=Enumerable.Range(0,Board.Count).First(i=>i!=move.Source&&Board.Bottle(i).Count==Puzzle.Capacity);
            yield return Tap("Bottle "+(move.Source+1));
            yield return Tap("Bottle "+(full+1));
            Check(Board.Moves==0&&Selected==move.Source&&GameObject.Find("Status").GetComponent<TMP_Text>().text==
                L.Resolve(L.V("gameplay.target_full")),"Geçersiz dolu hedef geri bildirimi ve seçim korunumu");
            yield return Shot("Gecersiz_Hamle",1600);
            yield return Tap("Bottle "+(move.Target+1));
            Check(Board.Moves==1,"Geçerli dokunuş tek hamle");
            yield return Tap("Bottle "+(move.Target+1));
            Check(Board.Moves==1,"Animasyonda hızlı tekrar hamle üretmedi");
            typeof(PrototypeApp).GetMethod("OnApplicationPause",Private).Invoke(App,new object[]{true});
            Check(App.IsPaused&&!FindFirstObjectByType<GameplayPresentation>().IsAnimating&&GameObject.Find("Pour stream")==null&&Board.Moves==1,
                "Arka plan simülasyonu döküşü tek kez toparladı");
            for(int i=0;i<Board.Count;i++)
                Check(GameObject.Find("Bottle "+(i+1)).GetComponent<GameplayBottleView>().DisplayedLayers.SequenceEqual(Board.Bottle(i)),
                    "Arka plan sonrası şişe görseli modelle aynı");
            yield return null;
            yield return Tap("Resume");
            Check(!App.IsPaused&&Board.Moves==1,"Devam tek hamleyi korudu");

            yield return Tap("Bottle "+(move.Source+1));
            yield return Tap("Bottle "+(move.Target+1));
            Check(Board.Moves==1&&GameObject.Find("Status").GetComponent<TMP_Text>().text==
                L.Resolve(L.V("gameplay.target_color")),"Uyumsuz renk geri bildirimi");
            yield return Tap("Undo");
            Check(Board.Moves==0&&GameplayProgress.Current.Remaining(BoosterKind.Undo)==2&&Guide.State==PawGuide.Phase.Feature,"Undo ve ilk kısa pati ipucu");
            yield return Tap("Hint");
            int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);
            Check(hints==2&&FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Length==2,"İpucu iki şişeyi işaretledi");
            yield return Tap("Hint");
            Check(GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints,"Aynı ipucu stoktan ikinci kez düşmedi");
            yield return new WaitForSecondsRealtime(3.25f);
            Check(!Guide.ThoughtVisible,"Kısa mekanik balonu kendiliğinden kapandı");
            int baseCount=Board.Count;
            yield return Tap("ExtraBottle");
            Check(Board.Count==baseCount+1&&GameplayProgress.Current.Remaining(BoosterKind.ExtraBottle)==0,"Ek Şişe bir boş şişe ekledi");
            yield return new WaitForSecondsRealtime(.32f);
            Check(Guide.ThoughtVisible&&GameObject.Find("PawCursor")!=null,"Ek Şişe için kısa hareketli pati");
            yield return Shot("Ek_Sise",1600);
            yield return Tap("Shuffle");
            Check(PuzzleSolver.Solve(Board.Export()).Status==SearchStatus.Solved&&GameplayProgress.Current.Remaining(BoosterKind.Shuffle)==0,
                "Karıştır çözülebilirliği ve stok kuralını korudu");
            yield return Tap("Restart");

            foreach(int height in new[]{1600,1950,2000})
            {
                Size(height);yield return new WaitForSecondsRealtime(.6f);
                var safe=GameObject.Find("Safe area").GetComponent<RectTransform>();
                Vector2 min=safe.anchorMin,max=safe.anchorMax;
                safe.anchorMin=new Vector2(.035f,.035f);safe.anchorMax=new Vector2(.965f,.955f);
                Canvas.ForceUpdateCanvases();FindFirstObjectByType<GameplayPresentation>().Layout();
                Canvas.ForceUpdateCanvases();yield return null;
                var bounds=new List<Rect>();
                for(int i=0;i<Board.Count;i++)
                {
                    var bottle=GameObject.Find("Bottle "+(i+1));
                    var rect=(RectTransform)bottle.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);
                    var a=(Vector2)safe.InverseTransformPoint(corners[0]);var b=(Vector2)safe.InverseTransformPoint(corners[2]);
                    var box=Rect.MinMaxRect(a.x,a.y,b.x,b.y);
                    Check(box.width*safe.GetComponentInParent<Canvas>().scaleFactor>=44&&safe.rect.Contains(box.min)&&safe.rect.Contains(box.max)&&
                        bounds.All(x=>!x.Overlaps(box))&&Hit(Center(bottle))==bottle,"Portre şişe sınırı ve dokunma "+height);
                    bounds.Add(box);
                }
                yield return Shot("Oynanis_"+height,height);
                safe.anchorMin=min;safe.anchorMax=max;
            }
            Size(1600);yield return new WaitForSecondsRealtime(.55f);
            var solution=PuzzleSolver.Solve(Board.Export()).Moves;
            for(int i=0;i<solution.Count;i++)
            {
                var step=solution[i];
                yield return Tap("Bottle "+(step.Source+1));yield return Tap("Bottle "+(step.Target+1));
                if(i!=solution.Count-1){yield return new WaitForSecondsRealtime(.95f);continue;}
                App.PauseGame();
                typeof(PrototypeApp).GetMethod("OnApplicationPause",Private).Invoke(App,new object[]{true});
                Check(Board.Won&&GameplayProgress.Open(key).Completed(1),"Son hamlede duraklatma ve arka plan kazanışı kaydetti");
                long balance=GameplayProgress.Current.Presentation.coins;
                yield return null;
                yield return Tap("Resume");
                Check(GameplayProgress.Current.Presentation.coins==balance,"Devam ödülü ikinci kez vermedi");
            }
            Check(Board.Won&&GameplayProgress.Open(key).Completed(1),"Gerçek dokunmalarla kazanış kaydedildi");
            yield return new WaitForSecondsRealtime(.35f);
            yield return Shot("Bolum_Sonu",1600);
            yield return Tap("ResultHome");
            yield return Until(()=>SceneManager.GetActiveScene().name=="LevelMap"&&FindFirstObjectByType<MapWorld>()?.Mounted==true,
                "Sonuçtan haritaya gerçek dönüş");
            yield return new WaitForSecondsRealtime(2);
            if(GameObject.Find("ChapterClose")!=null)yield return Tap("ChapterClose");
            yield return Tap("MapHome");
            yield return Until(()=>GameObject.Find("SettingsButton")!=null,"Ana menü hazır");
            yield return new WaitForSecondsRealtime(.55f);
            if(GameObject.Find("ArrivalOpen")!=null)
            {
                yield return Tap("ArrivalOpen");yield return new WaitForSecondsRealtime(1.3f);yield return Tap("ArrivalContinue");
            }
            string before=JsonUtility.ToJson(GameplayProgress.Current.Snapshot);
            yield return Tap("SettingsButton");
            yield return Shot("Ayarlar_Tekrar",1600);
            yield return Tap("TutorialReplay");
            yield return Until(()=>SceneManager.GetActiveScene().name=="Gameplay"&&Guide!=null&&App.IsTutorialReplay,"Ayarlar izole ilk bölümü açtı");
            yield return Until(()=>Guide.ThoughtVisible,"Tekrar rehberi kayda dokunmadan balona ulaştı");
            float thoughtAt=(float)typeof(PawGuide).GetField("thoughtAt",Private).GetValue(Guide);
            yield return Until(()=>SceneManager.GetActiveScene().name=="Home"&&GameObject.Find("SettingsButton")!=null,"Tekrardan ana menüye dönüş");
            Check(Time.unscaledTime-thoughtAt>=3&&Time.unscaledTime-thoughtAt<4.5f,"Tekrar balonu yaklaşık üç saniyede kapandı");
            Check(JsonUtility.ToJson(GameplayProgress.Current.Snapshot)==before,"Tekrar modu ilerleme, ödül ve eğitim kaydını değiştirmedi");
        }

        IEnumerator Shot(string name,int height)
        {
            yield return new WaitForEndOfFrame();
            Check(Screen.width==900&&Screen.height==height,"Game View portre "+name+" / "+Screen.width+"x"+Screen.height);
            var raw=RenderTexture.GetTemporary(Screen.width,Screen.height,0);
            var flipped=RenderTexture.GetTemporary(Screen.width,Screen.height,0);
            var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;
            try
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(raw);
                Graphics.Blit(raw,flipped,new Vector2(1,-1),new Vector2(0,1));
                RenderTexture.active=flipped;
                image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Output,name+".png"),image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=previous;RenderTexture.ReleaseTemporary(raw);RenderTexture.ReleaseTemporary(flipped);Destroy(image);
            }
        }

        void OnDestroy()
        {
            if(active!=this)return;
            active=null;Application.logMessageReceived-=Log;
            if(touch!=null&&touch.added)InputSystem.RemoveDevice(touch);
            GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
        }
    }
}
#endif
