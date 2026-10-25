using System;using UnityEngine;using UnityEngine.UI;
namespace Mewgic
{
    public sealed class GameplayEndScreen : MonoBehaviour
    {
        RectTransform root;Image[] stars=new Image[3],sparks=new Image[24];Text reward,summary;Button next;int awarded;float clock;Func<bool> paused;bool showing;
        public GameObject Panel=>root.gameObject;
        public bool IsShowing=>showing;
        public void Mount(RectTransform safe,Action replay,Action advance,Action home,Func<bool> isPaused)
        {
            paused=isPaused;var panel=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");var star=Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
            root=GameplayTools.Art(safe,"Win panel",panel,new Vector2(.08f,.29f),new Vector2(.92f,.745f),true).rectTransform;root.GetComponent<Image>().raycastTarget=true;
            GameplayTools.Label(root,"Success","BÜYÜ TAMAMLANDI!",38,new Vector2(.05f,.82f),new Vector2(.95f,.94f));
            for(int i=0;i<3;i++){stars[i]=GameplayTools.Art(root,"AwardStar"+(i+1),star,new Vector2(.20f+i*.21f,.62f),new Vector2(.38f+i*.21f,.82f));stars[i].preserveAspect=true;}
            summary=GameplayTools.Label(root,"ResultSummary","",25,new Vector2(.08f,.54f),new Vector2(.92f,.63f));
            reward=GameplayTools.Label(root,"Reward","",25,new Vector2(.08f,.40f),new Vector2(.92f,.55f));
            next=Control("NextLevel","SONRAKİ BÖLÜM",new Vector2(.12f,.275f),new Vector2(.88f,.39f),advance,panel);
            Control("Replay","TEKRAR OYNA",new Vector2(.12f,.145f),new Vector2(.88f,.26f),replay,panel);
            Control("ResultHome",GameplayLevelCatalog.FromMap?"HARİTA":"ANA MENÜ",new Vector2(.12f,.025f),new Vector2(.88f,.13f),home,panel);
            for(int i=0;i<sparks.Length;i++){var im=GameplayTools.Art(root,"GoldParticle"+i,star,Vector2.zero,Vector2.zero);im.preserveAspect=true;im.raycastTarget=false;im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,.5f);im.rectTransform.sizeDelta=Vector2.one*(5+i%4*2);sparks[i]=im;}
            root.gameObject.SetActive(false);
        }
        Button Control(string name,string text,Vector2 min,Vector2 max,Action action,Sprite panel)
        {var im=GameplayTools.Art(root,name,panel,min,max,true);im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>action());GameplayTools.Label(im.transform,"Label",text,27,new Vector2(.05f,.12f),new Vector2(.95f,.88f));return b;}
        public void Show(int level,int moves,int starCount,bool granted,bool saved,bool hasNext)
        {
            if(showing)return;showing=true;awarded=starCount;clock=0;root.gameObject.SetActive(true);root.SetAsLastSibling();
            summary.text="Bölüm "+level+" • "+moves+" hamle • "+starCount+" yıldız";
            reward.text=!saved?"Ödül kaydedilemedi; tekrar deneyebilirsin.":granted?"İlk tamamlama ödülü: +1 İpucu\nHak kalıcı olarak kaydedildi.":"Bu bölümün ödülü daha önce alındı.\nEn iyi yıldız sonucun korunur.";
            next.interactable=hasNext;next.transform.Find("Label").GetComponent<Text>().text=hasNext?"SONRAKİ BÖLÜM":"SON BÖLÜM TAMAMLANDI";
            Animate();
        }
        public void Hide(){showing=false;if(root!=null)root.gameObject.SetActive(false);}
        void Update(){if(showing&&!paused()){clock+=Time.unscaledDeltaTime;Animate();}}
        void Animate()
        {
            for(int i=0;i<3;i++){float t=Mathf.Clamp01((clock-i*.17f)/.4f);stars[i].rectTransform.localScale=Vector3.one*Mathf.Lerp(.05f,1,Mathf.Sin(t*Mathf.PI*.5f));stars[i].color=i<awarded?Color.white:new Color(.3f,.24f,.4f,.75f);}
            for(int i=0;i<sparks.Length;i++){float t=Mathf.Repeat(clock*.45f+i*.071f,1);var r=sparks[i].rectTransform;r.anchoredPosition=new Vector2(Mathf.Sin(i*2.4f)*root.rect.width*.43f,Mathf.Lerp(root.rect.height*.35f,-root.rect.height*.15f,t));r.localRotation=Quaternion.Euler(0,0,clock*25+i*13);sparks[i].color=new Color(1,.8f,.3f,clock<2.3f?Mathf.Sin(t*Mathf.PI)*.8f:0);}
        }
    }
}
