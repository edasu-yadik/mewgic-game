using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    public sealed class GameplayHud : MonoBehaviour
    {
        TMP_Text level,goal,moves;GameObject overlay;RectTransform safe,root;Image specialGoal;
        Sprite panel,pauseArt;TMP_FontAsset font;Action resume,restart,home;
        public GameplayHudState State {get;private set;}
        public bool IsPaused=>overlay!=null&&overlay.activeSelf;
        static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        Image Art(Transform parent,string name,Sprite sprite,Vector2 min,Vector2 max,bool sliced=false)
        {var im=Rect(parent,name,min,max).gameObject.AddComponent<Image>();im.sprite=sprite;im.type=sliced?Image.Type.Sliced:Image.Type.Simple;im.raycastTarget=false;return im;}
        TMP_Text TMP_Text(Transform parent,string name,string value,int size,Vector2 min,Vector2 max)
        {var t=Rect(parent,name,min,max).gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();t.font=font;t.text=value;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=17;t.fontSizeMax=size;t.alignment=TextAlignmentOptions.Midline;t.color=new Color32(255,248,224,255);t.raycastTarget=false;MewgicTypographySettings.Prepare(t);return t;}
        Button Control(Transform parent,string name,string value,Vector2 min,Vector2 max,Action action)
        {var im=Art(parent,name,panel,min,max,true);im.raycastTarget=true;var b=im.gameObject.AddComponent<PremiumButton>();b.targetGraphic=im;b.onClick.AddListener(()=>action());TMP_Text(im.transform,"Label",value,32,new Vector2(.08f,.12f),new Vector2(.92f,.88f));return b;}
        static Sprite Load(string name)=>Resources.Load<Sprite>("Gameplay/HUD/HUD_"+name+"_v001");
        public void Mount(RectTransform safeArea,Action pause,Action onResume,Action onRestart,Action onHome)
        {
            safe=safeArea;resume=onResume;restart=onRestart;home=onHome;font=MewgicTypographySettings.Current.body;panel=Load("Panel");pauseArt=Load("Pause");
            root=Rect(safe,"GameplayHUD",new Vector2(0,1),Vector2.one);root.pivot=new Vector2(.5f,1);
            var l=Art(root,"LevelPanel",panel,new Vector2(.025f,.15f),new Vector2(.29f,.90f),true);
            var crystal=new GameObject("HudCrystal",typeof(RectTransform),typeof(CanvasRenderer),typeof(HudCrystalAccent));var cr=(RectTransform)crystal.transform;cr.SetParent(l.transform,false);cr.anchorMin=cr.anchorMax=new Vector2(.5f,1);cr.sizeDelta=new Vector2(12,16);cr.anchoredPosition=new Vector2(0,-1);crystal.GetComponent<HudCrystalAccent>().raycastTarget=false;
            level=TMP_Text(l.transform,"LevelValue","",30,new Vector2(.07f,.12f),new Vector2(.93f,.88f));level.font=MewgicTypographySettings.Current.heading;
            var m=Art(root,"MovesPanel",panel,new Vector2(.60f,.15f),new Vector2(.845f,.90f),true);
            moves=TMP_Text(m.transform,"Moves","",28,new Vector2(.07f,.12f),new Vector2(.93f,.88f));
            var p=Art(root,"Pause",pauseArt,new Vector2(.875f,.15f),new Vector2(.975f,.90f));p.preserveAspect=true;p.raycastTarget=true;var pb=p.gameObject.AddComponent<PremiumButton>();pb.targetGraphic=p;pb.onClick.AddListener(()=>pause());
            specialGoal=Art(safe,"SpecialGoal",panel,new Vector2(.18f,.86f),new Vector2(.82f,.91f),true);goal=TMP_Text(specialGoal.transform,"SpecialGoalValue","",22,new Vector2(.06f,.1f),new Vector2(.94f,.9f));specialGoal.gameObject.SetActive(false);Layout();
            var cover=Rect(safe.parent,"GameplayPauseOverlay",Vector2.zero,Vector2.one);overlay=cover.gameObject;var dim=overlay.AddComponent<Image>();dim.color=new Color(.025f,.018f,.065f,.84f);dim.raycastTarget=true;
            var pauseSafe=Rect(cover,"PauseSafeArea",safe.anchorMin,safe.anchorMax);
            var modal=Art(pauseSafe,"PausePanel",panel,new Vector2(.10f,.30f),new Vector2(.90f,.72f),true);
            TMP_Text(modal.transform,"PauseTitle",L.V("gameplay.paused"),40,new Vector2(.06f,.78f),new Vector2(.94f,.94f));
            Control(modal.transform,"Resume",L.V("gameplay.resume"),new Vector2(.12f,.55f),new Vector2(.88f,.74f),resume);
            Control(modal.transform,"PauseRestart",L.V("gameplay.restart_full"),new Vector2(.12f,.32f),new Vector2(.88f,.51f),restart);
            Control(modal.transform,"PauseHome",GameplayLevelCatalog.FromMap&&GetComponent<PrototypeApp>()?.IsTutorialReplay!=true?L.V("common.map"):L.V("common.home"),new Vector2(.12f,.09f),new Vector2(.88f,.28f),home);
            overlay.SetActive(false);
        }
        public void Present(GameplayHudState state)
        {
            State=state;level.text=L.V("gameplay.level",state.Level);moves.text=L.V("gameplay.moves",state.Moves);
            bool different=!string.Equals(state.Goal?.Trim(),"Renkleri ayır",StringComparison.OrdinalIgnoreCase)&&!string.Equals(state.Goal?.Trim(),"Üç rengi ayır",StringComparison.OrdinalIgnoreCase);
            specialGoal.gameObject.SetActive(different);goal.text=different?state.Goal:"";
            GetComponent<GameplayPresentation>()?.SetTopLimit(different?.84f:.90f);

        }
        public void ShowPaused(bool value){overlay.SetActive(value);if(value)overlay.transform.SetAsLastSibling();}
        public void Layout(){if(root==null)return;root.sizeDelta=new Vector2(0,Mathf.Clamp(safe.rect.width*.105f,76,100));root.anchoredPosition=new Vector2(0,-12);}
        void LateUpdate(){Layout();if(overlay!=null){var r=(RectTransform)overlay.transform.Find("PauseSafeArea");r.anchorMin=safe.anchorMin;r.anchorMax=safe.anchorMax;}}
    }
    [RequireComponent(typeof(CanvasRenderer))]public sealed class HudCrystalAccent:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper v){v.Clear();var r=rectTransform.rect;v.AddVert(new Vector2(r.xMin,r.center.y),new Color32(112,38,190,255),Vector2.zero);v.AddVert(new Vector2(r.center.x,r.yMin),new Color32(81,20,143,255),Vector2.zero);v.AddVert(new Vector2(r.xMax,r.center.y),new Color32(202,118,255,255),Vector2.zero);v.AddVert(new Vector2(r.center.x,r.yMax),new Color32(255,229,255,255),Vector2.zero);v.AddVert(r.center,new Color32(185,89,249,255),Vector2.zero);for(int i=0;i<4;i++)v.AddTriangle(4,i,(i+1)%4);}
    }
}
