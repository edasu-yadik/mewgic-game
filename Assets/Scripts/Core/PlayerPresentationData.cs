using System;
namespace Mewgic.Core
{
    [Serializable] public sealed class PlayerPresentationData
    {
        public int version=1,currentLevel=1,energy=5;
        public long coins,gems;
        public bool soundEnabled=true;
        public PlayerPresentationData Copy()=>(PlayerPresentationData)MemberwiseClone();
        public void Validate(){if(version!=1||currentLevel<1||coins<0||gems<0||energy<0||energy>5)throw new ArgumentException("Geçersiz oyuncu sunum kaydı");}
    }
    public sealed partial class ProgressLedger
    {
        public PlayerPresentationData Presentation=>(state.presentation??new PlayerPresentationData()).Copy();
        public bool SetCurrentLevel(int level){if(level<1)return false;if(Presentation.currentLevel==level)return true;var copy=state.Copy();copy.presentation.currentLevel=level;return Commit(copy);}
        public bool SetSoundEnabled(bool enabled){if(Presentation.soundEnabled==enabled)return true;var copy=state.Copy();copy.presentation.soundEnabled=enabled;return Commit(copy);}
        public bool SetResources(long coins,long gems,int energy){if(coins<0||gems<0||energy<0||energy>5)return false;var copy=state.Copy();copy.presentation.coins=coins;copy.presentation.gems=gems;copy.presentation.energy=energy;return Commit(copy);}
        // No purchases or currency rewards exist yet. Stored balances are separate from art preview values.
    }
}
