using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mewgic;
using Mewgic.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MewgicPrototypeSetup
{
    static int checks;
    static void Assert(bool ok,string name){if(!ok)throw new Exception("FAILED: "+name);checks++;}
    [MenuItem("Mewgic/Run Model Tests")]
    public static void TestModel()
    {
        checks=0;
        var p=new Puzzle(new[]{new[]{0,1,1},new[]{1,1,1},new int[0],new[]{0}});
        Assert(p.Pour(0,0)==0&&p.Pour(-1,2)==0&&p.Moves==0,"invalid indices and same bottle");
        Assert(p.Pour(0,3)==0&&p.Moves==0,"different color");
        Assert(p.Pour(0,1)==1&&p.Bottle(0).Count==2&&p.Bottle(1).Count==4,"partial capacity");
        Assert(p.Pour(0,1)==0&&p.Moves==1,"full target immutable");
        Assert(p.Undo()&&p.Moves==0&&p.Bottle(0).Count==3&&p.Bottle(1).Count==3,"undo exact state");
        Assert(p.Pour(0,2)==2&&p.Bottle(2).SequenceEqual(new[]{1,1}),"contiguous group to empty");
        p.Restart();Assert(p.Moves==0&&!p.CanUndo&&p.Bottle(2).Count==0,"restart clears history");
        Assert(!p.Undo(),"undo empty history");
        Assert(!new Puzzle(new[]{new[]{0},new int[0]}).Won,"partial monochrome not won");
        Assert(new Puzzle(new[]{new[]{0,0,0,0},new int[0]}).Won,"full monochrome won");
        bool rejected=false;try{new Puzzle(new[]{new[]{0,0,0,0,0},new int[0]});}catch(ArgumentException){rejected=true;}
        Assert(rejected,"oversize rejected");
        var level=PrototypeApp.LoadLevel();
        var queue=new Queue<Tuple<int[][],string>>();
        queue.Enqueue(Tuple.Create(level.Export(),""));var seen=new HashSet<string>();string solution=null;
        while(queue.Count>0&&seen.Count<50000)
        {
            var item=queue.Dequeue();string key=string.Join("|",item.Item1.Select(b=>string.Join(",",b)));
            if(!seen.Add(key))continue;
            var current=new Puzzle(item.Item1);
            if(current.Won){solution=item.Item2;break;}
            for(int s=0;s<current.Count;s++)for(int t=0;t<current.Count;t++)
            {var next=new Puzzle(item.Item1);if(next.Pour(s,t)>0)queue.Enqueue(Tuple.Create(next.Export(),item.Item2+$"{s+1}>{t+1} "));}
        }
        Assert(solution!=null,"starter solvable by BFS");
        File.WriteAllText("docs/MODEL_TEST_RESULTS.md",$"# Model testleri\n\n{checks} kontrol başarılı.\n\nBölüm çözümü (şişeler 1 tabanlı): {solution}\nZiyaret edilen durum: {seen.Count}\n");
        Debug.Log("MEWGIC_MODEL_TESTS_OK "+checks+" solution="+solution);
    }
    [MenuItem("Mewgic/Configure Prototype Scenes")]
    public static void Configure()
    {
        TestModel();
        foreach(var name in new[]{"Boot","Home","Gameplay"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
            if(!scene.GetRootGameObjects().Any(g=>g.GetComponent<PrototypeApp>()!=null))
                new GameObject("Mewgic Prototype",typeof(PrototypeApp));
            EditorSceneManager.SaveScene(scene);
        }
        MewgicFoundationSetup.Validate();
        EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        AssetDatabase.SaveAssets();
        File.WriteAllText("docs/prototype-configured.txt","Configured and model tests passed");
    }
    [MenuItem("Mewgic/Build Android Prototype")]
    public static void Build()
    {
        var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{"Boot","Home","Gameplay","LevelMap"}.Select(n=>"Assets/Scenes/"+n+".unity").ToArray(),
            locationPathName="Builds/Android/Mewgic-Prototype.apk",target=BuildTarget.Android,options=BuildOptions.Development});
        if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Prototype build: "+result.summary.result);
        File.WriteAllText("docs/prototype-build-success.txt",DateTime.UtcNow.ToString("O")+" bytes="+result.summary.totalSize);
        Debug.Log("MEWGIC_PROTOTYPE_BUILD_OK");
    }
}


