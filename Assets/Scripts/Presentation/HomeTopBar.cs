using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;using Mewgic.Core;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    public sealed class HomeTopBar : MonoBehaviour
    {
        public Sprite panel,coin,gem,heart,plus,settings;
        public HomeResourceModel Model {get;private set;}
        RectTransform root,safe,popup,shop; TMP_Text shopBalance,shopNotice; int lastPurchaseFrame=-1;
        double displayedCoins=-1;
        TMP_Text coinsText,gemsText,energyText,heartText,message;
        Button soundButton;TMP_Text soundLabel;GameObject languagePicker;Button languageButton,tutorialReplayButton;
        static CultureInfo Turkish=>L.Culture;
        public void Mount(RectTransform safeArea)
        {
            if(root!=null)return;safe=safeArea;Model=new HomeResourceModel(GameplayProgress.Current);
            root=Rect(safe,"HomeTopBar",new Vector2(.015f,1),new Vector2(.985f,1));root.pivot=new Vector2(.5f,1);
            var coins=Meter("CoinsDisplay",0,coin,"CoinsPlus",L.V("home.coins_info"),out coinsText);
            Meter("GemsDisplay",1,gem,"GemsPlus",L.V("home.gems_info"),out gemsText);
            var energy=Meter("EnergyDisplay",2,heart,"EnergyPlus",L.V("home.energy_info"),out energyText);
            heartText=Label(energy,"HeartCount","",new Vector2(.025f,.15f),new Vector2(.28f,.85f),28);
            var gear=Art(root,"SettingsButton",settings,new Vector2(.91f,0),new Vector2(1,1));var b=gear.gameObject.AddComponent<Button>();b.targetGraphic=gear;b.onClick.AddListener(OpenSettings);gear.raycastTarget=true;gear.raycastPadding=new Vector4(-4,-4,-4,-4);
            Model.Changed+=Refresh;L.Changed+=Refresh;Refresh();BuildPopup();Layout();
        }
        RectTransform Meter(string name,int i,Sprite icon,string buttonName,string info,out TMP_Text text)
        {
            float start=i*.302f;var r=Rect(root,name,new Vector2(start,0),new Vector2(start+.29f,1));Art(r,"Panel",panel,Vector2.zero,Vector2.one);
            Art(r,"Icon",icon,new Vector2(-.015f,-.05f),new Vector2(.30f,1.05f));
            text=Label(r,"Value","",new Vector2(.30f,.16f),new Vector2(.775f,.84f),31);
            var a=Art(r,buttonName,plus,new Vector2(.77f,.1f),new Vector2(.99f,.9f));a.raycastTarget=true;a.raycastPadding=new Vector4(-8,-8,-8,-8);var b=a.gameObject.AddComponent<Button>();b.targetGraphic=a;b.onClick.AddListener(()=>{if(i==0)OpenShop();else Show(info);});return r;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        static Image Art(Transform parent,string name,Sprite s,Vector2 min,Vector2 max)
        {
            var i=Rect(parent,name,min,max).gameObject.AddComponent<Image>();i.sprite=s;i.preserveAspect=true;i.raycastTarget=false;return i;
        }
        static TMP_Text Label(Transform p,string name,string text,Vector2 min,Vector2 max,int size)
        {
            var t=Rect(p,name,min,max).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();t.font=MewgicTypographySettings.Current.body;t.fontStyle=FontStyles.Bold;t.text=text;t.fontSize=size;t.color=Color.white;t.alignment=TextAlignmentOptions.Midline;t.raycastTarget=false;t.enableAutoSizing=true;t.fontSizeMin=12;t.fontSizeMax=size;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Truncate;MewgicTypographySettings.Prepare(t);return t;
        }
        public static string FormatAmount(long n)
        {
            if(n>=1000000000000000L)return L.V("number.quadrillion",n/1000000000000000m);
            if(n>=1000000000000L)return L.V("number.trillion",n/1000000000000m);
            if(n>=1000000000L)return L.V("number.billion",n/1000000000m);
            if(n>=1000000L)return L.V("number.million",n/1000000m);
            return n.ToString("N0",Turkish);
        }
        void Refresh(){if(displayedCoins<0)displayedCoins=Model.Coins;coinsText.text=FormatAmount(displayedCoins>=long.MaxValue?long.MaxValue:System.Math.Max(0,(long)displayedCoins));gemsText.text=FormatAmount(Model.Gems);heartText.text=Model.Energy.ToString();energyText.text=Model.Energy==HomeResourceModel.MaximumEnergy?L.V("home.full"):Model.Energy+"/5";}
        void BuildPopup()
        {
            popup=Rect(safe,"ResourceInfoPanel",Vector2.zero,Vector2.one);var blocker=popup.gameObject.AddComponent<Image>();blocker.color=new Color(0,0,0,.7f);blocker.raycastTarget=true;
            var card=Art(popup,"Card",panel,new Vector2(.08f,.43f),new Vector2(.92f,.68f));card.preserveAspect=false;
            message=Label(card.transform,"Message","",new Vector2(.08f,.30f),new Vector2(.92f,.90f),26);
            var close=Art(card.transform,"ResourceInfoClose",panel,new Vector2(.32f,.04f),new Vector2(.68f,.28f));close.preserveAspect=false;close.raycastTarget=true;var button=close.gameObject.AddComponent<Button>();button.targetGraphic=close;button.onClick.AddListener(Close);Label(close.transform,"Label",L.V("common.close"),Vector2.zero,Vector2.one,22);popup.gameObject.SetActive(false);
        }
        void OpenShop()
        {
            if(shop==null)
            {
                shop=Rect(safe,"BoosterShop",Vector2.zero,Vector2.one);
                var dim=shop.gameObject.AddComponent<Image>();dim.color=new Color(0,0,.08f,.82f);
                var card=Art(shop,"BoosterShopCard",Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001"),new Vector2(.075f,.24f),new Vector2(.925f,.77f));
                card.preserveAspect=false;card.type=Image.Type.Sliced;
                Label(card.transform,"Title",L.V("economy.shop"),new Vector2(.07f,.87f),new Vector2(.93f,.98f),34);
                shopBalance=Label(card.transform,"Balance","",new Vector2(.07f,.77f),new Vector2(.93f,.87f),26);
                var kinds=new[]{BoosterKind.Hint,BoosterKind.Undo,BoosterKind.ExtraBottle,BoosterKind.Shuffle};
                var keys=new[]{"booster.hint","booster.undo","booster.extra","booster.shuffle"};
                for(int i=0;i<kinds.Length;i++)
                {
                    var kind=kinds[i];float y=.64f-i*.12f;
                    SettingButton(card.transform,"Buy"+kind,L.V("economy.buy",L.V(keys[i]),ProgressLedger.BoosterPrice(kind)),
                        new Vector2(.11f,y),new Vector2(.89f,y+.10f),()=>Purchase(kind));
                }
                shopNotice=Label(card.transform,"PurchaseNotice","",new Vector2(.07f,.125f),new Vector2(.93f,.26f),22);
                SettingButton(card.transform,"ShopClose",L.V("common.close"),new Vector2(.28f,.03f),new Vector2(.72f,.12f),()=>shop.gameObject.SetActive(false));
            }
            shopBalance.text=L.V("economy.balance",FormatAmount(GameplayProgress.Current.Presentation.coins));
            shopNotice.text="";
            shop.gameObject.SetActive(true);
            shop.SetAsLastSibling();
        }
        void Purchase(BoosterKind kind)
        {
            if(lastPurchaseFrame==Time.frameCount)return;
            lastPurchaseFrame=Time.frameCount;
            var ledger=GameplayProgress.Current;
            if(ledger.Presentation.coins<ProgressLedger.BoosterPrice(kind)){shopNotice.text=L.V("economy.insufficient");return;}
            shopNotice.text=ledger.BuyBooster(kind)?L.V("economy.bought",ledger.Remaining(kind)):L.V("error.save");
            shopBalance.text=L.V("economy.balance",FormatAmount(ledger.Presentation.coins));
        }

        void OpenSettings()
        {
            Show(L.V("settings.title"));
            if(soundButton==null){var r=Art(popup.Find("Card"),"SoundToggle",panel,new Vector2(.16f,.625f),new Vector2(.84f,.765f));r.preserveAspect=false;r.raycastTarget=true;soundButton=r.gameObject.AddComponent<Button>();soundButton.targetGraphic=r;soundLabel=Label(r.transform,"Label","",Vector2.zero,Vector2.one,25);soundButton.onClick.AddListener(()=>{var ledger=GameplayProgress.Current;if(ledger.SetSoundEnabled(!ledger.Presentation.soundEnabled)){AudioListener.volume=ledger.Presentation.soundEnabled?1:0;RefreshSound();}else{languagePicker.SetActive(false);message.text=L.V("error.save");}});}
            if(tutorialReplayButton==null)tutorialReplayButton=SettingButton(popup.Find("Card"),"TutorialReplay",
                L.V("settings.tutorial_replay"),new Vector2(.16f,.265f),new Vector2(.84f,.415f),PrototypeApp.RequestTutorialReplay);
            tutorialReplayButton.gameObject.SetActive(true);
            soundButton.gameObject.SetActive(true);if(languageButton==null)BuildLanguage();languageButton.gameObject.SetActive(true);var settingsCard=(RectTransform)popup.Find("Card");var settingsFrame=settingsCard.GetComponent<Image>();settingsFrame.sprite=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");settingsFrame.type=Image.Type.Sliced;settingsCard.anchorMin=new Vector2(.08f,.22f);settingsCard.anchorMax=new Vector2(.92f,.76f);message.rectTransform.anchorMin=new Vector2(.08f,.80f);message.rectTransform.anchorMax=new Vector2(.92f,.94f);var close=(RectTransform)settingsCard.Find("ResourceInfoClose");close.anchorMin=new Vector2(.30f,.06f);close.anchorMax=new Vector2(.70f,.21f);RefreshSound();
        }
        void BuildLanguage()
        {
            languageButton=SettingButton(popup.Find("Card"),"LanguageButton",L.V("settings.language"),new Vector2(.16f,.445f),new Vector2(.84f,.585f),()=>{languagePicker.SetActive(true);languagePicker.transform.SetAsLastSibling();});
            languagePicker=Rect(popup,"LanguagePicker",Vector2.zero,Vector2.one).gameObject;var dim=languagePicker.AddComponent<Image>();dim.color=new Color(0,0,0,.75f);dim.raycastTarget=true;
            var card=Art(languagePicker.transform,"LanguageCard",Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001"),new Vector2(.06f,.20f),new Vector2(.94f,.80f));card.preserveAspect=false;card.type=Image.Type.Sliced;card.raycastTarget=true;
            Label(card.transform,"Title",L.V("settings.language"),new Vector2(.08f,.85f),new Vector2(.92f,.96f),32);
            for(int i=0;i<L.Codes.Length;i++){int index=i;float x=i%2==0?.05f:.52f,y=.65f-(i/2)*.16f;SettingButton(card.transform,"Locale_"+L.Codes[i],L.Names[i],new Vector2(x,y),new Vector2(x+.43f,y+.13f),()=>{if(L.Select(L.Codes[index]))languagePicker.SetActive(false);else{languagePicker.SetActive(false);message.text=L.V("error.save");}});}
            SettingButton(card.transform,"LanguageClose",L.V("common.close"),new Vector2(.23f,.035f),new Vector2(.77f,.15f),()=>languagePicker.SetActive(false));languagePicker.SetActive(false);
        }
        Button SettingButton(Transform p,string name,string value,Vector2 min,Vector2 max,System.Action action){var image=Art(p,name,panel,min,max);image.preserveAspect=false;image.raycastTarget=true;var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(()=>action());Label(image.transform,"Label",value,new Vector2(.04f,.12f),new Vector2(.96f,.88f),25);return b;}
        void RefreshSound(){soundLabel.text=L.V(GameplayProgress.Current.Presentation.soundEnabled?"settings.sound_on":"settings.sound_off");}
        public void Show(string text){var card=(RectTransform)popup.Find("Card");var frame=card.GetComponent<Image>();frame.sprite=panel;frame.type=Image.Type.Simple;card.anchorMin=new Vector2(.08f,.43f);card.anchorMax=new Vector2(.92f,.68f);message.rectTransform.anchorMin=new Vector2(.08f,.30f);message.rectTransform.anchorMax=new Vector2(.92f,.90f);var close=(RectTransform)card.Find("ResourceInfoClose");close.anchorMin=new Vector2(.32f,.04f);close.anchorMax=new Vector2(.68f,.28f);if(languagePicker!=null)languagePicker.SetActive(false);if(languageButton!=null)languageButton.gameObject.SetActive(false);if(soundButton!=null)soundButton.gameObject.SetActive(false);if(tutorialReplayButton!=null)tutorialReplayButton.gameObject.SetActive(false);message.text=text;popup.gameObject.SetActive(true);popup.SetAsLastSibling();}
        public void Close(){popup.gameObject.SetActive(false);}
        public void Layout(){if(root==null)return;float height=Mathf.Min(safe.rect.width*.082f,safe.rect.height*.06f);root.sizeDelta=new Vector2(0,height);root.anchoredPosition=new Vector2(0,-safe.rect.width*.018f);}
        void LateUpdate(){Layout();Model?.RefreshFromLedger();if(Model!=null&&displayedCoins!=Model.Coins){displayedCoins=System.Math.Abs(displayedCoins-Model.Coins)<1?Model.Coins:displayedCoins+(Model.Coins-displayedCoins)*(1-System.Math.Exp(-Time.unscaledDeltaTime*6));coinsText.text=FormatAmount(displayedCoins>=long.MaxValue?long.MaxValue:System.Math.Max(0,(long)displayedCoins));}}
        void OnDestroy(){L.Changed-=Refresh;if(Model!=null)Model.Changed-=Refresh;}
    }
}
