using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;

[InitializeOnLoad]
public static class Part11BEditor
{
    const string Output="docs/ProductionReports/Part11B/";
    static Part11BEditor(){EditorApplication.update+=Tick;}
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        Directory.CreateDirectory(Output);GameplayBoosterTests.ModelTests();
        SessionState.SetString("Part11B.Key","Mewgic.Tests.Part11B."+Guid.NewGuid().ToString("N"));
        SessionState.SetInt("Part11B.Phase",1);SessionState.SetBool("Part11B.Run",true);
        Launch(false);
    }
    static void Launch(bool resume)
    {
        EditorSceneManager.OpenScene(resume?"Assets/Scenes/Gameplay.unity":"Assets/Scenes/Home.unity");
        var probe=new GameObject("PART11B QA").AddComponent<Part11BProbe>();probe.verifyCompletedOnReload=resume;
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART11B 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);window.Focus();EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool("Part11B.Run",false)||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        int phase=SessionState.GetInt("Part11B.Phase",0);string path=Output+(phase==1?"PHASE1.json":"TESTS.json");
        if(!File.Exists(path))return;
        bool ok=Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)).Value<bool>("success");
        if(phase==1&&ok){SessionState.SetInt("Part11B.Phase",2);Launch(true);}else SessionState.SetBool("Part11B.Run",false);
    }
    public static void Preview()
    {
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        new GameObject("PART11B Safe Preview").AddComponent<Part11BProbe>().previewOnly=true;
        EditorApplication.isPlaying=true;
    }
}
