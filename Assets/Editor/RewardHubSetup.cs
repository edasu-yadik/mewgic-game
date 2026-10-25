using System;
using UnityEditor;
using UnityEngine;
using Mewgic.Rewards;
public static class RewardHubSetup
{
 public static void Install()
 {
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Play kapalı olmalı");
  const string path="Assets/Resources/Rewards/Art.asset";
  var a=AssetDatabase.LoadAssetAtPath<RewardHubArt>(path);
  if(a==null){a=ScriptableObject.CreateInstance<RewardHubArt>();AssetDatabase.CreateAsset(a,path);}
  a.weeklyIsland=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/Map/Map_Crystal_v001.png");
  a.closedChest=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/Map/Map_Chest_v001.png");
  a.wizardCard=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Characters/WizardCat/MageCat_FullBody_v001.png");
  a.node=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environment/Map/Map_Node_v001.png");
  a.eventsIcon=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Navigation/Nav_Events.png");
  if(a.weeklyIsland==null||a.closedChest==null||a.wizardCard==null||a.node==null||a.eventsIcon==null)throw new InvalidOperationException("Ödül varlığı eksik");
  EditorUtility.SetDirty(a);AssetDatabase.SaveAssets();
 }
}
