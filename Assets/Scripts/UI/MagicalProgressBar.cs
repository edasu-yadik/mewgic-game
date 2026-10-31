using L=Mewgic.Localization.L;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Mewgic.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class MagicalProgressBar:MonoBehaviour
    {
        public ProgressBarStyle style;
        public float minimum,maximum=5,current;
        public bool loaded=true,locked;
        public string suffix="";
        public float Percent {get;private set;}
        public float DisplayedPercent {get;private set;}
        public TMP_Text label;
        MagicalBarGraphic art;float celebration;
        public static MagicalProgressBar Create(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform));var r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
            var bar=go.AddComponent<MagicalProgressBar>();bar.Build();return bar;
        }
        public void Build()
        {
            if(art!=null)return;if(style==null)style=Resources.Load<ProgressBarStyle>("UI/ProgressStyle");
            art=GetComponent<MagicalBarGraphic>();if(art==null)art=gameObject.AddComponent<MagicalBarGraphic>();art.raycastTarget=false;art.owner=this;
            if(label!=null)return;
            var g=new GameObject("ProgressValue",typeof(RectTransform));g.transform.SetParent(transform,false);var r=(RectTransform)g.transform;r.anchorMin=new Vector2(.08f,0);r.anchorMax=new Vector2(.92f,1);r.offsetMin=r.offsetMax=Vector2.zero;
            label=g.AddComponent<Mewgic.Localization.LocalizedTMP>();MewgicTypographySettings.Prepare(label);label.alignment=TextAlignmentOptions.Midline;label.color=Color.white;label.fontSize=19;label.enableAutoSizing=true;label.fontSizeMin=14;label.fontSizeMax=21;
        }
        static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        public static float Normalize(float min,float max,float value,bool ready=true)
        {if(!ready||!Finite(min)||!Finite(max)||!Finite(value)||max<=min)return 0;return (float)System.Math.Clamp(((double)value-min)/((double)max-min),0,1);}
        public void SetValue(float value,float max,float min=0,bool ready=true,bool isLocked=false,bool animate=true)
        {
            Build();float previous=Percent;minimum=min;maximum=max;current=value;loaded=ready;locked=isLocked;Percent=Normalize(min,max,value,ready);
            if(!animate)DisplayedPercent=Percent;if(previous<1&&Percent>=1&&!locked)celebration=.7f;
            label.text=!ready?"…":locked?L.V("common.locked"):(!Finite(max)||max<=min)?"0/0":Mathf.Clamp(Finite(value)?value:min,min,max).ToString("0")+"/"+max.ToString("0")+suffix;
            art.canvasRenderer.SetAlpha(!ready?.3f:locked?.5f:1);label.color=locked?new Color(.72f,.66f,.83f):Color.white;art.SetVerticesDirty();
        }
        void OnEnable(){Build();SetValue(current,maximum,minimum,loaded,locked,false);}
        void Update(){float old=DisplayedPercent;DisplayedPercent=Mathf.MoveTowards(old,Percent,Time.unscaledDeltaTime/Mathf.Max(.01f,style.changeSeconds));celebration=Mathf.Max(0,celebration-Time.unscaledDeltaTime);if((DisplayedPercent>0&&!locked&&loaded)||old!=DisplayedPercent||celebration>0)art.SetVerticesDirty();}
        public float Celebration=>celebration;
    }
}
