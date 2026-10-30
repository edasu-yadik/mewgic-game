using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;
[InitializeOnLoad]
public static class Part09EEditor {
const string Output="docs/Part09E/";
static Part09EEditor(){EditorApplication.update+=Tick;}
    public static void Before(){SessionState.SetString("Part09E.Phase","Before");RunTests();}
public static void After(){SessionState.SetString("Part09E.Phase","After");RunTests();}
public static void RunTests(){Directory.CreateDirectory(Output);SessionState.SetBool("Part09E.Run",true);SessionState.SetBool("Part09E.Started",false);}
    static void Tick()
    {
        if(!SessionState.GetBool("Part09E.Run",false)||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(SessionState.GetBool("Part09E.Started",false)){
            SessionState.SetBool("Part09E.Run",false);string path=Output+"UI_AUDIT.json";bool ok=File.Exists(path)&&Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)).Value<bool>("success");EditorApplication.Exit(ok?0:1);return;
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");new GameObject("PART09E Runtime Probe").AddComponent<HomeEventsAuditProbe>();SetGameSize();EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();SessionState.SetBool("Part09E.Started",true);EditorApplication.isPlaying=true;
    }
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART09E 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);
    }
}
