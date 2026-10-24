using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Mewgic.Characters;

public static class WizardCatAssetBuilder
{
    const string Art="Assets/Art/Characters/WizardCat/";
    const string Anim="Assets/Animations/WizardCat/";
    const string Prefab="Assets/Prefabs/Characters/WizardCat.prefab";
    public const string Scene="Assets/Scenes/WizardCatAnimationTest.unity";
    [MenuItem("Mewgic/Wizard Cat/Build Android Animation Test")]
    public static void BuildAndroidTest()
    {
        try
        {
            Directory.CreateDirectory("Builds/Android");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{Scene},locationPathName="Builds/Android/Mewgic-WizardCat-Test.apk",
                target=BuildTarget.Android,options=BuildOptions.Development});
            bool ok=report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded;
            File.WriteAllText("docs/PART02B_ANDROID_BUILD.json",JsonUtility.ToJson(new AndroidResult{
                success=ok,result=report.summary.result.ToString(),errors=(int)report.summary.totalErrors,
                warnings=(int)report.summary.totalWarnings,path="Builds/Android/Mewgic-WizardCat-Test.apk"},true));
            if(!ok)Debug.LogError("WizardCat Android test build failed");
        }
        catch(Exception e){File.WriteAllText("docs/PART02B_ANDROID_BUILD.json",JsonUtility.ToJson(new AndroidResult{success=false,result=e.ToString()},true));Debug.LogException(e);}
    }
    [Serializable] class AndroidResult{public bool success;public string result;public int errors,warnings;public string path;}
    static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos)
    {
        var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        if(parent!=null)rt.SetParent(parent,false);
        rt.anchorMin=rt.anchorMax=new Vector2(.5f,0);rt.pivot=new Vector2(.5f,0);
        rt.sizeDelta=size;rt.anchoredPosition=pos;return rt;
    }
    static Image Image(Transform parent,string name,Sprite sprite,Vector2 size,Vector2 pos)
    {
        var rt=Rect(parent,name,size,pos);var im=rt.gameObject.AddComponent<Image>();
        im.sprite=sprite;im.raycastTarget=false;im.preserveAspect=true;return im;
    }
    static Text Text(Transform parent,string name,string value,int size,Vector2 min,Vector2 max)
    {
        var rt=Rect(parent,name,Vector2.zero,Vector2.zero);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var t=rt.gameObject.AddComponent<Text>();t.font=Font;t.text=value;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(1,.85f,.55f);t.raycastTarget=false;
        return t;
    }
    static Button Button(Transform parent,string name,string label,Vector2 min,Vector2 max)
    {
        var rt=Rect(parent,name,Vector2.zero,Vector2.zero);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var im=rt.gameObject.AddComponent<Image>();im.color=new Color(.21f,.12f,.36f);
        var outline=rt.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.95f,.75f,.4f);outline.effectDistance=new Vector2(2,-2);
        var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=im;
        Text(rt,"Label",label,26,new Vector2(.03f,.05f),new Vector2(.97f,.95f));return b;
    }
    static void Import(string path)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var i=(TextureImporter)AssetImporter.GetAtPath(path);
        i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=100;
        i.alphaIsTransparency=true;i.mipmapEnabled=false;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=2048;
        i.textureCompression=TextureImporterCompression.Uncompressed;i.filterMode=FilterMode.Bilinear;i.wrapMode=TextureWrapMode.Clamp;
        var s=new TextureImporterSettings();i.ReadTextureSettings(s);s.spriteAlignment=9;s.spritePivot=new Vector2(.5f,0);s.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(s);
        i.SaveAndReimport();
    }
    static AnimationClip Clip(string name,bool loop)
    {
        var path=Anim+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null){clip=new AnimationClip{name=name,frameRate=60};AssetDatabase.CreateAsset(clip,path);}
        else clip.ClearCurves();
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);return clip;
    }
    static void Curve(AnimationClip clip,string path,Type type,string property,params float[] values)
    {
        var keys=new Keyframe[values.Length/2];for(int i=0;i<keys.Length;i++)keys[i]=new Keyframe(values[i*2],values[i*2+1]);
        var c=new AnimationCurve(keys);for(int i=0;i<keys.Length;i++){AnimationUtility.SetKeyLeftTangentMode(c,i,AnimationUtility.TangentMode.ClampedAuto);AnimationUtility.SetKeyRightTangentMode(c,i,AnimationUtility.TangentMode.ClampedAuto);}
        clip.SetCurve(path,type,property,c);
    }
    static Sprite Eye(Texture2D texture,Rect rect,string name)
    {
        var path=Art+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null)return existing;
        var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static AnimatorController Controller(bool blink)
    {
        string path=Anim+"WizardCat.controller";
        var existing=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if(existing!=null)return existing;
        var idle=Clip("Idle",true);
        Curve(idle,"BreathingRig",typeof(Transform),"m_LocalScale.x",0,1,1.8f,.998f,3.6f,1);
        Curve(idle,"BreathingRig",typeof(Transform),"m_LocalScale.y",0,1,1.8f,1.006f,3.6f,1);
        Curve(idle,"BreathingRig",typeof(Transform),"m_LocalPosition.y",0,0,1.8f,2.2f,3.6f,0);
        var rest=Clip("WandRest",true);
        var magic=Clip("WandMagic",false);
        foreach(var part in new[]{"WandBackPivot","WandFrontPivot"})
        {
            Curve(rest,"BreathingRig/"+part,typeof(Transform),"localEulerAnglesRaw.z",0,0,1.6f,0);
            Curve(magic,"BreathingRig/"+part,typeof(Transform),"localEulerAnglesRaw.z",0,0,.35f,7,.7f,4,1.2f,0,1.6f,0);
        }
        Curve(rest,"BreathingRig/WandFrontPivot/TipGlow",typeof(CanvasGroup),"m_Alpha",0,0,1.6f,0);
        Curve(magic,"BreathingRig/WandFrontPivot/TipGlow",typeof(CanvasGroup),"m_Alpha",0,0,.3f,1,.65f,.6f,1.25f,0,1.6f,0);
        var controller=AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Magic",AnimatorControllerParameterType.Trigger);controller.AddParameter("Blink",AnimatorControllerParameterType.Trigger);
        var state=controller.layers[0].stateMachine.AddState("Idle");state.motion=idle;state.writeDefaultValues=false;controller.layers[0].stateMachine.defaultState=state;
        AddLayer(controller,"Wand Magic",rest,magic,"Magic");
        var blinkRest=Clip("BlinkRest",true);var blinkClip=Clip("Blink",false);
        if(blink){Curve(blinkRest,"BreathingRig/BlinkOverlay",typeof(CanvasGroup),"m_Alpha",0,0,1,0);Curve(blinkClip,"BreathingRig/BlinkOverlay",typeof(CanvasGroup),"m_Alpha",0,0,.065f,1,.13f,1,.22f,0,.28f,0);}
        AddLayer(controller,"Blink",blinkRest,blinkClip,"Blink");
        EditorUtility.SetDirty(controller);return controller;
    }
    static void AddLayer(AnimatorController c,string name,AnimationClip rest,AnimationClip action,string trigger)
    {
        c.AddLayer(name);var layers=c.layers;var layer=layers[layers.Length-1];layer.defaultWeight=1;c.layers=layers;
        var a=layer.stateMachine.AddState("Rest");a.motion=rest;a.writeDefaultValues=false;layer.stateMachine.defaultState=a;
        var b=layer.stateMachine.AddState(trigger);b.motion=action;b.writeDefaultValues=false;
        var enter=a.AddTransition(b);enter.hasExitTime=false;enter.duration=.03f;enter.AddCondition(AnimatorConditionMode.If,0,trigger);
        var exit=b.AddTransition(a);exit.hasExitTime=true;exit.exitTime=1;exit.duration=.03f;
    }
    static RectTransform Wand(Transform parent,string name,Sprite sprite)
    {
        var pivot=Rect(parent,name,Vector2.zero,new Vector2(376,582));
        if(name=="WandBackPivot")WandImage(pivot,sprite,Vector2.zero);
        else {
            var mask=Rect(pivot,"HandOcclusionMask",new Vector2(850,430),new Vector2(0,65));mask.gameObject.AddComponent<RectMask2D>();
            WandImage(mask,sprite,new Vector2(0,-65));
            var glow=Rect(pivot,"TipGlow",new Vector2(190,190),new Vector2(39,193));glow.pivot=new Vector2(.5f,.5f);
            var graphic=glow.gameObject.AddComponent<WizardSparkGraphic>();graphic.raycastTarget=false;graphic.color=new Color(1,.85f,.35f);
            glow.gameObject.AddComponent<CanvasGroup>().alpha=0;
        }
        return pivot;
    }
    static void WandImage(Transform parent,Sprite sprite,Vector2 pos)
    {
        var im=Image(parent,"WandArt",sprite,new Vector2(1122,1402),pos);
        im.rectTransform.pivot=new Vector2(455f/1122f,1-900f/1402f);im.rectTransform.localScale=Vector3.one*.34f;im.rectTransform.localRotation=Quaternion.Euler(0,0,19);
    }
    [MenuItem("Mewgic/Wizard Cat/Build Animation Assets")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit Play Mode first");
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new Exception("Save pending scene changes before rebuilding character assets.");
        foreach(var n in new[]{"MageCat_FullBody_v001.png","MageCat_NoWand_v001.png","MageCat_Wand_v001.png","WizardCat_ClosedEyePatches_v001.png"})Import(Art+n);
        var body=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"MageCat_NoWand_v001.png");var wand=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"MageCat_Wand_v001.png");
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"WizardCat_ClosedEyePatches_v001.png");
        var left=Eye(texture,new Rect(290,1402-837,325,275),"BlinkLeft");
        var right=Eye(texture,new Rect(620,1402-744,278,244),"BlinkRight");
        var root=Rect(null,"WizardCat",new Vector2(1122,1402),Vector2.zero);
        try {
            var rig=Rect(root,"BreathingRig",root.sizeDelta,Vector2.zero);
            Wand(rig,"WandBackPivot",wand);
            Image(rig,"Body",body,root.sizeDelta,Vector2.zero);
            var lids=Rect(rig,"BlinkOverlay",root.sizeDelta,Vector2.zero);var cg=lids.gameObject.AddComponent<CanvasGroup>();cg.alpha=0;cg.blocksRaycasts=false;cg.interactable=false;
            var eye=Image(lids,"LeftClosedEye",left,new Vector2(147,124),new Vector2(512-561,1402-639));eye.rectTransform.pivot=new Vector2(.5f,.5f);
            eye=Image(lids,"RightClosedEye",right,new Vector2(133,117),new Vector2(704-561,1402-588));eye.rectTransform.pivot=new Vector2(.5f,.5f);
            var front=Wand(rig,"WandFrontPivot",wand);
            var fx=Rect(rig,"MagicFX",root.sizeDelta,Vector2.zero);
            var animator=root.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=Controller(true);animator.updateMode=AnimatorUpdateMode.UnscaledTime;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var presenter=root.gameObject.AddComponent<WizardCatPresentation>();presenter.animator=animator;presenter.magicOrigin=(RectTransform)front.Find("TipGlow");presenter.fxRoot=fx;presenter.blinkOverlay=cg;
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,Prefab);
        }finally{UnityEngine.Object.DestroyImmediate(root.gameObject);}
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var cam=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.tag="MainCamera";cam.transform.position=new Vector3(0,0,-10);cam.orthographic=true;cam.orthographicSize=8;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.055f,.065f,.12f);
        var canvas=new GameObject("Character Test Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(900,1600);scaler.matchWidthOrHeight=.5f;
        var bg=Image(canvas.transform,"Background",null,Vector2.zero,Vector2.zero);bg.color=cam.backgroundColor;bg.rectTransform.anchorMin=Vector2.zero;bg.rectTransform.anchorMax=Vector2.one;bg.rectTransform.offsetMin=bg.rectTransform.offsetMax=Vector2.zero;
        Text(canvas.transform,"Header","MEWGIC / KARAKTER LAB",36,new Vector2(.05f,.91f),new Vector2(.95f,.97f));
        Text(canvas.transform,"Subtitle","Onaylı karakter • Ayrı değnek • Mobil FX",23,new Vector2(.05f,.86f),new Vector2(.95f,.91f));
        var cat=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab),canvas.transform);
        var catRt=cat.GetComponent<RectTransform>();catRt.anchorMin=catRt.anchorMax=new Vector2(.5f,.23f);catRt.anchoredPosition=Vector2.zero;catRt.localScale=Vector3.one*.64f;
        var panel=canvas.AddComponent<WizardCatTestPanel>();panel.cat=cat.GetComponent<WizardCatPresentation>();panel.background=bg;
        var blink=Button(canvas.transform,"BlinkButton","GÖZ KIRP",new Vector2(.07f,.07f),new Vector2(.47f,.14f));panel.blinkButton=blink;
        var magic=Button(canvas.transform,"MagicButton","DEĞNEK BÜYÜSÜ",new Vector2(.53f,.07f),new Vector2(.93f,.14f));panel.magicButton=magic;
        var toggle=Button(canvas.transform,"BackgroundButton","AÇIK / KOYU ZEMİN",new Vector2(.18f,.16f),new Vector2(.82f,.21f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(blink.onClick,panel.Blink);UnityEditor.Events.UnityEventTools.AddPersistentListener(magic.onClick,panel.Magic);UnityEditor.Events.UnityEventTools.AddPersistentListener(toggle.onClick,panel.ToggleBackground);
        panel.status=Text(canvas.transform,"AnimationStatus","Idle hazır",21,new Vector2(.04f,.015f),new Vector2(.96f,.055f));
        new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)).GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();
        Debug.Log("WIZARD_CAT_ASSETS_READY");
    }
}
