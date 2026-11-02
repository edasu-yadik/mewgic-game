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
 public sealed class Part13HProbe : MonoBehaviour
 {
  const string Output="docs/ProductionReports/Part13H/";
  public bool preview;
  string key,archiveRoot;
  readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
  readonly Dictionary<string,string> player=new Dictionary<string,string>();
  static Part13HProbe active;readonly List<double> loadTimes=new List<double>();
  MapWorld Map=>FindFirstObjectByType<MapWorld>();
  PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
  Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(App);
  public static void Begin(bool view=false)
  {
   if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı");
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
   new GameObject("PART13H Güvenli QA").AddComponent<Part13HProbe>().preview=view;
   EditorApplication.isPlaying=true;
  }
  void Awake()
  {
   if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);
   foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}
   Application.runInBackground=true;key="Mewgic.Tests.Part13H."+Guid.NewGuid().ToString("N");archiveRoot=Path.Combine(Path.GetTempPath(),"Mewgic-Part13H-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(archiveRoot);
   GameplayLevelCatalog.TestArchive=new FileLevelArchive(archiveRoot);Bind(Part12BProbe.Data(preview?500:499));GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;
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
  IEnumerator Library()
  {
   var loadWatch=System.Diagnostics.Stopwatch.StartNew();Map.FocusRegion(10,false);float until=Time.unscaledTime+20;
   while(Time.unscaledTime<until&&FindFirstObjectByType<LibraryIslandPresentation>()==null)yield return null;
   Check(FindFirstObjectByType<LibraryIslandPresentation>()!=null,"Gerçek Addressables library prefabı yüklendi");loadWatch.Stop();loadTimes.Add(loadWatch.Elapsed.TotalMilliseconds);
   yield return new WaitForSecondsRealtime(1);
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
   if(preview){yield return Ready("LevelMap");L.Select("tr");yield return Library();yield break;}
   var stack=new Stack<IEnumerator>();stack.Push(Run());string failure=null;
   while(stack.Count>0){bool more=false;object next=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){failure=e.ToString();break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested&&!(next is CustomYieldInstruction)){stack.Push(nested);continue;}yield return next;}
   bool intact=player.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.GetString(p.Key)==p.Value);
   File.WriteAllText(Output+"UNITY_TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&warnings.Count==0&&intact,failure,count=checks.Count,checks,errors,warnings,playerSaveAndBackupIntact=intact},Newtonsoft.Json.Formatting.Indented));
   File.WriteAllText(Output+"LOAD_PERFORMANCE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{environment="Windows Editor AssetDatabase / warm session",loadToInstanceMs=loadTimes.ToArray(),note="Giriş animasyonunun1sn QA beklemesi dahil değil; telefon soğuk açılış ölçümü değildir."},Newtonsoft.Json.Formatting.Indented));
   EditorApplication.isPlaying=false;
  }
  IEnumerator Run()
  {
   yield return Ready("LevelMap");L.Select("tr");
   var def=MapProgressionCatalog.Definition();Check(def.regions.Length==12&&def.regions[10].artReady&&def.regions.Skip(11).All(r=>!r.artReady),"Yalnız on birinci ada sanatı açıldı");
   var old=Map.terrain.Select(s=>s.GetInstanceID()).ToArray();
   yield return Library();Check(!Map.Progression.RegionOpen(10)&&!Map.Progression.CanEnter(501),"500 kazanılmadan library501 kilitli");Capture("Library_Locked",900,1600);
   Map.OpenChapter(10,0);Map.ShowInformation(Map.Points.First(p=>p.kind==MapPointKind.LevelSlot&&p.slotIndex==0));Check(Map.InformationVisible&&SceneManager.GetActiveScene().name=="LevelMap"&&Directory.GetFiles(archiveRoot,"level-*.json").Length==0,"Kilitli501 üretici başlatmaz");Tap("MapInfoClose");Tap("ChapterClose");
   Map.EnterLevel(500);yield return Ready("Gameplay",500);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",501);Check(GameplayLevelCatalog.Current().Generated.level==501&&GameplayLevelCatalog.Current().Generated.target==LevelDifficulty.Easy,"Gerçek500→501, yumuşak ada girişi");
   var start=Model.Export();var move=GameplayLevelCatalog.Current().Generated.solution[0];Tap("Bottle "+(move.source+1));Tap("Bottle "+(move.target+1));yield return new WaitForSecondsRealtime(1.05f);Tap("Undo");Check(Model.Moves==0&&LevelGenerator.Exact(start,Model.Export()),"501 Undo aynı pin");Tap("Restart");Check(LevelGenerator.Exact(start,Model.Export()),"501 Restart aynı düzen");
   string signature=GameplayLevelCatalog.Current().Generated.signature;yield return Solve();Tap("ResultHome");yield return Ready("LevelMap");yield return Library();Check(Map.Progression.Stars(501)>0&&Map.Progression.CanEnter(502)&&!Map.Progression.CanEnter(503),"501 yıldızı/502 açık/503 kilitli");
   Map.OpenChapter(10,0);yield return null;Check(Map.Points.Count(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy)==10,"501–510 seçicisinde yalnız10 gerçek düğüm");Capture("Library_Chapter",900,1600);Tap("ChapterClose");
   for(int i=0;i<5;i++){Map.OpenChapter(10,i);Check(Map.Points.Where(p=>p.kind==MapPointKind.LevelSlot).All(p=>Map.Progression.LevelId(p.regionIndex,p.slotIndex)>=501&&Map.Progression.LevelId(p.regionIndex,p.slotIndex)<=550),"Gerçek50 bölüm grup bağlantısı "+i);Tap("ChapterClose");}
   yield return new WaitForSecondsRealtime(1);var data=GameplayProgress.Current.Snapshot;int stars=Map.Progression.Stars(501);Bind();SceneManager.LoadScene("LevelMap");yield return Ready("LevelMap");yield return Library();Check(Map.Progression.Stars(501)==stars&&GameplayLevelCatalog.Load(501).Generated.signature==signature,"Diskten yıldız ve değişmez seed/pin yeniden açıldı");
   var visual=FindFirstObjectByType<LibraryIslandPresentation>();float phase=visual.LightPhase;Vector2 book=visual.books[0].anchoredPosition;float angle=visual.books[0].localEulerAngles.z;Color ring=visual.rings[0].color;Capture("Library_Motion_A",900,1600);yield return new WaitForSecondsRealtime(1.1f);
   Check(visual.LightPhase>phase&&visual.books[0].anchoredPosition!=book&&visual.books[0].localEulerAngles.z!=angle&&visual.rings[0].color!=ring,"Akademi ışık saati, kitabın süzülme/eğimi ve halka alpha hareketli");
   Check(visual.books.Length==3&&visual.rings.Length==2,"Yalnız3 kitap ve2 sihir halkası");Check(visual.GetComponentsInChildren<Rigidbody>(true).Length==0&&visual.GetComponentsInChildren<Rigidbody2D>(true).Length==0,"Ada animasyonu fizik simülasyonu kullanmaz");Check(visual.EntranceProgress==1,"Hafif açılış tamamlanır");Capture("Library_Motion_B",900,1600);
   foreach(int h in new[]{1600,1950,2000}){Capture("Library_Map-"+h,900,h,h==2000);Map.OpenChapter(10,4);Capture("Library_Chapter541-"+h,900,h,h==2000);Tap("ChapterClose");}
   CaptureIsland();
   yield return Performance();
   int[] ids=Map.Points.Select(p=>p.GetInstanceID()).ToArray();int peakHandles=0,peakInstances=0;var rebindWatch=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<120;i++){Map.FocusRegion(i%2==0?0:10,false);peakHandles=Mathf.Max(peakHandles,IslandArtSlot.LiveHandles);peakInstances=Mathf.Max(peakInstances,IslandArtSlot.LiveInstances);Check(IslandArtSlot.LiveHandles<=5&&IslandArtSlot.LiveInstances<=5,"Hızlı kaydırma handle sınırı "+i);}rebindWatch.Stop();File.WriteAllText(Output+"REBIND_PERFORMANCE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{scrollRebinds=120,totalEditorMs=rebindWatch.Elapsed.TotalMilliseconds,meanEditorMs=rebindWatch.Elapsed.TotalMilliseconds/120,handleLimit=5,peakHandles,peakInstances,pooledPoints=Map.Points.Count}));yield return Library();Check(ids.SequenceEqual(Map.Points.Select(p=>p.GetInstanceID()))&&Map.PooledIslands==5&&Map.Points.Count==40,"Aynı40 düğüm/5 ada havuzu");
   Map.FocusRegion(0,false);yield return null;yield return null;Check(IslandArtSlot.LiveHandles==0&&IslandArtSlot.LiveInstances==0&&FindFirstObjectByType<LibraryIslandPresentation>()==null,"Görünmeyen library instance ve handle bırakıldı");Check(old.SequenceEqual(Map.terrain.Select(s=>s.GetInstanceID())),"İlk dört sprite referansı aynı");
   yield return Load(549);Map.EnterLevel(550);yield return Ready("Gameplay",550);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",551);Check(GameplayLevelCatalog.Current().Generated.level==551&&GameplayLevelCatalog.Current().Generated.target==LevelDifficulty.Easy,"Gerçek550→551, sonraki ada davranışı aynı");Capture("Gameplay_551",900,1600);Tap("Home");yield return Ready("LevelMap");Check(Map.Progression.CurrentId==551&&!Map.Progression.CanEnter(552),"551 açık552 kilitli");yield return Library();Check(Map.Progression.Completed(10)==50&&Map.Progression.ChestReady(10),"50/50 gerçek tamamlanma sandığı");
   var chest=Map.Points.First(p=>p.kind==MapPointKind.Chest&&p.regionIndex==10&&p.gameObject.activeInHierarchy);Map.ShowInformation(chest);int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);Tap("ClaimMapChest");Check(GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints+1&&Map.Progression.ChestClaimed(10),"Library sandığı tek atomik ödül");bool granted;Check(Map.Progression.ClaimChest(10,out granted)&&!granted,"Ödül tekrarı engellendi");Tap("MapInfoClose");Bind();Check(MapProgressionCatalog.Open(GameplayProgress.Current).ChestClaimed(10),"Sandık kaydı yeniden açılışta korunur");
   yield return QuestsAndNavigation();
  }
  IEnumerator QuestsAndNavigation()
  {
   var before=GameplayProgress.Current.Snapshot;Tap("MapHome");yield return Ready("Home");
   Tap("MissionsButton");yield return null;var daily=FindFirstObjectByType<Mewgic.Daily.DailyActivitiesPanel>();Check(daily!=null&&daily.IsOpen&&!daily.GiftVisible,"Gerçek günlük görevler paneli açılır");Capture("Daily_Missions",900,1600);Tap("DailyClose");
   Tap("NavEvents");yield return null;var hub=FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>();Check(hub!=null&&hub.IsOpen,"Gerçek etkinlik merkezi açılır");Capture("Events_Hub",900,1600);Tap("RewardHubClose");
   var after=GameplayProgress.Current.Snapshot;Check(before.daily.progress.SequenceEqual(after.daily.progress)&&before.daily.claimed.SequenceEqual(after.daily.claimed)&&before.daily.wins.SequenceEqual(after.daily.wins),"Görev sayaç/makbuzları harita→Home gezinmesinde aynı");Check(before.hub.weekWins==after.hub.weekWins&&before.hub.seasonWins==after.hub.seasonWins&&before.hub.weekReceipts.SequenceEqual(after.hub.weekReceipts),"Haftalık/sezon görevi ilerlemesi korunur");
   Tap("NavMap");yield return Ready("LevelMap");yield return Library();Check(Map.Progression.ChestClaimed(10)&&Map.Progression.Stars(550)>0,"Home/görev/etkinlik→Library dönüşünde yıldız/sandık korunur");
  }
  IEnumerator Performance()
  {
   var samples=new List<double>();for(int i=0;i<180;i++){yield return null;samples.Add(Time.unscaledDeltaTime*1000);}
   samples.Sort();var p=FindFirstObjectByType<LibraryIslandPresentation>();
   File.WriteAllText(Output+"FRAME_PERFORMANCE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{environment="Windows Unity Editor / Android import target / AssetDatabase Play Mode",frames=samples.Count,medianFrameMs=samples[samples.Count/2],p95FrameMs=samples[(int)(samples.Count*.95)],maxFrameMs=samples.Last(),liveHandles=IslandArtSlot.LiveHandles,liveInstances=IslandArtSlot.LiveInstances,renderedImages=p.GetComponentsInChildren<Image>().Length,drawCalls=UnityEditor.UnityStats.drawCalls,note="Editor kare süreleri; telefon FPS/GPU/termal ölçümü değildir."},Newtonsoft.Json.Formatting.Indented));
   Check(IslandArtSlot.LiveHandles<=5&&IslandArtSlot.LiveInstances<=5,"180 gerçek kare boyunca görünür sanat sınırı");
  }
  void Audit(MapWorld map,int height)
  {
   foreach(var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))if(t.enabled&&t.gameObject.activeInHierarchy){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Taşma yok "+height+"/"+t.name);}
   if(map==null)return;
   var targets=map.Points.Where(p=>p.regionIndex==10&&p.gameObject.activeInHierarchy&&p.kind!=MapPointKind.LevelSlot).ToArray();
   for(int i=0;i<targets.Length;i++)for(int j=i+1;j<targets.Length;j++) {
    var ca=new Vector3[4];var cb=new Vector3[4];((RectTransform)targets[i].transform).GetWorldCorners(ca);((RectTransform)targets[j].transform).GetWorldCorners(cb);
    bool overlap=Mathf.Min(ca[2].x,cb[2].x)-Mathf.Max(ca[0].x,cb[0].x)>.1f&&Mathf.Min(ca[2].y,cb[2].y)-Mathf.Max(ca[0].y,cb[0].y)>.1f;
    Check(!overlap,"Library grup/sandık dokunma alanları çakışmaz "+height+"/"+i+"/"+j);
   }
   foreach(string name in new[]{"MapHeader","RegionNavigation","MapNavigation"}){
    var r=(RectTransform)map.SafeArea.Find(name);var c=new Vector3[4];r.GetWorldCorners(c);var a=map.SafeArea.InverseTransformPoint(c[0]);var b=map.SafeArea.InverseTransformPoint(c[2]);Check(a.x>=map.SafeArea.rect.xMin-.1f&&a.y>=map.SafeArea.rect.yMin-.1f&&b.x<=map.SafeArea.rect.xMax+.1f&&b.y<=map.SafeArea.rect.yMax+.1f,"Safe Area "+name+"/"+height);
   }
   foreach(var p in map.Points.Where(p=>p.regionIndex==10&&p.gameObject.activeInHierarchy)){
    var r=(RectTransform)p.transform;float size=r.rect.width*map.Canvas.scaleFactor*r.lossyScale.x/map.Canvas.transform.lossyScale.x;
    Check(size>=43.9f,"Dokunma hedefi en az44 ekran pikseli "+height+"/"+p.kind);
   }
   Check(L.Missing.Count==0,"Eksik yerelleştirme yok");
  }
  public void Capture(string name,int width,int height,bool inset=false)
  {
   var canvas=Map!=null?Map.Canvas:FindFirstObjectByType<Canvas>();var camera=Camera.main;var map=Map;var safe=map!=null?map.SafeArea:null;
   var mode=canvas.renderMode;var oldCam=canvas.worldCamera;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;float dist=canvas.planeDistance;
   Vector2 min=safe!=null?safe.anchorMin:Vector2.zero,max=safe!=null?safe.anchorMax:Vector2.one;float scroll=map!=null?map.Scroll.verticalNormalizedPosition:0;
   var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
    if(safe!=null&&inset){safe.anchorMin=new Vector2(.035f,.04f);safe.anchorMax=new Vector2(.965f,.955f);}Canvas.ForceUpdateCanvases();
    if(map!=null){map.Layout();map.FocusRegion(10,false);}FindFirstObjectByType<GameplayPresentation>()?.Layout();Canvas.ForceUpdateCanvases();Audit(map,height);camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Output+name+".png",tex.EncodeToPNG());
   }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=dist;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(map!=null){map.Layout();map.Scroll.verticalNormalizedPosition=scroll;}}
  }
  public void CaptureIsland()
  {
   var camera=Camera.main;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var map=Map;var src=FindFirstObjectByType<LibraryIslandPresentation>();
   var go=new GameObject("LibraryLargePreview",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   var rt=new RenderTexture(1200,1400,24);var tex=new Texture2D(1200,1400,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=rt;map.Canvas.gameObject.SetActive(false);Canvas.ForceUpdateCanvases();
    var bg=new GameObject("PreviewSky",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);var br=(RectTransform)bg.transform;br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=br.offsetMax=Vector2.zero;bg.GetComponent<Image>().sprite=map.sky;bg.GetComponent<Image>().color=new Color(.5f,.5f,.65f,1);
    var island=Instantiate(src.gameObject,go.transform,false);var r=(RectTransform)island.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(640,640);r.anchoredPosition=Vector2.zero;island.GetComponent<LibraryIslandPresentation>().Animate=false;island.GetComponent<CanvasGroup>().alpha=1;island.transform.localScale=Vector3.one*(1160f/640f);
    Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,1400),0,0);tex.Apply();File.WriteAllBytes(Output+"Library_LargePreview.png",tex.EncodeToPNG());
   }finally{map.Canvas.gameObject.SetActive(true);camera.targetTexture=oldTarget;RenderTexture.active=oldActive;DestroyImmediate(go);DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();map.Layout();}
  }
  void OnDestroy(){if(active!=this)return;Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();GameplayLevelCatalog.TestArchive=null;active=null;}
 }
}
#endif
