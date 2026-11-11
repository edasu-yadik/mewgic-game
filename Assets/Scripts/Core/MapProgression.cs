using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
 [Serializable] public sealed class MapRegionData {public string key,displayName,theme,artKey;public int artIndex=-1,firstLevel,levelCount;public bool artReady;public string[] tutorialIds=new string[0];public int[] levels;public MilestoneReward reward;}
 [Serializable] public sealed class MapDefinition {public int version=1;public bool expandable;public MapRegionData[] regions;}
 [Serializable] public sealed class MilestoneReward {public string id;public BoosterKind kind;public int amount;}
 public enum MapLevelStatus {Unavailable,Locked,Available,Current,Completed}
 // Addressable is not the same as prepared: the catalog must validate a generated pin before entry.
 public sealed class MapProgression
 {
  readonly MapDefinition definition;readonly ProgressLedger ledger;readonly Func<int,bool> addressable;int current,revision=-1;
  Dictionary<int,int> completed;
  public MapProgression(MapDefinition data,ProgressLedger progress,Func<int,bool> isAddressable)
  {
   if(data?.version!=1||data?.regions==null||data.regions.Length==0||progress==null||isAddressable==null)throw new ArgumentException("Harita verisi eksik");
   var keys=new HashSet<string>();var rewards=new HashSet<string>();var ids=new HashSet<int>();
   for(int i=0;i<data.regions.Length;i++){var r=data.regions[i];if(r==null||string.IsNullOrWhiteSpace(r.key)||!keys.Add(r.key))throw new ArgumentException("Harita bölgesi geçersiz");if(data.expandable){if(r.firstLevel!=WorldMapRules.FirstLevel(i)||r.levelCount!=50||string.IsNullOrWhiteSpace(r.displayName))throw new ArgumentException("Ada aralığı geçersiz");if(i>=4&&r.reward?.id!="map.island."+r.firstLevel+".v1")throw new ArgumentException("Yayımlanan ada ödül kimliği değişemez");}else{if(r.levels==null||r.levels.Any(x=>x<0))throw new ArgumentException("Harita konumları geçersiz");foreach(int id in r.levels.Where(x=>x>0))if(!ids.Add(id))throw new ArgumentException("Yinelenen bölüm");}if(r.reward==null||string.IsNullOrWhiteSpace(r.reward.id)||!rewards.Add(r.reward.id)||!Enum.IsDefined(typeof(BoosterKind),r.reward.kind)||r.reward.amount<1||r.reward.amount>100)throw new ArgumentException("Sandık ödülü geçersiz");}
   definition=data;ledger=progress;addressable=isAddressable;EnsureFresh();
  }
  void EnsureFresh(){if(revision==ledger.CompletionRevision)return;completed=ledger.CompletedRecords.ToDictionary(x=>x.level,x=>x.bestStars);current=ledger.CurrentUnlockedLevel;revision=ledger.CompletionRevision;}
  public int DefinedIslands=>definition.regions.Length;
  public bool Expandable=>definition.expandable;
  public MapRegionData Region(int index){if(index<0)throw new ArgumentOutOfRangeException(nameof(index));if(index<definition.regions.Length)return definition.regions[index];if(!Expandable)throw new ArgumentOutOfRangeException(nameof(index));int first=WorldMapRules.FirstLevel(index);return new MapRegionData{key="frontier-"+first,displayName="Yeni Ufuklar "+(index+1),theme="Gelecek dünya paketi",artIndex=index%4,artReady=true,firstLevel=first,levelCount=50,reward=new MilestoneReward{id="map.island."+first+".v1",kind=BoosterKind.Hint,amount=1}};}
  public int LevelId(int region,int slot){var r=Region(region);if(Expandable){if(slot<0||slot>=50)throw new ArgumentOutOfRangeException(nameof(slot));return r.firstLevel+slot;}return r.levels[slot];}
  public int Total(int region)=>Expandable?Region(region).levelCount:Region(region).levels.Count(x=>x>0);
  public int Completed(int region){EnsureFresh();int n=0;for(int i=0;i<(Expandable?Total(region):Region(region).levels.Length);i++)if(completed.ContainsKey(LevelId(region,i)))n++;return n;}
  public int Stars(int id){EnsureFresh();return completed.TryGetValue(id,out var count)?count:0;}
  public bool RegionComplete(int region)=>Total(region)>0&&Completed(region)==Total(region);
  public bool RegionOpen(int region)=>region>=0&&(Expandable?WorldMapRules.FirstLevel(region)<=CurrentId:region<DefinedIslands&&Enumerable.Range(0,region).All(RegionComplete));
  public bool CanEnter(int id)=>CanEnterOrdered(id);
  public MapLevelStatus Status(int region,int slot)=>Status(LevelId(region,slot));
  public MapLevelStatus Status(int id){EnsureFresh();if(id<1||!addressable(id))return MapLevelStatus.Unavailable;if(!CanEnterOrdered(id))return MapLevelStatus.Locked;if(completed.ContainsKey(id))return MapLevelStatus.Completed;return CurrentId==id?MapLevelStatus.Current:MapLevelStatus.Available;}
  public bool CanEnterOrdered(int id){EnsureFresh();if(id<1||!addressable(id))return false;if(Expandable)return id<=current;foreach(var r in definition.regions)foreach(int x in r.levels){if(x==id)return true;if(x>0&&(!addressable(x)||!completed.ContainsKey(x)))return false;}return false;}
  public int CurrentId {get{EnsureFresh();return Expandable?current:definition.regions.SelectMany(r=>r.levels).FirstOrDefault(x=>x>0&&addressable(x)&&!completed.ContainsKey(x)&&CanEnterOrdered(x));}}
  public int CurrentRegion=>Expandable?WorldMapRules.Island(CurrentId):Math.Max(0,Array.FindIndex(definition.regions,r=>r.levels.Contains(CurrentId)));
  public bool ChestClaimed(int region)=>ledger.MilestoneClaimed(Reward(region).id);
  bool Entitled(int region)=>(RegionOpen(region)&&RegionComplete(region))||ledger.WorldMap.legacyChestEntitlements.Contains(Reward(region).id);
  public bool ChestReady(int region)=>Entitled(region)&&!ChestClaimed(region);
  public MilestoneReward Reward(int region)=>Region(region).reward;
  public bool ClaimChest(int region,out bool granted)=>ledger.ClaimMilestone(Reward(region),Entitled(region),out granted);
 }
}
