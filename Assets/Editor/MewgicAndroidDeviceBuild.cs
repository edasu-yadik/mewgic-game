using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MewgicAndroidDeviceBuild
{
    const string Output = "Builds/Android/Part19B";
    const string Package = "com.mewgic.game";

    public static void TestApk() => Build(false, true);
    public static void ReleaseAab() => Build(true, false);
    public static void RcLocalApk() => Build(false, false);

    static void Build(bool release, bool development)
    {
        if (EditorApplication.isPlaying || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android ||
            !BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new InvalidOperationException("Android hedefi ve Build Support etkin olmalı.");
        var target = NamedBuildTarget.Android;
        if (PlayerSettings.GetApplicationIdentifier(target) != Package ||
            PlayerSettings.GetScriptingBackend(target) != ScriptingImplementation.IL2CPP ||
            PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64 ||
            (int)PlayerSettings.Android.minSdkVersion != 26)
            throw new InvalidOperationException("Android kimliği, IL2CPP, ARM64 veya minimum API 26 doğrulanamadı.");

        string rawCode = Environment.GetEnvironmentVariable("MEWGIC_ANDROID_BUILD_CODE");
        if (string.IsNullOrEmpty(rawCode) && !release) rawCode = (PlayerSettings.Android.bundleVersionCode + (development ? 1 : 0)).ToString();
        if (!int.TryParse(rawCode, out int code) || code < PlayerSettings.Android.bundleVersionCode || code > 2100000000)
            throw new InvalidOperationException("Geçerli ve önceki sürümden büyük Android build kodu gerekli.");

        string keystore = null, storePassword = null, alias = null, aliasPassword = null;
        if (release)
        {
            keystore = Secret("MEWGIC_ANDROID_KEYSTORE");
            if (!Path.IsPathRooted(keystore) || !File.Exists(keystore))
                throw new InvalidOperationException("İmzalama dosyası için var olan mutlak yol gerekli.");
            storePassword = Secret("MEWGIC_ANDROID_KEYSTORE_PASSWORD");
            alias = Secret("MEWGIC_ANDROID_KEY_ALIAS");
            aliasPassword = Secret("MEWGIC_ANDROID_KEY_PASSWORD");
        }

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        string[] expected = { "Boot", "Home", "Gameplay", "LevelMap" };
        if (!scenes.Select(Path.GetFileNameWithoutExtension).SequenceEqual(expected))
            throw new InvalidOperationException("Üretim sahneleri veya açılış sırası doğrulanamadı.");

        Directory.CreateDirectory(Output);
        string path = $"{Output}/Mewgic-{PlayerSettings.bundleVersion}-{code}-{(release ? "release.aab" : development ? "qa.apk" : "rc-local.apk")}";
        int oldCode = PlayerSettings.Android.bundleVersionCode;
        var oldTarget = PlayerSettings.Android.targetSdkVersion;
        bool oldBundle = EditorUserBuildSettings.buildAppBundle, oldSplit = PlayerSettings.Android.buildApkPerCpuArchitecture;
        bool oldCustom = PlayerSettings.Android.useCustomKeystore;
        string oldStore = PlayerSettings.Android.keystoreName, oldStorePassword = PlayerSettings.Android.keystorePass;
        string oldAlias = PlayerSettings.Android.keyaliasName, oldAliasPassword = PlayerSettings.Android.keyaliasPass;
        try
        {
            PlayerSettings.Android.bundleVersionCode = code;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
            EditorUserBuildSettings.buildAppBundle = release;
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
            PlayerSettings.Android.useCustomKeystore = release;
            if (release)
            {
                PlayerSettings.Android.keystoreName = keystore;
                PlayerSettings.Android.keystorePass = storePassword;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = aliasPassword;
            }
            var options = BuildOptions.DetailedBuildReport;
            if (development) options |= BuildOptions.Development;
            var summary = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = path, target = BuildTarget.Android, options = options
            }).summary;
            File.WriteAllText(path + ".summary.txt",
                $"result={summary.result}\nerrors={summary.totalErrors}\nwarnings={summary.totalWarnings}\nbytes={(File.Exists(path) ? new FileInfo(path).Length : 0)}\nseconds={summary.totalTime.TotalSeconds:F1}\n");
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android derlemesi başarısız. Unity Console ve özet dosyasını inceleyin.");
        }
        finally
        {
            PlayerSettings.Android.bundleVersionCode = oldCode;
            PlayerSettings.Android.targetSdkVersion = oldTarget;
            EditorUserBuildSettings.buildAppBundle = oldBundle;
            PlayerSettings.Android.buildApkPerCpuArchitecture = oldSplit;
            PlayerSettings.Android.useCustomKeystore = oldCustom;
            PlayerSettings.Android.keystoreName = oldStore;
            PlayerSettings.Android.keystorePass = oldStorePassword;
            PlayerSettings.Android.keyaliasName = oldAlias;
            PlayerSettings.Android.keyaliasPass = oldAliasPassword;
        }
    }

    static string Secret(string name) => Environment.GetEnvironmentVariable(name) is string value && value.Length > 0
        ? value : throw new InvalidOperationException("Güvenli build değişkeni eksik: " + name);
}
