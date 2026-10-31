using L=Mewgic.Localization.L;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mewgic
{
    // Sadece ilk Boot sahnesine aittir. Oyun/kayıt şemalarını değiştirmez.
    public sealed class BootLoadingController : MonoBehaviour
    {
        public Texture2D background, logo, character;
        public string homeScene = "Home";
        public string State { get; private set; } = "Created";
        public float ActualProgress { get; private set; }
        public string Error { get; private set; }
        public BootLoadingView View { get; private set; }
        public int ActivationCount { get; private set; }
        public static BootLoadingController Active { get; private set; }
        public event Action<string, float> StageChanged;
        bool running;
        AsyncOperation homeLoad;
#if UNITY_EDITOR
        public bool EditorHoldBeforeActivation;
#endif
        void Awake()
        {
            if (Active != null && Active != this) { Destroy(gameObject); return; }
            Active = this;
            DontDestroyOnLoad(gameObject);
        }
        void Start()
        {
            View = gameObject.AddComponent<BootLoadingView>();
            View.Mount(background, logo, character, Retry);
            Retry();
        }
        public void Retry()
        {
            if (running || homeLoad != null) return;
            Error = null; ActualProgress = 0;
            View.ResetDisplay();
            running = true;
            StartCoroutine(Guard(Load()));
        }
        IEnumerator Guard(IEnumerator steps)
        {
            while (true)
            {
                object next = null;
                bool more = false;
                try { more = steps.MoveNext(); if (more) next = steps.Current; }
                catch (Exception e)
                {
                    Error = e.Message; State = "Failed"; running = false;
                    View.ShowError(L.V("boot.error"));
                    Debug.LogWarning("MEWGIC_BOOT_ERROR: " + Error);
                    break;
                }
                if (!more) break;
                yield return next;
            }
            (steps as IDisposable)?.Dispose();
        }
        void Stage(string state, float progress, string message)
        {
            State = state;
            ActualProgress = Mathf.Max(ActualProgress, Mathf.Clamp01(progress));
            View.SetProgress(ActualProgress, message);
            StageChanged?.Invoke(state, ActualProgress);
        }
        IEnumerator Load()
        {
            while(!L.Ready)yield return null;
            Stage("Save", 0, L.V("boot.preparing"));
            // Önce UI render edilsin; uzun süreli yapay bekleme yok.
            yield return null;
            var progress = GameplayProgress.Current;
            if (GameplayProgress.LoadError != null)
                throw new InvalidOperationException(L.V("error.load"));
            GameplayLevelCatalog.ResumeFromSave();
            Stage("Dependencies", .15f, L.V("boot.save"));
            if (background == null || logo == null || character == null)
                throw new InvalidOperationException("Açılış görselleri yüklenemedi. Lütfen tekrar dene.");
            if (!Application.CanStreamedLevelBeLoaded(homeScene))
                throw new InvalidOperationException("Ana menü yüklenemedi. Lütfen tekrar dene.");
            var prepare=GameplayLevelCatalog.PrepareAsync(GameplayLevelCatalog.SelectedId);
            while(!prepare.IsCompleted){Stage("Dependencies",.25f,L.V("boot.resources"));yield return null;}
            if(prepare.IsFaulted)throw new InvalidOperationException("Başlangıç bölümü doğrulanamadı. Kaydın korundu; tekrar dene.");
            GameplayLevelCatalog.Current(); // Bölüm kimliği, içerik ve çözülebilirlik doğrulanır.
            if (Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") == null)
                throw new InvalidOperationException("Yazı kaynağı yüklenemedi.");
            Stage("HomeLoading", .30f, L.V("boot.loading"));
            homeLoad = SceneManager.LoadSceneAsync(homeScene, LoadSceneMode.Single);
            if (homeLoad == null) throw new InvalidOperationException("Ana menü yüklemesi başlatılamadı.");
            homeLoad.allowSceneActivation = false;
            while (homeLoad.progress < .9f)
            {
                Stage("HomeLoading", .30f + .65f * Mathf.Clamp01(homeLoad.progress / .9f), L.V("boot.loading"));
                yield return null;
            }
            Stage("Ready", .95f, L.V("boot.ready"));
#if UNITY_EDITOR
            while (EditorHoldBeforeActivation) yield return null;
#endif
            // Home kendi EventSystem'ini kurar; iki EventSystem aynı anda çalışmaz.
            View.ReleaseInput();
            yield return null;
            homeLoad.allowSceneActivation = true;
            while (!homeLoad.isDone) yield return null;
            if (SceneManager.GetActiveScene().name != homeScene)
                throw new InvalidOperationException("Ana menü etkinleştirilemedi.");
            homeLoad = null; ActivationCount++;
            Stage("Complete", 1, L.V("boot.welcome"));
            while (View.DisplayProgress < .999f) yield return null;
            // Yalnız 180ms görsel geçiş; yüzdeyi yükselten zamanlayıcı yok.
            float elapsed = 0;
            while (elapsed < .18f)
            {
                elapsed += Time.unscaledDeltaTime;
                View.Opacity = 1 - Mathf.Clamp01(elapsed / .18f);
                yield return null;
            }
            running = false;
            Destroy(gameObject);
        }
        void OnDestroy() { if (Active == this) Active = null; }
    }
}
