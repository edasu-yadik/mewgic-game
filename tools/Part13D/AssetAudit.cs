using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
public static class Part13DAssetAudit
{
 public static object Run()
 {
  string root="Assets/Art/Environment/Map/Islands/coral/";
  var paths=new[]{root+"IslandBase/Coral_Island_v001.png",root+"Architecture/Coral_Palace_v001.png",root+"Vegetation/Coral_Cluster_v001.png",root+"ForegroundEffects/Coral_Rays_v001.png"};
  var textures=paths.Select(path=>{
   var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);
   return new{path,t.width,t.height,format=t.format.ToString(),editorRuntimeBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t),readWrite=i.isReadable,mipmap=i.mipmapEnabled,android=i.GetPlatformTextureSettings("Android").format.ToString(),ios=i.GetPlatformTextureSettings("iPhone").format.ToString()};
  }).ToArray();
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/CoralKingdom.prefab");
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-CoralKingdom");var schema=group.GetSchema<BundledAssetGroupSchema>();
  var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/CoralIslandLayers.shader");var messages=ShaderUtil.GetShaderMessages(shader);
  if(messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))throw new Exception("Shader derleme hatası");
  if(prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Any(g=>g.raycastTarget))throw new Exception("Sanat düğümlere raycast engeli oluşturur");
  if(group.entries.Count!=1||group.entries.First().address!="Map/coral")throw new Exception("Ada başına tek adres sözleşmesi");
  var presentation=prefab.GetComponent<Mewgic.Map.CoralIslandPresentation>();
  var ground=prefab.transform.Find("IslandBase/Coral_Island").GetComponent<UnityEngine.UI.Image>();
  var decoration=prefab.transform.Find("Vegetation/CoralGarden").GetComponent<UnityEngine.UI.Image>();
  var palace=prefab.transform.Find("Architecture/PearlPalace").GetComponent<UnityEngine.UI.Image>();
  bool separate=palace.sprite!=ground.sprite&&presentation.palaceWater.sprite==palace.sprite&&ground.sprite==presentation.water.sprite&&ground.material.GetFloat("_LayerMode")==0&&presentation.water.material.GetFloat("_LayerMode")==1&&decoration.sprite!=ground.sprite;
  if(!separate)throw new Exception("Taban/su/dekor ayrımı bozuk");
  var dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true);
  if(dependencies.Any(d=>d.EndsWith(".png")&&!d.StartsWith(root)))throw new Exception("Diğer ada sanatına bağımlılık oluştu");
  var result=new{success=true,separateGroundWaterDecoration=separate,textures,layers=Enumerable.Range(0,prefab.transform.childCount).Select(i=>prefab.transform.GetChild(i).name).ToArray(),bubbles=prefab.GetComponentsInChildren<Mewgic.Map.CoralBubbleGraphic>().Length,group=group.Name,localBuildPath=schema.BuildPath.GetValue(settings),localLoadPath=schema.LoadPath.GetValue(settings),includeInBuild=schema.IncludeInBuild,shaderMessages=messages.Select(m=>new{m.message,severity=m.severity.ToString()}).ToArray(),dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true)};
  string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});
  File.WriteAllText("docs/ProductionReports/Part13D/ASSET_AUDIT.json",json);return result;
 }
}
