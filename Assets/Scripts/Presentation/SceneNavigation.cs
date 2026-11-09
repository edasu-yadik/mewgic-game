using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    public static class SceneNavigation
    {
        static int lastFrame = -1;
        static SceneFade fade;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { lastFrame = -1; fade = null; }

        public static bool LoadOnce(string scene, Func<bool> select = null)
        {
            if (lastFrame == Time.frameCount || !Application.CanStreamedLevelBeLoaded(scene)) return false;
            if (select != null && !select()) return false;
            lastFrame = Time.frameCount;
            if (scene == "LevelMap" || SceneManager.GetActiveScene().name == "LevelMap") GameFeel.Play(GameCue.Map);
            SceneManager.LoadScene(scene);
            return true;
        }

        internal static bool LoadWithFade(string scene, Func<bool> select, Action failed)
        {
            if (fade != null || !Application.CanStreamedLevelBeLoaded(scene)) return false;
            if (GameFeel.Reduced) return LoadOnce(scene,select);
            var root = new GameObject("MapReturnFade",typeof(Canvas),typeof(GraphicRaycaster),typeof(SceneFade));
            UnityEngine.Object.DontDestroyOnLoad(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;
            fade = root.GetComponent<SceneFade>();
            fade.StartCoroutine(fade.Play(()=>LoadOnce(scene,select),failed));
            return true;
        }
    }

    internal sealed class SceneFade : MonoBehaviour
    {
        internal IEnumerator Play(Func<bool> load, Action failed)
        {
            var veil = GameplayTools.Art(transform,"MagicVeil",null,Vector2.zero,Vector2.one);
            veil.raycastTarget = true;
            yield return Blend(veil,0,1,.25f);
            bool loaded = load();
            if (loaded)
            {
                yield return null;
                yield return new WaitForEndOfFrame();
            }
            else failed?.Invoke();
            yield return Blend(veil,1,0,.33f);
            Destroy(gameObject);
        }

        static IEnumerator Blend(Image veil,float from,float to,float duration)
        {
            for (float time = 0; time < duration; time += Time.unscaledDeltaTime)
            {
                veil.color = new Color(.105f,.031f,.19f,Mathf.SmoothStep(from,to,time/duration));
                yield return null;
            }
            veil.color = new Color(.105f,.031f,.19f,to);
        }
    }
}
