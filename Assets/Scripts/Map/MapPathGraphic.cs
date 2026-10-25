using UnityEngine;using UnityEngine.UI;
namespace Mewgic.Map
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MapPathGraphic : MaskableGraphic
    {
        public RectTransform[] Points;
        static Vector2 Curve(Vector2 a,Vector2 b,float t)
        {float bend=Mathf.Abs(b.y-a.y)>100?Mathf.Sign(a.x<450?1:-1)*130:0;var c=a+new Vector2((b.x-a.x)*.05f+bend,(b.y-a.y)*.55f);var d=b-new Vector2((b.x-a.x)*.05f-bend,(b.y-a.y)*.55f);float k=1-t;return k*k*k*a+3*k*k*t*c+3*k*t*t*d+t*t*t*b;}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();if(Points==null)return;
            for(int i=0;i<Points.Length-1;i++)
            {
                if(Points[i]==null||Points[i+1]==null)continue;
                Vector2 a=rectTransform.InverseTransformPoint(Points[i].position),b=rectTransform.InverseTransformPoint(Points[i+1].position);
                for(int n=0;n<24;n++)
                {var p=Curve(a,b,n/24f);var q=Curve(a,b,(n+1)/24f);Strip(v,p,q,20,new Color(1,.75f,.2f,.12f));if(n%4!=3)Strip(v,p,q,5,new Color(1,.96f,.68f,.82f));}
            }
        }
        static void Strip(VertexHelper v,Vector2 a,Vector2 b,float width,Color c)
        {var delta=b-a;if(delta.sqrMagnitude<.001f)return;var normal=new Vector2(-delta.y,delta.x).normalized*width*.5f;int i=v.currentVertCount;v.AddVert(a-normal,c,Vector2.zero);v.AddVert(b-normal,c,Vector2.zero);v.AddVert(b+normal,c,Vector2.zero);v.AddVert(a+normal,c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
    }
}
