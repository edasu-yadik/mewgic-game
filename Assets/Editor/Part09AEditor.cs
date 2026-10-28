using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;

[InitializeOnLoad]
public static class Part09AEditor
{
    const string Output="docs/Part09A/";
    static Part09AEditor(){EditorApplication.update+=Tick;}
    [MenuItem("Mewgic/PART09A/Install Opening")]
    public static void Install()
    {
        if(Application.unityVersion!="6000.3.27f1")throw new Exception("Ana proje Editor sürümü korunmalıdır.");
        Directory.CreateDirectory(Output);
        string art="Assets/Art/Boot/Mewgic_Startup_Background_v001.png";
        AssetDatabase.ImportAsset(art,ImportAssetOptions.ForceSynchronousImport);
        var imp=(TextureImporter)AssetImporter.GetAtPath(art);imp.textureType=TextureImporterType.Default;imp.mipmapEnabled=false;imp.sRGBTexture=true;imp.alphaSource=TextureImporterAlphaSource.None;imp.maxTextureSize=2048;imp.npotScale=TextureImporterNPOTScale.None;imp.wrapMode=TextureWrapMode.Clamp;imp.textureCompression=TextureImporterCompression.CompressedHQ;
        foreach(string platform in new[]{"Android","iPhone"}){var p=imp.GetPlatformTextureSettings(platform);p.overridden=true;p.maxTextureSize=2048;p.format=TextureImporterFormat.ASTC_6x6;p.compressionQuality=100;imp.SetPlatformTextureSettings(p);}imp.SaveAndReimport();
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        foreach(var app in UnityEngine.Object.FindObjectsByType<PrototypeApp>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(app);
        var boot=UnityEngine.Object.FindFirstObjectByType<BootLoadingController>();if(boot==null)boot=new GameObject("Mewgic Startup").AddComponent<BootLoadingController>();
        boot.background=AssetDatabase.LoadAssetAtPath<Texture2D>(art);
        boot.logo=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/Logo/Mewgic_Logo_Main.png");
        boot.character=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Characters/MageCat/MageCat_FullBody_v001.png");
        if(boot.background==null||boot.logo==null||boot.character==null)throw new Exception("Onaylı açılış asset'i eksik.");
        boot.homeScene="Home";EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        // Unity 6 Personal: resmî izin; lisans veya splash çalışma zamanı atlatması yok.
        PlayerSettings.SplashScreen.showUnityLogo=false;PlayerSettings.SplashScreen.show=false;
        AssetDatabase.SaveAssets();
        File.WriteAllText(Output+"SETUP.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=true,editor=Application.unityVersion,license="Unity Personal (CLI license list)",splashOptionalInUnity6=true,showSplash=PlayerSettings.SplashScreen.show,showUnityLogo=PlayerSettings.SplashScreen.showUnityLogo,firstScene=EditorBuildSettings.scenes.First(s=>s.enabled).path},Newtonsoft.Json.Formatting.Indented));
    }
    public static void Models()
    {
        MewgicPrototypeSetup.TestModel();BetaValidation.SaveTests();MapProgressionTests.Models();GameplayBoosterTests.ModelTests();DailyActivitiesTests.Models();RewardHubTests.Models();
        foreach(var n in new[]{"Boot","Home","Gameplay","LevelMap"}){var s=EditorSceneManager.OpenScene("Assets/Scenes/"+n+".unity");if(s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Component>(true)).Any(c=>c==null))throw new Exception("Eksik script: "+n);}
        File.WriteAllText(Output+"MODELS.json","{\"success\":true,\"groups\":6,\"scenes\":4}");
    }
    public static void RunTests(){SessionState.SetInt("Part09A.Index",0);SessionState.SetBool("Part09A.Run",true);SessionState.SetBool("Part09A.Wait",false);}
    static void Tick()
    {
        if(!SessionState.GetBool("Part09A.Run",false)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(SessionState.GetBool("Part09A.Wait",false)){
            var path=Output+"scenario-"+SessionState.GetInt("Part09A.Index",0)+".json";
            if(!File.Exists(path)||!Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)).Value<bool>("success")){SessionState.SetBool("Part09A.Run",false);EditorApplication.Exit(1);return;}
            SessionState.SetInt("Part09A.Index",SessionState.GetInt("Part09A.Index",0)+1);
        }
        int index=SessionState.GetInt("Part09A.Index",0);
        if(index>=5){SessionState.SetBool("Part09A.Run",false);EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");EditorApplication.Exit(0);return;}
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");var probe=new GameObject("PART09A Test Probe").AddComponent<BootScenarioProbe>();probe.scenario=index;
        SetGameSize(900,index==1?1950:index==2?2000:1600);
        EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        SessionState.SetBool("Part09A.Wait",true);EditorApplication.isPlaying=true;
    }
    static void SetGameSize(int width,int height)
    {
        var assembly=typeof(Editor).Assembly;var sizesType=assembly.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);var sizes=singleton.GetProperty("instance").GetValue(null);
        var groupType=sizesType.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var sizeType=assembly.GetType("UnityEditor.GameViewSize");var kind=assembly.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(sizeType,new object[]{Enum.ToObject(kind,1),width,height,"PART09A "+width+"x"+height});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);
    }
}
