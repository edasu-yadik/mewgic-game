using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;using UnityEngine.UI;using UnityEditor.AddressableAssets;using UnityEditor.AddressableAssets.Settings.GroupSchemas;using Mewgic.Map;
public static class Part13GSetup
{
 [Serializable] public class AlphaRow {public string path;public int width,height,transparentPixels,softAlphaPixels,edgePixelsAbove8;public int[] bounds;}
 [Serializable] public class AlphaReport {public AlphaRow[] rows;}
 const string Root="Assets/Art/Environment/Map/Islands/starlight/";
 public static void Run()
 {
  if(EditorApplication.isPlaying)throw new Exception("Play Mode durdurulmalı");
  Import(Root+"IslandBase/Starlight_Island_v001.png",2048);Import(Root+"Vegetation/Starlight_Tree_v001.png",1024);Import(Root+"Architecture/Starlight_Cottages_v001.png",512);Import(Root+"ForegroundEffects/Starlight_Petal_v001.png",256);
  var island=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"IslandBase/Starlight_Island_v001.png");var tree=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Vegetation/Starlight_Tree_v001.png");
  var ground=Mat("Starlight_Ground",0);var water=Mat("Starlight_Water",1);var light=Mat("Starlight_Lights",2);
  var go=new GameObject("StarlightGrove",typeof(RectTransform),typeof(CanvasGroup),typeof(StarlightIslandPresentation));
  try{
   var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
   var p=go.GetComponent<StarlightIslandPresentation>();go.GetComponent<CanvasGroup>().blocksRaycasts=false;go.GetComponent<CanvasGroup>().interactable=false;
   foreach(var n in new[]{"Background","IslandBase","Architecture","Vegetation","WaterOrSpecialEffects","Decorations","LevelNodes","Rewards","ForegroundEffects"})Layer(rect,n);
   var baseImage=Art(rect.Find("IslandBase"),"Starlight_Island",island);baseImage.material=ground;Ground(baseImage);
   var treeImage=Art(rect.Find("Vegetation"),"GreatFloweringTree",tree);Tree(treeImage);
   p.water=Art(rect.Find("WaterOrSpecialEffects"),"WaterfallFlow",island);p.water.material=water;Ground(p.water);

   var cottages=Art(rect.Find("Architecture"),"FlowerCottages",AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Architecture/Starlight_Cottages_v001.png"));Place(cottages,.23f,.50f,185,185);
   p.treeGlow=Art(rect.Find("Decorations"),"GreatTreeGlow",tree);p.treeGlow.material=light;Tree(p.treeGlow);
   var atmosphere=Layer(rect.Find("Background"),"VioletAtmosphere").gameObject.AddComponent<StarlightEffectGraphic>();atmosphere.halo=true;atmosphere.color=new Color(.40f,.14f,.65f,.30f);atmosphere.raycastTarget=false;PlaceGraphic(atmosphere.rectTransform,.5f,.72f,670,430);
   var petalSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"ForegroundEffects/Starlight_Petal_v001.png");p.petals=new RectTransform[6];
   for(int i=0;i<6;i++){var petal=Art(rect.Find("ForegroundEffects"),"Petal_"+i,petalSprite);Place(petal,.22f+i*.115f,.72f+(i%3)*.035f,i%2==0?12:9,i%2==0?12:9);petal.rectTransform.pivot=new Vector2(.5f,.5f);p.petals[i]=petal.rectTransform;}
   p.stars=new RectTransform[4];for(int i=0;i<4;i++){var star=Layer(rect.Find("ForegroundEffects"),"Star_"+i).gameObject.AddComponent<StarlightEffectGraphic>();star.raycastTarget=false;PlaceGraphic(star.rectTransform,.22f+i*.18f,.66f+(i%2)*.13f,i%2==0?8:6,i%2==0?8:6);p.stars[i]=star.rectTransform;}
   PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Map/StarlightGrove.prefab");
  }finally{UnityEngine.Object.DestroyImmediate(go);}
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-StarlightGrove");if(group==null)group=settings.CreateGroup("Map-StarlightGrove",false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
  var schema=group.GetSchema<BundledAssetGroupSchema>();schema.BuildPath.SetVariableByName(settings,"Local.BuildPath");schema.LoadPath.SetVariableByName(settings,"Local.LoadPath");schema.BundleMode=BundledAssetGroupSchema.BundlePackingMode.PackTogether;schema.IncludeInBuild=true;
  settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID("Assets/Prefabs/Map/StarlightGrove.prefab"),group).address="Map/starlight";EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();AuditAlpha();
 }
 static void Ground(Image image){var r=image.rectTransform;r.anchorMin=new Vector2(.05f,-.09f);r.anchorMax=new Vector2(.95f,.81f);r.offsetMin=r.offsetMax=Vector2.zero;}
 static void Tree(Image image)=>Place(image,.60f,.50f,340,340);
 static void PlaceGraphic(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(x,y);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=Vector2.zero;}
 static void Place(Image image,float x,float y,float w,float h){var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(x,y);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=Vector2.zero;}
 static RectTransform Layer(Transform p,string name) {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 static Image Art(Transform parent,string name,Sprite sprite) {var image=Layer(parent,name).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
 static Material Mat(string name,int mode) {
  string path=Root+"WaterOrSpecialEffects/"+name+".mat";
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("Mewgic/StarlightIslandLayers"));AssetDatabase.CreateAsset(m,path);}
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
  foreach(var path in new[]{Root+"IslandBase/Starlight_Island_v001.png",Root+"Vegetation/Starlight_Tree_v001.png",Root+"Architecture/Starlight_Cottages_v001.png",Root+"ForegroundEffects/Starlight_Petal_v001.png"}) {
   var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(tex,File.ReadAllBytes(path));var pixels=tex.GetPixels32();int transparent=0,edge=0,semi=0;int minX=tex.width,minY=tex.height,maxX=-1,maxY=-1;
   for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){int a=pixels[y*tex.width+x].a;if(a==0)transparent++;if(a>0&&a<255)semi++;if(a>8){minX=Math.Min(x,minX);minY=Math.Min(y,minY);maxX=Math.Max(x,maxX);maxY=Math.Max(y,maxY);if(x==0||y==0||x==tex.width-1||y==tex.height-1)edge++;}}
   rows.Add(new AlphaRow{path=path,width=tex.width,height=tex.height,transparentPixels=transparent,softAlphaPixels=semi,edgePixelsAbove8=edge,bounds=new[]{minX,minY,maxX,maxY}});
   UnityEngine.Object.DestroyImmediate(tex);
  }
  File.WriteAllText("docs/ProductionReports/Part13G/ALPHA_AUDIT.json",(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{new AlphaReport{rows=rows.ToArray()}}));
 }
}
