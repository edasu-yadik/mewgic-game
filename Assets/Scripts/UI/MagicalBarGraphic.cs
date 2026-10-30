using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.UI
{
    // Sabit sayıda vertex; texture, shader veya sürekli parçacık üretimi yok.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MagicalBarGraphic:MaskableGraphic
    {
        public MagicalProgressBar owner;
        void Quad(VertexHelper v,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color lower,Color upper)
        {int k=v.currentVertCount;v.AddVert(a,lower,Vector2.zero);v.AddVert(b,lower,Vector2.zero);v.AddVert(c,upper,Vector2.zero);v.AddVert(d,upper,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
        void Capsule(VertexHelper v,Rect r,Color low,Color high)
        {
            float rad=r.height*.5f;var mid=r.center;int start=v.currentVertCount;v.AddVert(mid,Color.Lerp(low,high,.5f),Vector2.zero);
            for(int i=0;i<=40;i++){float a=i*Mathf.PI*2/40;var p=new Vector2((Mathf.Cos(a)>=0?r.xMax-rad:r.xMin+rad)+Mathf.Cos(a)*rad,mid.y+Mathf.Sin(a)*rad);v.AddVert(p,Color.Lerp(low,high,(p.y-r.yMin)/r.height),Vector2.zero);if(i>0)v.AddTriangle(start,start+i,start+i+1);}
        }
        void Diamond(VertexHelper v,Vector2 p,float x,float y,Color a,Color b)
        {Quad(v,p+Vector2.left*x,p+Vector2.down*y,p+Vector2.right*x,p+Vector2.up*y,a,b);Quad(v,p+Vector2.left*x*.65f,p,p+Vector2.right*x*.65f,p+Vector2.up*y*.8f,b,Color.white);}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();if(owner==null||owner.style==null)return;var s=owner.style;var full=rectTransform.rect;if(full.width<12||full.height<10)return;float h=Mathf.Min(full.height,full.width*.24f);full=new Rect(full.x,full.center.y-h*.5f,full.width,h);var r=new Rect(full.xMin+h*.27f,full.yMin,Mathf.Max(h,full.width-h*.54f),h);
            Capsule(v,r,s.goldShadow,s.gold);r=new Rect(r.x+2,r.y+2,r.width-4,r.height-4);Capsule(v,r,s.gold,new Color(1,.94f,.66f));r=new Rect(r.x+2,r.y+2,r.width-4,r.height-4);Capsule(v,r,s.empty,new Color(.20f,.035f,.34f));
            float fill=owner.DisplayedPercent;if(fill>0){var f=new Rect(r.x,r.y,r.width*fill,r.height);float radius=f.height*.5f;
                // Sıfıra yakın dolumda kapsül uçlarının taşmasını önleyen geometrik genişlik.
                if(f.width>=f.height)Capsule(v,f,s.violet,s.light);else Quad(v,new Vector2(f.xMin,f.yMin),new Vector2(f.xMax,f.yMin),new Vector2(f.xMax,f.yMax),new Vector2(f.xMin,f.yMax),s.violet,s.light);
                for(int i=0;i<3;i++){float phase=Mathf.Repeat(Time.unscaledTime*.10f+i*.31f,1);var p=new Vector2(f.xMin+f.width*phase,f.center.y+Mathf.Sin(i*2.4f)*f.height*.18f);float size=1.4f+Mathf.Sin(phase*Mathf.PI)*1.1f;Diamond(v,p,size,size*.6f,new Color(1,.7f,1,.3f),new Color(1,.85f,1,.45f));}
            }
            float labelWidth=Mathf.Min(r.width*.6f,90);Capsule(v,new Rect(r.center.x-labelWidth*.5f,r.center.y-r.height*.42f,labelWidth,r.height*.84f),new Color(.08f,.015f,.16f,.94f),new Color(.19f,.035f,.3f,.94f));
            float tip=h*.22f;for(int end=0;end<2;end++){float x=end==0?full.xMin+h*.27f:full.xMax-h*.27f;Diamond(v,new Vector2(x,full.center.y),tip+2,h*.46f,s.goldShadow,s.gold);Diamond(v,new Vector2(x,full.center.y),tip,h*.37f,s.violet,s.light);}
            if(owner.Celebration>0)for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;var p=r.center+new Vector2(Mathf.Cos(a)*r.width*.3f,Mathf.Sin(a)*h*.3f);Diamond(v,p,2,3,s.gold,s.gold);}
        }
    }
}
