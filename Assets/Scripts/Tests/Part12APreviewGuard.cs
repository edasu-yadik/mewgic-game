#if UNITY_EDITOR
using System.Linq;using System.Collections.Generic;using UnityEngine;using Mewgic.Core;using Mewgic.UI;
namespace Mewgic {
 [DefaultExecutionOrder(-10000)]public sealed class Part12APreviewGuard:MonoBehaviour {void Awake(){DontDestroyOnLoad(gameObject);GameplayProgress.UseTestProfile(new ProgressData{completedTutorials=new List<string>{PawGuide.BasicId}});var pool=Newtonsoft.Json.JsonConvert.DeserializeObject<GeneratedLevel[]>(Resources.Load<TextAsset>("Levels/GeneratedFallbacks").text);var l=LevelGenerator.Resolve(GeneratorRequest.ForLevel(8),System.Array.Empty<string>(),pool).level;var archive=new MemoryLevelArchive();archive.Write(l);GameplayLevelCatalog.TestArchive=archive;GameplayProgress.Current.Claim(7,3,true,out _);GameplayProgress.Current.SetCurrentLevel(8);GameplayLevelCatalog.ResumeFromSave();}void OnDestroy(){GameplayProgress.RestorePlayerProfile();GameplayLevelCatalog.TestArchive=null;GameplayLevelCatalog.ResetSelection();}}
}
#endif
