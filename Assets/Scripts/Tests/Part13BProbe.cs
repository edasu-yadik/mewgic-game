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
 public sealed class Part13BProbe : MonoBehaviour
 {
  const string Output="docs/ProductionReports/Part13B/";
  public bool preview;
  string key,archiveRoot;
  readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
  readonly Dictionary<string,string> player=new Dictionary<string,string>();
  static Part13BProbe active;
  MapWorld Map=>FindFirstObjectByType<MapWorld>();
  PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
  Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(App);
  public static void Begin(bool view=false)
  {
   if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı");
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
   new GameObject("PART13B Güvenli QA").AddComponent<Part13BProbe>().preview=view;
   EditorApplication.isPlaying=true;
  }
  void Awake()
  {
   if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);
   foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}
   Application.runInBackground=true;key="Mewgic.Tests.Part13B."+Guid.NewGuid().ToString("N");archiveRoot=Path.Combine(Path.GetTempPath(),"Mewgic-Part13B-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(archiveRoot);
   GameplayLevelCatalog.TestArchive=new FileLevelArchive(archiveRoot);Bind(Part12BProbe.Data(preview?200:199));GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;
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
   while(Time.unscaledTime<until&&(!L.Ready||SceneManager.GetActiveScene().name!=scene||scene=="LevelMap"&&(Map==null||!Map.Mounted)||scene=="Gameplay"&&(App==null||GameplayLevelCatalog.SelectedId!=id)))yield return null;
   yield return null;yield return null;
   Check(L.Ready&&SceneManager.GetActiveScene().name==scene,"Hazır gerçek sahne "+scene+"/"+id);
   if(scene=="LevelMap")Check(Map!=null&&Map.Mounted,"LevelMap mount");
   if(scene=="Gameplay"){Check(App!=null&&GameplayLevelCatalog.SelectedId==id,"Gameplay kimliği "+id);FindFirstObjectByType<PawGuide>()?.Close();}
  }
  IEnumerator Woods()
  {
   Map.FocusRegion(4,false);float until=Time.unscaledTime+20;
   while(Time.unscaledTime<until&&FindFirstObjectByType<WoodsIslandPresentation>()==null)yield return null;
   Check(FindFirstObjectByType<WoodsIslandPresentation>()!=null,"Gerçek Addressables woods prefabı yüklendi");
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
   if(preview){yield return Ready("LevelMap");L.Select("tr");yield return Woods();yield break;}
   var stack=new Stack<IEnumerator>();stack.Push(Run());string failure=null;
   while(stack.Count>0){bool more=false;object next=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){failure=e.ToString();break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested&&!(next is CustomYieldInstruction)){stack.Push(nested);continue;}yield return next;}
   bool intact=player.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.GetString(p.Key)==p.Value);
   File.WriteAllText(Output+"UNITY_TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&warnings.Count==0&&intact,failure,count=checks.Count,checks,errors,warnings,playerSaveAndBackupIntact=intact},Newtonsoft.Json.Formatting.Indented));
   EditorApplication.isPlaying=false;
  }
  IEnumerator Run()
  {
   yield return Ready("LevelMap");L.Select("tr");
   var def=MapProgressionCatalog.Definition();Check(def.regions.Length==12&&def.regions[4].artReady&&def.regions.Skip(5).All(r=>!r.artReady),"Yalnız beşinci ada sanatı açıldı");
   var old=Map.terrain.Select(s=>s.GetInstanceID()).ToArray();
   yield return Woods();Check(!Map.Progression.RegionOpen(4)&&!Map.Progression.CanEnter(201),"200 kazanılmadan woods201 kilitli");Capture("Woods_Locked",900,1600);
   Map.OpenChapter(4,0);Map.ShowInformation(Map.Points.First(p=>p.kind==MapPointKind.LevelSlot&&p.slotIndex==0));Check(Map.InformationVisible&&SceneManager.GetActiveScene().name=="LevelMap"&&Directory.GetFiles(archiveRoot,"level-*.json").Length==0,"Kilitli201 üretici başlatmaz");Tap("MapInfoClose");Tap("ChapterClose");
   Map.EnterLevel(200);yield return Ready("Gameplay",200);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",201);Check(GameplayLevelCatalog.Current().Generated.level==201&&GameplayLevelCatalog.Current().Generated.target==LevelDifficulty.Easy,"Gerçek200→201, yumuşak ada girişi");
   var start=Model.Export();var move=GameplayLevelCatalog.Current().Generated.solution[0];Tap("Bottle "+(move.source+1));Tap("Bottle "+(move.target+1));yield return new WaitForSecondsRealtime(1.05f);Tap("Undo");Check(Model.Moves==0&&LevelGenerator.Exact(start,Model.Export()),"201 Undo aynı pin");Tap("Restart");Check(LevelGenerator.Exact(start,Model.Export()),"201 Restart aynı düzen");
   string signature=GameplayLevelCatalog.Current().Generated.signature;yield return Solve();Tap("ResultHome");yield return Ready("LevelMap");yield return Woods();Check(Map.Progression.Stars(201)>0&&Map.Progression.CanEnter(202)&&!Map.Progression.CanEnter(203),"201 yıldızı/202 açık/203 kilitli");
   Map.OpenChapter(4,0);yield return null;Check(Map.Points.Count(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy)==10,"201–210 seçicisinde yalnız10 gerçek düğüm");Capture("Woods_Chapter",900,1600);Tap("ChapterClose");
   for(int i=0;i<5;i++){Map.OpenChapter(4,i);Check(Map.Points.Where(p=>p.kind==MapPointKind.LevelSlot).All(p=>Map.Progression.LevelId(p.regionIndex,p.slotIndex)>=201&&Map.Progression.LevelId(p.regionIndex,p.slotIndex)<=250),"Gerçek50 bölüm grup bağlantısı "+i);Tap("ChapterClose");}
   yield return new WaitForSecondsRealtime(1);var data=GameplayProgress.Current.Snapshot;int stars=Map.Progression.Stars(201);Bind();SceneManager.LoadScene("LevelMap");yield return Ready("LevelMap");yield return Woods();Check(Map.Progression.Stars(201)==stars&&GameplayLevelCatalog.Load(201).Generated.signature==signature,"Diskten yıldız ve değişmez seed/pin yeniden açıldı");
   var visual=FindFirstObjectByType<WoodsIslandPresentation>();float phase=visual.WaterPhase;Vector2 mist=visual.mist.rectTransform.anchoredPosition;Color fly=visual.fireflies[0].GetComponent<Graphic>().color;Capture("Woods_Motion_A",900,1600);yield return new WaitForSecondsRealtime(1.1f);Check(visual.WaterPhase>phase&&visual.mist.rectTransform.anchoredPosition!=mist&&visual.fireflies[0].GetComponent<Graphic>().color!=fly,"Şelale shader saati, sis konumu ve ateşböceği alpha hareketli");Check(visual.EntranceProgress==1,"Hafif açılış tamamlanır");Capture("Woods_Motion_B",900,1600);
   foreach(int h in new[]{1600,1950,2000}){Capture("Woods_Map-"+h,900,h,h==2000);Map.OpenChapter(4,4);Capture("Woods_Chapter241-"+h,900,h,h==2000);Tap("ChapterClose");}
   CaptureIsland();
   int[] ids=Map.Points.Select(p=>p.GetInstanceID()).ToArray();for(int i=0;i<120;i++){Map.FocusRegion(i%2==0?0:4,false);Check(IslandArtSlot.LiveHandles<=1&&IslandArtSlot.LiveInstances<=1,"Hızlı kaydırma handle sınırı "+i);}yield return Woods();Check(ids.SequenceEqual(Map.Points.Select(p=>p.GetInstanceID()))&&Map.PooledIslands==5&&Map.Points.Count==40,"Aynı40 düğüm/5 ada havuzu");
   Map.FocusRegion(0,false);yield return null;yield return null;Check(IslandArtSlot.LiveHandles==0&&IslandArtSlot.LiveInstances==0&&FindFirstObjectByType<WoodsIslandPresentation>()==null,"Görünmeyen woods instance ve handle bırakıldı");Check(old.SequenceEqual(Map.terrain.Select(s=>s.GetInstanceID())),"İlk dört sprite referansı aynı");
   yield return Load(249);Map.EnterLevel(250);yield return Ready("Gameplay",250);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",251);Check(GameplayLevelCatalog.Current().Generated.level==251&&GameplayLevelCatalog.Current().Generated.target==LevelDifficulty.Easy,"Gerçek250→251, sonraki ada davranışı aynı");Capture("Gameplay_251",900,1600);Tap("Home");yield return Ready("LevelMap");Check(Map.Progression.CurrentId==251&&!Map.Progression.CanEnter(252),"251 açık252 kilitli");yield return Woods();Check(Map.Progression.Completed(4)==50&&Map.Progression.ChestReady(4),"50/50 gerçek tamamlanma sandığı");
   var chest=Map.Points.First(p=>p.kind==MapPointKind.Chest&&p.regionIndex==4&&p.gameObject.activeInHierarchy);Map.ShowInformation(chest);int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);Tap("ClaimMapChest");Check(GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints+1&&Map.Progression.ChestClaimed(4),"Woods sandığı tek atomik ödül");bool granted;Check(Map.Progression.ClaimChest(4,out granted)&&!granted,"Ödül tekrarı engellendi");Tap("MapInfoClose");Bind();Check(MapProgressionCatalog.Open(GameplayProgress.Current).ChestClaimed(4),"Sandık kaydı yeniden açılışta korunur");
  }
  void Audit(MapWorld map,int height)
  {
   foreach(var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))if(t.enabled&&t.gameObject.activeInHierarchy){t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Taşma yok "+height+"/"+t.name);}
   if(map==null)return;
   var targets=map.Points.Where(p=>p.regionIndex==4&&p.gameObject.activeInHierarchy&&p.kind!=MapPointKind.LevelSlot).ToArray();
   for(int i=0;i<targets.Length;i++)for(int j=i+1;j<targets.Length;j++) {
    var ca=new Vector3[4];var cb=new Vector3[4];((RectTransform)targets[i].transform).GetWorldCorners(ca);((RectTransform)targets[j].transform).GetWorldCorners(cb);
    bool overlap=Mathf.Min(ca[2].x,cb[2].x)-Mathf.Max(ca[0].x,cb[0].x)>.1f&&Mathf.Min(ca[2].y,cb[2].y)-Mathf.Max(ca[0].y,cb[0].y)>.1f;
    Check(!overlap,"Woods grup/sandık dokunma alanları çakışmaz "+height+"/"+i+"/"+j);
   }
   foreach(string name in new[]{"MapHeader","RegionNavigation","MapNavigation"}){
    var r=(RectTransform)map.SafeArea.Find(name);var c=new Vector3[4];r.GetWorldCorners(c);var a=map.SafeArea.InverseTransformPoint(c[0]);var b=map.SafeArea.InverseTransformPoint(c[2]);Check(a.x>=map.SafeArea.rect.xMin-.1f&&a.y>=map.SafeArea.rect.yMin-.1f&&b.x<=map.SafeArea.rect.xMax+.1f&&b.y<=map.SafeArea.rect.yMax+.1f,"Safe Area "+name+"/"+height);
   }
   foreach(var p in map.Points.Where(p=>p.regionIndex==4&&p.gameObject.activeInHierarchy)){
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
    if(map!=null){map.Layout();map.FocusRegion(4,false);}FindFirstObjectByType<GameplayPresentation>()?.Layout();Canvas.ForceUpdateCanvases();Audit(map,height);camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Output+name+".png",tex.EncodeToPNG());
   }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=dist;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(map!=null){map.Layout();map.Scroll.verticalNormalizedPosition=scroll;}}
  }
  public void CaptureIsland()
  {
   var camera=Camera.main;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var map=Map;var src=FindFirstObjectByType<WoodsIslandPresentation>();
   var go=new GameObject("WoodsLargePreview",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
   var rt=new RenderTexture(1200,1400,24);var tex=new Texture2D(1200,1400,TextureFormat.RGB24,false);
   try{
    camera.targetTexture=rt;map.Canvas.gameObject.SetActive(false);Canvas.ForceUpdateCanvases();
    var bg=new GameObject("PreviewSky",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);var br=(RectTransform)bg.transform;br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=br.offsetMax=Vector2.zero;bg.GetComponent<Image>().sprite=map.sky;bg.GetComponent<Image>().color=new Color(.5f,.5f,.65f,1);
    var island=Instantiate(src.gameObject,go.transform,false);var r=(RectTransform)island.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(1160,1160);r.anchoredPosition=Vector2.zero;island.GetComponent<WoodsIslandPresentation>().Animate=false;island.GetComponent<CanvasGroup>().alpha=1;island.transform.localScale=Vector3.one;
    Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1200,1400),0,0);tex.Apply();File.WriteAllBytes(Output+"Woods_LargePreview.png",tex.EncodeToPNG());
   }finally{map.Canvas.gameObject.SetActive(true);camera.targetTexture=oldTarget;RenderTexture.active=oldActive;DestroyImmediate(go);DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();map.Layout();}
  }
  void OnDestroy(){if(active!=this)return;Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();GameplayLevelCatalog.TestArchive=null;active=null;}
 }
}
#endif
