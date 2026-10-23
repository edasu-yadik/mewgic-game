using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class MewgicFoundationSetup
{
    static readonly string[] Names = { "Boot", "Home", "Gameplay", "LevelMap" };
    public static void Configure()
    {
        foreach (var name in new[] { "Art", "Audio", "Materials", "Prefabs", "Scenes", "Scripts", "ScriptableObjects", "UI" })
            if (!AssetDatabase.IsValidFolder("Assets/" + name)) AssetDatabase.CreateFolder("Assets", name);
        GraphicsSettings.defaultRenderPipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
        PlayerSettings.productName = "Mewgic";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.defaultScreenWidth = 1080;
        PlayerSettings.defaultScreenHeight = 1920;
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.mewgic.game");
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        foreach (var name in Names)
        {
            var path = "Assets/Scenes/" + name + ".unity";
            if (File.Exists(path)) continue;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.14f, 0.10f, 0.22f);
            var lightObject = new GameObject("Global Light 2D", typeof(Light2D));
            lightObject.GetComponent<Light2D>().lightType = Light2D.LightType.Global;
            EditorSceneManager.SaveScene(scene, path);
        }
        var ordered = Names.Select(n => new EditorBuildSettingsScene("Assets/Scenes/" + n + ".unity", true)).ToList();
        ordered.AddRange(EditorBuildSettings.scenes.Where(s => !ordered.Any(o => o.path == s.path)).Select(s => new EditorBuildSettingsScene(s.path, false)));
        EditorBuildSettings.scenes = ordered.ToArray();
        EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        AssetDatabase.SaveAssets();
        Validate();
    }
    public static void Validate()
    {
        if (GraphicsSettings.defaultRenderPipeline == null) throw new Exception("URP pipeline missing");
        if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait) throw new Exception("Portrait missing");
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android) throw new Exception("Android target inactive");
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android)) throw new Exception("Android module missing");
        if (EditorBuildSettings.scenes[0].path != "Assets/Scenes/Boot.unity") throw new Exception("Boot must be first");
        foreach (var name in Names)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            if (scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>(true)).Any(c => c == null)) throw new Exception("Missing script: " + name);
            if (Camera.main == null || !Camera.main.orthographic) throw new Exception("2D camera missing: " + name);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        Debug.Log("MEWGIC_FOUNDATION_VALIDATION_OK " + Application.unityVersion);
    }
    public static void BuildSmokeTest()
    {
        Validate();
        Directory.CreateDirectory("Builds/Android");
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = Names.Select(n => "Assets/Scenes/" + n + ".unity").ToArray(),
            locationPathName = "Builds/Android/Mewgic-Setup-Test.apk",
            target = BuildTarget.Android,
            options = BuildOptions.Development
        });
        if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Android smoke build failed: " + result.summary.result);
        Debug.Log("MEWGIC_ANDROID_BUILD_OK");
    }
}
