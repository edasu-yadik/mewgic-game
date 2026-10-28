#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
namespace Mewgic
{
    // Yalnız Editor test koşusunda, diske kaydedilmeyen Boot örneğine eklenir.
    [DefaultExecutionOrder(-10000)]
    public sealed class BootScenarioProbe : MonoBehaviour
    {
        public int scenario;
        BootLoadingController boot;float started;readonly List<string> checks=new List<string>();readonly List<object> stages=new List<object>();
        string before;bool had;float previous=-1;string phase;readonly List<string> runtimeErrors=new List<string>();
        void Awake()
        {
            DontDestroyOnLoad(gameObject);started=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;
            had=PlayerPrefs.HasKey(GameplayProgress.StorageKey);before=PlayerPrefs.GetString(GameplayProgress.StorageKey,"");
            GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            boot=FindFirstObjectByType<BootLoadingController>();
            if(scenario==3){var warm=GameplayProgress.Current;GameplayLevelCatalog.ResumeFromSave();GameplayLevelCatalog.Current();}
            if(scenario==4)boot.homeScene="Missing_Home_PART09A";
            boot.EditorHoldBeforeActivation=true;
            boot.StageChanged+=(state,value)=>{if(phase=="Failed")previous=-1;if(value<previous)throw new Exception("İlerleme geriye gitti");previous=value;phase=state;stages.Add(new{state,value,seconds=Time.realtimeSinceStartup-started});};
        }
        IEnumerator Start()
        {
            var steps=Run();
            while(true)
            {
                bool more=false;object next=null;string error=null;
                try{more=steps.MoveNext();if(more)next=steps.Current;}catch(Exception e){error=e.ToString();}
                if(error!=null){Finish(error);yield break;}
                if(!more){Finish(null);yield break;}
                yield return next;
            }
        }
        void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        IEnumerator Run()
        {
            while(boot.State!="Ready"&&boot.State!="Failed"){if(Time.realtimeSinceStartup-started>45)throw new Exception("Başlangıç zaman aşımı");yield return null;}
            if(scenario==4)
            {
                phase="Failed";
                Check(boot.State=="Failed"&&SceneManager.GetActiveScene().name=="Boot","Eksik Home sahnesi güvenle reddedildi");
                Check(boot.View.RetryButton.gameObject.activeInHierarchy&&boot.Error!=null,"Anlaşılır hata ve çalışan tekrar dene gösterildi");
                yield return null;Capture("docs/Part09A/Error-GameView.png");yield return null;
                boot.homeScene="Home";boot.View.RetryButton.onClick.Invoke();
                while(boot.State!="Ready"){if(boot.State=="Failed")throw new Exception("Tekrar yükleme başarısız");if(Time.realtimeSinceStartup-started>60)throw new Exception("Tekrar yükleme zaman aşımı");yield return null;}
                checks.Add("Gerçek Retry butonu ile Home asenkron yeniden hazırlandı");
            }
            Check(boot.State=="Ready"&&SceneManager.GetActiveScene().name=="Boot","Yükleme hazırlanırken Home etkinleştirilmedi");
            Check(boot.ActualProgress==.95f&&boot.ActivationCount==0,"Yüzde100 yalnız aktivasyon sonrası; gerçek hazırlık95");
            var view=boot.View;
            if(scenario==1)view.EditorSafeArea=new Rect(0,50,900,1810);
            if(scenario==2)view.EditorSafeArea=new Rect(0,65,900,1825);
            yield return null;yield return null;
            while(view.DisplayProgress<.949f)yield return null;
            Canvas.ForceUpdateCanvases();
            Check(Screen.width==900&&Screen.height==(scenario==1?1950:scenario==2?2000:1600),"Gerçek Game View boyutu doğru");
            var bounds=new Vector3[4];view.Safe.GetWorldCorners(bounds);var safeRect=new Rect(bounds[0].x,bounds[0].y,bounds[2].x-bounds[0].x,bounds[2].y-bounds[0].y);
            foreach(var r in new[]{view.Logo,view.Character,view.Bar,(RectTransform)view.Message.transform}){var corners=new Vector3[4];r.GetWorldCorners(corners);Check(safeRect.Contains((Vector2)corners[0]+Vector2.one)&&safeRect.Contains((Vector2)corners[2]-Vector2.one),r.name+" Safe Area içinde");}
            Check(FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"Boot sırasında tek EventSystem");
            Check(FindObjectsByType<BootMagicGraphic>(FindObjectsSortMode.None).Length==2,"Sabit maliyetli yıldız parıltısı ve10 parçacık");
            float y=view.Character.anchoredPosition.y;
            for(int i=0;i<12;i++)yield return null;
            Check(Mathf.Abs(y-view.Character.anchoredPosition.y)>.001f,"Kedi hafif hareket ediyor");
            yield return null;Capture("docs/Part09A/Opening-"+scenario+"-GameView.png");yield return null;
            Check(view.DisplayProgress<=boot.ActualProgress+.001f&&Mathf.Abs(view.RenderedProgress-view.DisplayProgress)<.001f,"Çizilen dolgu gerçek ilerleme değerini gösteriyor");
            boot.EditorHoldBeforeActivation=false;float release=Time.realtimeSinceStartup;
            while(boot!=null){if(Time.realtimeSinceStartup-release>8)throw new Exception("Home geçişi zaman aşımı");yield return null;}
            Check(SceneManager.GetActiveScene().name=="Home"&&GameObject.Find("Play")!=null,"Gerçek Home ve Play açıldı");
            Check(FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"Home sonrası tek EventSystem; Boot kaynakları temizlendi");
            Check(GameplayProgress.LoadError==null,"Kayıt sistemi başarıyla hazır");
            Check(PlayerPrefs.HasKey(GameplayProgress.StorageKey)==had&&PlayerPrefs.GetString(GameplayProgress.StorageKey,"")==before,"Gerçek oyuncu kaydı değişmedi");
            if(scenario==3)Check(Time.realtimeSinceStartup-release<1.5f,"Hızlı başlangıçta uzun zorunlu bekleme yok");
            if(scenario==0){yield return null;Capture("docs/Part09A/Home-After-Boot.png");yield return null;}
        }
        void Capture(string filename)
        {
            int width=900,height=scenario==1?1950:scenario==2?2000:1600;
            var canvas=FindFirstObjectByType<Canvas>();var camera=Camera.main;
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var rt=new RenderTexture(width,height,24);var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(filename,pixels.EncodeToPNG());
            }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;RenderTexture.active=oldActive;DestroyImmediate(pixels);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();}
        }
        void Finish(string error)
        {
            Application.logMessageReceived-=OnLog;if(runtimeErrors.Count>0)error=(error??"")+string.Join("\n",runtimeErrors);
            File.WriteAllText("docs/Part09A/scenario-"+scenario+".json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=error==null,scenario,error,checks,stages,runtimeErrors,seconds=Time.realtimeSinceStartup-started},Newtonsoft.Json.Formatting.Indented));
            GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            EditorApplication.isPlaying=false;
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)runtimeErrors.Add(message);}
    }
}
#endif
