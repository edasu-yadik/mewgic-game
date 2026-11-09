using Mewgic.UI;
using System.Collections;using Mewgic.Core;using UnityEngine;using UnityEngine.UI;using TMPro;using L=Mewgic.Localization.L;
namespace Mewgic.Daily
{
 public sealed class DailyArrivalPopup:MonoBehaviour
 {
  static long offeredForGift=long.MinValue;RectTransform safeArea;public System.Action OnGranted;GameObject root;RectTransform chest;TMP_Text result;Button open,close,dismiss;bool opening;Sprite openArt, closedArt;RewardMotion motion;GiftChestSlice lid,body;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession()=>offeredForGift=long.MinValue;
  public void Mount(RectTransform safe,bool manual=false){safeArea=safe;if(root!=null&&root.activeSelf)return;if(!GameplayProgress.Current.GiftReady)return;long receipt=GameplayProgress.Current.LastGiftUtc;if(!manual&&offeredForGift==receipt)return;offeredForGift=receipt;if(root!=null)Destroy(root);
   var art=Resources.Load<DailyArt>("Daily/Art");openArt=art.chest;closedArt=Resources.Load<Mewgic.Rewards.RewardHubArt>("Rewards/Art").closedChest;var panel=Resources.Load<Sprite>("Gameplay/HUD/HUD_Panel_v001");
   var dim=GameplayTools.Art(safe,"DailyArrival",null,Vector2.zero,Vector2.one);dim.color=new Color(0,0,.08f,.88f);dim.raycastTarget=true;root=dim.gameObject;
   var card=GameplayTools.Art(root.transform,"DailyArrivalCard",panel,new Vector2(.07f,.24f),new Vector2(.93f,.79f),true);
   GameplayTools.Label(card.transform,"DailyArrivalTitle",L.V("daily.gift"),38,new Vector2(.06f,.82f),new Vector2(.94f,.96f));
   var picture=GameplayTools.Art(card.transform,"ArrivalChest",closedArt,new Vector2(.18f,.30f),new Vector2(.82f,.81f));picture.preserveAspect=true;chest=picture.rectTransform;
   lid=ChestSlice("ArrivalChestLid",true);
   body=ChestSlice("ArrivalChestBody",false);
   var top=FindFirstObjectByType<HomeTopBar>();
   motion=new RewardMotion((RectTransform)root.transform,art.coin,new Vector2(.016f,.935f),new Vector2(.30f,.984f),
       value=>top?.PresentRewardCoins(value));
   motion.Reset();
   result=GameplayTools.Label(card.transform,"ArrivalReward",L.V("reward.daily",ProgressLedger.DailyGiftCoins,ProgressLedger.DailyGiftHints),27,new Vector2(.06f,.21f),new Vector2(.94f,.33f));
   open=Control(card.transform,"ArrivalOpen",L.V("reward.open"),new Vector2(.18f,.07f),new Vector2(.82f,.20f),()=>OpenReward(receipt),panel);
   close=Control(card.transform,"ArrivalContinue",L.V("common.ok"),new Vector2(.18f,.07f),new Vector2(.82f,.20f),()=>{motion.Reset();root.SetActive(false);},panel);close.gameObject.SetActive(false);dismiss=Control(card.transform,"ArrivalDismiss",L.V("common.close"),new Vector2(.73f,.01f),new Vector2(.97f,.07f),()=>root.SetActive(false),panel);root.transform.SetAsLastSibling();
  }
  Button Control(Transform p,string n,string text,Vector2 a,Vector2 b,UnityEngine.Events.UnityAction action,Sprite sprite){var im=GameplayTools.Art(p,n,sprite,a,b,true);im.raycastTarget=true;var btn=im.gameObject.AddComponent<PremiumButton>();btn.targetGraphic=im;btn.onClick.AddListener(action);GameplayTools.Label(p.Find(n),"Label",text,27,new Vector2(.04f,.1f),new Vector2(.96f,.9f));return btn;}
  void Update()
  {
   if(root!=null&&root.activeSelf&&!opening&&open.gameObject.activeSelf&&!GameFeel.Reduced)
    chest.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.unscaledTime*2)*2);
  }
  void OnApplicationPause(bool pause){if(!pause&&safeArea!=null&&(root==null||!root.activeSelf))Mount(safeArea);}
  void OpenReward(long receipt)
  {
   if(opening)return;
   var ledger=GameplayProgress.Current;
   long before=ledger.Presentation.coins;
   if(!ledger.ClaimDailyGift(receipt))
   {
    result.text=ledger.GiftClockBlocked?L.V("error.clock"):ledger.GiftReady?L.V("error.save"):L.V("daily.wait");
    open.interactable=ledger.GiftReady;
    return;
   }
   opening=true;
   open.gameObject.SetActive(false);
   dismiss.gameObject.SetActive(false);
   motion.Show(before);
   GameFeel.Play(GameCue.Chest);
   OnGranted?.Invoke();
   StartCoroutine(OpenChest(ledger.Presentation.coins));
  }
  GiftChestSlice ChestSlice(string name,bool top)
  {
   var rect=GameplayTools.Rect(chest,name,Vector2.one*.5f,Vector2.one*.5f);
   rect.pivot=new Vector2(.5f,.46f);
   var image=rect.gameObject.AddComponent<GiftChestSlice>();
   image.Bind(closedArt,top);
   image.raycastTarget=false;
   image.gameObject.SetActive(false);
   return image;
  }

  IEnumerator OpenChest(long finalBalance)
  {
   var picture=chest.GetComponent<Image>();
   if(!GameFeel.Reduced)
   {
    picture.sprite=openArt;
    chest.localRotation=Quaternion.identity;
    lid.gameObject.SetActive(true);
    body.gameObject.SetActive(true);
    result.alpha=0;
    for(float time=0;time<1.12f;time+=Time.unscaledDeltaTime)
    {
     float windup=Mathf.Clamp01(time/.22f);
     float hinge=Mathf.SmoothStep(0,1,Mathf.Clamp01((time-.18f)/.40f));
     float opened=Mathf.SmoothStep(0,1,Mathf.Clamp01((time-.37f)/.28f));
     float reveal=Mathf.SmoothStep(0,1,Mathf.Clamp01((time-.43f)/.34f));
     var source=closedArt.rect.size;
     var size=source*Mathf.Min(chest.rect.width/source.x,chest.rect.height/source.y);
     lid.rectTransform.sizeDelta=size;
     lid.rectTransform.anchoredPosition=new Vector2(hinge*size.x*.035f,(hinge*.20f-.04f)*size.y);
     lid.rectTransform.localRotation=Quaternion.Euler(-62*hinge,0,9*hinge);
     body.rectTransform.sizeDelta=size;
     body.rectTransform.anchoredPosition=Vector2.down*(size.y*.04f);
     lid.color=body.color=new Color(1,1,1,1-opened);
     picture.color=new Color(1,1,1,opened);
     chest.localScale=Vector3.one*(1+.085f*Mathf.Sin(windup*Mathf.PI)+.045f*Mathf.Sin(hinge*Mathf.PI));
     result.rectTransform.localScale=Vector3.one*(.86f+.14f*reveal+.16f*Mathf.Sin(reveal*Mathf.PI));
     result.color=Color.Lerp(new Color(1,.72f,.24f),Color.white,reveal);
     result.alpha=reveal;
     if(time>.31f) motion.Burst(new Vector2(.5f,.56f),Mathf.Clamp01((time-.31f)/.78f),true,.48f);
     yield return null;
    }
    motion.Burst(new Vector2(.5f,.56f),1,true,.48f);
   }
   lid.gameObject.SetActive(false);
   body.gameObject.SetActive(false);
   picture.sprite=openArt;
   picture.color=Color.white;
   chest.localRotation=Quaternion.identity;
   chest.localScale=Vector3.one;
   result.color=Color.white;
   result.rectTransform.localScale=Vector3.one;
   yield return motion.Fly(chest,finalBalance);
   open.gameObject.SetActive(false);
   close.gameObject.SetActive(true);
   opening=false;
  }
 }

 [RequireComponent(typeof(CanvasRenderer))]
 sealed class GiftChestSlice : MaskableGraphic
 {
  // Follow the metal lip and leave the front clasp attached to the moving lid.
  static readonly Vector2[] Seam =
  {
   new(0,.51f), new(.12f,.51f), new(.26f,.47f), new(.30f,.40f), new(.36f,.34f),
   new(.44f,.33f), new(.49f,.43f), new(.57f,.40f), new(.66f,.35f), new(.74f,.38f),
   new(.86f,.47f), new(.94f,.53f), new(1,.54f)
  };
  Sprite art;
  Vector4 uv;
  bool lid;
  public override Texture mainTexture=>art!=null?art.texture:base.mainTexture;

  internal void Bind(Sprite sprite,bool top)
  {
   art=sprite;
   uv=UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
   lid=top;
   SetAllDirty();
  }

  protected override void OnPopulateMesh(VertexHelper mesh)
  {
   mesh.Clear();
   for(int i=0;i<Seam.Length;i++)
   {
    var point=Seam[i];
    Add(point.x,lid?point.y:0);
    Add(point.x,lid?1:point.y);
    if(i==0)continue;
    int vertex=i*2;
    mesh.AddTriangle(vertex-2,vertex-1,vertex+1);
    mesh.AddTriangle(vertex-2,vertex+1,vertex);
   }

   void Add(float x,float y)
   {
    Rect bounds=rectTransform.rect;
    mesh.AddVert(bounds.min+Vector2.Scale(new Vector2(x,y),bounds.size),color,
        new Vector2(Mathf.Lerp(uv.x,uv.z,x),Mathf.Lerp(uv.y,uv.w,y)));
   }
  }
 }
}
