using UnityEngine;
using UnityEngine.UI;
namespace Mewgic
{
    // Home presentation only. No puzzle state or input handling lives here.
    [DefaultExecutionOrder(100)] public sealed class HomePotionDisplay : MonoBehaviour
    {
        public Sprite left, center, right, leftGlow, rightGlow, leftSparkles, rightSparkles, centerSparkles;
        RectTransform root;
        readonly RectTransform[] bottles=new RectTransform[3];
        readonly Image[] glows=new Image[3], sparks=new Image[3];
        public float AnimationTime { get; private set; }
        public void Mount(RectTransform parent)
        {
            if(root!=null)return;
            root=Rect(parent,"HomePotionDisplay");
            var marker=Rect(root,"PotionPlatform"); // Existing painted platform remains in WorldDecorations.
            marker.gameObject.SetActive(false);
            var sprites=new[]{left,center,right};
            var glowSprites=new[]{leftGlow,(Sprite)null,rightGlow};
            var sparkleSprites=new[]{leftSparkles,centerSparkles,rightSparkles};
            for(int i=0;i<3;i++)
            {
                bottles[i]=Rect(root,new[]{"LeftPotion","CenterPotion","RightPotion"}[i]);
                Image(bottles[i],"BaseSprite",sprites[i],1);
                if(glowSprites[i]!=null)glows[i]=Image(bottles[i],"Glow",glowSprites[i],.09f);
                sparks[i]=Image(bottles[i],"Sparkles",sparkleSprites[i],.15f);
                if(i==1){var spiral=Rect(bottles[i],"MagicSpiral_BakedIntoBaseSprite");spiral.gameObject.SetActive(false);}
            }
            // Golden spiral is already baked into the approved center art: do not double it.
            Layout();
        }
        static RectTransform Rect(Transform parent,string name)
        {
            var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);
            var r=(RectTransform)g.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        static Image Image(Transform p,string name,Sprite s,float alpha)
        {
            var r=Rect(p,name);var image=r.gameObject.AddComponent<Image>();image.sprite=s;image.preserveAspect=true;image.raycastTarget=false;image.color=new Color(1,1,1,alpha);return image;
        }
        public void Layout()
        {
            if(root==null)return;
            float w=root.rect.width,h=root.rect.height;
            float unit=Mathf.Min(w/900f,h/1600f);
            // Align to the existing environment platform, compensating canvas transparent margins.
            float floor=646*unit;
            var platform=transform.Find("WorldDecorations/EnvironmentComposition/Layer05_Foreground") as RectTransform;
            if(platform!=null){var point=platform.TransformPoint(new Vector3(platform.rect.center.x,platform.rect.yMin+platform.rect.height*.414f));floor=root.InverseTransformPoint(point).y-root.rect.yMin-6*unit;}
            float[] xs={-.17f,.055f,.265f};float[] heights={420,560,403};
            for(int i=0;i<3;i++)
            {
                var r=bottles[i];r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.pivot=new Vector2(.5f,.075f);
                r.sizeDelta=new Vector2(heights[i]*1018/1545f,heights[i])*unit;
                r.anchoredPosition=new Vector2(xs[i]*w,floor+Mathf.Sin(AnimationTime*.65f+i*1.8f)*1.5f*unit);
            }
        }
        void LateUpdate()
        {
            AnimationTime=Time.unscaledTime;Layout();
            for(int i=0;i<3;i++)
            {
                if(glows[i]!=null)glows[i].color=new Color(1,1,1,.085f+Mathf.Sin(AnimationTime*.8f+i)*.025f);
                if(sparks[i]!=null)sparks[i].color=new Color(1,1,1,.08f+Mathf.Pow((Mathf.Sin(AnimationTime*.9f+i*2)+1)*.5f,5)*.16f);
            }
            if(bottles[1]!=null)bottles[1].GetChild(0).GetComponent<Image>().color=new Color(1,1,1,.975f+Mathf.Sin(AnimationTime*.7f)*.025f);
        }
    }
}



