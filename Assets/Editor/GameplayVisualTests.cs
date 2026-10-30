using System;using System.IO;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using Mewgic;
[InitializeOnLoad] public static class GameplayVisualTests
{
 static string output;
static double next;static int step;static readonly List<string> checks=new List<string>();
static GameplayVisualTests(){EditorApplication.update+=Tick;}
public static void Run(){output=BetaCapturePaths.NewRun("GameplayVisualTests");if(EditorApplication.isPlaying)throw new Exception("Stop Play first");Mewgic.GameplayLevelCatalog.ResetSelection();Mewgic.GameplayProgress.UseTestProfile();checks.Clear();step=0;SessionState.SetBool("GameplayVisualTest",true);EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");EditorApplication.isPlaying=true;next=EditorApplication.timeSinceStartup+1;}
static void Check(bool ok,string name){if(!ok)throw new Exception(name);checks.Add(name);}
static Rect Bounds(RectTransform t,RectTransform relative){var c=new Vector3[4];t.GetWorldCorners(c);Vector2 min=Vector2.one*float.MaxValue,max=-min;foreach(var p in c){var v=(Vector2)relative.InverseTransformPoint(p);min=Vector2.Min(min,v);max=Vector2.Max(max,v);}return Rect.MinMaxRect(min.x,min.y,max.x,max.y);}
static void Click(string name){Canvas.ForceUpdateCanvases();var g=GameObject.Find(name);Check(g!=null,"Button exists "+name);var r=(RectTransform)g.transform;var d=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(d,hits);Check(hits.Count>0&&hits[0].gameObject==g,"Top touch target "+name);ExecuteEvents.Execute(g,d,ExecuteEvents.pointerClickHandler);}
public static void Capture(int height,bool inset=false,string suffix="")
{
var p=UnityEngine.Object.FindFirstObjectByType<GameplayPresentation>();var canvas=UnityEngine.Object.FindFirstObjectByType<Canvas>();var safe=(RectTransform)GameObject.Find("Safe area").transform;var cam=Camera.main;var min=safe.anchorMin;var max=safe.anchorMax;var mode=canvas.renderMode;var camera=canvas.worldCamera;var dist=canvas.planeDistance;var rt=new RenderTexture(900,height,24);var old=RenderTexture.active;var target=cam.targetTexture;Texture2D tex=null;
try{canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;cam.targetTexture=rt;if(inset){safe.anchorMin=new Vector2(.04f,.05f);safe.anchorMax=new Vector2(.96f,.96f);}Canvas.ForceUpdateCanvases();p.Layout();Canvas.ForceUpdateCanvases();var cat=(RectTransform)GameObject.Find("GameplayWizardCat").transform;var cb=Bounds(cat,safe);Check(safe.rect.Contains(cb.min)&&safe.rect.Contains(cb.max),"Cat inside safe "+height+suffix);
foreach(var b in GameObject.Find("Bottle board").GetComponentsInChildren<Button>()){var bb=Bounds((RectTransform)b.transform,safe);Check(safe.rect.Contains(bb.min)&&safe.rect.Contains(bb.max),"Bottle inside safe "+b.name+height+suffix);Check(!bb.Overlaps(cb),"Bottle avoids cat "+b.name+height+suffix);foreach(var control in new[]{"Undo","Restart","Home"})Check(!bb.Overlaps(Bounds((RectTransform)GameObject.Find(control).transform,safe)),"Bottle avoids "+control);}
cam.Render();RenderTexture.active=rt;tex=new Texture2D(900,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,height),0,0);tex.Apply();File.WriteAllBytes(output+"gameplay-05a-"+height+suffix+".png",tex.EncodeToPNG());}
finally{cam.targetTexture=target;RenderTexture.active=old;canvas.renderMode=mode;canvas.worldCamera=camera;canvas.planeDistance=dist;safe.anchorMin=min;safe.anchorMax=max;if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);Canvas.ForceUpdateCanvases();p.Layout();}}
static void Finish(string error){SessionState.SetBool("GameplayVisualTest",false);File.WriteAllText("docs/PART05A_TEST_RESULTS.json",JsonUtility.ToJson(new Result{screenshots=output,success=error==null,error=error,checks=checks.ToArray()},true));EditorApplication.isPlaying=false;Mewgic.GameplayLevelCatalog.ResetSelection();Mewgic.GameplayProgress.RestorePlayerProfile();}
[Serializable]class Result{public string screenshots;public bool success;public string error;public string[] checks;}
static void Tick(){if(!SessionState.GetBool("GameplayVisualTest",false)||!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;try{next=EditorApplication.timeSinceStartup+1.1;var p=UnityEngine.Object.FindFirstObjectByType<GameplayPresentation>();switch(step){
case 0:Check(p!=null,"Gameplay presentation mounted");Check(GameObject.Find("Bottle board").GetComponentsInChildren<Button>().Length==4,"Original four dynamic bottles");Check(GameObject.Find("StoneShelves").transform.childCount==2,"Two independent shelf rows");Check(GameObject.Find("Bottle board").GetComponent<Image>().color.a==0,"Old opaque board removed");foreach(var graphic in GameObject.Find("GameplayWorld").GetComponentsInChildren<Graphic>())Check(!graphic.raycastTarget,"World does not intercept touches "+graphic.name);
for(int count=1;count<=24;count++){var rects=new List<Rect>();for(int i=0;i<count;i++){var slot=GameplayPresentation.Slot(i,count,new Vector2(630,688));Check(slot.xMin>=0&&slot.yMin>=0&&slot.xMax<=1&&slot.yMax<=1,"Layout slot inside "+count+":"+i);foreach(var r in rects)Check(!slot.Overlaps(r),"Layout slots separate "+count+":"+i);rects.Add(slot);}}
Capture(1600);Capture(1950);Capture(2000);Capture(2000,true,"-safe");break;
case 1:Click("Bottle 1");break;
case 2:Click("Bottle 2");Check(GameObject.Find("Moves").GetComponent<TMPro.TMP_Text>().text=="HAMLE  0","Invalid move unchanged");break;
case 3:Click("Bottle 3");Check(p.IsAnimating,"Pour locks layout");break;
case 4:Check(!p.IsAnimating,"Pour releases layout");Check(GameObject.Find("Moves").GetComponent<TMPro.TMP_Text>().text=="HAMLE  1","Successful move counted");Click("Undo");break;
case 5:Check(GameObject.Find("Moves").GetComponent<TMPro.TMP_Text>().text=="HAMLE  0","Undo exact count");Click("Restart");break;
case 6:Click("Bottle 1");break;
case 7:Click("Bottle 3");break;
case 8:Click("Bottle 2");break;
case 9:Click("Bottle 1");break;
case 10:Click("Bottle 2");break;
case 11:Click("Bottle 3");break;
case 12:Check(GameObject.Find("Win panel")!=null,"Original level won");Capture(1600,false,"-win");Click("Replay");break;
case 13:Check(GameObject.Find("Moves").GetComponent<TMPro.TMP_Text>().text=="HAMLE  0","Replay resets");Click("Home");break;
case 14:Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Home","Gameplay Home return");Click("Play");break;
case 15:Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Gameplay","Home PLAY returns Gameplay");Finish(null);return;}step++;}catch(Exception e){Finish(e.ToString());}}
}


