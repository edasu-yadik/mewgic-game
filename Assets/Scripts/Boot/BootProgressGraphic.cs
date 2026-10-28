using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    // Texture/material ayırmadan çizilen, gerçek değere kırpılan kapsül çubuk.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class BootProgressGraphic : MaskableGraphic
    {
        float value;
        public float Value {get=>value;set{float v=Mathf.Clamp01(value);if(Mathf.Approximately(this.value,v))return;this.value=v;SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            Capsule(vh,r,new Color32(150,94,30,255),new Color32(255,225,142,255));
            var inner=new Rect(r.x+3,r.y+3,Mathf.Max(0,r.width-6),Mathf.Max(0,r.height-6));
            Capsule(vh,inner,new Color32(17,10,36,255),new Color32(44,23,70,255));
            inner.width*=value;
            if(inner.width>0)Capsule(vh,inner,new Color32(103,42,182,255),new Color32(214,161,255,255));
        }
        static void Capsule(VertexHelper vh,Rect r,Color bottom,Color top)
        {
            if(r.width<=0||r.height<=0)return;float radius=Mathf.Min(r.width,r.height)*.5f;int center=vh.currentVertCount;
            vh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
            int index=0;
            for(int corner=0;corner<4;corner++)for(int step=0;step<=6;step++)
            {
                float a=(corner*90+step*15)*Mathf.Deg2Rad;
                var p=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;
                vh.AddVert(p,Color.Lerp(bottom,top,(p.y-r.yMin)/r.height),Vector2.zero);index++;if(index>1)vh.AddTriangle(center,center+index-1,center+index);
            }
            vh.AddTriangle(center,center+index,center+1);
        }
    }
}
