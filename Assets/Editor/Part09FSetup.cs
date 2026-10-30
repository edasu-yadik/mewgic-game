using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;using TMPro;using UnityEngine.TextCore.LowLevel;using Mewgic.UI;
public static class Part09FSetup
{
    public static void Essential()
    {
        string package=Directory.GetFiles("Library/PackageCache","TMP Essential Resources.unitypackage",SearchOption.AllDirectories).Single();
        AssetDatabase.ImportPackage(package,false);
    }
    static TMP_FontAsset Font(string name)
    {
        var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Typography/"+name+".ttf");
        var tmp=TMP_FontAsset.CreateFontAsset(font,64,8,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic);
        tmp.name=name+" SDF";string chars=new string(Enumerable.Range(32,352).Select(i=>(char)i).ToArray())+"…•–—×✓▼★→←";
        tmp.TryAddCharacters(chars,out string missing);
        foreach(char c in "ÇĞİÖŞÜçğıöşü0123456789")if(!tmp.HasCharacter(c))throw new Exception("Eksik Türkçe glif:"+c);
        string path="Assets/Resources/UI/"+name+".asset";AssetDatabase.CreateAsset(tmp,path);AssetDatabase.AddObjectToAsset(tmp.material,tmp);foreach(var atlas in tmp.atlasTextures)AssetDatabase.AddObjectToAsset(atlas,tmp);
        tmp.atlasPopulationMode=AtlasPopulationMode.Static;EditorUtility.SetDirty(tmp);return tmp;
    }
    public static void Install()
    {
        var theme=ScriptableObject.CreateInstance<MewgicTypographySettings>();theme.heading=Font("MewgicTitle");theme.body=Font("MewgicBody");AssetDatabase.CreateAsset(theme,"Assets/Resources/UI/Typography.asset");
        var style=ScriptableObject.CreateInstance<ProgressBarStyle>();AssetDatabase.CreateAsset(style,"Assets/Resources/UI/ProgressStyle.asset");AssetDatabase.SaveAssets();
        Prefab();
    }
    public static void Prefab()
    {
        EditorUtility.SetDirty(Resources.Load<MewgicTypographySettings>("UI/Typography"));
        var style=Resources.Load<ProgressBarStyle>("UI/ProgressStyle");
        var go=new GameObject("MagicalProgressBar",typeof(RectTransform));((RectTransform)go.transform).sizeDelta=new Vector2(400,32);var bar=go.AddComponent<MagicalProgressBar>();bar.style=style;bar.Build();bar.SetValue(3,5,animate:false);
        Directory.CreateDirectory("Assets/Prefabs/UI");PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/UI/MagicalProgressBar.prefab");UnityEngine.Object.DestroyImmediate(go);AssetDatabase.SaveAssets();
    }
    public static void Models()
    {
        DailyActivitiesTests.Models();RewardHubTests.Models();
        File.WriteAllText("docs/Part09F/MODELS.json","{\"success\":true,\"groups\":[\"DailyActivitiesTests.Models\",\"RewardHubTests.Models\"]}");
        Prefab();
    }
}
