using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
public static class Part13FAssetAudit
{
 public static object Run()
 {
  string root="Assets/Art/Environment/Map/Islands/clockwork/";
  var paths=new[]{root+"IslandBase/Clockwork_Island_v001.png",root+"Architecture/Clockwork_Tower_v001.png",root+"Architecture/Clockwork_Workshops_v001.png",root+"Decorations/Clockwork_Gear_v001.png"};
  var textures=paths.Select(path=>{
   var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);
   return new{path,t.width,t.height,format=t.format.ToString(),editorRuntimeBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t),readWrite=i.isReadable,mipmap=i.mipmapEnabled,android=i.GetPlatformTextureSettings("Android").format.ToString(),ios=i.GetPlatformTextureSettings("iPhone").format.ToString()};
  }).ToArray();
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/ClockworkIsles.prefab");
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-ClockworkIsles");var schema=group.GetSchema<BundledAssetGroupSchema>();
  var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/ClockworkIslandLayers.shader");var messages=ShaderUtil.GetShaderMessages(shader);
  if(messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))throw new Exception("Shader derleme hatası");
  if(prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Any(g=>g.raycastTarget))throw new Exception("Sanat düğümlere raycast engeli oluşturur");
  if(group.entries.Count!=1||group.entries.First().address!="Map/clockwork")throw new Exception("Ada başına tek adres sözleşmesi");
  var presentation=prefab.GetComponent<Mewgic.Map.ClockworkIslandPresentation>();
  var ground=prefab.transform.Find("IslandBase/Clockwork_Island").GetComponent<UnityEngine.UI.Image>();
  var decoration=prefab.transform.Find("Architecture/ClockmakerWorkshops").GetComponent<UnityEngine.UI.Image>();
  var palace=prefab.transform.Find("Architecture/ClockTower").GetComponent<UnityEngine.UI.Image>();
  bool separate=palace.sprite!=ground.sprite&&presentation.towerEnergy.sprite==palace.sprite&&ground.sprite==presentation.energy.sprite&&ground.material.GetFloat("_LayerMode")==0&&presentation.energy.material.GetFloat("_LayerMode")==1&&decoration.sprite!=ground.sprite;
  if(!separate)throw new Exception("Taban/kule/atölye ayrımı bozuk");
  if(presentation.gears.Length!=3||presentation.gears.Any(g=>g.pivot!=new Vector2(.5f,.5f)||g.GetComponent<UnityEngine.UI.Image>().sprite!=presentation.gears[0].GetComponent<UnityEngine.UI.Image>().sprite))throw new Exception("Üç ayrı merkez pivotlu ortak dişli gerekli");
  if(prefab.GetComponentsInChildren<Rigidbody>(true).Length!=0||prefab.GetComponentsInChildren<Rigidbody2D>(true).Length!=0)throw new Exception("Fizik simülasyonu eklenmemeli");
  var dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true);
  if(dependencies.Any(d=>d.EndsWith(".png")&&!d.StartsWith(root)))throw new Exception("Diğer ada sanatına bağımlılık oluştu");
  var result=new{success=true,separateGroundTowerWorkshops=separate,textures,layers=Enumerable.Range(0,prefab.transform.childCount).Select(i=>prefab.transform.GetChild(i).name).ToArray(),gears=presentation.gears.Length,group=group.Name,localBuildPath=schema.BuildPath.GetValue(settings),localLoadPath=schema.LoadPath.GetValue(settings),includeInBuild=schema.IncludeInBuild,shaderMessages=messages.Select(m=>new{m.message,severity=m.severity.ToString()}).ToArray(),dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true)};
  string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});
  File.WriteAllText("docs/ProductionReports/Part13F/ASSET_AUDIT.json",json);return result;
 }
}
