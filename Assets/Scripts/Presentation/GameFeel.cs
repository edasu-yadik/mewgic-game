using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mewgic
{
    public enum GameCue { Pour, Valid, Invalid, Victory, Coin, Chest, Map }

    public sealed class GameFeel : MonoBehaviour
    {
        static GameFeel instance;
        static readonly string[] CueNames = { "Pour", "Valid", "Invalid", "Victory", "Coin", "Chest", "Map" };
        AudioSource music, ambience, effects;
        AudioClip[] cues;
        bool suspended;
        float lastHaptic = -1;
        public static bool Reduced => GameplayProgress.Current.ReducedMotion;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntime() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            var root = new GameObject("Mewgic Game Feel");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<GameFeel>();
        }

        void Awake()
        {
            music = Channel("Music", true, .28f);
            ambience = Channel("Waterfall", true, .14f);
            effects = Channel("Effects", false, .65f);
            cues = new AudioClip[CueNames.Length];
            for (int i = 0; i < cues.Length; i++) cues[i] = Load(CueNames[i]);
            ambience.clip = Load("Waterfall");
            SceneManager.sceneLoaded += OnScene;
        }

        AudioSource Channel(string label, bool loop, float volume)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = volume;
            return source;
        }

        static AudioClip Load(string name) => Resources.Load<AudioClip>("Audio/Part18/" + name);

        void OnScene(Scene scene, LoadSceneMode mode)
        {
            var track = scene.name switch
            {
                "Gameplay" => Load("PuzzleMusic"),
                "Home" or "LevelMap" => Load("HomeMusic"),
                _ => null
            };
            if (music.clip != track) { music.Stop(); music.clip = track; }
            bool hasWater = scene.name is "Home" or "LevelMap" or "Gameplay";
            ambience.enabled = hasWater;
            ApplyPreferences();
        }

        public static void ApplyPreferences()
        {
            if (instance == null) return;
            var settings = GameplayProgress.Current.Presentation;
            SetLoop(instance.music, settings.MusicEnabled && !instance.suspended);
            SetLoop(instance.ambience, settings.EffectsEnabled && !instance.suspended);
            if (!settings.EffectsEnabled || instance.suspended) instance.effects.Stop();
        }

        static void SetLoop(AudioSource source, bool play)
        {
            if (!play) { if (source.isPlaying) source.Stop(); }
            else if (source.enabled && source.clip != null && !source.isPlaying) source.Play();
        }

        public static void Play(GameCue cue)
        {
            if (instance == null || instance.suspended) return;
            var settings = GameplayProgress.Current.Presentation;
            if (settings.EffectsEnabled && instance.cues[(int)cue] != null)
                instance.effects.PlayOneShot(instance.cues[(int)cue]);
            if (settings.vibrationEnabled && Application.isMobilePlatform &&
                (cue is GameCue.Invalid or GameCue.Victory) && Time.unscaledTime - instance.lastHaptic >= .6f)
            {
                instance.lastHaptic = Time.unscaledTime;
                Handheld.Vibrate();
            }
        }

        void OnApplicationPause(bool paused)
        {
            suspended = paused;
            ApplyPreferences();
        }

        void OnDestroy() => SceneManager.sceneLoaded -= OnScene;
    }
}
