using L=Mewgic.Localization.L;
using TMPro;using Mewgic.UI;using System;using Mewgic.Core;using UnityEngine;using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed partial class MapWorld
 {
  MapProgression progression;ProgressLedger ledger;Button claimButton;int selectedChest=-1;bool navigating;
  public MapProgression Progression=>progression;
  void BindProgress(){ledger=GameplayProgress.Current;try{progression=MapProgressionCatalog.Open(ledger);}catch(Exception){Message(L.V("boot.error"));return;}if(GameplayProgress.LoadError!=null)Message(L.V("error.load"));}
  public void RefreshProgress(){if(ledger==null)return;progression=MapProgressionCatalog.Open(ledger);foreach(var v in views)if(v.region>=0)BindIsland(v,v.region);UpdateProgressTitle();}
  void UpdateProgressTitle(){if(progression==null)return;regionTitle.text=progression.RegionOpen(ActiveRegion)?L.Island(ActiveRegion):L.V("map.region_locked",L.Island(ActiveRegion));}
  void Message(string text){CancelJump();Scroll.StopMovement();informationText.text=text;information.SetActive(true);information.transform.SetAsLastSibling();claimButton.gameObject.SetActive(false);}
  public void ShowInformation(MapPoint point){if(navigating)return;if(progression==null){Message(L.V("boot.error"));return;}int r=point.regionIndex;if(point.kind==MapPointKind.LevelGroup){OpenChapter(r,point.slotIndex);return;}if(point.kind==MapPointKind.LevelSlot){int id=progression.LevelId(r,point.slotIndex);if(!progression.CanEnter(id)){Message(L.V("map.level_locked",id));return;}EnterLevel(id);return;}selectedChest=r;if(progression.ChestClaimed(r)){Message(L.V("map.chest_taken"));return;}if(!progression.ChestReady(r)){Message(L.V("map.chest_wait",L.Island(r)));return;}Message(L.V("map.chest_ready",progression.Reward(r).amount));claimButton.gameObject.SetActive(true);}
  public async void EnterLevel(int id){if(navigating||progression==null||!progression.CanEnter(id))return;navigating=true;Message(L.V("map.preparing"));try{await GameplayLevelCatalog.PrepareAsync(id);if(this==null)return;if(!GameplayLevelCatalog.Select(id,ledger))throw new InvalidOperationException();pendingFocus=-1;ledger.SaveMapFocus(id);SceneNavigation.LoadOnce("Gameplay");}catch(Exception){if(this!=null){navigating=false;Message(L.V("error.prepare"));}}}
  void ClaimSelectedChest(){if(selectedChest<0||progression==null)return;bool saved=progression.ClaimChest(selectedChest,out bool granted);RefreshProgress();Message(!saved?L.V("error.save"):granted?L.V("map.chest_saved"):L.V("map.chest_taken"));}
 }
 public sealed class MapNodeBadge : MonoBehaviour
 {
  Image art,halo;TMP_Text number,arrow;Image[] stars;MapLockGraphic locked;MapClaimGraphic claimedMark;bool chest;float clock;bool current;
  public MapLevelStatus Status {get;private set;}public int Stars {get;private set;}
  static RectTransform Rect(Transform p,string name,Vector2 center,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=center;return r;}
  public void Build(bool isChest,Sprite star,TMP_FontAsset font){chest=isChest;art=transform.Find("Art").GetComponent<Image>();
   var h=Rect(transform,"CurrentGlow",Vector2.zero,art.rectTransform.sizeDelta*1.25f);h.SetAsFirstSibling();halo=h.gameObject.AddComponent<Image>();halo.sprite=art.sprite;halo.preserveAspect=true;halo.raycastTarget=false;halo.gameObject.SetActive(false);
   var n=Rect(transform,"LevelNumber",new Vector2(0,1),new Vector2(65,46));number=n.gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();number.font=font;MewgicTypographySettings.Prepare(number);number.fontStyle=FontStyles.Bold;number.fontSize=40;number.alignment=TextAlignmentOptions.Midline;number.textWrappingMode=TextWrappingModes.NoWrap;number.enableAutoSizing=true;number.fontSizeMin=18;number.fontSizeMax=36;number.color=Color.white;number.raycastTarget=false;
   var arrowRect=Rect(transform,"CurrentMarker",new Vector2(0,52),new Vector2(40,38));arrow=arrowRect.gameObject.AddComponent<Mewgic.Localization.LocalizedTMP>();arrow.font=font;MewgicTypographySettings.Prepare(arrow);arrow.fontSize=22;arrow.textWrappingMode=TextWrappingModes.NoWrap;arrow.text="▼";arrow.alignment=TextAlignmentOptions.Midline;arrow.color=new Color32(255,220,80,255);arrow.raycastTarget=false;arrow.gameObject.SetActive(false);
   locked=Rect(transform,"Lock",new Vector2(0,3),new Vector2(26,29)).gameObject.AddComponent<MapLockGraphic>();locked.raycastTarget=false;
   claimedMark=Rect(transform,"ClaimedCheck",new Vector2(0,-28),new Vector2(30,24)).gameObject.AddComponent<MapClaimGraphic>();claimedMark.raycastTarget=false;claimedMark.gameObject.SetActive(false);
   stars=new Image[3];for(int i=0;i<3;i++){var im=Rect(transform,"MapStar"+i,new Vector2((i-1)*22,-31),Vector2.one*25).gameObject.AddComponent<Image>();im.sprite=star;im.preserveAspect=true;im.raycastTarget=false;stars[i]=im;}
  }
  public void Present(int id,MapLevelStatus status,int count){Status=status;Stars=count;current=status==MapLevelStatus.Current;bool closed=status==MapLevelStatus.Locked||status==MapLevelStatus.Unavailable;number.text=id>0?id.ToString():"…";number.rectTransform.anchoredPosition=closed?new Vector2(0,-16):new Vector2(0,1);number.fontSize=closed?23:40;locked.gameObject.SetActive(closed);halo.gameObject.SetActive(current);arrow.gameObject.SetActive(current);art.color=closed?new Color(.48f,.32f,.64f,1):status==MapLevelStatus.Completed?new Color(.40f,1,.50f,1):Color.white;
   for(int i=0;i<3;i++){stars[i].gameObject.SetActive(!closed);stars[i].color=i<count?Color.white:new Color(.25f,.22f,.35f,.75f);} }
  public void PresentGroup(int first,int last,int done,bool isCurrent,bool open){Present(first,isCurrent?MapLevelStatus.Current:done==10?MapLevelStatus.Completed:open?MapLevelStatus.Available:MapLevelStatus.Locked,0);number.rectTransform.sizeDelta=new Vector2(90,48);number.text=first+"–"+last;number.fontSizeMin=14;number.fontSizeMax=21;number.rectTransform.anchoredPosition=new Vector2(0,open?0:-22);foreach(var s in stars)s.gameObject.SetActive(false);}
  public void PresentChest(bool ready,bool claimed){current=ready;halo.gameObject.SetActive(ready);locked.gameObject.SetActive(!ready&&!claimed);claimedMark.gameObject.SetActive(claimed);number.text="";number.fontSize=34;number.rectTransform.anchoredPosition=new Vector2(0,-30);art.color=claimed?new Color(.65f,.65f,.75f,1):ready?Color.white:new Color(.65f,.56f,.80f,1);foreach(var s in stars)s.gameObject.SetActive(false);}
  void Update(){if(!current)return;clock+=Time.unscaledDeltaTime;halo.color=new Color(1,.84f,.28f,.25f+Mathf.Sin(clock*2)*.12f);halo.rectTransform.localScale=Vector3.one*(1+Mathf.Sin(clock*2)*.04f);}
 }
 [RequireComponent(typeof(CanvasRenderer))]public sealed class MapLockGraphic:MaskableGraphic
 {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;var gold=new Color32(255,224,142,255);Box(v,new Rect(-w*.4f,-h*.45f,w*.8f,h*.58f),gold);for(int n=0;n<16;n++){float a=n*Mathf.PI/16,b=(n+1)*Mathf.PI/16;Vector2 p=new Vector2(Mathf.Cos(a)*w*.27f,Mathf.Sin(a)*h*.33f+h*.10f),q=new Vector2(Mathf.Cos(b)*w*.27f,Mathf.Sin(b)*h*.33f+h*.10f);int i=v.currentVertCount;var d=(q-p).normalized;var normal=new Vector2(-d.y,d.x)*w*.08f;v.AddVert(p-normal,gold,Vector2.zero);v.AddVert(q-normal,gold,Vector2.zero);v.AddVert(q+normal,gold,Vector2.zero);v.AddVert(p+normal,gold,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}Box(v,new Rect(-w*.055f,-h*.28f,w*.11f,h*.20f),new Color32(62,35,93,255));}
  static void Box(VertexHelper v,Rect r,Color c){int i=v.currentVertCount;v.AddVert(new Vector2(r.xMin,r.yMin),c,Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMin),c,Vector2.zero);v.AddVert(new Vector2(r.xMax,r.yMax),c,Vector2.zero);v.AddVert(new Vector2(r.xMin,r.yMax),c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
 }
 [RequireComponent(typeof(CanvasRenderer))]public sealed class MapClaimGraphic:MaskableGraphic
 {
  protected override void OnPopulateMesh(VertexHelper v){v.Clear();var size=rectTransform.rect.size;Line(v,new Vector2(-.42f,-.02f)*size,new Vector2(-.12f,-.32f)*size);Line(v,new Vector2(-.12f,-.32f)*size,new Vector2(.44f,.36f)*size);}
  static void Line(VertexHelper v,Vector2 a,Vector2 b){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*2.5f;int i=v.currentVertCount;var c=new Color32(255,225,115,255);v.AddVert(a-n,c,Vector2.zero);v.AddVert(b-n,c,Vector2.zero);v.AddVert(b+n,c,Vector2.zero);v.AddVert(a+n,c,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
 }

}
