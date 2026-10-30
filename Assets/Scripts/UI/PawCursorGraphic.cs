using UnityEngine;using UnityEngine.UI;
namespace Mewgic.UI
{
    // Hafif UI geometrisi: sprite/partikül üretmeden pati, tıklama halkası ve düşünce balonu.
    public abstract class PawSoftGraphic:MaskableGraphic
    {
        protected void Oval(VertexHelper v,Vector2 p,Vector2 size,Color tint,int sides=36){int k=v.currentVertCount;v.AddVert(p,tint,Vector2.zero);for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;v.AddVert(p+new Vector2(Mathf.Cos(a)*size.x,Mathf.Sin(a)*size.y),tint,Vector2.zero);if(i>0)v.AddTriangle(k,k+i,k+i+1);}}
        protected void Ring(VertexHelper v,Vector2 p,float radius,float width,Color tint){int k=v.currentVertCount;for(int i=0;i<=48;i++){float a=i*Mathf.PI/24;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));v.AddVert(p+d*radius,tint,Vector2.zero);v.AddVert(p+d*(radius-width),tint,Vector2.zero);if(i>0){int n=k+i*2;v.AddTriangle(n-2,n,n-1);v.AddTriangle(n-1,n,n+1);}}}
    }
    [RequireComponent(typeof(CanvasRenderer))]public sealed class PawCursorGraphic:PawSoftGraphic
    {
        public float Press;
        void Pad(VertexHelper v,Vector2 p,Vector2 size){Oval(v,p+Vector2.down*3,size+Vector2.one*4,new Color(.15f,.04f,.3f,.35f));Oval(v,p,size+Vector2.one*2,new Color(1,.76f,.23f));Oval(v,p,size,new Color(1,.94f,.76f));Oval(v,p+new Vector2(0,-1),size*.58f,new Color(.58f,.22f,.65f));Oval(v,p+new Vector2(-size.x*.15f,size.y*.22f),size*.24f,new Color(1,.83f,.89f,.72f));}
        protected override void OnPopulateMesh(VertexHelper v){v.Clear();float glow=.10f+.025f*Mathf.Sin(Time.unscaledTime*3);Oval(v,Vector2.zero,new Vector2(43,46),new Color(1,.75f,.25f,glow));if(Press>0){float t=Press;Ring(v,new Vector2(0,-7),Mathf.Lerp(12,46,t),2,new Color(1,.83f,.35f,Mathf.Sin(t*Mathf.PI)*.85f));}Pad(v,new Vector2(0,-12),new Vector2(19,18));Pad(v,new Vector2(-23,9),new Vector2(9,12));Pad(v,new Vector2(-9,23),new Vector2(10,13));Pad(v,new Vector2(12,23),new Vector2(10,13));Pad(v,new Vector2(26,8),new Vector2(9,12));}
        void Update(){SetVerticesDirty();}
    }
    [RequireComponent(typeof(CanvasRenderer))]public sealed class PawThoughtGraphic:PawSoftGraphic
    {
        protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;var size=r.size*.5f;Oval(v,r.center+Vector2.down*3,size,new Color(.12f,.03f,.24f,.22f),64);Oval(v,r.center,size,new Color(.4f,.16f,.55f),64);Oval(v,r.center,size-Vector2.one*2,new Color(1,.78f,.29f),64);Oval(v,r.center,size-Vector2.one*4,new Color(1,.97f,.87f),64);float x=r.xMin+r.width*.17f;Oval(v,new Vector2(x,r.yMax+9),new Vector2(11,8),new Color(.58f,.29f,.59f));Oval(v,new Vector2(x,r.yMax+10),new Vector2(9,6),new Color(1,.96f,.85f));Oval(v,new Vector2(x-14,r.yMax+26),new Vector2(5,4),new Color(1,.88f,.61f));}
    }
}
