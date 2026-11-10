using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    // Geometry samples individual artwork regions. Text and stars are rendered by their own UI.
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class VictoryArtwork : MaskableGraphic
    {
        internal enum Layer { Crop, Environment, Frame, Panel, Mascot, Logo, GardenExtension }
        internal readonly struct ClearText
        {
            internal readonly Rect Area;
            internal readonly float Sample;
            internal ClearText(float x, float y, float w, float h, float sample)
            { Area = new Rect(x,y,w,h); Sample = sample; }
        }

        static readonly Vector3[] Profile =
        {
            new(199,65,201), new(177,73,223), new(153,76,247), new(139,83,262),
            new(126,94,275), new(107,99,295), new(86,108,316), new(75,122,328),
            new(62,135,343), new(45,149,357), new(38,166,362), new(40,193,361),
            new(40,479,360), new(40,563,359), new(43,581,359), new(51,595,353),
            new(71,601,339), new(110,602,306), new(150,611,258), new(175,620,229), new(200,636,200)
        };
        static readonly Vector3[] CatProfile =
        {
            new(0,481,34), new(0,487,53), new(0,498,69), new(0,506,91),
            new(0,524,94), new(0,539,91), new(5,554,91), new(0,570,89),
            new(0,588,92), new(6,601,92), new(18,609,87), new(34,612,65)
        };

        static readonly Vector3[] LogoProfile =
        {
            new(195,3,205),new(182,7,218),new(175,15,226),new(160,18,240),new(150,25,249),
            new(146,34,254),new(153,42,249),new(162,48,238),new(190,50,212),new(198,54,204)
        };

        Texture2D atlas;
        Layer layer;
        Rect design;
        ClearText[] corrections;
        bool feather;
        float trace = 1;
        public override Texture mainTexture => layer == Layer.Panel || atlas == null ? base.mainTexture : atlas;

        internal void Bind(Texture2D art, Layer kind, Rect box, bool softEdge = false, params ClearText[] patches)
        {
            atlas = art;
            layer = kind;
            design = box;
            feather = softEdge;
            corrections = patches;
            raycastTarget = false;
            SetAllDirty();
        }

        internal void Trace(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (Mathf.Abs(progress-trace)<.002f) return;
            trace = progress;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (rectTransform.rect.width<=0 || design.width<=0 || atlas==null) return;
            switch (layer)
            {
                case Layer.Environment: DrawEnvironment(mesh); break;
                case Layer.Frame: DrawFrame(mesh); break;
                case Layer.Panel: DrawPanel(mesh); break;
                case Layer.Mascot: DrawSilhouette(mesh,CatProfile); break;
                case Layer.Logo: DrawSilhouette(mesh,LogoProfile); break;
                case Layer.GardenExtension: DrawGarden(mesh); break;
                default: DrawCrop(mesh,design,corrections); break;
            }
        }

        void DrawEnvironment(VertexHelper mesh)
        {
            DrawCrop(mesh,Rect.MinMaxRect(0,0,400,65),new[]
            {
                new ClearText(140,0,119,61,-1), new ClearText(284,5,113,36,-1)
            });
            for (int i=1; i<Profile.Length; i++)
            {
                var a=Profile[i-1]; var b=Profile[i];
                if (a.y>=479 && a.y<610)
                    Quad(mesh,new Rect(0,a.y,Mathf.Max(a.x,b.x),b.y-a.y),
                        Rect.MinMaxRect(0,Mathf.Lerp(403,458,(a.y-479)/132),42,Mathf.Lerp(403,458,(b.y-479)/132)),Color.white);
                else
                    Band(mesh,a.y,b.y,0,a.x,0,b.x);
                Band(mesh,a.y,b.y,a.z,400,b.z,400);
            }
            Quad(mesh,Rect.MinMaxRect(0,636,400,660),Rect.MinMaxRect(0,636,400,660),Color.white);
        }

        void DrawGarden(VertexHelper mesh)
        {
            // Blend into the approved existing foreground before the attachment's viewer buttons.
            float blend=design.yMin+14;
            Vertex(mesh,new Vector2(design.xMin,design.yMin),Vector2.zero,Color.clear);
            Vertex(mesh,new Vector2(design.xMax,design.yMin),new Vector2(400,0),Color.clear);
            Vertex(mesh,new Vector2(design.xMax,blend),new Vector2(400,124),Color.white);
            Vertex(mesh,new Vector2(design.xMin,blend),new Vector2(0,124),Color.white);
            AddTriangles(mesh,0);
            Quad(mesh,Rect.MinMaxRect(design.xMin,blend,design.xMax,design.yMax),
                Rect.MinMaxRect(0,124,400,714),Color.white);
        }

        internal static Vector2 FramePosition(float progress,bool right)
        {
            float y=Mathf.Lerp(Profile[0].y,Profile[^1].y,progress);
            for(int i=1;i<Profile.Length;i++)
            {
                var a=Profile[i-1];
                var b=Profile[i];
                if(y>b.y) continue;
                var row=Vector3.Lerp(a,b,Mathf.InverseLerp(a.y,b.y,y));
                return new Vector2(right?row.z-3:row.x+3,y);
            }
            return new Vector2(200,636);
        }

        void DrawFrame(VertexHelper mesh)
        {
            float maxY=Mathf.Lerp(65,636,trace);
            if (maxY<=65) return;
            for (int i=1; i<Profile.Length; i++)
            {
                var a=Profile[i-1]; var b=Profile[i];
                if (a.y>=maxY) break;
                b=Vector3.Lerp(a,b,Mathf.Clamp01((maxY-a.y)/(b.y-a.y)));
                Band(mesh,a.y,b.y,a.x,a.x+Mathf.Min(12,(a.z-a.x)*.5f),
                    b.x,b.x+Mathf.Min(12,(b.z-b.x)*.5f));
                Band(mesh,a.y,b.y,a.z-Mathf.Min(12,(a.z-a.x)*.5f),a.z,
                    b.z-Mathf.Min(12,(b.z-b.x)*.5f),b.z);
            }
            if (maxY>108) DrawCrop(mesh,Rect.MinMaxRect(63,55,336,107),null,true);
            if (trace>=.999f) DrawCrop(mesh,Rect.MinMaxRect(90,587,359,636),null,true);
        }

        void DrawPanel(VertexHelper mesh)
        {
            const int columns=16, steps=4;
            for (int row=1;row<Profile.Length;row++)
            {
                var a=Profile[row-1]; var b=Profile[row];
                for (int step=0;step<steps;step++)
                {
                    var p=Vector3.Lerp(a,b,(float)step/steps);
                    var q=Vector3.Lerp(a,b,(float)(step+1)/steps);
                    for (int c=0;c<columns;c++)
                    {
                        Vector2 tl=PanelPoint(p,c),tr=PanelPoint(p,c+1),br=PanelPoint(q,c+1),bl=PanelPoint(q,c);
                        int first=mesh.currentVertCount;
                        PanelVertex(tl); PanelVertex(tr); PanelVertex(br); PanelVertex(bl);
                        AddTriangles(mesh,first);
                    }
                }
            }

            Vector2 PanelPoint(Vector3 row,int col) =>
                new(Mathf.Lerp(row.x,row.z,(float)col/columns),row.y);
            void PanelVertex(Vector2 p)
            {
                float center=Mathf.Clamp01(1-Mathf.Abs(p.x-200)/172);
                float warmth=Mathf.Exp(-Mathf.Pow((p.y-249)/114,2))*center;
                float grain=Mathf.Sin(p.x*.205f+p.y*.079f)*Mathf.Cos(p.y*.117f-p.x*.18f)*.026f;
                var tint=Color.Lerp(new Color(.105f,.031f,.191f),new Color(.283f,.079f,.385f),center*.57f+warmth*.43f);
                tint.r+=grain+.033f*warmth;
                tint.b+=grain;
                Vertex(mesh,p,p,tint);
            }
        }

        void DrawSilhouette(VertexHelper mesh,Vector3[] profile)
        {
            for(int i=1;i<profile.Length;i++)
            {
                var a=profile[i-1]; var b=profile[i];
                float aa=Mathf.Min(3,(a.z-a.x)*.25f),bb=Mathf.Min(3,(b.z-b.x)*.25f);
                Band(mesh,a.y,b.y,a.x+aa,a.z-aa,b.x+bb,b.z-bb);
                int first=mesh.currentVertCount;
                Edge(a.x,a.y,0); Edge(a.x+aa,a.y,1); Edge(b.x+bb,b.y,1); Edge(b.x,b.y,0);
                AddTriangles(mesh,first);
                first=mesh.currentVertCount;
                Edge(a.z-aa,a.y,1); Edge(a.z,a.y,0); Edge(b.z,b.y,0); Edge(b.z-bb,b.y,1);
                AddTriangles(mesh,first);
            }
            void Edge(float x,float y,float alpha) =>
                Vertex(mesh,new Vector2(x,y),new Vector2(x,y),new Color(1,1,1,alpha));
        }

        void DrawCrop(VertexHelper mesh, Rect box, ClearText[] patches, bool soft=false)
        {
            bool softEdge=feather||soft;
            if ((patches==null || patches.Length==0) && !softEdge)
            {
                Quad(mesh,box,box,Color.white);
                return;
            }
            var xs=new System.Collections.Generic.List<float>{box.xMin,box.xMax};
            var ys=new System.Collections.Generic.List<float>{box.yMin,box.yMax};
            if(softEdge)
            {
                float inset=Mathf.Min(8,Mathf.Min(box.width,box.height)*.21f);
                xs.Add(box.xMin+inset); xs.Add(box.xMax-inset);
                ys.Add(box.yMin+inset); ys.Add(box.yMax-inset);
            }
            if(patches!=null)
                foreach(var patch in patches)
                {
                    xs.Add(patch.Area.xMin);xs.Add(patch.Area.xMax);
                    ys.Add(patch.Area.yMin);ys.Add(patch.Area.yMax);
                    if(patch.Sample<0)
                    {
                        xs.Add(Mathf.Max(box.xMin,patch.Area.xMin-5));xs.Add(Mathf.Min(box.xMax,patch.Area.xMax+5));
                        ys.Add(Mathf.Max(box.yMin,patch.Area.yMin-5));ys.Add(Mathf.Min(box.yMax,patch.Area.yMax+5));
                    }
                }
            xs.Sort();ys.Sort();
            for(int y=1;y<ys.Count;y++)
                for(int x=1;x<xs.Count;x++)
                {
                    var cell=Rect.MinMaxRect(xs[x-1],ys[y-1],xs[x],ys[y]);
                    if(cell.width<=0 || cell.height<=0) continue;
                    ClearText? mask=null;
                    if(patches!=null)
                        foreach(var patch in patches)
                            if(patch.Area.Contains(cell.center)){mask=patch;break;}
                    if(!mask.HasValue) DrawCell(cell,cell.xMin,cell.xMax,1,1);
                    else
                    {
                        var patch=mask.Value;
                        if(patch.Sample<0) continue;
                        DrawCell(cell,patch.Sample,patch.Sample+.7f,1,1);
                        float rightSample=patch.Area.xMax>=box.xMax ? patch.Sample : patch.Area.xMax+1;
                        DrawCell(cell,rightSample,rightSample+.7f,
                            Mathf.InverseLerp(patch.Area.xMin,patch.Area.xMax,cell.xMin),
                            Mathf.InverseLerp(patch.Area.xMin,patch.Area.xMax,cell.xMax));
                    }
                }

            void DrawCell(Rect cell,float leftU,float rightU,float leftAlpha,float rightAlpha)
            {
                int first=mesh.currentVertCount;
                Corner(cell.xMin,cell.yMin,leftU,cell.yMin,leftAlpha);
                Corner(cell.xMax,cell.yMin,rightU,cell.yMin,rightAlpha);
                Corner(cell.xMax,cell.yMax,rightU,cell.yMax,rightAlpha);
                Corner(cell.xMin,cell.yMax,leftU,cell.yMax,leftAlpha);
                AddTriangles(mesh,first);
            }
            void Corner(float x,float y,float u,float v,float alpha)
            {
                bool edge=softEdge && (x==box.xMin||x==box.xMax||y==box.yMin||y==box.yMax);
                if(patches!=null)
                    foreach(var patch in patches)
                    {
                        if(patch.Sample>=0) continue;
                        Rect hole=patch.Area;
                        float distance=Mathf.Max(x-hole.xMax,hole.xMin-x,y-hole.yMax,hole.yMin-y);
                        alpha*=Mathf.Clamp01(distance/5);
                    }
                Vertex(mesh,new Vector2(x,y),new Vector2(u,v),new Color(1,1,1,edge?0:alpha));
            }
        }

        void Band(VertexHelper mesh,float y0,float y1,float l0,float r0,float l1,float r1) =>
            Poly(mesh,new Vector2(l0,y0),new Vector2(r0,y0),new Vector2(r1,y1),new Vector2(l1,y1),Color.white);

        void Quad(VertexHelper mesh,Rect box,Rect source,Color tint)
        {
            int first=mesh.currentVertCount;
            Vertex(mesh,new Vector2(box.xMin,box.yMin),new Vector2(source.xMin,source.yMin),tint);
            Vertex(mesh,new Vector2(box.xMax,box.yMin),new Vector2(source.xMax,source.yMin),tint);
            Vertex(mesh,new Vector2(box.xMax,box.yMax),new Vector2(source.xMax,source.yMax),tint);
            Vertex(mesh,new Vector2(box.xMin,box.yMax),new Vector2(source.xMin,source.yMax),tint);
            AddTriangles(mesh,first);
        }

        void Poly(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint)
        {
            int first=mesh.currentVertCount;
            Vertex(mesh,a,a,tint);Vertex(mesh,b,b,tint);Vertex(mesh,c,c,tint);Vertex(mesh,d,d,tint);
            AddTriangles(mesh,first);
        }

        void Vertex(VertexHelper mesh,Vector2 pos,Vector2 source,Color tint)
        {
            Rect r=rectTransform.rect;
            Vector2 local=new(r.xMin+(pos.x-design.xMin)*r.width/design.width,
                r.yMax-(pos.y-design.yMin)*r.height/design.height);
            mesh.AddVert(local,tint*color,new Vector2(source.x/400,1-source.y/714));
        }

        static void AddTriangles(VertexHelper mesh,int first)
        {
            mesh.AddTriangle(first,first+1,first+2);
            mesh.AddTriangle(first,first+2,first+3);
        }
    }
}
