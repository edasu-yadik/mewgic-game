using L=Mewgic.Localization.L;
using System;using System.Collections;using UnityEngine;using UnityEngine.UI;using UnityEngine.InputSystem;using TMPro;using Mewgic.Core;
namespace Mewgic.UI
{
    public sealed partial class PawGuide:MonoBehaviour
    {
        public const string BasicId="paw.basic.v1";
        public const string BoosterId="paw.boosters.v1",BoosterOfferId="paw.boosters.offer.v1";
        // Önceki QA araçlarının derlenebilmesi için eski enum değerleri korunur; menü üretilmez.
        public enum Phase{Closed,Welcome,Source,Target,Celebration,Menu,General,Feature,Boosters,BoosterOffer,Delay,SourceClick,TargetClick,Pouring,CatTravel,CatClick,Thought}
        public Phase State{get;private set;}
        public PuzzleMove Recommended{get;private set;}
        public bool Visible=>State!=Phase.Closed;
        public bool DemoRunning=>demo;
        public bool BlocksGameplayActions=>isActiveAndEnabled&&demo;
        public bool ThoughtVisible=>bubble!=null&&bubble.gameObject.activeSelf&&fade.alpha>.05f;
        public float CursorPress=>cursorArt!=null?cursorArt.Press:0;
        public event Action BoosterHelpRequested;
        internal Action ReplayFinished;
        public static string HelpText=>L.V("tutorial.help");
        PrototypeApp app;Puzzle model;ProgressLedger ledger;RectTransform safe,cat,hit,bubble,cursor;
        TMP_Text message,gameplayStatus;Button thoughtClose;CanvasGroup fade,cursorFade;PawCursorGraphic cursorArt;
        Mewgic.Characters.WizardCatPresentation actor;Coroutine routine;PawTutorialDefinition feature;
        bool demo,driving,sourceSelected,transferFinished,thoughtClosing,clearDemoStatus,cursorAtTarget;RectTransform cursorTarget;
        int startMoves;float thoughtAt,fadeStart,fadeFrom;
        readonly Vector3[] corners=new Vector3[4];
        public void Mount(PrototypeApp host,RectTransform area,Puzzle puzzle,ProgressLedger progress,int level)
        {
            app=host;safe=area;model=puzzle;ledger=progress;cat=GameObject.Find("GameplayWizardCat")?.GetComponent<RectTransform>();actor=cat!=null?cat.GetComponent<Mewgic.Characters.WizardCatPresentation>():null;gameplayStatus=GameObject.Find("Status")?.GetComponent<TMP_Text>();
            var root=(RectTransform)GameObject.Find("TutorialOverlay").transform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            hit=Rect(root,"PawCatTouch",Vector2.zero,Vector2.zero);var image=hit.gameObject.AddComponent<Image>();image.color=Color.clear;hit.gameObject.AddComponent<Button>().onClick.AddListener(OpenMenu);
            bubble=Rect(root,"PawThought",new Vector2(.025f,.205f),new Vector2(.645f,.303f));bubble.gameObject.AddComponent<PawThoughtGraphic>().raycastTarget=false;
            fade=bubble.gameObject.AddComponent<CanvasGroup>();fade.blocksRaycasts=fade.interactable=true;
            var text=Rect(bubble,"PawMessage",new Vector2(.065f,.10f),new Vector2(.83f,.90f));message=text.gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();MewgicTypographySettings.Prepare(message);message.font=MewgicTypographySettings.Current.body;message.fontSize=23;message.enableAutoSizing=true;message.fontSizeMin=16;message.fontSizeMax=23;message.alignment=TextAlignmentOptions.Center;message.color=new Color32(62,34,56,255);message.raycastTarget=false;
            var close=Rect(bubble,"PawThoughtClose",new Vector2(.835f,.38f),new Vector2(.965f,.93f));
            var closeHit=close.gameObject.AddComponent<Image>();closeHit.color=Color.clear;
            thoughtClose=close.gameObject.AddComponent<Button>();thoughtClose.targetGraphic=closeHit;
            thoughtClose.onClick.AddListener(DismissThought);
            var closeText=Rect(close,"Label",Vector2.zero,Vector2.one).gameObject.AddComponent<TextMeshProUGUI>();
            closeText.font=message.font;closeText.text="×";closeText.fontSize=29;closeText.alignment=TextAlignmentOptions.Center;
            closeText.color=message.color;closeText.raycastTarget=false;
            cursor=Rect(root,"PawCursor",new Vector2(.5f,.5f),new Vector2(.5f,.5f));cursor.sizeDelta=new Vector2(74,84);cursorArt=cursor.gameObject.AddComponent<PawCursorGraphic>();cursorArt.raycastTarget=false;cursorFade=cursor.gameObject.AddComponent<CanvasGroup>();cursorFade.blocksRaycasts=cursorFade.interactable=false;
            bubble.gameObject.SetActive(false);cursor.gameObject.SetActive(false);fade.alpha=0;
            if(app.IsTutorialReplay)BeginDemo();else ConfigureOnboarding(level);
        }
        static RectTransform Rect(Transform p,string n,Vector2 a,Vector2 b){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=a;r.anchorMax=b;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        bool Ready=>cat!=null&&app!=null&&app.GuideCanInteract&&safe!=null&&safe.rect.width>0&&GameObject.Find("Bottle 1")!=null;
        RectTransform Bottle(int index){var go=GameObject.Find("Bottle "+(index+1));return go!=null?go.GetComponent<GameplayBottleView>().Rig:null;}
        Vector2 Point(RectTransform target){return safe.InverseTransformPoint(target.TransformPoint(target.rect.center));}
        bool TargetsPresent=>Recommended.Source>=0&&Recommended.Target>=0&&Recommended.Source<model.Count&&Recommended.Target<model.Count&&Bottle(Recommended.Source)!=null&&Bottle(Recommended.Target)!=null;
        bool BeginDemo(){if(demo||app==null)return false;demo=true;clearDemoStatus=true;State=Phase.Delay;app.UpdateTools();routine=StartCoroutine(Demonstrate());return true;}
        IEnumerator Demonstrate()
        {
            float deadline=Time.unscaledTime+3;while(!Ready&&Time.unscaledTime<deadline)yield return null;
            if(!Ready){Close();yield break;}yield return new WaitForSecondsRealtime(.6f);
            if(!Ready){Close();yield break;}var solved=PuzzleSolver.Solve(model.Export(),3000);
            if(solved.Status!=SearchStatus.Solved||solved.Moves.Count<2){demo=false;app.UpdateTools();routine=null;ShowThought(HelpText,Phase.Thought);yield break;}
            Recommended=solved.Moves[0];startMoves=model.Moves;sourceSelected=transferFinished=false;
            if(!TargetsPresent){Close();yield break;}
            cursor.gameObject.SetActive(true);cursorFade.alpha=0;cursor.anchoredPosition=Point(cat)+new Vector2(70,80);
            State=Phase.Source;yield return Travel(Bottle(Recommended.Source),.8f);if(!demo)yield break;
            State=Phase.SourceClick;yield return Press(()=>DriveBottle(Recommended.Source));if(!demo)yield break;
            if(!sourceSelected||model.Moves!=startMoves){Close();yield break;}
            State=Phase.Target;yield return Travel(Bottle(Recommended.Target),.85f);if(!demo)yield break;
            State=Phase.TargetClick;yield return Press(()=>DriveBottle(Recommended.Target));if(!demo)yield break;
            State=Phase.Pouring;deadline=Time.unscaledTime+4;while(!transferFinished&&Time.unscaledTime<deadline)yield return null;
            if(!transferFinished||model.Moves!=startMoves+1||!Ready){Close();yield break;}
            State=Phase.Celebration;actor?.TriggerMagic();yield return new WaitForSecondsRealtime(.35f);
            State=Phase.CatTravel;yield return Travel(cat,.8f);if(!demo)yield break;
            State=Phase.CatClick;yield return Press(()=>{actor?.TriggerMagic();ShowThought(HelpText,Phase.Thought);});
            float start=Time.unscaledTime;while(demo&&Time.unscaledTime-start<.22f){cursorFade.alpha=1-(Time.unscaledTime-start)/.22f;yield return null;}cursor.gameObject.SetActive(false);
            while(demo&&Time.unscaledTime-thoughtAt<3)yield return null;
            if(demo){routine=null;DismissThought();}
        }
        IEnumerator Travel(RectTransform target,float duration)
        {
            if(target==null){Close();yield break;}cursorTarget=target;cursorAtTarget=false;Vector2 from=cursor.anchoredPosition;float start=Time.unscaledTime;
            while(demo&&Time.unscaledTime-start<duration){if(target==null){Close();yield break;}float t=(Time.unscaledTime-start)/duration;cursorFade.alpha=Mathf.MoveTowards(cursorFade.alpha,1,Time.unscaledDeltaTime/.2f);cursor.localRotation=Quaternion.Euler(0,0,-8*Mathf.Sin(t*Mathf.PI));cursor.anchoredPosition=Vector2.Lerp(from,Point(target),Mathf.SmoothStep(0,1,t));yield return null;}
            if(demo&&target!=null){cursorAtTarget=true;cursor.localRotation=Quaternion.identity;cursor.anchoredPosition=Point(target);yield return new WaitForSecondsRealtime(.16f);}
        }
        IEnumerator Press(Action click)
        {
            Vector2 point=cursor.anchoredPosition;float start=Time.unscaledTime;bool clicked=false;
            while(demo&&Time.unscaledTime-start<.36f){float t=Mathf.Clamp01((Time.unscaledTime-start)/.36f);cursorArt.Press=t;float down=Mathf.Sin(t*Mathf.PI);cursor.localScale=Vector3.one*(1-.16f*down);cursor.anchoredPosition=point+Vector2.down*(7*down);if(!clicked&&t>=.5f){clicked=true;click();}yield return null;}
            if(demo&&!clicked)click();cursor.localScale=Vector3.one;cursor.anchoredPosition=point;cursorArt.Press=0;
        }
        void DriveBottle(int index)
        {
            if(!demo||!Ready||!TargetsPresent){Close();return;}driving=true;
            try{GameObject.Find("Bottle "+(index+1)).GetComponent<Button>().onClick.Invoke();}finally{driving=false;}
        }
        public bool AllowsBottleTap(int index)=>!isActiveAndEnabled||!demo||(driving&&((State==Phase.SourceClick&&index==Recommended.Source)||(State==Phase.TargetClick&&index==Recommended.Target)));
        public void Selected(int index){if(driving&&State==Phase.SourceClick&&index==Recommended.Source)sourceSelected=true;else onboardingSelected=index;}
        public void SelectionCleared(){onboardingSelected=-1;}
        public void MoveFinished(int source,int target){if(demo&&source==Recommended.Source&&target==Recommended.Target&&model.Moves==startMoves+1)transferFinished=true;else CompleteOnboardingMove();}
        // Eski çağrılar aynı küçük balona yönlenir; hiçbir menü/buton oluşturulmaz.
        public void OpenMenu(){if(State==Phase.Thought&&ThoughtVisible)DismissThought();else if(!demo&&Ready)ShowThought(HelpText,Phase.Thought);}
        public void ShowHowToPlay(){OpenMenu();}
        public void General(){OpenMenu();}
        public void Continue(){OpenMenu();}
        public void OpenBoosterHelp(){if(!demo&&Ready)BoosterHelpRequested?.Invoke();}
        public void SuspendHelp(){Close();}
        public void ResetGuide(){onboardingSelected=-1;lastOnboardingInput=Time.unscaledTime;Close();}
        public bool HandleBack(){if(!Visible)return false;if(State==Phase.Thought||State==Phase.Feature)DismissThought();else Close();return true;}
        public bool TryStartFeature(PawTutorialDefinition definition,bool conditionMet){if(definition==null||string.IsNullOrWhiteSpace(definition.tutorialId)||!conditionMet||Visible||!Ready||ledger.TutorialCompleted(definition.tutorialId))return false;feature=definition;ShowThought(string.IsNullOrWhiteSpace(definition.messageKey)?L.V("tutorial.help"):L.V(definition.messageKey),Phase.Feature);if(!string.IsNullOrWhiteSpace(definition.targetName))PointAt(GameObject.Find(definition.targetName)?.GetComponent<RectTransform>());return true;}
        public bool CompleteFeature(string completionKey){if(State!=Phase.Feature||feature==null||completionKey!=feature.completionKey||!ledger.CompleteTutorial(feature.tutorialId))return false;Close();return true;}
        public void RestartReplay(){if(ReplayFinished!=null)BeginDemo();}

        public void ShowBoosterTip(BoosterKind kind,RectTransform target)
        {
            if(demo||!Ready||target==null)return;
            string id="paw.tool."+kind+".v1";
            if(ledger.TutorialCompleted(id)||!ledger.CompleteTutorial(id))return;
            Close();
            ShowThought(L.V(kind==BoosterKind.Undo?"tutorial.tool_undo":kind==BoosterKind.Hint?"tutorial.tool_hint":
                kind==BoosterKind.ExtraBottle?"tutorial.tool_extra":"tutorial.tool_shuffle"),Phase.Feature);
            PointAt(target);
        }

        void PointAt(RectTransform target)
        {
            if(target==null)return;
            cursorTarget=target;cursorAtTarget=true;
            cursor.gameObject.SetActive(true);cursorFade.alpha=1;
        }

        void DismissThought()
        {
            bool completed=demo&&State==Phase.Thought;
            if(completed)ledger.CompleteTutorial(BasicId);
            Close();
            if(completed)ReplayFinished?.Invoke();
        }

        void ShowThought(string text,Phase phase){State=phase;thoughtClose.interactable=true;message.text=text;thoughtAt=Time.unscaledTime;thoughtClosing=false;fadeStart=thoughtAt;fade.alpha=0;bubble.localScale=Vector3.one*.94f;bubble.gameObject.SetActive(true);}
        public void Close()
        {
            StopOnboardingVisual();
            if(routine!=null){StopCoroutine(routine);routine=null;}demo=driving=false;if(isActiveAndEnabled&&app!=null)app.UpdateTools();if(thoughtClose!=null)thoughtClose.interactable=false;State=Phase.Closed;feature=null;if(cursor!=null){cursor.gameObject.SetActive(false);cursor.localScale=Vector3.one;cursorArt.Press=0;}
            if(bubble!=null&&bubble.gameObject.activeSelf&&!thoughtClosing){thoughtClosing=true;fadeFrom=fade.alpha;fadeStart=Time.unscaledTime;}
            if(clearDemoStatus&&gameplayStatus!=null){gameplayStatus.text="";clearDemoStatus=false;}
        }
#if UNITY_EDITOR
        // Yalnız izole Editor önizlemesi; gerçek oyuncu kaydını sıfırlamaz.
        public bool ReplayForPreview(){if(!GameplayProgress.IsTestProfile||!Ready||demo)return false;Close();return BeginDemo();}
#endif
        void OnDisable(){Close();if(bubble!=null)bubble.gameObject.SetActive(false);if(gameplayStatus!=null)gameplayStatus.enabled=true;}
        void LateUpdate()
        {
            TickOnboarding();
            if(cat==null){Close();if(gameplayStatus!=null)gameplayStatus.enabled=true;return;}cat.GetWorldCorners(corners);Vector2 min=safe.InverseTransformPoint(corners[0]),max=safe.InverseTransformPoint(corners[2]);hit.anchorMin=hit.anchorMax=new Vector2(.5f,.5f);hit.anchoredPosition=(min+max)*.5f;hit.sizeDelta=max-min;
            if(cursorAtTarget&&cursor.gameObject.activeSelf&&cursorTarget!=null)cursor.anchoredPosition=Point(cursorTarget)+Vector2.down*(7*Mathf.Sin(CursorPress*Mathf.PI));
            if(Visible&&(Keyboard.current?.escapeKey.wasPressedThisFrame??false))HandleBack();
            if(bubble.gameObject.activeSelf){float t=Mathf.SmoothStep(0,1,(Time.unscaledTime-fadeStart)/.2f);fade.alpha=thoughtClosing?Mathf.Lerp(fadeFrom,0,t):t;bubble.localScale=Vector3.one*Mathf.Lerp(.94f,1,fade.alpha);if(thoughtClosing&&t>=1)bubble.gameObject.SetActive(false);}
            if(!demo&&State==Phase.Feature&&cursor.gameObject.activeSelf){float age=Time.unscaledTime-thoughtAt;cursorArt.Press=Mathf.Repeat(age*1.6f,1);cursor.localScale=Vector3.one*(.88f+.12f*Mathf.Sin(age*7)*Mathf.Sin(age*7));}
            if(!demo&&(State==Phase.Thought||State==Phase.Feature)&&Time.unscaledTime-thoughtAt>=3)DismissThought();
            if(gameplayStatus!=null)gameplayStatus.enabled=!(demo||ThoughtVisible);
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]public sealed class PawHighlight:MaskableGraphic
    {
        static Vector2 Edge(int index,Rect r,float radius){int i=index%44,corner=i/11;float angle=(corner+(i%11)/10f)*Mathf.PI*.5f;var center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);return center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;}
        void Halo(VertexHelper v,Rect r,float width,float alpha){int start=v.currentVertCount;float radius=Mathf.Min(20,r.width*.15f,r.height*.15f);var inner=new Rect(r.x+width,r.y+width,r.width-width*2,r.height-width*2);var tint=new Color(1,.78f,.25f,alpha);for(int i=0;i<=44;i++){v.AddVert(Edge(i,r,radius),tint,Vector2.zero);v.AddVert(Edge(i,inner,Mathf.Max(1,radius-width)),tint,Vector2.zero);if(i>0){int n=start+i*2;v.AddTriangle(n-2,n,n-1);v.AddTriangle(n-1,n,n+1);}}}
        void Circle(VertexHelper v,Vector2 p,float radius){int k=v.currentVertCount;v.AddVert(p,new Color(1,.82f,.3f,.9f),Vector2.zero);for(int i=0;i<=16;i++){float a=i*Mathf.PI/8;v.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Color(1,.82f,.3f,.9f),Vector2.zero);if(i>0)v.AddTriangle(k,k+i,k+i+1);}}
        protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;float pulse=(Mathf.Sin(Time.unscaledTime*2.5f)+1)*.5f;Halo(v,r,9,.055f+pulse*.025f);Halo(v,r,3,.32f+pulse*.12f);var paw=new Vector2(r.center.x,r.yMax+12+Mathf.Sin(Time.unscaledTime*2.5f)*3);Circle(v,paw,5);for(int i=0;i<4;i++)Circle(v,paw+new Vector2(-8+i*5,7+Mathf.Sin(i)*2),2.5f);}
        void Update(){SetVerticesDirty();}
    }
}
