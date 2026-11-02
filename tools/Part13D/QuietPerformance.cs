using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mewgic.Map;
public static class Part13DQuietPerformance
{
 public static object Run()
 {
  if(!EditorApplication.isPlaying||UnityEngine.Object.FindFirstObjectByType<CoralIslandPresentation>()==null)throw new Exception("Önce güvenli Coral önizlemesi açılmalı");
  var samples=new List<double>();int warmup=60,last=Time.frameCount;EditorApplication.CallbackFunction tick=null;
  tick=()=>{
   if(!EditorApplication.isPlaying){EditorApplication.update-=tick;return;}
   if(Time.frameCount==last)return;last=Time.frameCount;
   if(warmup>0){warmup--;return;}
   samples.Add(Time.unscaledDeltaTime*1000);
   if(samples.Count<180)return;
   EditorApplication.update-=tick;samples.Sort();
   var result=new{success=true,environment="Windows Unity6000.3.27f1 / Android import target / AssetDatabase Play Mode",warmupFrames=60,sampledFrames=samples.Count,medianMs=samples[samples.Count/2],p95Ms=samples[(int)(samples.Count*.95)],maxMs=samples.Last(),drawCalls=UnityStats.drawCalls,triangles=UnityStats.triangles,liveHandles=IslandArtSlot.LiveHandles,liveInstances=IslandArtSlot.LiveInstances,note="Ölçüm sırasında Unity araç çağrısı yapılmadı. Telefon/GPU/termal ölçümü değildir."};
   string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});File.WriteAllText("docs/ProductionReports/Part13D/PERFORMANCE_QUIET.json",json);
  };
  EditorApplication.update+=tick;return new{started=true,warmupFrames=60,sampledFrames=180};
 }
}
