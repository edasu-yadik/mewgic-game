using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Mewgic
{
    public enum HomeDestination {Home,Map,Events,Team,Inventory}
    public sealed class HomeNavigation : MonoBehaviour
    {
        public Sprite home,map,events,team,inventory,tile,activeTile;
        [Tooltip("Enable only when LevelMap contains usable map UI and a return path.")] public bool mapSceneReady;
        public string mapScene="LevelMap";
        RectTransform root,safe;
        HomeTopBar information;
        bool navigating;
        public HomeDestination ActiveDestination=>HomeDestination.Home;
        public void Mount(RectTransform safeArea)
        {
            if(root!=null)return;safe=safeArea;information=GetComponent<HomeTopBar>();
            root=new GameObject("HomeBottomNavigation",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(safe,false);root.anchorMin=new Vector2(.012f,0);root.anchorMax=new Vector2(.988f,0);root.pivot=new Vector2(.5f,0);
            var sprites=new[]{home,map,events,team,inventory};
            for(int i=0;i<5;i++)
            {
                var destination=(HomeDestination)i;
                var g=new GameObject("Nav"+destination,typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(root,false);var r=(RectTransform)g.transform;r.anchorMin=new Vector2(i*.2f+.003f,0);r.anchorMax=new Vector2((i+1)*.2f-.003f,1);r.offsetMin=r.offsetMax=Vector2.zero;
                var image=g.GetComponent<Image>();image.sprite=i==0?activeTile:tile;image.raycastTarget=true;
                var button=g.GetComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>Navigate(destination));
                var icon=new GameObject("Icon",typeof(RectTransform),typeof(Image));icon.transform.SetParent(g.transform,false);var ir=(RectTransform)icon.transform;ir.anchorMin=new Vector2(.08f,.23f);ir.anchorMax=new Vector2(.92f,.97f);ir.offsetMin=ir.offsetMax=Vector2.zero;var art=icon.GetComponent<Image>();art.sprite=sprites[i];art.preserveAspect=true;art.raycastTarget=false;
                var label=new GameObject("Label",typeof(RectTransform),typeof(Text));label.transform.SetParent(g.transform,false);var lr=(RectTransform)label.transform;lr.anchorMin=new Vector2(.035f,.025f);lr.anchorMax=new Vector2(.965f,.255f);lr.offsetMin=lr.offsetMax=Vector2.zero;var text=label.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontStyle=FontStyle.Bold;text.fontSize=25;text.color=Color.white;text.text=destination.ToString();text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;text.resizeTextForBestFit=true;text.resizeTextMinSize=14;text.resizeTextMaxSize=25;var shadow=label.AddComponent<Shadow>();shadow.effectColor=Color.black;shadow.effectDistance=new Vector2(1,-2);
            }
            Layout();
        }
        public void Navigate(HomeDestination destination)
        {
            if(navigating)return;
            if(destination==HomeDestination.Events){var hub=UnityEngine.Object.FindFirstObjectByType<Mewgic.Rewards.RewardHubPanel>();if(hub!=null){hub.Open(0);return;}}
            if(destination==HomeDestination.Home){information.Close();return;}
            if(destination==HomeDestination.Map&&(mapSceneReady||Mewgic.Map.MapProgressionCatalog.Ready)&&Application.CanStreamedLevelBeLoaded(mapScene)){navigating=true;SceneManager.LoadScene(mapScene);return;}
            string message=destination==HomeDestination.Map?"Harita verisi veya sahne bağlantısı doğrulanamadı.":destination==HomeDestination.Events?"Etkinlik merkezi şu anda açılamadı.":destination==HomeDestination.Team?"Takım sistemi henüz geliştirilmedi. Bu sekme yalnızca önizleme bilgi panelidir.":"Envanter sistemi henüz geliştirilmedi. Bu sekme yalnızca önizleme bilgi panelidir.";
            information.Show(message);
        }
        public void Layout(){if(root==null)return;float gap=4f;root.sizeDelta=new Vector2(0,Mathf.Min(safe.rect.width*.18f,safe.rect.height*.095f-gap));root.anchoredPosition=new Vector2(0,gap);}
        void LateUpdate(){Layout();}
    }
}
