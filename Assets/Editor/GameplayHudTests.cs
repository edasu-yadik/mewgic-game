using System;using System.IO;using System.Collections.Generic;using System.Reflection;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using Mewgic;using Mewgic.Core;
[InitializeOnLoad] public static class GameplayHudTests
{
    static double next;static int step;static Vector3 pausedPosition;static Quaternion pausedRotation;static float pausedVolume;static readonly List<string> checks=new List<string>();
    static GameplayHudTests(){EditorApplication.update+=Tick;}
    public static void Run(){if(EditorApplication.isPlaying)throw new Exception("Stop Play first");checks.Clear();step=0;SessionState.SetBool("GameplayHudTest",true);EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");EditorApplication.isPlaying=true;next=EditorApplication.timeSinceStartup+1;}
    static void Check(bool value,string name){if(!value)throw new Exception(name);checks.Add(name);}
    static Puzzle Model()=> (Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(UnityEngine.Object.FindFirstObjectByType<PrototypeApp>());
    static void Click(string name)
    {Canvas.ForceUpdateCanvases();var g=GameObject.Find(name);Check(g!=null,"Button exists "+name);var r=(RectTransform)g.transform;var d=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(d,hits);Check(hits.Count>0&&hits[0].gameObject==g,"Real top touch target "+name);ExecuteEvents.Execute(g,d,ExecuteEvents.pointerClickHandler);}
    static Rect Bounds(RectTransform t,RectTransform relative){var c=new Vector3[4];t.GetWorldCorners(c);Vector2 min=Vector2.one*float.MaxValue,max=-min;foreach(var p in c){var v=(Vector2)relative.InverseTransformPoint(p);min=Vector2.Min(min,v);max=Vector2.Max(max,v);}return Rect.MinMaxRect(min.x,min.y,max.x,max.y);}
    static void UnitTests()
    {
        var info=new GameplayLevelInfo{id=17,goal="Dinamik test hedefi",threeStarMoves=3,twoStarMoves=5};info.Validate();
        for(int extra=0;extra<=4;extra+=2)
        {
            var model=PrototypeApp.LoadLevel();var start=new GameplayHudState(info,model);Check(start.Level==17&&start.Goal==info.goal,"Level and objective come from metadata");Check(start.Total==2&&start.Completed==0&&start.AwardedStars==0,"No fake goal or award at start");
            Check(model.Pour(0,2)>0,"Initial real move");for(int n=0;n<extra;n++){Check(model.Pour(2,3)>0&&model.Pour(3,2)>0,"Real extra moves");}
            Check(model.Pour(1,0)>0&&model.Pour(1,2)>0,"Real winning solution");
            var result=new GameplayHudState(info,model);int expected=extra==0?3:extra==2?1:1;
            // extra is a count of two-move detours: 0 -> 3, 2 -> 7, 4 -> 11 moves.
            Check(model.Won&&result.AwardedStars==expected&&result.Completed==2,"Award follows actual solved performance "+model.Moves);
        }
        var p=PrototypeApp.LoadLevel();p.Pour(0,2);for(int i=0;i<5;i++)p.Pour(i%2==0?2:3,i%2==0?3:2);Check(new GameplayHudState(info,p).PotentialStars==1,"Six moves yield one-star target");p.Undo();Check(new GameplayHudState(info,p).PotentialStars==2,"Undo restores two-star threshold");p.Undo();Check(new GameplayHudState(info,p).PotentialStars==2,"Four-move two-star boundary");p.Undo();Check(new GameplayHudState(info,p).PotentialStars==3,"Three-move three-star boundary");p.Restart();Check(new GameplayHudState(info,p).Moves==0,"Restart resets HUD state");
        var two=PrototypeApp.LoadLevel();two.Pour(0,2);two.Pour(2,3);two.Pour(3,2);two.Pour(1,0);two.Pour(1,2);Check(new GameplayHudState(info,two).AwardedStars==2,"Five-move win earns two stars");
    }
    public static void Capture(int height,bool inset=false,string suffix="")
    {
        var presentation=UnityEngine.Object.FindFirstObjectByType<GameplayPresentation>();var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();var safe=(RectTransform)GameObject.Find("Safe area").transform;var cam=Camera.main;var min=safe.anchorMin;var max=safe.anchorMax;var mode=canvas.renderMode;var camera=canvas.worldCamera;var dist=canvas.planeDistance;var rt=new RenderTexture(900,height,24);var old=RenderTexture.active;var target=cam.targetTexture;Texture2D tex=null;
        try{canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;cam.targetTexture=rt;if(inset){safe.anchorMin=new Vector2(.04f,.05f);safe.anchorMax=new Vector2(.96f,.96f);}Canvas.ForceUpdateCanvases();presentation.Layout();Canvas.ForceUpdateCanvases();
            foreach(var name in new[]{"LevelPanel","GoalPanel","MovesPanel","PerformancePanel","Pause"}){var r=Bounds((RectTransform)GameObject.Find(name).transform,safe);Check(safe.rect.Contains(r.min)&&safe.rect.Contains(r.max),"HUD inside safe "+name+height+suffix);foreach(var b in GameObject.Find("Bottle board").GetComponentsInChildren<Button>())Check(!r.Overlaps(Bounds((RectTransform)b.transform,safe)),"HUD avoids bottle "+name+height+suffix);}
            var pr=(RectTransform)GameObject.Find("Pause").transform;Check(pr.rect.width*canvas.scaleFactor>=44&&pr.rect.height*canvas.scaleFactor>=44,"Pause touch area "+height);
            foreach(var name in new[]{"LevelValue","GoalValue","Moves","PerformanceValue"}){var t=GameObject.Find(name).GetComponent<Text>();Check(!t.cachedTextGenerator.Populate(t.text,t.GetGenerationSettings(t.rectTransform.rect.size))||t.cachedTextGenerator.lineCount<=3,"Readable text lines "+name+height);}
            cam.Render();RenderTexture.active=rt;tex=new Texture2D(900,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,height),0,0);tex.Apply();File.WriteAllBytes("docs/gameplay-05c-"+height+suffix+".png",tex.EncodeToPNG());}
        finally{cam.targetTexture=target;RenderTexture.active=old;canvas.renderMode=mode;canvas.worldCamera=camera;canvas.planeDistance=dist;safe.anchorMin=min;safe.anchorMax=max;if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);Canvas.ForceUpdateCanvases();presentation.Layout();}
    }
    static void HudMatches()
    {
        var h=UnityEngine.Object.FindFirstObjectByType<GameplayHud>();if(h==null)return;var p=Model();var info=JsonUtility.FromJson<GameplayLevelInfo>(Resources.Load<TextAsset>("Levels/Level01").text);var expected=new GameplayHudState(info,p);Check(h.State.Level==info.id,"Loaded level identity "+step);Check(h.State.Moves==p.Moves&&GameObject.Find("Moves").GetComponent<Text>().text=="HAMLE  "+p.Moves,"Live model move count "+step);Check(h.State.Completed==expected.Completed&&h.State.Total==expected.Total,"Dynamic objective progress "+step);Check(h.State.PotentialStars==expected.PotentialStars&&h.State.AwardedStars==expected.AwardedStars,"Honest performance state "+step);
    }
    static void Finish(string error){SessionState.SetBool("GameplayHudTest",false);File.WriteAllText("docs/PART05C_TEST_RESULTS.json",JsonUtility.ToJson(new Result{success=error==null,error=error,checks=checks.ToArray()},true));EditorApplication.isPlaying=false;}
    [Serializable]class Result{public bool success;public string error;public string[] checks;}
    static void Tick()
    {
        if(!SessionState.GetBool("GameplayHudTest",false)||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        try{next=EditorApplication.timeSinceStartup+1.1;var app=UnityEngine.Object.FindFirstObjectByType<PrototypeApp>();var visual=UnityEngine.Object.FindFirstObjectByType<GameplayPresentation>();HudMatches();switch(step){
        case 0:UnitTests();Capture(1600);Capture(1950);Capture(2000);Capture(2000,true,"-safe");Click("Pause");break;
        case 1:Check(app.IsPaused,"Pause idle");var bottle=GameObject.Find("Bottle 1");var d=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,bottle.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(d,hits);Check(hits.Count>0&&hits[0].gameObject.name=="GameplayPauseOverlay","Modal intercepts underlying bottle touches");GameObject.Find("Bottle 1").GetComponent<Button>().onClick.Invoke();GameObject.Find("Bottle 3").GetComponent<Button>().onClick.Invoke();GameObject.Find("Restart").GetComponent<Button>().onClick.Invoke();Check(Model().Moves==0,"Paused callbacks cannot change model");Click("Resume");break;
        case 2:Check(!app.IsPaused,"Resume idle");Click("Bottle 1");break;
        case 3:Click("Bottle 3");next=EditorApplication.timeSinceStartup+.40;break;
        case 4:
            if(!app.IsPaused){Check(visual.IsAnimating&&GameObject.Find("Pour stream")!=null,"Pause exercised during actual liquid transfer");Click("Pause");var v=GameObject.Find("Bottle 1").GetComponent<GameplayBottleView>();pausedPosition=v.Rig.position;pausedRotation=v.Rig.rotation;pausedVolume=v.DisplayedVolume;ScreenCapture.CaptureScreenshot("C:/Dev/Mewgic/docs/PART05C_PAUSE.png");step--;break;}
            var frozen=GameObject.Find("Bottle 1").GetComponent<GameplayBottleView>();Check(app.IsPaused&&visual.IsAnimating,"Pause during active pour");Check(Vector3.Distance(pausedPosition,frozen.Rig.position)<.001f&&Quaternion.Angle(pausedRotation,frozen.Rig.rotation)<.01f&&Mathf.Abs(pausedVolume-frozen.DisplayedVolume)<.001f,"Pour pose and volume frozen");Check(Model().Moves==1,"Paused logical move not duplicated");Click("Resume");break;
        case 5:Check(!visual.IsAnimating&&!app.IsPaused&&Model().Moves==1,"Resume finishes exactly one pour");Click("Undo");break;
        case 6:Check(Model().Moves==0,"Undo updates HUD");Click("Bottle 1");break;
        case 7:Click("Bottle 3");Click("Pause");break;
        case 8:Click("PauseRestart");break;
        case 9:Check(!visual.IsAnimating&&!app.IsPaused&&Model().Moves==0,"Pause Restart cancels in-flight animation and resets");foreach(var b in GameObject.Find("Bottle board").GetComponentsInChildren<GameplayBottleView>())Check(Quaternion.Angle(b.Rig.localRotation,Quaternion.identity)<.01f,"Restart restores bottle rotation");Check(GameObject.Find("Pour stream")==null,"Restart leaves no stream");Click("Bottle 1");break;
        case 10:Click("Bottle 3");break;
        case 11:Click("Bottle 2");break;
        case 12:Click("Bottle 1");break;
        case 13:Check(UnityEngine.Object.FindFirstObjectByType<GameplayHud>().State.Completed==1,"Objective updates one of two colors");Click("Bottle 2");break;
        case 14:Click("Bottle 3");break;
        case 15:Check(Model().Won&&UnityEngine.Object.FindFirstObjectByType<GameplayHud>().State.AwardedStars==3,"Win awards real three-star result");Capture(1600,false,"-win");Click("Pause");break;
        case 16:Click("PauseHome");break;
        case 17:Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Home","Pause Home from win");Click("Play");break;
        case 18:Click("Bottle 1");break;
        case 19:Click("Bottle 3");Click("Pause");break;
        case 20:Click("PauseHome");break;
        case 21:Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Home"&&Mathf.Approximately(Time.timeScale,1),"Pause Home mid-pour and no global clock leak");Click("Play");break;
        case 22:Check(Model().Moves==0&&!app.IsPaused&&!visual.IsAnimating,"Clean gameplay re-entry");Finish(null);return;
        }step++;}catch(Exception e){Finish(e.ToString());}
    }
}
