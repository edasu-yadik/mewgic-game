#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using TMPro;
using Mewgic.Core;

namespace Mewgic
{
    [DefaultExecutionOrder(-10000)]
    public sealed class WaterfallProbe : MonoBehaviour
    {
        const string Output="docs/ProductionReports/Waterfalls/";
        readonly List<string> checks=new List<string>(),errors=new List<string>(),warnings=new List<string>();
        readonly List<object> gpu=new List<object>();
        readonly List<object> frameTimes=new List<object>();
        bool had;string saved;static WaterfallProbe active;
        void Awake(){if(active!=null){Destroy(gameObject);return;}active=this;DontDestroyOnLoad(gameObject);had=PlayerPrefs.HasKey(GameplayProgress.StorageKey);saved=PlayerPrefs.GetString(GameplayProgress.StorageKey,"");GameplayProgress.UseTestProfile(new ProgressData{completedTutorials=new List<string>{Mewgic.UI.PawGuide.BasicId}});Application.logMessageReceived+=Log;}
        void Log(string m,string s,LogType t){if(t==LogType.Warning)warnings.Add(m);if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m);}
        void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add(name);}
        IEnumerator Start()
        {
            var run=Run();string failure=null;
            while(true){object next=null;bool more=false;try{more=run.MoveNext();if(more)next=run.Current;}catch(Exception e){failure=e.ToString();}if(failure!=null||!more)break;yield return next;}
            bool intact=had==PlayerPrefs.HasKey(GameplayProgress.StorageKey)&&saved==PlayerPrefs.GetString(GameplayProgress.StorageKey,"");
            Application.logMessageReceived-=Log;GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();
            File.WriteAllText(Output+"TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=failure==null&&errors.Count==0&&intact,failure,checks,gpu,frameTimes,errors,warnings,playerSaveIntact=intact,unity=Application.unityVersion},Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying=false;
        }
        void Tap(string name){var go=GameObject.Find(name);Check(go!=null,"Buton: "+name);go.GetComponent<Button>().onClick.Invoke();}
        Puzzle Model=>(Puzzle)typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(FindFirstObjectByType<PrototypeApp>());
        IEnumerator Run()
        {
            yield return null;yield return null;
            var water=FindObjectsByType<WaterfallAnimator>(FindObjectsSortMode.None);
            Check(water.Length==3,"Home mevcut üç maskeli katmanı kullanıyor");
            foreach(var item in water)Audit(item);
            Direction(water[0]);
            var poses=water.Select(w=>w.transform.localPosition).ToArray();
            int transforms=FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
            for(int frame=0;frame<32;frame++){frameTimes.Add(new{scene="Home",frame,time=Time.unscaledTime});Capture(Output+"Home-frame-"+frame.ToString("D3")+".png",450,800);yield return new WaitForSecondsRealtime(.125f);}
            Check(FindObjectsByType<Transform>(FindObjectsSortMode.None).Length==transforms,"Home su döngüsünde nesne sayısı sabit");
            for(int i=0;i<water.Length;i++)Check(water[i].transform.localPosition==poses[i],"Home mimari transform sabit: "+water[i].name);
            foreach(int h in new[]{1600,1950,2000})Capture(Output+"Home-"+h+".png",900,h,h==2000);
            Tap("Play");yield return new WaitForSecondsRealtime(.6f);yield return null;
            Check(SceneManager.GetActiveScene().name=="Gameplay","PLAY → Gameplay");
            water=FindObjectsByType<WaterfallAnimator>(FindObjectsSortMode.None);
            Check(water.Length==1,"Gameplay arka planında tek su animatörü");
            Check(water[0].speed<.022f&&water[0].brightness<.045f,"Gameplay Home'dan sakin");
            Check(water[0].GetComponent<Image>().raycastTarget==false,"Su etkileşim yakalamıyor");
            Check(water[0].transform.parent.GetSiblingIndex()==0,"Su bulmaca ve UI arkasında");
            Check(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.EndsWith(" Water (runtime)"))==1,"Home materyalleri geçişte serbest bırakıldı");
            Audit(water[0]);
            transforms=FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
            for(int frame=0;frame<32;frame++){frameTimes.Add(new{scene="Gameplay",frame,time=Time.unscaledTime});Capture(Output+"Gameplay-frame-"+frame.ToString("D3")+".png",450,800);yield return new WaitForSecondsRealtime(.125f);}
            Check(FindObjectsByType<Transform>(FindObjectsSortMode.None).Length==transforms,"Gameplay su döngüsünde nesne sayısı sabit");
            foreach(int h in new[]{1600,1950,2000})Capture(Output+"Gameplay-"+h+".png",900,h,h==2000);
            var move=PuzzleSolver.Solve(Model.Export()).Moves[0];
            Tap("Bottle "+(move.Source+1));Tap("Bottle "+(move.Target+1));yield return new WaitForSecondsRealtime(1.1f);
            Check(Model.Moves==1,"Şişe seçimi ve sıvı aktarımı çalışıyor");Tap("Undo");Check(Model.Moves==0,"Undo çalışıyor");
            Tap("Restart");Check(Model.Moves==0,"Restart çalışıyor");Tap("Home");yield return null;yield return null;
            Check(SceneManager.GetActiveScene().name=="Home"&&FindObjectsByType<WaterfallAnimator>(FindObjectsSortMode.None).Length==3,"Gameplay → Home; katmanlar tekrarlanmıyor");
        }
        Color32[] Render(Image image,Material material,float time)
        {
            var rt=new RenderTexture(256,512,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);var read=new Texture2D(256,512,TextureFormat.RGBA32,false,true);var previous=RenderTexture.active;
            try{RenderTexture.active=rt;GL.Clear(true,true,Color.clear);material.SetFloat("_EnvironmentTime",time);Graphics.Blit(image.sprite.texture,rt,material);read.ReadPixels(new Rect(0,0,256,512),0,0);read.Apply();return read.GetPixels32();}
            finally{RenderTexture.active=previous;DestroyImmediate(read);rt.Release();DestroyImmediate(rt);}
        }
        void Audit(WaterfallAnimator animator)
        {
            var image=animator.GetComponent<Image>();var mat=animator.WaterMaterial;var mask=(Texture2D)mat.GetTexture("_EffectMask");
            Check(mat.shader.isSupported&&mask!=null,"Shader ve maske: "+image.name);
            float savedTime=mat.GetFloat("_EnvironmentTime");var first=Render(image,mat,0);var second=Render(image,mat,2.8f);var masks=mask.GetPixels32();int water=0,outside=0,stable=0;
            for(int y=1;y<511;y++)for(int x=1;x<255;x++) {
                int n=y*256+x;bool changed=!first[n].Equals(second[n]);if(masks[n].r>20&&changed)water++;
                bool clear=true;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var v=masks[n+dy*256+dx];clear&=v.r==0&&v.g==0;}
                if(clear){outside++;if(!changed)stable++;}
            }
            Check(water>25,"Zamanlar arası GPU su pikselleri değişiyor: "+image.name);
            Check(outside>10000&&outside==stable,"Maske dışındaki mimari piksel piksel sabit: "+image.name);
            // Warm first; this measures the new component, not camera capture/test allocation.
            animator.Apply(savedTime);long before=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<1000;i++)animator.Apply(savedTime+i*.01f);long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
            Check(allocated==0,"1000 animasyon güncellemesinde managed tahsis yok: "+image.name);animator.Apply(savedTime);
            gpu.Add(new{layer=image.name,waterChangedPixels=water,outsidePixels=outside,unchangedOutside=stable,allocatedBytesPer1000Updates=allocated,maskBytes=mask.width*mask.height*4});
        }
        void Direction(WaterfallAnimator source)
        {
            var mat=new Material(source.WaterMaterial);var texture=new Texture2D(2,2,TextureFormat.RGBA32,false,true);texture.SetPixels(new[]{new Color(.3f,.3f,.3f),new Color(.3f,.3f,.3f),new Color(.3f,.3f,.3f),new Color(.3f,.3f,.3f)});texture.Apply();
            var go=new GameObject("Direction QA",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var image=go.GetComponent<Image>();image.sprite=Sprite.Create(texture,new Rect(0,0,2,2),Vector2.one*.5f);
            try {
                mat.SetTexture("_EffectMask",Texture2D.redTexture);mat.SetFloat("_FlowSpeed",.02f);mat.SetFloat("_FlowBrightness",.1f);mat.SetFloat("_FlowOpacity",1);
                var a=Render(image,mat,0);var b=Render(image,mat,1f/(512*.02f));long down=0,up=0;
                for(int y=4;y<508;y++)for(int x=4;x<252;x++){int n=y*256+x;down+=Math.Abs(b[n].b-a[n+256].b);up+=Math.Abs(b[n].b-a[n-256].b);}
                Check(down<up*.1,"GPU desen korelasyonu: su yukarıdan aşağı akıyor");gpu.Add(new{directionDownError=down,directionUpError=up});
            }finally{DestroyImmediate(image.sprite);DestroyImmediate(go);DestroyImmediate(mat);DestroyImmediate(texture);}
        }
        void Capture(string filename,int width,int height,bool inset=false)
        {
            var canvas=FindFirstObjectByType<Canvas>();var camera=Camera.main;var p=FindFirstObjectByType<HomeCharacterPresenter>();var safe=p!=null?(RectTransform)p.transform.Find("UI"):(GameObject.Find("Safe area")??GameObject.Find("MapSafeArea"))?.GetComponent<RectTransform>();
            var mode=canvas.renderMode;var oldCam=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;Vector2 min=safe!=null?safe.anchorMin:Vector2.zero,max=safe!=null?safe.anchorMax:Vector2.one;
            var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;if(inset&&safe!=null){safe.anchorMin=new Vector2(.035f,.04f);safe.anchorMax=new Vector2(.965f,.955f);}
                Canvas.ForceUpdateCanvases();var gameplay=FindFirstObjectByType<GameplayPresentation>();if(gameplay!=null)gameplay.Layout();var pause=GameObject.Find("PauseSafeArea");if(pause!=null&&safe!=null){var pr=(RectTransform)pause.transform;pr.anchorMin=safe.anchorMin;pr.anchorMax=safe.anchorMax;}if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}var map=FindFirstObjectByType<Mewgic.Map.MapWorld>();if(map!=null)map.Layout();var daily=FindFirstObjectByType<Mewgic.Daily.DailyActivitiesPanel>();if(daily!=null&&daily.IsOpen)daily.Layout();var hub=FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>();if(hub!=null&&hub.IsOpen)hub.Layout();Canvas.ForceUpdateCanvases();
                foreach(var text in FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None))text.ForceMeshUpdate();FindFirstObjectByType<Mewgic.UI.PawGuide>()?.SendMessage("LateUpdate");foreach(var text in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){text.ForceMeshUpdate();if(text.name=="PawMessage")Check(!text.isTextOverflowing,"Pati balonunda taşma yok: "+height);}camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(filename,tex.EncodeToPNG());
            }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=distance;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}}
        }
    }
}
#endif
