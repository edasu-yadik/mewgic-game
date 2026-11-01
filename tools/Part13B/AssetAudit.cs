using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
public static class Part13BAssetAudit
{
 public static object Run()
 {
  string root="Assets/Art/Environment/Map/Islands/woods/";
  var paths=new[]{root+"IslandBase/Woods_Island_v002.png",root+"ForegroundEffects/Woods_Mist_v001.png"};
  var textures=paths.Select(path=>{
   var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);
   return new{path,t.width,t.height,format=t.format.ToString(),editorRuntimeBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t),readWrite=i.isReadable,mipmap=i.mipmapEnabled,android=i.GetPlatformTextureSettings("Android").format.ToString(),ios=i.GetPlatformTextureSettings("iPhone").format.ToString()};
  }).ToArray();
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/WhisperingWoods.prefab");
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-WhisperingWoods");var schema=group.GetSchema<BundledAssetGroupSchema>();
  var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/WoodsIslandEffects.shader");var messages=ShaderUtil.GetShaderMessages(shader);
  if(messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))throw new Exception("Shader derleme hatası");
  if(prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Any(g=>g.raycastTarget))throw new Exception("Sanat düğümlere raycast engeli oluşturur");
  if(group.entries.Count!=1||group.entries.First().address!="Map/woods")throw new Exception("Ada başına tek adres sözleşmesi");
  var result=new{success=true,textures,layers=Enumerable.Range(0,prefab.transform.childCount).Select(i=>prefab.transform.GetChild(i).name).ToArray(),fireflies=prefab.GetComponentsInChildren<Mewgic.Map.WoodsFireflyGraphic>().Length,group=group.Name,localBuildPath=schema.BuildPath.GetValue(settings),localLoadPath=schema.LoadPath.GetValue(settings),includeInBuild=schema.IncludeInBuild,shaderMessages=messages.Select(m=>new{m.message,severity=m.severity.ToString()}).ToArray(),dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true)};
  string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});
  File.WriteAllText("docs/ProductionReports/Part13B/ASSET_AUDIT.json",json);return result;
 }
}
