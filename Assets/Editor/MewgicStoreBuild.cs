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
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using AndroidAppInfo = UnityEngine.Localization.Platform.Android.AppInfo;

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
        if (report.summary.platform == BuildTarget.Android) VerifyAndroidAppInfo();
    }

#if UNITY_ANDROID
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string launcher = Path.GetFullPath(Path.Combine(path, "..", "launcher", "src", "main"));
        string strings = Path.Combine(launcher, "res", "values", "strings.xml");
        var resources = XDocument.Load(strings);
        var name = resources.Root?.Elements("string").SingleOrDefault(e => (string)e.Attribute("name") == "app_name");
        if (name == null) throw new BuildFailedException("Android varsayılan uygulama adı kaynağı bulunamadı.");
        name.Value = "MeWGic";
        resources.Save(strings);
        if (!removeInternet) return;
        string manifest = Path.Combine(launcher, "AndroidManifest.xml");
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
    public static void ApplyIosIcons() => MewgicIosIcon.Configure();

    public static void ApplyAndroidAppInfo()
    {
        var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
        var collection = LocalizationEditorSettings.GetStringTableCollection("Mewgic");
        if (settings == null || collection == null)
            throw new InvalidOperationException("Yerelleştirme ayarları ve Mewgic tablosu gerekli.");
        foreach (var table in collection.StringTables)
        {
            table.AddEntry("app.display_name", "MeWGic").IsSmart = false;
            EditorUtility.SetDirty(table);
        }
        var info = settings.GetMetadata().GetMetadata<AndroidAppInfo>();
        if (info == null)
        {
            info = new AndroidAppInfo();
            settings.GetMetadata().AddMetadata(info);
        }
        info.DisplayName = new LocalizedString("Mewgic", "app.display_name");
        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    public static void VerifyAndroidAppInfo()
    {
        var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
        var collection = LocalizationEditorSettings.GetStringTableCollection("Mewgic");
        var info = settings?.GetMetadata().GetMetadata<AndroidAppInfo>();
        if (info == null || collection == null ||
            info.DisplayName.TableReference.TableCollectionName != "Mewgic" ||
            info.DisplayName.TableEntryReference.Key != "app.display_name")
            throw new BuildFailedException("Android App Info uygulama adı tablosuna bağlı olmalı.");
        foreach (var locale in LocalizationEditorSettings.GetLocales())
        {
            var table = collection.GetTable(locale.Identifier) as StringTable;
            if (table?.GetEntry("app.display_name")?.Value != "MeWGic")
                throw new BuildFailedException("Android uygulama adı eksik: " + locale.Identifier.Code);
        }
    }

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
