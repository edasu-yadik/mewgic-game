using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;
using Mewgic.Core;

public static class Part11AEditor
{
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        Directory.CreateDirectory("docs/ProductionReports/Part11A/");
        GameplayBoosterTests.ModelTests();
        var ledger=new ProgressLedger(new ProgressData(),_=>{});
        var model=PrototypeApp.LoadLevel();
        var hint=new BoosterActions(model,ledger,model.Count);
        if(!hint.Hint(out var move)||hint.Message!=""||model.Moves!=0||model.Pour(move.Source,move.Target)==0)throw new Exception("Sessiz geçerli ipucu testi başarısız");
        var blocked=new Puzzle(new[]{new[]{0,1,0,1},new[]{1,0,1,0}});var stock=ledger.Remaining(BoosterKind.Hint);
        if(new BoosterActions(blocked,ledger,2).Hint(out _)||ledger.Remaining(BoosterKind.Hint)!=stock)throw new Exception("Çözümsüz ipucu hakkı korumadı");
        File.WriteAllText("docs/ProductionReports/Part11A/MODELS.json","{\"success\":true,\"existingBoosterModels\":true,\"silentLegalHint\":true,\"unavailableHintPreservesCharge\":true}");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        new GameObject("PART11A QA").AddComponent<Part11AProbe>();SetGameSize();EditorApplication.isPlaying=true;
    }
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART11A 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);window.Focus();window.Repaint();
    }
}
