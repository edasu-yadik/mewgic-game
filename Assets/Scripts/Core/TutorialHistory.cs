using System;
namespace Mewgic.Core
{
    public sealed partial class ProgressLedger
    {
        public bool TutorialCompleted(string id)=>!string.IsNullOrWhiteSpace(id)&&state.completedTutorials.Contains(id);
        public bool CompleteTutorial(string id)
        {
            if(string.IsNullOrWhiteSpace(id)||id.Length>120)return false;
            if(TutorialCompleted(id))return true;
            var copy=state.Copy();copy.completedTutorials.Add(id);return Commit(copy);
        }
    }
}
