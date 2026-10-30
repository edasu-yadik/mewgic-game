using System;using UnityEngine;using UnityEngine.UI;using UnityEngine.InputSystem;using TMPro;using Mewgic.Core;
namespace Mewgic.UI
{
    public sealed class PawGuide:MonoBehaviour
    {
        public const string BasicId="paw.basic.v1";
        public const string BoosterId="paw.boosters.v1",BoosterOfferId="paw.boosters.offer.v1";
        public enum Phase{Closed,Welcome,Source,Target,Celebration,Menu,General,Feature,Boosters,BoosterOffer}
        public Phase State{get;private set;}
        public PuzzleMove Recommended{get;private set;}
        PrototypeApp app;Puzzle model;ProgressLedger ledger;RectTransform safe,bubble,hit,marker,menuShield,boosterShield,overlayRoot;CanvasGroup fade;TMP_Text message,gameplayStatus;Button next,how,hint,boosters,close;
        readonly Vector3[] corners=new Vector3[4];
        RectTransform cat;Mewgic.Characters.WizardCatPresentation actor;float closeAt,readyDeadline,fadeStartedAt,fadeFrom;PawTutorialDefinition feature;
        bool autoWelcomePending;int mountedFrame,recommendedAtMove;
        bool manualHelp,closingModal,closingBoosters,offerAfterCelebration;int helpPage;
        public event Action BoosterHelpRequested;
        public bool BlocksGameplayActions=>isActiveAndEnabled&&Visible&&(State==Phase.Menu||manualHelp);
        static readonly string[] Rules={"Üstteki renge dokunarak bir şişe seç. Sonra boş veya üst rengi aynı olan şişeye dokun.","Aynı renkleri kendi şişelerinde topla. Gerekirse Geri Al veya Yeniden ile devam edebilirsin."};
        static readonly string[] BoosterPages={"Geri Al: Son geçerli aktarımını geri alır.","İpucu: Takıldığında doğrulanmış bir hamleyi görsel olarak gösterir.","Ek Şişe: Bu denemeye fazladan boş bir şişe ekler.","Karıştır: Sıvıları karıştırmadan şişelerin yerlerini değiştirir; çözülebilirlik doğrulanır."};
        static readonly string[] BoosterTargets={"Undo","Hint","ExtraBottle","Shuffle"};
        const string WelcomeText="Merhaba! Ben senin büyücü yardımcınım. Hadi ilk iksirimizi hazırlayalım!";
        const string SourceText="Önce parlayan şişeye dokun!";
        const string TargetText="Şimdi aynı renkli sıvıyı uygun şişeye aktar!";
        const string CelebrationText="Harika! Artık renkleri kendi şişelerinde toplayabilirsin.";
        public bool Visible=>State!=Phase.Closed;
        public void Mount(PrototypeApp host,RectTransform area,Puzzle puzzle,ProgressLedger progress,int level)
        {
            app=host;safe=area;model=puzzle;ledger=progress;cat=GameObject.Find("GameplayWizardCat")?.GetComponent<RectTransform>();actor=cat!=null?cat.GetComponent<Mewgic.Characters.WizardCatPresentation>():null;
            gameplayStatus=GameObject.Find("Status")?.GetComponent<TMP_Text>();
            var root=(RectTransform)GameObject.Find("TutorialOverlay").transform;overlayRoot=root;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
            menuShield=Rect(root,"PawMenuShield",Vector2.zero,Vector2.one);var shieldImage=menuShield.gameObject.AddComponent<Image>();shieldImage.color=Color.clear;shieldImage.raycastTarget=true;menuShield.gameObject.SetActive(false);
            var toolRoot=GameObject.Find("GameplayBoosters");if(toolRoot!=null){boosterShield=Rect(toolRoot.transform,"PawBoosterShield",Vector2.zero,Vector2.one);var blocker=boosterShield.gameObject.AddComponent<Image>();blocker.color=Color.clear;blocker.raycastTarget=true;boosterShield.gameObject.SetActive(false);}
            hit=Rect(root,"PawCatTouch",Vector2.zero,Vector2.zero);var image=hit.gameObject.AddComponent<Image>();image.color=Color.clear;var button=hit.gameObject.AddComponent<Button>();button.onClick.AddListener(OpenMenu);
            bubble=Rect(root,"PawBubble",new Vector2(.025f,.201f),new Vector2(.825f,.286f));var art=bubble.gameObject.AddComponent<Image>();art.sprite=Resources.Load<Mewgic.Daily.DailyArt>("Daily/Art").parchment;art.type=Image.Type.Sliced;art.pixelsPerUnitMultiplier=2;art.raycastTarget=false;
            fade=bubble.gameObject.AddComponent<CanvasGroup>();message=Label(bubble,"PawMessage",new Vector2(.055f,.40f),new Vector2(.945f,.82f));
            next=Control("PawNext","Devam",.24f,.65f,Continue);how=Control("PawHow","Nasıl Oynanır?",.035f,.48f,ShowHowToPlay);hint=Control("PawHint","Hamle Öner (1 hak)",.52f,.965f,()=>{if(app.GuideCanInteract&&ledger.Remaining(BoosterKind.Hint)>0){Close();app.UseBooster(BoosterKind.Hint);}});boosters=Control("PawBoosters","Güçlendiricileri Tanı",.035f,.48f,OpenBoosterHelp);close=Control("PawClose","Kapat",.72f,.965f,Dismiss);
            marker=Rect(root,"PawHighlight",Vector2.zero,Vector2.zero);marker.gameObject.AddComponent<PawHighlight>().raycastTarget=false;
            Close();fade.alpha=0;bubble.gameObject.SetActive(false);
            mountedFrame=Time.frameCount;readyDeadline=Time.unscaledTime+3;
            autoWelcomePending=level==1&&!ledger.Completed(1)&&!ledger.TutorialCompleted(BasicId);
        }
        static RectTransform Rect(Transform p,string n,Vector2 a,Vector2 b){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=a;r.anchorMax=b;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        TMP_Text Label(Transform p,string n,Vector2 a,Vector2 b){var t=Rect(p,n,a,b).gameObject.AddComponent<TextMeshProUGUI>();MewgicTypographySettings.Prepare(t);t.fontSize=23;t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=23;t.alignment=TextAlignmentOptions.Midline;t.color=new Color32(60,30,24,255);return t;}
        Button Control(string n,string value,float left,float right,UnityEngine.Events.UnityAction action){var r=Rect(bubble,n,new Vector2(left,.04f),new Vector2(right,.38f));var im=r.gameObject.AddComponent<Image>();im.sprite=Resources.Load<Mewgic.Daily.DailyArt>("Daily/Art").button;var b=r.gameObject.AddComponent<Button>();b.onClick.AddListener(action);var t=Label(r,n+"Label",Vector2.zero,Vector2.one);t.text=value;t.color=new Color32(255,229,169,255);t.fontSizeMin=14;t.fontSizeMax=20;return b;}
        static void Place(Button button,Vector2 min,Vector2 max){var r=(RectTransform)button.transform;r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;}
        void Show(Phase phase,string text)
        {
            autoWelcomePending=false;closingModal=closingBoosters=false;State=phase;message.text=text;bubble.gameObject.SetActive(true);fadeStartedAt=Time.unscaledTime;fade.alpha=0;fade.blocksRaycasts=fade.interactable=true;bubble.localScale=Vector3.one*.95f;
            bool menu=phase==Phase.Menu,offer=phase==Phase.BoosterOffer,reading=manualHelp&&!menu&&!offer;
            bubble.anchorMin=new Vector2(.025f,menu?.196f:phase==Phase.Boosters?.22f:.201f);bubble.anchorMax=new Vector2(.825f,menu?.298f:phase==Phase.Boosters?.305f:.286f);
            next.gameObject.SetActive(phase==Phase.Welcome||reading||offer);how.gameObject.SetActive(menu);hint.gameObject.SetActive(menu);boosters.gameObject.SetActive(menu);close.gameObject.SetActive(true);
            message.font=menu?MewgicTypographySettings.Current.heading:MewgicTypographySettings.Current.body;message.fontSizeMin=menu?24:18;message.fontSizeMax=menu?26:23;
            message.rectTransform.anchorMin=new Vector2(.055f,menu?.71f:reading?.39f:.40f);message.rectTransform.anchorMax=new Vector2(.945f,menu?.90f:phase==Phase.Boosters?.84f:reading?.92f:.82f);
            Place(how,new Vector2(.035f,.39f),new Vector2(.48f,.675f));Place(hint,new Vector2(.52f,.39f),new Vector2(.965f,.675f));Place(boosters,new Vector2(.035f,.08f),new Vector2(.48f,.365f));Place(close,new Vector2(menu?.52f:.72f,menu?.08f:.04f),new Vector2(.965f,menu?.365f:.38f));
            next.GetComponentInChildren<TMP_Text>(true).text=offer?"Evet, Öğrenelim":phase==Phase.Boosters?(helpPage==BoosterPages.Length-1?"Bitir":"İleri"):reading&&helpPage==Rules.Length-1?"Menüye Dön":"Devam";
            close.GetComponentInChildren<TMP_Text>(true).text=offer?"Şimdi Değil":phase==Phase.Boosters?"Atla":"Kapat";
            hint.interactable=ledger.Remaining(BoosterKind.Hint)>0;hint.GetComponentInChildren<TMP_Text>(true).text=hint.interactable?"Hamle Öner (1 hak)":"Hamle Öner (hak yok)";
            menuShield.gameObject.SetActive(BlocksGameplayActions);
            if(boosterShield!=null)boosterShield.gameObject.SetActive(phase==Phase.Boosters);
            marker.SetParent(phase==Phase.Boosters?safe:overlayRoot,false);marker.SetAsLastSibling();
        }
        public void Continue(){if(!app.GuideCanInteract)return;if(State==Phase.BoosterOffer){OpenBoosterHelp();return;}if(State==Phase.Boosters){if(++helpPage>=BoosterPages.Length){ledger.CompleteTutorial(BoosterId);Close();}else Show(Phase.Boosters,BoosterPages[helpPage]);return;}if(manualHelp){if(++helpPage>=Rules.Length)OpenMenu();else Show(State,Rules[helpPage]);return;}if(TryMove()){Show(Phase.Source,SourceText);}else General();}
        bool TryMove(){var solution=PuzzleSolver.Solve(model.Export(),3000);if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0)return false;Recommended=solution.Moves[0];recommendedAtMove=model.Moves;return true;}
        bool Ready=>cat!=null&&app!=null&&app.GuideCanInteract&&safe!=null&&safe.rect.width>0&&GameObject.Find("Bottle 1")!=null;
        bool TargetsPresent=>Recommended.Source>=0&&Recommended.Target>=0&&Recommended.Source<model.Count&&Recommended.Target<model.Count&&GameObject.Find("Bottle "+(Recommended.Source+1))!=null&&GameObject.Find("Bottle "+(Recommended.Target+1))!=null;
        public bool AllowsBottleTap(int index)
        {
            if(!isActiveAndEnabled)return true;
            if(BlocksGameplayActions||closingModal||autoWelcomePending||State==Phase.Welcome)return false;
            if(State!=Phase.Source&&State!=Phase.Target)return true;
            if(!Ready||!TargetsPresent){Close();return true;}
            return State==Phase.Source?index==Recommended.Source:index==Recommended.Source||index==Recommended.Target;
        }
        public void OpenMenu(){if(!app.GuideCanInteract||State==Phase.Menu)return;manualHelp=true;helpPage=0;Show(Phase.Menu,"Pati Yardımı");actor?.TriggerMagic();}
        public void ShowHowToPlay(){if(!app.GuideCanInteract)return;manualHelp=true;helpPage=0;Show(Phase.General,Rules[0]);}
        public void OpenBoosterHelp(){if(!app.GuideCanInteract)return;ledger.CompleteTutorial(BoosterOfferId);if(BoosterHelpRequested!=null){Close();BoosterHelpRequested.Invoke();return;}manualHelp=true;helpPage=0;Show(Phase.Boosters,BoosterPages[0]);}
        void ShowBoosterOffer(){manualHelp=true;helpPage=0;offerAfterCelebration=false;Show(Phase.BoosterOffer,"Özel güçleri de öğrenmek ister misin?");}
        void Dismiss(){if(State==Phase.BoosterOffer)ledger.CompleteTutorial(BoosterOfferId);Close();}
        public void General(){if(!app.GuideCanInteract)return;manualHelp=false;Show(Phase.General,"Üstteki aynı renkleri bir araya getir. Boş veya üst rengi aynı olan şişeye aktarabilirsin.");}
        public void SuspendHelp(){if(BlocksGameplayActions)Close();}
        public void Selected(int index){if(State==Phase.Source||State==Phase.Target){if(index==Recommended.Source)Show(Phase.Target,TargetText);}}
        public void MoveFinished(int source,int target)
        {
            if(State==Phase.Target&&source==Recommended.Source&&target==Recommended.Target&&model.Moves==recommendedAtMove+1){bool saved=ledger.CompleteTutorial(BasicId);Show(Phase.Celebration,saved?CelebrationText:"Harika! Eğitim kaydedilemedi; daha sonra tekrar deneyebilirsin.");offerAfterCelebration=saved&&!ledger.TutorialCompleted(BoosterOfferId)&&!ledger.TutorialCompleted(BoosterId);actor?.TriggerMagic();closeAt=Time.unscaledTime+1.6f;}
            else if(State==Phase.General||State==Phase.Menu||State==Phase.Welcome)Close();else if(State==Phase.Source||State==Phase.Target){Close();}
        }
        public bool HandleBack(){if(!Visible)return false;Dismiss();return true;}
        public void Close(){autoWelcomePending=offerAfterCelebration=false;closingBoosters=State==Phase.Boosters||closingBoosters;closingModal=BlocksGameplayActions||closingModal;manualHelp=false;if(Visible&&fade!=null){fadeFrom=fade.alpha;fadeStartedAt=Time.unscaledTime;}State=Phase.Closed;if(fade!=null){fade.blocksRaycasts=false;fade.interactable=false;}if(marker!=null)marker.gameObject.SetActive(false);feature=null;}
        public bool TryStartFeature(PawTutorialDefinition definition,bool conditionMet){if(definition==null||string.IsNullOrWhiteSpace(definition.tutorialId)||!conditionMet||Visible||!app.GuideCanInteract||ledger.TutorialCompleted(definition.tutorialId))return false;feature=definition;Show(Phase.Feature,definition.message);return true;}
        public bool CompleteFeature(string completionKey){if(State!=Phase.Feature||feature==null||completionKey!=feature.completionKey)return false;if(!ledger.CompleteTutorial(feature.tutorialId))return false;Close();return true;}
        public void SelectionCleared(){if(State==Phase.Target)Show(Phase.Source,SourceText);}
        public void ResetGuide(){Close();}
        void OnDisable(){Close();closingModal=closingBoosters=false;if(bubble!=null)bubble.gameObject.SetActive(false);if(gameplayStatus!=null)gameplayStatus.enabled=true;if(menuShield!=null)menuShield.gameObject.SetActive(false);if(boosterShield!=null)boosterShield.gameObject.SetActive(false);}
        void LateUpdate()
        {
            if(autoWelcomePending){if(Time.frameCount>mountedFrame&&Ready)Show(Phase.Welcome,WelcomeText);else if(Time.unscaledTime>readyDeadline)Close();}
            if(cat==null){Close();closingModal=closingBoosters=false;menuShield.gameObject.SetActive(false);if(boosterShield!=null)boosterShield.gameObject.SetActive(false);if(bubble!=null)bubble.gameObject.SetActive(false);if(gameplayStatus!=null)gameplayStatus.enabled=true;return;}cat.GetWorldCorners(corners);Vector2 min=safe.InverseTransformPoint(corners[0]),max=safe.InverseTransformPoint(corners[2]);hit.anchorMin=hit.anchorMax=new Vector2(.5f,.5f);hit.anchoredPosition=(min+max)*.5f;hit.sizeDelta=max-min;
            if(Visible&&(Keyboard.current?.escapeKey.wasPressedThisFrame??false))HandleBack();
            if((State==Phase.Source||State==Phase.Target)&&!TargetsPresent)Close();
            float ease=Mathf.SmoothStep(0,1,(Time.unscaledTime-fadeStartedAt)/.18f);fade.alpha=Visible?ease:Mathf.Lerp(fadeFrom,0,ease);bubble.localScale=Vector3.one*Mathf.Lerp(.95f,1,fade.alpha);
            if(!Visible&&Time.unscaledTime-fadeStartedAt>=.18f)closingModal=closingBoosters=false;
            menuShield.gameObject.SetActive(app.GuideCanInteract&&(BlocksGameplayActions||closingModal));
            if(boosterShield!=null)boosterShield.gameObject.SetActive(app.GuideCanInteract&&(State==Phase.Boosters||closingBoosters));
            if(State==Phase.Celebration&&Time.unscaledTime>=closeAt&&!app.IsPaused){if(offerAfterCelebration&&app.GuideCanInteract)ShowBoosterOffer();else Close();}
            if(gameplayStatus!=null)gameplayStatus.enabled=!app.GuideCanInteract||!(Visible||fade.alpha>0);
            if(!app.GuideCanInteract){bubble.gameObject.SetActive(false);marker.gameObject.SetActive(false);return;}bubble.gameObject.SetActive(Visible||fade.alpha>0);
            Transform target=State==Phase.Source?GameObject.Find("Bottle "+(Recommended.Source+1))?.transform:State==Phase.Target?GameObject.Find("Bottle "+(Recommended.Target+1))?.transform:State==Phase.Boosters&&helpPage<BoosterTargets.Length?GameObject.Find(BoosterTargets[helpPage])?.transform:State==Phase.Feature&&!string.IsNullOrWhiteSpace(feature?.targetName)?GameObject.Find(feature.targetName)?.transform:null;
            if(State==Phase.Boosters&&target==null)Close();
            marker.gameObject.SetActive(target!=null);if(target!=null){var view=target.GetComponent<GameplayBottleView>();var r=view!=null?view.Rig:(RectTransform)target;r.GetWorldCorners(corners);min=safe.InverseTransformPoint(corners[0]);max=safe.InverseTransformPoint(corners[2]);marker.anchorMin=marker.anchorMax=new Vector2(.5f,.5f);marker.anchoredPosition=(min+max)*.5f;marker.sizeDelta=max-min+Vector2.one*12;}
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
