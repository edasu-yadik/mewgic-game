using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    // Each band is model data; the glass art contains no baked liquids.
    public sealed class GameplayLiquidGraphic : MaskableGraphic
    {
        public int[] Layers = new int[0];
        public float Volume;
        public Color[] Palette;
        public bool Completed;
        public bool Paused;
        static readonly Vector2[] BottleShape =
        {
            new(.2f,.065f),new(.8f,.065f),new(.875f,.10f),new(.875f,.73f),new(.68f,.84f),
            new(.68f,.96f),new(.32f,.96f),new(.32f,.84f),new(.125f,.73f),new(.125f,.10f)
        };
        readonly List<Vector2> shape=new List<Vector2>(12), clipped=new List<Vector2>(12), polygon=new List<Vector2>(12);
        public bool Animating;
        float visualTime, lastMeshTime;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if(Palette==null)return;
            if(Mathf.Abs(Mathf.DeltaAngle(rectTransform.eulerAngles.z,0))>1){Tilted(vh);return;}
            var r=rectTransform.rect; float phase=visualTime;
            for(int layer=0;layer<Layers.Length;layer++)
            {
                float amount=Mathf.Clamp01(Volume-layer);if(amount<=0)continue;
                float bottom=.065f+layer*.169f,top=bottom+amount*.169f;
                var c=Palette[Layers[layer]%Palette.Length];
                const int steps=24;
                for(int x=0;x<steps;x++)
                {
                    float u=x/(float)steps,v=(x+1)/(float)steps;
                    float waveA=Mathf.Sin(u*6.28f+phase*1.6f)*.004f,waveB=Mathf.Sin(v*6.28f+phase*1.6f)*.004f;
                    float y0=bottom,y1=top+waveA,y2=top+waveB;
                    float left=.125f,right=.875f;
                    // Rounded lower inner corners, matching the glass body.
                    float lowA=y0+(layer==0?Mathf.Pow(Mathf.Abs(u-.5f)*2,8)*.035f:0);
                    float lowB=y0+(layer==0?Mathf.Pow(Mathf.Abs(v-.5f)*2,8)*.035f:0);
                    lowA=Mathf.Min(lowA,y1);lowB=Mathf.Min(lowB,y2);
                    float xa=Mathf.Lerp(left,right,u),xb=Mathf.Lerp(left,right,v);
                    Color tint=Color.Lerp(c*.62f,Color.Lerp(c,Color.white,.08f),Mathf.Sin((u+v)*.5f*Mathf.PI));tint.a=.96f;
                    Quad(vh,r,xa,lowA,xb,lowB,y1,y2,tint,Color.Lerp(tint,Color.white,.18f));
                    Color surface=Color.Lerp(c,Color.white,.67f);surface.a=.86f;
                    Quad(vh,r,xa,y1-.005f,xb,y2-.005f,y1+.003f,y2+.003f,surface,surface);
                }
                for(int n=0;n<7;n++)
                {
                    float x=.22f+Mathf.Repeat(n*.237f+layer*.11f,.56f);
                    float y=bottom+.025f+Mathf.Repeat(n*.041f+phase*.012f,.12f);
                    if(y>top-.015f)continue;
                    float size=n%3==0?.009f:.004f;
                    Color glow=Color.Lerp(c,Color.white,.8f);glow.a=.65f;
                    Dot(vh,r,new Vector2(x,y),size,glow,n%3==0);
                }
            }
        }
        // Clip bands against world-horizontal planes while the glass rotates.
        // Conserves each band's area instead of rotating a painted liquid rectangle.
        void Tilted(VertexHelper vh)
        {
            Rect r=rectTransform.rect;float angle=rectTransform.eulerAngles.z*Mathf.Deg2Rad;
            Vector2 up=new Vector2(Mathf.Sin(angle),Mathf.Cos(angle));
            shape.Clear();
            foreach(var p in BottleShape)shape.Add(new Vector2(p.x*r.width,p.y*r.height));
            Clip(shape,Vector2.up,.741f*r.height,clipped);
            float capacity=Area(clipped);
            float extent=r.width+r.height;
            for(int band=0;band<Layers.Length;band++)
            {
                float amount=Mathf.Clamp(Volume-band,0,1);if(amount<=0)continue;
                float bottom=Plane(shape,up,capacity*band/4,extent),top=Plane(shape,up,capacity*(band+amount)/4,extent);
                Clip(shape,up,top,clipped);Clip(clipped,-up,-bottom,polygon);if(polygon.Count<3)continue;
                Color c=Palette[Layers[band]%Palette.Length];int start=vh.currentVertCount;
                foreach(var p in polygon){float highlight=Mathf.Sin(p.x/r.width*Mathf.PI);Color tint=Color.Lerp(c*.65f,Color.Lerp(c,Color.white,.08f),highlight);tint.a=.97f;vh.AddVert(new Vector3(r.xMin+p.x,r.yMin+p.y),tint,Vector2.zero);}
                for(int n=1;n<polygon.Count-1;n++)vh.AddTriangle(start,start+n,start+n+1);
            }
        }
        static void Clip(List<Vector2> input,Vector2 normal,float height,List<Vector2> output)
        {
            output.Clear();if(input.Count==0)return;
            Vector2 previous=input[input.Count-1];float before=Vector2.Dot(previous,normal)-height;
            foreach(var current in input){float after=Vector2.Dot(current,normal)-height;if((before<=0)!=(after<=0))output.Add(Vector2.Lerp(previous,current,before/(before-after)));if(after<=0)output.Add(current);previous=current;before=after;}
        }
        static float Area(List<Vector2> p){float a=0;for(int i=0;i<p.Count;i++){var b=p[(i+1)%p.Count];a+=p[i].x*b.y-b.x*p[i].y;}return Mathf.Abs(a)*.5f;}
        static float AreaBelow(List<Vector2> shape,Vector2 normal,float height)
        {
            Vector2 first=Vector2.zero,last=Vector2.zero,previous=shape[shape.Count-1];int count=0;float area=0,before=Vector2.Dot(previous,normal)-height;
            void Append(Vector2 point){if(count++==0)first=point;else area+=last.x*point.y-point.x*last.y;last=point;}
            foreach(var current in shape){float after=Vector2.Dot(current,normal)-height;if((before<=0)!=(after<=0))Append(Vector2.Lerp(previous,current,before/(before-after)));if(after<=0)Append(current);previous=current;before=after;}
            if(count>0)area+=last.x*first.y-first.x*last.y;return Mathf.Abs(area)*.5f;
        }
        static float Plane(List<Vector2> shape,Vector2 up,float area,float extent){float lo=-extent,hi=extent;for(int i=0;i<20;i++){float mid=(lo+hi)*.5f;if(AreaBelow(shape,up,mid)<area)lo=mid;else hi=mid;}return (lo+hi)*.5f;}
        static Vector3 P(Rect r,float x,float y)=>new Vector3(r.xMin+x*r.width,r.yMin+y*r.height);
        static void Quad(VertexHelper v,Rect r,float xa,float ba,float xb,float bb,float ta,float tb,Color low,Color high)
        {int i=v.currentVertCount;v.AddVert(P(r,xa,ba),low,Vector2.zero);v.AddVert(P(r,xb,bb),low,Vector2.zero);v.AddVert(P(r,xb,tb),high,Vector2.zero);v.AddVert(P(r,xa,ta),high,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
        static void Dot(VertexHelper v,Rect r,Vector2 p,float size,Color c,bool ring)
        {const int sides=10;for(int n=0;n<sides;n++){float a=n*Mathf.PI*2/sides,b=(n+1)*Mathf.PI*2/sides;var o=new Vector2(Mathf.Cos(a)*size*1.8f,Mathf.Sin(a)*size);var q=new Vector2(Mathf.Cos(b)*size*1.8f,Mathf.Sin(b)*size);int i=v.currentVertCount;v.AddVert(P(r,p.x+o.x,p.y+o.y),c,Vector2.zero);v.AddVert(P(r,p.x+q.x,p.y+q.y),c,Vector2.zero);v.AddVert(P(r,p.x+q.x*(ring?.65f:0),p.y+q.y*(ring?.65f:0)),c,Vector2.zero);v.AddVert(P(r,p.x+o.x*(ring?.65f:0),p.y+o.y*(ring?.65f:0)),c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}}
        void Update()
        {
            if(Paused||GameFeel.Reduced)return;
            visualTime+=Time.unscaledDeltaTime;
            if(Layers.Length==0||(!Animating&&visualTime-lastMeshTime<1f/30f))return;
            lastMeshTime=visualTime;
            SetVerticesDirty();
        }
    }
    public sealed class GameplayBottleView : MonoBehaviour
    {
        public RectTransform Rig {get;private set;}
        public GameplayLiquidGraphic Liquid {get;private set;}
        public int[] DisplayedLayers=>Liquid.Layers;
        public float DisplayedVolume=>Liquid.Volume;
        float feedbackUntil;Color feedbackTint;
        bool selected,animating,paused;int hintRole;Image shell; Outline outline;HintCueGraphic hintCue;
        public Vector3 Mouth=>Rig.TransformPoint(new Vector3(0,Rig.rect.height*.46f,0));
        public void Initialize(Sprite glass,Color[] palette)
        {
            Rig=new GameObject("BottleVisual",typeof(RectTransform)).GetComponent<RectTransform>();Rig.SetParent(transform,false);Rig.anchorMin=Vector2.zero;Rig.anchorMax=Vector2.one;Rig.offsetMin=Rig.offsetMax=Vector2.zero;
            var fluid=new GameObject("DynamicLiquids",typeof(RectTransform),typeof(CanvasRenderer),typeof(GameplayLiquidGraphic));fluid.transform.SetParent(Rig,false);var fr=(RectTransform)fluid.transform;fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;Liquid=fluid.GetComponent<GameplayLiquidGraphic>();Liquid.raycastTarget=false;Liquid.Palette=palette;
            var glassGo=new GameObject("GlassShell",typeof(RectTransform),typeof(Image));glassGo.transform.SetParent(Rig,false);var gr=(RectTransform)glassGo.transform;gr.anchorMin=Vector2.zero;gr.anchorMax=Vector2.one;gr.offsetMin=gr.offsetMax=Vector2.zero;shell=glassGo.GetComponent<Image>();shell.sprite=glass;shell.raycastTarget=false;outline=glassGo.AddComponent<Outline>();outline.effectDistance=new Vector2(1,-1);
            var cue=new GameObject("HintCue",typeof(RectTransform),typeof(CanvasRenderer),typeof(HintCueGraphic));cue.transform.SetParent(Rig,false);var cr=(RectTransform)cue.transform;cr.anchorMin=cr.anchorMax=new Vector2(.5f,1);cr.anchoredPosition=new Vector2(0,18);cr.sizeDelta=new Vector2(28,28);hintCue=cue.GetComponent<HintCueGraphic>();hintCue.raycastTarget=false;cue.SetActive(false);
        }
        public void Present(IReadOnlyList<int> layers,bool chosen)
        {hintRole=0;hintCue.gameObject.SetActive(false);outline.effectDistance=new Vector2(1,-1);Liquid.Layers=new int[layers.Count];for(int i=0;i<layers.Count;i++)Liquid.Layers[i]=layers[i];Liquid.Volume=layers.Count;selected=chosen;bool complete=layers.Count==4;for(int i=1;i<layers.Count;i++)complete&=layers[i]==layers[0];Liquid.Completed=complete;outline.effectColor=chosen?new Color(1,.78f,.25f,.85f):complete?new Color(.6f,1,.8f,.4f):Color.clear;Liquid.SetVerticesDirty();}
        public void ShowHintSource(){hintRole=1;HintLight();}
        public void ShowHintTarget(){hintRole=2;HintLight();}
        void HintLight(){float wave=GameFeel.Reduced?0:Mathf.Sin(Time.unscaledTime*2.5f);var tint=hintRole==1?new Color(1,.78f,.25f):new Color(.1f,.85f,1);tint.a=.72f+wave*.12f;outline.effectColor=tint;outline.effectDistance=new Vector2(2,-2);hintCue.destination=hintRole==2;hintCue.color=tint;hintCue.gameObject.SetActive(true);hintCue.rectTransform.anchoredPosition=new Vector2(0,18+wave*2);}
        public void Reject()=>Flash(new Color(1,.44f,.51f));
        public void Accept()=>Flash(new Color(.55f,1,.83f));
        void Flash(Color tint){feedbackTint=tint;feedbackUntil=Time.unscaledTime+.24f;}
        public void SetPaused(bool value){paused=value;Liquid.Paused=value;}
        public void SetAnimating(bool value){animating=Liquid.Animating=value;}
        void Update(){if(shell!=null&&feedbackUntil>0){float left=feedbackUntil-Time.unscaledTime;shell.color=Color.Lerp(Color.white,feedbackTint,Mathf.Clamp01(left/.16f));if(left<=0)feedbackUntil=0;}if(!paused&&hintRole>0)HintLight();if(!animating&&!paused&&Rig!=null){var p=Rig.anchoredPosition;p.y=GameFeel.Reduced?(selected?12:0):Mathf.Lerp(p.y,selected?12:0,1-Mathf.Exp(-Time.unscaledDeltaTime*15));p.x=0;Rig.anchoredPosition=p;}}
    }
}
