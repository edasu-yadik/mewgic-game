using System;using UnityEngine;using UnityEngine.UI;using UnityEngine.InputSystem;using TMPro;using Mewgic.Core;
namespace Mewgic.UI
{
    public sealed class PawGuide:MonoBehaviour
    {
        public const string BasicId="paw.basic.v1";
        public enum Phase{Closed,Welcome,Source,Target,Celebration,Menu,General,Feature}
        public Phase State{get;private set;}
        public PuzzleMove Recommended{get;private set;}
        PrototypeApp app;Puzzle model;ProgressLedger ledger;RectTransform safe,bubble,hit,marker;CanvasGroup fade;TMP_Text message;Button next,how,hint,close;
        readonly Vector3[] corners=new Vector3[4];
        RectTransform cat;Mewgic.Characters.WizardCatPresentation actor;float closeAt;PawTutorialDefinition feature;
        public bool Visible=>State!=Phase.Closed;
        public void Mount(PrototypeApp host,RectTransform area,Puzzle puzzle,ProgressLedger progress,int level)
        {
            app=host;safe=area;model=puzzle;ledger=progress;cat=GameObject.Find("GameplayWizardCat")?.GetComponent<RectTransform>();actor=cat!=null?cat.GetComponent<Mewgic.Characters.WizardCatPresentation>():null;
            var root=(RectTransform)GameObject.Find("TutorialOverlay").transform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;
            hit=Rect(root,"PawCatTouch",Vector2.zero,Vector2.zero);var image=hit.gameObject.AddComponent<Image>();image.color=Color.clear;var button=hit.gameObject.AddComponent<Button>();button.onClick.AddListener(OpenMenu);
            bubble=Rect(root,"PawBubble",new Vector2(.025f,.201f),new Vector2(.825f,.286f));var art=bubble.gameObject.AddComponent<Image>();art.sprite=Resources.Load<Mewgic.Daily.DailyArt>("Daily/Art").parchment;art.type=Image.Type.Sliced;art.pixelsPerUnitMultiplier=2;art.raycastTarget=false;
            fade=bubble.gameObject.AddComponent<CanvasGroup>();message=Label(bubble,"PawMessage",new Vector2(.055f,.40f),new Vector2(.945f,.82f));
            next=Control("PawNext","DEVAM",.30f,.70f,Continue);how=Control("PawHow","Nasıl oynanır?",.035f,.37f,()=>General());hint=Control("PawHint","Hamle öner",.38f,.70f,()=>{if(app.GuideCanInteract){Close();app.UseBooster(BoosterKind.Hint);}});close=Control("PawClose","Kapat",.72f,.965f,Close);
            marker=Rect(root,"PawHighlight",Vector2.zero,Vector2.zero);marker.gameObject.AddComponent<PawHighlight>().raycastTarget=false;
            Close();if(level==1&&!ledger.Completed(1)&&!ledger.TutorialCompleted(BasicId)){Show(Phase.Welcome,"Merhaba! Ben senin büyücü yardımcınım. Renkleri kendi şişelerinde toplamaya hazır mısın?");}
        }
        static RectTransform Rect(Transform p,string n,Vector2 a,Vector2 b){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=a;r.anchorMax=b;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        TMP_Text Label(Transform p,string n,Vector2 a,Vector2 b){var t=Rect(p,n,a,b).gameObject.AddComponent<TextMeshProUGUI>();MewgicTypographySettings.Prepare(t);t.fontSize=23;t.enableAutoSizing=true;t.fontSizeMin=18;t.fontSizeMax=23;t.alignment=TextAlignmentOptions.Midline;t.color=new Color32(60,30,24,255);return t;}
        Button Control(string n,string value,float left,float right,UnityEngine.Events.UnityAction action){var r=Rect(bubble,n,new Vector2(left,.04f),new Vector2(right,.38f));var im=r.gameObject.AddComponent<Image>();im.sprite=Resources.Load<Mewgic.Daily.DailyArt>("Daily/Art").button;var b=r.gameObject.AddComponent<Button>();b.onClick.AddListener(action);var t=Label(r,n+"Label",Vector2.zero,Vector2.one);t.text=value;t.color=new Color32(255,229,169,255);t.fontSizeMin=14;t.fontSizeMax=20;return b;}
        void Show(Phase phase,string text){State=phase;message.text=text;bubble.gameObject.SetActive(true);fade.alpha=0;bubble.localScale=Vector3.one*.95f;next.gameObject.SetActive(phase==Phase.Welcome);how.gameObject.SetActive(phase==Phase.Menu);hint.gameObject.SetActive(phase==Phase.Menu);close.gameObject.SetActive(phase!=Phase.Welcome);}
        public void Continue(){if(!app.GuideCanInteract)return;if(TryMove()){Show(Phase.Source,"Önce bu şişeye dokun!");}else General();}
        bool TryMove(){var solution=PuzzleSolver.Solve(model.Export(),3000);if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0)return false;Recommended=solution.Moves[0];return true;}
        public void OpenMenu(){if(!app.GuideCanInteract)return;Show(Phase.Menu,"Pati Yardımı");actor?.TriggerMagic();}
        public void General(){if(!app.GuideCanInteract)return;Show(Phase.General,"Üstteki aynı renkleri bir araya getir. Boş veya üst rengi aynı olan şişeye aktarabilirsin.");}
        public void Selected(int index){if(State==Phase.Source||State==Phase.Target){if(index==Recommended.Source)Show(Phase.Target,"Harika! Şimdi aynı renkli sıvıyı bu şişeye aktar.");else if(TryMove())Show(Phase.Source,"Önce bu şişeye dokun!");else General();}}
        public void MoveFinished(int source,int target)
        {
            if((State==Phase.Source||State==Phase.Target)&&source==Recommended.Source&&target==Recommended.Target){bool saved=ledger.CompleteTutorial(BasicId);Show(Phase.Celebration,saved?"Harikasın! Artık sıra sende.":"Harikasın! Öğretici kaydedilemedi; tekrar deneyebilirsin.");actor?.TriggerMagic();closeAt=Time.unscaledTime+1.4f;}
            else if(State==Phase.General||State==Phase.Menu||State==Phase.Welcome)Close();else if(State==Phase.Source||State==Phase.Target){if(TryMove())Show(Phase.Source,"Önce bu şişeye dokun!");else General();}
        }
        public bool HandleBack(){if(!Visible)return false;Close();return true;}
        public void Close(){State=Phase.Closed;if(bubble!=null)bubble.gameObject.SetActive(false);if(marker!=null)marker.gameObject.SetActive(false);feature=null;}
        public bool TryStartFeature(PawTutorialDefinition definition,bool conditionMet){if(definition==null||string.IsNullOrWhiteSpace(definition.tutorialId)||!conditionMet||Visible||!app.GuideCanInteract||ledger.TutorialCompleted(definition.tutorialId))return false;feature=definition;Show(Phase.Feature,definition.message);return true;}
        public bool CompleteFeature(string completionKey){if(State!=Phase.Feature||feature==null||completionKey!=feature.completionKey)return false;if(!ledger.CompleteTutorial(feature.tutorialId))return false;Close();return true;}
        public void SelectionCleared(){if(State==Phase.Target)Show(Phase.Source,"Önce bu şişeye dokun!");}
        public void ResetGuide(){Close();}
        void LateUpdate()
        {
            if(cat==null)return;cat.GetWorldCorners(corners);Vector2 min=safe.InverseTransformPoint(corners[0]),max=safe.InverseTransformPoint(corners[2]);hit.anchorMin=hit.anchorMax=new Vector2(.5f,.5f);hit.anchoredPosition=(min+max)*.5f;hit.sizeDelta=max-min;
            if(Visible&&(Keyboard.current?.escapeKey.wasPressedThisFrame??false))HandleBack();
            if(Visible){fade.alpha=Mathf.MoveTowards(fade.alpha,1,Time.unscaledDeltaTime*6);bubble.localScale=Vector3.one*Mathf.Lerp(.95f,1,fade.alpha);if(State==Phase.Celebration&&Time.unscaledTime>=closeAt)Close();}
            if(!app.GuideCanInteract){bubble.gameObject.SetActive(false);marker.gameObject.SetActive(false);return;}bubble.gameObject.SetActive(Visible);
            Transform target=State==Phase.Source?GameObject.Find("Bottle "+(Recommended.Source+1))?.transform:State==Phase.Target?GameObject.Find("Bottle "+(Recommended.Target+1))?.transform:State==Phase.Feature&&!string.IsNullOrWhiteSpace(feature?.targetName)?GameObject.Find(feature.targetName)?.transform:null;
            marker.gameObject.SetActive(target!=null);if(target!=null){var r=(RectTransform)target;r.GetWorldCorners(corners);min=safe.InverseTransformPoint(corners[0]);max=safe.InverseTransformPoint(corners[2]);marker.anchorMin=marker.anchorMax=new Vector2(.5f,.5f);marker.anchoredPosition=(min+max)*.5f;marker.sizeDelta=max-min+Vector2.one*12;}
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]public sealed class PawHighlight:MaskableGraphic
    {
        void Quad(VertexHelper v,Rect r){int k=v.currentVertCount;var c=new Color(1,.77f,.19f,.55f+Mathf.Sin(Time.unscaledTime*3)*.15f);v.AddVert(new Vector2(r.xMin,r.yMin),c,Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMin),c,Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMax),c,Vector2.zero);v.AddVert(new Vector2(r.xMin,r.yMax),c,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
        void Circle(VertexHelper v,Vector2 p,float radius){int k=v.currentVertCount;v.AddVert(p,new Color(1,.82f,.3f,.9f),Vector2.zero);for(int i=0;i<=16;i++){float a=i*Mathf.PI/8;v.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Color(1,.82f,.3f,.9f),Vector2.zero);if(i>0)v.AddTriangle(k,k+i,k+i+1);}}
        protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;Quad(v,new Rect(r.x,r.y,r.width,3));Quad(v,new Rect(r.x,r.yMax-3,r.width,3));Quad(v,new Rect(r.x,r.y,3,r.height));Quad(v,new Rect(r.xMax-3,r.y,3,r.height));var paw=new Vector2(r.center.x,r.yMax+12+Mathf.Sin(Time.unscaledTime*3)*3);Circle(v,paw,5);for(int i=0;i<4;i++)Circle(v,paw+new Vector2(-8+i*5,7+Mathf.Sin(i)*2),2.5f);}
        void Update(){SetVerticesDirty();}
    }
}
