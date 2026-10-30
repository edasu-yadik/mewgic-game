#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;
using UnityEngine;using UnityEngine.UI;using UnityEngine.SceneManagement;using UnityEditor;using TMPro;using Mewgic.Core;using Mewgic.UI;
namespace Mewgic
{
    [DefaultExecutionOrder(-10000)] public sealed class Part11BProbe:MonoBehaviour
    {
        public bool verifyCompletedOnReload,previewOnly;
        const string Output="docs/ProductionReports/Part11B/";
        readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
        readonly Dictionary<string,string> playerKeys=new Dictionary<string,string>();
        string key;static Part11BProbe active;
        void Awake()
        {
            if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);
            foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;playerKeys[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}
            key=SessionState.GetString("Part11B.Key","");if(previewOnly)GameplayProgress.UseTestProfile();else Bind(key);GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;
        }
        void Bind(string profile){GameplayProgress.UseTestProfile();typeof(GameplayProgress).GetField("current",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,GameplayProgress.Open(profile));}
        void Log(string m,string s,LogType t){if(t==LogType.Warning)warnings.Add(m);if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m);}
        void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
        PawGuide Guide=>FindFirstObjectByType<PawGuide>();
        Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(App);
        int Selected=>(int)typeof(PrototypeApp).GetField("selected",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(App);
        void Tap(string name){var go=GameObject.Find(name);Check(go!=null,"Buton: "+name);go.GetComponent<Button>().onClick.Invoke();}
        IEnumerator Start()
        {
            if(previewOnly)yield break;
            var run=verifyCompletedOnReload?Reload():Run();string failure=null;
            while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception e){failure=e.ToString();}if(failure!=null||!more)break;yield return next;}
            bool intact=playerKeys.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.HasKey(p.Key)&&PlayerPrefs.GetString(p.Key)==p.Value);
            Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            var result=new{success=failure==null&&errors.Count==0&&intact,failure,checks,errors,warnings,playerSaveAndBackupIntact=intact,realPlayerPrefsTest=true,playModeReload=verifyCompletedOnReload,unity=Application.unityVersion};
            File.WriteAllText(Output+(verifyCompletedOnReload?"TESTS.json":"PHASE1.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
            if(verifyCompletedOnReload||failure!=null)foreach(var profile in new[]{key,key+".interrupted",key+".failed",key+".missing"})foreach(var suffix in new[]{"",".backup",".corrupt"})PlayerPrefs.DeleteKey(profile+suffix);
            if(verifyCompletedOnReload||failure!=null)PlayerPrefs.Save();EditorApplication.isPlaying=false;
        }
        void OnDestroy(){if(previewOnly){Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();}}
        IEnumerator WaitGameplay()
        {
            float deadline=Time.realtimeSinceStartup+8;
            do{yield return null;}while(Time.realtimeSinceStartup<deadline&&(Guide==null||GameObject.Find("Bottle 1")==null||(!Guide.Visible&&!GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId)&&!GameplayProgress.Current.Completed(1))));
        }
        IEnumerator Run()
        {
            yield return null;yield return null;
            Check(SceneManager.GetActiveScene().name=="Home"&&Guide==null,"Home'da eğitim veya ikinci yönetici yok");Tap("Play");
            float deadline=Time.realtimeSinceStartup+8;while((Guide==null||Guide.State!=PawGuide.Phase.Welcome)&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(Guide!=null&&Guide.State==PawGuide.Phase.Welcome&&GameObject.Find("Bottle 1")!=null&&GameObject.Find("GameplayWizardCat")!=null,"Hazır Gameplay'de ilk ziyaret otomatik karşılama / "+SceneManager.GetActiveScene().name+" / "+(Guide==null?"no guide":Guide.State.ToString())+" / done="+GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId));
            Check(GameObject.Find("PawMessage").GetComponent<TMP_Text>().text=="Merhaba! Ben senin büyücü yardımcınım. Hadi ilk iksirimizi hazırlayalım!","Yeni karşılama metni");
            Tap("Bottle 1");Check(Model.Moves==0&&Selected<0,"Karşılama Devam/Kapat ile güvenli ilerliyor");
            yield return new WaitForSecondsRealtime(.25f);
            foreach(int h in new[]{1600,1950,2000})Capture(Output+"Welcome-"+h+".png",900,h,h==2000);
            Tap("PawNext");Check(Guide.State==PawGuide.Phase.Source,"Devam kaynak aşaması");var move=Guide.Recommended;
            var entrance=GameObject.Find("PawBubble").GetComponent<CanvasGroup>();Check(entrance.alpha==0,"Konuşma balonu şeffaf başlayarak yumuşak açılıyor");yield return new WaitForSecondsRealtime(.25f);Check(entrance.alpha>.9f,"Konuşma balonu giriş animasyonu tamamlanıyor");
            var solved=PuzzleSolver.Solve(Model.Export());Check(solved.Status==SearchStatus.Solved&&solved.Moves[0].Source==move.Source&&solved.Moves[0].Target==move.Target,"Öneri doğrulanmış çözüm yolunda");
            int wrongSource=Enumerable.Range(0,Model.Count).First(i=>i!=move.Source&&Model.Bottle(i).Count>0);Tap("Bottle "+(wrongSource+1));Check(Guide.State==PawGuide.Phase.Source&&Selected<0&&Model.Moves==0,"Yanlış kaynak aşamayı ve modeli değiştirmiyor");
            yield return new WaitForSecondsRealtime(.25f);foreach(int h in new[]{1600,1950,2000})Capture(Output+"Source-"+h+".png",900,h,h==2000);
            for(int i=0;i<12;i++){Capture(Output+"SourceMotion-"+i.ToString("D2")+".png",450,800);yield return new WaitForSecondsRealtime(.10f);}
            Tap("Bottle "+(move.Source+1));Check(Guide.State==PawGuide.Phase.Target&&Model.Moves==0,"Doğru kaynak hedef aşamasına geçiriyor");
            int wrongTarget=Enumerable.Range(0,Model.Count).First(i=>i!=move.Source&&i!=move.Target);Tap("Bottle "+(wrongTarget+1));Check(Guide.State==PawGuide.Phase.Target&&Model.Moves==0&&!GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId),"Yanlış hedef eğitimi tamamlamıyor");
            int otherLegal=Enumerable.Range(0,Model.Count).First(i=>i!=move.Source&&i!=move.Target&&new Puzzle(Model.Export()).Pour(move.Source,i)>0);Tap("Bottle "+(otherLegal+1));Check(Guide.State==PawGuide.Phase.Target&&Model.Moves==0,"İşaretlenmeyen diğer geçerli hedef de eğitim hamlesini değiştirmiyor");
            yield return new WaitForSecondsRealtime(.25f);foreach(int h in new[]{1600,1950,2000})Capture(Output+"Target-"+h+".png",900,h,h==2000);
            int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);Tap("Bottle "+(move.Target+1));Tap("Bottle "+(move.Target+1));Tap("Bottle "+(move.Source+1));
            Check(Model.Moves==1&&!GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId),"Tek hamle uygulandı; animasyon bitmeden eğitim kaydedilmedi");
            yield return new WaitForSecondsRealtime(1.05f);Check(Guide.State==PawGuide.Phase.Celebration&&Model.Moves==1,"Gerçek animasyon tamamlanınca kutlama");
            Check(FindFirstObjectByType<Characters.WizardCatPresentation>().MagicBusy,"Mevcut WizardCat Wand Magic kutlaması");
            Check(GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId)&&GameplayProgress.Open(key).TutorialCompleted(PawGuide.BasicId)&&GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints,"Tamamlama gerçek PlayerPrefs'e kaydedildi; eğitim ücretsiz");
            yield return new WaitForSecondsRealtime(.3f);Check(FindFirstObjectByType<Characters.WizardCatPresentation>().ActiveSparks>0,"Mevcut Wand Magic yıldızları gerçekten görünür");
            Check(!GameObject.Find("Status").GetComponent<TMP_Text>().enabled,"Kutlama altında durum yazısı üst üste görünmüyor");
            foreach(int h in new[]{1600,1950,2000})Capture(Output+"Celebration-"+h+".png",900,h,h==2000);
            for(int i=0;i<12;i++){Capture(Output+"CelebrationMotion-"+i.ToString("D2")+".png",450,800);yield return new WaitForSecondsRealtime(.08f);}
            yield return new WaitForSecondsRealtime(1.9f);Check(!Guide.Visible&&GameObject.Find("PawBubble")==null&&GameObject.Find("PawHighlight")==null,"Kutlama sonrasında yumuşak kapanış tamamlandı");Check(GameObject.Find("Status").GetComponent<TMP_Text>().enabled,"Eğitim kapanınca normal oyuncu bildirimi geri geliyor");Capture(Output+"Normal-1600.png",900,1600);
            Tap("Undo");Tap("Restart");Check(Model.Moves==0&&GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId)&&!Guide.Visible,"Undo/Restart öğrenilmiş eğitimi sıfırlamıyor");
            SessionState.SetString("Part11B.Expected",JsonUtility.ToJson(GameplayProgress.Current.Snapshot));
            Tap("Home");yield return null;yield return null;Tap("Play");yield return WaitGameplay();Check(!Guide.Visible,"Tamamlanan eğitim sahneye dönüşte açılmıyor");
            // Separate real test save: close halfway, then unload/reload and reopen disk data.
            Tap("Home");yield return null;yield return null;Bind(key+".interrupted");Tap("Play");yield return WaitGameplay();Tap("PawNext");move=Guide.Recommended;Tap("Bottle "+(move.Source+1));
            Check(Guide.State==PawGuide.Phase.Target,"Yarım bırakma için hedef aşaması");var bubble=GameObject.Find("PawBubble").GetComponent<CanvasGroup>();
            yield return new WaitForSecondsRealtime(.25f);Tap("PawClose");Check(!Guide.Visible&&!bubble.blocksRaycasts&&!GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId)&&Model.Moves==0,"Kapat anında girdiyi serbest bırakır; eksik eğitim kaydedilmez");
            Check(bubble.gameObject.activeSelf&&bubble.alpha>0,"Çıkış sert kesilmeden başlıyor");yield return new WaitForSecondsRealtime(.3f);Check(!bubble.gameObject.activeSelf,"Çıkış animasyonu tamamlanıyor");
            Tap("Home");yield return null;yield return null;Bind(key+".interrupted");Tap("Play");yield return WaitGameplay();Check(Guide.State==PawGuide.Phase.Welcome&&Model.Moves==0&&Selected<0,"Yarım eğitim sonraki girişte karşılama aşamasından güvenle başlar");
            Tap("PawNext");move=Guide.Recommended;Tap("Bottle "+(move.Source+1));Tap("Home");yield return null;yield return null;Bind(key+".interrupted");Tap("Play");yield return WaitGameplay();Check(Guide.State==PawGuide.Phase.Welcome,"Kapatmadan çıkış ve diskten yeniden giriş de baştan başlatır");
            Tap("PawClose");yield return new WaitForSecondsRealtime(.3f);var valid=PuzzleSolver.Solve(Model.Export()).Moves[0];Tap("Bottle "+(valid.Source+1));Tap("Bottle "+(valid.Target+1));yield return new WaitForSecondsRealtime(1.05f);Check(Model.Moves==1&&!Guide.Visible,"Eğitim kapalıyken normal oynanış çalışıyor");
            Tap("Home");yield return null;yield return null;Bind(key+".failed");Tap("Play");yield return WaitGameplay();
            var failed=new ProgressLedger(GameplayProgress.Current.Snapshot,_=>throw new IOException("Test kaydetme hatası"));typeof(PawGuide).GetField("ledger",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(Guide,failed);
            Tap("PawNext");move=Guide.Recommended;Tap("Bottle "+(move.Source+1));Tap("Bottle "+(move.Target+1));yield return new WaitForSecondsRealtime(1.05f);Check(Guide.State==PawGuide.Phase.Celebration&&!failed.TutorialCompleted(PawGuide.BasicId),"Kayıt hatası yanlış tamamlanma üretmiyor");yield return new WaitForSecondsRealtime(1.9f);Check(!Guide.Visible&&Model.Moves==1,"Kayıt hatası oyunu kilitlemiyor");
            Tap("Home");yield return null;yield return null;Bind(key+".missing");Tap("Play");yield return WaitGameplay();Tap("PawNext");var target=GameObject.Find("Bottle "+(Guide.Recommended.Target+1));target.SetActive(false);yield return null;yield return null;Check(!Guide.Visible,"Eksik hedefte eğitim güvenle kapanıyor");target.SetActive(true);
            Tap("PawCatTouch");Tap("PawHow");Check(Guide.State==PawGuide.Phase.General,"Mevcut Nasıl oynanır menüsü korundu");Check(Guide.HandleBack()&&!Guide.Visible,"Geri güvenli kapatıyor");
            Tap("Home");yield return null;yield return null;Bind(key);Check(GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId),"Tamamlanan asıl test profili korunuyor");
            SessionState.SetString("Part11B.Expected",JsonUtility.ToJson(GameplayProgress.Current.Snapshot));
        }
        IEnumerator Reload()
        {
            yield return null;yield return null;yield return new WaitForSecondsRealtime(.4f);
            Check(GameplayProgress.Current.TutorialCompleted(PawGuide.BasicId)&&!Guide.Visible,"Gerçek Play Mode çıkış/yeniden girişinde tamamlanan eğitim açılmıyor");
            Check(JsonUtility.ToJson(GameplayProgress.Current.Snapshot)==SessionState.GetString("Part11B.Expected",""),"Diskten yeniden açılan bütün oyuncu test verileri aynı");
            Check(Model.Moves==0,"Yeniden giriş normal bölüm başlangıcını koruyor");Capture(Output+"Completed-Reentry.png",900,1600);
            Tap("PawCatTouch");Check(Guide.State==PawGuide.Phase.Menu,"Tamamlanmış eğitimde kedi menüsü istek üzerine çalışıyor");Tap("PawHint");Check(!Guide.Visible&&FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Length==2,"PART11A metinsiz Hint korundu");
        }
        Rect Bounds(RectTransform r,RectTransform relative){var corners=new Vector3[4];r.GetWorldCorners(corners);var a=(Vector2)relative.InverseTransformPoint(corners[0]);var b=(Vector2)relative.InverseTransformPoint(corners[2]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
        void AuditLayout(RectTransform safe,int height)
        {
            var bubble=GameObject.Find("PawBubble");if(safe==null||bubble==null)return;var area=Bounds((RectTransform)bubble.transform,safe);
            Check(area.xMin>=safe.rect.xMin&&area.xMax<=safe.rect.xMax&&area.yMin>=safe.rect.yMin&&area.yMax<=safe.rect.yMax,"Balon Safe Area içinde: "+height);
            foreach(var bottle in FindObjectsByType<GameplayBottleView>(FindObjectsSortMode.None))Check(!area.Overlaps(Bounds(bottle.Rig,safe)),"Balon şişeyi kapatmıyor: "+height);
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
