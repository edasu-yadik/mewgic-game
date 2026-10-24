using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Mewgic.Characters;

[InitializeOnLoad]
public static class WizardCatAnimationTests
{
    static double next,deadline;
    static float initialBreath;
    static List<string> checks=new List<string>();
    static WizardCatAnimationTests(){EditorApplication.update+=Tick;}
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new Exception("Already playing");
        checks.Clear();
        SessionState.SetBool("WizardCatTestActive",true);SessionState.SetInt("WizardCatTestStep",0);
        SessionState.SetString("WizardCatTestDeadline",DateTime.UtcNow.AddMinutes(3).ToString("O"));
        EditorSceneManager.OpenScene(WizardCatAssetBuilder.Scene);
        EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        EditorApplication.EnterPlaymode();
    }
    static void Check(bool condition,string label){if(!condition)throw new Exception(label);checks.Add(label);}
    static void Click(string name)
    {
        var go=GameObject.Find(name);Check(go!=null,"Button exists: "+name);
        var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,((RectTransform)go.transform).TransformPoint(((RectTransform)go.transform).rect.center)),button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
        Check(hits.Exists(h=>h.gameObject==go),"Button raycast: "+name);
        ExecuteEvents.Execute(go,data,ExecuteEvents.pointerClickHandler);
    }
    public static void Capture(string file)
    {
        var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();var cam=Camera.main;
        var rt=new RenderTexture(900,1600,24);var previous=RenderTexture.active;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;
        cam.targetTexture=rt;Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=rt;
        var texture=new Texture2D(900,1600,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,900,1600),0,0);texture.Apply();
        File.WriteAllBytes("docs/"+file,texture.EncodeToPNG());
        cam.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;RenderTexture.active=previous;
        UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
    }
    static void Tick()
    {
        if(!SessionState.GetBool("WizardCatTestActive",false)||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try
        {
            if(DateTime.UtcNow>DateTime.Parse(SessionState.GetString("WizardCatTestDeadline","")))throw new Exception("Animation tests timeout");
            var cat=UnityEngine.Object.FindFirstObjectByType<WizardCatPresentation>();
            if(cat==null||cat.PoolSize==0)return;
            Application.runInBackground=true;
            int step=SessionState.GetInt("WizardCatTestStep",0);
            var rig=cat.transform.Find("BreathingRig");
            next=EditorApplication.timeSinceStartup+.1;
            switch(step)
            {
                case 0:
                    cat.automaticBlink=false;
                    Check(cat.PoolSize==18,"Fixed spark pool = 18");
                    Check(cat.CanBlink,"Closed-eye sprites available");
                    initialBreath=rig.localScale.y;Capture("wizardcat-idle.png");
                    next=EditorApplication.timeSinceStartup+1.15;break;
                case 1:
                    Check(Mathf.Abs(rig.localScale.y-initialBreath)>.0001f,"Idle breathing changes scale subtly");
                    Check(rig.localScale.y>=.999f&&rig.localScale.y<=1.007f,"Idle amplitude within 0.7 percent");
                    Click("BlinkButton");deadline=EditorApplication.timeSinceStartup+.8;next=EditorApplication.timeSinceStartup+.065;break;
                case 2:
                    if(cat.blinkOverlay.alpha<.8f){if(EditorApplication.timeSinceStartup<deadline)return;throw new Exception("Blink never closed");}
                    Check(cat.blinkOverlay.alpha>=.8f,"Blink overlay closes eyes");
                    Capture("wizardcat-blink.png");next=EditorApplication.timeSinceStartup+.45;break;
                case 3:
                    Check(cat.blinkOverlay.alpha<.02f,"Blink returns to original open eyes");
                    Click("MagicButton");Check(cat.MagicBusy,"Magic button starts effect");Check(!cat.TriggerMagic(),"Repeated magic blocked while active");
                    next=EditorApplication.timeSinceStartup+.40;break;
                case 4:
                    Check(cat.ActiveSparks>0&&cat.ActiveSparks<=18,"Magic emits within fixed particle budget");
                    float angle=rig.Find("WandFrontPivot").localEulerAngles.z;
                    Check(Mathf.Abs(Mathf.DeltaAngle(angle,0))>.2f,"Wand rotates around grip");
                    Capture("wizardcat-magic.png"); foreach(RectTransform rt in cat.fxRoot)Check(rt.GetComponent<CanvasRenderer>()!=null,"Spark CanvasRenderer exists: "+rt.name);Click("BackgroundButton");Capture("wizardcat-light-background.png");
                    next=EditorApplication.timeSinceStartup+1.7;break;
                case 5:
                    Check(!cat.MagicBusy&&cat.ActiveSparks==0,"Magic returns to rest and hides particles");
                    Check(Mathf.Abs(Mathf.DeltaAngle(rig.Find("WandFrontPivot").localEulerAngles.z,0))<.1f,"Wand returns to exact starting rotation");
                    Check(cat.PoolSize==18,"Spark pool unchanged after effect");
                    Check(cat.TriggerMagic(),"Magic can be triggered again");next=EditorApplication.timeSinceStartup+1.8;break;
                case 6:
                    Check(!cat.MagicBusy&&cat.PoolSize==18&&cat.ActiveSparks==0,"Repeated magic completes without pool growth");
                    cat.automaticBlink=true;deadline=EditorApplication.timeSinceStartup+1;next=EditorApplication.timeSinceStartup+.025;break;
                case 7:
                    if(cat.blinkOverlay.alpha<.5f){if(EditorApplication.timeSinceStartup<deadline){next=EditorApplication.timeSinceStartup+.025;return;}throw new Exception("Automatic blink did not trigger");}
                    Check(cat.blinkOverlay.alpha>=.5f,"Automatic blink timer closes lids");next=EditorApplication.timeSinceStartup+.4;break;
                case 8:
                    Check(cat.blinkOverlay.alpha<.02f,"Automatic blink reopens eyes");
                    File.WriteAllText("docs/PART02B_ANIMATION_TESTS.json",JsonUtility.ToJson(new TestReport{passed=true,checks=checks.ToArray()},true));
                    SessionState.SetBool("WizardCatTestActive",false);SessionState.SetInt("WizardCatTestCode",0);EditorApplication.isPlaying=false;return;
            }
            SessionState.SetInt("WizardCatTestStep",step+1);
        }
        catch(Exception e)
        {
            File.WriteAllText("docs/PART02B_ANIMATION_TESTS.json",JsonUtility.ToJson(new TestReport{passed=false,checks=checks.ToArray(),error=e.ToString()},true));
            SessionState.SetBool("WizardCatTestActive",false);SessionState.SetInt("WizardCatTestCode",1);Debug.LogException(e);EditorApplication.isPlaying=false;
        }
    }
    [Serializable] class TestReport {public bool passed;public string[] checks;public string error;}
}
