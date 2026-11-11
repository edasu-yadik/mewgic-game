using Mewgic.Core;
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
        Image costumeBody;
        Sprite wizardBody;
        RectTransform bodyTransform;
        GameObject wandBack, wandFront;
        Vector2 baseBodySize, baseBodyPosition;
        public ShopItem WornCostume => costume;
        public bool WearingCostumeArt => costumeBody != null && wizardBody != null && costumeBody.sprite != wizardBody;
        int cosmeticRevision = -1;
        ShopItem costume, effect;
        bool playerCosmetics;

        public void BindPlayerCosmetics()
        {
            foreach (var image in GetComponentsInChildren<Image>(true))
                if (image.name == "Body") { costumeBody = image; break; }
            if (costumeBody != null && wizardBody == null)
            {
                wizardBody = costumeBody.sprite;
                bodyTransform = costumeBody.rectTransform;
                baseBodySize = bodyTransform.sizeDelta;
                baseBodyPosition = bodyTransform.anchoredPosition;
                wandBack = bodyTransform.parent.Find("WandBackPivot")?.gameObject;
                wandFront = bodyTransform.parent.Find("WandFrontPivot")?.gameObject;
            }
            playerCosmetics = true;
            RefreshCosmetics();
        }

        void RefreshCosmetics()
        {
            var ledger = GameplayProgress.Current;
            if (cosmeticRevision == ledger.Revision) return;
            cosmeticRevision = ledger.Revision;
            var next = ledger.Equipped(ShopCategory.Cats);
            if (next != costume && costumeBody != null)
            {
                costume = next;
                ApplyCostumePose(MagicBusy ? CostumePose.Victory : CostumePose.Front);
            }
            effect = ledger.Equipped(ShopCategory.Effects);
        }
        void ApplyCostumePose(CostumePose pose)
        {
            if (costumeBody == null) return;
            var rendered = CostumeArt.Get(costume, pose) ?? CostumeArt.Get(costume, CostumePose.Front);
            costumeBody.sprite = rendered != null ? rendered : wizardBody;
            costumeBody.material = !CostumeArt.IsReferenceCostume(costume) && rendered == null
                ? ShopCosmetics.CostumeMaterial(costume) : null;
            bool usesWizard = rendered == null;
            bodyTransform.sizeDelta = usesWizard ? baseBodySize : new Vector2(baseBodySize.x, baseBodySize.y * .82f);
            bodyTransform.anchoredPosition = usesWizard ? baseBodyPosition :
                baseBodyPosition + Vector2.up * (baseBodySize.y * .08f);
            if (wandBack != null) wandBack.SetActive(usesWizard);
            if (wandFront != null) wandFront.SetActive(usesWizard);
            if (blinkOverlay != null) blinkOverlay.gameObject.SetActive(usesWizard);
        }

        [Range(4,24)] public int sparkBudget=18;
        RectTransform[] sparks;
        WizardSparkGraphic[] graphics;
        Vector2[] velocities;
        Vector2[] spawnOffsets;
        Vector2 burstOrigin;
        float magicTime=-1;
        const float MagicDuration=1.6f;
        public bool CanBlink => blinkOverlay != null && blinkOverlay.gameObject.activeSelf;
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
            if (playerCosmetics) ApplyCostumePose(CostumePose.Victory);
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
            if(playerCosmetics)RefreshCosmetics();
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
                var tint=ShopCosmetics.EffectTint(effect);tint.a=alpha;graphics[i].color=tint;
                sparks[i].localScale=Vector3.one*(.45f+.55f*alpha);
            }
            if(magicTime>=MagicDuration){magicTime=-1;ActiveSparks=0;foreach(var s in sparks)s.gameObject.SetActive(false);if(playerCosmetics)ApplyCostumePose(CostumePose.Front);}
        }
        void OnDisable()
        {
            magicTime=-1;ActiveSparks=0;
            if(sparks!=null)foreach(var s in sparks)if(s!=null)s.gameObject.SetActive(false);
        }
    }
}
