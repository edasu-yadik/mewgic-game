#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;
using UnityEngine;using UnityEngine.UI;using UnityEngine.SceneManagement;using UnityEngine.EventSystems;using UnityEditor;using TMPro;using Mewgic.Core;using Mewgic.UI;
namespace Mewgic
{
    [DefaultExecutionOrder(-10000)] public sealed class Part11DProbe:MonoBehaviour
    {
        const string Output="docs/ProductionReports/Part11D/";
        readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();readonly Dictionary<string,string> playerKeys=new Dictionary<string,string>();
        static readonly string[] Tools={"Undo","Hint","ExtraBottle","Shuffle"};static readonly string[] Labels={"Geri Al:","İpucu:","Ek Şişe:","Karıştır:"};
        static Part11DProbe active;
        void Awake(){if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;playerKeys[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}GameplayProgress.UseTestProfile();GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;}
        void Log(string m,string s,LogType t){if(t==LogType.Warning)warnings.Add(m);if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m);}
        void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();PawGuide Guide=>FindFirstObjectByType<PawGuide>();
        Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(App);
        string Board=>string.Join("|",Model.Export().Select(b=>string.Join(",",b)));
        string Stock=>string.Join(",",Enum.GetValues(typeof(BoosterKind)).Cast<BoosterKind>().Select(k=>GameplayProgress.Current.Remaining(k)));
        void Tap(string name){var go=GameObject.Find(name);Check(go!=null,"Buton: "+name);go.GetComponent<Button>().onClick.Invoke();}
        IEnumerator Start(){var run=Run();string failure=null;while(true){bool more=false;object next=null;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception e){failure=e.ToString();}if(!more||failure!=null)break;yield return next;}bool intact=playerKeys.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.HasKey(p.Key)&&PlayerPrefs.GetString(p.Key)==p.Value);Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();File.WriteAllText(Output+"TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&intact,failure,checks,errors,warnings,playerSaveAndBackupIntact=intact,unity=Application.unityVersion},Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        IEnumerator WaitGameplay(){float until=Time.realtimeSinceStartup+8;do{yield return null;}while((Guide==null||GameObject.Find("Bottle 1")==null)&&Time.realtimeSinceStartup<until);yield return null;yield return null;}
        IEnumerator LearnFirstMove(){Tap("PawNext");var move=Guide.Recommended;Tap("Bottle "+(move.Source+1));Tap("Bottle "+(move.Target+1));yield return new WaitForSecondsRealtime(1.1f);}
        IEnumerator Run()
        {
            yield return WaitGameplay();yield return new WaitForSecondsRealtime(.25f);Check(Guide.State==PawGuide.Phase.Welcome&&GameObject.Find("PawBoosterShield")==null,"İlk bölüm başında yalnız temel eğitim var");Capture(Output+"BasicStart.png",900,1600);string initialStock=Stock;
            Tap("PawNext");var move=Guide.Recommended;Tap("Bottle "+(move.Source+1));Tap("Bottle "+(move.Target+1));yield return new WaitForSecondsRealtime(1.1f);Check(Model.Moves==1&&Guide.State==PawGuide.Phase.Celebration&&Stock==initialStock,"Önce gerçek temel hamle ve kutlama");
            yield return new WaitForSecondsRealtime(1.9f);Check(Guide.State==PawGuide.Phase.BoosterOffer&&GameObject.Find("PawMessage").GetComponent<TMP_Text>().text=="Özel güçleri de öğrenmek ister misin?","Temel eğitimden sonra isteğe bağlı davet");Check(GameObject.Find("PawHighlight")==null,"Davet güçleri otomatik anlatmıyor");yield return new WaitForSecondsRealtime(.25f);foreach(int h in new[]{1600,1950,2000})Capture(Output+"Offer-"+h+".png",900,h,h==2000);
            string before=Board;Tap("PawNext");Check(Guide.State==PawGuide.Phase.Boosters&&GameplayProgress.Current.TutorialCompleted(PawGuide.BoosterOfferId),"Kabul ilk güç açıklamasını açıyor");
            for(int step=0;step<4;step++) {
                yield return new WaitForSecondsRealtime(.25f);Check(GameObject.Find("PawMessage").GetComponent<TMP_Text>().text.StartsWith(Labels[step]),"Tek ve doğru açıklama: "+Tools[step]);
                Check(TopHit(Tools[step])=="PawBoosterShield","Gerçek buton dokunuşu eğitim katmanında: "+Tools[step]);
                foreach(var name in Tools)Tap(name);Tap("Bottle 1");Tap("Bottle 3");Check(Stock==initialStock&&Board==before&&Model.Moves==1,"Eğitim hiçbir güç hakkını ve şişeyi değiştirmiyor: "+Tools[step]);
                foreach(int h in new[]{1600,1950,2000})Capture(Output+Tools[step]+"-"+h+".png",900,h,h==2000);
                if(step==0)for(int frame=0;frame<12;frame++){Capture(Output+"Motion-"+frame.ToString("D2")+".png",450,800);yield return new WaitForSecondsRealtime(.10f);}
                Tap("PawNext");
            }
            Check(!Guide.Visible&&GameplayProgress.Current.TutorialCompleted(PawGuide.BoosterId)&&Stock==initialStock&&Board==before,"Dört adım bitince kapanır; miktarlar aynı kalır");yield return new WaitForSecondsRealtime(.3f);Check(GameObject.Find("PawBoosterShield")==null&&GameObject.Find("PawMenuShield")==null,"Eğitim bittiğinde tüm koruma katmanları kalkıyor");
            Tap("Undo");Check(Model.Moves==0&&GameplayProgress.Current.Remaining(BoosterKind.Undo)==2,"Normal Geri Al hâlâ çalışır ve kendi hakkını tüketir");Tap("Restart");
            Tap("PawCatTouch");Tap("PawBoosters");Check(Guide.State==PawGuide.Phase.Boosters,"Tamamlanan rehber kediden yeniden açılır");Tap("PawNext");string stock=Stock;Tap("PawClose");Check(!Guide.Visible&&Stock==stock,"Rehber ortasında Atla hak tüketmez");yield return new WaitForSecondsRealtime(.3f);Tap("PawCatTouch");Tap("PawBoosters");Check(GameObject.Find("PawMessage").GetComponent<TMP_Text>().text.StartsWith(Labels[0]),"Yeniden açılan rehber güvenli biçimde ilk adımdan başlar");Tap("Pause");Check(App.IsPaused&&!Guide.Visible,"Pause eğitimi güvenle kapatır");Tap("Resume");yield return new WaitForSecondsRealtime(.3f);Check(GameObject.Find("PawBoosterShield")==null,"Resume sonrasında güçler kilitli kalmaz");
            var saved=JsonUtility.ToJson(GameplayProgress.Current.Snapshot);GameplayProgress.UseTestProfile(JsonUtility.FromJson<ProgressData>(saved));SceneManager.LoadScene("Gameplay");yield return WaitGameplay();yield return new WaitForSecondsRealtime(.25f);Check(!Guide.Visible&&GameplayProgress.Current.TutorialCompleted(PawGuide.BoosterId),"Öğrenilmiş eğitim yeni girişte otomatik tekrarlanmaz");
            // New player who declines the optional invitation.
            GameplayProgress.UseTestProfile();SceneManager.LoadScene("Gameplay");yield return WaitGameplay();Tap("PawNext");move=Guide.Recommended;Tap("Bottle "+(move.Source+1));Tap("Bottle "+(move.Target+1));yield return new WaitForSecondsRealtime(3.1f);Check(Guide.State==PawGuide.Phase.BoosterOffer,"Atlama senaryosunda davet");stock=Stock;Tap("PawClose");Check(!Guide.Visible&&GameplayProgress.Current.TutorialCompleted(PawGuide.BoosterOfferId)&&!GameplayProgress.Current.TutorialCompleted(PawGuide.BoosterId)&&Stock==stock,"Şimdi Değil kararı saklanır; rehber tamamlanmış sayılmaz");yield return new WaitForSecondsRealtime(.3f);
            Tap("PawCatTouch");Tap("PawBoosters");Tap("PawNext");Tap("PawClose");Check(!GameplayProgress.Current.TutorialCompleted(PawGuide.BoosterId)&&Stock==stock,"Yarım rehber Atla ile tamamlanmış sayılmaz");yield return new WaitForSecondsRealtime(.3f);
            // The guide works even when all real powers have zero stock.
            GameplayProgress.UseTestProfile(new ProgressData{undo=0,hint=0,extra=0,shuffle=0,completedTutorials=new List<string>{PawGuide.BasicId}});SceneManager.LoadScene("Gameplay");yield return WaitGameplay();Tap("PawCatTouch");Tap("PawBoosters");before=Board;
            for(int i=0;i<4;i++){foreach(var name in Tools)Tap(name);Check(Stock=="0,0,0,0"&&Board==before&&Model.Moves==0,"Sıfır hakla tanıtım yeni hak üretmez: "+Tools[i]);Tap("PawNext");}yield return new WaitForSecondsRealtime(.3f);Check(!Guide.Visible&&Stock=="0,0,0,0","Sıfır hak senaryosu güvenle biter");
            // Existing gameplay and Hint remain usable after the lesson.
            GameplayProgress.UseTestProfile(new ProgressData{completedTutorials=new List<string>{PawGuide.BasicId,PawGuide.BoosterId,PawGuide.BoosterOfferId}});SceneManager.LoadScene("Gameplay");yield return WaitGameplay();var valid=PuzzleSolver.Solve(Model.Export()).Moves[0];Tap("Bottle "+(valid.Source+1));Tap("Bottle "+(valid.Target+1));yield return new WaitForSecondsRealtime(1.1f);Check(Model.Moves==1,"Rehber sonrası normal sıvı aktarımı");Tap("Undo");Tap("Hint");Check(Model.Moves==0&&GameplayProgress.Current.Remaining(BoosterKind.Hint)==2&&FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Length==2,"Gerçek Hint mevcut kurala göre çalışır");
        }
        Rect Bounds(RectTransform r,RectTransform relative){var c=new Vector3[4];r.GetWorldCorners(c);var a=(Vector2)relative.InverseTransformPoint(c[0]);var b=(Vector2)relative.InverseTransformPoint(c[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
        void AuditLayout(RectTransform safe,int height)
        {
            var go=GameObject.Find("PawBubble");if(go==null||safe==null)return;var area=Bounds((RectTransform)go.transform,safe);Check(area.xMin>=safe.rect.xMin&&area.xMax<=safe.rect.xMax&&area.yMin>=safe.rect.yMin&&area.yMax<=safe.rect.yMax,"Balon Safe Area içinde: "+height);
            foreach(var view in FindObjectsByType<GameplayBottleView>(FindObjectsSortMode.None))Check(!area.Overlaps(Bounds(view.Rig,safe)),"Açıklama şişeyi örtmüyor: "+height);
            foreach(var t in go.GetComponentsInChildren<TMP_Text>()){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Açıklama/metin taşmıyor: "+t.name+height);}
            if(Guide.State==PawGuide.Phase.Boosters){int page=Array.FindIndex(Labels,l=>GameObject.Find("PawMessage").GetComponent<TMP_Text>().text.StartsWith(l));var marker=GameObject.Find("PawHighlight").GetComponent<RectTransform>();var target=GameObject.Find(Tools[page]).GetComponent<RectTransform>();var m=Bounds(marker,safe);var r=Bounds(target,safe);Check(Vector2.Distance(m.center,r.center)<1,"Doğru güç vurgulanıyor: "+Tools[page]+height);Check(m.yMax+28<area.yMin,"Pati konuşma balonuna çarpmıyor: "+height);Check(FindObjectsByType<PawHighlight>(FindObjectsSortMode.None).Length==1,"Aynı anda tek güç vurgusu");}
        }
        string TopHit(string name)
        {
            var canvas=FindFirstObjectByType<Canvas>();var camera=Camera.main;var mode=canvas.renderMode;var oldCam=canvas.worldCamera;var target=camera.targetTexture;float distance=canvas.planeDistance;var rt=new RenderTexture(900,1600,24);
            try {camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();FindFirstObjectByType<GameplayPresentation>().Layout();Guide.SendMessage("LateUpdate");Canvas.ForceUpdateCanvases();var rect=GameObject.Find(name).GetComponent<RectTransform>();var point=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center))};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(point,hits);return hits.Count>0?hits[0].gameObject.name:"";}
            finally{camera.targetTexture=target;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=distance;rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();FindFirstObjectByType<GameplayPresentation>().Layout();Guide.SendMessage("LateUpdate");}
        }

        void Capture(string filename,int width,int height,bool inset=false)
        {
            var canvas=FindFirstObjectByType<Canvas>();var camera=Camera.main;var p=FindFirstObjectByType<HomeCharacterPresenter>();var safe=p!=null?(RectTransform)p.transform.Find("UI"):(GameObject.Find("Safe area")??GameObject.Find("MapSafeArea"))?.GetComponent<RectTransform>();
            var mode=canvas.renderMode;var oldCam=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;Vector2 min=safe!=null?safe.anchorMin:Vector2.zero,max=safe!=null?safe.anchorMax:Vector2.one;
            var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;if(inset&&safe!=null){safe.anchorMin=new Vector2(.035f,.04f);safe.anchorMax=new Vector2(.965f,.955f);}
                Canvas.ForceUpdateCanvases();var gameplay=FindFirstObjectByType<GameplayPresentation>();if(gameplay!=null)gameplay.Layout();var pause=GameObject.Find("PauseSafeArea");if(pause!=null&&safe!=null){var pr=(RectTransform)pause.transform;pr.anchorMin=safe.anchorMin;pr.anchorMax=safe.anchorMax;}if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}var map=FindFirstObjectByType<Mewgic.Map.MapWorld>();if(map!=null)map.Layout();var daily=FindFirstObjectByType<Mewgic.Daily.DailyActivitiesPanel>();if(daily!=null&&daily.IsOpen)daily.Layout();var hub=FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>();if(hub!=null&&hub.IsOpen)hub.Layout();Canvas.ForceUpdateCanvases();
                foreach(var text in FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None))text.ForceMeshUpdate();FindFirstObjectByType<Mewgic.UI.PawGuide>()?.SendMessage("LateUpdate");foreach(var text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){text.ForceMeshUpdate();if(text.name=="PawMessage")Check(!text.isTextOverflowing,"Pati balonunda taşma yok: "+height);}AuditLayout(safe,height);camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(filename,tex.EncodeToPNG());
            }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=distance;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}}
        }
    }
}
#endif
