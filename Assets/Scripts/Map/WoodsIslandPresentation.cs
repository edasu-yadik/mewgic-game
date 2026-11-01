using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    public sealed class WoodsIslandPresentation : MonoBehaviour
    {
        public Image water, lights, mist;
        public RectTransform[] fireflies;
        public bool Animate = true;
        public float Clock { get; private set; }
        public float EntranceProgress { get; private set; }
        public float WaterPhase => waterMaterial != null ? waterMaterial.GetFloat("_EnvironmentTime") : 0;
        Material waterMaterial, lightMaterial;
        CanvasGroup fade;
        Vector2 mistOrigin;
        float entered;

        void Awake()
        {
            fade = GetComponent<CanvasGroup>();
            waterMaterial = new Material(water.material);
            lightMaterial = new Material(lights.material);
            water.material = waterMaterial;
            lights.material = lightMaterial;
            mistOrigin = mist.rectTransform.anchoredPosition;
        }
        public void BeginEntrance() { entered = Time.unscaledTime; Tick(0); }
        void Update() { if (Animate) Clock += Time.unscaledDeltaTime; Tick(Clock); }
        void Tick(float time)
        {
            EntranceProgress = Animate ? Mathf.Clamp01((Time.unscaledTime - entered) / .85f) : 1;
            float ease = 1 - Mathf.Pow(1 - EntranceProgress, 3);
            fade.alpha = ease;
            transform.localScale = Vector3.one * Mathf.Lerp(.975f, 1, ease);
            waterMaterial.SetFloat("_EnvironmentTime", time);
            lightMaterial.SetFloat("_EnvironmentTime", time);
            // Stencil materyal kopyaları kullanıldığında da saat güncel kalır.
            water.materialForRendering.SetFloat("_EnvironmentTime", time);
            lights.materialForRendering.SetFloat("_EnvironmentTime", time);
            mist.rectTransform.anchoredPosition = mistOrigin + new Vector2(Mathf.Sin(time * .12f) * 8, Mathf.Sin(time * .17f) * 3);
            mist.color = new Color(.70f,.86f,1, .075f + Mathf.Sin(time * .25f) * .015f);
            for (int i = 0; i < fireflies.Length; i++)
            {
                float t = time * .55f + i * 1.73f;
                fireflies[i].anchoredPosition = new Vector2(Mathf.Sin(t * .7f) * 8, Mathf.Cos(t) * 6);
                fireflies[i].GetComponent<Graphic>().color = new Color(1,.83f,.32f,.2f + .45f * Mathf.Pow(.5f + .5f * Mathf.Sin(t * 1.4f), 2));
            }
        }
        void OnDestroy()
        {
            if (waterMaterial != null) Destroy(waterMaterial);
            if (lightMaterial != null) Destroy(lightMaterial);
        }
    }

}
