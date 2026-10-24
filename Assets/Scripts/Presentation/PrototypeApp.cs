using System;
using System.Collections;
using Mewgic.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Mewgic
{
    public sealed class PrototypeApp : MonoBehaviour
    {
        [SerializeField] HomeCharacterPresenter homeCharacter;
        [Serializable] public class BottleData { public int[] layers; }
        [Serializable] public class LevelData { public BottleData[] bottles; }
        static readonly Color Night = new Color32(16,22,46,255);
        static readonly Color Purple = new Color32(57,36,94,255);
        static readonly Color Gold = new Color32(244,202,114,255);
        static readonly Color[] Colors = { new Color32(35,210,242,255), new Color32(240,65,164,255), new Color32(255,211,59,255), new Color32(151,88,244,255) };
        RectTransform safe, board;
        Font font;
        Puzzle puzzle;
        Text status, moves;
        Button undo, restart, home;
        GameObject win;
        RectTransform[] bottleViews;
        int selected = -1;
        bool busy;
        Rect previousSafe;
        int previousWidth, previousHeight;

        void Start()
        {
            if (SceneManager.GetActiveScene().name == "Boot") { SceneManager.LoadScene("Home"); return; }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Mewgic Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(900,1600); scaler.matchWidthOrHeight = .5f;
            var background = Box(canvasObject.transform, "Night", Night, Vector2.zero, Vector2.one);
            // Decorative islands and stars are independent UI elements.
            for(int i=0;i<26;i++)
            {
                float x=((i*37)%97)/100f, y=((i*23)%91)/100f;
                Label(background, "Spark", i%4==0 ? "+" : "·", 18, new Color(1,.84f,.5f,.35f), new Vector2(x,y), new Vector2(x+.025f,y+.025f));
            }
            safe = Box(canvasObject.transform,"Safe area",Color.clear,Vector2.zero,Vector2.one);
            ApplySafeArea();
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            if(SceneManager.GetActiveScene().name=="Gameplay") Gameplay(); else Home();
        }
        void Update()
        {
            if(safe != null && (Screen.safeArea != previousSafe || Screen.width != previousWidth || Screen.height != previousHeight)) ApplySafeArea();
        }
        void ApplySafeArea()
        {
            previousSafe=Screen.safeArea; previousWidth=Screen.width; previousHeight=Screen.height;
            if(Screen.width<=0 || Screen.height<=0) return;
            safe.anchorMin=new Vector2(previousSafe.xMin/Screen.width,previousSafe.yMin/Screen.height);
            safe.anchorMax=new Vector2(previousSafe.xMax/Screen.width,previousSafe.yMax/Screen.height);
            safe.offsetMin=safe.offsetMax=Vector2.zero;
        }
        RectTransform Box(Transform parent,string name,Color color,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=rt.offsetMax=Vector2.zero;
            var image=go.GetComponent<Image>(); image.color=color; image.raycastTarget=false;
            return rt;
        }
        Text Label(Transform parent,string name,string value,int size,Color color,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=rt.offsetMax=Vector2.zero;
            var text=go.GetComponent<Text>(); text.font=font; text.text=value; text.fontSize=size; text.color=color;
            text.alignment=TextAnchor.MiddleCenter; text.raycastTarget=false; text.resizeTextForBestFit=true; text.resizeTextMinSize=14; text.resizeTextMaxSize=size;
            return text;
        }
        Button Button(Transform parent,string name,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
        {
            var rt=Box(parent,name,Purple,min,max); var image=rt.GetComponent<Image>(); image.raycastTarget=true;
            var outline=rt.gameObject.AddComponent<Outline>(); outline.effectColor=Gold; outline.effectDistance=new Vector2(2,-2);
            var b=rt.gameObject.AddComponent<Button>(); b.targetGraphic=image; b.onClick.AddListener(action);
            Label(rt,"Label",text,32,Gold,new Vector2(.04f,.08f),new Vector2(.96f,.92f)); return b;
        }
        void Home()
        {
            if(homeCharacter != null) homeCharacter.Mount(safe, (RectTransform)safe.parent.Find("Night"));
            if(homeCharacter != null) { homeCharacter.GetComponent<HomeLogoPresenter>()?.Mount(safe); homeCharacter.GetComponent<HomeTopBar>()?.Mount(safe); }
            Label(safe,"Subtitle","BİR ŞİŞE DOLUSU SİHİR",25,Color.white,new Vector2(.1f,.71f),new Vector2(.9f,.77f));
            if(homeCharacter != null) homeCharacter.GetComponent<HomePotionDisplay>()?.Mount(homeCharacter.potionLayer);
            var play=Button(safe,"Play","OYNA  ›",new Vector2(.15f,.18f),new Vector2(.85f,.28f),()=>SceneManager.LoadScene("Gameplay"));
            if(homeCharacter != null) { homeCharacter.GetComponent<HomePlayButtonPresenter>()?.Bind(play); homeCharacter.Bind(play); }
            Label(safe,"Footnote","İlk büyü • Renkleri bir araya getir",24,new Color(.75f,.72f,.87f),new Vector2(.1f,.08f),new Vector2(.9f,.14f));
        }
        public static Puzzle LoadLevel()
        {
            var asset=Resources.Load<TextAsset>("Levels/Level01");
            if(asset==null) throw new InvalidOperationException("Level01 missing");
            var data=JsonUtility.FromJson<LevelData>(asset.text);
            return new Puzzle(Array.ConvertAll(data.bottles,b=>b.layers));
        }
        void Gameplay()
        {
            puzzle=LoadLevel();
            Label(safe,"Chapter","İLK BÜYÜ  /  01",32,Gold,new Vector2(.1f,.91f),new Vector2(.9f,.97f));
            Label(safe,"Goal","Her rengi kendi şişesinde topla",38,Color.white,new Vector2(.06f,.82f),new Vector2(.94f,.9f));
            moves=Label(safe,"Moves","",27,Gold,new Vector2(.2f,.76f),new Vector2(.8f,.82f));
            board=Box(safe,"Bottle board",new Color(.12f,.10f,.23f,.85f),new Vector2(.05f,.29f),new Vector2(.95f,.73f));
            status=Label(safe,"Status","Önce kaynak, sonra hedef şişeye dokun.",25,Color.white,new Vector2(.06f,.2f),new Vector2(.94f,.27f));
            undo=Button(safe,"Undo","GERİ AL",new Vector2(.07f,.11f),new Vector2(.47f,.18f),()=>{if(busy)return;puzzle.Undo();selected=-1;Refresh();});
            restart=Button(safe,"Restart","YENİDEN",new Vector2(.53f,.11f),new Vector2(.93f,.18f),Reset);
            home=Button(safe,"Home","ANA MENÜ",new Vector2(.27f,.025f),new Vector2(.73f,.08f),()=>{if(!busy)SceneManager.LoadScene("Home");});
            win=Box(safe,"Win panel",Night,new Vector2(.08f,.33f),new Vector2(.92f,.70f)).gameObject;
            Label(win.transform,"Success","BÜYÜ TAMAMLANDI!",44,Gold,new Vector2(.07f,.65f),new Vector2(.93f,.9f));
            Label(win.transform,"Message","Renkler yerini buldu.",28,Color.white,new Vector2(.1f,.43f),new Vector2(.9f,.64f));
            Button(win.transform,"Replay","TEKRAR OYNA",new Vector2(.12f,.12f),new Vector2(.88f,.34f),Reset);
            Refresh();
        }
        void Reset(){if(busy)return;puzzle.Restart();selected=-1;status.text="Önce kaynak, sonra hedef şişeye dokun.";Refresh();}
        void Refresh()
        {
            foreach(Transform child in board) Destroy(child.gameObject);
            bottleViews=new RectTransform[puzzle.Count];
            for(int i=0;i<puzzle.Count;i++)
            {
                int index=i, row=i/2,col=i%2;
                float x=.17f+col*.48f,y=.55f-row*.49f;
                var b=Box(board,"Bottle "+(i+1),new Color(.55f,.73f,.95f,.18f),new Vector2(x,y),new Vector2(x+.18f,y+.36f));
                bottleViews[i]=b;
                var outline=b.gameObject.AddComponent<Outline>();outline.effectColor=selected==i?Gold:new Color(.7f,.85f,1,.8f);outline.effectDistance=new Vector2(3,-3);
                var hit=b.gameObject.AddComponent<Button>();hit.targetGraphic=b.GetComponent<Image>();b.GetComponent<Image>().raycastTarget=true;hit.onClick.AddListener(()=>Select(index));
                var layers=puzzle.Bottle(i);
                for(int j=0;j<layers.Count;j++)Box(b,"Liquid "+j,Colors[layers[j]%Colors.Length],new Vector2(.08f,.03f+j*.215f),new Vector2(.92f,.235f+j*.215f));
                Box(b,"Glass reflection",new Color(1,1,1,.35f),new Vector2(.13f,.07f),new Vector2(.17f,.88f));
                Box(b,"Neck",new Color(.7f,.85f,1,.45f),new Vector2(.28f,.97f),new Vector2(.72f,1.10f)); Box(b,"Rim",Gold,new Vector2(.2f,1.08f),new Vector2(.8f,1.12f));
                Label(b,"Number",(i+1).ToString(),22,Gold,new Vector2(.2f,-.19f),new Vector2(.8f,-.02f));
            }
            moves.text="HAMLE  "+puzzle.Moves;
            undo.interactable=puzzle.CanUndo&&!busy;restart.interactable=!busy;home.interactable=!busy;
            win.SetActive(puzzle.Won&&!busy);
        }
        void Select(int target)
        {
            if(busy||puzzle.Won)return;
            if(selected<0){if(puzzle.Bottle(target).Count==0){status.text="Önce dolu bir şişe seç.";return;}selected=target;status.text="Hedef şişeyi seç.";Refresh();return;}
            if(selected==target){selected=-1;status.text="Seçim kaldırıldı.";Refresh();return;}
            int source=selected;
            int color=puzzle.Bottle(source)[puzzle.Bottle(source).Count-1];
            int amount=puzzle.Pour(source,target);
            if(amount==0){status.text="Bu şişeye dökülemez. Başka bir hedef seç.";return;}
            selected=-1;StartCoroutine(AnimatePour(source,target,color));
        }
        IEnumerator AnimatePour(int source,int target,int color)
        {
            busy=true;undo.interactable=restart.interactable=home.interactable=false;
            var a=bottleViews[source];var b=bottleViews[target];
            var start=a.localPosition;var destination=b.localPosition+new Vector3(0,90,0);
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.25f){a.localPosition=Vector3.Lerp(start,destination,t);a.localRotation=Quaternion.Euler(0,0,Mathf.Lerp(0,-28,t));yield return null;}
            var stream=Box(board,"Pour stream",Colors[color%Colors.Length],Vector2.zero,Vector2.zero);
            stream.anchorMin=stream.anchorMax=new Vector2(.5f,.5f);stream.sizeDelta=new Vector2(8,90);stream.localPosition=destination-new Vector3(0,60,0);
            yield return new WaitForSecondsRealtime(.22f);
            Destroy(stream.gameObject);
            busy=false;status.text="Güzel hamle!";Refresh();
        }
    }
}
