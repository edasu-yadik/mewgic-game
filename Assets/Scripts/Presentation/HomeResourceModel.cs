using System;
namespace Mewgic
{
    /// <summary>Explicit art preview or persisted player presentation; no purchase logic.</summary>
    public sealed class HomeResourceModel
    {
        public const int MaximumEnergy=5;
        public long Coins {get;private set;}
        public long Gems {get;private set;}
        public int Energy {get;private set;}
        readonly Mewgic.Core.ProgressLedger ledger;
        public bool IsPreview=>ledger==null;
        public event Action Changed;
        public HomeResourceModel(long coins=12450,long gems=853,int energy=5){SetValues(coins,gems,energy);}
        public HomeResourceModel(Mewgic.Core.ProgressLedger progress){ledger=progress??throw new ArgumentNullException(nameof(progress));var d=ledger.Presentation;Coins=d.coins;Gems=d.gems;Energy=d.energy;}
        int observedRevision=-1;
        public void RefreshFromLedger(){if(ledger==null||observedRevision==ledger.Revision)return;observedRevision=ledger.Revision;var d=ledger.Presentation;if(Coins==d.coins&&Gems==d.gems&&Energy==d.energy)return;Coins=d.coins;Gems=d.gems;Energy=d.energy;Changed?.Invoke();}
        public void SetValues(long coins,long gems,int energy)
        {
            coins=Math.Max(0,coins);gems=Math.Max(0,gems);energy=Math.Max(0,Math.Min(MaximumEnergy,energy));
            if(Coins==coins&&Gems==gems&&Energy==energy)return;
            if(ledger!=null&&!ledger.SetResources(coins,gems,energy))return;
            Coins=coins;Gems=gems;Energy=energy;Changed?.Invoke();
        }
    }
}
