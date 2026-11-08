using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    /// <summary>Home-only composition; the shared character prefab remains unchanged.</summary>
    public sealed class HomeCharacterPresenter : MonoBehaviour
    {
        public RectTransform character, potionLayer, wizardLayer, effectsLayer;
        public Mewgic.Characters.WizardCatPresentation cat;
        RectTransform safe, play;
        float nextMagic;
        public int MagicCount { get; private set; }

        public void Mount(RectTransform safeArea, RectTransform background)
        {
            safe = safeArea;
            var root = (RectTransform)transform;
            root.SetParent(safe.parent, false);
            Stretch(root);
            background.SetParent(transform.Find("Background"), false);
            Stretch(background);
            var environment = GetComponent<HomeEnvironment>();
            if (environment != null) environment.Mount();
            Destroy(transform.Find("UI").gameObject);
            safe.SetParent(transform, false);
            safe.name = "UI";
            safe.SetSiblingIndex(4);
            character.gameObject.SetActive(true);
            cat?.BindPlayerCosmetics();
            nextMagic = Time.unscaledTime + 3.25f;
        }

        public void Bind(Button playButton) { play = (RectTransform)playButton.transform; Layout(); }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        public void Layout()
        {
            if (safe == null || play == null) return;
            MatchSafeArea(potionLayer); MatchSafeArea(wizardLayer); MatchSafeArea(effectsLayer);
            float width = safe.rect.width, height = safe.rect.height;
            float bottom = height * play.anchorMax.y + 18f;
            float scale = Mathf.Min(width * .34f / character.rect.width,
                Mathf.Max(1f, height * .65f - bottom) / (character.rect.height * 1.01f));
            character.anchorMin = character.anchorMax = new Vector2(.5f, 0f);
            character.pivot = new Vector2(.5f, 0f);
            character.localScale = Vector3.one * scale;
            character.anchoredPosition = new Vector2(-width * .5f + width * .012f + character.rect.width * scale * .5f, bottom);
        }

        void MatchSafeArea(RectTransform layer)
        {
            layer.anchorMin = safe.anchorMin; layer.anchorMax = safe.anchorMax;
            layer.offsetMin = layer.offsetMax = Vector2.zero;
        }

        void LateUpdate()
        {
            Layout();
            if (safe != null && Time.unscaledTime >= nextMagic && !cat.MagicBusy)
            {
                if (cat.TriggerMagic()) MagicCount++;
                nextMagic = Time.unscaledTime + Random.Range(8f, 13f);
            }
        }
    }
}
