using System;
namespace Mewgic.Core
{
    [Serializable] public sealed class PlayerPresentationData
    {
        public int version=1,currentLevel=1,energy=5;
        public long coins,gems;
        public bool soundEnabled=true;public string manualLocale;
        public bool audioPreferencesSet,musicEnabled,effectsEnabled,vibrationEnabled,reduceMotion,motionPreferenceSet;
        public bool MusicEnabled=>audioPreferencesSet?musicEnabled:soundEnabled;
        public bool EffectsEnabled=>audioPreferencesSet?effectsEnabled:soundEnabled;
        public PlayerPresentationData Copy()=>(PlayerPresentationData)MemberwiseClone();
        public void Validate(){if(version!=1||currentLevel<1||coins<0||gems<0||energy<0||energy>5||(manualLocale!=null&&manualLocale.Length>20))throw new ArgumentException("Geçersiz oyuncu sunum kaydı");}
    }
    public sealed partial class ProgressLedger
    {
        public bool ReducedMotion=>state.presentation.reduceMotion;
        public PlayerPresentationData Presentation=>(state.presentation??new PlayerPresentationData()).Copy();
        public bool SetCurrentLevel(int level){if(level<1)return false;if(Presentation.currentLevel==level)return true;var copy=state.Copy();copy.presentation.currentLevel=level;return Commit(copy);}
        public bool SetManualLocale(string code){if(string.IsNullOrWhiteSpace(code)||code.Length>20)return false;if(Presentation.manualLocale==code)return true;var copy=state.Copy();copy.presentation.manualLocale=code;return Commit(copy);}
        public bool SetSoundEnabled(bool enabled){var copy=state.Copy();copy.presentation.soundEnabled=enabled;copy.presentation.audioPreferencesSet=true;copy.presentation.musicEnabled=copy.presentation.effectsEnabled=enabled;return Commit(copy);}
        public bool SetResources(long coins,long gems,int energy){if(coins<0||gems<0||energy<0||energy>5)return false;var copy=state.Copy();copy.presentation.coins=coins;copy.presentation.gems=gems;copy.presentation.energy=energy;return Commit(copy);}
        public bool SetFeedbackPreferences(bool music,bool effects,bool vibration,bool reduced)
        {
            var copy=state.Copy();
            var p=copy.presentation;
            p.audioPreferencesSet=p.motionPreferenceSet=true;
            p.musicEnabled=music;p.effectsEnabled=effects;p.vibrationEnabled=vibration;p.reduceMotion=reduced;
            return Commit(copy);
        }
        public bool SetReducedMotion(bool reduced)
        {
            var copy=state.Copy();
            copy.presentation.motionPreferenceSet=true;
            copy.presentation.reduceMotion=reduced;
            return Commit(copy);
        }
    }
}
