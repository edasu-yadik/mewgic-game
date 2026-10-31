using L=Mewgic.Localization.L;
using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
    public enum BoosterKind{Undo,Hint,ExtraBottle,Shuffle}
    [Serializable] public sealed class LevelRecord{public int level;public int bestStars;public bool rewardClaimed;}
    [Serializable] public sealed class ProgressData
    {
        public DailyData daily=new DailyData();public RewardHubData hub=new RewardHubData();
        public WorldMapData worldMap=new WorldMapData();
        public PlayerPresentationData presentation=new PlayerPresentationData();
        public List<string> completedTutorials=new List<string>();
        public int version=1;public int undo=3,hint=3,extra=1,shuffle=1;
        public List<LevelRecord> levels=new List<LevelRecord>();public List<string> claimedMilestones=new List<string>();
        public ProgressData Copy()=>new ProgressData{worldMap=(worldMap??new WorldMapData()).Copy(),version=version,completedTutorials=completedTutorials==null?new List<string>():new List<string>(completedTutorials),presentation=(presentation??new PlayerPresentationData()).Copy(),hub=(hub??new RewardHubData()).Copy(),daily=(daily??new DailyData()).Copy(),undo=undo,hint=hint,extra=extra,shuffle=shuffle,claimedMilestones=claimedMilestones==null?new List<string>():new List<string>(claimedMilestones),levels=levels.Select(l=>new LevelRecord{level=l.level,bestStars=l.bestStars,rewardClaimed=l.rewardClaimed}).ToList()};
    }
    public sealed partial class ProgressLedger
    {
        ProgressData state;readonly Action<ProgressData> save;
        public string LastError {get;private set;}public int Revision{get;private set;}
        public ProgressData Snapshot=>state.Copy();
        public ProgressLedger(ProgressData data,Action<ProgressData> persist,Func<DateTimeOffset> localClock=null)
        {
            if(data==null||data.version!=1||data.levels==null||data.undo<0||data.hint<0||data.extra<0||data.shuffle<0||data.levels.Any(l=>l==null||l.level<1||l.bestStars<1||l.bestStars>3)||data.levels.Select(l=>l.level).Distinct().Count()!=data.levels.Count)throw new ArgumentException("Geçersiz ilerleme kaydı");
            if(data.claimedMilestones!=null&&(data.claimedMilestones.Any(string.IsNullOrWhiteSpace)||data.claimedMilestones.Distinct().Count()!=data.claimedMilestones.Count))throw new ArgumentException("Geçersiz sandık kaydı");
            if(data.completedTutorials!=null&&(data.completedTutorials.Any(id=>string.IsNullOrWhiteSpace(id)||id.Length>120)||data.completedTutorials.Distinct().Count()!=data.completedTutorials.Count))throw new ArgumentException("Geçersiz öğretici geçmişi");
            (data.presentation??new PlayerPresentationData()).Validate();(data.hub??new RewardHubData()).Validate();(data.daily??new DailyData()).Validate();clock=localClock??(()=>DateTimeOffset.Now);
            state=data.Copy();WorldMapRules.Initialize(state);save=persist??throw new ArgumentNullException(nameof(persist));
        }
        public int Remaining(BoosterKind kind)=>Enum.IsDefined(typeof(BoosterKind),kind)?Get(state,kind):0;
        static int Get(ProgressData d,BoosterKind k)=>k==BoosterKind.Undo?d.undo:k==BoosterKind.Hint?d.hint:k==BoosterKind.ExtraBottle?d.extra:d.shuffle;
        static void Set(ProgressData d,BoosterKind k,int value){switch(k){case BoosterKind.Undo:d.undo=value;break;case BoosterKind.Hint:d.hint=value;break;case BoosterKind.ExtraBottle:d.extra=value;break;case BoosterKind.Shuffle:d.shuffle=value;break;}}
        bool Commit(ProgressData copy){try{save(copy.Copy());state=copy;Revision++;LastError=null;return true;}catch(Exception e){LastError=e.Message;return false;}}
        public bool TrySpend(BoosterKind kind){if(Remaining(kind)<=0)return false;var copy=state.Copy();Set(copy,kind,Get(copy,kind)-1);if(DailyRules.Sync(copy.daily,clock()))DailyRules.Add(copy.daily,DailyTaskKind.Boosters,1);return Commit(copy);}
        public bool Completed(int level)=>state.levels.Any(l=>l.level==level&&l.rewardClaimed);
        public int BestStars(int level)=>state.levels.FirstOrDefault(l=>l.level==level&&l.rewardClaimed)?.bestStars??0;
        public bool MilestoneClaimed(string id)=>state.claimedMilestones.Contains(id);
        public bool ClaimMilestone(MilestoneReward reward,bool eligible,out bool granted)
        {
            granted=false;if(reward==null||string.IsNullOrWhiteSpace(reward.id)||!Enum.IsDefined(typeof(BoosterKind),reward.kind)||reward.amount<1||reward.amount>100||!eligible)return false;
            if(MilestoneClaimed(reward.id))return true;
            var copy=state.Copy();try{Set(copy,reward.kind,checked(Get(copy,reward.kind)+reward.amount));}catch(OverflowException){LastError="Ödül sayacı sınırda.";return false;}
            copy.claimedMilestones.Add(reward.id);if(!Commit(copy))return false;granted=true;return true;
        }
        // The award and its idempotency marker are saved in the same document.
        public bool Claim(int level,int stars,bool won,out bool granted,string victoryReceipt=null,int completedBottles=0)
        {
            granted=false;if(!won||level<1||stars<1||stars>3)return false;
            var existing=state.levels.FirstOrDefault(l=>l.level==level);
            var copy=state.Copy();var now=clock();bool dailyChanged=DailyRules.Victory(copy.daily,now,victoryReceipt,stars,completedBottles);bool hubChanged=!DailyRules.Blocked(copy.daily,now)&&RewardHubRules.Victory(copy.hub,now,victoryReceipt);if(hubChanged)DailyRules.Sync(copy.daily,now);
            if(existing!=null&&existing.rewardClaimed&&existing.bestStars>=stars&&!dailyChanged&&!hubChanged)return true;
            var record=copy.levels.FirstOrDefault(l=>l.level==level);
            if(record==null){record=new LevelRecord{level=level};copy.levels.Add(record);}
            bool first=!record.rewardClaimed;record.bestStars=Math.Max(record.bestStars,stars);
            if(first){try{copy.hint=checked(copy.hint+1);}catch(OverflowException){LastError="Ödül sayacı sınırda.";return false;}record.rewardClaimed=true;}
            WorldMapRules.Sync(copy);if(!Commit(copy))return false;granted=first;return true;
        }
    }
    public sealed class BoosterActions
    {
        readonly Puzzle puzzle;readonly ProgressLedger ledger;readonly int maximumBottles;
        public string Message {get;private set;}
        public BoosterActions(Puzzle model,ProgressLedger progress,int originalBottleCount){puzzle=model;ledger=progress;maximumBottles=originalBottleCount+1;}
        bool Allowed(BoosterKind kind){if(puzzle.Won){Message=L.V("gameplay.success");return false;}if(ledger.Remaining(kind)<=0){Message=L.V("booster.none");return false;}return true;}
        bool Spend(BoosterKind kind){if(ledger.TrySpend(kind))return true;Message=L.V("error.save");return false;}
        public bool Undo(){if(!Allowed(BoosterKind.Undo))return false;if(!puzzle.CanUndo){Message=L.V("booster.no_undo");return false;}if(!Spend(BoosterKind.Undo))return false;return puzzle.Undo();}
        public bool Hint(out PuzzleMove move)
        {
            move=default;if(!Allowed(BoosterKind.Hint))return false;var solution=PuzzleSolver.Solve(puzzle.Export());
            if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0){Message=L.V("booster.no_hint");return false;}
            if(!Spend(BoosterKind.Hint))return false;move=solution.Moves[0];Message="";return true;
        }
        public bool Extra(){if(!Allowed(BoosterKind.ExtraBottle))return false;if(puzzle.Count>=maximumBottles){Message=L.V("booster.extra_limit");return false;}if(!Spend(BoosterKind.ExtraBottle))return false;return puzzle.AddEmptyBottle(maximumBottles);}
        public bool Shuffle()
        {
            if(!Allowed(BoosterKind.Shuffle))return false;var snapshot=puzzle.Export();var probe=new Puzzle(snapshot);if(!probe.RotateBottles()){Message=L.V("booster.no_shuffle");return false;}
            if(PuzzleSolver.Solve(snapshot).Status!=SearchStatus.Solved||PuzzleSolver.Solve(probe.Export()).Status!=SearchStatus.Solved){Message=L.V("booster.no_shuffle");return false;}
            if(!Spend(BoosterKind.Shuffle))return false;puzzle.RotateBottles();Message=L.V("booster.used_shuffle");return true;
        }
    }
}
