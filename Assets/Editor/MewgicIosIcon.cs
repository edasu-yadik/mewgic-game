using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Yalnız iOS ikon kimliği; Android, sürüm, imza ve sahneler değişmez.
public sealed class MewgicIosIcon : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public const string AssetPath = "Assets/Art/AppIcon/Mewgic_AppIcon_1024_v001.png";
    public const string SourceSha256 = "bcb3f10d1dfbb6d58088b320da1f1780915aa6e154b7aab6e0f95879ae3e0410";
    public int callbackOrder => 10000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS) Configure();
    }

    [MenuItem("Mewgic/Branding/Apply and Verify Original iOS Icon")]
    public static void Configure()
    {
        using (var sha = SHA256.Create())
            Require(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(AssetPath))).Replace("-", "").ToLowerInvariant() == SourceSha256,
                "Tarihsel iOS ikon dosyası değişmiş.");
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
        var platform = importer.GetPlatformTextureSettings("iPhone");
        platform.overridden = true;
        platform.maxTextureSize = 1024;
        platform.format = TextureImporterFormat.RGB24;
        platform.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SetPlatformTextureSettings(platform);
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath);
        Require(texture != null && texture.width == 1024 && texture.height == 1024, "1024x1024 ikon gerekli.");
        var kinds = PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.iOS);
        Require(kinds.Length > 0, "iOS Build Support modülü gerekli.");
        bool marketing = false;
        int count = 0;
        foreach (var kind in kinds)
        {
            var slots = PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS, kind);
            foreach (var slot in slots)
            {
                slot.SetTextures(Enumerable.Repeat(texture, slot.maxLayerCount).ToArray());
                marketing |= slot.width == 1024 && slot.height == 1024;
                count++;
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.iOS, kind, slots);
            Require(PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS, kind)
                .All(s => s.GetTextures().Length > 0 && s.GetTextures().All(t => t == texture)), "iOS ikon ataması doğrulanamadı.");
        }
        Require(marketing, "1024x1024 App Store ikon yuvası eksik.");
        AssetDatabase.SaveAssets();
        Debug.Log("MEWGIC_ORIGINAL_IOS_ICON_PASS " + SourceSha256 + " slots=" + count);
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS) ValidateCatalog(report.summary.outputPath);
    }

    public static void ValidateCatalog(string root)
    {
        var catalogs = Directory.GetFiles(root, "Contents.json", SearchOption.AllDirectories)
            .Where(p => new DirectoryInfo(Path.GetDirectoryName(p)).Name == "AppIcon.appiconset").ToArray();
        Require(catalogs.Length == 1, "Tek AppIcon.appiconset gerekli.");
        var folder = Path.GetDirectoryName(catalogs[0]);
        var images = JObject.Parse(File.ReadAllText(catalogs[0]))["images"] as JArray;
        Require(images != null && images.Count > 0, "AppIcon kataloğu boş.");
        bool marketing = false;
        foreach (var entry in images.OfType<JObject>())
        {
            string filename = (string)entry["filename"];
            Require(!string.IsNullOrEmpty(filename) && Path.GetFileName(filename) == filename &&
                File.Exists(Path.Combine(folder, filename)), "AppIcon dosyası eksik/geçersiz.");
            if ((string)entry["size"] != "1024x1024" ||
                ((string)entry["idiom"] != "ios-marketing" && (string)entry["idiom"] != "universal") ||
                (entry["appearances"] != null && entry["appearances"].HasValues)) continue;
            var actual = new Texture2D(2, 2);
            var expected = new Texture2D(2, 2);
            try
            {
                byte[] png = File.ReadAllBytes(Path.Combine(folder, filename));
                Require(png.Length > 25 && png[24] == 8 && png[25] == 2, "Marketing ikonu 8-bit RGB PNG olmalı.");
                Require(ImageConversion.LoadImage(actual, png) && ImageConversion.LoadImage(expected, File.ReadAllBytes(AssetPath)), "İkon PNG okunamadı.");
                Require(actual.width == 1024 && actual.height == 1024 && actual.GetPixels32().SequenceEqual(expected.GetPixels32()),
                    "Xcode marketing ikonu tarihsel görselle eşleşmiyor.");
                marketing = true;
            }
            finally { UnityEngine.Object.DestroyImmediate(actual); UnityEngine.Object.DestroyImmediate(expected); }
        }
        Require(marketing, "1024x1024 Any Appearance ikonu eksik.");
        Debug.Log("MEWGIC_ORIGINAL_IOS_CATALOG_PASS " + SourceSha256);
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new BuildFailedException(message);
    }
}
