using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;using Mewgic.Core;
using System;using UnityEngine;using UnityEngine.UI;
namespace Mewgic
{
    public sealed class GameplayEndScreen : MonoBehaviour
    {
        RectTransform root;Image backdrop;Image[] stars=new Image[3],sparks=new Image[24];TMP_Text reward,summary,breakdown;Button next,skip;int awarded;float clock;Func<bool> paused;bool showing;CompletionReward payout;bool saveOk;
        public GameObject Panel=>root.gameObject;
        public bool IsShowing=>showing;
        public void Mount(RectTransform safe,Action replay,Action advance,Action home,Func<bool> isPaused,Action retry)
        {
            paused=isPaused;var panel=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");var star=Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
            backdrop=GameplayTools.Art(safe,"VictoryBackdrop",null,Vector2.zero,Vector2.one);backdrop.color=new Color(.035f,.022f,.075f,.57f);backdrop.raycastTarget=true;backdrop.gameObject.SetActive(false);
            root=GameplayTools.Art(safe,"Win panel",panel,new Vector2(.08f,.22f),new Vector2(.92f,.79f),true).rectTransform;root.GetComponent<Image>().raycastTarget=true;
            GameplayTools.Label(root,"Success",L.V("gameplay.success"),38,new Vector2(.05f,.82f),new Vector2(.95f,.94f));
            for(int i=0;i<3;i++){stars[i]=GameplayTools.Art(root,"AwardStar"+(i+1),star,new Vector2(.20f+i*.21f,.62f),new Vector2(.38f+i*.21f,.82f));stars[i].preserveAspect=true;}
            summary=GameplayTools.Label(root,"ResultSummary","",25,new Vector2(.08f,.54f),new Vector2(.92f,.63f));
            reward=GameplayTools.Label(root,"Reward","",25,new Vector2(.08f,.46f),new Vector2(.92f,.54f));
            breakdown=GameplayTools.Label(root,"RewardBreakdown","",22,new Vector2(.08f,.35f),new Vector2(.92f,.46f));
            skip=Control("SkipReward","",new Vector2(.19f,.245f),new Vector2(.81f,.33f),()=>{if(!saveOk)retry();else{clock=2.4f;Animate();}},panel);
            next=Control("NextLevel",L.V("gameplay.next"),new Vector2(.12f,.135f),new Vector2(.88f,.23f),advance,panel);
            Control("Replay",L.V("gameplay.replay"),new Vector2(.085f,.025f),new Vector2(.485f,.12f),replay,panel);
            Control("ResultHome",GetComponent<PrototypeApp>()?.IsTutorialReplay!=true?L.V("common.map"):L.V("common.home"),new Vector2(.515f,.025f),new Vector2(.915f,.12f),home,panel);
            for(int i=0;i<sparks.Length;i++){var im=GameplayTools.Art(root,"GoldParticle"+i,star,Vector2.zero,Vector2.zero);im.preserveAspect=true;im.raycastTarget=false;im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,.5f);im.rectTransform.sizeDelta=Vector2.one*(5+i%4*2);sparks[i]=im;}
            root.gameObject.SetActive(false);
        }
        Button Control(string name,string text,Vector2 min,Vector2 max,Action action,Sprite panel)
        {var im=GameplayTools.Art(root,name,panel,min,max,true);im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>action());GameplayTools.Label(im.transform,"Label",text,27,new Vector2(.05f,.12f),new Vector2(.95f,.88f));return b;}
        public void Show(int level,int moves,int starCount,bool saved,bool hasNext,CompletionReward coins)
        {
            if(showing)return;payout=coins;saveOk=saved;showing=true;awarded=starCount;clock=0;backdrop.gameObject.SetActive(true);backdrop.transform.SetAsLastSibling();root.gameObject.SetActive(true);root.SetAsLastSibling();
            summary.text=L.V("gameplay.result",level,moves,starCount);
            reward.text=saved?"":L.V("error.save");
            breakdown.text=saved?L.V("economy.breakdown",payout.Base,payout.Stars,payout.FirstAttempt,payout.NoHint):"";
            skip.transform.Find("Label").GetComponent<TMP_Text>().text=L.V(saved?"economy.skip":"economy.retry_save");
            next.interactable=hasNext;next.transform.Find("Label").GetComponent<TMP_Text>().text=hasNext?L.V("gameplay.next"):L.V("gameplay.last");
            Animate();
        }
        public void Hide(){showing=false;if(root!=null){root.gameObject.SetActive(false);backdrop.gameObject.SetActive(false);}}
        void Update(){if(showing&&!paused()&&clock<2.4f){clock+=Time.unscaledDeltaTime;Animate();}}
        void Animate()
        {
            root.localScale=Vector3.one*Mathf.Lerp(.94f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(clock/.35f)));
            if(saveOk)reward.text=L.V("reward.coins",Mathf.RoundToInt(payout.Coins*Mathf.SmoothStep(0,1,Mathf.Clamp01(clock/1.2f))));
            skip.gameObject.SetActive(!saveOk||clock<2.4f);
            for(int i=0;i<3;i++){float t=Mathf.Clamp01((clock-i*.17f)/.4f);stars[i].rectTransform.localScale=Vector3.one*Mathf.Lerp(.05f,1,Mathf.Sin(t*Mathf.PI*.5f));stars[i].color=i<awarded?Color.Lerp(new Color(1,.75f,.2f,1),Color.white,Mathf.Clamp01((clock-i*.17f)/.8f)):new Color(.3f,.24f,.4f,.75f);}
            for(int i=0;i<sparks.Length;i++){float t=Mathf.Repeat(clock*.45f+i*.071f,1);var r=sparks[i].rectTransform;r.anchoredPosition=new Vector2(Mathf.Sin(i*2.4f)*root.rect.width*.43f,Mathf.Lerp(root.rect.height*.35f,-root.rect.height*.15f,t));r.localRotation=Quaternion.Euler(0,0,clock*25+i*13);sparks[i].color=new Color(1,.8f,.3f,clock<2.3f?Mathf.Sin(t*Mathf.PI)*.8f:0);}
        }
    }
}
