using System;
using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    public sealed class GameplayHud : MonoBehaviour
    {
        Text level,goal,moves,performance;Image[] stars;GameObject overlay;RectTransform safe;
        Sprite panel,star,pauseArt;Font font;Action resume,restart,home;
        public GameplayHudState State {get;private set;}
        public bool IsPaused=>overlay!=null&&overlay.activeSelf;
        static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        Image Art(Transform parent,string name,Sprite sprite,Vector2 min,Vector2 max,bool sliced=false)
        {var im=Rect(parent,name,min,max).gameObject.AddComponent<Image>();im.sprite=sprite;im.type=sliced?Image.Type.Sliced:Image.Type.Simple;im.raycastTarget=false;return im;}
        Text Text(Transform parent,string name,string value,int size,Vector2 min,Vector2 max)
        {var t=Rect(parent,name,min,max).gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.resizeTextForBestFit=true;t.resizeTextMinSize=17;t.resizeTextMaxSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=new Color32(255,248,224,255);t.raycastTarget=false;var shadow=t.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,0,0,.8f);shadow.effectDistance=new Vector2(1,-2);return t;}
        Button Control(Transform parent,string name,string value,Vector2 min,Vector2 max,Action action)
        {var im=Art(parent,name,panel,min,max,true);im.raycastTarget=true;var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>action());Text(im.transform,"Label",value,32,new Vector2(.08f,.12f),new Vector2(.92f,.88f));return b;}
        static Sprite Load(string name)=>Resources.Load<Sprite>("Gameplay/HUD/HUD_"+name+"_v001");
        public void Mount(RectTransform safeArea,Action pause,Action onResume,Action onRestart,Action onHome)
        {
            safe=safeArea;resume=onResume;restart=onRestart;home=onHome;font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");panel=Load("Panel");star=Load("Star");pauseArt=Load("Pause");
            var root=Rect(safe,"GameplayHUD",new Vector2(0,.745f),Vector2.one);
            var l=Art(root,"LevelPanel",panel,new Vector2(.025f,.48f),new Vector2(.275f,.84f),true);
            level=Text(l.transform,"LevelValue","",30,new Vector2(.07f,.12f),new Vector2(.93f,.88f));
            var g=Art(root,"GoalPanel",panel,new Vector2(.29f,.33f),new Vector2(.72f,.84f),true);
            Text(g.transform,"GoalTitle","BÖLÜM HEDEFİ",25,new Vector2(.06f,.63f),new Vector2(.94f,.91f));
            goal=Text(g.transform,"GoalValue","",25,new Vector2(.06f,.11f),new Vector2(.94f,.64f));
            var m=Art(root,"MovesPanel",panel,new Vector2(.735f,.33f),new Vector2(.975f,.73f),true);
            Text(m.transform,"MovesTitle","KULLANILAN",21,new Vector2(.07f,.65f),new Vector2(.93f,.90f));
            moves=Text(m.transform,"Moves","",29,new Vector2(.07f,.10f),new Vector2(.93f,.67f));
            var p=Art(root,"Pause",pauseArt,new Vector2(.872f,.77f),new Vector2(.975f,.98f));p.preserveAspect=true;p.raycastTarget=true;var pb=p.gameObject.AddComponent<Button>();pb.targetGraphic=p;pb.onClick.AddListener(()=>pause());
            var row=Art(root,"PerformancePanel",panel,new Vector2(.19f,.035f),new Vector2(.81f,.28f),true);
            stars=new Image[3];for(int i=0;i<3;i++){stars[i]=Art(row.transform,"PerformanceStar"+(i+1),star,new Vector2(.26f+i*.18f,.38f),new Vector2(.38f+i*.18f,.96f));stars[i].preserveAspect=true;}
            performance=Text(row.transform,"PerformanceValue","",20,new Vector2(.08f,.08f),new Vector2(.92f,.32f));
            var cover=Rect(safe.parent,"GameplayPauseOverlay",Vector2.zero,Vector2.one);overlay=cover.gameObject;var dim=overlay.AddComponent<Image>();dim.color=new Color(.025f,.018f,.065f,.84f);dim.raycastTarget=true;
            var pauseSafe=Rect(cover,"PauseSafeArea",safe.anchorMin,safe.anchorMax);
            var modal=Art(pauseSafe,"PausePanel",panel,new Vector2(.10f,.30f),new Vector2(.90f,.72f),true);
            Text(modal.transform,"PauseTitle","DURAKLATILDI",40,new Vector2(.06f,.78f),new Vector2(.94f,.94f));
            Control(modal.transform,"Resume","DEVAM ET",new Vector2(.12f,.55f),new Vector2(.88f,.74f),resume);
            Control(modal.transform,"PauseRestart","YENİDEN BAŞLAT",new Vector2(.12f,.32f),new Vector2(.88f,.51f),restart);
            Control(modal.transform,"PauseHome",GameplayLevelCatalog.FromMap?"HARİTA":"ANA MENÜ",new Vector2(.12f,.09f),new Vector2(.88f,.28f),home);
            overlay.SetActive(false);
        }
        public void Present(GameplayHudState state)
        {
            State=state;level.text="BÖLÜM "+state.Level;goal.text=state.Goal+"\n"+state.Completed+" / "+state.Total+" renk tamamlandı";
            moves.text="HAMLE  "+state.Moves;
            for(int i=0;i<stars.Length;i++)stars[i].color=i<state.PotentialStars?Color.white:new Color(.38f,.32f,.48f,.8f);
            performance.text=state.AwardedStars>0?state.AwardedStars+" YILDIZ KAZANILDI":"HEDEF: "+state.PotentialStars+" YILDIZ";
        }
        public void ShowPaused(bool value){overlay.SetActive(value);if(value)overlay.transform.SetAsLastSibling();}
        void LateUpdate(){if(overlay!=null){var r=(RectTransform)overlay.transform.Find("PauseSafeArea");r.anchorMin=safe.anchorMin;r.anchorMax=safe.anchorMax;}}
    }
}
