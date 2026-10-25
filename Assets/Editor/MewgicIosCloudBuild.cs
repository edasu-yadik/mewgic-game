using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Explicit cloud hook only; never changes the local Android target on import.
public static class MewgicIosCloudBuild
{
    public const string ConfigPath="BuildConfig/iOS-TestFlight.json";
    [Serializable] public sealed class Configuration
    {
        public string editorVersion,productName,bundleIdentifier,appleTeamId,marketingVersion,minimumIOSVersion;
        public bool registrationVerified;
    }
    public static string Validate(string bundle,string team,string number,bool registered)
    {
        if(!registered)throw new InvalidOperationException("Apple App ID kaydı doğrulanmadan iOS build başlatılamaz.");
        if(string.IsNullOrWhiteSpace(bundle)||bundle.Length>255||!Regex.IsMatch(bundle,@"^[A-Za-z0-9][A-Za-z0-9-]*(\.[A-Za-z0-9][A-Za-z0-9-]*){2,}$"))throw new ArgumentException("Geçerli iOS Bundle ID gerekli.");
        if(string.IsNullOrWhiteSpace(team)||!Regex.IsMatch(team,@"^[A-Z0-9]{10}$"))throw new ArgumentException("Apple Team ID gerekli.");
        if(string.IsNullOrWhiteSpace(number)||!Regex.IsMatch(number,@"^[0-9]+$")||!int.TryParse(number,NumberStyles.None,CultureInfo.InvariantCulture,out int build)||build<1)throw new ArgumentException("Pozitif ve benzersiz iOS build numarası gerekli.");
        return build.ToString(CultureInfo.InvariantCulture);
    }
    public static void PreExport()
    {
        if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.iOS)throw new InvalidOperationException("Bu hook yalnız bulut iOS hedefinde çalışır; Android ayarları değiştirilmedi.");
        var config=JsonUtility.FromJson<Configuration>(File.ReadAllText(ConfigPath));
        if(config==null||config.editorVersion!=Application.unityVersion)throw new InvalidOperationException("Bulut Unity sürümü proje sürümüyle aynı olmalı.");
        string number=Environment.GetEnvironmentVariable("MEWGIC_IOS_BUILD_NUMBER");
        if(string.IsNullOrWhiteSpace(number))number=Environment.GetEnvironmentVariable("BUILD_NUMBER");
        number=Validate(config.bundleIdentifier,config.appleTeamId,number,config.registrationVerified);
        if(config.productName!="Mewgic"||!Regex.IsMatch(config.marketingVersion??"",@"^[0-9]+\.[0-9]+\.[0-9]+$"))throw new InvalidOperationException("Mewgic sürüm bilgisi geçersiz.");
        var scenes=new[]{"Boot","Home","Gameplay","LevelMap"}.Select(n=>"Assets/Scenes/"+n+".unity").ToArray();
        if(scenes.Any(p=>!File.Exists(p)))throw new InvalidOperationException("Üretim sahnesi eksik.");
        PlayerSettings.productName=config.productName;
        PlayerSettings.bundleVersion=config.marketingVersion;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,config.bundleIdentifier);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
        PlayerSettings.iOS.appleDeveloperTeamID=config.appleTeamId;
        PlayerSettings.iOS.buildNumber=number;
        PlayerSettings.iOS.targetOSVersionString=config.minimumIOSVersion;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
        EditorUserBuildSettings.development=false;
        EditorUserBuildSettings.allowDebugging=false;
        EditorUserBuildSettings.connectProfiler=false;
        EditorBuildSettings.scenes=scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
        Debug.Log("Mewgic iOS cloud pre-export validated. Distribution signing is supplied by cloud credentials.");
    }
    [Serializable] sealed class Report{public bool success;public string error;public string[] checks;public string localTarget;public bool iosSupport;}
    public static void TestConfiguration()
    {
        var checks=new System.Collections.Generic.List<string>();string settings=File.ReadAllText("ProjectSettings/ProjectSettings.asset");
        void Check(bool ok,string label){if(!ok)throw new Exception(label);checks.Add(label);}
        void Reject(Action action,string label){try{action();}catch(ArgumentException){checks.Add(label);return;}catch(InvalidOperationException){checks.Add(label);return;}throw new Exception("Beklenen ret yok: "+label);}
        try{
            Check(Validate("com.edasuyadik.mewgic","VKW7GYV7B9","1",true)=="1","Valid registered identity");
            Check(Validate("com.edasuyadik.mewgic","VKW7GYV7B9","0009",true)=="9","Canonical build number");
            foreach(string bundle in new[]{null,"","0","mewgic","com.mewgic","com..mewgic","com.mewgic game.app",".com.mewgic.game","com.mewgic.game."})Reject(()=>Validate(bundle,"VKW7GYV7B9","1",true),"Reject bundle "+(bundle??"null"));
            foreach(string team in new[]{null,"","123","vkw7gyv7b9","VKW7GYV7B90"})Reject(()=>Validate("com.edasuyadik.mewgic",team,"1",true),"Reject invalid team");
            foreach(string number in new[]{null,"","0","-1","1.0","abc","2147483648"})Reject(()=>Validate("com.edasuyadik.mewgic","VKW7GYV7B9",number,true),"Reject invalid build number");
            Reject(()=>Validate("com.edasuyadik.mewgic","VKW7GYV7B9","1",false),"Unverified registration blocks cloud build");
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.iOS)Reject(PreExport,"Local Android target cannot execute iOS hook");
            Check(settings==File.ReadAllText("ProjectSettings/ProjectSettings.asset"),"All local PlayerSettings preserved");
            Check(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)=="com.mewgic.game","Android identifier retained");
            File.WriteAllText("docs/TESTFLIGHT_PREFLIGHT_TESTS.json",JsonUtility.ToJson(new Report{success=true,checks=checks.ToArray(),localTarget=EditorUserBuildSettings.activeBuildTarget.ToString(),iosSupport=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS)},true));
        }catch(Exception e){File.WriteAllText("docs/TESTFLIGHT_PREFLIGHT_TESTS.json",JsonUtility.ToJson(new Report{success=false,error=e.ToString(),checks=checks.ToArray()},true));throw;}
    }
}
