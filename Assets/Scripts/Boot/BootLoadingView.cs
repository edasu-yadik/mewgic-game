using TMPro;using Mewgic.UI;
using L=Mewgic.Localization.L;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Mewgic
{
    public sealed class BootLoadingView : MonoBehaviour
    {
        public float DisplayProgress {get;private set;}
        public float Opacity {set {if(group!=null)group.alpha=value;}}
        public RectTransform Safe {get;private set;}
        public RectTransform Logo {get;private set;}
        public RectTransform Character {get;private set;}
        public RectTransform Bar {get;private set;}
        public TMP_Text Message {get;private set;}
        public Button RetryButton {get;private set;}
        CanvasGroup group; BootProgressGraphic fill; TMP_Text percentage; float target;
        public float RenderedProgress=>fill!=null?fill.Value:0;
        GameObject input; Rect lastSafe;int lastWidth,lastHeight;bool failed;
#if UNITY_EDITOR
        public Rect? EditorSafeArea;
#endif
        public void Mount(Texture2D background,Texture2D logo,Texture2D cat,Action retry)
        {
            var root=new GameObject("Mewgic Opening Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(CanvasGroup));root.transform.SetParent(transform,false);
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
            group=root.GetComponent<CanvasGroup>();
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(900,1600);scaler.matchWidthOrHeight=.5f;
            var back=Raw(root.transform,"Fantasy Background",background,Vector2.zero,Vector2.one,AspectRatioFitter.AspectMode.EnvelopeParent);
            back.GetComponent<RawImage>().color=background!=null?Color.white:new Color(.08f,.025f,.16f);
            Safe=Rect(root.transform,"Opening Safe Area",Vector2.zero,Vector2.one);ApplySafeArea();
            Logo=Raw(Safe,"Mewgic Logo",logo,new Vector2(.065f,.77f),new Vector2(.935f,.95f));
            var catBox=Rect(Safe,"Wizard Cat Float",new Vector2(.12f,.285f),new Vector2(.88f,.77f));
            Character=Raw(catBox,"Wizard Cat",cat,Vector2.zero,Vector2.one);
            var star=Rect(Character,"Wand Star Glow",new Vector2(.76f,.48f),new Vector2(.94f,.62f));star.gameObject.AddComponent<BootMagicGraphic>().glow=true;
            Rect(Safe,"Magic Motes",new Vector2(.06f,.25f),new Vector2(.94f,.82f)).gameObject.AddComponent<BootMagicGraphic>().raycastTarget=false;
            Bar=Rect(Safe,"Actual Loading Progress",new Vector2(.105f,.115f),new Vector2(.895f,.14f));fill=Bar.gameObject.AddComponent<BootProgressGraphic>();
            percentage=Label(Safe,"Loading Percentage","0%",22,new Vector2(.4f,.145f),new Vector2(.6f,.176f));
            Message=Label(Safe,"Loading Message",L.V("boot.preparing"),26,new Vector2(.055f,.055f),new Vector2(.945f,.105f));
            var retryRect=Rect(Safe,"Retry Loading",new Vector2(.28f,.185f),new Vector2(.72f,.235f));var retryGraphic=retryRect.gameObject.AddComponent<BootProgressGraphic>();retryGraphic.Value=1;
            RetryButton=retryRect.gameObject.AddComponent<PremiumButton>();RetryButton.targetGraphic=retryGraphic;RetryButton.onClick.AddListener(()=>retry());
            Label(retryRect,"Retry Label",L.V("boot.retry"),23,Vector2.zero,Vector2.one);RetryButton.gameObject.SetActive(false);
            input=new GameObject("Opening EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));input.transform.SetParent(transform,false);input.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            foreach(var graphic in root.GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=graphic.gameObject==retryRect.gameObject;
        }
        public void SetProgress(float value,string message){target=Mathf.Clamp01(value);Message.text=message;}
        public void ResetDisplay(){failed=false;target=DisplayProgress=0;Opacity=1;Message.color=new Color(1,.94f,.8f);RetryButton.gameObject.SetActive(false);}
        public void ShowError(string detail){failed=true;Message.text=detail;Message.color=new Color(1,.86f,.68f);RetryButton.gameObject.SetActive(true);}
        public void ReleaseInput(){if(input!=null){input.SetActive(false);Destroy(input);}}
        void Update()
        {
            ApplySafeArea();
            DisplayProgress=Mathf.MoveTowards(DisplayProgress,target,Time.unscaledDeltaTime*4);
            if(fill!=null)fill.Value=DisplayProgress;
            if(percentage!=null)percentage.text=failed?L.V("gameplay.paused"):Mathf.FloorToInt(DisplayProgress*100)+"%";
            if(Character!=null){Character.anchoredPosition=new Vector2(0,Mathf.Sin(Time.unscaledTime*1.35f)*4);Character.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.unscaledTime*.8f)*.35f);}
        }
        void ApplySafeArea()
        {
            if(Safe==null||Screen.width==0||Screen.height==0)return;var area=Screen.safeArea;
#if UNITY_EDITOR
            if(EditorSafeArea.HasValue)area=EditorSafeArea.Value;
#endif
            if(lastSafe==area&&lastWidth==Screen.width&&lastHeight==Screen.height)return;
            lastSafe=area;lastWidth=Screen.width;lastHeight=Screen.height;
            Safe.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);Safe.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);Safe.offsetMin=Safe.offsetMax=Vector2.zero;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static RectTransform Raw(Transform parent,string name,Texture texture,Vector2 min,Vector2 max,AspectRatioFitter.AspectMode mode=AspectRatioFitter.AspectMode.FitInParent)
        {var box=Rect(parent,name+" Bounds",min,max);var r=Rect(box,name,Vector2.zero,Vector2.one);var image=r.gameObject.AddComponent<RawImage>();image.texture=texture;image.raycastTarget=false;var fit=r.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=mode;fit.aspectRatio=texture!=null?(float)texture.width/texture.height:1;return r;}
        static TMP_Text Label(Transform parent,string name,string message,int size,Vector2 min,Vector2 max)
        {var r=Rect(parent,name,min,max);var t=r.gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();MewgicTypographySettings.Prepare(t);t.text=message;t.fontSize=size;t.alignment=TextAlignmentOptions.Midline;t.color=new Color(1,.94f,.8f);t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=size;t.raycastTarget=false;return t;}
    }
}
