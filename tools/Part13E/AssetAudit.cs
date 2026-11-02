using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
public static class Part13EAssetAudit
{
 public static object Run()
 {
  string root="Assets/Art/Environment/Map/Islands/frost/";
  var paths=new[]{root+"IslandBase/Frost_Island_v001.png",root+"Architecture/Frost_Palace_v001.png",root+"Vegetation/Frost_Trees_v001.png",root+"ForegroundEffects/Frost_Aurora_v001.png"};
  var textures=paths.Select(path=>{
   var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);
   return new{path,t.width,t.height,format=t.format.ToString(),editorRuntimeBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t),readWrite=i.isReadable,mipmap=i.mipmapEnabled,android=i.GetPlatformTextureSettings("Android").format.ToString(),ios=i.GetPlatformTextureSettings("iPhone").format.ToString()};
  }).ToArray();
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/FrostlightPeaks.prefab");
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-FrostlightPeaks");var schema=group.GetSchema<BundledAssetGroupSchema>();
  var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/FrostIslandLayers.shader");var messages=ShaderUtil.GetShaderMessages(shader);
  if(messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))throw new Exception("Shader derleme hatası");
  if(prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Any(g=>g.raycastTarget))throw new Exception("Sanat düğümlere raycast engeli oluşturur");
  if(group.entries.Count!=1||group.entries.First().address!="Map/frost")throw new Exception("Ada başına tek adres sözleşmesi");
  var presentation=prefab.GetComponent<Mewgic.Map.FrostIslandPresentation>();
  var ground=prefab.transform.Find("IslandBase/Frost_Island").GetComponent<UnityEngine.UI.Image>();
  var decoration=prefab.transform.Find("Vegetation/CrystalTrees").GetComponent<UnityEngine.UI.Image>();
  var palace=prefab.transform.Find("Architecture/IcePalace").GetComponent<UnityEngine.UI.Image>();
  bool separate=palace.sprite!=ground.sprite&&presentation.palaceIce.sprite==palace.sprite&&ground.sprite==presentation.ice.sprite&&ground.material.GetFloat("_LayerMode")==0&&presentation.ice.material.GetFloat("_LayerMode")==1&&decoration.sprite!=ground.sprite;
  if(!separate)throw new Exception("Taban/buz/dekor ayrımı bozuk");
  var dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true);
  if(dependencies.Any(d=>d.EndsWith(".png")&&!d.StartsWith(root)))throw new Exception("Diğer ada sanatına bağımlılık oluştu");
  var result=new{success=true,separateGroundIceDecoration=separate,textures,layers=Enumerable.Range(0,prefab.transform.childCount).Select(i=>prefab.transform.GetChild(i).name).ToArray(),snow=prefab.GetComponentsInChildren<Mewgic.Map.FrostSnowGraphic>().Length,group=group.Name,localBuildPath=schema.BuildPath.GetValue(settings),localLoadPath=schema.LoadPath.GetValue(settings),includeInBuild=schema.IncludeInBuild,shaderMessages=messages.Select(m=>new{m.message,severity=m.severity.ToString()}).ToArray(),dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true)};
  string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});
  File.WriteAllText("docs/ProductionReports/Part13E/ASSET_AUDIT.json",json);return result;
 }
}
