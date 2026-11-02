#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
using Mewgic.Core;
using Mewgic.Map;
using Mewgic.Localization;
using Mewgic.UI;

namespace Mewgic
{
 [DefaultExecutionOrder(-20000)]
 public sealed class Part14AProbe : MonoBehaviour
 {
  const string Output="docs/ProductionReports/Part14A/";
  public bool preview;
  string key,archiveRoot;
  readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
  readonly Dictionary<string,string> player=new Dictionary<string,string>();
  static Part14AProbe active;
  MapWorld Map=>FindFirstObjectByType<MapWorld>();
  PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
  Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(App);
  public static void Begin(bool view=false)
  {
   if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı");
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
   var qa=new GameObject("PART14A Güvenli QA").AddComponent<Part14AProbe>();qa.preview=view;
   EditorApplication.isPlaying=true;
  }
  void Awake()
  {
   if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);
   foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}
   Application.runInBackground=true;key="Mewgic.Tests.Part14A."+Guid.NewGuid().ToString("N");archiveRoot=Path.Combine(Path.GetTempPath(),"Mewgic-Part14A-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(archiveRoot);
   GameplayLevelCatalog.TestArchive=new FileLevelArchive(archiveRoot);Bind(Part12BProbe.Data(99));GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;
  }
  void Bind(ProgressData d=null)
  {
   GameplayProgress.UseTestProfile();
   if(d!=null){d.presentation.manualLocale="tr";PlayerPrefs.SetString(key,JsonUtility.ToJson(d));PlayerPrefs.DeleteKey(key+".backup");PlayerPrefs.Save();}
   typeof(GameplayProgress).GetField("current",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,GameplayProgress.Open(key));
  }
  void Log(string m,string stack,LogType type){if(type==LogType.Warning)warnings.Add(m);if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(m);}
  void Check(bool ok,string m){if(!ok)throw new Exception(m);checks.Add(m);}
  void Tap(string name){var go=GameObject.Find(name);Check(go!=null&&go.GetComponent<Button>()!=null&&go.GetComponent<Button>().interactable,"Gerçek dokunulabilir düğme: "+name);go.GetComponent<Button>().onClick.Invoke();}
  IEnumerator Ready(string scene,int id=0)
  {
   float until=Time.unscaledTime+30;
   while(Time.unscaledTime<until&&(!L.Ready||SceneManager.GetActiveScene().name!=scene||scene=="LevelMap"&&(Map==null||!Map.Mounted)||scene=="Gameplay"&&(App==null||GameplayLevelCatalog.SelectedId!=id)||scene=="Home"&&GameObject.Find("Play")==null))yield return null;
   yield return null;yield return null;
   Check(L.Ready&&SceneManager.GetActiveScene().name==scene,"Hazır gerçek sahne "+scene+"/"+id);
   if(scene=="LevelMap")Check(Map!=null&&Map.Mounted,"LevelMap mount");
   if(scene=="Gameplay"){Check(App!=null&&GameplayLevelCatalog.SelectedId==id,"Gameplay kimliği "+id);FindFirstObjectByType<PawGuide>()?.Close();}
  }
  IEnumerator Load(int complete){Bind(Part12BProbe.Data(complete));SceneManager.LoadScene("LevelMap");yield return Ready("LevelMap");}
  IEnumerator Solve()
  {
   var pin=GameplayLevelCatalog.Current().Generated;pin.Validate();
   foreach(var m in pin.solution){Tap("Bottle "+(m.source+1));Tap("Bottle "+(m.target+1));yield return new WaitForSecondsRealtime(1.05f);}
   Check(Model.Won&&GameplayProgress.Current.Completed(pin.level),"Gerçek kazanma/yıldız kaydı "+pin.level);
  }
  IEnumerator Start()
  {
   Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size").Invoke(null,new object[]{1600});var window=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));window.Show();window.maximized=true;window.Focus();yield return null;yield return null;

   if(preview){yield return Ready("LevelMap");L.Select("tr");yield return Focus(0);yield break;}
   var stack=new Stack<IEnumerator>();stack.Push(Run());string failure=null;
   while(stack.Count>0){bool more=false;object next=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){failure=e.ToString();break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested&&!(next is CustomYieldInstruction)){stack.Push(nested);continue;}yield return next;}
   bool intact=player.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.GetString(p.Key)==p.Value);
   File.WriteAllText(Output+"UNITY_TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&warnings.Count==0&&intact,failure,count=checks.Count,checks,errors,warnings,playerSaveAndBackupIntact=intact},Newtonsoft.Json.Formatting.Indented));
   EditorApplication.isPlaying=false;
  }
  int region;
  IEnumerator Focus(int r){region=r;Map.FocusRegion(r,false);float until=Time.unscaledTime+20;while(Time.unscaledTime<until&&FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).Any(x=>x.BoundKey!=null&&!x.Loaded))yield return null;yield return new WaitForSecondsRealtime(1.1f);Check(!FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).Any(x=>x.BoundKey!=null&&!x.Loaded),"Görünür katmanlar hazır "+r);}
  void Hit(MapPoint p){var ev=UnityEngine.EventSystems.EventSystem.current;var d=new UnityEngine.EventSystems.PointerEventData(ev){position=RectTransformUtility.WorldToScreenPoint(Map.Canvas.worldCamera,p.transform.position)};var h=new List<UnityEngine.EventSystems.RaycastResult>();ev.RaycastAll(d,h);Check(h.Count>0&&h[0].gameObject.GetComponentInParent<MapPoint>()==p,"Gerçek raycast "+region+"/"+p.kind+"/"+p.slotIndex);}
  IEnumerator Run(){
   yield return Ready("LevelMap");L.Select("tr");Tap("MapHome");yield return Ready("Home");yield return new WaitForSecondsRealtime(.5f);
   Check(GameObject.Find("CollectionButton")==null&&GameObject.Find("DailyGiftButton")==null,"Home sabit koleksiyon/hediye yok");Check(GameObject.Find("ArrivalOpen")!=null,"Ilk uygun giriste otomatik sandik");Capture("DailyReward",900,1600,false,false);long before=GameplayProgress.Current.Presentation.coins;int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);Tap("ArrivalOpen");Check(GameplayProgress.Current.Presentation.coins==before,"Animasyon bitmeden odeme yok");yield return new WaitForSecondsRealtime(1.3f);Check(GameplayProgress.Current.Presentation.coins==before+100&&GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints+1,"Sandik sonrasi coin+hint atomik");Tap("ArrivalContinue");yield return new WaitForSecondsRealtime(2);Capture("Home",900,1600,false,false);
   Tap("NavMap");yield return Ready("LevelMap");yield return Focus(1);Capture("World",900,1600);var entry=Map.Points.Single(x=>x.kind==MapPointKind.LevelGroup&&x.regionIndex==1&&x.gameObject.activeInHierarchy);entry.GetComponent<Button>().onClick.Invoke();yield return null;
   Check(Map.SelectedChapter==9,"Aktif100 cevresi91-100 sayfasi");for(int page=4;page>=0;page--){var targets=Map.Points.Where(x=>x.kind==MapPointKind.LevelSlot&&x.gameObject.activeInHierarchy).ToArray();Check(targets.Length==10&&targets.All(x=>Map.Progression.LevelId(x.regionIndex,x.slotIndex)>=51+page*10&&Map.Progression.LevelId(x.regionIndex,x.slotIndex)<=60+page*10),"Yalniz ada icinde10 tekil node "+page);foreach(var p in targets)Hit(p);if(page>0){Tap("DetailPrevious");yield return null;}}
   for(int page=0;page<4;page++){Tap("DetailNext");yield return null;}foreach(int h in new[]{1600,1950,2000})Capture("IslandDetail_"+h,900,h,h==2000);Capture("IslandDetail_360",360,640);foreach(string code in L.Codes){L.Select(code,false);yield return null;Audit(Map,1600);}L.Select("tr",false);Tap("ChapterClose");
   yield return Focus(2);Map.OpenChapter(2,0);yield return null;var closed=Map.Points.First(x=>x.kind==MapPointKind.LevelSlot&&x.slotIndex==0);closed.GetComponent<Button>().onClick.Invoke();yield return null;Check(Map.InformationVisible&&UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="LevelMap","101 kilitli secim oyuna gecmez");Tap("MapInfoClose");Tap("ChapterClose");
   yield return Focus(1);Map.OpenChapter(1,4);yield return null;Map.Points.First(x=>x.kind==MapPointKind.LevelSlot&&x.slotIndex==49).GetComponent<Button>().onClick.Invoke();yield return Ready("Gameplay",100);long coins=GameplayProgress.Current.Presentation.coins;yield return Solve();Check(GameplayProgress.Current.Presentation.coins==coins+GameplayProgress.Current.LastCoinReward&&GameplayProgress.Current.LastCoinReward>0,"Gercek kazanma coin odulu");yield return new WaitForSecondsRealtime(1.5f);foreach(int h in new[]{1600,1950,2000})Capture("LevelComplete_"+h,900,h,false,false);Tap("ResultHome");yield return Ready("LevelMap");Check(Map.Progression.Stars(100)>0&&Map.Progression.CanEnter(101),"Odul ve ilerleme birlikte korundu");Tap("MapHome");yield return Ready("Home");Check(GameObject.Find("ArrivalOpen")==null,"Home donusunde ikinci popup yok");
  }
  void Audit(MapWorld map,int height)
  {
   foreach(var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))if(t.enabled&&t.gameObject.activeInHierarchy){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Taşma yok "+height+"/"+t.name);}
   if(map==null)return;
   var targets=map.Points.Where(p=>p.regionIndex==region&&p.gameObject.activeInHierarchy&&p.kind!=MapPointKind.LevelSlot).ToArray();
   for(int i=0;i<targets.Length;i++)for(int j=i+1;j<targets.Length;j++) {
    var ca=new Vector3[4];var cb=new Vector3[4];((RectTransform)targets[i].transform).GetWorldCorners(ca);((RectTransform)targets[j].transform).GetWorldCorners(cb);
    bool overlap=Mathf.Min(ca[2].x,cb[2].x)-Mathf.Max(ca[0].x,cb[0].x)>.1f&&Mathf.Min(ca[2].y,cb[2].y)-Mathf.Max(ca[0].y,cb[0].y)>.1f;
    Check(!overlap,"Ada grup/sandık dokunma alanları çakışmaz "+height+"/"+i+"/"+j);
   }
   foreach(string name in new[]{"MapHeader","RegionNavigation","MapNavigation"}){
    var r=(RectTransform)map.SafeArea.Find(name);var c=new Vector3[4];r.GetWorldCorners(c);var a=map.SafeArea.InverseTransformPoint(c[0]);var b=map.SafeArea.InverseTransformPoint(c[2]);Check(a.x>=map.SafeArea.rect.xMin-.1f&&a.y>=map.SafeArea.rect.yMin-.1f&&b.x<=map.SafeArea.rect.xMax+.1f&&b.y<=map.SafeArea.rect.yMax+.1f,"Safe Area "+name+"/"+height);
   }
   foreach(var p in map.Points.Where(p=>p.regionIndex==region&&p.gameObject.activeInHierarchy)){
    var r=(RectTransform)p.transform;float size=r.rect.width*map.Canvas.scaleFactor*r.lossyScale.x/map.Canvas.transform.lossyScale.x;
    Check(size>=43.9f,"Dokunma hedefi en az44 ekran pikseli "+height+"/"+p.kind);
   }
   foreach(var point in map.Points.Where(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy)){var r=(RectTransform)point.transform;float size=Mathf.Min(r.rect.width,r.rect.height)*map.Canvas.scaleFactor*r.lossyScale.x/map.Canvas.transform.lossyScale.x;Check(size>=43.9f,"Kart dokunma alani44 piksel "+height);}
   Check(L.Missing.Count==0,"Eksik yerelleştirme yok");
  }
  public void Capture(string name,int width,int height,bool inset=false,bool focusIsland=true)
  {
   var canvas=Map!=null?Map.Canvas:FindFirstObjectByType<Canvas>();var camera=Camera.main;var map=Map;var safe=map!=null?map.SafeArea:null;
   var mode=canvas.renderMode;var oldCam=canvas.worldCamera;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;float dist=canvas.planeDistance;
   Vector2 min=safe!=null?safe.anchorMin:Vector2.zero,max=safe!=null?safe.anchorMax:Vector2.one;float scroll=map!=null?map.Scroll.verticalNormalizedPosition:0;
   var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
    if(safe!=null&&inset){safe.anchorMin=new Vector2(.035f,.04f);safe.anchorMax=new Vector2(.965f,.955f);}Canvas.ForceUpdateCanvases();
    if(map!=null){map.Layout();if(focusIsland)map.FocusRegion(region,false);}FindFirstObjectByType<GameplayPresentation>()?.Layout();Canvas.ForceUpdateCanvases();Audit(map,height);camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Output+name+".png",tex.EncodeToPNG());
   }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=dist;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(map!=null){map.Layout();map.Scroll.verticalNormalizedPosition=scroll;}}
  }
  void OnDestroy(){if(active!=this)return;Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();GameplayLevelCatalog.TestArchive=null;active=null;}
 }
}
#endif
