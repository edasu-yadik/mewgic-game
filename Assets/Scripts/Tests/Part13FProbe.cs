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
 public sealed class Part13FProbe : MonoBehaviour
 {
  const string Output="docs/ProductionReports/Part13F/";
  public bool preview;
  string key,archiveRoot;
  readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
  readonly Dictionary<string,string> player=new Dictionary<string,string>();
  static Part13FProbe active;readonly List<double> loadTimes=new List<double>();
  MapWorld Map=>FindFirstObjectByType<MapWorld>();
  PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
  Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(App);
  public static void Begin(bool view=false)
  {
   if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı");
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
   new GameObject("PART13F Güvenli QA").AddComponent<Part13FProbe>().preview=view;
   EditorApplication.isPlaying=true;
  }
  void Awake()
  {
   if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);
   foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}
   Application.runInBackground=true;key="Mewgic.Tests.Part13F."+Guid.NewGuid().ToString("N");archiveRoot=Path.Combine(Path.GetTempPath(),"Mewgic-Part13F-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(archiveRoot);
   GameplayLevelCatalog.TestArchive=new FileLevelArchive(archiveRoot);Bind(Part12BProbe.Data(preview?400:399));GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;
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
  IEnumerator Clockwork()
  {
   var loadWatch=System.Diagnostics.Stopwatch.StartNew();Map.FocusRegion(8,false);float until=Time.unscaledTime+20;
   while(Time.unscaledTime<until&&FindFirstObjectByType<ClockworkIslandPresentation>()==null)yield return null;
   Check(FindFirstObjectByType<ClockworkIslandPresentation>()!=null,"Gerçek Addressables clockwork prefabı yüklendi");loadWatch.Stop();loadTimes.Add(loadWatch.Elapsed.TotalMilliseconds);
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
   if(preview){yield return Ready("LevelMap");L.Select("tr");yield return Clockwork();yield break;}
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
   var def=MapProgressionCatalog.Definition();Check(def.regions.Length==12&&def.regions[8].artReady&&def.regions.Skip(9).All(r=>!r.artReady),"Yalnız dokuzuncu ada sanatı açıldı");
   var old=Map.terrain.Select(s=>s.GetInstanceID()).ToArray();
   yield return Clockwork();Check(!Map.Progression.RegionOpen(8)&&!Map.Progression.CanEnter(401),"400 kazanılmadan clockwork401 kilitli");Capture("Clockwork_Locked",900,1600);
   Map.OpenChapter(8,0);Map.ShowInformation(Map.Points.First(p=>p.kind==MapPointKind.LevelSlot&&p.slotIndex==0));Check(Map.InformationVisible&&SceneManager.GetActiveScene().name=="LevelMap"&&Directory.GetFiles(archiveRoot,"level-*.json").Length==0,"Kilitli401 üretici başlatmaz");Tap("MapInfoClose");Tap("ChapterClose");
   Map.EnterLevel(400);yield return Ready("Gameplay",400);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",401);Check(GameplayLevelCatalog.Current().Generated.level==401&&GameplayLevelCatalog.Current().Generated.target==LevelDifficulty.Easy,"Gerçek400→401, yumuşak ada girişi");
   var start=Model.Export();var move=GameplayLevelCatalog.Current().Generated.solution[0];Tap("Bottle "+(move.source+1));Tap("Bottle "+(move.target+1));yield return new WaitForSecondsRealtime(1.05f);Tap("Undo");Check(Model.Moves==0&&LevelGenerator.Exact(start,Model.Export()),"401 Undo aynı pin");Tap("Restart");Check(LevelGenerator.Exact(start,Model.Export()),"401 Restart aynı düzen");
   string signature=GameplayLevelCatalog.Current().Generated.signature;yield return Solve();Tap("ResultHome");yield return Ready("LevelMap");yield return Clockwork();Check(Map.Progression.Stars(401)>0&&Map.Progression.CanEnter(402)&&!Map.Progression.CanEnter(403),"401 yıldızı/402 açık/403 kilitli");
   Map.OpenChapter(8,0);yield return null;Check(Map.Points.Count(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy)==10,"401–410 seçicisinde yalnız10 gerçek düğüm");Capture("Clockwork_Chapter",900,1600);Tap("ChapterClose");
   for(int i=0;i<5;i++){Map.OpenChapter(8,i);Check(Map.Points.Where(p=>p.kind==MapPointKind.LevelSlot).All(p=>Map.Progression.LevelId(p.regionIndex,p.slotIndex)>=401&&Map.Progression.LevelId(p.regionIndex,p.slotIndex)<=450),"Gerçek50 bölüm grup bağlantısı "+i);Tap("ChapterClose");}
   yield return new WaitForSecondsRealtime(1);var data=GameplayProgress.Current.Snapshot;int stars=Map.Progression.Stars(401);Bind();SceneManager.LoadScene("LevelMap");yield return Ready("LevelMap");yield return Clockwork();Check(Map.Progression.Stars(401)==stars&&GameplayLevelCatalog.Load(401).Generated.signature==signature,"Diskten yıldız ve değişmez seed/pin yeniden açıldı");
   var visual=FindFirstObjectByType<ClockworkIslandPresentation>();float phase=visual.EnergyPhase;float[] angles=visual.gears.Select(g=>g.localEulerAngles.z).ToArray();Capture("Clockwork_Motion_A",900,1600);yield return new WaitForSecondsRealtime(1.1f);
   Check(visual.EnergyPhase>phase,"Kristal/saat ışığı shader saati ilerler");Check(visual.gears.Length==3,"Yalnız3 önemli dişli ayrı transform");
   for(int i=0;i<3;i++){float delta=Mathf.DeltaAngle(angles[i],visual.gears[i].localEulerAngles.z);Check(Mathf.Abs(delta)>1&&Mathf.Abs(delta)<5&&Mathf.Sign(delta)==(i%2==0?1:-1),"Yavaş ters yönlü dekoratif dişli "+i);}
   Check(visual.GetComponentsInChildren<Rigidbody>(true).Length==0&&visual.GetComponentsInChildren<Rigidbody2D>(true).Length==0,"Saat adasında fizik simülasyonu yok");Check(visual.EntranceProgress==1,"Hafif açılış tamamlanır");Capture("Clockwork_Motion_B",900,1600);
   foreach(int h in new[]{1600,1950,2000}){Capture("Clockwork_Map-"+h,900,h,h==2000);Map.OpenChapter(8,4);Capture("Clockwork_Chapter441-"+h,900,h,h==2000);Tap("ChapterClose");}
   CaptureIsland();
   yield return Performance();
   int[] ids=Map.Points.Select(p=>p.GetInstanceID()).ToArray();var rebindWatch=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<120;i++){Map.FocusRegion(i%2==0?0:8,false);Check(IslandArtSlot.LiveHandles<=3&&IslandArtSlot.LiveInstances<=3,"Hızlı kaydırma handle sınırı "+i);}rebindWatch.Stop();File.WriteAllText(Output+"REBIND_PERFORMANCE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{scrollRebinds=120,totalEditorMs=rebindWatch.Elapsed.TotalMilliseconds,meanEditorMs=rebindWatch.Elapsed.TotalMilliseconds/120,handleLimit=3,pooledPoints=Map.Points.Count}));yield return Clockwork();Check(ids.SequenceEqual(Map.Points.Select(p=>p.GetInstanceID()))&&Map.PooledIslands==5&&Map.Points.Count==40,"Aynı40 düğüm/5 ada havuzu");
   Map.FocusRegion(0,false);yield return null;yield return null;Check(IslandArtSlot.LiveHandles==0&&IslandArtSlot.LiveInstances==0&&FindFirstObjectByType<ClockworkIslandPresentation>()==null,"Görünmeyen clockwork instance ve handle bırakıldı");Check(old.SequenceEqual(Map.terrain.Select(s=>s.GetInstanceID())),"İlk dört sprite referansı aynı");
   yield return Load(449);Map.EnterLevel(450);yield return Ready("Gameplay",450);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",451);Check(GameplayLevelCatalog.Current().Generated.level==451&&GameplayLevelCatalog.Current().Generated.target==LevelDifficulty.Easy,"Gerçek450→451, sonraki ada davranışı aynı");Capture("Gameplay_451",900,1600);Tap("Home");yield return Ready("LevelMap");Check(Map.Progression.CurrentId==451&&!Map.Progression.CanEnter(452),"451 açık452 kilitli");yield return Clockwork();Check(Map.Progression.Completed(8)==50&&Map.Progression.ChestReady(8),"50/50 gerçek tamamlanma sandığı");
   var chest=Map.Points.First(p=>p.kind==MapPointKind.Chest&&p.regionIndex==8&&p.gameObject.activeInHierarchy);Map.ShowInformation(chest);int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);Tap("ClaimMapChest");Check(GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints+1&&Map.Progression.ChestClaimed(8),"Clockwork sandığı tek atomik ödül");bool granted;Check(Map.Progression.ClaimChest(8,out granted)&&!granted,"Ödül tekrarı engellendi");Tap("MapInfoClose");Bind();Check(MapProgressionCatalog.Open(GameplayProgress.Current).ChestClaimed(8),"Sandık kaydı yeniden açılışta korunur");
   yield return QuestsAndNavigation();
  }
  IEnumerator QuestsAndNavigation()
  {
   var before=GameplayProgress.Current.Snapshot;Tap("MapHome");yield return Ready("Home");
   Tap("MissionsButton");yield return null;var daily=FindFirstObjectByType<Mewgic.Daily.DailyActivitiesPanel>();Check(daily!=null&&daily.IsOpen&&!daily.GiftVisible,"Gerçek günlük görevler paneli açılır");Capture("Daily_Missions",900,1600);Tap("DailyClose");
   Tap("NavEvents");yield return null;var hub=FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>();Check(hub!=null&&hub.IsOpen,"Gerçek etkinlik merkezi açılır");Capture("Events_Hub",900,1600);Tap("RewardHubClose");
   var after=GameplayProgress.Current.Snapshot;Check(before.daily.progress.SequenceEqual(after.daily.progress)&&before.daily.claimed.SequenceEqual(after.daily.claimed)&&before.daily.wins.SequenceEqual(after.daily.wins),"Görev sayaç/makbuzları harita→Home gezinmesinde aynı");Check(before.hub.weekWins==after.hub.weekWins&&before.hub.seasonWins==after.hub.seasonWins&&before.hub.weekReceipts.SequenceEqual(after.hub.weekReceipts),"Haftalık/sezon görevi ilerlemesi korunur");
   Tap("NavMap");yield return Ready("LevelMap");yield return Clockwork();Check(Map.Progression.ChestClaimed(8)&&Map.Progression.Stars(450)>0,"Home/görev/etkinlik→Clockwork dönüşünde yıldız/sandık korunur");
  }
  IEnumerator Performance()
  {
   var samples=new List<double>();for(int i=0;i<180;i++){yield return null;samples.Add(Time.unscaledDeltaTime*1000);}
   samples.Sort();var p=FindFirstObjectByType<ClockworkIslandPresentation>();
   File.WriteAllText(Output+"FRAME_PERFORMANCE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{environment="Windows Unity Editor / Android import target / AssetDatabase Play Mode",frames=samples.Count,medianFrameMs=samples[samples.Count/2],p95FrameMs=samples[(int)(samples.Count*.95)],maxFrameMs=samples.Last(),liveHandles=IslandArtSlot.LiveHandles,liveInstances=IslandArtSlot.LiveInstances,renderedImages=p.GetComponentsInChildren<Image>().Length,drawCalls=UnityEditor.UnityStats.drawCalls,note="Editor kare süreleri; telefon FPS/GPU/termal ölçümü değildir."},Newtonsoft.Json.Formatting.Indented));
   Check(IslandArtSlot.LiveHandles<=3&&IslandArtSlot.LiveInstances<=3,"180 gerçek kare boyunca görünür sanat sınırı");
  }
  void Audit(MapWorld map,int height)
  {
   foreach(var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))if(t.enabled&&t.gameObject.activeInHierarchy){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Taşma yok "+height+"/"+t.name);}
   if(map==null)return;
   var targets=map.Points.Where(p=>p.regionIndex==8&&p.gameObject.activeInHierarchy&&p.kind!=MapPointKind.LevelSlot).ToArray();
   for(int i=0;i<targets.Length;i++)for(int j=i+1;j<targets.Length;j++) {
    var ca=new Vector3[4];var cb=new Vector3[4];((RectTransform)targets[i].transform).GetWorldCorners(ca);((RectTransform)targets[j].transform).GetWorldCorners(cb);
    bool overlap=Mathf.Min(ca[2].x,cb[2].x)-Mathf.Max(ca[0].x,cb[0].x)>.1f&&Mathf.Min(ca[2].y,cb[2].y)-Mathf.Max(ca[0].y,cb[0].y)>.1f;
    Check(!overlap,"Clockwork grup/sandık dokunma alanları çakışmaz "+height+"/"+i+"/"+j);
   }
   foreach(string name in new[]{"MapHeader","RegionNavigation","MapNavigation"}){
    var r=(RectTransform)map.SafeArea.Find(name);var c=new Vector3[4];r.GetWorldCorners(c);var a=map.SafeArea.InverseTransformPoint(c[0]);var b=map.SafeArea.InverseTransformPoint(c[2]);Check(a.x>=map.SafeArea.rect.xMin-.1f&&a.y>=map.SafeArea.rect.yMin-.1f&&b.x<=map.SafeArea.rect.xMax+.1f&&b.y<=map.SafeArea.rect.yMax+.1f,"Safe Area "+name+"/"+height);
   }
   foreach(var p in map.Points.Where(p=>p.regionIndex==8&&p.gameObject.activeInHierarchy)){
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
    if(map!=null){map.Layout();map.FocusRegion(8,false);}FindFirstObjectByType<GameplayPresentation>()?.Layout();Canvas.ForceUpdateCanvases();Audit(map,height);camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Output+name+".png",tex.EncodeToPNG());
   }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=dist;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(map!=null){map.Layout();map.Scroll.verticalNormalizedPosition=scroll;}}
  }
  public void CaptureIsland()
  {
   var camera=Camera.main;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var map=Map;var src=FindFirstObjectByType<ClockworkIslandPresentation>();
   var go=new GameObject("ClockworkLargePreview",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   var rt=new RenderTexture(1200,1400,24);var tex=new Texture2D(1200,1400,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=rt;map.Canvas.gameObject.SetActive(false);Canvas.ForceUpdateCanvases();
    var bg=new GameObject("PreviewSky",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);var br=(RectTransform)bg.transform;br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=br.offsetMax=Vector2.zero;bg.GetComponent<Image>().sprite=map.sky;bg.GetComponent<Image>().color=new Color(.5f,.5f,.65f,1);
    var island=Instantiate(src.gameObject,go.transform,false);var r=(RectTransform)island.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(640,640);r.anchoredPosition=Vector2.zero;island.GetComponent<ClockworkIslandPresentation>().Animate=false;island.GetComponent<CanvasGroup>().alpha=1;island.transform.localScale=Vector3.one*(1160f/640f);
    Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,1400),0,0);tex.Apply();File.WriteAllBytes(Output+"Clockwork_LargePreview.png",tex.EncodeToPNG());
   }finally{map.Canvas.gameObject.SetActive(true);camera.targetTexture=oldTarget;RenderTexture.active=oldActive;DestroyImmediate(go);DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();map.Layout();}
  }
  void OnDestroy(){if(active!=this)return;Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();GameplayLevelCatalog.TestArchive=null;active=null;}
 }
}
#endif
