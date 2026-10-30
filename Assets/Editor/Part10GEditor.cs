using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;
[InitializeOnLoad]
public static class Part10GEditor {
const string Output="docs/ProductionReports/Part10/";
static Part10GEditor(){EditorApplication.update+=Tick;}
    public static void Before(){SessionState.SetString("Part10G.Phase","Before");RunTests();}
public static void After(){SessionState.SetString("Part10G.Phase","After");RunTests();}
public static void RunTests(){Directory.CreateDirectory(Output);SessionState.SetBool("Part10G.Run",true);SessionState.SetBool("Part10G.Started",false);}
    public static void Models(){typeof(GameplayHudTests).GetMethod("UnitTests",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,null);DailyActivitiesTests.Models();RewardHubTests.Models();File.WriteAllText("docs/ProductionReports/Part10/MODELS.json","{\"success\":true,\"groups\":[\"GameplayHudTests.UnitTests\",\"DailyActivitiesTests.Models\",\"RewardHubTests.Models\"]}");}
    static void Tick()
    {
        if(!SessionState.GetBool("Part10G.Run",false)||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(SessionState.GetBool("Part10G.Started",false)){
            SessionState.SetBool("Part10G.Run",false);string path=Output+"UI_AUDIT.json";bool ok=File.Exists(path)&&Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)).Value<bool>("success");if(ok)Preview();else EditorApplication.Exit(1);return;
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART10G Runtime Probe").AddComponent<Part10RegressionProbe>();SetGameSize();EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();SessionState.SetBool("Part10G.Started",true);EditorApplication.isPlaying=true;
    }
    public static void Preview(){EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART10 Safe Preview").AddComponent<Part10PreviewGuard>();SetGameSize();EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();EditorApplication.isPlaying=true;EditorApplication.delayCall+=PreviewStatus;}
    static void PreviewStatus(){if(!EditorApplication.isPlaying||!GameplayProgress.IsTestProfile||UnityEngine.Object.FindFirstObjectByType<GameplayHud>()==null){EditorApplication.delayCall+=PreviewStatus;return;}File.WriteAllText(Output+"PREVIEW.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{editorPlayMode=EditorApplication.isPlaying,scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,testProfile=GameplayProgress.IsTestProfile,unity=Application.unityVersion}));EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();}
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART10G 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);
    }
}
