using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Mewgic;

public static class HomeEnvironmentBuilder
{
    public const string Art="Assets/Art/Environment/Home/";
    public static readonly string[] Files={"Layer01_Sky_v001.png","Layer02_DistantIslands_v001.png","Layer03_MainCastle_v001.png","Layer04_Village_v001.png","Layer05_Foreground_v001.png","Layer06_Atmosphere_v001.png"};
    [MenuItem("Mewgic/Home/Install environment references")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop Play first");
        foreach(var file in Files)
        {
            var path=Art+file;
            if(!File.Exists(path))throw new FileNotFoundException(path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.sRGBTexture=true;importer.isReadable=false;importer.maxTextureSize=2048;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;
            android.maxTextureSize=2048;android.format=TextureImporterFormat.ASTC_6x6;
            android.compressionQuality=100;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
        }
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
        var root=GameObject.Find("Home");var env=root.GetComponent<HomeEnvironment>();
        if(env==null)env=root.AddComponent<HomeEnvironment>();
        env.sky=AssetDatabase.LoadAssetAtPath<Sprite>(Art+Files[0]);
        env.distantIslands=AssetDatabase.LoadAssetAtPath<Sprite>(Art+Files[1]);
        env.mainCastle=AssetDatabase.LoadAssetAtPath<Sprite>(Art+Files[2]);
        env.village=AssetDatabase.LoadAssetAtPath<Sprite>(Art+Files[3]);
        env.foreground=AssetDatabase.LoadAssetAtPath<Sprite>(Art+Files[4]);
        env.atmosphere=AssetDatabase.LoadAssetAtPath<Sprite>(Art+Files[5]);
        env.waterLightShader=Shader.Find("Mewgic/HomeWaterLight");
        if(env.waterLightShader==null)throw new Exception("Environment shader missing");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
}
