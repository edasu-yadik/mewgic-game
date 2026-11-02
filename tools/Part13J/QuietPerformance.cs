using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mewgic.Map;
public static class Part13JQuietPerformance
{
 public static object Run()
 {
  if(!EditorApplication.isPlaying||UnityEngine.Object.FindFirstObjectByType<AuroraIslandPresentation>()==null)throw new Exception("Önce güvenli Aurora önizlemesi açılmalı");
  var original=UnityEngine.Object.FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).SelectMany(x=>x.GetComponentsInChildren<MonoBehaviour>()).Where(x=>x!=null&&x.GetType().Name.EndsWith("IslandPresentation")).Select(x=>x.GetInstanceID()).OrderBy(x=>x).ToArray();bool stable=true;
  var samples=new List<double>();int warmup=60,last=Time.frameCount;EditorApplication.CallbackFunction tick=null;
  tick=()=>{
   if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
   if(Time.frameCount==last)return;last=Time.frameCount;
   if(warmup>0){warmup--;return;}
   samples.Add(Time.unscaledDeltaTime*1000);stable &= original.SequenceEqual(UnityEngine.Object.FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).SelectMany(x=>x.GetComponentsInChildren<MonoBehaviour>()).Where(x=>x!=null&&x.GetType().Name.EndsWith("IslandPresentation")).Select(x=>x.GetInstanceID()).OrderBy(x=>x));
   if(samples.Count<180)return;
   EditorApplication.update-=tick;samples.Sort();
   var result=new{success=stable,stationaryArtInstanceIdsStable=stable,environment="Windows Unity6000.3.27f1 / Android import target / AssetDatabase Play Mode",warmupFrames=60,sampledFrames=samples.Count,medianMs=samples[samples.Count/2],p95Ms=samples[(int)(samples.Count*.95)],maxMs=samples.Last(),drawCalls=UnityStats.drawCalls,triangles=UnityStats.triangles,liveHandles=IslandArtSlot.LiveHandles,liveInstances=IslandArtSlot.LiveInstances,note="Ölçüm sırasında Unity araç çağrısı yapılmadı. Telefon/GPU/termal ölçümü değildir."};
   string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});File.WriteAllText("docs/ProductionReports/Part13J/PERFORMANCE_QUIET.json",json);
  };
  EditorApplication.update+=tick;return new{started=true,warmupFrames=60,sampledFrames=180};
 }
}
