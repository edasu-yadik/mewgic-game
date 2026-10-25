using System;using System.IO;using System.Linq;using System.Collections.Generic;using Mewgic;using Mewgic.Core;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
[InitializeOnLoad]public static class BetaValidation
{
 static BetaValidation(){EditorApplication.update+=DispatchBuild;}
 public static void RequestBuild(){SessionState.SetBool("MewgicBetaBuildRequested",true);}
 static void DispatchBuild()
 {
  if(!SessionState.GetBool("MewgicBetaBuildRequested",false)||EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  SessionState.SetBool("MewgicBetaBuildRequested",false);
  try{BuildAndroid();}catch(Exception e){File.WriteAllText("docs/PART08_ANDROID_BUILD.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{result="Exception",error=e.ToString()},Newtonsoft.Json.Formatting.Indented));Debug.LogException(e);}
 }
 [Serializable] public class Result{public bool success;public string error;public string[] checks;}
 public static void SaveTests()
 {
  var checks=new List<string>();string key="Mewgic.Tests.Part08."+Guid.NewGuid().ToString("N");
  void Check(bool v,string m){if(!v)throw new Exception(m);checks.Add(m);}
  try{
   var now=new DateTimeOffset(2026,10,25,12,0,0,TimeSpan.Zero);var l=GameplayProgress.Open(key,()=>now);
   Check(l.Presentation.currentLevel==1&&l.Presentation.coins==0&&l.Presentation.gems==0,"New profile has real zero balances, level1");
   Check(l.SetResources(345,78,3)&&l.SetSoundEnabled(false),"Resources and settings committed");
   Check(l.Claim(1,3,true,out bool award,"beta-win",2)&&award,"First win committed");
   Check(l.SetCurrentLevel(2),"Current level committed");
   Check(l.ClaimDailyGift(),"Daily gift committed");
   Check(l.TrySpend(BoosterKind.Undo),"Booster spend committed");
   var a=GameplayProgress.Open(key,()=>now);
   Check(a.Presentation.currentLevel==2&&a.Presentation.coins==345&&a.Presentation.gems==78&&a.Presentation.energy==3&&!a.Presentation.soundEnabled,"Reopened current level,currency,energy,settings preserved");
   Check(a.BestStars(1)==3&&a.Remaining(BoosterKind.Undo)==2&&a.Remaining(BoosterKind.Hint)==5,"Reopened stars and booster balances preserved");
   Check(!a.ClaimDailyGift()&&a.DailySnapshot.progress[1]==1&&a.HubSnapshot.weekWins==1,"Daily claim and event progress survive reload");
   a.Claim(1,3,true,out award,"beta-win",2);Check(!award&&a.HubSnapshot.weekWins==1,"Repeated win receipt never duplicates reward/event");
   string good=PlayerPrefs.GetString(key);Check(PlayerPrefs.GetString(key+".backup")==good,"Validated backup contains same committed reward markers");
   PlayerPrefs.SetString(key,"{truncated");PlayerPrefs.Save();a=GameplayProgress.Open(key,()=>now);
   Check(a.Presentation.currentLevel==2&&a.BestStars(1)==3&&a.Remaining(BoosterKind.Hint)==5,"Corrupt primary recovered from committed backup");
   Check(PlayerPrefs.GetString(key+".corrupt")=="{truncated","Original corrupt document preserved");
   Check(!a.ClaimDailyGift(),"Recovery cannot duplicate claimed gift");
   PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();a=GameplayProgress.Open(key,()=>now);Check(a.Presentation.currentLevel==2&&a.Remaining(BoosterKind.Hint)==5,"Missing primary recovers committed backup instead of resetting profile");
   PlayerPrefs.SetString(key,"{\"version\":1}");PlayerPrefs.Save();a=GameplayProgress.Open(key,()=>now);Check(a.BestStars(1)==3&&a.Presentation.coins==345,"Syntactically valid but incomplete save recovers backup instead of defaults");
   PlayerPrefs.SetString(key,"{\"version\":999}");PlayerPrefs.Save();bool rejected=false;try{GameplayProgress.Open(key);}catch{rejected=true;}
   Check(rejected&&PlayerPrefs.GetString(key).Contains("999"),"Future schema rejected without downgrade or overwrite");
   PlayerPrefs.SetString(key,"broken");PlayerPrefs.SetString(key+".backup","also broken");PlayerPrefs.Save();rejected=false;try{GameplayProgress.Open(key);}catch{rejected=true;}
   Check(rejected&&PlayerPrefs.GetString(key)=="broken","Two corrupt copies preserved without silent reset");
   PlayerPrefs.SetString(key,"{\"version\":1,\"undo\":2,\"hint\":7,\"extra\":1,\"shuffle\":1,\"levels\":[]}");PlayerPrefs.DeleteKey(key+".backup");PlayerPrefs.Save();a=GameplayProgress.Open(key);
   Check(a.Remaining(BoosterKind.Hint)==7&&a.Presentation.coins==0&&a.Presentation.currentLevel==1,"Legacy save migration retains rights and defaults new child");
   var failed=new ProgressLedger(new ProgressData(),_=>throw new IOException("injected"));Check(!failed.SetResources(5,5,1)&&!failed.SetSoundEnabled(false)&&!failed.SetCurrentLevel(2),"Write failure rejects presentation changes");
   Check(failed.Presentation.coins==0&&failed.Presentation.soundEnabled&&failed.Presentation.currentLevel==1,"Failed writes leave in-memory presentation intact");
   var overflow=new ProgressLedger(new ProgressData{hint=int.MaxValue},_=>{});Check(!overflow.Claim(1,3,true,out award)&&!award&&!overflow.Completed(1),"First victory overflow is safely rejected atomically");
   var model=new HomeResourceModel(a);model.SetValues(900,42,2);Check(GameplayProgress.Open(key).Presentation.coins==900&&!model.IsPreview,"Observable UI uses persisted data");
   File.WriteAllText("docs/PART08_SAVE_TESTS.json",JsonUtility.ToJson(new Result{success=true,checks=checks.ToArray()},true));
  }catch(Exception e){File.WriteAllText("docs/PART08_SAVE_TESTS.json",JsonUtility.ToJson(new Result{success=false,error=e.ToString(),checks=checks.ToArray()},true));throw;}
  finally{foreach(string suffix in new[]{"",".backup",".corrupt"})PlayerPrefs.DeleteKey(key+suffix);PlayerPrefs.Save();}
 }
 public static object AuditScenes()
 {
  if(EditorApplication.isPlaying)throw new Exception("Stop Play first");var original=EditorSceneManager.GetActiveScene().path;var scenes=new List<object>();
  try{foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath)){
   var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);int missing=0,broken=0,refs=0;
   foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true)){
    missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
    if(PrefabUtility.GetPrefabInstanceStatus(t.gameObject)==PrefabInstanceStatus.MissingAsset)broken++;
    foreach(var c in t.GetComponents<Component>()){if(c==null)continue;var s=new SerializedObject(c);var prop=s.GetIterator();while(prop.NextVisible(true))if(prop.propertyType==SerializedPropertyType.ObjectReference&&prop.objectReferenceValue==null&&prop.objectReferenceInstanceIDValue!=0)refs++;}
   }
   scenes.Add(new{path,roots=scene.GetRootGameObjects().Select(g=>g.name).ToArray(),missingScripts=missing,missingPrefabs=broken,brokenReferences=refs});
  }}finally{EditorSceneManager.OpenScene(original);}
  var result=new{scenes,build=EditorBuildSettings.scenes.Select(s=>new{s.path,s.enabled}).ToArray(),product=PlayerSettings.productName,identifier=PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android),version=PlayerSettings.bundleVersion,code=PlayerSettings.Android.bundleVersionCode,minSDK=(int)PlayerSettings.Android.minSdkVersion,targetSDK=(int)PlayerSettings.Android.targetSdkVersion,architecture=PlayerSettings.Android.targetArchitectures.ToString(),backend=PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android).ToString(),orientation=PlayerSettings.defaultInterfaceOrientation.ToString()};
  File.WriteAllText("docs/PART08_SCENE_AUDIT.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 public static object TextureAudit()
 {
  var paths=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Art","Assets/Resources"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.EndsWith(".png")).ToArray();var rows=new List<object>();long estimate=0;
  foreach(var p in paths){var importer=(TextureImporter)AssetImporter.GetAtPath(p);var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(p);var android=importer.GetPlatformTextureSettings("Android");int max=android.overridden?android.maxTextureSize:importer.maxTextureSize;float scale=Mathf.Min(1,(float)max/Mathf.Max(tex.width,tex.height));int w=Mathf.Max(1,Mathf.RoundToInt(tex.width*scale)),h=Mathf.Max(1,Mathf.RoundToInt(tex.height*scale));int block=android.format==TextureImporterFormat.ASTC_6x6?6:4;long bytes=((w+block-1)/block)*((h+block-1)/block)*16L;estimate+=bytes;rows.Add(new{path=p,width=tex.width,height=tex.height,readable=importer.isReadable,mipmaps=importer.mipmapEnabled,androidOverride=android.overridden,androidFormat=android.format.ToString(),max,estimatedASTCBytes=bytes});}
  var result=new{note="All imported PNGs, including unused art. ASTC estimate excludes atlases, framebuffers, runtime overhead; not device RAM.",count=rows.Count,estimatedASTCBytes=estimate,textures=rows};File.WriteAllText("docs/PART08_TEXTURE_AUDIT.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return new{count=rows.Count,estimatedASTCBytes=estimate};
 }
 public static void OptimizeCharacterImports()
 {
  foreach(var path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Art/Characters/WizardCat"}).Select(AssetDatabase.GUIDToAssetPath)){
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);var settings=importer.GetPlatformTextureSettings("Android");settings.overridden=true;settings.maxTextureSize=2048;settings.format=TextureImporterFormat.ASTC_4x4;settings.compressionQuality=100;importer.SetPlatformTextureSettings(settings);importer.SaveAndReimport();
  }
 }
 public static void BuildAndroid()
 {
  if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
  PlayerSettings.runInBackground=false;PlayerSettings.productName="Mewgic";PlayerSettings.bundleVersion="0.8.0";PlayerSettings.Android.bundleVersionCode=8;PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.Android.buildApkPerCpuArchitecture=false;EditorUserBuildSettings.buildAppBundle=false;
  Directory.CreateDirectory("Builds/Android");AssetDatabase.SaveAssets();
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.Android,locationPathName="Builds/Android/Mewgic-0.8.0-beta.apk",options=BuildOptions.Development|BuildOptions.DetailedBuildReport});
  File.WriteAllText("docs/PART08_PACKED_ASSETS.json",Newtonsoft.Json.JsonConvert.SerializeObject(report.packedAssets.SelectMany(p=>p.contents).Select(p=>new{p.sourceAssetPath,p.packedSize}),Newtonsoft.Json.Formatting.Indented));
  var result=new{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,path=report.summary.outputPath,messages=report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>new{type=m.type.ToString(),m.content}).ToArray()};File.WriteAllText("docs/PART08_ANDROID_BUILD.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));
 }
}
