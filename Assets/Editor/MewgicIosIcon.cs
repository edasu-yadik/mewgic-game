using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Yalnız iOS ikonlarını düzenler; sahnelere ve oyun koduna dokunmaz.
public sealed class MewgicIosIcon : IPostprocessBuildWithReport
{
    public const string AssetPath = "Assets/Art/AppIcon/Mewgic_AppIcon_1024_v001.png";
    public int callbackOrder => 10000;

    public static void Configure()
    {
        if (Application.unityVersion != "6000.3.26f1")
            throw new BuildFailedException("iOS ikon doğrulaması Unity 6000.3.26f1 gerektirir.");
        AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(AssetPath);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.alphaIsTransparency = false;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath);
        Require(texture != null && texture.width == 1024 && texture.height == 1024, "1024x1024 ikon gerekli.");
        ValidatePng(File.ReadAllBytes(AssetPath), 1024, 1024);
        var kinds = PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.iOS);
        Require(kinds.Length > 0, "iOS Build Support modülü gerekli.");
        var slots = new List<object>();
        bool marketing = false;
        foreach (var kind in kinds)
        {
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS, kind);
            foreach (var icon in icons)
            {
                icon.SetTextures(Enumerable.Repeat(texture, icon.maxLayerCount).ToArray());
                if (icon.width == 1024 && icon.height == 1024) marketing = true;
                slots.Add(new { kind = kind.ToString(), icon.width, icon.height, icon.maxLayerCount });
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.iOS, kind, icons);
            Require(PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS, kind)
                .All(i => i.GetTextures().Length > 0 && i.GetTextures().All(t => t == texture)), "iOS ikon ataması okunamadı.");
        }
        Require(marketing, "1024x1024 App Store/Marketing yuvası bulunamadı.");
        PlayerSettings.iOS.buildNumber = "2";
        PlayerSettings.bundleVersion = "0.8.0";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.edasuyadik.mewgic");
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("docs/iOSIcon");
        File.WriteAllText("docs/iOSIcon/PLAYER_ICON_CHECK.json", JsonConvert.SerializeObject(new {
            success = true, editor = Application.unityVersion, asset = AssetPath,
            bundle = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS),
            version = PlayerSettings.bundleVersion, build = PlayerSettings.iOS.buildNumber, slots
        }, Formatting.Indented));
        Debug.Log("MEWGIC_ICON_SETTINGS_PASS: 1024x1024 RGB PNG; tüm iOS ikon yuvaları atandı; 0.8.0 (2).");
    }

    public static void ValidatePng(byte[] bytes, int width, int height)
    {
        Require(bytes.Length > 33 && bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71, "PNG imzası geçersiz.");
        int ReadInt(int offset) => (bytes[offset] << 24) | (bytes[offset+1] << 16) | (bytes[offset+2] << 8) | bytes[offset+3];
        Require(ReadInt(16) == width && ReadInt(20) == height, "PNG boyutu geçersiz.");
        Require(bytes[24] == 8 && bytes[25] == 2, "İkon 8-bit RGB PNG olmalı; alpha kanalı bulunmamalı.");
        for (int offset = 8; offset + 12 <= bytes.Length;)
        {
            int length = ReadInt(offset);
            Require(length >= 0 && length <= bytes.Length - offset - 12, "PNG chunk geçersiz.");
            Require(System.Text.Encoding.ASCII.GetString(bytes, offset+4, 4) != "tRNS", "PNG şeffaflık içermemeli.");
            offset += length + 12;
        }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS) ValidateCatalog(report.summary.outputPath);
    }

    public static void ValidateCatalog(string root)
    {
        var catalogs = Directory.GetFiles(root, "Contents.json", SearchOption.AllDirectories)
            .Where(p => new DirectoryInfo(Path.GetDirectoryName(p)).Name == "AppIcon.appiconset").ToArray();
        Require(catalogs.Length == 1, "Tek bir AppIcon.appiconset bulunmalı.");
        var catalog = catalogs[0];
        var images = (JArray)JObject.Parse(File.ReadAllText(catalog))["images"];
        Require(images != null, "AppIcon images eksik.");
        var any = images.OfType<JObject>().Where(i => (string)i["size"] == "1024x1024"
            && (i["appearances"] == null || !i["appearances"].HasValues)
            && ((string)i["idiom"] == "ios-marketing" || (string)i["idiom"] == "universal")).ToArray();
        Require(any.Length > 0, "91111: 1024x1024 Any Appearance eksik.");
        foreach (var entry in any)
        {
            var filename = (string)entry["filename"];
            Require(!string.IsNullOrEmpty(filename) && Path.GetFileName(filename) == filename, "Any Appearance filename eksik/geçersiz.");
            ValidatePng(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(catalog), filename)), 1024, 1024);
        }
        foreach (var entry in images.OfType<JObject>())
            Require(!string.IsNullOrEmpty((string)entry["filename"]) && File.Exists(Path.Combine(Path.GetDirectoryName(catalog), (string)entry["filename"])), "Boş veya eksik AppIcon yuvası.");
        File.WriteAllText(Path.Combine(root, "Mewgic-AppIcon-Verification.json"), JsonConvert.SerializeObject(new {
            success = true, catalog, anyAppearance1024 = any, imageCount = images.Count,
            editor = Application.unityVersion, version = PlayerSettings.bundleVersion,
            build = PlayerSettings.iOS.buildNumber, bundle = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS)
        }, Formatting.Indented));
        Debug.Log("MEWGIC_APPICON_CATALOG_PASS: Any Appearance 1024x1024 RGB PNG dosyası mevcut; 91111 ön kontrolü geçti.");
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new BuildFailedException(message);
    }

    public static void Preflight()
    {
        Configure();
        IosCompatibilityValidation.Run();
        MewgicIosCloudBuild.TestConfiguration();
        var folder = "Temp/MewgicIconNegativeTest/AppIcon.appiconset";
        Directory.CreateDirectory(folder);
        var cases = new[] {
            new JObject { ["idiom"] = "ios-marketing", ["size"] = "1024x1024", ["scale"] = "1x" },
            new JObject { ["idiom"] = "ios-marketing", ["size"] = "1024x1024", ["scale"] = "1x", ["filename"] = "icon.png", ["appearances"] = new JArray(new JObject { ["appearance"] = "luminosity", ["value"] = "dark" }) }
        };
        foreach (var entry in cases)
        {
            File.WriteAllText(Path.Combine(folder, "Contents.json"), new JObject { ["images"] = new JArray(entry) }.ToString());
            bool rejected = false;
            try { ValidateCatalog("Temp/MewgicIconNegativeTest"); }
            catch (BuildFailedException) { rejected = true; }
            Require(rejected, "Eksik Any Appearance kataloğu reddedilmeliydi.");
        }
        File.WriteAllText("docs/iOSIcon/PREFLIGHT.json", new JObject {
            ["success"] = true, ["missingFilenameRejected"] = true, ["darkOnlyRejected"] = true,
            ["modelsAndScenes"] = "docs/IOS_COMPATIBILITY_MODELS.json"
        }.ToString());
        Debug.Log("MEWGIC_ICON_PREFLIGHT_PASS: modeller, sahneler ve iki gerçek 91111 hata senaryosu geçti.");
    }

    public static void ExportXcode()
    {
        Configure();
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Boot", "Home", "Gameplay", "LevelMap" }.Select(n => "Assets/Scenes/"+n+".unity").ToArray(),
            locationPathName = "Builds/iOS/IconBuild2-Xcode", target = BuildTarget.iOS, options = BuildOptions.None
        });
        Require(result.summary.result == BuildResult.Succeeded, "Yerel iOS Xcode export başarısız: " + result.summary.result);
        ValidateCatalog("Builds/iOS/IconBuild2-Xcode");
    }
}
