using UnityEngine;
using UnityEngine.UI;
using static Mewgic.Clans.ClanPanel;

namespace Mewgic.Clans
{
    // Vector previews for the six divisions. No new finalized art is baked into the project.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LeagueBadge : MaskableGraphic
    {
        static readonly Color[] Accents =
        {
            new(.77f,.39f,.23f), new(.63f,.78f,.87f), new(1f,.69f,.20f),
            new(.40f,.91f,.83f), new(.51f,.76f,1f), new(.98f,.39f,.77f)
        };
        static readonly int[] Sides = { 20, 8, 10, 6, 4, 12 };
        int tier;
        public static Color Accent(int index) => Accents[Mathf.Clamp(index,0,5)];

        internal static LeagueBadge Add(Transform parent,string name,int index,float x,float y,float w,float h)
        {
            var node = Rect(parent,name,x,y,w,h).gameObject;
            node.AddComponent<CanvasRenderer>();
            var badge = node.AddComponent<LeagueBadge>();
            badge.tier = Mathf.Clamp(index,0,5);
            badge.raycastTarget = false;
            return badge;
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect r = rectTransform.rect;
            float scale = Mathf.Min(r.width,r.height)*.47f;
            if (scale <= 0) return;
            Vector2 center = r.center;
            var accent = Accent(tier);
            float rotation = Mathf.PI/2;
            bool pointed = tier is 2 or 5;
            Shape(mesh,center,new Vector2(scale,scale),Sides[tier],rotation,pointed,new Color(1,.79f,.40f));
            Shape(mesh,center,new Vector2(scale*.89f,scale*.89f),Sides[tier],rotation,pointed,accent);
            Shape(mesh,center,new Vector2(scale*.70f,scale*.70f),Sides[tier],rotation,pointed,
                Color.Lerp(new Color(.10f,.035f,.19f),accent,.19f));
            var ivory = Color.Lerp(new Color(1,.96f,.79f),accent,.24f);
            Shape(mesh,center+Vector2.down*(scale*.14f),new Vector2(scale*.24f,scale*.19f),20,0,false,ivory);
            for (int i = 0; i < 4; i++)
            {
                float x = (i-1.5f)*.15f;
                float y = i is 1 or 2 ? .27f : .14f;
                Shape(mesh,center+new Vector2(x,y)*scale,new Vector2(scale*.072f,scale*.095f),12,0,false,ivory);
            }
        }

        static void Shape(VertexHelper mesh,Vector2 center,Vector2 radius,int sides,float rotation,bool pointed,Color tint)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(center,tint,Vector2.zero);
            for (int i = 0; i < sides; i++)
            {
                float angle = rotation + i*Mathf.PI*2/sides;
                float shape = pointed && i%2 != 0 ? .77f : 1f;
                float light = Mathf.Sin(angle);
                var edge = light >= 0 ? Color.Lerp(tint,Color.white,light*.17f) : Color.Lerp(tint,Color.black,-light*.22f);
                mesh.AddVert(center+new Vector2(Mathf.Cos(angle)*radius.x,light*radius.y)*shape,edge,Vector2.zero);
                mesh.AddTriangle(start,start+1+i,start+1+(i+1)%sides);
            }
        }
    }
}
