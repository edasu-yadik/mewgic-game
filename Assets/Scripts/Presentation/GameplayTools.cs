using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;
using System;using Mewgic.Core;using UnityEngine;using UnityEngine.UI;
namespace Mewgic
{
    public sealed class BoosterBadge : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper v)
        {v.Clear();var r=rectTransform.rect;var center=r.center;float radius=Mathf.Min(r.width,r.height)*.48f;for(int n=0;n<32;n++){float a=n*Mathf.PI/16,b=(n+1)*Mathf.PI/16;int k=v.currentVertCount;v.AddVert(center,new Color32(255,115,136,255),Vector2.zero);v.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Color32(170,13,58,255),Vector2.zero);v.AddVert(center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,new Color32(170,13,58,255),Vector2.zero);v.AddTriangle(k,k+1,k+2);}}
    }
    public sealed class GameplayTools : MonoBehaviour
    {
        Button[] buttons; TMP_Text[] counts; Image[] icons;
        BoosterKind[] kinds;
        public static RectTransform Rect(Transform p,string n,Vector2 min,Vector2 max){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        public static Image Art(Transform p,string n,Sprite sprite,Vector2 min,Vector2 max,bool sliced=false){var im=Rect(p,n,min,max).gameObject.AddComponent<Image>();im.sprite=sprite;im.type=sliced?Image.Type.Sliced:Image.Type.Simple;im.raycastTarget=false;return im;}
        public static TMP_Text Label(Transform p,string n,string text,int size,Vector2 min,Vector2 max){var t=Rect(p,n,min,max).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();t.font=MewgicTypographySettings.Current.body;t.text=text;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=14;t.fontSizeMax=size;t.color=Color.white;t.alignment=TextAlignmentOptions.Midline;t.raycastTarget=false;MewgicTypographySettings.Prepare(t);return t;}
        public Button Mount(RectTransform safe,Action<BoosterKind> use,int level)
        {
            var root=Rect(safe,"GameplayBoosters",new Vector2(.045f,.075f),new Vector2(.955f,.195f));
            var available = new System.Collections.Generic.List<BoosterKind> { BoosterKind.Undo, BoosterKind.Hint, BoosterKind.ExtraBottle, BoosterKind.Shuffle };
            foreach (var advanced in new[] { BoosterKind.ColorSwap, BoosterKind.BottleHammer })
                if (level >= EconomyConfig.Current.UnlockLevel(ShopCatalog.Product(advanced))) available.Add(advanced);
            kinds = available.ToArray();
            int count = kinds.Length;
            buttons=new Button[count]; counts=new TMP_Text[count]; icons=new Image[count];
            var labels=new[]{L.V("booster.undo"),L.V("booster.hint"),L.V("booster.extra"),L.V("booster.shuffle"),L.V("booster.color_swap"),L.V("booster.bottle_hammer")};
            var iconNames=new[]{"Undo","Hint","Extra","Shuffle","ColorSwap","BottleHammer"};
            var tile=Resources.Load<Sprite>("Gameplay/Tools/Tool_Tile_v001");
            for(int i=0;i<buttons.Length;i++)
            {
                var kind=kinds[i];int index=(int)kind;var im=Art(root,kind.ToString(),tile,new Vector2((float)i/count+.007f,0),new Vector2((float)(i+1)/count-.007f,1));im.preserveAspect=true;im.raycastTarget=true;var b=im.gameObject.AddComponent<PremiumButton>();b.targetGraphic=im;b.onClick.AddListener(()=>use(kind));buttons[i]=b;
                var icon=Art(im.transform,"Icon",Resources.Load<Sprite>("Gameplay/Tools/Tool_"+iconNames[index]+"_v001"),new Vector2(.16f,.28f),new Vector2(.84f,.89f));icon.preserveAspect=true;icons[i]=icon;
                var label=Label(im.transform,"Label",labels[index],count==6?23:26,
                    new Vector2(.025f,.08f),new Vector2(.975f,.30f));
                if (count==6) label.fontSizeMin=11;
                var badge=Rect(im.transform,"CounterBadge",new Vector2(.72f,.77f),new Vector2(.99f,1.02f));badge.gameObject.AddComponent<CanvasRenderer>();var graphic=badge.gameObject.AddComponent<BoosterBadge>();graphic.raycastTarget=false;counts[i]=Label(badge,"Charges","",28,Vector2.zero,Vector2.one);
            }
            return buttons[0];
        }
        public RectTransform Target(BoosterKind kind)=>(RectTransform)buttons[Array.IndexOf(kinds,kind)].transform;
        public void Present(Puzzle p,ProgressLedger ledger,bool locked)
        {
            for(int i=0;i<buttons.Length;i++){var kind=kinds[i];int stock=ledger.Remaining(kind);counts[i].text=stock.ToString();
                buttons[i].interactable=!locked&&!p.Won&&(kind!=BoosterKind.Undo||p.CanUndo&&stock>0);
                icons[i].color=buttons[i].interactable&&stock>0?Color.white:new Color(.55f,.55f,.6f,.65f);}
        }
    }
}
