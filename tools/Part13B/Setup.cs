using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using Mewgic.Map;

public static class Part13BSetup
{
 [Serializable] public class AlphaRow {public string path;public int width,height,transparentPixels,softAlphaPixels,edgePixelsAbove8;public int[] bounds;}
 [Serializable] public class AlphaReport {public AlphaRow[] rows;}
 const string Root="Assets/Art/Environment/Map/Islands/woods/";
 public static void Run()
 {
  if(EditorApplication.isPlaying) throw new Exception("Play Mode durdurulmalı");
  Import(Root+"IslandBase/Woods_Island_v002.png",2048);
  Import(Root+"ForegroundEffects/Woods_Mist_v001.png",1024);
  var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"IslandBase/Woods_Island_v002.png");
  var mist=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"ForegroundEffects/Woods_Mist_v001.png");
  Directory.CreateDirectory(Root+"WaterOrSpecialEffects");
  var water=Mat("Woods_Water",0);var light=Mat("Woods_Lights",1);
  var go=new GameObject("WhisperingWoods",typeof(RectTransform),typeof(CanvasGroup),typeof(WoodsIslandPresentation));
  try {
   var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
   var p=go.GetComponent<WoodsIslandPresentation>();go.GetComponent<CanvasGroup>().blocksRaycasts=false;go.GetComponent<CanvasGroup>().interactable=false;
   var names=new[]{"Background","IslandBase","Architecture","Vegetation","WaterOrSpecialEffects","Decorations","LevelNodes","Rewards","ForegroundEffects"};
   foreach(var name in names) Layer(rect,name);
   Art(rect.Find("IslandBase"),"Woods_Island",sprite);
   p.water=Art(rect.Find("WaterOrSpecialEffects"),"WaterfallFlow",sprite);p.water.material=water;
   p.lights=Art(rect.Find("Decorations"),"GoldenWindowBreathing",sprite);p.lights.material=light;
   p.mist=Art(rect.Find("ForegroundEffects"),"ForestMist",mist);p.mist.color=new Color(.7f,.86f,1,.08f);
   var flies=new RectTransform[12];
   for(int i=0;i<flies.Length;i++) {
    var f=new GameObject("Firefly_"+i,typeof(RectTransform),typeof(CanvasRenderer),typeof(WoodsFireflyGraphic));
    var r=(RectTransform)f.transform;r.SetParent(rect.Find("ForegroundEffects"),false);
    r.anchorMin=r.anchorMax=new Vector2(.16f+(i*17%70)/100f,.43f+(i*13%38)/100f);r.sizeDelta=Vector2.one*(i%3==0?7:5);
    f.GetComponent<WoodsFireflyGraphic>().raycastTarget=false;flies[i]=r;
   }
   p.fireflies=flies;
   PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Map/WhisperingWoods.prefab");
  } finally { UnityEngine.Object.DestroyImmediate(go); }
  var settings=AddressableAssetSettingsDefaultObject.Settings;
  var group=settings.FindGroup("Map-WhisperingWoods");
  if(group==null)group=settings.CreateGroup("Map-WhisperingWoods",false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
  var schema=group.GetSchema<BundledAssetGroupSchema>();schema.BuildPath.SetVariableByName(settings,"Local.BuildPath");schema.LoadPath.SetVariableByName(settings,"Local.LoadPath");schema.BundleMode=BundledAssetGroupSchema.BundlePackingMode.PackTogether;schema.IncludeInBuild=true;
  var entry=settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID("Assets/Prefabs/Map/WhisperingWoods.prefab"),group);entry.address="Map/woods";
  EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();
  AuditAlpha();
 }
 static RectTransform Layer(Transform p,string name) {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 static Image Art(Transform parent,string name,Sprite sprite) {var image=Layer(parent,name).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
 static Material Mat(string name,int mode) {
  string path=Root+"WaterOrSpecialEffects/"+name+".mat";
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("Mewgic/WoodsIslandEffects"));AssetDatabase.CreateAsset(m,path);}
  m.SetFloat("_EffectMode",mode);EditorUtility.SetDirty(m);return m;
 }
 static void Import(string path,int max) {
  var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
  importer.spritePixelsPerUnit=100;var ts=new TextureImporterSettings();importer.ReadTextureSettings(ts);ts.spriteAlignment=9;ts.spritePivot=new Vector2(.5f,0);ts.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(ts);importer.alphaIsTransparency=true;importer.isReadable=false;importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=max;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  foreach(string platform in new[]{"Android","iPhone"}){var ps=importer.GetPlatformTextureSettings(platform);ps.name=platform;ps.overridden=true;ps.maxTextureSize=max;ps.format=TextureImporterFormat.ASTC_6x6;ps.compressionQuality=100;importer.SetPlatformTextureSettings(ps);}
  importer.SaveAndReimport();
 }
 public static void AuditAlpha() {
  var rows=new System.Collections.Generic.List<AlphaRow>();
  foreach(var path in new[]{Root+"IslandBase/Woods_Island_v002.png",Root+"ForegroundEffects/Woods_Mist_v001.png"}) {
   var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(tex,File.ReadAllBytes(path));var pixels=tex.GetPixels32();int transparent=0,edge=0,semi=0;int minX=tex.width,minY=tex.height,maxX=-1,maxY=-1;
   for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){int a=pixels[y*tex.width+x].a;if(a==0)transparent++;if(a>0&&a<255)semi++;if(a>8){minX=Math.Min(x,minX);minY=Math.Min(y,minY);maxX=Math.Max(x,maxX);maxY=Math.Max(y,maxY);if(x==0||y==0||x==tex.width-1||y==tex.height-1)edge++;}}
   rows.Add(new AlphaRow{path=path,width=tex.width,height=tex.height,transparentPixels=transparent,softAlphaPixels=semi,edgePixelsAbove8=edge,bounds=new[]{minX,minY,maxX,maxY}});
   UnityEngine.Object.DestroyImmediate(tex);
  }
  File.WriteAllText("docs/ProductionReports/Part13B/ALPHA_AUDIT.json",(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{new AlphaReport{rows=rows.ToArray()}}));
 }
}
