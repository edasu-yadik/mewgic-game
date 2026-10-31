using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Mewgic
{
    public sealed class PremiumPlayButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ICanvasRaycastFilter
    {
        public Sprite normal, pressed, glow;
        public Shader sweepShader;
        Material sweepMaterial;
        static readonly int SweepId=Shader.PropertyToID("_Sweep");
        Button button; Image art, frameLight; RectTransform visual;
        Button.ButtonClickedEvent original;
        bool held, queued; float scale=1;
        public bool TransitionQueued=>queued;
        public int AcceptedClicks {get;private set;}
        public void Bind(Button target)
        {
            button=target;var hitRect=(RectTransform)button.transform;hitRect.anchorMin=new Vector2(.15f,.14f); original=button.onClick;button.onClick=new Button.ButtonClickedEvent();button.onClick.AddListener(Activate);
            button.transition=Selectable.Transition.None;
            var outline=button.GetComponent<Outline>();if(outline!=null)outline.enabled=false;
            var label=button.transform.Find("Label");if(label!=null)label.gameObject.SetActive(false);
            var hit=button.GetComponent<Image>();hit.color=Color.clear;hit.raycastTarget=true;
            visual=new GameObject("PlayArtwork",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();visual.SetParent(button.transform,false);visual.anchorMin=Vector2.zero;visual.anchorMax=Vector2.one;visual.offsetMin=visual.offsetMax=Vector2.zero;
            art=visual.GetComponent<Image>();art.sprite=normal;art.preserveAspect=true;art.raycastTarget=false;
            var fx=new GameObject("FrameGlow",typeof(RectTransform),typeof(Image));fx.transform.SetParent(visual,false);var r=(RectTransform)fx.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            frameLight=fx.GetComponent<Image>();frameLight.sprite=glow;frameLight.preserveAspect=true;frameLight.raycastTarget=false;
            if(sweepShader!=null){sweepMaterial=new Material(sweepShader);frameLight.material=sweepMaterial;}
        }
        void Update()
        {
            if(visual==null)return;
            bool idle=!held&&!queued&&button.IsInteractable();
            float target=held?.966f:idle?1+Mathf.Sin(Time.unscaledTime*1.05f)*.006f:1;
            scale=Mathf.MoveTowards(scale,target,Time.unscaledDeltaTime*.5f);visual.localScale=Vector3.one*scale;
            art.sprite=held?pressed:normal;
            frameLight.color=new Color(1,1,1,held?.025f:.045f+Mathf.Sin(Time.unscaledTime*.8f)*.015f);
            if(sweepMaterial!=null)sweepMaterial.SetFloat(SweepId,idle?Mathf.Repeat(Time.unscaledTime/7f,1)*1.8f-.4f:-1);
        }
        void OnDestroy(){if(sweepMaterial!=null)Destroy(sweepMaterial);}
        public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&!queued&&button.IsInteractable())held=true;}
        public void OnPointerUp(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)held=false;}
        public void OnPointerExit(PointerEventData e){held=false;}
        public void AllowRetry(){queued=false;held=false;if(button!=null)button.interactable=true;}
        void Activate(){if(queued)return;queued=true;held=false;AcceptedClicks++;button.interactable=false;StartCoroutine(Release());}
        IEnumerator Release(){yield return new WaitForSecondsRealtime(.14f);original.Invoke();}
        public bool IsRaycastLocationValid(Vector2 point,Camera camera)
        {
            var r=(RectTransform)transform;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(r,point,camera,out var p))return false;
            var q=(p-r.rect.center);float x=q.x/(r.rect.width*.5f),y=q.y/(r.rect.height*.5f);return x*x+y*y<=1.08f;
        }
    }
}
