using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
 [Serializable]public sealed class RewardHubData
 {
  public int version=1,weekStart,seasonStart,weekWins,seasonWins,highestDay;public long highestUtc;public bool weeklyClaimed;public bool[] seasonClaimed=new bool[3];public List<string> weekReceipts=new List<string>(),seasonReceipts=new List<string>(),owned=new List<string>();
  public RewardHubData Copy()=>new RewardHubData{version=version,weekStart=weekStart,seasonStart=seasonStart,weekWins=weekWins,seasonWins=seasonWins,highestDay=highestDay,highestUtc=highestUtc,weeklyClaimed=weeklyClaimed,seasonClaimed=seasonClaimed.ToArray(),weekReceipts=new List<string>(weekReceipts),seasonReceipts=new List<string>(seasonReceipts),owned=new List<string>(owned)};
  public void Validate(){if(version!=1||weekStart<0||seasonStart<0||weekWins<0||weekWins>3||seasonWins<0||seasonWins>6||highestDay<weekStart||highestDay<seasonStart||highestDay>3652059||highestUtc<0||highestUtc>DateTime.MaxValue.Ticks||seasonClaimed==null||seasonClaimed.Length!=3||weekReceipts==null||seasonReceipts==null||owned==null||weeklyClaimed&&weekWins<3)throw new ArgumentException("Etkinlik kaydı geçersiz");foreach(var receipts in new[]{weekReceipts,seasonReceipts})if(receipts.Count>20||receipts.Any(string.IsNullOrWhiteSpace)||receipts.Distinct().Count()!=receipts.Count)throw new ArgumentException("Etkinlik makbuzu geçersiz");if(weekWins!=weekReceipts.Count||seasonWins!=seasonReceipts.Count)throw new ArgumentException("Etkinlik sayaç/makbuz uyuşmazlığı");if(owned.Distinct().Count()!=owned.Count||owned.Any(id=>!RewardHubRules.KnownItem(id)))throw new ArgumentException("Koleksiyon kaydı geçersiz");for(int i=0;i<3;i++)if(seasonClaimed[i]&&seasonWins<RewardHubRules.Threshold(i))throw new ArgumentException("Sezon ödül kaydı geçersiz");}
 }
 public static class RewardHubRules
 {
  public const string StarBadge="star_badge.v1",WizardCard="wizard_card.v1";
  public static bool KnownItem(string id)=>id==StarBadge||id==WizardCard;
  public static int Threshold(int tier)=>2*(tier+1);
  public static BoosterKind Reward(int tier)=>tier==0?BoosterKind.Undo:tier==1?BoosterKind.Hint:BoosterKind.ExtraBottle;
  public static int Week(DateTimeOffset now)=>DailyRules.Day(now)-(((int)now.DayOfWeek+6)%7);
  public static int Season(DateTimeOffset now){int day=DailyRules.Day(now),epoch=(int)(new DateTime(2026,1,5).Ticks/TimeSpan.TicksPerDay);return Math.Max(0,day-((day-epoch)%28+28)%28);}
  public static bool Blocked(RewardHubData d,DateTimeOffset now)=>DailyRules.Day(now)<d.highestDay||now.UtcTicks<d.highestUtc-TimeSpan.FromMinutes(5).Ticks;
  public static bool Sync(RewardHubData d,DateTimeOffset now)
  {if(Blocked(d,now))return false;int week=Week(now),season=Season(now);if(week!=d.weekStart){d.weekStart=week;d.weekWins=0;d.weeklyClaimed=false;d.weekReceipts.Clear();}if(season!=d.seasonStart){d.seasonStart=season;d.seasonWins=0;d.seasonClaimed=new bool[3];d.seasonReceipts.Clear();}d.highestDay=Math.Max(d.highestDay,DailyRules.Day(now));d.highestUtc=Math.Max(d.highestUtc,now.UtcTicks);return true;}
  public static bool Victory(RewardHubData d,DateTimeOffset now,string receipt)
  {
   if(string.IsNullOrWhiteSpace(receipt)||receipt.Length>80||!Sync(d,now))return false;bool changed=false;
   if(d.weekWins<3&&!d.weekReceipts.Contains(receipt)){d.weekReceipts.Add(receipt);d.weekWins++;changed=true;}
   if(d.seasonWins<6&&!d.seasonReceipts.Contains(receipt)){d.seasonReceipts.Add(receipt);d.seasonWins++;changed=true;}
   return changed;
  }
 }
 public sealed partial class ProgressLedger
 {
  public RewardHubData HubSnapshot=>state.hub.Copy();
  public bool HubClockBlocked=>DailyClockBlocked||RewardHubRules.Blocked(state.hub,clock());
  bool SyncHub(ProgressData copy,DateTimeOffset now)=>DailyRules.Sync(copy.daily,now)&&RewardHubRules.Sync(copy.hub,now);
  public bool ObserveHub(){var copy=state.Copy();if(!SyncHub(copy,clock()))return false;return Commit(copy);}
  public bool OwnsCollection(string id)=>RewardHubRules.KnownItem(id)&&state.hub.owned.Contains(id);
  static void UnlockCollection(RewardHubData d,string id){if(!d.owned.Contains(id))d.owned.Add(id);}
  public bool ClaimWeekly()
  {var copy=state.Copy();if(!SyncHub(copy,clock())||copy.hub.weekWins<3||copy.hub.weeklyClaimed)return false;try{copy.shuffle=checked(copy.shuffle+1);}catch(OverflowException){return false;}copy.hub.weeklyClaimed=true;UnlockCollection(copy.hub,RewardHubRules.StarBadge);return Commit(copy);}
  public bool ClaimSeason(int tier)
  {if(tier<0||tier>=3)return false;var copy=state.Copy();if(!SyncHub(copy,clock())||copy.hub.seasonWins<RewardHubRules.Threshold(tier)||copy.hub.seasonClaimed[tier])return false;var kind=RewardHubRules.Reward(tier);try{Set(copy,kind,checked(Get(copy,kind)+1));}catch(OverflowException){return false;}copy.hub.seasonClaimed[tier]=true;if(tier==2)UnlockCollection(copy.hub,RewardHubRules.WizardCard);return Commit(copy);}
 }
}
