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
                    View.ShowError(Error);
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
            Stage("Save", 0, "Büyülü yolculuğun hazırlanıyor…");
            // Önce UI render edilsin; uzun süreli yapay bekleme yok.
            yield return null;
            var progress = GameplayProgress.Current;
            if (GameplayProgress.LoadError != null)
                throw new InvalidOperationException("Kayıt okunamadı. Verilerin korundu. Uygulamayı yeniden açıp tekrar dene.");
            GameplayLevelCatalog.ResumeFromSave();
            Stage("Dependencies", .15f, "Yolculuğun hatırlanıyor…");
            if (background == null || logo == null || character == null)
                throw new InvalidOperationException("Açılış görselleri yüklenemedi. Lütfen tekrar dene.");
            if (!Application.CanStreamedLevelBeLoaded(homeScene))
                throw new InvalidOperationException("Ana menü yüklenemedi. Lütfen tekrar dene.");
            var request = Resources.LoadAsync<TextAsset>("Levels/Level" + GameplayLevelCatalog.SelectedId.ToString("D2"));
            while (!request.isDone) { Stage("Dependencies", .15f + .15f * request.progress, "Büyülü kaynaklar hazırlanıyor…"); yield return null; }
            if (request.asset == null) throw new InvalidOperationException("Başlangıç bölümünün verileri bulunamadı.");
            GameplayLevelCatalog.Current(); // Bölüm kimliği, içerik ve çözülebilirlik doğrulanır.
            if (Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") == null)
                throw new InvalidOperationException("Yazı kaynağı yüklenemedi.");
            Stage("HomeLoading", .30f, "Büyülü dünya yükleniyor…");
            homeLoad = SceneManager.LoadSceneAsync(homeScene, LoadSceneMode.Single);
            if (homeLoad == null) throw new InvalidOperationException("Ana menü yüklemesi başlatılamadı.");
            homeLoad.allowSceneActivation = false;
            while (homeLoad.progress < .9f)
            {
                Stage("HomeLoading", .30f + .65f * Mathf.Clamp01(homeLoad.progress / .9f), "Büyülü dünya yükleniyor…");
                yield return null;
            }
            Stage("Ready", .95f, "Son sihirli dokunuşlar…");
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
            Stage("Complete", 1, "Mewgic’e hoş geldin!");
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
