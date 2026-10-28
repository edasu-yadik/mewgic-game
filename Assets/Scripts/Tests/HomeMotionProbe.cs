#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEditor;
namespace Mewgic
{
    [DefaultExecutionOrder(-10000)]
    public sealed class HomeMotionProbe : MonoBehaviour
    {
        readonly List<string> checks=new List<string>(),errors=new List<string>();readonly List<object> shaderChecks=new List<object>();
        float started;HomeCharacterPresenter home;HomeEnvironment env;Button play;PremiumPlayButton premium;
        string realSave;bool hadSave;
        const string Output="docs/Part09B/";
        void Awake(){DontDestroyOnLoad(gameObject);started=Time.realtimeSinceStartup;hadSave=PlayerPrefs.HasKey(GameplayProgress.StorageKey);realSave=PlayerPrefs.GetString(GameplayProgress.StorageKey,"");GameplayProgress.UseTestProfile();Application.logMessageReceived+=Log;}
        void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m);}
        IEnumerator Start()
        {
            var steps=Run();while(true){bool more=false;object next=null;string error=null;try{more=steps.MoveNext();if(more)next=steps.Current;}catch(Exception e){error=e.ToString();}if(error!=null){Finish(error);yield break;}if(!more){Finish(null);yield break;}yield return next;}
        }
        void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        IEnumerator Run()
        {
            while((home=FindFirstObjectByType<HomeCharacterPresenter>())==null||!home.GetComponent<HomeEnvironment>().Mounted){if(Time.realtimeSinceStartup-started>25)throw new Exception("Home zaman aşımı");yield return null;}
            yield return null;yield return null;
            env=home.GetComponent<HomeEnvironment>();play=GameObject.Find("Play").GetComponent<Button>();premium=play.GetComponent<PremiumPlayButton>();
            Check(Screen.width==900&&Screen.height==1600,"Gerçek Home Play Mode 900x1600");
            Check(home.cat.automaticBlink&&home.cat.CanBlink&&home.cat.PoolSize==18,"Mevcut göz kırpma/18 kıvılcım havuzu korunuyor");
            var world=home.transform.Find("WorldDecorations/EnvironmentComposition");var names=new[]{"Layer02_DistantIslands","Layer03_MainCastle","Layer04_Village"};
            var architecture=names.Select(n=>(RectTransform)world.Find(n)).ToArray();var positions=architecture.Select(t=>t.localPosition).ToArray();var scales=architecture.Select(t=>t.localScale).ToArray();
            var hit=(RectTransform)play.transform;var bounds=hit.rect;var anchorMin=hit.anchorMin;var anchorMax=hit.anchorMax;var hitScale=hit.localScale;
            var points=new List<Vector2>();for(int y=-3;y<=3;y++)for(int x=-4;x<=4;x++)points.Add(RectTransformUtility.WorldToScreenPoint(null,hit.TransformPoint(hit.rect.center+new Vector2(x*hit.rect.width/8,y*hit.rect.height/6))));var hits=points.Select(p=>premium.IsRaycastLocationValid(p,null)).ToArray();
            var visual=(RectTransform)play.transform.Find("PlayArtwork");var frame=visual.Find("FrameGlow").GetComponent<Image>();
            Check(frame.material.shader.name=="Mewgic/PlayFrameSweep"&&frame.material.shader.isSupported,"Altın kenar geçişi tek örnek materyalle destekleniyor");
            var particleRoot=world.Find("Layer06_Atmosphere");Check(particleRoot.childCount==17,"Sis ve mevcut16 ortam ışığı; ek parçacık sistemi yok");
            Check(home.GetComponentsInChildren<ParticleSystem>(true).Length==0,"ParticleSystem eklenmedi");
            int objectCount=home.GetComponentsInChildren<Transform>(true).Length;
            var potion=(RectTransform)GameObject.Find("CenterPotion").transform;float potionY=potion.anchoredPosition.y;float breath=home.cat.transform.Find("BreathingRig").localScale.y;float moteY=particleRoot.Find("GoldMote_0").localPosition.y;
            float min=2,max=0;bool potionMoved=false,catMoved=false,magic=false;float sweepStart=frame.material.GetFloat("_Sweep");
            Directory.CreateDirectory("Builds/Previews/Part09B");int sample=0;float until=Time.realtimeSinceStartup+6.5f,next=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup<until)
            {
                min=Mathf.Min(min,visual.localScale.x);max=Mathf.Max(max,visual.localScale.x);potionMoved|=Mathf.Abs(potionY-potion.anchoredPosition.y)>.1f;catMoved|=Mathf.Abs(breath-home.cat.transform.Find("BreathingRig").localScale.y)>.00005f;magic|=home.cat.MagicBusy;
                if(Time.realtimeSinceStartup>=next){Capture("Builds/Previews/Part09B/frame-"+(sample++).ToString("D3")+".png",450,800);next=Time.realtimeSinceStartup+.125f;}
                yield return null;
            }
            Check(min<.999f&&max>1.001f&&min>=.9939f&&max<=1.0061f,"PLAY nefesi en fazla ±%0.6");
            Check(Mathf.Abs(frame.material.GetFloat("_Sweep")-sweepStart)>.01f,"Altın ışık kenar boyunca ilerliyor");
            Check(hit.rect==bounds&&hit.anchorMin==anchorMin&&hit.anchorMax==anchorMax&&hit.localScale==hitScale,"PLAY dokunma RectTransform'u birebir aynı");
            Check(points.Select((p,i)=>premium.IsRaycastLocationValid(p,null)==hits[i]).All(v=>v),"63 dokunma noktası nefes animasyonundan etkilenmedi");
            for(int i=0;i<architecture.Length;i++)Check(architecture[i].localPosition==positions[i]&&architecture[i].localScale==scales[i],names[i]+" mimari transform'u sabit");
            Check(potionMoved&&catMoved&&magic,"Mevcut şişe salınımı, kedi nefesi ve otomatik değnek hareketi devam ediyor");
            Check(Mathf.Abs(moteY-particleRoot.Find("GoldMote_0").localPosition.y)>.1f,"Mevcut büyülü parçacıklar yavaş hareket ediyor");
            Check(home.GetComponentsInChildren<Transform>(true).Length==objectCount,"6.5sn canlılık sırasında nesne sayısı sabit");
            Check(architecture.Select(a=>a.GetComponent<Image>().material).Distinct().Count()==3,"Üç katmana özel paylaşılan maskeli materyal; frame başına üretim yok");
            AuditShader(architecture);
            foreach(int h in new[]{1600,1950,2000})Capture(Output+"Home-"+h+".png",900,h,h==2000);
            var point=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,hit.TransformPoint(hit.rect.center))};
            ExecuteEvents.Execute(play.gameObject,point,ExecuteEvents.pointerDownHandler);float down=Time.realtimeSinceStartup+.16f;while(Time.realtimeSinceStartup<down)yield return null;
            Check(visual.localScale.x<.98f&&visual.GetComponent<Image>().sprite==premium.pressed,"Basma animasyonu nefese üstün; mevcut pressed sprite korunuyor");
            Check(frame.material.GetFloat("_Sweep")==-1,"Basma sırasında kenar geçişi duruyor");Capture(Output+"Play-Pressed.png",900,1600);
            ExecuteEvents.Execute(play.gameObject,point,ExecuteEvents.pointerExitHandler);yield return null;ExecuteEvents.Execute(play.gameObject,point,ExecuteEvents.pointerUpHandler);
            var raycasts=new List<RaycastResult>();EventSystem.current.RaycastAll(point,raycasts);Check(raycasts.Count>0&&raycasts[0].gameObject==play.gameObject,"Efektler PLAY dokunmasını yakalamıyor");
            ExecuteEvents.Execute(play.gameObject,point,ExecuteEvents.pointerClickHandler);play.onClick.Invoke();Check(premium.AcceptedClicks==1&&premium.TransitionQueued,"Çift tıklama koruması aynı");
            Check(SceneManager.GetActiveScene().name=="Home","Mevcut basma bırakma gecikmesi korunuyor");float deadline=Time.realtimeSinceStartup+15;
            while(SceneManager.GetActiveScene().name!="Gameplay"){if(Time.realtimeSinceStartup>deadline)throw new Exception("Gameplay geçişi zaman aşımı");yield return null;}
            yield return null;yield return null;Check(GameObject.Find("Bottle 1")!=null,"Gerçek Gameplay açıldı");Capture(Output+"Gameplay.png",900,1600);
            GameObject.Find("Home").GetComponent<Button>().onClick.Invoke();yield return null;yield return null;
            Check(SceneManager.GetActiveScene().name=="Home"&&FindObjectsByType<HomeEnvironment>(FindObjectsSortMode.None).Length==1,"Home'a dönüş; mimari/efekt component'i tek");
            Check(FindObjectsByType<PremiumPlayButton>(FindObjectsSortMode.None).Length==1,"Dönüşte tek PLAY sunucusu");
            Check(FindFirstObjectByType<HomeCharacterPresenter>().cat.PoolSize==18,"Kedi havuzu dönüşte18 olarak korundu");Capture(Output+"Home-Return.png",900,1600);
            Check(PlayerPrefs.HasKey(GameplayProgress.StorageKey)==hadSave&&PlayerPrefs.GetString(GameplayProgress.StorageKey,"")==realSave,"Gerçek oyuncu kaydı değişmedi");
        }
        void AuditShader(RectTransform[] layers)
        {
            foreach(var layer in layers)
            {
                var image=layer.GetComponent<Image>();var mat=image.material;var mask=(Texture2D)mat.GetTexture("_EffectMask");Check(mask!=null&&mask.width==256&&mask.height==512,layer.name+" veri maskesi mevcut");
                var rt=new RenderTexture(256,512,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);var read=new Texture2D(256,512,TextureFormat.RGBA32,false,true);var active=RenderTexture.active;float time=mat.GetFloat("_EnvironmentTime");
                try{
                    RenderTexture.active=rt;GL.Clear(true,true,Color.clear);mat.SetFloat("_EnvironmentTime",0);Graphics.Blit(image.sprite.texture,rt,mat);RenderTexture.active=rt;read.ReadPixels(new Rect(0,0,256,512),0,0);read.Apply();var first=read.GetPixels32();
                    GL.Clear(true,true,Color.clear);mat.SetFloat("_EnvironmentTime",2.8f);Graphics.Blit(image.sprite.texture,rt,mat);read.ReadPixels(new Rect(0,0,256,512),0,0);read.Apply();var second=read.GetPixels32();var masks=mask.GetPixels32();
                    int water=0,window=0,outside=0,unchangedOutside=0;
                    for(int y=1;y<511;y++)for(int x=1;x<255;x++){
                        int n=y*256+x;var a=first[n];var b=second[n];bool differs=a.r!=b.r||a.g!=b.g||a.b!=b.b||a.a!=b.a;
                        if(masks[n].r>20&&differs)water++;if(masks[n].g>20&&differs)window++;
                        bool clear=true;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){var c=masks[n+dy*256+dx];clear&=c.r==0&&c.g==0;}
                        if(clear){outside++;if(!differs)unchangedOutside++;}
                    }
                    Check(water>10,layer.name+" gerçek GPU render'ında su akıyor");Check(window>2,layer.name+" seçilmiş pencere ışıkları değişiyor");Check(outside>10000&&outside==unchangedOutside,layer.name+" maske dışı pikseller birebir sabit");
                    shaderChecks.Add(new{layer=layer.name,waterChangedPixels=water,windowChangedPixels=window,outsidePixels=outside,unchangedOutside});
                }finally{mat.SetFloat("_EnvironmentTime",time);RenderTexture.active=active;DestroyImmediate(read);rt.Release();DestroyImmediate(rt);}
            }
        }
        void Capture(string filename,int width,int height,bool inset=false)
        {
            var canvas=FindFirstObjectByType<Canvas>();var camera=Camera.main;var p=FindFirstObjectByType<HomeCharacterPresenter>();var safe=p!=null?(RectTransform)p.transform.Find("UI"):null;
            var mode=canvas.renderMode;var oldCam=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;Vector2 min=safe!=null?safe.anchorMin:Vector2.zero,max=safe!=null?safe.anchorMax:Vector2.one;
            var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try{
                camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;if(inset&&safe!=null){safe.anchorMin=new Vector2(.035f,.04f);safe.anchorMax=new Vector2(.965f,.955f);}
                Canvas.ForceUpdateCanvases();if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}Canvas.ForceUpdateCanvases();
                camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(filename,tex.EncodeToPNG());
            }finally{camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCam;canvas.planeDistance=distance;if(safe!=null){safe.anchorMin=min;safe.anchorMax=max;}RenderTexture.active=oldActive;DestroyImmediate(tex);rt.Release();DestroyImmediate(rt);Canvas.ForceUpdateCanvases();if(p!=null){p.Layout();p.GetComponent<HomeEnvironment>().Layout();p.GetComponent<HomePotionDisplay>().Layout();}}
        }
        void Finish(string error)
        {
            Application.logMessageReceived-=Log;if(errors.Count>0)error=(error??"")+string.Join("\n",errors);
            File.WriteAllText(Output+"HOME_MOTION_TESTS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=error==null,error,checks,shaderChecks,runtimeErrors=errors,seconds=Time.realtimeSinceStartup-started},Newtonsoft.Json.Formatting.Indented));GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();EditorApplication.isPlaying=false;
        }
    }
}
#endif
