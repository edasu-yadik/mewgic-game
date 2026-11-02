using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mewgic.Map;
public static class Part13DTouchAudit
{
 public static object Run()
 {
  var close=GameObject.Find("ChapterClose");if(close!=null)close.GetComponent<Button>().onClick.Invoke();
  var map=UnityEngine.Object.FindFirstObjectByType<MapWorld>();map.FocusRegion(6,false);Canvas.ForceUpdateCanvases();var checks=new List<string>();
  foreach(var p in map.Points.Where(p=>p.regionIndex==6&&p.gameObject.activeInHierarchy))Hit(map,p,checks);
  Write("RAYCAST_MAP_TESTS",checks);map.OpenChapter(6,0);return new{success=true,count=checks.Count,note="Bölüm raycast ölçümü sonraki çizilen karede yapılır"};
 }
 public static object Chapter()
 {
  var map=UnityEngine.Object.FindFirstObjectByType<MapWorld>();var checks=new List<string>();
  foreach(var p in map.Points.Where(p=>p.kind==MapPointKind.LevelSlot&&p.gameObject.activeInHierarchy))Hit(map,p,checks);
  if(checks.Count!=10)throw new Exception("10 gerçek bölüm hedefi gerekli");
  Write("RAYCAST_CHAPTER_TESTS",checks);GameObject.Find("ChapterClose").GetComponent<Button>().onClick.Invoke();return new{success=true,count=checks.Count};
 }
 static void Write(string name,List<string> checks)
 {
  var result=new{success=true,count=checks.Count,checks};
  string json=(string)Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json").GetMethod("SerializeObject",new[]{typeof(object)}).Invoke(null,new object[]{result});File.WriteAllText("docs/ProductionReports/Part13D/"+name+".json",json);
 }

 static void Hit(MapWorld map,MapPoint p,List<string> checks)
 {
  var data=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(map.Canvas.worldCamera,p.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
  if(hits.Count==0||hits[0].gameObject.GetComponentInParent<MapPoint>()!=p)throw new Exception("Sanat/UI dokunmayı engelliyor: "+p.name);
  checks.Add("Gerçek GraphicRaycaster ilk hedef: "+p.kind+"/"+p.slotIndex);
 }
}
