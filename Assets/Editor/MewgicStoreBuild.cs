using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class MewgicStoreBuild : IPreprocessBuildWithReport
#if UNITY_ANDROID
    , IPostGenerateGradleAndroidProject
#endif
{
    const string Art = "Assets/Art/Store/";
    static readonly XNamespace Android = "http://schemas.android.com/apk/res/android";
    static readonly XNamespace Tools = "http://schemas.android.com/tools";
    static bool removeInternet;
    public int callbackOrder => 500;

    public void OnPreprocessBuild(BuildReport report)
    {
        removeInternet = report.summary.platform == BuildTarget.Android &&
            (report.summary.options & BuildOptions.Development) == 0;
    }

#if UNITY_ANDROID
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        if (!removeInternet) return;
        string manifest = Path.GetFullPath(Path.Combine(path, "..", "launcher", "src", "main", "AndroidManifest.xml"));
        if (!File.Exists(manifest))
            throw new BuildFailedException("Launcher manifesti bulunamadı. Release izinleri doğrulanamadı.");
        RemoveInternet(manifest);
    }

#endif

    static void RemoveInternet(string path)
    {
        var xml = XDocument.Load(path);
        var root = xml.Root ?? throw new InvalidDataException("Manifest kökü eksik.");
        const string permission = "android.permission.INTERNET";
        root.SetAttributeValue(XNamespace.Xmlns + "tools", Tools.NamespaceName);
        foreach (var existing in root.Elements().Where(e =>
            (e.Name == "uses-permission" || e.Name == "uses-permission-sdk-23") &&
            (string)e.Attribute(Android + "name") == permission).ToArray())
            existing.Remove();
        var removal = new XElement("uses-permission",
            new XAttribute(Android + "name", permission), new XAttribute(Tools + "node", "remove"));
        root.AddFirst(removal);
        xml.Save(path);
    }

    public static void ApplyAndroidIcons() => ApplyIcons(NamedBuildTarget.Android);
    public static void ApplyIosIcons() => ApplyIcons(NamedBuildTarget.iOS);

    static void ApplyIcons(NamedBuildTarget target)
    {
        var icon = Texture("Mewgic_Icon1024.png");
        var kinds = PlayerSettings.GetSupportedIconKinds(target);
        if (kinds.Length == 0) throw new InvalidOperationException("Platform ikon yuvaları bulunamadı.");
        foreach (var kind in kinds)
        {
            var slots = PlayerSettings.GetPlatformIcons(target, kind);
            bool adaptive = target == NamedBuildTarget.Android &&
                kind.ToString().IndexOf("Adaptive", StringComparison.OrdinalIgnoreCase) >= 0;
            foreach (var slot in slots)
            {
                if (adaptive)
                {
                    slot.SetTexture(Texture("Mewgic_AdaptiveBackground.png"), 0);
                    slot.SetTexture(Texture("Mewgic_AdaptiveForeground.png"), 1);
                }
                else slot.SetTexture(icon);
            }
            PlayerSettings.SetPlatformIcons(target, kind, slots);
        }
    }

    static Texture2D Texture(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(Art + name)
        ?? throw new FileNotFoundException("Mağaza ikon dokusu eksik.", name);

    public static void ApplyAndVerifyAndroid()
    {
        ApplyAndroidIcons();
        VerifyConfiguration();
        AssetDatabase.SaveAssets();
    }

    public static void VerifyConfiguration()
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path,
                "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\"><uses-permission android:name=\"android.permission.INTERNET\"/><uses-permission android:name=\"android.permission.VIBRATE\"/><application/></manifest>");
            RemoveInternet(path);
            RemoveInternet(path);
            var root = XDocument.Load(path).Root;
            var permissions = root.Elements("uses-permission").ToArray();
            if (permissions.Length != 2 ||
                !permissions.Any(e => (string)e.Attribute(Android + "name") == "android.permission.VIBRATE") ||
                !permissions.Any(e => (string)e.Attribute(Android + "name") == "android.permission.INTERNET" &&
                    (string)e.Attribute(Tools + "node") == "remove"))
                throw new InvalidOperationException("İzin düzenleme regresyonu.");
            var kinds = PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android);
            var slots = kinds.SelectMany(k => PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, k)).ToArray();
            if (slots.Length == 0 || slots.Any(s => s.GetTexture() == null))
                throw new InvalidOperationException("Android ikon yuvası boş.");
            if (PlayerSettings.SplashScreen.show)
                throw new InvalidOperationException("Özel Boot sahnesi için Unity splash ayarı beklenenden farklı.");
            Debug.Log("PART20 mağaza yapılandırması doğrulandı. Android ikon yuvası: " + slots.Length);
        }
        finally { File.Delete(path); }
    }
}
