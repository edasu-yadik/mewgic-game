using System;using Mewgic.Core;using UnityEngine;using UnityEngine.UI;using UnityEngine.SceneManagement;
namespace Mewgic.Map
{
 public sealed partial class MapWorld
 {
  MapProgression progression;ProgressLedger ledger;Button claimButton;int selectedChest=-1;bool navigating;MapNodeBadge[] badges;Text[] regionProgress;
  public MapProgression Progression=>progression;
  void BindProgress()
  {
   ledger=GameplayProgress.Current;
   try{progression=MapProgressionCatalog.Open(ledger);}catch(Exception e){Message("Harita verisi açılamadı: "+e.Message);return;}
   badges=new MapNodeBadge[points.Count];regionProgress=new Text[4];
   var star=Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");
   for(int i=0;i<points.Count;i++){badges[i]=points[i].gameObject.AddComponent<MapNodeBadge>();badges[i].Build(points[i].kind==MapPointKind.Chest,star,font);}
   for(int i=0;i<4;i++){
    var plate=WorldRoot.Find("RegionLabels/"+RegionNames[i]+"/RegionName_"+i);
    plate.GetComponent<RectTransform>().sizeDelta=new Vector2(0,86);
    var title=plate.Find("Label").GetComponent<RectTransform>();title.anchorMin=new Vector2(.04f,.47f);title.anchorMax=new Vector2(.96f,.86f);plate.Find("Label").GetComponent<Text>().resizeTextMaxSize=23;
    regionProgress[i]=Label(plate,"RegionCompletion","",17,new Vector2(.04f,.15f),new Vector2(.96f,.44f));
   }
   var box=information.transform.Find("InformationPanel");box.Find("Title").GetComponent<Text>().text="HARİTA";
   var msg=informationText.rectTransform;msg.anchorMin=new Vector2(.08f,.48f);msg.anchorMax=new Vector2(.92f,.73f);
   claimButton=Control(box,"ClaimMapChest","ÖDÜLÜ AL",new Vector2(.15f,.30f),new Vector2(.85f,.46f),ClaimSelectedChest);claimButton.gameObject.SetActive(false);
   RefreshProgress();
   if(GameplayProgress.LoadError!=null)Message("Kayıt okunamadı. Özgün kayıt korundu; ödül kaydetme kapalı.");
  }
  public void RefreshProgress()
  {
   if(progression==null)return;
   for(int i=0;i<points.Count;i++){
    var p=points[i];int r=p.regionIndex;
    if(p.kind==MapPointKind.LevelSlot)badges[i].Present(progression.LevelId(r,p.slotIndex),progression.Status(r,p.slotIndex),progression.Stars(progression.LevelId(r,p.slotIndex)));
    else badges[i].PresentChest(progression.ChestReady(r),progression.ChestClaimed(r));
   }
   for(int r=0;r<4;r++){
    int count=progression.Completed(r),total=progression.Total(r),stars=0;for(int n=0;n<3;n++)stars+=progression.Stars(progression.LevelId(r,n));
    regionProgress[r].text=progression.RegionOpen(r)?count+"/"+total+" bölüm • "+stars+"/"+(total*3)+" yıldız"+(progression.RegionComplete(r)?" • TAMAMLANDI":""):"KİLİTLİ • Bölümler hazırlanıyor";
    var tab=tabs.Find("RegionJump_"+r+"/Label").GetComponent<Text>();tab.resizeTextMaxSize=progression.RegionOpen(r)?24:20;tab.text=RegionNames[r].Replace(" ","\n")+(progression.RegionOpen(r)?"":"\nKİLİTLİ");
   }
   UpdateProgressTitle();
  }
  void UpdateProgressTitle(){if(progression==null)return;regionTitle.text=RegionNames[ActiveRegion]+(progression.RegionOpen(ActiveRegion)?"":" • KİLİTLİ");}
  void Message(string text){CancelJump();Scroll.StopMovement();informationText.text=text;information.SetActive(true);information.transform.SetAsLastSibling();if(claimButton!=null)claimButton.gameObject.SetActive(false);}
  public void ShowInformation(MapPoint point)
  {
   if(navigating)return;
   if(progression==null){Message("Harita verisi doğrulanamadı.");return;}
   int r=point.regionIndex;
   if(point.kind==MapPointKind.LevelSlot){int id=progression.LevelId(r,point.slotIndex);var state=progression.Status(r,point.slotIndex);
    if(state==MapLevelStatus.Unavailable){Message("Bu konum henüz hazır değil.\nDoğrulanmış bölüm verisi bulunmuyor.");return;}
    if(state==MapLevelStatus.Locked||!GameplayLevelCatalog.Select(id,ledger)){Message("Bölüm "+id+" kilitli.\nÖnce önceki bölümleri tamamla.");return;}
    navigating=true;Mewgic.SceneNavigation.LoadOnce("Gameplay");return;
   }
   selectedChest=r;
   if(progression.ChestClaimed(r)){Message("Bu sandığın ödülü daha önce alındı.");return;}
   if(!progression.ChestReady(r)){Message(RegionNames[r]+"\nSandık için bölgedeki doğrulanmış bölümlerin tamamı bitmeli. Hazır olmayan bölgeler açılamaz.");return;}
   Message(RegionNames[r]+" tamamlandı!\nSandık ödülü: +"+progression.Reward(r).amount+" İpucu");claimButton.gameObject.SetActive(true);
  }
  void ClaimSelectedChest(){if(selectedChest<0||progression==null)return;bool saved=progression.ClaimChest(selectedChest,out bool granted);RefreshProgress();Message(!saved?"Ödül kaydedilemedi. Hak değişmedi; tekrar deneyebilirsin.":granted?"Sandık açıldı! +1 İpucu kaydedildi.":"Bu sandığın ödülü zaten alındı.");}
 }
 public sealed class MapNodeBadge : MonoBehaviour
 {
  Image art,halo;Text number,arrow;Image[] stars;MapLockGraphic locked;bool chest;float clock;bool current;
  public MapLevelStatus Status {get;private set;}public int Stars {get;private set;}
  static RectTransform Rect(Transform p,string name,Vector2 center,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=center;return r;}
  public void Build(bool isChest,Sprite star,Font font){chest=isChest;art=transform.Find("Art").GetComponent<Image>();
   var h=Rect(transform,"CurrentGlow",Vector2.zero,art.rectTransform.sizeDelta*1.25f);h.SetAsFirstSibling();halo=h.gameObject.AddComponent<Image>();halo.sprite=art.sprite;halo.preserveAspect=true;halo.raycastTarget=false;halo.gameObject.SetActive(false);
   var n=Rect(transform,"LevelNumber",new Vector2(0,1),new Vector2(65,46));number=n.gameObject.AddComponent<Text>();number.font=font;number.fontStyle=FontStyle.Bold;number.fontSize=40;number.alignment=TextAnchor.MiddleCenter;number.color=Color.white;number.raycastTarget=false;var shadow=n.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.03f,.02f,.10f,1);shadow.effectDistance=new Vector2(2,-2);
   var arrowRect=Rect(transform,"CurrentMarker",new Vector2(0,52),new Vector2(40,38));arrow=arrowRect.gameObject.AddComponent<Text>();arrow.font=font;arrow.fontSize=30;arrow.text="▼";arrow.alignment=TextAnchor.MiddleCenter;arrow.color=new Color32(255,220,80,255);arrow.raycastTarget=false;arrow.gameObject.SetActive(false);
   locked=Rect(transform,"Lock",new Vector2(0,3),new Vector2(26,29)).gameObject.AddComponent<MapLockGraphic>();locked.raycastTarget=false;
   stars=new Image[3];for(int i=0;i<3;i++){var im=Rect(transform,"MapStar"+i,new Vector2((i-1)*22,-31),Vector2.one*25).gameObject.AddComponent<Image>();im.sprite=star;im.preserveAspect=true;im.raycastTarget=false;stars[i]=im;}
  }
  public void Present(int id,MapLevelStatus status,int count){Status=status;Stars=count;current=status==MapLevelStatus.Current;bool closed=status==MapLevelStatus.Locked||status==MapLevelStatus.Unavailable;number.text=id>0?id.ToString():"…";number.rectTransform.anchoredPosition=closed?new Vector2(0,-16):new Vector2(0,1);number.fontSize=closed?23:40;locked.gameObject.SetActive(closed);halo.gameObject.SetActive(current);arrow.gameObject.SetActive(current);art.color=closed?new Color(.48f,.32f,.64f,1):status==MapLevelStatus.Completed?new Color(.40f,1,.50f,1):Color.white;
   for(int i=0;i<3;i++){stars[i].gameObject.SetActive(!closed);stars[i].color=i<count?Color.white:new Color(.25f,.22f,.35f,.75f);} }
  public void PresentChest(bool ready,bool claimed){current=ready;halo.gameObject.SetActive(ready);locked.gameObject.SetActive(!ready&&!claimed);number.text=claimed?"✓":"";number.fontSize=34;number.rectTransform.anchoredPosition=new Vector2(0,-30);art.color=claimed?new Color(.65f,.65f,.75f,1):ready?Color.white:new Color(.65f,.56f,.80f,1);foreach(var s in stars)s.gameObject.SetActive(false);}
  void Update(){if(!current)return;clock+=Time.unscaledDeltaTime;halo.color=new Color(1,.84f,.28f,.25f+Mathf.Sin(clock*2)*.12f);halo.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(clock*2)*.04f);}
 }
 [RequireComponent(typeof(CanvasRenderer))]public sealed class MapLockGraphic:MaskableGraphic
 {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;var gold=new Color32(255,224,142,255);Box(v,new Rect(-w*.4f,-h*.45f,w*.8f,h*.58f),gold);for(int n=0;n<16;n++){float a=n*Mathf.PI/16,b=(n+1)*Mathf.PI/16;Vector2 p=new Vector2(Mathf.Cos(a)*w*.27f,Mathf.Sin(a)*h*.33f+h*.10f),q=new Vector2(Mathf.Cos(b)*w*.27f,Mathf.Sin(b)*h*.33f+h*.10f);int i=v.currentVertCount;var d=(q-p).normalized;var normal=new Vector2(-d.y,d.x)*w*.08f;v.AddVert(p-normal,gold,Vector2.zero);v.AddVert(q-normal,gold,Vector2.zero);v.AddVert(q+normal,gold,Vector2.zero);v.AddVert(p+normal,gold,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}Box(v,new Rect(-w*.055f,-h*.28f,w*.11f,h*.20f),new Color32(62,35,93,255));}
  static void Box(VertexHelper v,Rect r,Color c){int i=v.currentVertCount;v.AddVert(new Vector2(r.xMin,r.yMin),c,Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMin),c,Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMax),c,Vector2.zero);v.AddVert(new Vector2(r.xMin,r.yMax),c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
 }
}
