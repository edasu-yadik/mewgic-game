using System;
namespace Mewgic
{
    /// <summary>Preview-only observable data; never reads or writes player saves.</summary>
    public sealed class HomeResourceModel
    {
        public const int MaximumEnergy=5;
        public long Coins {get;private set;}
        public long Gems {get;private set;}
        public int Energy {get;private set;}
        public bool IsPreview=>true;
        public event Action Changed;
        public HomeResourceModel(long coins=12450,long gems=853,int energy=5){SetValues(coins,gems,energy);}
        public void SetValues(long coins,long gems,int energy)
        {
            coins=Math.Max(0,coins);gems=Math.Max(0,gems);energy=Math.Max(0,Math.Min(MaximumEnergy,energy));
            if(Coins==coins&&Gems==gems&&Energy==energy)return;
            Coins=coins;Gems=gems;Energy=energy;Changed?.Invoke();
        }
    }
}
