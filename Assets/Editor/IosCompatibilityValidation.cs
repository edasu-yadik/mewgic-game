using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class IosCompatibilityValidation
{
 public static void Run()
 {
  var passed=new List<string>();
  try {
   if(Application.unityVersion!="6000.3.26f1")throw new Exception("Wrong compatibility Editor");
   foreach(var pair in new[]{new[]{"MewgicPrototypeSetup","TestModel"},new[]{"BetaValidation","SaveTests"},new[]{"MapProgressionTests","Models"},new[]{"GameplayBoosterTests","ModelTests"},new[]{"DailyActivitiesTests","Models"},new[]{"RewardHubTests","Models"}}){
    var type=typeof(IosCompatibilityValidation).Assembly.GetType(pair[0]);
    type.GetMethod(pair[1]).Invoke(null,null);passed.Add(pair[0]+"."+pair[1]);
   }
   foreach(var name in new[]{"Boot","Home","Gameplay","LevelMap"}){
    var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");
    if(scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Component>(true)).Any(c=>c==null))throw new Exception("Missing script in "+name);
    if(Camera.main==null)throw new Exception("Missing camera in "+name);passed.Add(name+" scene references");
   }
   File.WriteAllText("docs/IOS_COMPATIBILITY_MODELS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=true,editor=Application.unityVersion,checks=passed},Newtonsoft.Json.Formatting.Indented));
  }catch(Exception e){File.WriteAllText("docs/IOS_COMPATIBILITY_MODELS.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{success=false,error=e.ToString(),checks=passed},Newtonsoft.Json.Formatting.Indented));throw;}
 }
}
