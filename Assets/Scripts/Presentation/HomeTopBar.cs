using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    public sealed class HomeTopBar : MonoBehaviour
    {
        public Sprite panel,coin,gem,heart,plus,settings;
        public HomeResourceModel Model {get;private set;}
        RectTransform root,safe,popup;
        Text coinsText,gemsText,energyText,heartText,message;
        static readonly CultureInfo Turkish=CultureInfo.GetCultureInfo("tr-TR");
        public void Mount(RectTransform safeArea)
        {
            if(root!=null)return;safe=safeArea;Model=new HomeResourceModel();
            root=Rect(safe,"HomeTopBar",new Vector2(.015f,1),new Vector2(.985f,1));root.pivot=new Vector2(.5f,1);
            var coins=Meter("CoinsDisplay",0,coin,"CoinsPlus","Para mağazası sonraki aşamada açılacak. Bu ekran önizleme verisi kullanıyor.",out coinsText);
            Meter("GemsDisplay",1,gem,"GemsPlus","Elmas mağazası sonraki aşamada açılacak. Satın alma yapılmadı.",out gemsText);
            var energy=Meter("EnergyDisplay",2,heart,"EnergyPlus","Enerji yenileme ve ödüller sonraki aşamada açılacak. Enerji değiştirilmedi.",out energyText);
            heartText=Label(energy,"HeartCount","",new Vector2(.025f,.15f),new Vector2(.28f,.85f),28);
            var gear=Art(root,"SettingsButton",settings,new Vector2(.91f,0),new Vector2(1,1));var b=gear.gameObject.AddComponent<Button>();b.targetGraphic=gear;b.onClick.AddListener(()=>Show("Ayarlar paneli — geçici önizleme. Ayrıntılı ayarlar sonraki aşamada eklenecek."));gear.raycastTarget=true;
            Model.Changed+=Refresh;Refresh();BuildPopup();Layout();
        }
        RectTransform Meter(string name,int i,Sprite icon,string buttonName,string info,out Text text)
        {
            float start=i*.302f;var r=Rect(root,name,new Vector2(start,0),new Vector2(start+.29f,1));Art(r,"Panel",panel,Vector2.zero,Vector2.one);
            Art(r,"Icon",icon,new Vector2(-.015f,-.05f),new Vector2(.30f,1.05f));
            text=Label(r,"Value","",new Vector2(.30f,.16f),new Vector2(.775f,.84f),31);
            var a=Art(r,buttonName,plus,new Vector2(.77f,.1f),new Vector2(.99f,.9f));a.raycastTarget=true;var b=a.gameObject.AddComponent<Button>();b.targetGraphic=a;b.onClick.AddListener(()=>Show(info));return r;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        static Image Art(Transform parent,string name,Sprite s,Vector2 min,Vector2 max)
        {
            var i=Rect(parent,name,min,max).gameObject.AddComponent<Image>();i.sprite=s;i.preserveAspect=true;i.raycastTarget=false;return i;
        }
        static Text Label(Transform p,string name,string text,Vector2 min,Vector2 max,int size)
        {
            var t=Rect(p,name,min,max).gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontStyle=FontStyle.Bold;t.text=text;t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=12;t.resizeTextMaxSize=size;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.8f);shadow.effectDistance=new Vector2(1,-2);return t;
        }
        public static string FormatAmount(long n)
        {
            if(n>=1000000000000000L)return (n/1000000000000000m).ToString("0.#",Turkish)+"Q";
            if(n>=1000000000000L)return (n/1000000000000m).ToString("0.#",Turkish)+"T";
            if(n>=1000000000L)return (n/1000000000m).ToString("0.#",Turkish)+"B";
            if(n>=1000000L)return (n/1000000m).ToString("0.#",Turkish)+"M";
            return n.ToString("N0",Turkish);
        }
        void Refresh(){coinsText.text=FormatAmount(Model.Coins);gemsText.text=FormatAmount(Model.Gems);heartText.text=Model.Energy.ToString();energyText.text=Model.Energy==HomeResourceModel.MaximumEnergy?"Full":Model.Energy+"/5";}
        void BuildPopup()
        {
            popup=Rect(safe,"ResourceInfoPanel",Vector2.zero,Vector2.one);var blocker=popup.gameObject.AddComponent<Image>();blocker.color=new Color(0,0,0,.7f);blocker.raycastTarget=true;
            var card=Art(popup,"Card",panel,new Vector2(.08f,.40f),new Vector2(.92f,.61f));card.preserveAspect=false;
            message=Label(card.transform,"Message","",new Vector2(.08f,.30f),new Vector2(.92f,.90f),26);
            var close=Art(card.transform,"ResourceInfoClose",panel,new Vector2(.32f,.04f),new Vector2(.68f,.28f));close.preserveAspect=false;close.raycastTarget=true;var button=close.gameObject.AddComponent<Button>();button.targetGraphic=close;button.onClick.AddListener(Close);Label(close.transform,"Label","KAPAT",Vector2.zero,Vector2.one,22);popup.gameObject.SetActive(false);
        }
        public void Show(string text){message.text=text;popup.gameObject.SetActive(true);popup.SetAsLastSibling();}
        public void Close(){popup.gameObject.SetActive(false);}
        public void Layout(){if(root==null)return;float height=Mathf.Min(safe.rect.width*.082f,safe.rect.height*.06f);root.sizeDelta=new Vector2(0,height);root.anchoredPosition=new Vector2(0,-safe.rect.width*.018f);}
        void LateUpdate(){Layout();}
        void OnDestroy(){if(Model!=null)Model.Changed-=Refresh;}
    }
}

