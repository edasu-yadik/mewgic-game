using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    /// <summary>Text-free hint: gold paw at the source, cyan arrow at the destination.</summary>
    public sealed class HintCueGraphic : MaskableGraphic
    {
        public bool destination;
        void Dot(VertexHelper mesh,Vector2 center,float radius)
        {
            int start=mesh.currentVertCount;mesh.AddVert(center,color,Vector2.zero);
            for(int i=0;i<=16;i++){float angle=i*Mathf.PI/8;mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,color,Vector2.zero);if(i>0)mesh.AddTriangle(start,start+i,start+i+1);}
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var center=rectTransform.rect.center;
            if(destination){mesh.AddVert(center+new Vector2(-10,7),color,Vector2.zero);mesh.AddVert(center+new Vector2(10,7),color,Vector2.zero);mesh.AddVert(center+new Vector2(0,-8),color,Vector2.zero);mesh.AddTriangle(0,1,2);}
            else{Dot(mesh,center+new Vector2(0,-3),6);for(int i=0;i<4;i++)Dot(mesh,center+new Vector2(-9+i*6,5+Mathf.Sin(i)*2),3);}
        }
    }
}
