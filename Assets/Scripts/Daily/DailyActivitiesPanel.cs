using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;
using System;using Mewgic.Core;using UnityEngine;using UnityEngine.UI;using UnityEngine.SceneManagement;
namespace Mewgic.Daily
{
 public sealed class DailyActivitiesPanel:MonoBehaviour
 {
  readonly System.Collections.Generic.List<Button> buttons=new System.Collections.Generic.List<Button>();
  Image background;DailyArt art;RectTransform safe,screen,hero,cat;GameObject tasks,gift;TMP_Text status,timer,giftState;MagicalProgressBar[] fills=new MagicalProgressBar[4];TMP_Text[] counts=new TMP_Text[4];Button[] actions=new Button[4];Button giftClaim;PremiumButton tasksTab,giftTab;float refreshAt, timerAt;bool observed;TMP_Text homeTimer;int observedCycleRevision;
  public Action OnClosed;
  public bool IsOpen=>screen!=null&&screen.gameObject.activeSelf;public bool GiftVisible=>gift!=null&&gift.activeSelf;
  static readonly Color Ink=new Color32(64,33,23,255);
  static RectTransform Rect(Transform p,string n,Vector2 a,Vector2 b){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=a;r.anchorMax=b;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
  Image Image(Transform p,string n,Sprite s,Vector2 a,Vector2 b,bool sliced=false){var im=Rect(p,n,a,b).gameObject.AddComponent<Image>();im.sprite=s;im.raycastTarget=false;im.type=sliced?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;if(sliced)im.pixelsPerUnitMultiplier=2;return im;}
  TMP_Text TMP_Text(Transform p,string n,string value,int size,Color color,Vector2 a,Vector2 b){var t=Rect(p,n,a,b).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();t.font=MewgicTypographySettings.Current.body;t.text=value;t.color=color;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=15;t.fontSizeMax=size;t.alignment=TextAlignmentOptions.Midline;t.raycastTarget=false;MewgicTypographySettings.Prepare(t);return t;}
  Button Button(Transform p,string n,string value,Vector2 a,Vector2 b,Action click){var im=Image(p,n,art.button,a,b,false);im.raycastTarget=true;var button=im.gameObject.AddComponent<PremiumButton>();button.targetGraphic=im;buttons.Add(button);button.onClick.AddListener(()=>click());var text=TMP_Text(im.transform,"Label",value,25,Color.white,new Vector2(.05f,.14f),new Vector2(.95f,.86f));return button;}
  public void Mount(RectTransform safeArea)
  {
   safe=safeArea;art=Resources.Load<DailyArt>("Daily/Art");if(art==null)throw new InvalidOperationException("Daily art missing");
   var entry=Image(safe,"DailyHomeEntries",null,new Vector2(.018f,.535f),new Vector2(.225f,.665f));entry.color=Color.clear;
   var mission=Button(entry.transform,"MissionsButton",L.V("daily.missions"),Vector2.zero,Vector2.one,()=>Open(false));
   mission.GetComponent<Image>().color=Color.clear;
   Image(mission.transform,"QuestLabelPlate",art.button,new Vector2(0,.17f),new Vector2(1,.34f)).transform.SetAsFirstSibling();
   Image(mission.transform,"QuestTimerPlate",Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001"),new Vector2(.07f,0),new Vector2(.93f,.17f),true).transform.SetAsFirstSibling();
   var missionLabel=mission.transform.Find("Label").GetComponent<RectTransform>();missionLabel.anchorMin=new Vector2(0,.17f);missionLabel.anchorMax=new Vector2(1,.34f);
   var questIcon=Image(mission.transform,"QuestIcon",Resources.Load<Sprite>("Daily/QuestIcon_v001"),new Vector2(.15f,.35f),new Vector2(.85f,1));questIcon.preserveAspect=true;
   homeTimer=TMP_Text(mission.transform,"HomeQuestCountdown","",19,new Color32(255,231,169,255),new Vector2(0,0),new Vector2(1,.17f));
   observed=GameplayProgress.Current.ObserveDaily();observedCycleRevision=GameplayProgress.Current.Revision;
   screen=Rect(safe,"DailyActivitiesScreen",Vector2.zero,Vector2.one);var block=screen.gameObject.AddComponent<UnityEngine.UI.Image>();block.color=new Color(.025f,.03f,.12f,1);block.raycastTarget=true;
   background=Image(screen,"DailySky",art.sky,Vector2.zero,Vector2.one);background.color=new Color(.65f,.65f,.86f,1);var castle=Image(screen,"DailyCastle",art.castle,Vector2.zero,Vector2.one);castle.preserveAspect=true;castle.color=new Color(.65f,.65f,.83f,1);
   var header=Image(screen,"DailyHeader",Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001"),new Vector2(.02f,.915f),new Vector2(.98f,.995f),true);TMP_Text(header.transform,"Title",L.V("daily.title"),38,Color.white,new Vector2(.04f,.15f),new Vector2(.73f,.85f));Button(header.transform,"DailyClose",L.V("common.back"),new Vector2(.76f,.15f),new Vector2(.96f,.85f),Close);
   hero=Rect(screen,"DailyHero",new Vector2(.01f,.65f),new Vector2(.99f,.915f));var chest=Image(hero,"DailyRewardChest",art.chest,new Vector2(.30f,0),new Vector2(.99f,1));chest.preserveAspect=true;
   var catObject=Instantiate(art.catPrefab,hero,false);catObject.name="DailyWizardCat";cat=catObject.GetComponent<RectTransform>();cat.anchorMin=cat.anchorMax=new Vector2(.23f,.02f);cat.pivot=new Vector2(.5f,0);cat.anchoredPosition=Vector2.zero;foreach(var g in cat.GetComponentsInChildren<Graphic>())g.raycastTarget=false;
   tasksTab=(PremiumButton)Button(screen,"DailyTasksTab",L.V("daily.tasks_tab"),new Vector2(.03f,.59f),new Vector2(.49f,.65f),()=>ShowTab(false));giftTab=(PremiumButton)Button(screen,"DailyGiftTab",L.V("daily.gift_tab"),new Vector2(.51f,.59f),new Vector2(.97f,.65f),()=>ShowTab(true));
   tasks=Image(screen,"DailyTasksPanel",art.parchment,new Vector2(.02f,.12f),new Vector2(.98f,.59f),true).gameObject;
   TMP_Text(tasks.transform,"DailyTasksTitle",L.V("daily.tasks_title"),32,Ink,new Vector2(.04f,.86f),new Vector2(.96f,.98f));
   for(int i=0;i<4;i++){int index=i;float y=.665f-i*.18f;var row=Image(tasks.transform,"DailyTask_"+i,art.parchment,new Vector2(.025f,y),new Vector2(.975f,y+.18f),true);row.color=new Color(1,.94f,.85f,1);var icon=Image(row.transform,"MissionIcon",i==0?art.rewards[2]:i==1||i==2?art.star:art.rewards[1],new Vector2(.025f,.22f),new Vector2(.15f,.78f));icon.preserveAspect=true;
    var title=TMP_Text(row.transform,"TaskTitle",L.V(new[]{"daily.task.bottles","daily.task.wins","daily.task.stars","daily.task.boosters"}[i],DailyRules.Target((DailyTaskKind)i)),24,Ink,new Vector2(.17f,.42f),new Vector2(.65f,.78f));title.alignment=TextAlignmentOptions.MidlineLeft;title.textWrappingMode=TextWrappingModes.Normal;title.overflowMode=TextOverflowModes.Truncate;
    fills[i]=MagicalProgressBar.Create(row.transform,"ProgressTrack",new Vector2(.17f,.17f),new Vector2(.65f,.39f));
    var reward=Image(row.transform,"RewardIcon",art.rewards[(int)DailyRules.Reward((DailyTaskKind)i)],new Vector2(.665f,.32f),new Vector2(.79f,.82f));reward.preserveAspect=true;TMP_Text(row.transform,"RewardAmount",L.V("shop.task_payout",EconomyConfig.Current.dailyTaskBooster,EconomyConfig.Current.DailyGold((DailyTaskKind)i)),21,new Color32(117,63,17,255),new Vector2(.637f,.075f),new Vector2(.788f,.34f));actions[i]=Button(row.transform,"DailyAction_"+i,L.V("common.go"),new Vector2(.79f,.20f),new Vector2(.975f,.79f),()=>TaskAction((DailyTaskKind)index));
   }
   timer=TMP_Text(tasks.transform,"DailyReset","",20,Ink,new Vector2(.05f,.015f),new Vector2(.95f,.10f));
   gift=Image(screen,"DailyGiftPanel",art.parchment,new Vector2(.02f,.12f),new Vector2(.98f,.59f),true).gameObject;TMP_Text(gift.transform,"GiftTitle",L.V("daily.gift"),38,Ink,new Vector2(.07f,.77f),new Vector2(.93f,.94f));var giftReward=Image(gift.transform,"DailyGiftReward",art.rewards[1],new Vector2(.36f,.47f),new Vector2(.64f,.76f));giftReward.preserveAspect=true;TMP_Text(gift.transform,"GiftAmount",L.V("reward.daily",ProgressLedger.DailyGiftCoins,ProgressLedger.DailyGiftHints),32,Ink,new Vector2(.10f,.38f),new Vector2(.90f,.49f));giftState=TMP_Text(gift.transform,"GiftState","",25,Ink,new Vector2(.10f,.22f),new Vector2(.90f,.38f));giftClaim=Button(gift.transform,"ClaimDailyGift",L.V("daily.claim_gift"),new Vector2(.20f,.08f),new Vector2(.80f,.22f),ClaimGift);
   status=TMP_Text(screen,"DailyStatus","",22,Color.white,new Vector2(.05f,.02f),new Vector2(.95f,.105f));screen.gameObject.SetActive(false);
  }
  public void Open(bool showGift){screen.gameObject.SetActive(true);screen.SetAsLastSibling();observed=GameplayProgress.Current.ObserveDaily();status.text=observed?L.V("daily.real_rewards"):L.V(GameplayProgress.Current.DailyClockBlocked?"error.clock":"error.save");ShowTab(showGift);Layout();}
  void ShowTab(bool showGift){tasksTab.SetRole(showGift?ButtonRole.Navigation:ButtonRole.Selected);giftTab.SetRole(showGift?ButtonRole.Selected:ButtonRole.Navigation);tasks.SetActive(!showGift);gift.SetActive(showGift);Refresh();}
  public void Close(){screen.gameObject.SetActive(false);var after=OnClosed;OnClosed=null;after?.Invoke();}
  void ClaimGift(){var popup=gameObject.AddComponent<DailyArrivalPopup>();popup.OnGranted=Refresh;popup.Mount(safe,true);}
  void TaskAction(DailyTaskKind kind){var ledger=GameplayProgress.Current;if(!observed||ledger.DailyClockBlocked)return;int i=(int)kind;var d=ledger.DailySnapshot;if(d.claimed[i])return;if(d.progress[i]>=DailyRules.Target(kind)){status.text=ledger.ClaimDailyTask(kind)?L.V("daily.task_saved"):L.V("error.save");Refresh();return;}GetComponent<PrototypeApp>()?.PlayCurrentLevel(message=>status.text=message);}
  public void Refresh(){var ledger=GameplayProgress.Current;var d=ledger.DailySnapshot;bool valid=observed&&!ledger.DailyClockBlocked&&ledger.DailyTaskWait>TimeSpan.Zero;for(int i=0;i<4;i++){int target=DailyRules.Target((DailyTaskKind)i);fills[i].SetValue(d.progress[i],target,0,true,!valid);actions[i].transform.Find("Label").GetComponent<TMP_Text>().text=d.claimed[i]?L.V("common.claimed"):d.progress[i]>=target?L.V("common.claim"):L.V("common.go");actions[i].interactable=valid&&!d.claimed[i];((PremiumButton)actions[i]).SetRole(d.progress[i]>=target?ButtonRole.Reward:ButtonRole.Secondary);}
   giftClaim.interactable=valid&&ledger.GiftReady;giftState.text=!valid||ledger.GiftClockBlocked?L.V("error.clock"):ledger.GiftReady?L.V("daily.ready"):L.V("economy.gift_wait",(int)Math.Ceiling(ledger.GiftWait.TotalMinutes));RefreshTimer();refreshAt=Time.unscaledTime+15;
  }
  public void Layout(){if(cat==null)return;var imageSize=background.sprite.rect.size;var view=screen.rect.size;background.rectTransform.anchorMin=background.rectTransform.anchorMax=new Vector2(.5f,.5f);background.rectTransform.anchoredPosition=Vector2.zero;background.rectTransform.sizeDelta=imageSize*Mathf.Max(view.x/imageSize.x,view.y/imageSize.y);float factor=safe.GetComponentInParent<Canvas>().scaleFactor;foreach(var b in buttons){var r=(RectTransform)b.transform;float x=Mathf.Max(0,(44/factor-r.rect.width)*.5f),y=Mathf.Max(0,(44/factor-r.rect.height)*.5f);b.GetComponent<UnityEngine.UI.Image>().raycastPadding=new Vector4(-x,-y,-x,-y);}cat.localScale=Vector3.one*Mathf.Min(hero.rect.width*.40f/cat.rect.width,hero.rect.height*.97f/cat.rect.height);}
  void RefreshTimer()
  {
   var ledger=GameplayProgress.Current;var wait=ledger.DailyTaskWait;
   string time=string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0:00}:{1:00}:{2:00}",(int)wait.TotalHours,wait.Minutes,wait.Seconds);
   bool valid=observed&&!ledger.DailyClockBlocked;
   homeTimer.text=valid?time:L.V("error.clock");
   timer.text=valid?L.V("daily.reset",time):L.V("error.clock");
   timerAt=Time.unscaledTime+1;
  }
  void Update()
  {
   var ledger=GameplayProgress.Current;
   if(Time.unscaledTime>=timerAt)
   {
    if(ledger.DailyTaskWait==TimeSpan.Zero||ledger.Revision!=observedCycleRevision)
    {observed=ledger.ObserveDaily();observedCycleRevision=ledger.Revision;if(IsOpen)Refresh();}
    RefreshTimer();
   }
   if(!IsOpen)return;Layout();
   if(Time.unscaledTime>=refreshAt){observed=ledger.ObserveDaily();observedCycleRevision=ledger.Revision;Refresh();}
  }
  void OnApplicationPause(bool paused){if(!paused){timerAt=0;refreshAt=0;}}

 }
}
