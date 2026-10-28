using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    // Sabit sayıda küçük yıldız: ParticleSystem, ışık veya yeni materyal üretmez.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BootMagicGraphic : MaskableGraphic
    {
        public bool glow;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var r=rectTransform.rect; float t=Time.unscaledTime;
            if(glow)
            {
                for(int ring=5;ring>=1;ring--)
                {
                    float radius=r.width*.1f*ring; var c=new Color(1,.78f,.24f,.045f*(1+.25f*Mathf.Sin(t*2)));
                    int center=vh.currentVertCount;vh.AddVert(r.center,c,Vector2.zero);
                    for(int j=0;j<=24;j++){float a=j*Mathf.PI*2/24;vh.AddVert(r.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);if(j>0)vh.AddTriangle(center,center+j,center+j+1);}
                }
                Star(vh,r.center,Mathf.Min(r.width,r.height)*.14f,new Color(1,.94f,.6f,.7f));
            }
            else for(int i=0;i<10;i++)
            {
                float x=Mathf.Repeat(i*.618f,1),y=Mathf.Repeat(i*.317f+t*.012f,1);
                float alpha=(.3f+.25f*Mathf.Sin(t*1.7f+i))*(1-Mathf.Abs(y-.5f)*1.1f);
                Star(vh,new Vector2(r.x+x*r.width,r.y+y*r.height),i%3==0?5:2.6f,new Color(1,.84f,.48f,alpha));
            }
        }
        static void Star(VertexHelper v,Vector2 p,float radius,Color c)
        {
            int start=v.currentVertCount;v.AddVert(p,c,Vector2.zero);
            for(int i=0;i<=8;i++){float a=i*Mathf.PI/4;float s=i%2==0?radius:radius*.22f;v.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s,c,Vector2.zero);if(i>0)v.AddTriangle(start,start+i,start+i+1);}
        }
        void Update(){SetVerticesDirty();}
    }
}
