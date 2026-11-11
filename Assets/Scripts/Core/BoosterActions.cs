using L=Mewgic.Localization.L;
using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
    public enum BoosterKind{Undo,Hint,ExtraBottle,Shuffle,ColorSwap}
    [Serializable] public sealed class LevelRecord{public int level;public int bestStars;public bool rewardClaimed;public string lastCoinReceipt,rewardedVictoryReceipt;public int rewardedVictoryBase;}
    [Serializable] public sealed class ProgressData
    {
        public DailyData daily=new DailyData();public RewardHubData hub=new RewardHubData();
        public WorldMapData worldMap=new WorldMapData();
        public ShopData shop=new ShopData();
        public LeagueWalletData leagueWallet=new LeagueWalletData();
        public PlayerPresentationData presentation=new PlayerPresentationData();
        public List<int> attemptedLevels=new List<int>(); public List<string> completedTutorials=new List<string>();
        public int version=1;public long storageRevision;public int undo=EconomyConfig.Current.initialUndo,hint=EconomyConfig.Current.initialHint,extra=EconomyConfig.Current.initialExtra,shuffle=EconomyConfig.Current.initialShuffle;
        public int colorSwap;
        public List<LevelRecord> levels=new List<LevelRecord>();public List<string> claimedMilestones=new List<string>();
        public ProgressData Copy()=>new ProgressData{leagueWallet=(leagueWallet??new LeagueWalletData()).Copy(),shop=(shop??new ShopData()).Copy(),worldMap=(worldMap??new WorldMapData()).Copy(),version=version,storageRevision=storageRevision,attemptedLevels=attemptedLevels==null?new List<int>():new List<int>(attemptedLevels),completedTutorials=completedTutorials==null?new List<string>():new List<string>(completedTutorials),presentation=(presentation??new PlayerPresentationData()).Copy(),hub=(hub??new RewardHubData()).Copy(),daily=(daily??new DailyData()).Copy(),undo=undo,hint=hint,extra=extra,shuffle=shuffle,colorSwap=colorSwap,claimedMilestones=claimedMilestones==null?new List<string>():new List<string>(claimedMilestones),levels=levels.Select(l=>new LevelRecord{level=l.level,bestStars=l.bestStars,rewardClaimed=l.rewardClaimed,lastCoinReceipt=l.lastCoinReceipt,rewardedVictoryReceipt=l.rewardedVictoryReceipt,rewardedVictoryBase=l.rewardedVictoryBase}).ToList()};
    }
    public sealed partial class ProgressLedger
    {
        ProgressData state;readonly Action<ProgressData> save;
        public CompletionReward LastCompletionReward {get;private set;} public int LastCoinReward=>LastCompletionReward.Coins;
        public string LastError {get;private set;}public int Revision{get;private set;}
        public ProgressData Snapshot=>state.Copy();
        public ProgressLedger(ProgressData data,Action<ProgressData> persist,Func<DateTimeOffset> localClock=null,Func<TimeSpan> elapsedTime=null)
        {
            if(data==null||data.version!=1||data.levels==null||data.undo<0||data.hint<0||data.extra<0||data.shuffle<0||data.colorSwap<0||data.levels.Any(l=>l==null||l.level<1||l.bestStars<1||l.bestStars>3)||data.levels.Select(l=>l.level).Distinct().Count()!=data.levels.Count)throw new ArgumentException("Geçersiz ilerleme kaydı");
            if(data.claimedMilestones!=null&&(data.claimedMilestones.Any(string.IsNullOrWhiteSpace)||data.claimedMilestones.Distinct().Count()!=data.claimedMilestones.Count))throw new ArgumentException("Geçersiz sandık kaydı");
            if(data.completedTutorials!=null&&(data.completedTutorials.Any(id=>string.IsNullOrWhiteSpace(id)||id.Length>120)||data.completedTutorials.Distinct().Count()!=data.completedTutorials.Count))throw new ArgumentException("Geçersiz öğretici geçmişi");
            (data.leagueWallet??new LeagueWalletData()).Validate();(data.shop??new ShopData()).Validate();(data.presentation??new PlayerPresentationData()).Validate();(data.hub??new RewardHubData()).Validate();(data.daily??new DailyData()).Validate();clock=localClock??(()=>DateTimeOffset.Now);giftClockStart=clock();giftElapsed=elapsedTime;if(giftElapsed==null&&localClock==null){var session=System.Diagnostics.Stopwatch.StartNew();giftElapsed=()=>session.Elapsed;}
            state=data.Copy();if(state.hub.owned.Contains(RewardHubRules.StarBadge))state.shop.Grant(ShopItem.RoseEffect);if(state.hub.owned.Contains(RewardHubRules.WizardCard))state.shop.Grant(ShopItem.GoldBottle);CurrentUnlockedLevel=WorldMapRules.Initialize(state);
            HighestCompletedLevel=CompletedRecords.Select(record=>record.level).DefaultIfEmpty(0).Max();
            save=persist??throw new ArgumentNullException(nameof(persist));
        }
        public int Remaining(BoosterKind kind)=>Enum.IsDefined(typeof(BoosterKind),kind)?Get(state,kind):0;
        static int Get(ProgressData d,BoosterKind k)=>k switch
        {
            BoosterKind.Undo=>d.undo,BoosterKind.Hint=>d.hint,BoosterKind.ExtraBottle=>d.extra,
            BoosterKind.Shuffle=>d.shuffle,BoosterKind.ColorSwap=>d.colorSwap,_=>0
        };
        static void Set(ProgressData d,BoosterKind k,int value){switch(k){case BoosterKind.Undo:d.undo=value;break;case BoosterKind.Hint:d.hint=value;break;case BoosterKind.ExtraBottle:d.extra=value;break;case BoosterKind.Shuffle:d.shuffle=value;break;case BoosterKind.ColorSwap:d.colorSwap=value;break;}}
        bool Commit(ProgressData copy){try{save(copy.Copy());state=copy;Revision++;LastError=null;return true;}catch(Exception e){LastError=e.Message;return false;}}
        public bool TrySpend(BoosterKind kind){if(Remaining(kind)<=0)return false;var copy=state.Copy();Set(copy,kind,Get(copy,kind)-1);if(DailyRules.Sync(copy.daily,clock()))DailyRules.Add(copy.daily,DailyTaskKind.Boosters,1);return Commit(copy);}
        public bool BeginFirstAttempt(int level)
        {
            if(level<1||Completed(level)||state.attemptedLevels.Contains(level))return false;
            var copy=state.Copy();
            copy.attemptedLevels.Add(level);
            return Commit(copy);
        }

        public static int BoosterPrice(BoosterKind kind) => EconomyConfig.Current.Price(ShopCatalog.Product(kind));

        public bool BuyBooster(BoosterKind kind) =>
            Purchase(ShopCatalog.Product(kind), BoosterPrice(kind), NextShopPurchase) == ShopPurchaseResult.Purchased;

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
        public bool Claim(int level,int stars,bool won,out bool granted,string victoryReceipt=null,int completedBottles=0,bool firstAttempt=false,bool hintUsed=true)
        {
            LastCompletionReward=default;granted=false;if(!won||level<1||level>WorldMapRules.MaxLevel||stars<1||stars>3)return false;
            var existing=state.levels.FirstOrDefault(l=>l.level==level);
            if(existing!=null&&!string.IsNullOrEmpty(victoryReceipt)&&existing.lastCoinReceipt==victoryReceipt)return true;
            var copy=state.Copy();var now=clock();bool dailyChanged=DailyRules.Victory(copy.daily,now,victoryReceipt,stars,completedBottles);bool hubChanged=!DailyRules.Blocked(copy.daily,now)&&RewardHubRules.Victory(copy.hub,now,victoryReceipt);if(hubChanged)DailyRules.Sync(copy.daily,now);
            if(existing!=null&&existing.rewardClaimed&&existing.bestStars>=stars&&!dailyChanged&&!hubChanged)return true;
            var record=copy.levels.FirstOrDefault(l=>l.level==level);
            if(record==null){record=new LevelRecord{level=level};copy.levels.Add(record);}
            bool first=!record.rewardClaimed;var reward=new CompletionReward(first?0:record.bestStars,stars,first,firstAttempt&&copy.attemptedLevels.Contains(level),hintUsed);record.bestStars=Math.Max(record.bestStars,stars);
            if(first){try{copy.hint=checked(copy.hint+EconomyConfig.Current.firstClearHints);}catch(OverflowException){LastError="Ödül sayacı sınırda.";return false;}record.rewardClaimed=true;}
            try{copy.presentation.coins=checked(copy.presentation.coins+reward.Coins);}catch(OverflowException){return false;}
            if(!string.IsNullOrWhiteSpace(victoryReceipt)&&victoryReceipt.Length<=80)record.lastCoinReceipt=victoryReceipt;
            if(first && !string.IsNullOrWhiteSpace(victoryReceipt) && victoryReceipt.Length<=80)
            {
                record.rewardedVictoryReceipt=victoryReceipt;
                record.rewardedVictoryBase=reward.Coins;
            }
            copy.attemptedLevels.Remove(level);
            bool frontier=first&&CurrentUnlockedLevel==level;
            int unlocked=CurrentUnlockedLevel;
            if(frontier&&level<WorldMapRules.MaxLevel)
                unlocked=HighestCompletedLevel>level?WorldMapRules.CurrentLevel(copy):level+1;
            WorldMapRules.Sync(copy,unlocked);
            if(frontier)
            {
                copy.presentation.currentLevel=unlocked;
                if(unlocked>level) MapJourneyRules.Queue(copy,level,unlocked);
            }
            if(!Commit(copy))return false;
            CurrentUnlockedLevel=unlocked;
            HighestCompletedLevel=Math.Max(HighestCompletedLevel,level);
            if(first||existing.bestStars<stars) CompletionRevision++;
            LastCompletionReward=reward;granted=first;
            return true;
        }
    }
    public sealed class BoosterActions
    {
        readonly Puzzle puzzle;readonly ProgressLedger ledger;readonly int maximumBottles,level;
        public string Message {get;private set;}
        public BoosterActions(Puzzle model,ProgressLedger progress,int originalBottleCount,int level=1){puzzle=model;ledger=progress;maximumBottles=originalBottleCount+1;this.level=level;}
        bool Allowed(BoosterKind kind){if(puzzle.Won){Message=L.V("gameplay.success");return false;}if(ledger.Remaining(kind)<=0){Message=L.V("booster.none");return false;}return true;}
        bool Spend(BoosterKind kind){if(ledger.TrySpend(kind))return true;Message=L.V("error.save");return false;}
        public bool Undo(){if(!Allowed(BoosterKind.Undo))return false;if(!puzzle.CanUndo){Message=L.V("booster.no_undo");return false;}if(!Spend(BoosterKind.Undo))return false;return puzzle.Undo();}
        public bool Hint(out PuzzleMove move)
        {
            move=default;if(!Allowed(BoosterKind.Hint))return false;
            PuzzleSolution solution;
            using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            try { solution=PuzzleSolver.Solve(puzzle.Export(),cancellation:deadline.Token); }
            catch(OperationCanceledException) { Message=L.V("booster.no_hint");return false; }
            if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0){Message=L.V("booster.no_hint");return false;}
            if(!Spend(BoosterKind.Hint))return false;move=solution.Moves[0];Message="";return true;
        }
        public bool Extra(){if(!Allowed(BoosterKind.ExtraBottle))return false;if(puzzle.Count>=maximumBottles){Message=L.V("booster.extra_limit");return false;}if(!Spend(BoosterKind.ExtraBottle))return false;return puzzle.AddEmptyBottle(maximumBottles);}
        public bool ColorSwap(int source,out int target)
        {
            target=-1;
            if(!Allowed(BoosterKind.ColorSwap))return false;
            Message=L.V("booster.color_unavailable");
            if(level<EconomyConfig.Current.colorSwapUnlock||puzzle.ColorSwapUsed||source<0||source>=puzzle.Count)return false;
            var board=puzzle.Export();
            if(!CanSwap(board[source]))return false;

            using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(120));
            try
            {
                var before=PuzzleSolver.Solve(board,6000,deadline.Token);
                if(before.Status==SearchStatus.LimitReached)return false;
                int length=before.Status==SearchStatus.Solved?before.Moves.Count:int.MaxValue;
                for(int candidate=0;candidate<board.Length;candidate++)
                {
                    if(candidate==source||!CanSwap(board[candidate])||board[source][^1]==board[candidate][^1])continue;
                    var probe=new Puzzle(board);
                    probe.ApplyColorSwap(source,candidate);
                    if(probe.Won)continue;
                    var after=PuzzleSolver.Solve(probe.Export(),6000,deadline.Token);
                    if(after.Status!=SearchStatus.Solved||after.Moves.Count>=length)continue;
                    deadline.Token.ThrowIfCancellationRequested();
                    if(!Spend(BoosterKind.ColorSwap))return false;
                    puzzle.ApplyColorSwap(source,candidate);
                    target=candidate;
                    Message="";
                    return true;
                }
            }
            catch(OperationCanceledException) { }
            return false;
        }

        static bool CanSwap(int[] bottle)=>bottle.Length>0 &&
            (bottle.Length<Puzzle.Capacity||bottle.Any(color=>color!=bottle[0]));

        public bool Shuffle()
        {
            if(!Allowed(BoosterKind.Shuffle))return false;
            Message=L.V("booster.no_shuffle");
            var snapshot=puzzle.Export();
            if(!new Puzzle(snapshot).RotateBottles())return false;
            using var deadline=new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            try
            {
                if(PuzzleSolver.Solve(snapshot,cancellation:deadline.Token).Status!=SearchStatus.Solved)return false;
            }
            catch(OperationCanceledException) { return false; }
            // A whole-bottle permutation preserves the verified solution and its length.
            if(!Spend(BoosterKind.Shuffle))return false;
            puzzle.RotateBottles();
            Message=L.V("booster.used_shuffle");
            return true;
        }
    }
}
