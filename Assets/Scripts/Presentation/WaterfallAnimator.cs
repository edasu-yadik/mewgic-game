using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    /// <summary>Animates only the red channel of an artwork-aligned mask; owns one material.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Image))]
    public sealed class WaterfallAnimator : MonoBehaviour
    {
        [Min(0)] public float speed = .022f;
        [Range(0, .1f)] public float brightness = .045f;
        [Range(0, 1)] public float opacity = .8f;
        Material instance, previous;
        Image image;
        static readonly int Clock = Shader.PropertyToID("_EnvironmentTime");
        static readonly int Speed = Shader.PropertyToID("_FlowSpeed");
        static readonly int Brightness = Shader.PropertyToID("_FlowBrightness");
        static readonly int Opacity = Shader.PropertyToID("_FlowOpacity");
        public Material WaterMaterial => instance;

        public static WaterfallAnimator Attach(Image target, Shader shader, Texture2D mask, bool calm = false)
        {
            if (target == null || shader == null || mask == null) return null;
            var animator = target.GetComponent<WaterfallAnimator>();
            if (animator != null) return animator;
            animator = target.gameObject.AddComponent<WaterfallAnimator>();
            animator.image = target;
            animator.previous = target.material;
            animator.instance = new Material(shader) { name = target.name + " Water (runtime)" };
            animator.instance.SetTexture("_EffectMask", mask);
            if (calm) { animator.speed = .012f; animator.brightness = .028f; animator.opacity = .65f; }
            target.material = animator.instance;
            animator.Apply(Time.unscaledTime);
            return animator;
        }

        public void Apply(float time)
        {
            if (instance == null) return;
            instance.SetFloat(Clock, time);
            instance.SetFloat(Speed, Mathf.Max(0, speed));
            instance.SetFloat(Brightness, Mathf.Clamp(brightness, 0, .1f));
            instance.SetFloat(Opacity, Mathf.Clamp01(opacity));
        }
        void LateUpdate() { Apply(GameFeel.Reduced?0:Time.unscaledTime); }
        void OnDestroy()
        {
            if (image != null && image.material == instance) image.material = previous;
            if (instance != null) Destroy(instance);
        }
    }
}
