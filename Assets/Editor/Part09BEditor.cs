using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;

[InitializeOnLoad]
public static class Part09BEditor
{
    public const string Output="docs/Part09B/";
    const string Folder="Assets/Art/Environment/Home/";
    static Part09BEditor(){EditorApplication.update+=Tick;}
    static Rect R(float x,float y,float w,float h)=>new Rect(x,y,w,h);
    // Kaynak PNG'nin üst-sol köşesinden normalize edilmiş, görsel olarak denetlenmiş alanlar.
    static readonly Rect[][] Water={
        new[]{R(.15f,.145f,.065f,.14f),R(.62f,.11f,.045f,.09f),R(.44f,.26f,.065f,.11f),R(.125f,.44f,.045f,.12f),R(.40f,.47f,.055f,.12f)},
        new[]{R(.59f,.302f,.105f,.11f),R(.57f,.397f,.12f,.09f),R(.86f,.421f,.10f,.065f),R(.835f,.478f,.13f,.185f)},
        new[]{R(.225f,.535f,.105f,.155f),R(.90f,.54f,.07f,.105f)}
    };
    static readonly Rect[][] Windows={
        new[]{R(.14f,.075f,.035f,.025f),R(.61f,.205f,.04f,.035f),R(.235f,.345f,.04f,.03f),R(.085f,.27f,.035f,.035f)},
        new[]{R(.735f,.095f,.035f,.03f),R(.72f,.14f,.06f,.045f),R(.61f,.175f,.06f,.045f),R(.52f,.19f,.07f,.05f),R(.81f,.21f,.04f,.04f),R(.60f,.25f,.08f,.04f),R(.91f,.345f,.04f,.05f)},
        new[]{R(.10f,.16f,.05f,.035f),R(.075f,.24f,.05f,.04f),R(.21f,.30f,.05f,.045f),R(.075f,.315f,.08f,.06f),R(.86f,.425f,.05f,.05f),R(.18f,.40f,.05f,.04f)}
    };
    static float Region(Rect[] regions,Vector2 p)
    {
        float v=0;foreach(var r in regions){if(!r.Contains(p))continue;float edge=Mathf.Min(p.x-r.xMin,r.xMax-p.x,p.y-r.yMin,r.yMax-p.y);v=Mathf.Max(v,Mathf.Clamp01(edge/.008f));}return v;
    }
    [MenuItem("Mewgic/PART09B/Install Home motion masks")]
    public static void Install()
    {
        Directory.CreateDirectory(Output);var paths=new[]{"Layer02_DistantIslands_v001.png","Layer03_MainCastle_v001.png","Layer04_Village_v001.png"};var textures=new Texture2D[3];var rows=new List<object>();
        for(int i=0;i<paths.Length;i++)
        {
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false,true);ImageConversion.LoadImage(source,File.ReadAllBytes(Folder+paths[i]));
            var mask=new Texture2D(256,512,TextureFormat.RGBA32,false,true);mask.name="HomeMotionMask_"+i;mask.wrapMode=TextureWrapMode.Clamp;mask.filterMode=FilterMode.Bilinear;
            var pixels=new Color32[256*512];int water=0,windows=0;
            for(int y=0;y<512;y++)for(int x=0;x<256;x++)
            {
                float u=(x+.5f)/256f,v=(y+.5f)/512f;var c=source.GetPixelBilinear(u,v);var point=new Vector2(u,1-v);
                float r=Region(Water[i],point)*Mathf.Clamp01((Mathf.Min(c.g,c.b)-c.r-.04f)*7)*Mathf.Clamp01((c.b-.42f)*4)*c.a;
                float g=Region(Windows[i],point)*Mathf.Clamp01((c.r-c.b-.24f)*4)*Mathf.Clamp01((c.r-.6f)*5)*c.a;
                byte red=(byte)Mathf.RoundToInt(r*255),green=(byte)Mathf.RoundToInt(g*255);pixels[y*256+x]=new Color32(red,green,0,255);if(red>20)water++;if(green>20)windows++;
            }
            mask.SetPixels32(pixels);mask.Apply(false,false);string asset=Folder+"MotionMasks/"+Path.GetFileNameWithoutExtension(paths[i])+"_Motion.asset";
            var old=AssetDatabase.LoadAssetAtPath<Texture2D>(asset);if(old==null)AssetDatabase.CreateAsset(mask,asset);else{EditorUtility.CopySerialized(mask,old);UnityEngine.Object.DestroyImmediate(mask);mask=old;}
            textures[i]=mask;rows.Add(new{source=paths[i],mask=asset,waterPixels=water,windowPixels=windows,width=256,height=512,gpuBytes=256*512*4});
            UnityEngine.Object.DestroyImmediate(source);
            if(water<20||windows<5)throw new Exception("Maske içeriği yetersiz: "+paths[i]+" water="+water+" windows="+windows);
        }
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");var env=UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>();env.distantMotionMask=textures[0];env.castleMotionMask=textures[1];env.villageMotionMask=textures[2];
        var play=UnityEngine.Object.FindFirstObjectByType<HomePlayButtonPresenter>();play.frameSweepShader=Shader.Find("Mewgic/PlayFrameSweep");if(play.frameSweepShader==null)throw new Exception("Altın kenar shader'ı eksik");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Output+"MASK_AUDIT.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=true,method="Veri maskesi: uzamsal bölge + kaynak renk + alpha; resim yeniden üretilmedi",masks=rows},Newtonsoft.Json.Formatting.Indented));
    }
    public static void RunTests(){Directory.CreateDirectory(Output);SessionState.SetBool("Part09B.Run",true);SessionState.SetBool("Part09B.Started",false);}
    static void Tick()
    {
        if(!SessionState.GetBool("Part09B.Run",false)||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(SessionState.GetBool("Part09B.Started",false)){
            SessionState.SetBool("Part09B.Run",false);string path=Output+"HOME_MOTION_TESTS.json";bool ok=File.Exists(path)&&Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path)).Value<bool>("success");EditorApplication.Exit(ok?0:1);return;
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");new GameObject("PART09B Runtime Probe").AddComponent<HomeMotionProbe>();SetGameSize();EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();SessionState.SetBool("Part09B.Started",true);EditorApplication.isPlaying=true;
    }
    static void SetGameSize()
    {
        var a=typeof(Editor).Assembly;var type=a.GetType("UnityEditor.GameViewSizes");var sizes=typeof(ScriptableSingleton<>).MakeGenericType(type).GetProperty("instance").GetValue(null);var groupType=type.GetProperty("currentGroupType",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(sizes);var group=type.GetMethod("GetGroup").Invoke(sizes,new[]{groupType});var kind=a.GetType("UnityEditor.GameViewSizeType");var size=Activator.CreateInstance(a.GetType("UnityEditor.GameViewSize"),new object[]{Enum.ToObject(kind,1),900,1600,"PART09B 900x1600"});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);var window=EditorWindow.GetWindow(a.GetType("UnityEditor.GameView"));window.GetType().GetProperty("selectedSizeIndex",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(window,count-1);
    }
}
