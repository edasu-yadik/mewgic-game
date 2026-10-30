using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;

public static class WaterfallEditor
{
    public const string Output = "docs/ProductionReports/Waterfalls/";
    public static void Install()
    {
        if (EditorApplication.isPlaying) throw new Exception("Önce Play Mode durdurulmalı.");
        Directory.CreateDirectory(Output);
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        ImageConversion.LoadImage(source, File.ReadAllBytes("Assets/Art/Environment/Gameplay/Gameplay_Backdrop_v001.png"));
        // Source artwork, top-left normalized coordinates. Blue-water qualification
        // excludes vegetation and stone inside these narrow waterfall envelopes.
        var regions = new[] {
            new Rect(.102f,.211f,.047f,.105f),new Rect(.326f,.280f,.038f,.083f),
            new Rect(.808f,.266f,.040f,.071f),new Rect(.735f,.392f,.061f,.085f),
            new Rect(.582f,.413f,.067f,.057f),new Rect(.207f,.429f,.025f,.064f),
            new Rect(.416f,.471f,.035f,.071f),new Rect(.739f,.496f,.034f,.066f),
            new Rect(.274f,.555f,.046f,.060f),new Rect(.377f,.564f,.035f,.057f),
            new Rect(.614f,.570f,.143f,.047f),new Rect(.479f,.652f,.178f,.043f),
            new Rect(.424f,.791f,.203f,.036f)
        };
        var mask = new Texture2D(256,512,TextureFormat.RGBA32,false,true) {name="Gameplay_WaterMask",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
        var pixels=new Color32[256*512];int water=0;
        for(int y=0;y<512;y++)for(int x=0;x<256;x++) {
            float u=(x+.5f)/256,v=(y+.5f)/512;var point=new Vector2(u,1-v);float envelope=0;
            foreach(var region in regions)if(region.Contains(point)) {
                float edge=Mathf.Min(point.x-region.xMin,region.xMax-point.x,point.y-region.yMin,region.yMax-point.y);
                envelope=Mathf.Max(envelope,Mathf.Clamp01(edge/.004f));
            }
            var c=source.GetPixelBilinear(u,v);
            float value=envelope*Mathf.Clamp01((Mathf.Min(c.g,c.b)-c.r-.025f)*9)*Mathf.Clamp01((c.b-.40f)*5);
            byte red=(byte)Mathf.RoundToInt(value*255);if(red>20)water++;
            pixels[y*256+x]=new Color32(red,0,0,255);
        }
        mask.SetPixels32(pixels);mask.Apply(false,false);
        const string path="Assets/Art/Environment/Gameplay/Gameplay_WaterMask.asset";
        var old=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if(old==null)AssetDatabase.CreateAsset(mask,path);else {EditorUtility.CopySerialized(mask,old);UnityEngine.Object.DestroyImmediate(mask);mask=old;}
        File.WriteAllBytes(Output+"Gameplay_Mask.png",mask.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(source);
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        var presentation=UnityEngine.Object.FindFirstObjectByType<GameplayPresentation>();
        presentation.waterfallShader=Shader.Find("Mewgic/HomeWaterLight");presentation.waterfallMask=mask;
        if(presentation.waterfallShader==null||water<100)throw new Exception("Su shader/maske doğrulaması başarısız");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Output+"MASK.json",Newtonsoft.Json.JsonConvert.SerializeObject(new {waterPixels=water,width=256,height=512,bytes=256*512*4,regions=regions.Select(r=>new {r.x,r.y,r.width,r.height})},Newtonsoft.Json.Formatting.Indented));
    }
    public static void Run()
    {
        Directory.CreateDirectory(Output);
        if(EditorApplication.isPlaying)throw new Exception("Önce Play Mode durdurulmalı.");
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
        new GameObject("Waterfall QA").AddComponent<WaterfallProbe>();
        EditorApplication.isPlaying=true;
    }
}
