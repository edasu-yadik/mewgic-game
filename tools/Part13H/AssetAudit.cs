using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.AddressableAssets;using UnityEditor.AddressableAssets.Settings.GroupSchemas;
public static class Part13HAssetAudit
{
 public static object Run()
 {
  string root="Assets/Art/Environment/Map/Islands/library/";
  var paths=new[]{root+"IslandBase/Library_Island_v001.png",root+"Architecture/Library_Academy_v001.png",root+"Decorations/Library_Book_v001.png",root+"ForegroundEffects/Library_Ring_v001.png"};
  var textures=paths.Select(path=>{var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);return new{path,t.width,t.height,format=t.format.ToString(),editorRuntimeBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t),readWrite=i.isReadable,mipmap=i.mipmapEnabled,android=i.GetPlatformTextureSettings("Android").format.ToString(),ios=i.GetPlatformTextureSettings("iPhone").format.ToString()};}).ToArray();
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Map/ArcaneLibrary.prefab");var p=prefab.GetComponent<Mewgic.Map.LibraryIslandPresentation>();
  var settings=AddressableAssetSettingsDefaultObject.Settings;var group=settings.FindGroup("Map-ArcaneLibrary");var schema=group.GetSchema<BundledAssetGroupSchema>();
  var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/LibraryIslandLayers.shader");var messages=ShaderUtil.GetShaderMessages(shader);
  if(messages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error))throw new Exception("Shader derleme hatası");
  if(prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Any(g=>g.raycastTarget))throw new Exception("Sanat dokunmayı engelliyor");
  if(group.entries.Count!=1||group.entries.First().address!="Map/library")throw new Exception("Ada başına tek yerel adres gerekli");
  var ground=prefab.transform.Find("IslandBase/Library_Island").GetComponent<UnityEngine.UI.Image>();var academy=prefab.transform.Find("Architecture/GrandLibraryAcademy").GetComponent<UnityEngine.UI.Image>();
  var book=p.books[0].GetComponent<UnityEngine.UI.Image>();bool separate=ground.sprite!=academy.sprite&&p.lights.sprite==academy.sprite&&p.lights.material.GetFloat("_LayerMode")==1&&book.sprite!=academy.sprite&&book.sprite!=ground.sprite&&p.rings[0].sprite!=book.sprite;
  if(!separate||p.books.Length!=3||p.rings.Length!=2)throw new Exception("Taban/mimari/kitap/efekt ayrımı veya bütçe bozuk");
  if(prefab.GetComponentsInChildren<Rigidbody>(true).Length!=0||prefab.GetComponentsInChildren<Rigidbody2D>(true).Length!=0)throw new Exception("Fizik animasyonu eklenmemeli");
  var dependencies=AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(prefab),true);if(dependencies.Any(d=>d.EndsWith(".png")&&!d.StartsWith(root)))throw new Exception("Diğer ada sanatına bağımlılık oluştu");
  var result=new{success=true,separateGroundArchitectureBooksEffects=separate,textures,layers=Enumerable.Range(0,prefab.transform.childCount).Select(i=>prefab.transform.GetChild(i).name).ToArray(),books=p.books.Length,rings=p.rings.Length,group=group.Name,localBuildPath=schema.BuildPath.GetValue(settings),localLoadPath=schema.LoadPath.GetValue(settings),includeInBuild=schema.IncludeInBuild,shaderMessages=messages.Select(m=>new{m.message,severity=m.severity.ToString()}).ToArray(),dependencies};
  string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});File.WriteAllText("docs/ProductionReports/Part13H/ASSET_AUDIT.json",json);return result;
 }
}
