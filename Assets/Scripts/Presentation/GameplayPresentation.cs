using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    [DefaultExecutionOrder(120)] public sealed class GameplayPresentation : MonoBehaviour
    {
        public Sprite backdrop,shelf,foreground;
        public GameObject wizardCatPrefab;
        public Shader waterfallShader;
        public Texture2D waterfallMask;
        [Min(0)] public float waterfallSpeed=.012f;
        [Range(0,.1f)] public float waterfallBrightness=.028f;
        [Range(0,1)] public float waterfallOpacity=.65f;
        public bool IsAnimating {get;set;}
        RectTransform safe,board,background,world,cat,perch,edgeDecor;
        RectTransform[] bottles=new RectTransform[0];
        readonly System.Collections.Generic.List<RectTransform> shelves=new System.Collections.Generic.List<RectTransform>();
        Vector2 lastSize;
        public static int Columns(int count)=>Columns(count,new Vector2(630,960));
        public static int Columns(int count,Vector2 size)
        {
            int best=1;float largest=-1;for(int cols=1;cols<=Mathf.Max(1,count);cols++){int rows=Mathf.CeilToInt(count/(float)cols);float h=Mathf.Min(size.y/Mathf.Max(1,rows)-66,size.x/cols*.86f/.42f);if(h>largest+.01f){largest=h;best=cols;}}return best;
        }
        public static Rect Slot(int index,int count,Vector2 size)
        {
            int cols=Columns(count,size),rows=Mathf.Max(1,Mathf.CeilToInt(count/(float)cols));
            float cw=size.x/cols,ch=size.y/rows;
            float height=Mathf.Max(1,Mathf.Min(ch-66,cw*.86f/.42f)),width=height*.42f;
            int row=index/cols,col=index%cols;int inRow=Mathf.Min(cols,count-row*cols);
            float cx=(col+.5f)*cw+(cols-inRow)*cw*.5f;
            float y=size.y-(row+1)*ch+44;
            return new Rect((cx-width*.5f)/size.x,y/size.y,width/size.x,height/size.y);
        }
        static RectTransform Rect(Transform p,string name){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static Image Art(Transform p,string name,Sprite sprite){var image=Rect(p,name).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
        public void Mount(RectTransform safeArea,RectTransform bottleBoard)
        {
            safe=safeArea;board=bottleBoard;board.GetComponent<Image>().color=Color.clear;
            board.anchorMin=new Vector2(.21f,.30f);board.anchorMax=new Vector2(.97f,.90f);board.offsetMin=board.offsetMax=Vector2.zero;
            var night=safe.parent.Find("Night");if(night!=null)night.gameObject.SetActive(false);
            background=Rect(safe.parent,"GameplayBackdrop");background.SetAsFirstSibling();var bg=Art(background,"CastleWaterfalls",backdrop);bg.color=new Color(.85f,.88f,1,1);
            var water=WaterfallAnimator.Attach(bg,waterfallShader,waterfallMask,true);
            if(water!=null){water.speed=waterfallSpeed;water.brightness=waterfallBrightness;water.opacity=waterfallOpacity;}
            world=Rect(safe,"GameplayWorld");world.SetAsFirstSibling();Rect(world,"StoneShelves");
            edgeDecor=Art(background,"ForegroundEdges",foreground).rectTransform;
            perch=Art(world,"CatStonePerch",shelf).rectTransform;
            cat=Instantiate(wizardCatPrefab,world,false).GetComponent<RectTransform>();cat.name="GameplayWizardCat";
            foreach(var graphic in cat.GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=false;
            Layout();
        }
        public void SetTopLimit(float top){if(board==null||Mathf.Approximately(board.anchorMax.y,top))return;board.anchorMax=new Vector2(board.anchorMax.x,top);Layout();}
        public void Bind(RectTransform[] views){bottles=views;int rows=Mathf.CeilToInt(views.Length/(float)Columns(views.Length,board.rect.size));while(shelves.Count<rows)shelves.Add(Art(world.Find("StoneShelves"),"StoneShelf_"+shelves.Count,shelf).rectTransform);for(int i=0;i<shelves.Count;i++)shelves[i].gameObject.SetActive(i<rows);Layout();}
        public void Layout()
        {
            if(safe==null)return;
            var viewport=((RectTransform)safe.parent).rect.size;var bg=background.GetChild(0).GetComponent<RectTransform>();var sr=backdrop.rect.size;float cover=Mathf.Max(viewport.x/sr.x,viewport.y/sr.y);bg.anchorMin=bg.anchorMax=new Vector2(.5f,.5f);bg.sizeDelta=sr*cover;bg.anchoredPosition=Vector2.zero;
            var edgeSize=foreground.rect.size;float edgeCover=Mathf.Max(viewport.x/edgeSize.x,viewport.y/edgeSize.y);edgeDecor.anchorMin=edgeDecor.anchorMax=new Vector2(.5f,.5f);edgeDecor.sizeDelta=edgeSize*edgeCover;edgeDecor.anchoredPosition=Vector2.zero;
            float unit=Mathf.Min(safe.rect.width/900f,safe.rect.height/1600f);
            cat.anchorMin=cat.anchorMax=new Vector2(.015f,.29f);cat.pivot=new Vector2(.5f,0);cat.localScale=Vector3.one*Mathf.Min(safe.rect.width*.18f/cat.rect.width,300*unit/cat.rect.height);cat.anchoredPosition=new Vector2(cat.rect.width*cat.localScale.x*.5f,0);
            perch.anchorMin=perch.anchorMax=new Vector2(.105f,.29f);perch.pivot=new Vector2(.5f,.92f);perch.anchoredPosition=Vector2.zero;perch.sizeDelta=new Vector2(safe.rect.width*.20f,65*unit);
            if(IsAnimating)return;
            for(int i=0;i<bottles.Length;i++){if(bottles[i]==null)continue;var slot=Slot(i,bottles.Length,board.rect.size);var b=bottles[i];b.anchorMin=slot.min;b.anchorMax=slot.max;b.offsetMin=b.offsetMax=Vector2.zero;}
            int cols=Columns(bottles.Length,board.rect.size),rows=Mathf.Max(1,Mathf.CeilToInt(bottles.Length/(float)cols));
            while(shelves.Count<rows)shelves.Add(Art(world.Find("StoneShelves"),"StoneShelf_"+shelves.Count,shelf).rectTransform);for(int i=0;i<shelves.Count;i++)shelves[i].gameObject.SetActive(i<rows&&bottles.Length>0);
            for(int row=0;row<Mathf.CeilToInt(bottles.Length/(float)cols);row++)
            {
                var slot=Slot(row*cols,bottles.Length,board.rect.size);var point=board.TransformPoint(new Vector3(board.rect.center.x,board.rect.yMin+slot.yMin*board.rect.height));var local=(Vector2)safe.InverseTransformPoint(point);
                var r=shelves[row];r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.92f);r.anchoredPosition=local;r.sizeDelta=new Vector2(board.rect.width+40*unit,Mathf.Min(100*unit,board.rect.height/rows*.30f));
            }
            lastSize=board.rect.size;
        }
        void LateUpdate(){if(board!=null&&!IsAnimating&&(board.rect.size!=lastSize))Layout();}
    }
}
