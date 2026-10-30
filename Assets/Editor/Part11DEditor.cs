using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;
using Mewgic.UI;
using Mewgic.Core;

public static class Part11DEditor
{
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        Directory.CreateDirectory("docs/ProductionReports/Part11D/");GameplayBoosterTests.ModelTests();
        string key="Mewgic.Tests.Part11D."+Guid.NewGuid().ToString("N");
        try {
            var l=GameplayProgress.Open(key);var before=l.Snapshot;
            if(!l.CompleteTutorial(PawGuide.BoosterOfferId)||!l.CompleteTutorial(PawGuide.BoosterId))throw new Exception("Rehber kaydı başarısız");
            var again=GameplayProgress.Open(key);
            if(!again.TutorialCompleted(PawGuide.BoosterId)||!again.TutorialCompleted(PawGuide.BoosterOfferId)||again.Remaining(BoosterKind.Hint)!=before.hint||again.Remaining(BoosterKind.Undo)!=before.undo||again.Remaining(BoosterKind.ExtraBottle)!=before.extra||again.Remaining(BoosterKind.Shuffle)!=before.shuffle)throw new Exception("Rehber kaydı hakları değiştirdi");
            File.WriteAllText("docs/ProductionReports/Part11D/MODELS.json","{\"success\":true,\"existingBoosterModels\":true,\"actualPlayerPrefsHistoryReload\":true,\"chargesUnchanged\":true}");
        }finally {foreach(var suffix in new[]{"",".backup",".corrupt"})PlayerPrefs.DeleteKey(key+suffix);PlayerPrefs.Save();}
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART11D QA").AddComponent<Part11DProbe>();SetGameSize();EditorApplication.isPlaying=true;
    }
    public static void Regression(){if(EditorApplication.isPlaying)throw new Exception("Play Mode durdurulmalı");Part11CProbe.Output="docs/ProductionReports/Part11D/Regression11C/";Directory.CreateDirectory(Part11CProbe.Output);EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");new GameObject("PART11C Regression").AddComponent<Part11CProbe>();SetGameSize();EditorApplication.isPlaying=true;}
    public static void Preview(){Part11CEditor.Preview();}
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART11D 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);window.Focus();
    }
}
