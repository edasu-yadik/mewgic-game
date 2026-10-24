using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    /// <summary>Six independent painted planes. No interaction or game state belongs here.</summary>
    public sealed class HomeEnvironment : MonoBehaviour
    {
        public Sprite sky, distantIslands, mainCastle, village, foreground, atmosphere;
        public Shader waterLightShader;
        RectTransform composition, skyRect, fogRect, foregroundRect;
        Image fog;
        RectTransform[] motes;
        Graphic[] lights;
        Material waterLight;
        float fogBaseY;
        static readonly int EnvironmentTimeId=Shader.PropertyToID("_EnvironmentTime");
        public int LayerCount { get; private set; }
        public float AnimationTime { get; private set; }
        public bool Mounted { get; private set; }

        public void Mount()
        {
            if (Mounted) return;
            Mounted = true;
            var background = transform.Find("Background");
            var oldNight = background.Find("Night");
            if (oldNight != null) oldNight.gameObject.SetActive(false);
            skyRect = Plane(background, "Layer01_Sky", sky, false).rectTransform;
            composition = new GameObject("EnvironmentComposition", typeof(RectTransform)).GetComponent<RectTransform>();
            composition.SetParent(transform.Find("WorldDecorations"), false);
            composition.anchorMin=composition.anchorMax=new Vector2(.5f,1f);
            composition.pivot=new Vector2(.5f,1f);
            composition.sizeDelta=new Vector2(900,1600);
            if(waterLightShader != null) waterLight=new Material(waterLightShader);
            var distant=Plane(composition,"Layer02_DistantIslands",distantIslands,true);
            WidthFit(distant);
            distant.color=new Color(.76f,.82f,1f,.78f);
            var castle=Plane(composition,"Layer03_MainCastle",mainCastle,true);
            WidthFit(castle);
            castle.rectTransform.localScale=Vector3.one*.94f;
            var houses=Plane(composition,"Layer04_Village",village,true);
            WidthFit(houses);
            if(waterLight!=null){distant.material=waterLight;castle.material=waterLight;houses.material=waterLight;}
            foregroundRect=Plane(composition,"Layer05_Foreground",foreground,true).rectTransform;
            var ambient=new GameObject("Layer06_Atmosphere",typeof(RectTransform)).GetComponent<RectTransform>();
            ambient.SetParent(composition,false);Stretch(ambient);
            fog=Plane(ambient,"DriftingMist",atmosphere,true);fogRect=fog.rectTransform;
            fog.preserveAspect=false;
            fogRect.anchorMin=fogRect.anchorMax=new Vector2(.5f,.5f);
            motes=new RectTransform[16];lights=new Graphic[16];
            for(int i=0;i<motes.Length;i++)
            {
                var go=new GameObject("GoldMote_"+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(Characters.WizardSparkGraphic));
                go.transform.SetParent(ambient,false);
                motes[i]=(RectTransform)go.transform;motes[i].sizeDelta=Vector2.one*(i%3==0?9:4);
                lights[i]=go.GetComponent<Graphic>();lights[i].raycastTarget=false;
            }
            LayerCount=6;Layout();
        }
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void WidthFit(Image image)
        {
            image.preserveAspect=false;var r=image.rectTransform;
            r.anchorMin=r.anchorMax=new Vector2(.5f,1f);r.pivot=new Vector2(.5f,1f);
            r.sizeDelta=new Vector2(900,900*image.sprite.rect.height/image.sprite.rect.width);r.anchoredPosition=Vector2.zero;
        }
        static Image Plane(Transform parent,string label,Sprite sprite,bool preserve)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();
            image.sprite=sprite;image.preserveAspect=preserve;image.raycastTarget=false;Stretch(image.rectTransform);return image;
        }
        public void Layout()
        {
            if(!Mounted)return;
            var viewport=((RectTransform)transform).rect.size;
            float scale=Mathf.Min(viewport.x/900f,viewport.y/1600f);
            composition.localScale=Vector3.one*scale;
            foregroundRect.anchoredPosition=new Vector2(0,-(viewport.y/scale-1600));
            var fogSize=atmosphere.rect.size;
            fogRect.sizeDelta=fogSize*Mathf.Max(950f/fogSize.x,(viewport.y/scale+60)/fogSize.y);
            fogBaseY=-(viewport.y/scale-1600)*.5f;
            fogRect.anchoredPosition=new Vector2(fogRect.anchoredPosition.x,fogBaseY+Mathf.Sin(AnimationTime*.045f)*4);
            // Sky covers uniformly; opaque architectural content stays within the portrait frame.
            var size=sky.rect.size;float cover=Mathf.Max(viewport.x/size.x,viewport.y/size.y);
            skyRect.anchorMin=skyRect.anchorMax=new Vector2(.5f,.5f);skyRect.sizeDelta=size*cover;skyRect.anchoredPosition=Vector2.zero;
        }
        void LateUpdate()
        {
            if(!Mounted)return;
            Layout();AnimationTime=Time.unscaledTime;
            float t=AnimationTime;
            if(waterLight!=null)waterLight.SetFloat(EnvironmentTimeId,t);
            fogRect.anchoredPosition=new Vector2(Mathf.Sin(t*.065f)*12,fogBaseY+Mathf.Sin(t*.045f)*4);
            fog.color=new Color(1,1,1,.14f+Mathf.Sin(t*.16f)*.015f);
            for(int i=0;i<motes.Length;i++)
            {
                float phase=i*2.39996f;
                motes[i].anchoredPosition=new Vector2(Mathf.Sin(phase)*410+Mathf.Sin(t*.12f+phase)*9,Mathf.Repeat(i*113+t*(3+i%3),1500)-750);
                lights[i].color=new Color(1,.76f,.32f,.10f+(Mathf.Sin(t*(.6f+i*.02f)+phase)+1)*.10f);
            }
        }
        void OnDestroy(){if(waterLight!=null)Destroy(waterLight);}
    }
}
