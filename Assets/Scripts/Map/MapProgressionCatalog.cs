using System;using System.Collections.Generic;using Mewgic.Core;using UnityEngine;
namespace Mewgic.Map
{
 public static class MapProgressionCatalog
 {
  static readonly Dictionary<int,bool> verified=new Dictionary<int,bool>();
  public static bool Verified(int id){if(id<1)return false;if(verified.TryGetValue(id,out bool ok))return ok;try{GameplayLevelCatalog.Load(id);ok=true;}catch{ok=false;}verified[id]=ok;return ok;}
  public static MapDefinition Definition(){var asset=Resources.Load<TextAsset>("Map/World");if(asset==null)throw new InvalidOperationException("Harita kataloğu bulunamadı.");return JsonUtility.FromJson<MapDefinition>(asset.text);}
  public static MapProgression Open(ProgressLedger ledger)=>new MapProgression(Definition(),ledger,Verified);
  public static bool Ready {get{try{Open(GameplayProgress.Current);return Verified(1);}catch{return false;}}}
 }
}
