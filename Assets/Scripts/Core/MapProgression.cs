using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
 [Serializable] public sealed class MapRegionData { public string key; public int[] levels; public MilestoneReward reward; }
 [Serializable] public sealed class MapDefinition { public MapRegionData[] regions; }
 [Serializable] public sealed class MilestoneReward { public string id; public BoosterKind kind; public int amount; }
 public enum MapLevelStatus {Unavailable,Locked,Available,Current,Completed}
 // No Unity, currency, network or scene dependency. Verification is supplied by the catalog.
 public sealed class MapProgression
 {
  readonly MapDefinition definition;readonly ProgressLedger ledger;readonly Func<int,bool> verified;
  public MapProgression(MapDefinition data,ProgressLedger progress,Func<int,bool> isVerified)
  {
   if(data?.regions==null||data.regions.Length!=4||progress==null||isVerified==null)throw new ArgumentException("Harita verisi eksik");
   var ids=new HashSet<int>();var keys=new HashSet<string>();var rewards=new HashSet<string>();
   foreach(var r in data.regions){if(r==null||string.IsNullOrWhiteSpace(r.key)||!keys.Add(r.key)||r.levels==null||r.levels.Length!=3||r.levels.Any(x=>x<0))throw new ArgumentException("Harita bölgesi geçersiz");foreach(int id in r.levels.Where(x=>x>0))if(!ids.Add(id))throw new ArgumentException("Yinelenen bölüm");if(r.reward==null||string.IsNullOrWhiteSpace(r.reward.id)||!rewards.Add(r.reward.id)||!Enum.IsDefined(typeof(BoosterKind),r.reward.kind)||r.reward.amount<1||r.reward.amount>100)throw new ArgumentException("Sandık ödülü geçersiz");}
   definition=data;ledger=progress;verified=isVerified;
  }
  public int LevelId(int region,int slot)=>definition.regions[region].levels[slot];
  public int Total(int region)=>definition.regions[region].levels.Count(x=>x>0);
  public int Completed(int region)=>definition.regions[region].levels.Count(x=>x>0&&verified(x)&&ledger.Completed(x));
  public int Stars(int id)=>verified(id)?ledger.BestStars(id):0;
  public bool RegionComplete(int region)=>Total(region)>0&&definition.regions[region].levels.Where(x=>x>0).All(x=>verified(x)&&ledger.Completed(x));
  public bool RegionOpen(int region)=>region>=0&&region<4&&definition.regions.Take(region).Select((r,i)=>i).All(RegionComplete)&&definition.regions[region].levels.Any(x=>x>0&&verified(x));
  public bool CanEnter(int id)=>CanEnterOrdered(id);
  public MapLevelStatus Status(int region,int slot)
  {int id=LevelId(region,slot);if(id<1||!verified(id))return MapLevelStatus.Unavailable;if(!CanEnterOrdered(id))return MapLevelStatus.Locked;if(ledger.Completed(id))return MapLevelStatus.Completed;return CurrentId==id?MapLevelStatus.Current:MapLevelStatus.Available;}
  public bool CanEnterOrdered(int id)
  {if(id<1||!verified(id))return false;for(int r=0;r<4;r++)foreach(int x in definition.regions[r].levels){if(x==id)return RegionOpen(r);if(x>0&&(!verified(x)||!ledger.Completed(x)))return false;}return false;}
  public int CurrentId {get{foreach(var r in definition.regions)foreach(int id in r.levels)if(id>0&&verified(id)&&!ledger.Completed(id)&&CanEnterOrdered(id))return id;return 0;}}
  public int CurrentRegion {get{int id=CurrentId;for(int r=0;r<4;r++)if(definition.regions[r].levels.Contains(id)&&id>0)return r;return 0;}}
  public bool ChestClaimed(int region)=>ledger.MilestoneClaimed(definition.regions[region].reward.id);
  public bool ChestReady(int region)=>RegionOpen(region)&&RegionComplete(region)&&!ChestClaimed(region);
  public MilestoneReward Reward(int region)=>definition.regions[region].reward;
  public bool ClaimChest(int region,out bool granted)=>ledger.ClaimMilestone(Reward(region),RegionOpen(region)&&RegionComplete(region),out granted);
 }
}
