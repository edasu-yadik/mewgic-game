using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
 public enum DailyTaskKind {Bottles,Wins,Stars,Boosters}
 [Serializable]public sealed class DailyData
 {
  public int version=1,day,highestDay,lastGiftDay;public long highestUtc,lastGiftUtc,taskCycleUtc;
  public int[] progress=new int[4];public bool[] claimed=new bool[4];public List<string> wins=new List<string>();
  public DailyData Copy()=>new DailyData{taskCycleUtc=taskCycleUtc,version=version,day=day,highestDay=highestDay,lastGiftDay=lastGiftDay,highestUtc=highestUtc,lastGiftUtc=lastGiftUtc,progress=progress.ToArray(),claimed=claimed.ToArray(),wins=new List<string>(wins)};
  public void Validate(){if(taskCycleUtc<0||taskCycleUtc>highestUtc||version!=1||day<0||highestDay<day||highestDay>3652059||lastGiftDay<0||lastGiftDay>highestDay||highestUtc<0||highestUtc>DateTime.MaxValue.Ticks||lastGiftUtc<0||lastGiftUtc>highestUtc||progress==null||progress.Length!=4||claimed==null||claimed.Length!=4||wins==null||wins.Count>20||wins.Any(string.IsNullOrWhiteSpace)||wins.Distinct().Count()!=wins.Count)throw new ArgumentException("Günlük kayıt geçersiz");for(int i=0;i<4;i++)if(progress[i]<0||progress[i]>DailyRules.Target((DailyTaskKind)i)||(claimed[i]&&progress[i]<DailyRules.Target((DailyTaskKind)i)))throw new ArgumentException("Görev kaydı geçersiz");}
 }
 public static class DailyRules
 {
  public static int Day(DateTimeOffset now)=>(int)(now.Date.Ticks/TimeSpan.TicksPerDay);
  public static int Target(DailyTaskKind kind)=>kind==DailyTaskKind.Bottles?4:kind==DailyTaskKind.Wins?2:kind==DailyTaskKind.Stars?6:1;
  public static BoosterKind Reward(DailyTaskKind kind)=>kind==DailyTaskKind.Bottles?BoosterKind.Undo:kind==DailyTaskKind.Wins?BoosterKind.Hint:kind==DailyTaskKind.Stars?BoosterKind.ExtraBottle:BoosterKind.Shuffle;
  public static string Title(DailyTaskKind kind)=>kind==DailyTaskKind.Bottles?"Kazandığın bölümlerde 4 şişe tamamla":kind==DailyTaskKind.Wins?"2 bölüm kazan":kind==DailyTaskKind.Stars?"Kazandığın bölümlerde 6 yıldız topla":"1 güçlendirici kullan";
  public static bool Blocked(DailyData d,DateTimeOffset now)=>now.UtcTicks<d.highestUtc-TimeSpan.FromMinutes(5).Ticks||Day(now)<d.highestDay;
  public static bool Sync(DailyData d,DateTimeOffset now)
  {
   if(Blocked(d,now))return false;
   // Eski kayıtlarda mevcut ilerlemeyi koruyarak ilk 24 saatlik döngüyü başlat.
   if(d.taskCycleUtc==0)d.taskCycleUtc=now.UtcTicks;
   long elapsed=now.UtcTicks-d.taskCycleUtc;
   if(elapsed>=TimeSpan.TicksPerDay){d.taskCycleUtc+=(elapsed/TimeSpan.TicksPerDay)*TimeSpan.TicksPerDay;d.progress=new int[4];d.claimed=new bool[4];d.wins.Clear();}
   int day=Day(now);d.day=day;d.highestDay=Math.Max(d.highestDay,day);d.highestUtc=Math.Max(d.highestUtc,now.UtcTicks);return true;
  }
  public static TimeSpan TaskWait(DailyData d,DateTimeOffset now)=>TimeSpan.FromTicks(d.taskCycleUtc==0?TimeSpan.TicksPerDay:Math.Max(0,Math.Min(TimeSpan.TicksPerDay,TimeSpan.TicksPerDay-(now.UtcTicks-d.taskCycleUtc))));
  public static void Add(DailyData d,DailyTaskKind kind,int n){int i=(int)kind;d.progress[i]=(int)Math.Min(Target(kind),(long)d.progress[i]+n);}
  public static bool Victory(DailyData d,DateTimeOffset now,string receipt,int stars,int bottles)
  {
   if(string.IsNullOrWhiteSpace(receipt)||receipt.Length>80||stars<1||stars>3||bottles<1||bottles>24||!Sync(d,now)||d.wins.Contains(receipt))return false;
   if(d.progress[0]>=4&&d.progress[1]>=2&&d.progress[2]>=6)return false;
   d.wins.Add(receipt);Add(d,DailyTaskKind.Bottles,bottles);Add(d,DailyTaskKind.Wins,1);Add(d,DailyTaskKind.Stars,stars);return true;
  }
 }
 public sealed partial class ProgressLedger
 {
  readonly Func<DateTimeOffset> clock; readonly DateTimeOffset giftClockStart; readonly Func<TimeSpan> giftElapsed;
  public DateTimeOffset LocalNow=>clock();
  public const int DailyGiftCoins=100, DailyGiftHints=1; public long LastGiftUtc=>state.daily.lastGiftUtc;
  public DailyData DailySnapshot=>state.daily.Copy();
  public TimeSpan DailyTaskWait=>DailyRules.TaskWait(state.daily,clock());
  public bool DailyClockBlocked=>DailyRules.Blocked(state.daily,clock());
  public bool ObserveDaily(){var copy=state.Copy();if(!DailyRules.Sync(copy.daily,clock()))return false;return Commit(copy);}
  public TimeSpan GiftWait {get{var d=state.daily;long remaining=d.lastGiftUtc==0?0:TimeSpan.FromHours(24).Ticks-(clock().UtcTicks-d.lastGiftUtc);return TimeSpan.FromTicks(Math.Max(0,remaining));}}
  bool GiftTimeBlocked(DateTimeOffset now)=>DailyRules.Blocked(state.daily,now)||
   (giftElapsed!=null&&Math.Abs((now-giftClockStart-giftElapsed()).TotalMinutes)>5);
  public bool GiftClockBlocked=>GiftTimeBlocked(clock());
  public bool GiftReady=>!GiftClockBlocked&&GiftWait==TimeSpan.Zero;
  public bool ClaimDailyGift(long? expectedLastGift=null)
  {
   if(expectedLastGift.HasValue&&expectedLastGift.Value!=state.daily.lastGiftUtc)return false;
   var now=clock();if(GiftTimeBlocked(now))return false;var copy=state.Copy();if(!DailyRules.Sync(copy.daily,now)||(copy.daily.lastGiftUtc>0&&now.UtcTicks-copy.daily.lastGiftUtc<TimeSpan.FromHours(24).Ticks))return false;
   try{copy.hint=checked(copy.hint+DailyGiftHints);copy.presentation.coins=checked(copy.presentation.coins+DailyGiftCoins);}catch(OverflowException){return false;}copy.daily.lastGiftDay=copy.daily.day;copy.daily.lastGiftUtc=now.UtcTicks;return Commit(copy);
  }
  public bool ClaimDailyTask(DailyTaskKind kind)
  {
   if(!Enum.IsDefined(typeof(DailyTaskKind),kind))return false;var copy=state.Copy();if(!DailyRules.Sync(copy.daily,clock()))return false;int i=(int)kind;if(copy.daily.claimed[i]||copy.daily.progress[i]<DailyRules.Target(kind))return false;
   var reward=DailyRules.Reward(kind);try{Set(copy,reward,checked(Get(copy,reward)+1));}catch(OverflowException){return false;}copy.daily.claimed[i]=true;return Commit(copy);
  }
 }
}
