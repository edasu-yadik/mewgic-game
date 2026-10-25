using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEngine.U2D;using UnityEditor;using UnityEditor.U2D;using UnityEditor.SceneManagement;using Mewgic.Map;
public static class MapSceneBuilder
{
    const string Art="Assets/Art/Environment/Map/";
    [MenuItem("Mewgic/Map/Install environment foundation")]
    public static void Install()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play first");
        var audits=new List<Audit>();
        foreach(var name in new[]{"Village","Crystal","Moon","Castle","Sky","Chest","Node"})
        {
            string path=Art+"Map_"+name+"_v001.png";if(!File.Exists(path))throw new FileNotFoundException(path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var tex=new Texture2D(2,2);ImageConversion.LoadImage(tex,File.ReadAllBytes(path));var pixels=tex.GetPixels32();int min=255,max=0,partial=0,border=0;
            for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){var a=pixels[y*tex.width+x].a;min=Math.Min(min,a);max=Math.Max(max,a);if(a>0&&a<255)partial++;if((x==0||x==tex.width-1||y==0||y==tex.height-1)&&a>16)border++;}
            if(name!="Sky"&&(min!=0||max<128))throw new InvalidOperationException("Transparent source alpha missing: "+name);
            var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=100;i.alphaIsTransparency=name!="Sky";i.mipmapEnabled=false;i.isReadable=false;i.maxTextureSize=name=="Chest"||name=="Node"?512:2048;i.npotScale=TextureImporterNPOTScale.None;i.filterMode=FilterMode.Bilinear;i.wrapMode=TextureWrapMode.Clamp;i.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;i.SetTextureSettings(settings);
            var android=i.GetPlatformTextureSettings("Android");android.name="Android";android.overridden=true;android.maxTextureSize=i.maxTextureSize;android.format=TextureImporterFormat.ASTC_4x4;android.compressionQuality=100;i.SetPlatformTextureSettings(android);i.SaveAndReimport();
            audits.Add(new Audit{name=name,width=tex.width,height=tex.height,alphaMin=min,alphaMax=max,partialAlpha=partial,borderPixels=border,maxTexture=i.maxTextureSize});UnityEngine.Object.DestroyImmediate(tex);
        }
        BuildAtlas();
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/LevelMap.unity");var world=scene.GetRootGameObjects().Select(g=>g.GetComponent<MapWorld>()).FirstOrDefault(c=>c!=null);
        if(world==null)world=new GameObject("Mewgic Map",typeof(MapWorld)).GetComponent<MapWorld>();
        world.sky=Load("Sky");world.node=Load("Node");world.chest=Load("Chest");world.terrain=new[]{Load("Village"),Load("Crystal"),Load("Moon"),Load("Castle")};world.fog=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/Home/Layer06_Atmosphere_v001.png");world.homeIcon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Navigation/Nav_Home.png");world.waterLightShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/MapWaterLight.shader");
        if(world.fog==null||world.homeIcon==null||world.waterLightShader==null)throw new InvalidOperationException("Shared map references missing");
        EditorUtility.SetDirty(world);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText("docs/PART06A_IMPORT.json",JsonUtility.ToJson(new ImportReport{assets=audits.ToArray()},true));
    }
    static Sprite Load(string name){var path=Art+"Map_"+name+"_v001.png";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null)throw new InvalidOperationException(path);return sprite;}
    static void BuildAtlas()
    {
        string path=Art+"MapProps.spriteatlas";var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);if(atlas==null){atlas=new SpriteAtlas();AssetDatabase.CreateAsset(atlas,path);}
        var old=atlas.GetPackables();if(old.Length>0)atlas.Remove(old);
        atlas.Add(new UnityEngine.Object[]{AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Map_Node_v001.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Map_Chest_v001.png")});
        var packing=atlas.GetPackingSettings();packing.padding=4;packing.enableRotation=false;packing.enableTightPacking=false;atlas.SetPackingSettings(packing);
        var texture=atlas.GetTextureSettings();texture.generateMipMaps=false;texture.readable=false;texture.filterMode=FilterMode.Bilinear;texture.sRGB=true;atlas.SetTextureSettings(texture);
        var settings=atlas.GetPlatformSettings("Android");settings.name="Android";settings.overridden=true;settings.maxTextureSize=1024;settings.format=TextureImporterFormat.ASTC_4x4;atlas.SetPlatformSettings(settings);atlas.SetIncludeInBuild(true);EditorUtility.SetDirty(atlas);
        SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget);
    }
    [Serializable]class Audit{public string name;public int width,height,alphaMin,alphaMax,partialAlpha,borderPixels,maxTexture;}
    [Serializable]class ImportReport{public Audit[] assets;}
}
