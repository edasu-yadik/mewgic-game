using System;using System.IO;using System.Linq;using UnityEditor;using UnityEngine;using UnityEngine.UI;using UnityEditor.AddressableAssets;using UnityEditor.AddressableAssets.Settings.GroupSchemas;using Mewgic.Map;
public static class Part13FSetup
{
 [Serializable] public class AlphaRow {public string path;public int width,height,transparentPixels,softAlphaPixels,edgePixelsAbove8;public int[] bounds;}
 [Serializable] public class AlphaReport {public AlphaRow[] rows;}
 const string Root="Assets/Art/Environment/Map/Islands/clockwork/";
 public static void Run()
 {
  if(EditorApplication.isPlaying)throw new Exception("Play Mode durdurulmalı");
  Import(Root+"IslandBase/Clockwork_Island_v001.png",2048);Import(Root+"Architecture/Clockwork_Tower_v001.png",1024);Import(Root+"Architecture/Clockwork_Workshops_v001.png",512);Import(Root+"Decorations/Clockwork_Gear_v001.png",512);
  var island=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"IslandBase/Clockwork_Island_v001.png");var palace=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Architecture/Clockwork_Tower_v001.png");
  var ground=Mat("Clockwork_Static",0);var energy=Mat("Clockwork_Energy",1);
  var go=new GameObject("ClockworkIsles",typeof(RectTransform),typeof(CanvasGroup),typeof(ClockworkIslandPresentation));
  try{
   var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
   var p=go.GetComponent<ClockworkIslandPresentation>();go.GetComponent<CanvasGroup>().blocksRaycasts=false;go.GetComponent<CanvasGroup>().interactable=false;
   foreach(var n in new[]{"Background","IslandBase","Architecture","Vegetation","WaterOrSpecialEffects","Decorations","LevelNodes","Rewards","ForegroundEffects"})Layer(rect,n);
   var baseImage=Art(rect.Find("IslandBase"),"Clockwork_Island",island);baseImage.material=ground;Ground(baseImage);
   var palaceImage=Art(rect.Find("Architecture"),"ClockTower",palace);palaceImage.material=ground;Palace(palaceImage);
   p.energy=Art(rect.Find("WaterOrSpecialEffects"),"CrystalEnergy",island);p.energy.material=energy;Ground(p.energy);
   p.towerEnergy=Art(rect.Find("WaterOrSpecialEffects"),"ClockSymbolsGlow",palace);p.towerEnergy.material=energy;Palace(p.towerEnergy);
   var workshops=Art(rect.Find("Architecture"),"ClockmakerWorkshops",AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Architecture/Clockwork_Workshops_v001.png"));Workshop(workshops);
   p.workshopEnergy=Art(rect.Find("WaterOrSpecialEffects"),"WorkshopWindowsGlow",workshops.sprite);p.workshopEnergy.material=energy;Workshop(p.workshopEnergy);
   var gearSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"Decorations/Clockwork_Gear_v001.png");p.gears=new RectTransform[3];
   for(int i=0;i<3;i++){
    var g=Art(rect.Find("Decorations"),"GoldenGear_"+i,gearSprite);Place(g,.35f+i*.153f,.215f,112,112);g.rectTransform.pivot=new Vector2(.5f,.5f);g.rectTransform.localEulerAngles=new Vector3(0,0,i%2==0?0:15);p.gears[i]=g.rectTransform;
   }
   PrefabUtility.SaveAsPrefabAsset(go,"Assets/Prefabs/Map/ClockworkIsles.prefab");
  }finally{UnityEngine.Object.DestroyImmediate(go);}
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-ClockworkIsles");if(group==null)group=settings.CreateGroup("Map-ClockworkIsles",false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
  var schema=group.GetSchema<BundledAssetGroupSchema>();schema.BuildPath.SetVariableByName(settings,"Local.BuildPath");schema.LoadPath.SetVariableByName(settings,"Local.LoadPath");schema.BundleMode=BundledAssetGroupSchema.BundlePackingMode.PackTogether;schema.IncludeInBuild=true;
  settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID("Assets/Prefabs/Map/ClockworkIsles.prefab"),group).address="Map/clockwork";EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();AuditAlpha();
 }
 static void Ground(Image image){var r=image.rectTransform;r.anchorMin=new Vector2(.05f,-.09f);r.anchorMax=new Vector2(.95f,.81f);r.offsetMin=r.offsetMax=Vector2.zero;}
 static void Palace(Image image)=>Place(image,.58f,.53f,320,320);
 static void Workshop(Image image)=>Place(image,.24f,.53f,190,190);
 static void Place(Image image,float x,float y,float w,float h){var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(x,y);r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=Vector2.zero;}
 static RectTransform Layer(Transform p,string name) {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 static Image Art(Transform parent,string name,Sprite sprite) {var image=Layer(parent,name).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
 static Material Mat(string name,int mode) {
  string path=Root+"WaterOrSpecialEffects/"+name+".mat";
  var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("Mewgic/ClockworkIslandLayers"));AssetDatabase.CreateAsset(m,path);}
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
  foreach(var path in new[]{Root+"IslandBase/Clockwork_Island_v001.png",Root+"Architecture/Clockwork_Tower_v001.png",Root+"Architecture/Clockwork_Workshops_v001.png",Root+"Decorations/Clockwork_Gear_v001.png"}) {
   var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);ImageConversion.LoadImage(tex,File.ReadAllBytes(path));var pixels=tex.GetPixels32();int transparent=0,edge=0,semi=0;int minX=tex.width,minY=tex.height,maxX=-1,maxY=-1;
   for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++){int a=pixels[y*tex.width+x].a;if(a==0)transparent++;if(a>0&&a<255)semi++;if(a>8){minX=Math.Min(x,minX);minY=Math.Min(y,minY);maxX=Math.Max(x,maxX);maxY=Math.Max(y,maxY);if(x==0||y==0||x==tex.width-1||y==tex.height-1)edge++;}}
   rows.Add(new AlphaRow{path=path,width=tex.width,height=tex.height,transparentPixels=transparent,softAlphaPixels=semi,edgePixelsAbove8=edge,bounds=new[]{minX,minY,maxX,maxY}});
   UnityEngine.Object.DestroyImmediate(tex);
  }
  File.WriteAllText("docs/ProductionReports/Part13F/ALPHA_AUDIT.json",(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{new AlphaReport{rows=rows.ToArray()}}));
 }
}
