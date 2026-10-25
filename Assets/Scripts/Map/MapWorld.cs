using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.EventSystems;using UnityEngine.InputSystem.UI;using UnityEngine.SceneManagement;using UnityEngine.UI;
namespace Mewgic.Map
{
    // Harita katmanları; gerçek seviye ve kayıt bağlantısı diğer partial dosyadadır.
    public sealed partial class MapWorld : MonoBehaviour
    {
        public Sprite sky,node,chest,fog,homeIcon;
        public Sprite[] terrain=new Sprite[4];
        public Shader waterLightShader;
        public bool animateEnvironment=true;
        public Canvas Canvas {get;private set;}
        public RectTransform SafeArea {get;private set;}
        public RectTransform Viewport {get;private set;}
        public RectTransform Content {get;private set;}
        public RectTransform WorldRoot {get;private set;}
        public MapScrollRect Scroll {get;private set;}
        public float WorldHeight {get;private set;}
        public float EnvironmentTime {get;private set;}
        public int ActiveRegion {get;private set;}
        public bool Mounted {get;private set;}
        public bool ProgressionConnected=>progression!=null;
        public bool InformationVisible=>information!=null&&information.activeSelf;
        public IReadOnlyList<MapPoint> Points=>points;
        public static readonly string[] RegionNames={"Enchanted Village","Crystal Falls","Moon Garden","Sky Castle"};
        readonly List<MapPoint> points=new List<MapPoint>();readonly List<Graphic> motesGraphics=new List<Graphic>();
        readonly List<RectTransform>[] regionGroups={new List<RectTransform>(),new List<RectTransform>(),new List<RectTransform>(),new List<RectTransform>()};
        readonly Vector2[] bases={new Vector2(335,60),new Vector2(590,510),new Vector2(320,980),new Vector2(610,1430)};
        readonly float[] widths={620,560,600,540};
        readonly float[] heights=new float[4];
        readonly Vector2[] pointPositions={new Vector2(.23f,.43f),new Vector2(.45f,.39f),new Vector2(.67f,.44f)};
        Image background,mist;MapPathGraphic paths;RectTransform header,tabs,navigation,scrollArea;Text regionTitle,informationText;
        GameObject information;Image[] tabImages=new Image[4];Sprite panel;Font font;Material waterLight;Rect nativeSafe;int screenWidth,screenHeight;Vector2 lastSafeSize;bool jumping;int requestedRegion=-1;float jumpTarget,scale;

        static RectTransform Rect(Transform p,string name,Vector2 min,Vector2 max)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static Image Art(Transform p,string name,Sprite sprite,Vector2 min,Vector2 max)
        {var im=Rect(p,name,min,max).gameObject.AddComponent<Image>();im.sprite=sprite;im.raycastTarget=false;return im;}
        Text Label(Transform p,string name,string value,int size,Vector2 min,Vector2 max)
        {var t=Rect(p,name,min,max).gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.resizeTextForBestFit=true;t.resizeTextMinSize=14;t.resizeTextMaxSize=size;t.color=new Color32(255,248,228,255);t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;var s=t.gameObject.AddComponent<Shadow>();s.effectColor=new Color(0,0,0,.85f);s.effectDistance=new Vector2(1,-2);return t;}
        Image Panel(Transform p,string name,Vector2 min,Vector2 max)
        {var im=Art(p,name,panel,min,max);im.type=Image.Type.Sliced;return im;}
        Button Control(Transform p,string name,string text,Vector2 min,Vector2 max,Action action)
        {var im=Panel(p,name,min,max);im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>action());Label(im.transform,"Label",text,24,new Vector2(.07f,.1f),new Vector2(.93f,.9f));return b;}
        void Start(){Mount();}
        public void Mount()
        {
            if(Mounted)return;
            if(sky==null||node==null||chest==null||fog==null||terrain==null||terrain.Length!=4)throw new InvalidOperationException("Map art references missing");
            for(int i=0;i<4;i++)if(terrain[i]==null)throw new InvalidOperationException("Map region art missing");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");panel=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
            var canvasGo=new GameObject("LevelMapCanvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));Canvas=canvasGo.GetComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGo.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(900,1600);scaler.matchWidthOrHeight=.5f;
            background=Art(canvasGo.transform,"SkyAndDistantIslands",sky,Vector2.zero,Vector2.one);
            SafeArea=Rect(canvasGo.transform,"MapSafeArea",Vector2.zero,Vector2.one);ApplySafeArea();
            scrollArea=Rect(SafeArea,"MapScrollArea",Vector2.zero,Vector2.one);Scroll=scrollArea.gameObject.AddComponent<MapScrollRect>();Scroll.horizontal=false;Scroll.vertical=true;Scroll.movementType=ScrollRect.MovementType.Clamped;Scroll.inertia=true;Scroll.decelerationRate=.12f;Scroll.scrollSensitivity=45;
            Viewport=Rect(scrollArea,"MapViewport",Vector2.zero,Vector2.one);Viewport.gameObject.AddComponent<RectMask2D>();var hit=Viewport.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;Scroll.viewport=Viewport;
            Content=Rect(Viewport,"MapContent",Vector2.one*.5f,Vector2.one*.5f);Content.anchorMin=Content.anchorMax=new Vector2(.5f,1);Content.pivot=new Vector2(.5f,1);Scroll.content=Content;
            WorldRoot=Rect(Content,"MapWorldLayers",Vector2.zero,Vector2.zero);WorldRoot.pivot=Vector2.zero;WorldRoot.anchoredPosition=Vector2.zero;
            var ground=Rect(WorldRoot,"RegionTerrain",Vector2.zero,Vector2.one);
            var routes=Rect(WorldRoot,"MagicalPaths",Vector2.zero,Vector2.one);paths=routes.gameObject.AddComponent<MapPathGraphic>();paths.raycastTarget=false;
            var slots=Rect(WorldRoot,"LevelSlots",Vector2.zero,Vector2.one);var chests=Rect(WorldRoot,"TreasureChests",Vector2.zero,Vector2.one);var labels=Rect(WorldRoot,"RegionLabels",Vector2.zero,Vector2.one);
            if(waterLightShader!=null){waterLight=new Material(waterLightShader);waterLight.EnableKeyword("UNITY_UI_CLIP_RECT");}
            for(int i=0;i<4;i++){heights[i]=widths[i]*terrain[i].rect.height/terrain[i].rect.width;WorldHeight=Mathf.Max(WorldHeight,bases[i].y+heights[i]+130);}
            for(int i=3;i>=0;i--){var group=RegionGroup(ground,i);var im=Art(group,"Terrain_"+i,terrain[i],Vector2.zero,Vector2.one);im.preserveAspect=true;if(waterLight!=null)im.material=waterLight;}
            var routePoints=new List<RectTransform>();
            for(int i=0;i<4;i++)
            {
                var slotGroup=RegionGroup(slots,i);for(int n=0;n<3;n++){var location=Point(slotGroup,i,n,MapPointKind.LevelSlot,pointPositions[n],node,74);routePoints.Add((RectTransform)location.transform);}
                var chestGroup=RegionGroup(chests,i);Point(chestGroup,i,0,MapPointKind.Chest,new Vector2(.90f,.50f),chest,92);
                var labelGroup=RegionGroup(labels,i);var name=Panel(labelGroup,"RegionName_"+i,new Vector2(.10f,.14f),new Vector2(.90f,.25f));name.rectTransform.anchorMin=new Vector2(.10f,.18f);name.rectTransform.anchorMax=new Vector2(.90f,.18f);name.rectTransform.sizeDelta=new Vector2(0,62);Label(name.transform,"Label",RegionNames[i],26,new Vector2(.06f,.12f),new Vector2(.94f,.88f));
            }
            paths.Points=routePoints.ToArray();
            mist=Art(canvasGo.transform,"DriftingMist",fog,Vector2.zero,Vector2.one);mist.color=new Color(.78f,.86f,1,.18f);
            var motes=Rect(canvasGo.transform,"AtmosphereMotes",Vector2.zero,Vector2.one);
            for(int i=0;i<18;i++){var r=Rect(motes,"MapMote_"+i,new Vector2((i*37%97)/100f,(i*23%93)/100f),new Vector2((i*37%97)/100f,(i*23%93)/100f));r.sizeDelta=Vector2.one*(i%4==0?9:4);var g=r.gameObject.AddComponent<Characters.WizardSparkGraphic>();g.raycastTarget=false;g.color=new Color(1,.8f,.4f,.18f);motesGraphics.Add(g);}
            // Navigation is fixed above the world viewport; it never scrolls with terrain.
            header=Panel(SafeArea,"MapHeader",Vector2.zero,Vector2.one).rectTransform;
            Label(header,"MapPreviewTitle","MEWGIC • DÜNYA HARİTASI",23,new Vector2(.04f,.64f),new Vector2(.96f,.90f));
            regionTitle=Label(header,"VisibleRegion",RegionNames[0],35,new Vector2(.04f,.13f),new Vector2(.96f,.65f));
            tabs=Rect(SafeArea,"RegionNavigation",Vector2.zero,Vector2.one);
            for(int i=0;i<4;i++){int index=i;var b=Control(tabs,"RegionJump_"+i,RegionNames[i].Replace(" ","\n"),new Vector2(i*.25f+.003f,0),new Vector2((i+1)*.25f-.003f,1),()=>FocusRegion(index));tabImages[i]=b.GetComponent<Image>();}
            navigation=Panel(SafeArea,"MapNavigation",Vector2.zero,Vector2.one).rectTransform;
            var home=Control(navigation,"MapHome","ANA MENÜ",new Vector2(.015f,.14f),new Vector2(.31f,.86f),()=>{GameplayLevelCatalog.ResetSelection();SceneManager.LoadScene("Home");});
            if(homeIcon!=null){var icon=Art(home.transform,"HomeIcon",homeIcon,new Vector2(.02f,.1f),new Vector2(.27f,.9f));icon.preserveAspect=true;home.transform.Find("Label").GetComponent<RectTransform>().anchorMin=new Vector2(.27f,.1f);}
            Label(navigation,"ScrollInstruction","Keşfetmek için yukarı kaydır",23,new Vector2(.33f,.35f),new Vector2(.98f,.84f));
            Label(navigation,"PreviewNotice","Bölüm seç • Yıldızları topla",18,new Vector2(.33f,.10f),new Vector2(.98f,.43f));
            BuildInformation();
            if(UnityEngine.Object.FindFirstObjectByType<EventSystem>()==null){var es=new GameObject("MapEventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            Scroll.onValueChanged.AddListener(_=>UpdateVisibleRegion());Scroll.UserInput=CancelJump;
            Mounted=true;BindProgress();UnityEngine.Canvas.ForceUpdateCanvases();Layout();Scroll.verticalNormalizedPosition=0;FocusRegion(progression!=null?progression.CurrentRegion:0,false);UpdateVisibleRegion();
        }
        RectTransform RegionGroup(Transform parent,int index)
        {var r=Rect(parent,RegionNames[index],Vector2.zero,Vector2.zero);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(widths[index],heights[index]);r.anchoredPosition=bases[index];regionGroups[index].Add(r);return r;}
        MapPoint Point(RectTransform parent,int region,int slot,MapPointKind kind,Vector2 position,Sprite sprite,float artSize)
        {
            var r=Rect(parent,(kind==MapPointKind.Chest?"Chest_":"LevelSlot_")+region+"_"+slot,position,position);r.sizeDelta=Vector2.one*artSize;
            var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;var button=r.gameObject.AddComponent<Button>();button.targetGraphic=hit;
            var art=Art(r,"Art",sprite,new Vector2(.5f,.5f),new Vector2(.5f,.5f));art.rectTransform.sizeDelta=Vector2.one*artSize;art.preserveAspect=true;button.targetGraphic=art;
            var marker=r.gameObject.AddComponent<MapPoint>();marker.kind=kind;marker.regionIndex=region;marker.slotIndex=slot;points.Add(marker);
            button.onClick.AddListener(()=>ShowInformation(marker));return marker;
        }
        void BuildInformation()
        {
            var overlay=Rect(SafeArea,"MapInformation",Vector2.zero,Vector2.one);information=overlay.gameObject;var dim=overlay.gameObject.AddComponent<Image>();dim.color=new Color(.02f,.02f,.07f,.78f);dim.raycastTarget=true;
            var box=Panel(overlay,"InformationPanel",new Vector2(.10f,.34f),new Vector2(.90f,.68f));box.raycastTarget=true;
            Label(box.transform,"Title","HARİTA ÖNİZLEMESİ",35,new Vector2(.07f,.72f),new Vector2(.93f,.93f));informationText=Label(box.transform,"Message","",27,new Vector2(.08f,.32f),new Vector2(.92f,.70f));Control(box.transform,"MapInfoClose","TAMAM",new Vector2(.15f,.08f),new Vector2(.85f,.28f),()=>information.SetActive(false));information.SetActive(false);
        }
        void ApplySafeArea(){nativeSafe=Screen.safeArea;screenWidth=Screen.width;screenHeight=Screen.height;if(screenWidth<=0||screenHeight<=0)return;SafeArea.anchorMin=new Vector2(nativeSafe.xMin/screenWidth,nativeSafe.yMin/screenHeight);SafeArea.anchorMax=new Vector2(nativeSafe.xMax/screenWidth,nativeSafe.yMax/screenHeight);SafeArea.offsetMin=SafeArea.offsetMax=Vector2.zero;}
        static void Edge(RectTransform r,bool top,float height,float y){r.anchorMin=new Vector2(0,top?1:0);r.anchorMax=new Vector2(1,top?1:0);r.pivot=new Vector2(.5f,top?1:0);r.sizeDelta=new Vector2(-32,height);r.anchoredPosition=new Vector2(0,y);}
        public void Layout()
        {
            if(!Mounted)return;float normalized=Scroll.verticalNormalizedPosition;
            float headerHeight=Mathf.Max(148,68/Canvas.scaleFactor),tabHeight=Mathf.Max(96,48/Canvas.scaleFactor),bottomHeight=Mathf.Max(112,56/Canvas.scaleFactor);
            Edge(header,true,headerHeight,-8);Edge(tabs,true,tabHeight,-headerHeight-16);Edge(navigation,false,bottomHeight,8);
            scrollArea.offsetMin=new Vector2(0,bottomHeight+20);scrollArea.offsetMax=new Vector2(0,-headerHeight-tabHeight-28);
            UnityEngine.Canvas.ForceUpdateCanvases();scale=Viewport.rect.width/900f;
            WorldRoot.sizeDelta=new Vector2(900,WorldHeight);WorldRoot.localScale=Vector3.one*scale;Content.sizeDelta=new Vector2(Viewport.rect.width,WorldHeight*scale);
            float touch=Mathf.Max(78,44/(Canvas.scaleFactor*scale));foreach(var p in points)((RectTransform)p.transform).sizeDelta=Vector2.one*Mathf.Max(touch,p.kind==MapPointKind.Chest?92:78);
            Cover(background,((RectTransform)Canvas.transform).rect.size,40);Cover(mist,((RectTransform)Canvas.transform).rect.size,20);
            UnityEngine.Canvas.ForceUpdateCanvases();Scroll.verticalNormalizedPosition=Mathf.Clamp01(normalized);lastSafeSize=SafeArea.rect.size;UpdateVisibleRegion();
        }
        static void Cover(Image im,Vector2 viewport,float margin){var s=im.sprite.rect.size;im.rectTransform.anchorMin=im.rectTransform.anchorMax=new Vector2(.5f,.5f);im.rectTransform.sizeDelta=s*Mathf.Max((viewport.x+margin)/s.x,(viewport.y+margin)/s.y);im.rectTransform.anchoredPosition=Vector2.zero;}
        public float RegionScrollPosition(int index)
        {float hidden=Content.rect.height-Viewport.rect.height;if(hidden<=0)return 0;float center=(bases[index].y+heights[index]*.45f)*scale;return Mathf.Clamp01((center-Viewport.rect.height*.5f)/hidden);}
        public void FocusRegion(int index,bool smooth=true){if(index<0||index>=4)return;Scroll.StopMovement();requestedRegion=index;jumpTarget=RegionScrollPosition(index);jumping=smooth;if(!smooth){Scroll.verticalNormalizedPosition=jumpTarget;UpdateVisibleRegion();}}
        public void CancelJump(){jumping=false;requestedRegion=-1;UpdateVisibleRegion();}
        void UpdateVisibleRegion()
        {
            if(!Mounted)return;float center=(Mathf.Clamp01(Scroll.verticalNormalizedPosition)*Mathf.Max(0,Content.rect.height-Viewport.rect.height)+Viewport.rect.height*.5f)/Mathf.Max(.01f,scale);int closest=0;float distance=float.MaxValue;
            for(int i=0;i<4;i++){float d=Mathf.Abs(center-(bases[i].y+heights[i]*.45f));if(d<distance){distance=d;closest=i;}}
            if(requestedRegion>=0)closest=requestedRegion;else if(Scroll.verticalNormalizedPosition<.02f)closest=0;else if(Scroll.verticalNormalizedPosition>.98f)closest=3;
            ActiveRegion=closest;regionTitle.text=RegionNames[closest];for(int i=0;i<4;i++)tabImages[i].color=i==closest?Color.white:new Color(.65f,.65f,.83f,1);
            background.rectTransform.anchoredPosition=new Vector2(0,-Scroll.verticalNormalizedPosition*18);UpdateProgressTitle();
        }
        void LateUpdate()
        {
            if(!Mounted)return;if(screenWidth!=Screen.width||screenHeight!=Screen.height||nativeSafe!=Screen.safeArea)ApplySafeArea();if(lastSafeSize!=SafeArea.rect.size)Layout();
            if(jumping){Scroll.verticalNormalizedPosition=Mathf.Lerp(Scroll.verticalNormalizedPosition,jumpTarget,1-Mathf.Exp(-Time.unscaledDeltaTime*7));if(Mathf.Abs(Scroll.verticalNormalizedPosition-jumpTarget)<.001f){Scroll.verticalNormalizedPosition=jumpTarget;jumping=false;}}
            if(animateEnvironment)EnvironmentTime+=Time.unscaledDeltaTime;
            if(waterLight!=null)waterLight.SetFloat("_EnvironmentTime",EnvironmentTime);
            for(int i=0;i<4;i++){float bob=animateEnvironment?Mathf.Sin(EnvironmentTime*.45f+i*1.7f)*2:0;foreach(var r in regionGroups[i])r.anchoredPosition=bases[i]+new Vector2(0,bob);}
            for(int i=0;i<motesGraphics.Count;i++){var g=motesGraphics[i];float t=EnvironmentTime*.35f+i*1.8f;g.rectTransform.anchoredPosition=new Vector2(Mathf.Sin(t*.4f)*7,Mathf.Sin(t*.25f)*9);g.color=new Color(1,.8f,.4f,.12f+Mathf.Sin(t)*.06f);}
            if(animateEnvironment){mist.rectTransform.anchoredPosition=new Vector2(Mathf.Sin(EnvironmentTime*.06f)*6,Mathf.Cos(EnvironmentTime*.04f)*4);paths.SetVerticesDirty();}
        }
        void OnDestroy(){if(waterLight!=null)Destroy(waterLight);}
    }
}
