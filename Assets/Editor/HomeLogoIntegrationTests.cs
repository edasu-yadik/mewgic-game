using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mewgic;

[InitializeOnLoad]
public static class HomeLogoIntegrationTests
{
    static double next;
    static float breath; static float potionY,glowAlpha;
    static readonly List<string> checks = new List<string>();
    static HomeLogoIntegrationTests() { EditorApplication.update += Tick; }
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play first");
        SessionState.SetBool("LogoIntegrationTest", true); SessionState.SetInt("HomeStep", 0);
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
        EditorApplication.EnterPlaymode();
    }
    static void Check(bool ok, string text) { if (!ok) throw new Exception(text); checks.Add(text); }
    static Rect Bounds(RectTransform target, RectTransform relative)
    {
        var corners = new Vector3[4]; target.GetWorldCorners(corners);
        var min = new Vector2(float.MaxValue,float.MaxValue); var max = -min;
        foreach(var point in corners) { var p=(Vector2)relative.InverseTransformPoint(point); min=Vector2.Min(min,p); max=Vector2.Max(max,p); }
        return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
    }
    public static void Capture(int height, bool inset = false, string suffix = "")
    {
        var p=UnityEngine.Object.FindFirstObjectByType<HomeCharacterPresenter>();
        var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>(); var cam=Camera.main;
        var safe=(RectTransform)p.transform.Find("UI");
        var oldMin=safe.anchorMin; var oldMax=safe.anchorMax;
        var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;var oldDistance=canvas.planeDistance;
        var previous=RenderTexture.active;var oldTarget=cam.targetTexture;
        var rt=new RenderTexture(900,height,24); Texture2D texture=null;
        try
        {
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;cam.targetTexture=rt;
            if(inset){safe.anchorMin=new Vector2(.04f,.05f);safe.anchorMax=new Vector2(.96f,.96f);}
            Canvas.ForceUpdateCanvases();p.Layout();UnityEngine.Object.FindFirstObjectByType<HomePotionDisplay>()?.Layout();Canvas.ForceUpdateCanvases();
            UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>()?.Layout();
            Canvas.ForceUpdateCanvases();
            UnityEngine.Object.FindFirstObjectByType<HomePotionDisplay>()?.Layout();Canvas.ForceUpdateCanvases(); var bounds=Bounds(p.character,safe);var button=Bounds((RectTransform)GameObject.Find("Play").transform,safe);
            Check(safe.rect.Contains(bounds.min)&&safe.rect.Contains(bounds.max),"Full character inside safe area "+height+suffix);
            Check(!bounds.Overlaps(button),"No PLAY overlap "+height+suffix);
            Check(Mathf.Approximately(p.character.localScale.x,p.character.localScale.y),"Uniform scale "+height+suffix);
            Check(p.character.GetComponentsInChildren<Graphic>().Length>0,"Character graphics present "+height+suffix);
                        var display=UnityEngine.Object.FindFirstObjectByType<HomePotionDisplay>();Check(display!=null,"Potion presenter present");
            Check(GameObject.Find("Potion display")==null,"Prototype panel absent");
            foreach(var n in new[]{"LeftPotion","CenterPotion","RightPotion"}){var br=Bounds((RectTransform)GameObject.Find(n).transform,safe);Check(safe.rect.Contains(br.min)&&safe.rect.Contains(br.max),"Potion inside safe "+n+height+suffix);Check(!br.Overlaps(button),"Potion avoids Play "+n+height+suffix);}
                        var logo=GameObject.Find("MewgicLogo");Check(logo!=null,"Logo visible");Check(GameObject.Find("Brand")==null,"Flat Brand removed");var lr=(RectTransform)logo.transform;var lb=Bounds(lr,safe);Check(safe.rect.Contains(lb.min)&&safe.rect.Contains(lb.max),"Logo inside safe area "+height+suffix);Check(!lb.Overlaps(button),"Logo avoids PLAY");Check(!lb.Overlaps(Bounds((RectTransform)GameObject.Find("CenterPotion").transform,safe)),"Logo avoids center potion");var li=logo.GetComponent<Image>();Check(li.sprite!=null&&li.preserveAspect&&!li.raycastTarget,"Logo sprite preserves ratio and allows input");Check(lr.anchorMax.y<=.9251f,"Future resource bar space reserved");
            cam.Render();RenderTexture.active=rt;texture=new Texture2D(900,height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,900,height),0,0);texture.Apply();
            File.WriteAllBytes("docs/home-logo-"+height+suffix+".png",texture.EncodeToPNG());
        }
        finally
        {
            safe.anchorMin=oldMin;safe.anchorMax=oldMax;cam.targetTexture=oldTarget;
            canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;RenderTexture.active=previous;
            if(texture!=null)UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            Canvas.ForceUpdateCanvases();p.Layout();
            UnityEngine.Object.FindFirstObjectByType<HomeEnvironment>()?.Layout();
        }
    }
    static void Click(string name)
    {
        var go=GameObject.Find(name); Check(go!=null,"Button exists "+name);
        var r=(RectTransform)go.transform;
        var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
        Check(hits.Count>0&&hits[0].gameObject==go,"Top raycast target "+name);
        ExecuteEvents.Execute(go,data,ExecuteEvents.pointerClickHandler);
    }
    static void Finish(string error)
    {
        SessionState.SetBool("LogoIntegrationTest",false);
        File.WriteAllText("docs/PART04A_TEST_RESULTS.json",JsonUtility.ToJson(new Result{success=error==null,error=error,checks=checks.ToArray()},true));
        EditorApplication.isPlaying=false;
    }
    [Serializable] class Result { public bool success;public string error;public string[] checks; }
    static void Tick()
    {
        if(!SessionState.GetBool("LogoIntegrationTest",false)||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try
        {
            Application.runInBackground=true;
            int step=SessionState.GetInt("HomeStep",0);
            var p=UnityEngine.Object.FindFirstObjectByType<HomeCharacterPresenter>();
            next=EditorApplication.timeSinceStartup+.2;
            if(step<8&&(p==null||p.cat.PoolSize==0))return;
            switch(step)
            {
                case 0:
                    checks.Clear();Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Home","Actual Home Play Mode");
                    Check(UnityEngine.Object.FindObjectsByType<HomeCharacterPresenter>(FindObjectsSortMode.None).Length==1,"One Home character presenter");
                    Check(p.cat.automaticBlink&&p.cat.CanBlink,"Existing automatic Blink enabled");
                    Check(p.cat.animator.runtimeAnimatorController.name=="WizardCat","Existing Animator controller");
                    foreach(var g in p.character.GetComponentsInChildren<Graphic>())Check(!g.raycastTarget,"Character does not block input: "+g.name);
                    Capture(1600);Capture(1950);Capture(2000);Capture(2000,true,"-safe");
                    potionY=((RectTransform)GameObject.Find("CenterPotion").transform).anchoredPosition.y;glowAlpha=GameObject.Find("LeftPotion").transform.Find("Glow").GetComponent<Image>().color.a; breath=p.cat.transform.Find("BreathingRig").localScale.y;next=EditorApplication.timeSinceStartup+.75;break;
                case 1: Check(Mathf.Abs(potionY-((RectTransform)GameObject.Find("CenterPotion").transform).anchoredPosition.y)>.01f,"Potion idle changes position");Check(Mathf.Abs(glowAlpha-GameObject.Find("LeftPotion").transform.Find("Glow").GetComponent<Image>().color.a)>.0001f,"Potion glow changes brightness");
                    Check(Mathf.Abs(breath-p.cat.transform.Find("BreathingRig").localScale.y)>.00005f,"Idle breathing changes transform");
                    p.cat.TriggerBlink();next=EditorApplication.timeSinceStartup+.1;break;
                case 2:Check(p.cat.blinkOverlay.alpha>.5f,"Blink closes existing eyelids");Capture(1600,false,"-blink");next=EditorApplication.timeSinceStartup+.4;break;
                case 3:Check(p.cat.blinkOverlay.alpha<.01f,"Blink reopens");next=EditorApplication.timeSinceStartup+2;break;
                case 4:Check(p.MagicCount>0,"Home automatically schedules Wand Magic");next=EditorApplication.timeSinceStartup+2;break;
                case 5:Check(p.cat.TriggerMagic(),"Manual Wand Magic accepted");next=EditorApplication.timeSinceStartup+.35;break;
                case 6:Check(p.cat.ActiveSparks>0,"Gold sparks active");Capture(1600,false,"-magic");next=EditorApplication.timeSinceStartup+1.6;break;
                case 7:Check(!p.cat.MagicBusy&&p.cat.ActiveSparks==0,"Magic returns to rest");Click("Play");break;
                case 8:Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Gameplay","PLAY opens Gameplay");Check(GameObject.Find("Bottle 1")!=null,"Gameplay bottles remain available");Click("Home");break;
                case 9:Check(p!=null,"Gameplay Home returns to character menu");Check(UnityEngine.Object.FindObjectsByType<HomeCharacterPresenter>(FindObjectsSortMode.None).Length==1,"No duplicated character after return");Finish(null);return;
            }
            SessionState.SetInt("HomeStep",step+1);
        }
        catch(Exception ex){Finish(ex.ToString());}
    }
}





