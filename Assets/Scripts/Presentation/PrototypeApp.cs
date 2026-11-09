using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;
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
        readonly Clans.ClanWinTrace clanTrace = new();
        int originalBottleCount,hintTarget=-1;
        bool completionHandled,navigating,firstAttempt,hintUsed;
        GameplayLevelInfo levelInfo;
        bool paused;
        Coroutine pourRoutine;
        RectTransform pourStream; Image pourStreamImage;
        int pouringSource=-1, originalSibling, pointerFrame=-1, pointerBottle=-1;
        float feedbackUntil, pointerTime;
        static bool tutorialReplayPending;
        bool tutorialReplay;
        public bool IsPaused => paused;
        internal bool IsTutorialReplay => tutorialReplay;
        [Serializable] public class BottleData { public int[] layers; }
        [Serializable] public class LevelData { public BottleData[] bottles; }
        static readonly Color Night = new Color32(16,22,46,255);
        static readonly Color Purple = new Color32(57,36,94,255);
        static readonly Color Gold = new Color32(244,202,114,255);
        static readonly Color[] Colors = { new Color32(35,210,242,255), new Color32(240,65,164,255), new Color32(255,211,59,255), new Color32(151,88,244,255) };
        RectTransform safe, board;
        TMP_FontAsset font;
        Puzzle puzzle;
        TMP_Text status;
        Mewgic.UI.PawGuide pawGuide;
        public bool GuideCanInteract=>!busy&&!paused&&!navigating&&puzzle!=null&&!puzzle.Won;
        internal int GuideSelectedBottle => selected;
        Button undo, restart, home;
        RectTransform[] bottleViews;
        string victoryReceipt=Guid.NewGuid().ToString("N");
        int selected = -1;
        bool busy;
        Rect previousSafe;
        int previousWidth, previousHeight;

        public static void RequestTutorialReplay(){if(SceneNavigation.LoadOnce("Gameplay"))tutorialReplayPending=true;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetTutorialReplay(){tutorialReplayPending=false;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ConfigureFrameRate()
        {
            if(Application.isMobilePlatform)Application.targetFrameRate=60;
        }
        void Start()
        {
            if (SceneManager.GetActiveScene().name == "Boot") { GameplayLevelCatalog.ResumeFromSave(); Mewgic.SceneNavigation.LoadOnce("Home"); return; }

            font = MewgicTypographySettings.Current.body;
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
            if(feedbackUntil>0&&Time.unscaledTime>=feedbackUntil){feedbackUntil=0;if(status!=null)status.text="";}
        }
        void OnApplicationPause(bool suspended)
        {
            if(!suspended)return;
            if(puzzle!=null&&!navigating&&endScreen?.IsWatchingAd!=true)
            {
                if(busy){CancelPour();Refresh(true);}
                if(!paused)PauseGame();
            }
            PlayerPrefs.Save();
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
        TMP_Text Label(Transform parent,string name,string value,int size,Color color,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Mewgic.Localization.LocalizedTMP)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=rt.offsetMax=Vector2.zero;
            var text=go.GetComponent<TMP_Text>(); text.font=font; text.text=value; text.fontSize=size; text.color=color;
            text.alignment=TextAlignmentOptions.Midline; text.raycastTarget=false; text.enableAutoSizing=true; text.fontSizeMin=14; text.fontSizeMax=size;
            MewgicTypographySettings.Prepare(text);return text;
        }
        Button Button(Transform parent,string name,string text,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
        {
            var rt=Box(parent,name,Purple,min,max); var image=rt.GetComponent<Image>(); image.raycastTarget=true;
            var outline=rt.gameObject.AddComponent<Outline>(); outline.effectColor=Gold; outline.effectDistance=new Vector2(2,-2);
            var b=rt.gameObject.AddComponent<PremiumButton>(); b.targetGraphic=image; b.onClick.AddListener(action);
            Label(rt,"Label",text,32,Gold,new Vector2(.04f,.08f),new Vector2(.96f,.92f)); return b;
        }
        void Home()
        {
            if(!GameplayProgress.IsTestProfile)GameplayLevelCatalog.ResumeFromSave();
            if(homeCharacter != null) homeCharacter.Mount(safe, (RectTransform)safe.parent.Find("Night"));
            if(homeCharacter != null) { homeCharacter.GetComponent<HomeLogoPresenter>()?.Mount(safe); homeCharacter.GetComponent<HomeTopBar>()?.Mount(safe); }
            if(homeCharacter != null) homeCharacter.GetComponent<HomePotionDisplay>()?.Mount(homeCharacter.potionLayer);
            var play=Button(safe,"Play",L.V("home.play"),new Vector2(.15f,.18f),new Vector2(.85f,.28f),()=>PlayCurrentLevel());
            if(homeCharacter != null) { homeCharacter.GetComponent<HomePlayButtonPresenter>()?.Bind(play); homeCharacter.Bind(play); }
            if(homeCharacter != null) homeCharacter.GetComponent<HomeNavigation>()?.Mount(safe);
            gameObject.AddComponent<MagicShopPanel>().Mount(safe,homeCharacter?.GetComponent<HomeTopBar>()?.coin);
            gameObject.AddComponent<Clans.ClanPanel>().Mount(safe, homeCharacter?.GetComponent<HomeNavigation>()?.team);
            gameObject.AddComponent<Mewgic.Daily.DailyActivitiesPanel>().Mount(safe);
            gameObject.AddComponent<Mewgic.Rewards.RewardHubPanel>().Mount(safe);
            gameObject.AddComponent<Mewgic.Rewards.MagicalEventsPanel>().Mount(safe);
            gameObject.AddComponent<Mewgic.Daily.DailyArrivalPopup>().Mount(safe);
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
            tutorialReplay=tutorialReplayPending;tutorialReplayPending=false;
            var level=tutorialReplay?GameplayLevelCatalog.Load(1):GameplayLevelCatalog.Current();levelInfo=level.Info;puzzle=new Puzzle(level.Bottles);
            originalBottleCount=puzzle.Count;progress=tutorialReplay?new ProgressLedger(new ProgressData(),_=>{}):GameplayProgress.Current;boosters=new BoosterActions(puzzle,progress,originalBottleCount);
            firstAttempt=!tutorialReplay&&progress.BeginFirstAttempt(levelInfo.id);
            board=Box(safe,"Bottle board",new Color(.12f,.10f,.23f,.85f),new Vector2(.05f,.29f),new Vector2(.95f,.73f));
            status=Label(safe,"Status","",25,Color.white,new Vector2(.06f,.2f),new Vector2(.94f,.27f));
            var tutorialOverlay=new GameObject("TutorialOverlay",typeof(RectTransform)).GetComponent<RectTransform>();
            tutorialOverlay.SetParent(safe,false);tutorialOverlay.anchorMin=new Vector2(.06f,.2f);tutorialOverlay.anchorMax=new Vector2(.94f,.27f);tutorialOverlay.offsetMin=tutorialOverlay.offsetMax=Vector2.zero;
            restart=Button(safe,"Restart",L.V("gameplay.restart"),new Vector2(.09f,.027f),new Vector2(.47f,.065f),Reset);
            home=Button(safe,"Home",!tutorialReplay&&GameplayLevelCatalog.FromMap?L.V("common.map"):L.V("common.home"),new Vector2(.53f,.027f),new Vector2(.91f,.065f),()=>{if(!busy&&!paused)ReturnFromGameplay();});
            gameplayVisual=GetComponent<GameplayPresentation>();if(gameplayVisual!=null)gameplayVisual.Mount(safe,board);
            hud=gameObject.AddComponent<GameplayHud>();hud.Mount(safe,PauseGame,ResumeGame,RestartPaused,ExitPaused);
            SkinGameplayControl(restart);SkinGameplayControl(home);
            tools=gameObject.AddComponent<GameplayTools>();undo=tools.Mount(safe,UseBooster);
            endScreen=gameObject.AddComponent<GameplayEndScreen>();endScreen.Mount(safe,Reset,NextLevel,ReturnFromResults,()=>paused,RetryReward);
            if(GameplayProgress.LoadError!=null)status.text=L.V("error.load");
            Refresh();
            pawGuide=gameObject.AddComponent<Mewgic.UI.PawGuide>();pawGuide.Mount(this,safe,puzzle,progress,levelInfo.id);if(tutorialReplay)pawGuide.ReplayFinished=ReturnFromGameplay;else GameplayLevelCatalog.Prewarm(levelInfo.id);
        }
        void ReturnFromResults()
        {
            if (busy || paused || navigating) return;
            navigating = true;
            endScreen.SetTransition(true);
            if (!SceneNavigation.LoadWithFade(tutorialReplay ? "Home" : "LevelMap",
                tutorialReplay ? null : () => Map.MapWorld.RequestResultChapter(levelInfo.id), RestoreResultNavigation))
                RestoreResultNavigation();
        }

        void RestoreResultNavigation()
        {
            navigating = false;
            endScreen.SetTransition(false,L.V("boot.error"));
        }

        void ReturnFromGameplay()=>Mewgic.SceneNavigation.LoadOnce(tutorialReplay?"Home":GameplayLevelCatalog.ReturnScene);
        void SkinGameplayControl(Button control){control.GetComponent<Image>().sprite=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");control.GetComponent<Image>().type=Image.Type.Sliced;control.GetComponent<Image>().color=Color.white;control.GetComponent<Outline>().enabled=false;}
        internal void UpdateTools(){if(tools!=null)tools.Present(puzzle,progress,busy||paused||tutorialReplay||pawGuide?.BlocksGameplayActions==true,originalBottleCount);}
        public void UseBooster(BoosterKind kind)
        {
            if(busy||paused||puzzle.Won||navigating||tutorialReplay)return;
            if(pawGuide!=null&&pawGuide.BlocksGameplayActions)return;
            if(kind==BoosterKind.Hint&&hintTarget>=0){Feedback(L.V("booster.hint_active"));return;}
            bool applied=false;
            switch(kind)
            {
                case BoosterKind.Undo:applied=boosters.Undo();if(applied){selected=-1;hintTarget=-1;status.text=L.V("booster.used_undo");}break;
                case BoosterKind.Hint:if(boosters.Hint(out var move)){applied=true;hintUsed=true;selected=move.Source;hintTarget=move.Target;status.text="";}break;
                case BoosterKind.ExtraBottle:applied=boosters.Extra();if(applied){selected=-1;hintTarget=-1;status.text=L.V("booster.used_extra");}break;
                case BoosterKind.Shuffle:applied=boosters.Shuffle();if(applied){selected=-1;hintTarget=-1;status.text=boosters.Message;}break;
            }
            if(!applied)Feedback(boosters.Message);else{clanTrace.Booster(kind);pawGuide?.ResetGuide();if(!string.IsNullOrEmpty(status.text))feedbackUntil=Time.unscaledTime+1.45f;}Refresh();if(applied)pawGuide?.ShowBoosterTip(kind,tools.Target(kind));
        }
        void NextLevel()
        {
            if(busy||paused||tutorialReplay||!endScreen.IsShowing||!puzzle.Won||navigating||
                !GameplayLevelCatalog.CanAdvance(levelInfo.id,progress))return;
            StartCoroutine(AdvancePrepared());
        }

        internal void PlayCurrentLevel(Action<string> reportError=null)
        {
            if(navigating)return;
            GameplayLevelCatalog.ResumeFromSave();
            StartCoroutine(EnterPreparedGameplay(reportError));
        }

        IEnumerator EnterPreparedGameplay(Action<string> reportError)
        {
            navigating=true;
            var task=GameplayLevelCatalog.PrepareAsync(GameplayLevelCatalog.SelectedId);
            while(!task.IsCompleted)yield return null;
            if(task.IsCompletedSuccessfully&&SceneNavigation.LoadOnce("Gameplay"))yield break;

            _=task.Exception;
            navigating=false;
            GameObject.Find("Play")?.GetComponent<PremiumPlayButton>()?.AllowRetry();
            string message=L.V("error.prepare");
            if(reportError!=null)reportError(message);
            else if(GameObject.Find("LevelPreparationNotice")==null)
                Label(safe,"LevelPreparationNotice",message,24,Gold,new Vector2(.1f,.12f),new Vector2(.9f,.17f));
        }

        IEnumerator AdvancePrepared()
        {
            navigating=true;
            endScreen.SetTransition(true,L.V("map.preparing"));
            int completed=levelInfo.id;
            var task=GameplayLevelCatalog.PrepareAsync(completed+1);
            while(!task.IsCompleted)yield return null;
            if(task.IsCompletedSuccessfully&&SceneNavigation.LoadOnce("Gameplay",
                ()=>GameplayLevelCatalog.AdvancePrepared(completed,progress,task.Result)))yield break;

            _=task.Exception;
            navigating=false;
            endScreen.SetTransition(false,L.V("error.prepare"));
        }
        public void PauseGame(){if(hud==null||paused)return;pawGuide?.SuspendHelp();paused=true;SetPresentationPaused(true);hud.ShowPaused(true);UpdateBottleInput();UpdateTools();}
        public void ResumeGame(){if(!paused)return;paused=false;SetPresentationPaused(false);hud.ShowPaused(false);UpdateBottleInput();UpdateTools();if(!busy&&puzzle.Won)Refresh();if(tutorialReplay&&!busy){Reset();pawGuide?.RestartReplay();}}
        void SetPresentationPaused(bool value){if(bottleViews==null)return;foreach(var b in bottleViews)b.GetComponent<GameplayBottleView>().SetPaused(value);}
        void CancelPour()
        {
            if(pourRoutine!=null){StopCoroutine(pourRoutine);pourRoutine=null;}
            ClearPourStream();
            if(pouringSource>=0){bottleViews[pouringSource].SetSiblingIndex(originalSibling);pouringSource=-1;}
            if(bottleViews!=null)foreach(var b in bottleViews){var view=b.GetComponent<GameplayBottleView>();view.SetAnimating(false);view.Rig.localRotation=Quaternion.identity;view.Rig.anchoredPosition=Vector2.zero;}
            busy=false;if(gameplayVisual!=null)gameplayVisual.IsAnimating=false;
        }
        public void RestartPaused(){if(!paused)return;CancelPour();ResumeGame();if(!tutorialReplay)Reset();}
        public void ExitPaused(){if(!paused)return;CancelPour();ResumeGame();ReturnFromGameplay();}
        void UpdateHud(){if(hud!=null)hud.Present(new GameplayHudState(levelInfo,puzzle));}
        void Reset(){if(busy||paused||navigating)return;pawGuide?.ResetGuide();puzzle.Restart();clanTrace.Reset();firstAttempt=false;hintUsed=false;victoryReceipt=Guid.NewGuid().ToString("N");selected=-1;hintTarget=-1;completionHandled=false;endScreen.Hide();status.text="";Refresh();}
        void Refresh(bool settlePausedVictory=false)
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
                    var hit=b.gameObject.AddComponent<BottleTapButton>();hit.AcceptTap=()=>AcceptBottlePointer(index);hit.targetGraphic=b.GetComponent<Image>();b.GetComponent<Image>().raycastTarget=true;hit.onClick.AddListener(()=>Select(index));
                    var view=b.gameObject.AddComponent<GameplayBottleView>();view.Initialize(glass,Colors);
                }
            }
            for(int i=0;i<puzzle.Count;i++)bottleViews[i].GetComponent<GameplayBottleView>().Present(puzzle.Bottle(i),selected==i);
            if(hintTarget>=0&&hintTarget<bottleViews.Length){bottleViews[hintTarget].GetComponent<GameplayBottleView>().ShowHintTarget();if(selected>=0)bottleViews[selected].GetComponent<GameplayBottleView>().ShowHintSource();}
            if(gameplayVisual!=null)gameplayVisual.Bind(bottleViews);
            UpdateHud();UpdateBottleInput();
            UpdateTools();restart.interactable=!busy&&!paused;home.interactable=!busy&&!paused;
            if(puzzle.Won&&!busy&&(!paused||settlePausedVictory)&&!completionHandled)
            {
                completionHandled=true;int stars=new GameplayHudState(levelInfo,puzzle).AwardedStars;
                bool granted=false;bool saved=tutorialReplay||progress.Claim(levelInfo.id,stars,puzzle.Won,out granted,victoryReceipt,new GameplayHudState(levelInfo,puzzle).Completed,firstAttempt,hintUsed);UpdateTools();
                if(saved&&!tutorialReplay)clanTrace.Submit(levelInfo.id);
                endScreen.Show(levelInfo.id,stars,saved,saved&&!tutorialReplay&&GameplayLevelCatalog.CanAdvance(levelInfo.id,progress),tutorialReplay?default:progress.LastCompletionReward,tutorialReplay?null:progress,victoryReceipt);
            }
            bool showPlayControls = !endScreen.IsShowing;
            restart.gameObject.SetActive(showPlayControls);
            home.gameObject.SetActive(showPlayControls);
            status.gameObject.SetActive(showPlayControls);
        }
        void RetryReward()
        {
            if(busy||paused||navigating||!puzzle.Won)return;
            completionHandled=false;
            endScreen.Hide();
            Refresh();
        }

        bool AcceptBottlePointer(int index)
        {
            float now=Time.unscaledTime;
            if(!GuideCanInteract||pointerFrame==Time.frameCount||(pointerBottle==index&&now-pointerTime<.16f))
                return false;
            pointerFrame=Time.frameCount;pointerBottle=index;pointerTime=now;
            return true;
        }

        void UpdateBottleInput()
        {
            if(bottleViews==null)return;
            foreach(var bottle in bottleViews)bottle.GetComponent<Button>().interactable=GuideCanInteract;
        }

        void Feedback(string message)
        {
            status.text=message;
            feedbackUntil=Time.unscaledTime+1.45f;
        }

        void ClearPourStream()
        {
            if(pourStream==null)return;
            pourStream.gameObject.SetActive(false);
        }

        void Select(int target)
        {
            if(busy||paused||puzzle.Won||navigating)return;
            if(pawGuide!=null&&!pawGuide.AllowsBottleTap(target))return;
            pawGuide?.PlayerInteracted();
            hintTarget=-1;feedbackUntil=0;
            if(selected<0){if(puzzle.Bottle(target).Count==0){GameFeel.Play(GameCue.Invalid);Feedback(L.V("gameplay.empty"));return;}selected=target;status.text="";Refresh();pawGuide?.Selected(target);return;}
            if(selected==target){selected=-1;status.text="";Refresh();return;}
            int source=selected;
            int color=puzzle.Bottle(source)[puzzle.Bottle(source).Count-1];
            int amount=puzzle.Pour(source,target);
            if(amount==0){GameFeel.Play(GameCue.Invalid);Refresh();bottleViews[target].GetComponent<GameplayBottleView>().Reject();Feedback(L.V(puzzle.Bottle(target).Count==Puzzle.Capacity?"gameplay.target_full":"gameplay.target_color"));return;}
            clanTrace.Pour(source,target);
            selected=-1;busy=true;UpdateBottleInput();pourRoutine=StartCoroutine(AnimatePour(source,target,color,amount));
        }
        IEnumerator AnimatePour(int source,int target,int color,int amount)
        {
            if(gameplayVisual!=null)gameplayVisual.IsAnimating=true;
            busy=true;undo.interactable=restart.interactable=home.interactable=false;status.text="";UpdateHud();UpdateTools();
            if(GameFeel.Reduced)
            {
                GameFeel.Play(GameCue.Pour);
                yield return new WaitForSecondsRealtime(.12f);
                while(paused)yield return null;
                FinishPour(source,target);
                yield break;
            }
            var sourceView=bottleViews[source].GetComponent<GameplayBottleView>();
            var targetView=bottleViews[target].GetComponent<GameplayBottleView>();
            sourceView.SetAnimating(true);targetView.SetAnimating(true);
            var a=sourceView.Rig;var start=a.position;var returnPosition=bottleViews[source].position;var rotation=a.localRotation;
            pouringSource=source;originalSibling=bottleViews[source].GetSiblingIndex();bottleViews[source].SetAsLastSibling();
            float direction=bottleViews[target].position.x>=a.position.x?1:-1;
            var tilt=Quaternion.Euler(0,0,-direction*100);
            var destination=targetView.Mouth-a.parent.TransformVector(tilt*new Vector3(0,a.rect.height*.46f,0))+a.parent.TransformVector(new Vector3(0,36,0));
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.24f){while(paused)yield return null;float e=Mathf.SmoothStep(0,1,t);a.position=Vector3.Lerp(start,destination,e);a.localRotation=Quaternion.Slerp(rotation,tilt,e);yield return null;}
            while(paused)yield return null;
            a.position=destination;a.localRotation=tilt;GameFeel.Play(GameCue.Pour);
            if(pourStream==null)
            {
                pourStream=Box(board,"Pour stream",Color.white,Vector2.zero,Vector2.zero);
                pourStream.anchorMin=pourStream.anchorMax=new Vector2(.5f,.5f);
                pourStream.pivot=new Vector2(.5f,0);
                pourStreamImage=pourStream.GetComponent<Image>();
                pourStreamImage.raycastTarget=false;
                var shine=pourStream.gameObject.AddComponent<Outline>();
                shine.effectColor=new Color(1,1,1,.5f);
                shine.effectDistance=new Vector2(1,0);
            }
            var stream=pourStream;
            pourStreamImage.color=Colors[color%Colors.Length];
            stream.SetAsLastSibling();
            stream.gameObject.SetActive(true);
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
            ClearPourStream();
            sourceView.Present(puzzle.Bottle(source),false);targetView.Present(puzzle.Bottle(target),false);
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.22f){while(paused)yield return null;float e=Mathf.SmoothStep(0,1,t);a.position=Vector3.Lerp(destination,returnPosition,e);a.localRotation=Quaternion.Slerp(tilt,Quaternion.identity,e);yield return null;}
            while(paused)yield return null;
            a.localRotation=Quaternion.identity;a.anchoredPosition=Vector2.zero;bottleViews[source].SetSiblingIndex(originalSibling);pouringSource=-1;
            sourceView.SetAnimating(false);targetView.SetAnimating(false);
            FinishPour(source,target);
        }
        void FinishPour(int source,int target)
        {
            if(gameplayVisual!=null)gameplayVisual.IsAnimating=false;
            busy=false;pourRoutine=null;status.text="";
            if(!puzzle.Won)GameFeel.Play(GameCue.Valid);
            Refresh();bottleViews[target].GetComponent<GameplayBottleView>().Accept();pawGuide?.MoveFinished(source,target);
        }
    }
}
