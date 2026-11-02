using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEditor;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using Mewgic.Map;
public static class Part14AMapAudit{
public static object Run(){var map=UnityEngine.Object.FindFirstObjectByType<MapWorld>();var checks=new List<string>();int region=0,page=0,stage=0,wait=0,last=-1;EditorApplication.CallbackFunction tick=null;
Action<bool,string> C=(ok,s)=>{if(!ok)throw new Exception(s);checks.Add(s);};
tick=()=>{if(last==Time.frameCount)return;last=Time.frameCount;try{if(!EditorApplication.isPlaying)throw new Exception("Play Mode durdu");if(wait-->0)return;
if(stage==0){map.FocusRegion(region,false);stage=1;wait=2;return;}
if(stage==1){if(UnityEngine.Object.FindObjectsByType<IslandArtSlot>(FindObjectsSortMode.None).Any(s=>s.BoundKey!=null&&!s.Loaded))return;C(IslandArtSlot.LiveHandles<=5&&IslandArtSlot.LiveInstances<=5,"Sanat havuzu "+region);map.OpenChapter(region,page);stage=2;wait=2;return;}
var nodes=map.Points.Where(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy).ToArray();C(nodes.Length==10,"10 tekil bolum "+region+"/"+page);
foreach(var p in nodes){int id=map.Progression.LevelId(p.regionIndex,p.slotIndex);C(id>=region*50+page*10+1&&id<=region*50+page*10+10,"Aralik "+id);var d=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(map.Canvas.worldCamera,p.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(d,hits);C(hits.Count>0&&hits[0].gameObject.GetComponentInParent<MapPoint>()==p,"Gercek raycast "+id);var n=p.transform.Find("LevelNumber").GetComponent<TMPro.TMP_Text>();n.ForceMeshUpdate();C(!n.isTextOverflowing,"Numara tasmaz "+id);}
GameObject.Find("ChapterClose").GetComponent<Button>().onClick.Invoke();if(++page==5||region==12){page=0;region++;stage=0;}else stage=1;
if(region<=12)return;EditorApplication.update-=tick;map.FocusRegion(1,false);map.OpenChapter(1,4);var result=new{success=true,count=checks.Count,checks,firstTwelveAnd601Continuation=true};Write(result);}
catch(Exception e){EditorApplication.update-=tick;Write(new{success=false,error=e.ToString(),count=checks.Count,checks});}};EditorApplication.update+=tick;return new{started=true};}
static void Write(object value){var json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{value});File.WriteAllText("docs/ProductionReports/Part14A/MAP_ALL_ISLANDS.json",json);}}
