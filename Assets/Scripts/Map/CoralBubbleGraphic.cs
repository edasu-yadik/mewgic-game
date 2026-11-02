using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class CoralBubbleGraphic : MaskableGraphic
 {
  protected override void OnPopulateMesh(VertexHelper vh)
  {
   vh.Clear();const int count=20;float radius=rectTransform.rect.width*.5f;Vector2 center=rectTransform.rect.center;
   for(int ring=0;ring<4;ring++)for(int i=0;i<count;i++){
    float a=i*Mathf.PI*2/count;Color c=color;c.a*=(ring==0?.03f:ring==1?.08f:ring==2?.8f:0)*(.65f+.35f*Mathf.Max(0,Mathf.Sin(a)));
    vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*(ring==0?0:ring==1?.64f:ring==2?.82f:1),c,Vector2.zero);
   }
   for(int ring=0;ring<3;ring++)for(int i=0;i<count;i++){
    int a=ring*count+i,b=ring*count+(i+1)%count,c=(ring+1)*count+i,d=(ring+1)*count+(i+1)%count;
    vh.AddTriangle(a,c,b);vh.AddTriangle(b,c,d);
   }
  }
 }
}
