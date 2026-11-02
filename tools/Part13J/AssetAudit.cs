using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEngine.UI;using Mewgic.Map;using UnityEditor.AddressableAssets;
public static class Part13JAssetAudit{
public static object Run(){
var def=MapProgressionCatalog.Definition();var rows=new List<object>();long astc=0,rgba=0;
string[] names={"WhisperingWoods","GoldenMirage","CoralKingdom","FrostlightPeaks","ClockworkIsles","StarlightGrove","ArcaneLibrary","AuroraCitadel"};
for(int n=0;n<12;n++){
var d=def.regions[n];string path=n<4?new[]{"Assets/Art/Environment/Map/Map_Village_v001.png","Assets/Art/Environment/Map/Map_Crystal_v001.png","Assets/Art/Environment/Map/Map_Moon_v001.png","Assets/Art/Environment/Map/Map_Castle_v001.png"}[n]:"Assets/Prefabs/Map/"+names[n-4]+".prefab";
if(AssetDatabase.LoadMainAssetAtPath(path)==null)throw new Exception("Eksik sanat "+path);
var deps=AssetDatabase.GetDependencies(path,true);var textures=deps.Where(x=>x.EndsWith(".png")).Select(p=>{var t=AssetDatabase.LoadAssetAtPath<Texture2D>(p);var i=(TextureImporter)AssetImporter.GetAtPath(p);long a=((t.width+5)/6)*((t.height+5)/6)*16L;long r=t.width*t.height*4L;astc+=a;rgba+=r;return new{path=p,t.width,t.height,format=t.format.ToString(),astc6x6Bytes=a,rgbaBytes=r,editorBytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t),readWrite=i.isReadable,mipmap=i.mipmapEnabled,android=i.GetPlatformTextureSettings("Android").format.ToString(),ios=i.GetPlatformTextureSettings("iPhone").format.ToString()};}).ToArray();
string[] layers=Array.Empty<string>();string address=null;
if(n>=4){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);layers=Enumerable.Range(0,prefab.transform.childCount).Select(x=>prefab.transform.GetChild(x).name).ToArray();if(layers.Length!=9||prefab.GetComponentsInChildren<Graphic>(true).Any(x=>x.raycastTarget))throw new Exception("Katman/raycast "+path);var e=AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));address=e?.address;if(address!="Map/"+d.artKey)throw new Exception("Adres "+path);}
rows.Add(new{island=n+1,d.displayName,d.theme,d.firstLevel,d.levelCount,d.artReady,path,address,layers,textures});}
var result=new{success=true,islands=rows,allArtAstcEstimateBytes=astc,allArtRgbaFallbackBytes=rgba,note="Tum12 adanin sanat kaynaklari toplami; ayni anda yuklu RAM degildir. Sky/UI/material/driver/atlas overhead haric; Editor bellegi telefon RAM degildir."};
var json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});File.WriteAllText("docs/ProductionReports/Part13J/ASSET_AUDIT.json",json);return new{success=true,islands=rows.Count,astc,rgba};}}
