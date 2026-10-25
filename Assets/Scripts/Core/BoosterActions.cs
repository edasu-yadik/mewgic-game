using System;using System.Collections.Generic;using System.Linq;
namespace Mewgic.Core
{
    public enum BoosterKind{Undo,Hint,ExtraBottle,Shuffle}
    [Serializable] public sealed class LevelRecord{public int level;public int bestStars;public bool rewardClaimed;}
    [Serializable] public sealed class ProgressData
    {
        public int version=1;public int undo=3,hint=3,extra=1,shuffle=1;
        public List<LevelRecord> levels=new List<LevelRecord>();
        public ProgressData Copy()=>new ProgressData{version=version,undo=undo,hint=hint,extra=extra,shuffle=shuffle,levels=levels.Select(l=>new LevelRecord{level=l.level,bestStars=l.bestStars,rewardClaimed=l.rewardClaimed}).ToList()};
    }
    public sealed class ProgressLedger
    {
        ProgressData state;readonly Action<ProgressData> save;
        public string LastError {get;private set;}
        public ProgressData Snapshot=>state.Copy();
        public ProgressLedger(ProgressData data,Action<ProgressData> persist)
        {
            if(data==null||data.version!=1||data.levels==null||data.undo<0||data.hint<0||data.extra<0||data.shuffle<0||data.levels.Any(l=>l==null||l.level<1||l.bestStars<1||l.bestStars>3)||data.levels.Select(l=>l.level).Distinct().Count()!=data.levels.Count)throw new ArgumentException("Geçersiz ilerleme kaydı");
            state=data.Copy();save=persist??throw new ArgumentNullException(nameof(persist));
        }
        public int Remaining(BoosterKind kind)=>Enum.IsDefined(typeof(BoosterKind),kind)?Get(state,kind):0;
        static int Get(ProgressData d,BoosterKind k)=>k==BoosterKind.Undo?d.undo:k==BoosterKind.Hint?d.hint:k==BoosterKind.ExtraBottle?d.extra:d.shuffle;
        static void Set(ProgressData d,BoosterKind k,int value){switch(k){case BoosterKind.Undo:d.undo=value;break;case BoosterKind.Hint:d.hint=value;break;case BoosterKind.ExtraBottle:d.extra=value;break;case BoosterKind.Shuffle:d.shuffle=value;break;}}
        bool Commit(ProgressData copy){try{save(copy.Copy());state=copy;LastError=null;return true;}catch(Exception e){LastError=e.Message;return false;}}
        public bool TrySpend(BoosterKind kind){if(Remaining(kind)<=0)return false;var copy=state.Copy();Set(copy,kind,Get(copy,kind)-1);return Commit(copy);}
        public bool Completed(int level)=>state.levels.Any(l=>l.level==level&&l.rewardClaimed);
        // The award and its idempotency marker are saved in the same document.
        public bool Claim(int level,int stars,bool won,out bool granted)
        {
            granted=false;if(!won||level<1||stars<1||stars>3)return false;
            var existing=state.levels.FirstOrDefault(l=>l.level==level);
            if(existing!=null&&existing.rewardClaimed&&existing.bestStars>=stars)return true;
            var copy=state.Copy();var record=copy.levels.FirstOrDefault(l=>l.level==level);
            if(record==null){record=new LevelRecord{level=level};copy.levels.Add(record);}
            bool first=!record.rewardClaimed;record.bestStars=Math.Max(record.bestStars,stars);
            if(first){copy.hint=checked(copy.hint+1);record.rewardClaimed=true;}
            if(!Commit(copy))return false;granted=first;return true;
        }
    }
    public sealed class BoosterActions
    {
        readonly Puzzle puzzle;readonly ProgressLedger ledger;readonly int maximumBottles;
        public string Message {get;private set;}
        public BoosterActions(Puzzle model,ProgressLedger progress,int originalBottleCount){puzzle=model;ledger=progress;maximumBottles=originalBottleCount+1;}
        bool Allowed(BoosterKind kind){if(puzzle.Won){Message="Bölüm tamamlandı.";return false;}if(ledger.Remaining(kind)<=0){Message="Kullanım hakkı kalmadı. Satın alma sistemi yok.";return false;}return true;}
        bool Spend(BoosterKind kind){if(ledger.TrySpend(kind))return true;Message="Hak kaydedilemedi; işlem uygulanmadı.";return false;}
        public bool Undo(){if(!Allowed(BoosterKind.Undo))return false;if(!puzzle.CanUndo){Message="Geri alınacak geçerli hamle yok.";return false;}if(!Spend(BoosterKind.Undo))return false;return puzzle.Undo();}
        public bool Hint(out PuzzleMove move)
        {
            move=default;if(!Allowed(BoosterKind.Hint))return false;var solution=PuzzleSolver.Solve(puzzle.Export());
            if(solution.Status!=SearchStatus.Solved||solution.Moves.Count==0){Message=solution.Status==SearchStatus.LimitReached?"Arama sınırına ulaşıldı; hak harcanmadı.":"Doğrulanmış çözüm yok; hak harcanmadı.";return false;}
            if(!Spend(BoosterKind.Hint))return false;move=solution.Moves[0];Message="İpucu: "+(move.Source+1)+" → "+(move.Target+1);return true;
        }
        public bool Extra(){if(!Allowed(BoosterKind.ExtraBottle))return false;if(puzzle.Count>=maximumBottles){Message="Bu denemede en fazla bir ek şişe kullanılabilir.";return false;}if(!Spend(BoosterKind.ExtraBottle))return false;return puzzle.AddEmptyBottle(maximumBottles);}
        public bool Shuffle()
        {
            if(!Allowed(BoosterKind.Shuffle))return false;var snapshot=puzzle.Export();var probe=new Puzzle(snapshot);if(!probe.RotateBottles()){Message="Değiştirilecek düzen yok; hak harcanmadı.";return false;}
            if(PuzzleSolver.Solve(snapshot).Status!=SearchStatus.Solved||PuzzleSolver.Solve(probe.Export()).Status!=SearchStatus.Solved){Message="Çözüm doğrulanamadı; hak harcanmadı.";return false;}
            if(!Spend(BoosterKind.Shuffle))return false;puzzle.RotateBottles();Message="Şişeler bir sıra kaydırıldı; sıvılar karıştırılmadı.";return true;
        }
    }
}
