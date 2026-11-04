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
        Button[] buttons=new Button[4];TMP_Text[] counts=new TMP_Text[4];Image[] icons=new Image[4];
        public static RectTransform Rect(Transform p,string n,Vector2 min,Vector2 max){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        public static Image Art(Transform p,string n,Sprite sprite,Vector2 min,Vector2 max,bool sliced=false){var im=Rect(p,n,min,max).gameObject.AddComponent<Image>();im.sprite=sprite;im.type=sliced?Image.Type.Sliced:Image.Type.Simple;im.raycastTarget=false;return im;}
        public static TMP_Text Label(Transform p,string n,string text,int size,Vector2 min,Vector2 max){var t=Rect(p,n,min,max).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();t.font=MewgicTypographySettings.Current.body;t.text=text;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=14;t.fontSizeMax=size;t.color=Color.white;t.alignment=TextAlignmentOptions.Midline;t.raycastTarget=false;MewgicTypographySettings.Prepare(t);return t;}
        public Button Mount(RectTransform safe,Action<BoosterKind> use)
        {
            var root=Rect(safe,"GameplayBoosters",new Vector2(.045f,.075f),new Vector2(.955f,.195f));
            var names=new[]{"Undo","Hint","ExtraBottle","Shuffle"};var labels=new[]{L.V("booster.undo"),L.V("booster.hint"),L.V("booster.extra"),L.V("booster.shuffle")};var iconNames=new[]{"Undo","Hint","Extra","Shuffle"};
            var tile=Resources.Load<Sprite>("Gameplay/Tools/Tool_Tile_v001");
            for(int i=0;i<4;i++)
            {
                int index=i;var im=Art(root,names[i],tile,new Vector2(i*.25f+.007f,0),new Vector2((i+1)*.25f-.007f,1));im.preserveAspect=true;im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>use((BoosterKind)index));buttons[i]=b;
                var icon=Art(im.transform,"Icon",Resources.Load<Sprite>("Gameplay/Tools/Tool_"+iconNames[i]+"_v001"),new Vector2(.16f,.28f),new Vector2(.84f,.89f));icon.preserveAspect=true;icons[i]=icon;
                Label(im.transform,"Label",labels[i],26,new Vector2(.04f,.12f),new Vector2(.96f,.29f));
                var badge=Rect(im.transform,"CounterBadge",new Vector2(.72f,.77f),new Vector2(.99f,1.02f));badge.gameObject.AddComponent<CanvasRenderer>();var graphic=badge.gameObject.AddComponent<BoosterBadge>();graphic.raycastTarget=false;counts[i]=Label(badge,"Charges","",28,Vector2.zero,Vector2.one);
            }
            return buttons[0];
        }
        public RectTransform Target(BoosterKind kind)=>(RectTransform)buttons[(int)kind].transform;
        public void Present(Puzzle p,ProgressLedger ledger,bool locked,int baseCount)
        {
            for(int i=0;i<4;i++){var kind=(BoosterKind)i;counts[i].text=ledger.Remaining(kind).ToString();buttons[i].interactable=!locked&&!p.Won&&ledger.Remaining(kind)>0&&(kind!=BoosterKind.Undo||p.CanUndo)&&(kind!=BoosterKind.ExtraBottle||p.Count<baseCount+1);icons[i].color=buttons[i].interactable?Color.white:new Color(.55f,.55f,.6f,.65f);}
        }
    }
}
