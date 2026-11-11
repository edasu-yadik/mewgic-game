using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
 [Serializable] public sealed class WorldMapData
 {
  public int version=1;public bool migrated;public int highestUnlockedIsland,lastCelebratedIsland,lastFocusedLevel=1;
  public List<string> legacyChestEntitlements=new List<string>();
  public MapJourneyData journey=new MapJourneyData();
  public WorldMapData Copy()=>new WorldMapData{journey=(journey??new MapJourneyData()).Copy(),version=version,migrated=migrated,highestUnlockedIsland=highestUnlockedIsland,lastCelebratedIsland=lastCelebratedIsland,lastFocusedLevel=lastFocusedLevel,legacyChestEntitlements=new List<string>(legacyChestEntitlements??new List<string>())};
  public void Validate(){if(version!=1||highestUnlockedIsland<0||lastCelebratedIsland<0||lastFocusedLevel<1||legacyChestEntitlements==null||legacyChestEntitlements.Any(string.IsNullOrWhiteSpace)||legacyChestEntitlements.Distinct().Count()!=legacyChestEntitlements.Count)throw new ArgumentException("Geçersiz dünya haritası kaydı");}
 }
 public static partial class WorldMapRules
 {
  public const int IslandsPerPack=12;
  public static int Island(int level){if(level<1)throw new ArgumentOutOfRangeException(nameof(level));return (level-1)/LevelsPerIsland;}
  public static int FirstLevel(int island){if(island<0||island>LastIsland)throw new ArgumentOutOfRangeException(nameof(island));return checked(island*LevelsPerIsland+1);}
  public static int CurrentLevel(ProgressData data){var completed=new HashSet<int>(data.levels.Where(x=>x.rewardClaimed).Select(x=>x.level));int id=1;while(id<MaxLevel&&completed.Contains(id))id++;return id;}
  // Capture old earned chests once, before any new-game win can create a legacy entitlement.
  public static int Initialize(ProgressData data)
  {
   if(data.worldMap==null)data.worldMap=new WorldMapData();data.worldMap.Validate();
   if(!data.worldMap.migrated){int[][] legacy={new[]{1,2},new[]{3,4,5},new[]{6,7,8},new[]{9,10,11}};string[] keys={"village","crystal","moon","castle"};var done=new HashSet<int>(data.levels.Where(x=>x.rewardClaimed).Select(x=>x.level));for(int r=0;r<4;r++)if(legacy.Take(r+1).SelectMany(x=>x).All(done.Contains)&&!data.worldMap.legacyChestEntitlements.Contains("map."+keys[r]+".v1"))data.worldMap.legacyChestEntitlements.Add("map."+keys[r]+".v1");data.worldMap.migrated=true;}
   int current=CurrentLevel(data);
   Sync(data,current);
   MapJourneyRules.Initialize(data,current);
   return current;
  }
  public static void Sync(ProgressData data) => Sync(data,CurrentLevel(data));
  internal static void Sync(ProgressData data,int unlocked) => data.worldMap.highestUnlockedIsland=Math.Max(data.worldMap.highestUnlockedIsland,Island(unlocked));
 }
 public sealed partial class ProgressLedger
 {
  public WorldMapData WorldMap=>state.worldMap.Copy();
  public bool SaveMapFocus(int level){if(level<1)return false;if(state.worldMap.lastFocusedLevel==level)return true;var copy=state.Copy();copy.worldMap.lastFocusedLevel=level;return Commit(copy);}
  public bool AcknowledgeIsland(int island){if(island<0||island>state.worldMap.highestUnlockedIsland)return false;if(island<=state.worldMap.lastCelebratedIsland)return true;var copy=state.Copy();copy.worldMap.lastCelebratedIsland=island;return Commit(copy);}
 }
}
