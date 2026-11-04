using System.Collections;using Mewgic.Core;using UnityEngine;using UnityEngine.UI;using TMPro;using L=Mewgic.Localization.L;
namespace Mewgic.Daily
{
 public sealed class DailyArrivalPopup:MonoBehaviour
 {
  static long offeredForGift=long.MinValue;RectTransform safeArea;public System.Action OnGranted;GameObject root;RectTransform chest;TMP_Text result;Button open,close,dismiss;bool opening;Sprite openArt;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession()=>offeredForGift=long.MinValue;
  public void Mount(RectTransform safe,bool manual=false){safeArea=safe;if(root!=null&&root.activeSelf)return;if(!GameplayProgress.Current.GiftReady)return;long receipt=GameplayProgress.Current.LastGiftUtc;if(!manual&&offeredForGift==receipt)return;offeredForGift=receipt;if(root!=null)Destroy(root);
   var art=Resources.Load<DailyArt>("Daily/Art");openArt=art.chest;var closedArt=Resources.Load<Mewgic.Rewards.RewardHubArt>("Rewards/Art").closedChest;var panel=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
   var dim=GameplayTools.Art(safe,"DailyArrival",null,Vector2.zero,Vector2.one);dim.color=new Color(0,0,.08f,.88f);dim.raycastTarget=true;root=dim.gameObject;
   var card=GameplayTools.Art(root.transform,"DailyArrivalCard",panel,new Vector2(.07f,.24f),new Vector2(.93f,.79f),true);
   GameplayTools.Label(card.transform,"DailyArrivalTitle",L.V("daily.gift"),38,new Vector2(.06f,.82f),new Vector2(.94f,.96f));
   var picture=GameplayTools.Art(card.transform,"ArrivalChest",closedArt,new Vector2(.18f,.30f),new Vector2(.82f,.81f));picture.preserveAspect=true;chest=picture.rectTransform;
   result=GameplayTools.Label(card.transform,"ArrivalReward",L.V("reward.daily",ProgressLedger.DailyGiftCoins,ProgressLedger.DailyGiftHints),27,new Vector2(.06f,.21f),new Vector2(.94f,.33f));
   open=Control(card.transform,"ArrivalOpen",L.V("reward.open"),new Vector2(.18f,.07f),new Vector2(.82f,.20f),()=>OpenReward(receipt),panel);
   close=Control(card.transform,"ArrivalContinue",L.V("common.ok"),new Vector2(.18f,.07f),new Vector2(.82f,.20f),()=>root.SetActive(false),panel);close.gameObject.SetActive(false);dismiss=Control(card.transform,"ArrivalDismiss",L.V("common.close"),new Vector2(.73f,.01f),new Vector2(.97f,.07f),()=>root.SetActive(false),panel);root.transform.SetAsLastSibling();
  }
  Button Control(Transform p,string n,string text,Vector2 a,Vector2 b,UnityEngine.Events.UnityAction action,Sprite sprite){var im=GameplayTools.Art(p,n,sprite,a,b,true);im.raycastTarget=true;var btn=im.gameObject.AddComponent<Button>();btn.targetGraphic=im;btn.onClick.AddListener(action);GameplayTools.Label(p.Find(n),"Label",text,27,new Vector2(.04f,.1f),new Vector2(.96f,.9f));return btn;}
  void Update()
  {
   if(root!=null&&root.activeSelf&&!opening&&open.gameObject.activeSelf&&!GameFeel.Reduced)
    chest.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.unscaledTime*2)*2);
  }
  void OnApplicationPause(bool pause){if(!pause&&safeArea!=null&&(root==null||!root.activeSelf))Mount(safeArea);}
  void OpenReward(long receipt)
  {
   if(opening)return;
   var ledger=GameplayProgress.Current;
   if(!ledger.ClaimDailyGift(receipt))
   {
    result.text=ledger.GiftClockBlocked?L.V("error.clock"):ledger.GiftReady?L.V("error.save"):L.V("daily.wait");
    open.interactable=ledger.GiftReady;
    return;
   }
   opening=true;
   open.interactable=false;
   dismiss.gameObject.SetActive(false);
   GameFeel.Play(GameCue.Chest);GameFeel.Play(GameCue.Coin);
   OnGranted?.Invoke();
   StartCoroutine(OpenChest());
  }
  IEnumerator OpenChest()
  {
   for(float t=GameFeel.Reduced?1:0;t<1;t+=Time.unscaledDeltaTime)
   {
    if(t>.7f)chest.GetComponent<Image>().sprite=openArt;
    chest.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*35)*5*(1-t));
    chest.localScale=Vector3.one*(1+Mathf.Sin(t*Mathf.PI)*.12f);
    yield return null;
   }
   chest.GetComponent<Image>().sprite=openArt;
   chest.localRotation=Quaternion.identity;
   chest.localScale=Vector3.one;
   result.text=L.V("reward.daily",ProgressLedger.DailyGiftCoins,ProgressLedger.DailyGiftHints);
   open.gameObject.SetActive(false);
   close.gameObject.SetActive(true);
   opening=false;
  }
 }
}