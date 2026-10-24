using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Characters
{
    public sealed class WizardCatPresentation : MonoBehaviour
    {
        public Animator animator;
        public RectTransform magicOrigin;
        public RectTransform fxRoot;
        public CanvasGroup blinkOverlay;
        public bool automaticBlink=true;
        float nextBlinkTime;
        [Range(4,24)] public int sparkBudget=18;
        RectTransform[] sparks;
        WizardSparkGraphic[] graphics;
        Vector2[] velocities;
        Vector2[] spawnOffsets;
        Vector2 burstOrigin;
        float magicTime=-1;
        const float MagicDuration=1.6f;
        public bool CanBlink => blinkOverlay != null;
        public bool MagicBusy => magicTime>=0;
        public int ActiveSparks {get;private set;}
        public int PoolSize => sparks==null?0:sparks.Length;

        void Awake()
        {
            nextBlinkTime=Time.unscaledTime+5.2f;
            int count=Mathf.Clamp(sparkBudget,4,24);
            sparks=new RectTransform[count];graphics=new WizardSparkGraphic[count];velocities=new Vector2[count];spawnOffsets=new Vector2[count];
            for(int i=0;i<count;i++)
            {
                var go=new GameObject("GoldSpark_"+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(WizardSparkGraphic));
                var rt=go.GetComponent<RectTransform>();rt.SetParent(fxRoot,false);
                rt.anchorMin=rt.anchorMax=fxRoot.pivot;rt.sizeDelta=new Vector2(25,25);
                sparks[i]=rt;graphics[i]=go.GetComponent<WizardSparkGraphic>();
                graphics[i].raycastTarget=false;go.SetActive(false);
            }
        }
        public bool TriggerBlink()
        {
            if(!CanBlink)return false;
            animator.SetTrigger("Blink");return true;
        }
        public bool TriggerMagic()
        {
            if(MagicBusy)return false;
            magicTime=0;animator.SetTrigger("Magic");
            burstOrigin=fxRoot.InverseTransformPoint(magicOrigin.position);
            for(int i=0;i<sparks.Length;i++)
            {
                float a=i*2.39996f;float speed=64+(i%5)*17;
                spawnOffsets[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(85+(i%3)*10);
                velocities[i]=new Vector2(Mathf.Cos(a)*speed,Mathf.Sin(a)*speed+28);
                sparks[i].gameObject.SetActive(false);
            }
            return true;
        }
        void Update()
        {
            if(automaticBlink && CanBlink && Time.unscaledTime>=nextBlinkTime){TriggerBlink();nextBlinkTime=Time.unscaledTime+Random.Range(4.5f,7f);}
            if(magicTime<0)return;
            magicTime+=Time.unscaledDeltaTime;ActiveSparks=0;
            float age=magicTime-.24f;
            for(int i=0;i<sparks.Length;i++)
            {
                float t=age-i*.012f;
                bool active=t>=0&&t<.85f;
                sparks[i].gameObject.SetActive(active);
                if(!active)continue;
                ActiveSparks++;
                sparks[i].anchoredPosition=burstOrigin+spawnOffsets[i]+velocities[i]*t+new Vector2(0,-24*t*t);
                sparks[i].localRotation=Quaternion.Euler(0,0,i*27+t*70);
                float alpha=Mathf.Sin(Mathf.Clamp01(t/.85f)*Mathf.PI);
                graphics[i].color=new Color(1,.76f,.22f,alpha);
                sparks[i].localScale=Vector3.one*(.45f+.55f*alpha);
            }
            if(magicTime>=MagicDuration){magicTime=-1;ActiveSparks=0;foreach(var s in sparks)s.gameObject.SetActive(false);}
        }
        void OnDisable()
        {
            magicTime=-1;ActiveSparks=0;
            if(sparks!=null)foreach(var s in sparks)if(s!=null)s.gameObject.SetActive(false);
        }
    }
}
