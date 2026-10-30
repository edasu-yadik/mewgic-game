#if UNITY_EDITOR
using UnityEngine;using Mewgic.Core;
namespace Mewgic
{
    // Yalnız Editor incelemesi: kullanıcı gerçek kaydına yazmadan oynayabilir.
    [DefaultExecutionOrder(-10000)]public sealed class Part10PreviewGuard:MonoBehaviour
    {
        static Part10PreviewGuard active;bool owns;
        void Awake(){if(active!=null){Destroy(gameObject);return;}active=this;owns=true;DontDestroyOnLoad(gameObject);GameplayProgress.UseTestProfile(new ProgressData{completedTutorials=new System.Collections.Generic.List<string>{Mewgic.UI.PawGuide.BasicId}});GameplayLevelCatalog.ResetSelection();}
        void OnDestroy(){if(owns){GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.ResetSelection();}}
    }
}
#endif
