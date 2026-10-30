using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;

public static class Part11CEditor
{
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        Directory.CreateDirectory("docs/ProductionReports/Part11C/");GameplayBoosterTests.ModelTests();
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART11C QA").AddComponent<Part11CProbe>();SetGameSize();EditorApplication.isPlaying=true;
    }
    public static void Preview()
    {
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART11C Safe Preview").AddComponent<Part10PreviewGuard>();SetGameSize();EditorApplication.isPlaying=true;
    }
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART11C 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);window.Focus();
    }
}
