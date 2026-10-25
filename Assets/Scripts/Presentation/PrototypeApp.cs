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
        GameplayPresentation gameplayVisual;
        GameplayHud hud;
        GameplayTools tools;
        GameplayEndScreen endScreen;
        ProgressLedger progress;
        BoosterActions boosters;
        int originalBottleCount,hintTarget=-1;
        bool completionHandled,navigating;
        GameplayLevelInfo levelInfo;
        bool paused;
        Coroutine pourRoutine;
        public bool IsPaused => paused;
        [Serializable] public class BottleData { public int[] layers; }
        [Serializable] public class LevelData { public BottleData[] bottles; }
        static readonly Color Night = new Color32(16,22,46,255);
        static readonly Color Purple = new Color32(57,36,94,255);
        static readonly Color Gold = new Color32(244,202,114,255);
        static readonly Color[] Colors = { new Color32(35,210,242,255), new Color32(240,65,164,255), new Color32(255,211,59,255), new Color32(151,88,244,255) };
        RectTransform safe, board;
        Font font;
        Puzzle puzzle;
        Text status;
        Button undo, restart, home;
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
            var subtitle=Label(safe,"Subtitle","BİR ŞİŞE DOLUSU SİHİR",25,Color.white,new Vector2(.1f,.71f),new Vector2(.9f,.77f));
            var subtitleShadow=subtitle.gameObject.AddComponent<Shadow>();subtitleShadow.effectColor=new Color(0,0,0,.85f);subtitleShadow.effectDistance=new Vector2(1,-2);
            if(homeCharacter != null) homeCharacter.GetComponent<HomePotionDisplay>()?.Mount(homeCharacter.potionLayer);
            var play=Button(safe,"Play","OYNA  ›",new Vector2(.15f,.18f),new Vector2(.85f,.28f),()=>SceneManager.LoadScene("Gameplay"));
            if(homeCharacter != null) { homeCharacter.GetComponent<HomePlayButtonPresenter>()?.Bind(play); homeCharacter.Bind(play); }
            var footnote=Label(safe,"Footnote","İlk büyü • Renkleri bir araya getir",24,new Color(.94f,.92f,1f),new Vector2(.1f,.10f),new Vector2(.9f,.12f));
            var footnoteShadow=footnote.gameObject.AddComponent<Shadow>();footnoteShadow.effectColor=new Color(0,0,0,.9f);footnoteShadow.effectDistance=new Vector2(1,-2);
            if(homeCharacter != null) homeCharacter.GetComponent<HomeNavigation>()?.Mount(safe);
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
            var level=GameplayLevelCatalog.Current();levelInfo=level.Info;puzzle=new Puzzle(level.Bottles);
            originalBottleCount=puzzle.Count;progress=GameplayProgress.Current;boosters=new BoosterActions(puzzle,progress,originalBottleCount);
            board=Box(safe,"Bottle board",new Color(.12f,.10f,.23f,.85f),new Vector2(.05f,.29f),new Vector2(.95f,.73f));
            status=Label(safe,"Status","Önce kaynak, sonra hedef şişeye dokun.\nKarıştır: yalnızca şişelerin yerleri değişir.",25,Color.white,new Vector2(.06f,.2f),new Vector2(.94f,.27f));
            restart=Button(safe,"Restart","YENİDEN",new Vector2(.09f,.027f),new Vector2(.47f,.065f),Reset);
            home=Button(safe,"Home","ANA MENÜ",new Vector2(.53f,.027f),new Vector2(.91f,.065f),()=>{if(!busy&&!paused)SceneManager.LoadScene("Home");});
            gameplayVisual=GetComponent<GameplayPresentation>();if(gameplayVisual!=null)gameplayVisual.Mount(safe,board);
            hud=gameObject.AddComponent<GameplayHud>();hud.Mount(safe,PauseGame,ResumeGame,RestartPaused,ExitPaused);
            SkinGameplayControl(restart);SkinGameplayControl(home);
            tools=gameObject.AddComponent<GameplayTools>();undo=tools.Mount(safe,UseBooster);
            endScreen=gameObject.AddComponent<GameplayEndScreen>();endScreen.Mount(safe,Reset,NextLevel,()=>{if(!busy&&!paused)SceneManager.LoadScene("Home");},()=>paused);
            if(GameplayProgress.LoadError!=null)status.text="Kayıt okunamadı; haklar ve ödüller kilitli.";
            Refresh();
        }
        void SkinGameplayControl(Button control){control.GetComponent<Image>().sprite=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");control.GetComponent<Image>().type=Image.Type.Sliced;control.GetComponent<Image>().color=Color.white;control.GetComponent<Outline>().enabled=false;}
        void UpdateTools(){if(tools!=null)tools.Present(puzzle,progress,busy||paused,originalBottleCount);}
        public void UseBooster(BoosterKind kind)
        {
            if(busy||paused||puzzle.Won||navigating)return;
            if(kind==BoosterKind.Hint&&hintTarget>=0){status.text="İpucu zaten gösteriliyor; hak harcanmadı.";return;}
            bool applied=false;
            switch(kind)
            {
                case BoosterKind.Undo:applied=boosters.Undo();if(applied){selected=-1;hintTarget=-1;status.text="Son geçerli hamle geri alındı.";}break;
                case BoosterKind.Hint:if(boosters.Hint(out var move)){applied=true;selected=move.Source;hintTarget=move.Target;status.text=boosters.Message+" • Altın kaynak, mavi hedef.";}break;
                case BoosterKind.ExtraBottle:applied=boosters.Extra();if(applied){selected=-1;hintTarget=-1;status.text="Bu denemeye bir boş şişe eklendi.";}break;
                case BoosterKind.Shuffle:applied=boosters.Shuffle();if(applied){selected=-1;hintTarget=-1;status.text=boosters.Message;}break;
            }
            if(!applied)status.text=boosters.Message;Refresh();
        }
        void NextLevel(){if(busy||paused||!puzzle.Won||navigating)return;if(GameplayLevelCatalog.Advance(levelInfo.id,progress)){navigating=true;SceneManager.LoadScene("Gameplay");}}
        public void PauseGame(){if(hud==null||paused)return;paused=true;SetPresentationPaused(true);hud.ShowPaused(true);UpdateTools();}
        public void ResumeGame(){if(!paused)return;paused=false;SetPresentationPaused(false);hud.ShowPaused(false);UpdateTools();}
        void SetPresentationPaused(bool value){if(bottleViews==null)return;foreach(var b in bottleViews)b.GetComponent<GameplayBottleView>().SetPaused(value);}
        void CancelPour()
        {
            if(pourRoutine!=null){StopCoroutine(pourRoutine);pourRoutine=null;}
            var stream=board.Find("Pour stream");if(stream!=null)Destroy(stream.gameObject);
            if(bottleViews!=null)foreach(var b in bottleViews){var view=b.GetComponent<GameplayBottleView>();view.SetAnimating(false);view.Rig.localRotation=Quaternion.identity;view.Rig.anchoredPosition=Vector2.zero;}
            busy=false;if(gameplayVisual!=null)gameplayVisual.IsAnimating=false;
        }
        public void RestartPaused(){if(!paused)return;CancelPour();ResumeGame();Reset();}
        public void ExitPaused(){if(!paused)return;CancelPour();ResumeGame();SceneManager.LoadScene("Home");}
        void UpdateHud(){if(hud!=null)hud.Present(new GameplayHudState(levelInfo,puzzle));}
        void Reset(){if(busy||paused)return;puzzle.Restart();selected=-1;hintTarget=-1;completionHandled=false;endScreen.Hide();status.text="Önce kaynak, sonra hedef şişeye dokun.\nKarıştır: yalnızca şişelerin yerleri değişir.";Refresh();}
        void Refresh()
        {
            if(bottleViews==null||bottleViews.Length!=puzzle.Count)
            {
                if(bottleViews!=null)foreach(var old in bottleViews){old.gameObject.SetActive(false);Destroy(old.gameObject);}
                bottleViews=new RectTransform[puzzle.Count];
                var glass=Resources.Load<Sprite>("Gameplay/Gameplay_GlassShell_v001");
                for(int i=0;i<puzzle.Count;i++)
                {
                    int index=i;
                    var b=Box(board,"Bottle "+(i+1),Color.clear,Vector2.zero,Vector2.one);
                    bottleViews[i]=b;
                    var hit=b.gameObject.AddComponent<Button>();hit.targetGraphic=b.GetComponent<Image>();b.GetComponent<Image>().raycastTarget=true;hit.onClick.AddListener(()=>Select(index));
                    var view=b.gameObject.AddComponent<GameplayBottleView>();view.Initialize(glass,Colors);
                    Label(b,"Number",(i+1).ToString(),22,Gold,new Vector2(.2f,-.18f),new Vector2(.8f,-.02f));
                }
            }
            for(int i=0;i<puzzle.Count;i++)bottleViews[i].GetComponent<GameplayBottleView>().Present(puzzle.Bottle(i),selected==i);
            if(hintTarget>=0&&hintTarget<bottleViews.Length)bottleViews[hintTarget].GetComponent<GameplayBottleView>().ShowHintTarget();
            if(gameplayVisual!=null)gameplayVisual.Bind(bottleViews);
            UpdateHud();
            UpdateTools();restart.interactable=!busy&&!paused;home.interactable=!busy&&!paused;
            if(puzzle.Won&&!busy&&!paused&&!completionHandled)
            {
                completionHandled=true;int stars=new GameplayHudState(levelInfo,puzzle).AwardedStars;
                bool saved=progress.Claim(levelInfo.id,stars,puzzle.Won,out bool granted);UpdateTools();
                endScreen.Show(levelInfo.id,puzzle.Moves,stars,granted,saved,GameplayLevelCatalog.CanAdvance(levelInfo.id,progress));
            }
        }
        void Select(int target)
        {
            if(busy||paused||puzzle.Won)return;
            hintTarget=-1;
            if(selected<0){if(puzzle.Bottle(target).Count==0){status.text="Önce dolu bir şişe seç.";return;}selected=target;status.text="Hedef şişeyi seç.";Refresh();return;}
            if(selected==target){selected=-1;status.text="Seçim kaldırıldı.";Refresh();return;}
            int source=selected;
            int color=puzzle.Bottle(source)[puzzle.Bottle(source).Count-1];
            int amount=puzzle.Pour(source,target);
            if(amount==0){status.text="Bu şişeye dökülemez. Başka bir hedef seç.";Refresh();return;}
            selected=-1;pourRoutine=StartCoroutine(AnimatePour(source,target,color,amount));
        }
        IEnumerator AnimatePour(int source,int target,int color,int amount)
        {
            if(gameplayVisual!=null)gameplayVisual.IsAnimating=true;
            busy=true;undo.interactable=restart.interactable=home.interactable=false;status.text="İksir aktarılıyor…";UpdateHud();UpdateTools();
            var sourceView=bottleViews[source].GetComponent<GameplayBottleView>();
            var targetView=bottleViews[target].GetComponent<GameplayBottleView>();
            sourceView.SetAnimating(true);targetView.SetAnimating(true);
            var a=sourceView.Rig;var start=a.position;var returnPosition=bottleViews[source].position;var rotation=a.localRotation;
            int oldSibling=bottleViews[source].GetSiblingIndex();bottleViews[source].SetAsLastSibling();
            float direction=bottleViews[target].position.x>=a.position.x?1:-1;
            var tilt=Quaternion.Euler(0,0,-direction*100);
            var destination=targetView.Mouth-a.parent.TransformVector(tilt*new Vector3(0,a.rect.height*.46f,0))+a.parent.TransformVector(new Vector3(0,36,0));
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.24f){while(paused)yield return null;float e=Mathf.SmoothStep(0,1,t);a.position=Vector3.Lerp(start,destination,e);a.localRotation=Quaternion.Slerp(rotation,tilt,e);yield return null;}
            while(paused)yield return null;
            a.position=destination;a.localRotation=tilt;
            var stream=Box(board,"Pour stream",Colors[color%Colors.Length],Vector2.zero,Vector2.zero);
            stream.anchorMin=stream.anchorMax=new Vector2(.5f,.5f);stream.pivot=new Vector2(.5f,0);
            var shine=stream.gameObject.AddComponent<Outline>();shine.effectColor=new Color(1,1,1,.5f);shine.effectDistance=new Vector2(1,0);
            int sourceCount=sourceView.DisplayedLayers.Length,targetCount=targetView.DisplayedLayers.Length;
            var receiving=new int[targetCount+amount];System.Array.Copy(targetView.DisplayedLayers,receiving,targetCount);for(int i=targetCount;i<receiving.Length;i++)receiving[i]=color;
            targetView.Liquid.Layers=receiving;
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.32f)
            {
                while(paused)yield return null;
                sourceView.Liquid.Volume=sourceCount-amount*t;targetView.Liquid.Volume=targetCount+amount*t;
                var from=(Vector2)board.InverseTransformPoint(sourceView.Mouth);var to=(Vector2)board.InverseTransformPoint(targetView.Mouth);
                var delta=from-to;stream.anchoredPosition=to;stream.sizeDelta=new Vector2(4,delta.magnitude);stream.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg-90);
                yield return null;
            }
            while(paused)yield return null;
            Destroy(stream.gameObject);
            sourceView.Present(puzzle.Bottle(source),false);targetView.Present(puzzle.Bottle(target),false);
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.22f){while(paused)yield return null;float e=Mathf.SmoothStep(0,1,t);a.position=Vector3.Lerp(destination,returnPosition,e);a.localRotation=Quaternion.Slerp(tilt,Quaternion.identity,e);yield return null;}
            while(paused)yield return null;
            a.localRotation=Quaternion.identity;a.anchoredPosition=Vector2.zero;bottleViews[source].SetSiblingIndex(oldSibling);
            sourceView.SetAnimating(false);targetView.SetAnimating(false);
            if(gameplayVisual!=null)gameplayVisual.IsAnimating=false;
            busy=false;pourRoutine=null;status.text="Güzel hamle!";Refresh();
        }
    }
}
