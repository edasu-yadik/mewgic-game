using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Mewgic;

public static class HomeEnvironmentChecks
{
    static void Check(bool value,string label,List<string> checks)
    {if(!value)throw new Exception(label);checks.Add(label);}
    public static string Current()
    {
        var checks=new List<string>();var env=UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>();
        Check(env!=null&&env.Mounted&&env.LayerCount==6,"Six mounted independent environment layers",checks);
        env.Layout();Canvas.ForceUpdateCanvases();
        var home=(RectTransform)env.transform;var world=home.Find("WorldDecorations/EnvironmentComposition");
        Check(!home.Find("Background/Night").gameObject.activeSelf,"Old flat night disabled",checks);
        foreach(var graphic in home.Find("WorldDecorations").GetComponentsInChildren<Graphic>())
            Check(!graphic.raycastTarget,"Environment does not intercept input: "+graphic.name,checks);
        foreach(var name in new[]{"Layer02_DistantIslands","Layer03_MainCastle","Layer04_Village"})
        {
            var image=world.Find(name).GetComponent<Image>();
            Check(image.material.shader==env.waterLightShader&&env.waterLightShader.isSupported,"Water/window shader supported: "+name,checks);
            var tex=new Texture2D(2,2);ImageConversion.LoadImage(tex,File.ReadAllBytes(AssetDatabase.GetAssetPath(image.sprite)));
            try
            {
                int xmin=tex.width,ymin=tex.height,xmax=0,ymax=0;var pixels=tex.GetPixels32();
                for(int y=0;y<tex.height;y+=3)for(int x=0;x<tex.width;x+=3)
                    if(pixels[y*tex.width+x].a>128){xmin=Math.Min(xmin,x);ymin=Math.Min(ymin,y);xmax=Math.Max(xmax,x);ymax=Math.Max(ymax,y);}
                var r=image.rectTransform.rect;
                var a=new Vector2(r.xMin+r.width*xmin/tex.width,r.yMin+r.height*ymin/tex.height);
                var b=new Vector2(r.xMin+r.width*xmax/tex.width,r.yMin+r.height*ymax/tex.height);
                a=home.InverseTransformPoint(image.rectTransform.TransformPoint(a));b=home.InverseTransformPoint(image.rectTransform.TransformPoint(b));
                Check(home.rect.Contains(a)&&home.rect.Contains(b),"Opaque architecture fits viewport: "+name,checks);
                Check(Mathf.Approximately(image.rectTransform.rect.width/image.rectTransform.rect.height,(float)tex.width/tex.height),"Artwork aspect ratio preserved: "+name,checks);
            }
            finally{UnityEngine.Object.DestroyImmediate(tex);}
        }
        var island=world.Find("Layer02_DistantIslands");var castle=world.Find("Layer03_MainCastle");
        var original=island.localPosition;var castleOriginal=castle.localPosition;island.localPosition+=Vector3.right*5;
        Check(castle.localPosition==castleOriginal,"Layer movement is independent",checks);island.localPosition=original;
        Check(world.Find("Layer06_Atmosphere").childCount==17,"One mist plane and exactly 16 pooled environment lights",checks);
        return JsonUtility.ToJson(new Result{width=Screen.width,height=Screen.height,checks=checks.ToArray()},true);
    }
    [Serializable] class Result{public int width,height;public string[] checks;}
    public static void MotionBaseline()
    {
        var env=UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>();
        var root=env.transform.Find("WorldDecorations/EnvironmentComposition/Layer06_Atmosphere");
        SessionState.SetFloat("EnvironmentFogX",root.Find("DriftingMist").localPosition.x);
        SessionState.SetFloat("EnvironmentMoteY",root.Find("GoldMote_0").localPosition.y);
        SessionState.SetFloat("EnvironmentTime",env.AnimationTime);
    }
    public static string MotionResult()
    {
        var env=UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>();
        var root=env.transform.Find("WorldDecorations/EnvironmentComposition/Layer06_Atmosphere");
        var checks=new List<string>();
        Check(env.AnimationTime>SessionState.GetFloat("EnvironmentTime",0),"Environment animation time advances",checks);
        Check(Mathf.Abs(root.Find("DriftingMist").localPosition.x-SessionState.GetFloat("EnvironmentFogX",0))>.001f,"Mist drifts over time",checks);
        Check(Mathf.Abs(root.Find("GoldMote_0").localPosition.y-SessionState.GetFloat("EnvironmentMoteY",0))>.001f,"Gold light moves over time",checks);
        return JsonUtility.ToJson(new Result{width=Screen.width,height=Screen.height,checks=checks.ToArray()},true);
    }
    public static string ShaderMotion()
    {
        var env=UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>();
        var material=env.transform.Find("WorldDecorations/EnvironmentComposition/Layer03_MainCastle").GetComponent<Image>().material;
        var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();var camera=Camera.main;
        var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldDistance=canvas.planeDistance;
        var oldTarget=camera.targetTexture;var previous=RenderTexture.active;
        var target=new RenderTexture(450,800,24);var texture=new Texture2D(450,800,TextureFormat.RGB24,false);
        float oldTime=material.GetFloat("_EnvironmentTime");
        try
        {
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=target;
            Canvas.ForceUpdateCanvases();UnityEngine.Object.FindFirstObjectByType<HomeCharacterPresenter>().Layout();env.Layout();Canvas.ForceUpdateCanvases();
            material.SetFloat("_EnvironmentTime",0);camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,450,800),0,0);texture.Apply();var first=texture.GetPixels32();
            material.SetFloat("_EnvironmentTime",2);camera.Render();texture.ReadPixels(new Rect(0,0,450,800),0,0);texture.Apply();var second=texture.GetPixels32();
            int water=0,windows=0;
            for(int i=0;i<first.Length;i++)
            {
                var a=first[i];var b=second[i];int difference=Math.Abs(a.r-b.r)+Math.Abs(a.g-b.g)+Math.Abs(a.b-b.b);
                if(difference==0)continue;
                if(a.g>a.r+15&&a.b>a.r+15&&a.b>130)water++;
                if(a.r>a.b+55&&a.r>160)windows++;
            }
            if(water<20||windows<20)throw new Exception("Insufficient rendered water/window motion: "+water+" / "+windows);
            return "Real Home render, two shader phases, other scene transforms frozen: cyan water changed pixels="+water+", amber window/light changed pixels="+windows;
        }
        finally
        {
            material.SetFloat("_EnvironmentTime",oldTime);camera.targetTexture=oldTarget;canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;RenderTexture.active=previous;
            UnityEngine.Object.DestroyImmediate(texture);target.Release();UnityEngine.Object.DestroyImmediate(target);
            Canvas.ForceUpdateCanvases();env.Layout();UnityEngine.Object.FindFirstObjectByType<HomeCharacterPresenter>().Layout();
        }
    }
}
