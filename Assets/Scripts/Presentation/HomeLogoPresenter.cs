using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    /// <summary>Home-only logo presentation. Reserves the upper safe-area band for a future resource bar.</summary>
    public sealed class HomeLogoPresenter : MonoBehaviour
    {
        public Sprite logo;
        public void Mount(RectTransform safe)
        {
            if(safe.Find("MewgicLogo")!=null)return;
            if(logo==null){Debug.LogError("Home logo sprite missing",this);return;}
            var g=new GameObject("MewgicLogo",typeof(RectTransform),typeof(Image));
            g.transform.SetParent(safe,false);
            var r=(RectTransform)g.transform;r.anchorMin=new Vector2(.10f,.775f);r.anchorMax=new Vector2(.90f,.925f);r.offsetMin=r.offsetMax=Vector2.zero;
            var image=g.GetComponent<Image>();image.sprite=logo;image.preserveAspect=true;image.raycastTarget=false;
        }
    }
}
