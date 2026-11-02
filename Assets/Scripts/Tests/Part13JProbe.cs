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
 public sealed class Part13JProbe : MonoBehaviour
 {
  const string Output="docs/ProductionReports/Part13J/";
  public bool preview, resume;
  string key,archiveRoot;
  readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
  readonly Dictionary<string,string> player=new Dictionary<string,string>();
  static Part13JProbe active;
  MapWorld Map=>FindFirstObjectByType<MapWorld>();
  PrototypeApp App=>FindFirstObjectByType<PrototypeApp>();
  Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(App);
  public static void Begin(bool view=false,bool reopen=false)
  {
   if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı");
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");
   var qa=new GameObject("PART13J Güvenli QA").AddComponent<Part13JProbe>();qa.preview=view;qa.resume=reopen;
   EditorApplication.isPlaying=true;
  }
  void Awake()
  {
   if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);Directory.CreateDirectory(Output);
   foreach(var suffix in new[]{"",".backup",".corrupt"}){string k=GameplayProgress.StorageKey+suffix;player[k]=PlayerPrefs.HasKey(k)?PlayerPrefs.GetString(k):null;}
   Application.runInBackground=true;key="Mewgic.Tests.Part13J."+Guid.NewGuid().ToString("N");archiveRoot=Path.Combine(Path.GetTempPath(),"Mewgic-Part13J-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(archiveRoot);
   if(resume){key=EditorPrefs.GetString("Mewgic.Part13J.ResumeKey");archiveRoot=EditorPrefs.GetString("Mewgic.Part13J.ResumeArchive");}
   GameplayLevelCatalog.TestArchive=new FileLevelArchive(archiveRoot);if(resume)Bind();else Bind(Part12BProbe.Data(preview?599:0));GameplayLevelCatalog.ResetSelection();Application.logMessageReceived+=Log;
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
   if(preview){yield return Ready("LevelMap");L.Select("tr");yield return Focus(0);yield break;}
   var stack=new Stack<IEnumerator>();stack.Push(resume?Reopen():Run());string failure=null;
   while(stack.Count>0){bool more=false;object next=null;try{more=stack.Peek().MoveNext();if(more)next=stack.Peek().Current;}catch(Exception e){failure=e.ToString();break;}if(!more){stack.Pop();continue;}if(next is IEnumerator nested&&!(next is CustomYieldInstruction)){stack.Push(nested);continue;}yield return next;}
   bool intact=player.All(p=>p.Value==null?!PlayerPrefs.HasKey(p.Key):PlayerPrefs.GetString(p.Key)==p.Value);
   File.WriteAllText(Output+(resume?"REOPEN_TESTS.json":"UNITY_TESTS.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&warnings.Count==0&&intact,failure,count=checks.Count,checks,errors,warnings,playerSaveAndBackupIntact=intact},Newtonsoft.Json.Formatting.Indented));
   EditorApplication.isPlaying=false;
  }
  IEnumerator Reopen(){
   yield return Ready("LevelMap");L.Select("tr",false);yield return new WaitForSecondsRealtime(3);
   Check(Map.Progression.CurrentId==600&&Map.Progression.Stars(599)>0,"Play Mode kapat/ac: mevcut ilerleme diskte ayni");
   Check(GameplayLevelCatalog.Load(600).Generated.signature==EditorPrefs.GetString("Mewgic.Part13J.ResumeSignature"),"Play Mode kapat/ac: uretilmis600 pini ayni");
   Map.EnterLevel(600);yield return Ready("Gameplay",600);Tap("Home");yield return Ready("LevelMap");
   yield return Focus(0);foreach(int h in new[]{1600,1950,2000})Capture("Island_01_"+h,900,h,h==2000);
   for(int r=0;r<12;r++){
    yield return Focus(r);
    if(r>=4){var slot=FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).Single(x=>x.BoundKey==Map.Progression.Region(r).artKey);var p=slot.GetComponentsInChildren<MonoBehaviour>().Single(x=>x!=null&&x.GetType().Name.EndsWith("IslandPresentation"));var prop=p.GetType().GetProperties().Single(x=>x.Name.EndsWith("Phase"));float phase=(float)prop.GetValue(p);var transforms=p.GetComponentsInChildren<RectTransform>();var state=transforms.Select(x=>x.anchoredPosition.ToString()+x.localRotation.ToString()).ToArray();yield return new WaitForSecondsRealtime(.6f);Check((float)prop.GetValue(p)>phase,"Gercek efekt fazi: "+p.GetType().Name);Check(!state.SequenceEqual(transforms.Select(x=>x.anchoredPosition.ToString()+x.localRotation.ToString())),"Gercek dekor hareketi: "+p.GetType().Name);}
    if(r<11){Tap("NextIsland");yield return new WaitForSecondsRealtime(1.2f);Check(Map.ActiveRegion==r+1,"Sonraki ada navigasyonu "+r);Tap("PreviousIsland");yield return new WaitForSecondsRealtime(1.2f);Check(Map.ActiveRegion==r,"Onceki ada navigasyonu "+r);}
    if(r<11){var chest=Map.Points.First(x=>x.regionIndex==r&&x.kind==MapPointKind.Chest&&x.gameObject.activeInHierarchy);Map.ShowInformation(chest);int hints=GameplayProgress.Current.Remaining(BoosterKind.Hint);Tap("ClaimMapChest");Check(Map.Progression.ChestClaimed(r)&&GameplayProgress.Current.Remaining(BoosterKind.Hint)==hints+1,"Gercek sandik odulu "+r);bool granted;Check(Map.Progression.ClaimChest(r,out granted)&&!granted,"Sandik ikinci odul vermez "+r);Tap("MapInfoClose");}
   }
   yield return Load(600);yield return Focus(11);var last=Map.Points.First(x=>x.regionIndex==11&&x.kind==MapPointKind.Chest&&x.gameObject.activeInHierarchy);Map.ShowInformation(last);Tap("ClaimMapChest");Check(Map.Progression.ChestClaimed(11),"Aurora50/50 gercek sandik");Tap("MapInfoClose");Bind();Check(MapProgressionCatalog.Open(GameplayProgress.Current).ChestClaimed(11),"Sandik kapanis sonrasi kayitta");
  }
  int region;
  IEnumerator Focus(int r){region=r;Map.FocusRegion(r,false);float until=Time.unscaledTime+20;while(Time.unscaledTime<until&&FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).Any(x=>x.BoundKey!=null&&!x.Loaded))yield return null;yield return new WaitForSecondsRealtime(1.1f);Check(!FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).Any(x=>x.BoundKey!=null&&!x.Loaded),"Görünür katmanlar hazır "+r);}
  void Hit(MapPoint p){var ev=UnityEngine.EventSystems.EventSystem.current;var d=new UnityEngine.EventSystems.PointerEventData(ev){position=RectTransformUtility.WorldToScreenPoint(Map.Canvas.worldCamera,p.transform.position)};var h=new List<UnityEngine.EventSystems.RaycastResult>();ev.RaycastAll(d,h);Check(h.Count>0&&h[0].gameObject.GetComponentInParent<MapPoint>()==p,"Gerçek raycast "+region+"/"+p.kind+"/"+p.slotIndex);}
  IEnumerator Run()
  {
   yield return Ready("LevelMap");L.Select("tr");
   Check(Map.Progression.CurrentId==1&&Map.Progression.CanEnter(1)&&!Map.Progression.CanEnter(2),"Yeni oyuncu: yalnız bölüm1 açık");
   Check(L.Choose("ja-JP",new[]{"ko-KR","zh-CN"})=="en"&&L.Match("en-US")=="en","Desteklenmeyen cihaz dilinde İngilizce fallback");
   var def=MapProgressionCatalog.Definition();Check(def.regions.Length==12&&def.regions.All(x=>x.artReady),"12 gerçek sanat tanımı");
   yield return Load(599);
   for(int r=0;r<12;r++){
    yield return Focus(r);Check(Map.ActiveRegion==r&&Map.Progression.Region(r).firstLevel==r*50+1&&Map.Progression.Region(r).levelCount==50,"Ada tema/range "+r+"/"+def.regions[r].displayName);
    foreach(var p in Map.Points.Where(p=>p.regionIndex==r&&p.gameObject.activeInHierarchy))Hit(p);
    float phase=Map.EnvironmentTime;yield return new WaitForSecondsRealtime(.25f);Check(Map.EnvironmentTime>phase,"Ortam animasyonu saati "+r);
    var arts=FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None);if(r>=4){var slot=arts.Single(x=>x.BoundKey==def.regions[r].artKey);Check(slot.Loaded,"Gerçek ayrı katman prefabı "+r);var fields=slot.GetComponentsInChildren<MonoBehaviour>().Where(x=>x!=null&&x.GetType().Name.EndsWith("IslandPresentation")).ToArray();Check(fields.Length==1,"Ada animasyon bileşeni "+r);var prop=fields[0].GetType().GetProperty("WaterPhase");if(prop!=null){float before=(float)prop.GetValue(fields[0]);yield return new WaitForSecondsRealtime(.2f);Check((float)prop.GetValue(fields[0])>before,"Gerçek su/efekt fazı "+r);}}
    foreach(int height in new[]{1600,1950,2000})Capture("Island_"+(r+1).ToString("00")+"_"+height,900,height,height==2000);
    foreach(string code in L.Codes){Check(L.Select(code,false),"Dil seçimi "+code);yield return null;Check(!string.IsNullOrWhiteSpace(L.Resolve(L.Island(r))),"Çevrilmiş ada adı "+r+"/"+code);Audit(Map,1600);}
    L.Select("tr",false);yield return null;
    for(int g=0;g<5;g++){Map.OpenChapter(r,g);yield return null;var points=Map.Points.Where(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy).ToArray();Check(points.Length==10&&points.All(p=>Map.Progression.LevelId(p.regionIndex,p.slotIndex)==r*50+g*10+p.slotIndex%10+1),"Gerçek üretici 10'lu grup "+r+"/"+g);foreach(var p in points)Hit(p);if(g==0)Capture("Island_"+(r+1).ToString("00")+"_Chapter",900,1600);Tap("ChapterClose");yield return null;}
   }
   var ids=Map.Points.Select(p=>p.GetInstanceID()).ToArray();var frames=new List<double>();int peakH=0,peakI=0;
   for(int i=0;i<360;i++){Map.CancelJump();Map.Scroll.verticalNormalizedPosition=(Mathf.Sin(i*Mathf.PI/90)+1)*.5f;yield return null;frames.Add(Time.unscaledDeltaTime*1000);peakH=Math.Max(peakH,IslandArtSlot.LiveHandles);peakI=Math.Max(peakI,IslandArtSlot.LiveInstances);Check(IslandArtSlot.LiveHandles<=5&&IslandArtSlot.LiveInstances<=5,"Kaydırma görünür sanat sınırı "+i);}
   Check(ids.SequenceEqual(Map.Points.Select(p=>p.GetInstanceID()))&&Map.PooledIslands==5&&Map.Points.Count==40,"Kaydırma boyunca sabit40 düğüm/5 slot;600 aktif nesne yok");
   frames.Sort();File.WriteAllText(Output+"SCROLL_PERFORMANCE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{frames=frames.Count,medianMs=frames[180],p95Ms=frames[342],maxMs=frames.Last(),peakHandles=peakH,peakInstances=peakI,pooledNodes=40,note="Windows Editor, gerçek360 çizilen kaydırma karesi; telefon FPS değildir"},Newtonsoft.Json.Formatting.Indented));
   yield return Focus(0);Check(IslandArtSlot.LiveHandles==0&&IslandArtSlot.LiveInstances==0,"Görünmeyen yeni sanat handle/instance sıfır");
   for(int n=50;n<=600;n+=50){
    yield return Load(n-1);Check(!Map.Progression.CanEnter(n+1)&&!Map.Progression.RegionOpen(n/50),"Sınır öncesi kilit "+n);Map.EnterLevel(n);yield return Ready("Gameplay",n);yield return Solve();Tap("NextLevel");yield return Ready("Gameplay",n+1);Check(GameplayLevelCatalog.Current().Generated.level==n+1,"Gerçek sınır geçişi "+n+"→"+(n+1));string sig=GameplayLevelCatalog.Current().Generated.signature;
    yield return Solve();Tap("ResultHome");yield return Ready("LevelMap");Tap("FocusCurrent");yield return new WaitForSecondsRealtime(1.2f);Check(Map.Progression.CurrentId==n+2&&Map.Progression.Stars(n)>0&&Map.Progression.Stars(n+1)>0&&Map.ActiveRegion==n/50,"Kazanma→harita doğru ada/yıldız/ilerleme "+n);
    Bind();SceneManager.LoadScene("LevelMap");yield return Ready("LevelMap");Check(Map.Progression.CurrentId==n+2&&GameplayLevelCatalog.Load(n+1).Generated.signature==sig,"Kayıt yeniden okuma değişmez pin "+(n+1));Map.EnterLevel(n+1);yield return Ready("Gameplay",n+1);Check(GameplayLevelCatalog.Current().Generated.signature==sig,"Haritadan aynı bölümü tekrar seçme "+(n+1));Tap("Home");yield return Ready("LevelMap");
   }
   Check(Map.Progression.CurrentId==602&&!Map.Progression.Region(12).artReady,"600 sonrası602 devam;13.ada sanatı varmış gibi gösterilmez");Capture("Continuation_602",900,1600,false,false);
   var legacy=Part12BProbe.Data(175);legacy.worldMap=null;Bind(legacy);SceneManager.LoadScene("LevelMap");yield return Ready("LevelMap");Check(Map.Progression.CurrentId==176&&GameplayProgress.Current.WorldMap.legacyChestEntitlements.Count==4,"Eski kayıt göçü:176 ve dört eski sandık hakkı");Bind();Check(GameplayProgress.Current.Completed(175),"Eski kayıt göçü diskten tekrar açıldı");
   // Play Mode yeniden başlatma için yalnız izole profil/üretici arşivi saklanır.
   Bind(Part12BProbe.Data(599));var pin=GameplayLevelCatalog.Load(600).Generated.signature;
   EditorPrefs.SetString("Mewgic.Part13J.ResumeKey",key);EditorPrefs.SetString("Mewgic.Part13J.ResumeArchive",archiveRoot);EditorPrefs.SetString("Mewgic.Part13J.ResumeSignature",pin);
   Check(L.Missing.Count==0,"Sekiz dilde eksik anahtar yok");
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
