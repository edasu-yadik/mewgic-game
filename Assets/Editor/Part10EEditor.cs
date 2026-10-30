using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;
[InitializeOnLoad]
public static class Part10EEditor {
const string Output="docs/Part10E/";
static Part10EEditor(){EditorApplication.update+=Tick;}
    public static void Before(){SessionState.SetString("Part10E.Phase","Before");RunTests();}
public static void After(){SessionState.SetString("Part10E.Phase","After");RunTests();}
public static void RunTests(){Directory.CreateDirectory(Output);SessionState.SetBool("Part10E.Run",true);SessionState.SetBool("Part10E.Started",false);}
    public static void Models(){typeof(GameplayHudTests).GetMethod("UnitTests",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,null);DailyActivitiesTests.Models();RewardHubTests.Models();File.WriteAllText("docs/Part10E/MODELS.json","{\"success\":true,\"groups\":[\"GameplayHudTests.UnitTests\",\"DailyActivitiesTests.Models\",\"RewardHubTests.Models\"]}");}
    static void Tick()
    {
        if(!SessionState.GetBool("Part10E.Run",false)||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(SessionState.GetBool("Part10E.Started",false)){
            SessionState.SetBool("Part10E.Run",false);string path=Output+"UI_AUDIT.json";bool ok=File.Exists(path)&&Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)).Value<bool>("success");EditorApplication.Exit(ok?0:1);return;
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART10E Runtime Probe").AddComponent<BottleLayoutProbe>();SetGameSize();EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();SessionState.SetBool("Part10E.Started",true);EditorApplication.isPlaying=true;
    }
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART10E 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);
    }
}
