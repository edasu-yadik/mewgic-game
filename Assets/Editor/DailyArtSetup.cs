using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;using Mewgic.Daily;
public static class DailyArtSetup
{
 public static void Install()
 {
  const string root="Assets/Art/UI/Daily/";const string config="Assets/Resources/Daily/Art.asset";
  var art=AssetDatabase.LoadAssetAtPath<DailyArt>(config);if(art==null){art=ScriptableObject.CreateInstance<DailyArt>();AssetDatabase.CreateAsset(art,config);}
  foreach(var name in new[]{"Chest","Parchment","Button"}){
   string p=root+"Daily_"+name+"_v001.png";AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);var tex=new Texture2D(2,2);ImageConversion.LoadImage(tex,File.ReadAllBytes(p));var pixels=tex.GetPixels32();int left=tex.width,bottom=tex.height,right=0,top=0,min=255,max=0;
   for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){int alpha=pixels[y*tex.width+x].a;min=Math.Min(min,alpha);max=Math.Max(max,alpha);if(alpha>8){left=Math.Min(left,x);bottom=Math.Min(bottom,y);right=Math.Max(right,x);top=Math.Max(top,y);}}
   if(min!=0||max<200)throw new Exception("Alpha missing: "+name);left=Math.Max(0,left-4);bottom=Math.Max(0,bottom-4);right=Math.Min(tex.width-1,right+4);top=Math.Min(tex.height-1,top+4);
   var i=(TextureImporter)AssetImporter.GetAtPath(p);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=100;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.isReadable=false;i.maxTextureSize=2048;i.npotScale=TextureImporterNPOTScale.None;i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Bilinear;i.textureCompression=TextureImporterCompression.Uncompressed;var android=i.GetPlatformTextureSettings("Android");android.name="Android";android.overridden=true;android.maxTextureSize=2048;android.format=TextureImporterFormat.ASTC_4x4;i.SetPlatformTextureSettings(android);i.SaveAndReimport();
   var imported=AssetDatabase.LoadAssetAtPath<Texture2D>(p);var old=AssetDatabase.LoadAllAssetsAtPath(config).OfType<Sprite>().FirstOrDefault(s=>s.name=="Daily"+name);if(old!=null)UnityEngine.Object.DestroyImmediate(old,true);
   var sprite=Sprite.Create(imported,new Rect(left,bottom,right-left+1,top-bottom+1),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,name=="Chest"?Vector4.zero:Vector4.one*90);sprite.name="Daily"+name;AssetDatabase.AddObjectToAsset(sprite,art);
   if(name=="Chest")art.chest=sprite;else if(name=="Button")art.button=sprite;else art.parchment=sprite;UnityEngine.Object.DestroyImmediate(tex);
  }
  art.sky=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/Home/Layer01_Sky_v001.png");art.castle=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/Home/Layer03_MainCastle_v001.png");art.star=Resources.Load<Sprite>("Gameplay/HUD/HUD_Star_v001");art.catPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/WizardCat.prefab");art.rewards=new[]{"Undo","Hint","Extra","Shuffle"}.Select(n=>Resources.Load<Sprite>("Gameplay/Tools/Tool_"+n+"_v001")).ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
 }
}
