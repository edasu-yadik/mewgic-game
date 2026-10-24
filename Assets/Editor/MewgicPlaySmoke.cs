using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MewgicPlaySmoke
{
    static double next;
    static MewgicPlaySmoke(){EditorApplication.update+=Tick;}
    public static void RunInteractive(){SessionState.SetBool("MewgicSmokeKeepEditor",true);Run();}
    static void Finish(int code){SessionState.SetInt("MewgicSmokeExitCode",code);if(SessionState.GetBool("MewgicSmokeKeepEditor",false)){SessionState.SetBool("MewgicSmokeKeepEditor",false);EditorApplication.isPlaying=false;}else EditorApplication.Exit(code);}
    public static void Run()
    {
        SessionState.SetInt("MewgicSmokeStep",0); SessionState.SetString("MewgicSmokeDeadline",DateTime.UtcNow.AddMinutes(2).ToString("O"));
        SessionState.SetBool("MewgicSmokeActive",true);
        EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        EditorApplication.EnterPlaymode();
    }
    static void Capture(string path,int width,int height)
    {
        var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();
        var camera=Camera.main;
        var rt=new RenderTexture(width,height,24);
        var previous=RenderTexture.active;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        camera.targetTexture=rt;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active=rt;
        var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
        texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
        File.WriteAllBytes(path,texture.EncodeToPNG());
        camera.targetTexture=null;canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
    }
    static void Click(string name)
    {
        var go=GameObject.Find(name);
        if(go==null||go.GetComponent<Button>()==null)throw new Exception("Button missing "+name);
        go.GetComponent<Button>().onClick.Invoke();
    }
    static void Expect(bool condition,string message){if(!condition)throw new Exception(message);}
    static void Tick()
    {
        if(!SessionState.GetBool("MewgicSmokeActive",false)||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
        next=EditorApplication.timeSinceStartup+1.4;
        try
        {
            if(DateTime.UtcNow > DateTime.Parse(SessionState.GetString("MewgicSmokeDeadline",""))) throw new Exception("Play smoke timeout"); int step=SessionState.GetInt("MewgicSmokeStep",0); if(step==0 && (SceneManager.GetActiveScene().name!="Home" || GameObject.Find("Play")==null))return;
            if(step==2 && GameObject.Find("Bottle 4")==null)return;
            if(step>=4 && step<=10 && GameObject.Find("Restart")!=null && !GameObject.Find("Restart").GetComponent<Button>().interactable)return;
            UnityEngine.Application.runInBackground=true;
            switch(step)
            {
                case 0:
                    Expect(SceneManager.GetActiveScene().name=="Home","Boot to Home failed");
                    Expect(GameObject.Find("Play")!=null,"Play missing");
                    Capture("docs/prototype-home.png",900,1600);break;
                case 1: Click("Play");break;
                case 2:
                    Expect(SceneManager.GetActiveScene().name=="Gameplay","Play transition failed");
                    Expect(GameObject.Find("Bottle 4")!=null,"Bottles missing");
                    Capture("docs/prototype-gameplay.png",900,1600); Capture("docs/prototype-gameplay-tall.png",900,1950);break;
                case 3: Click("Bottle 1");Click("Bottle 2");Expect(GameObject.Find("Moves").GetComponent<Text>().text=="HAMLE  0","Invalid move changed count");Expect(GameObject.Find("Status").GetComponent<Text>().text.Contains("dökülemez"),"Invalid move not rejected");Click("Bottle 3");Click("Bottle 2");break;
                case 4:
                    Expect(GameObject.Find("Moves").GetComponent<Text>().text=="HAMLE  1","Move or animation lock failed");
                    Click("Undo");Expect(GameObject.Find("Moves").GetComponent<Text>().text=="HAMLE  0","Undo UI failed");break;
                case 5: Click("Bottle 1");Click("Bottle 3");break;
                case 6: Click("Restart");Expect(GameObject.Find("Moves").GetComponent<Text>().text=="HAMLE  0","Restart UI failed");break;
                case 7: Click("Bottle 1");Click("Bottle 3");break;
                case 8: Click("Bottle 2");Click("Bottle 1");break;
                case 9: Click("Bottle 2");Click("Bottle 3");break;
                case 10:
                    Expect(GameObject.Find("Win panel")!=null,"Win panel missing");
                    Capture("docs/prototype-win.png",900,1600);break;
                case 11: Click("Replay");Expect(GameObject.Find("Win panel")==null,"Replay failed");Click("Home");break;
                case 12:
                    Expect(SceneManager.GetActiveScene().name=="Home","Home return failed");
                    File.WriteAllText("docs/PLAY_SMOKE_RESULTS.md","# Play Mode doğrulaması\n\nBoot → Home → Gameplay → Home, gerçek UI butonları, animasyon kilidi, geri alma, yeniden başlatma, kazanma ve tekrar oynama başarılı.\n");
                    SessionState.SetBool("MewgicSmokeActive",false);Finish(0);return;
            }
            SessionState.SetInt("MewgicSmokeStep",step+1);
        }
        catch(Exception e)
        {
            File.WriteAllText("docs/play-smoke-failure.txt",e.ToString());Debug.LogException(e);
            SessionState.SetBool("MewgicSmokeActive",false);Finish(1);
        }
    }
}
