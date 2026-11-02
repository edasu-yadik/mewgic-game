using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;using UnityEngine.UI;using UnityEditor.AddressableAssets;using UnityEditor.AddressableAssets.Settings.GroupSchemas;using Mewgic.Map;
public static class Part13DSetup
{
 [Serializable] public class AlphaRow {public string path;public int width,height,transparentPixels,softAlphaPixels,edgePixelsAbove8;public int[] bounds;}
 [Serializable] public class AlphaReport {public AlphaRow[] rows;}
 const string Root="Assets/Art/Environment/Map/Islands/coral/";
 public static void Run()
 {
  if(EditorApplication.isPlaying)throw new Exception("Play Mode durdurulmalı");
  Import(Root+"IslandBase/Coral_Island_v001.png",2048);Import(Root+"Architecture/Coral_Palace_v001.png",1024);Import(Root+"Vegetation/Coral_Cluster_v001.png",512);Import(Root+"ForegroundEffects/Coral_Rays_v001.png",512);
  var island=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"IslandBase/Coral_Island_v001.png");var palace=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Architecture/Coral_Palace_v001.png");
  var ground=Mat("Coral_Ground",0);var water=Mat("Coral_Water",1);var light=Mat("Coral_Lights",2);
  var go=new GameObject("CoralKingdom",typeof(RectTransform),typeof(CanvasGroup),typeof(CoralIslandPresentation));
  try{
   var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
   var p=go.GetComponent<CoralIslandPresentation>();go.GetComponent<CanvasGroup>().blocksRaycasts=false;go.GetComponent<CanvasGroup>().interactable=false;
   foreach(var n in new[]{"Background","IslandBase","Architecture","Vegetation","WaterOrSpecialEffects","Decorations","LevelNodes","Rewards","ForegroundEffects"})Layer(rect,n);
   var baseImage=Art(rect.Find("IslandBase"),"Coral_Island",island);baseImage.material=ground;Ground(baseImage);
   var palaceImage=Art(rect.Find("Architecture"),"PearlPalace",palace);palaceImage.material=ground;Palace(palaceImage);
   p.water=Art(rect.Find("WaterOrSpecialEffects"),"WaterfallFlow",island);p.water.material=water;Ground(p.water);
   p.palaceWater=Art(rect.Find("WaterOrSpecialEffects"),"CrystalWaterArches",palace);p.palaceWater.material=water;Palace(p.palaceWater);
   var corals=Art(rect.Find("Vegetation"),"CoralGarden",AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Vegetation/Coral_Cluster_v001.png"));Place(corals,.18f,.45f,130,130);
   p.lights=Art(rect.Find("Decorations"),"PearlLightBreathing",palace);p.lights.material=light;Palace(p.lights);
   p.rays=Art(rect.Find("ForegroundEffects"),"SoftLightShafts",AssetDatabase.LoadAssetAtPath<Sprite>(Root+"ForegroundEffects/Coral_Rays_v001.png"));p.rays.color=new Color(.72f,.95f,1,.035f);
   p.bubbles=new RectTransform[8];for(int i=0;i<8;i++){
    var bubble=new GameObject("Bubble_"+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(CoralBubbleGraphic));var r=(RectTransform)bubble.transform;r.SetParent(rect.Find("ForegroundEffects"),false);r.anchorMin=r.anchorMax=new Vector2(.18f+(i*17%65)/100f,.32f+(i*7%20)/100f);r.sizeDelta=Vector2.one*(i%3==0?10:6);bubble.GetComponent<CoralBubbleGraphic>().raycastTarget=false;p.bubbles[i]=r;
   }
   PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Map/CoralKingdom.prefab");
  }finally{UnityEngine.Object.DestroyImmediate(go);}
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-CoralKingdom");if(group==null)group=settings.CreateGroup("Map-CoralKingdom",false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
  var schema=group.GetSchema<BundledAssetGroupSchema>();schema.BuildPath.SetVariableByName(settings,"Local.BuildPath");schema.LoadPath.SetVariableByName(settings,"Local.LoadPath");schema.BundleMode=BundledAssetGroupSchema.BundlePackingMode.PackTogether;schema.IncludeInBuild=true;
  settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID("Assets/Prefabs/Map/CoralKingdom.prefab"),group).address="Map/coral";EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();AuditAlpha();
 }
 static void Ground(Image image){var r=image.rectTransform;r.anchorMin=new Vector2(.05f,-.09f);r.anchorMax=new Vector2(.95f,.81f);r.offsetMin=r.offsetMax=Vector2.zero;}
 static void Palace(Image image)=>Place(image,.55f,.47f,360,360);
 static void Place(Image image,float x,float y,float w,float h){var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(x,y);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=Vector2.zero;}
 static RectTransform Layer(Transform p,string name) {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 static Image Art(Transform parent,string name,Sprite sprite) {var image=Layer(parent,name).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
 static Material Mat(string name,int mode) {
  string path=Root+"WaterOrSpecialEffects/"+name+".mat";
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("Mewgic/CoralIslandLayers"));AssetDatabase.CreateAsset(m,path);}
  m.SetFloat("_LayerMode",mode);EditorUtility.SetDirty(m);return m;
 }
 static void Import(string path,int max) {
  var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
  importer.spritePixelsPerUnit=100;var ts=new TextureImporterSettings();importer.ReadTextureSettings(ts);ts.spriteAlignment=9;ts.spritePivot=new Vector2(.5f,0);ts.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(ts);importer.alphaIsTransparency=true;importer.isReadable=false;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=max;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  foreach(string platform in new[]{"Android","iPhone"}){var ps=importer.GetPlatformTextureSettings(platform);ps.name=platform;ps.overridden=true;ps.maxTextureSize=max;ps.format=TextureImporterFormat.ASTC_6x6;ps.compressionQuality=100;importer.SetPlatformTextureSettings(ps);}
  importer.SaveAndReimport();
 }
 public static void AuditAlpha() {
  var rows=new System.Collections.Generic.List<AlphaRow>();
  foreach(var path in new[]{Root+"IslandBase/Coral_Island_v001.png",Root+"Architecture/Coral_Palace_v001.png",Root+"Vegetation/Coral_Cluster_v001.png",Root+"ForegroundEffects/Coral_Rays_v001.png"}) {
   var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(tex,File.ReadAllBytes(path));var pixels=tex.GetPixels32();int transparent=0,edge=0,semi=0;int minX=tex.width,minY=tex.height,maxX=-1,maxY=-1;
   for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){int a=pixels[y*tex.width+x].a;if(a==0)transparent++;if(a>0&&a<255)semi++;if(a>8){minX=Math.Min(x,minX);minY=Math.Min(y,minY);maxX=Math.Max(x,maxX);maxY=Math.Max(y,maxY);if(x==0||y==0||x==tex.width-1||y==tex.height-1)edge++;}}
   rows.Add(new AlphaRow{path=path,width=tex.width,height=tex.height,transparentPixels=transparent,softAlphaPixels=semi,edgePixelsAbove8=edge,bounds=new[]{minX,minY,maxX,maxY}});
   UnityEngine.Object.DestroyImmediate(tex);
  }
  File.WriteAllText("docs/ProductionReports/Part13D/ALPHA_AUDIT.json",(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{new AlphaReport{rows=rows.ToArray()}}));
 }
}
