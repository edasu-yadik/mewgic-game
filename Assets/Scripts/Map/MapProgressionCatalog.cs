using System;using System.Collections.Generic;using Mewgic.Core;using UnityEngine;
namespace Mewgic.Map
{
 public static class MapProgressionCatalog
 {
  static readonly Dictionary<int,bool> verified=new Dictionary<int,bool>();
  public static bool Verified(int id){if(id<1)return false;bool ok;if(id<=2&&verified.TryGetValue(id,out ok))return ok;try{GameplayLevelCatalog.Load(id);ok=true;}catch{ok=false;}if(id<=2)verified[id]=ok;return ok;}
  public static MapDefinition Definition(){var asset=Resources.Load<TextAsset>("Map/World");if(asset==null)throw new InvalidOperationException("Harita kataloğu bulunamadı.");return JsonUtility.FromJson<MapDefinition>(asset.text);}
  public static bool Addressable(int id)=>id>=3||Verified(id);
  public static MapProgression Open(ProgressLedger ledger)=>new MapProgression(Definition(),ledger,Addressable);
  public static bool Ready {get{try{Open(GameplayProgress.Current);return Verified(1);}catch{return false;}}}
 }
}
