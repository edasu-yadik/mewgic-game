#if UNITY_EDITOR
using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;using System.Reflection;using System.Text.RegularExpressions;
using UnityEngine;using UnityEngine.UI;using UnityEngine.SceneManagement;using UnityEditor;using TMPro;using Mewgic.Core;
namespace Mewgic
{
    [DefaultExecutionOrder(-10000)] public sealed class Part11AProbe:MonoBehaviour
    {
        const string Output="docs/ProductionReports/Part11A/";
        readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
        bool had;string saved;static Part11AProbe active;
        void Awake(){if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);had=PlayerPrefs.HasKey(GameplayProgress.StorageKey);saved=PlayerPrefs.GetString(GameplayProgress.StorageKey,"");GameplayProgress.UseTestProfile(new ProgressData{completedTutorials=new List<string>{Mewgic.UI.PawGuide.BasicId}});Application.logMessageReceived+=Log;}
        void Log(string m,string s,LogType t){if(t==LogType.Warning)warnings.Add(m);if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m);}
        void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
        Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(App);
        int Index(string name)=>(int)typeof(PrototypeApp).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(App);
        string Board=>string.Join("|",Model.Export().Select(b=>string.Join(",",b)));
        void Tap(string name){var go=GameObject.Find(name);Check(go!=null,"Buton: "+name);go.GetComponent<Button>().onClick.Invoke();}
        IEnumerator Start(){var run=Run();string failure=null;while(true){bool more=false;object next=null;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception e){failure=e.ToString();}if(failure!=null||!more)break;yield return next;}bool intact=had==PlayerPrefs.HasKey(GameplayProgress.StorageKey)&&saved==PlayerPrefs.GetString(GameplayProgress.StorageKey,"");Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();File.WriteAllText(Output+"TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&intact,failure,checks,errors,warnings,playerSaveIntact=intact},Newtonsoft.Json.Formatting.Indented));EditorApplication.isPlaying=false;}
        void Clean(string stage)
        {
            foreach(var text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy)) {
                Check(!Regex.IsMatch(text.text,@"İpucu\s*:?\s*\d+\s*→")&&!text.text.Contains("Altın kaynak")&&!text.text.Contains("mavi hedef")&&!text.text.Contains("Hedef şişeyi seç")&&!text.text.Contains("Önce kaynak"),stage+": teknik açıklama yok / "+text.name);
            }
            foreach(var view in FindObjectsByType<GameplayBottleView>(FindObjectsSortMode.None))Check(view.GetComponentsInChildren<TMP_Text>(true).Length==0,stage+": şişe numara etiketi oluşturulmuyor");
            Check(GameObject.Find("LevelValue")!=null&&GameObject.Find("Moves")!=null,stage+": bölüm ve hamle HUD korunuyor");
        }
        IEnumerator Run()
        {
            yield return null;yield return null;Clean("Bölüm açılışı");Capture(Output+"Gameplay_Normal.png",900,1600);
            int stock=GameplayProgress.Current.Remaining(BoosterKind.Hint);string before=Board;
            Tap("Hint");Clean("İpucu");int source=Index("selected"),target=Index("hintTarget");
            Check(source>=0&&target>=0&&source!=target&&new Puzzle(Model.Export()).Pour(source,target)>0,"İpucu kurallara uygun kaynak/hedef veriyor");
            Check(Model.Moves==0&&Board==before&&GameplayProgress.Current.Remaining(BoosterKind.Hint)==stock-1,"İpucu tek hak kullanır; şişe verileri değişmez");
            Check(GameObject.Find("Hint").transform.Find("CounterBadge/Charges").GetComponent<TMP_Text>().text==(stock-1).ToString(),"Güçlendirici sayacı gerçek hakkı gösteriyor");
            Check(FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Length==2&&FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Count(c=>c.destination)==1,"İki farklı metinsiz işaret: pati ve hedef oku");
            Check(GameObject.Find("Status").GetComponent<TMP_Text>().text=="","Başarılı ipucu durum metni boş");
            var a=GameObject.Find("Bottle "+(source+1)).GetComponentInChildren<Outline>();var b=GameObject.Find("Bottle "+(target+1)).GetComponentInChildren<Outline>();
            Check(a.effectColor.r>.9f&&a.effectColor.b<.3f&&b.effectColor.b>.9f&&b.effectColor.r<.2f,"Kaynak altın; hedef farklı mavi görsel vurgulu");
            float alpha=a.effectColor.a;yield return new WaitForSecondsRealtime(.4f);Check(Mathf.Abs(a.effectColor.a-alpha)>.005f,"Kaynak ışığı yumuşak hareket ediyor");
            foreach(int h in new[]{1600,1950,2000}){Clean("İpucu oran "+h);Capture(Output+"Gameplay_Hint-"+h+".png",900,h,h==2000);}
            ScreenCapture.CaptureScreenshot(Output+"Gameplay_Hint_GameView.png");yield return new WaitForEndOfFrame();yield return null;
            Tap("Hint");Check(GameplayProgress.Current.Remaining(BoosterKind.Hint)==stock-1,"Aynı ipucu tekrar ücretlendirilmedi");Clean("Tekrar ipucu");
            Tap("Restart");Clean("Restart");Check(FindObjectsByType<HintCueGraphic>(FindObjectsSortMode.None).Length==0,"Restart görsel ipucunu temizliyor");Tap("Bottle 1");Clean("Kaynak seçimi");Check(GameObject.Find("Status").GetComponent<TMP_Text>().text=="","Kaynak/hedef dahili yönlendirmesi yok");
            Tap("Bottle 2");Check(Model.Moves==0,"Geçersiz hamle modeli korudu");Clean("Geçersiz hamle");Check(GameObject.Find("Status").GetComponent<TMP_Text>().text=="Bu hamle yapılamıyor.","Gerekli oyuncu uyarısı korunuyor");
            Tap("Bottle 3");yield return new WaitForSecondsRealtime(1.1f);Check(Model.Moves==1,"İlk geçerli aktarım");Clean("İlk hamle");
            Tap("Undo");Check(Model.Moves==0,"Undo");Clean("Undo");Tap("Restart");Clean("Tekrar Restart");
            Tap("PawCatTouch");Tap("PawHint");Clean("Pati Yardımı Hint");Check(Index("hintTarget")>=0&&!FindFirstObjectByType<Mewgic.UI.PawGuide>().Visible,"Pati Yardımı aynı görsel ipucunu açıyor");
            Tap("Restart");Tap("ExtraBottle");yield return null;Clean("UI yeniden üretimi / Ek Şişe");Tap("Shuffle");Clean("Karıştır");Tap("Restart");yield return null;Clean("UI yeniden üretimi / Restart");
            foreach(var m in PuzzleSolver.Solve(Model.Export()).Moves){Tap("Bottle "+(m.Source+1));Tap("Bottle "+(m.Target+1));yield return new WaitForSecondsRealtime(1.1f);Clean("Geçerli hamle");}
            Check(Model.Won&&FindFirstObjectByType<GameplayEndScreen>().IsShowing,"Bölüm kazanma ve ödüller");Clean("Kazanma");Capture(Output+"Gameplay_Victory.png",900,1600);
            Tap("ResultHome");yield return null;yield return null;Check(SceneManager.GetActiveScene().name=="Home","Ana menüye dönüş");Tap("Play");yield return new WaitForSecondsRealtime(.6f);yield return null;Clean("Tekrar oynama");
            Capture(Output+"Gameplay_Reentry.png",900,1600);
        }
        void Capture(string filename,int width,int height,bool inset=false)
        {
            var canvas=FindFirstObjectByType<Canvas>();var camera=Camera.main;var p=FindFirstObjectByType<HomeCharacterPresenter>();var safe=p!=null?(RectTransform)p.transform.Find("UI"):(GameObject.Find("Safe area")??GameObject.Find("MapSafeArea"))?.GetComponent<RectTransform>();
            var mode=canvas.renderMode;var oldCam=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;Vector2 min=safe!=null?safe.anchorMin:Vector2.zero,max=safe!=null?safe.anchorMax:Vector2.one;
            var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;if(inset&&safe!=null){safe.anchorMin=new Vector2(.035f,.04f);safe.anchorMax=new Vector2(.965f,.955f);}
                Canvas.ForceUpdateCanvases();var gameplay=FindFirstObjectByType<GameplayPresentation>();if(gameplay!=null)gameplay.Layout();var pause=GameObject.Find("PauseSafeArea");if(pause!=null&&safe!=null){var pr=(RectTransform)pause.transform;pr.anchorMin=safe.anchorMin;pr.anchorMax=safe.anchorMax;}if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}var map=FindFirstObjectByType<Mewgic.Map.MapWorld>();if(map!=null)map.Layout();var daily=FindFirstObjectByType<Mewgic.Daily.DailyActivitiesPanel>();if(daily!=null&&daily.IsOpen)daily.Layout();var hub=FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>();if(hub!=null&&hub.IsOpen)hub.Layout();Canvas.ForceUpdateCanvases();
                foreach(var text in FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None))text.ForceMeshUpdate();FindFirstObjectByType<Mewgic.UI.PawGuide>()?.SendMessage("LateUpdate");foreach(var text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){text.ForceMeshUpdate();if(text.name=="PawMessage")Check(!text.isTextOverflowing,"Pati balonunda taşma yok: "+height);}camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(filename,tex.EncodeToPNG());
            }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=distance;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}}
        }
    }
}
#endif
