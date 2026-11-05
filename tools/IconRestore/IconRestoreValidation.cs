using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class IconRestoreValidation
{
    public static void Run()
    {
        var checks = new JArray();
        try
        {
            string identity = PlayerSettings.productName + PlayerSettings.bundleVersion + PlayerSettings.iOS.buildNumber;
            MewgicIosIcon.Configure();
            MewgicIosIcon.Configure();
            if (identity != PlayerSettings.productName + PlayerSettings.bundleVersion + PlayerSettings.iOS.buildNumber)
                throw new Exception("Identity changed");
            checks.Add("İkon ataması tekrarlanabilir; ürün/sürüm/build korunuyor.");
            var slots = PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.iOS)
                .SelectMany(k => PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS, k)).ToArray();
            if (slots.Length != 19 || slots.Any(s => s.GetTexture() == null)) throw new Exception("19 slots expected");
            checks.Add("19 iOS yuvası gerçek Unity API ile dolu.");
            string folder = "Temp/IconCases/AppIcon.appiconset";
            Directory.CreateDirectory(folder);
            File.Copy(MewgicIosIcon.AssetPath, folder + "/icon.png", true);
            var entry = new JObject { ["idiom"]="ios-marketing", ["size"]="1024x1024", ["scale"]="1x", ["filename"]="icon.png" };
            void Catalog(JObject image) => File.WriteAllText(folder + "/Contents.json", new JObject { ["images"]=new JArray(image) }.ToString());
            void Reject(string label)
            {
                try { MewgicIosIcon.ValidateCatalog("Temp/IconCases"); }
                catch (BuildFailedException) { checks.Add(label); return; }
                throw new Exception("Expected rejection: " + label);
            }
            Catalog(entry); MewgicIosIcon.ValidateCatalog("Temp/IconCases"); checks.Add("Doğru tarihsel katalog kabul edildi.");
            entry.Remove("filename"); Catalog(entry); Reject("Eksik dosya reddedildi.");
            entry["filename"]="icon.png";
            entry["appearances"]=new JArray(new JObject { ["appearance"]="luminosity", ["value"]="dark" });
            Catalog(entry); Reject("Yalnız dark appearance reddedildi.");
            entry.Remove("appearances"); Catalog(entry);
            var wrong = new Texture2D(1024,1024,TextureFormat.RGB24,false);
            ImageConversion.LoadImage(wrong, File.ReadAllBytes(MewgicIosIcon.AssetPath));
            wrong.SetPixel(500,500,Color.green); wrong.Apply();
            File.WriteAllBytes(folder + "/icon.png", wrong.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(wrong);
            Reject("Tek pikseli farklı 1024 RGB ikon reddedildi.");
            File.Copy(MewgicIosIcon.AssetPath, folder + "/icon.png", true);
            MewgicIosIcon.ValidateCatalog("Temp/IconCases");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Camera",typeof(Camera));
            EditorSceneManager.SaveScene(scene,"Assets/IconValidation.unity");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.edasuyadik.mewgic");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
            var build = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{"Assets/IconValidation.unity"},target=BuildTarget.iOS,
                locationPathName="Builds/IconValidation-Xcode",options=BuildOptions.None
            });
            if (build.summary.result != BuildResult.Succeeded) throw new Exception("Xcode export failed");
            MewgicIosIcon.ValidateCatalog("Builds/IconValidation-Xcode");
            checks.Add("Gerçek iOS Xcode export ve marketing ikonunda piksel eşitliği geçti.");
            File.WriteAllText("result.json",new JObject { ["success"]=true,["unity"]=Application.unityVersion,
                ["scope"]="Yalıtılmış ikon projesi; ana oyunun yeni build'i değil",["checks"]=checks,
                ["iosSlots"]=slots.Length,["buildWarnings"]=build.summary.totalWarnings }.ToString());
        }
        catch(Exception e)
        {
            File.WriteAllText("result.json",new JObject { ["success"]=false,["unity"]=Application.unityVersion,["checks"]=checks,["error"]=e.ToString() }.ToString());
            throw;
        }
    }
}
