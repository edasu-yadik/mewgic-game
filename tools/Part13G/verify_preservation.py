from pathlib import Path
import hashlib,json,subprocess
root=Path(__file__).resolve().parents[2];out=root/'docs/ProductionReports/Part13G'
baseline=json.loads((out/'BASELINE.json').read_text(encoding='utf-8-sig'))
allowed={'Assets/AddressableAssetsData/AddressableAssetSettings.asset','Assets/Art/Environment/Map/Islands/starlight/ProductionManifest.json','Assets/Editor/Part12BEditor.cs','Assets/Resources/Map/World.json','Assets/Scripts/Map/MapWorld.cs','Assets/Scripts/Map/IslandArtSlot.cs','docs/PRODUCTION_PROGRESS.md'}
changed=[p for p,h in baseline.items() if not(root/p).is_file() or hashlib.sha256((root/p).read_bytes()).hexdigest()!=h]
unexpected=[p for p in changed if p not in allowed]
old=json.loads(subprocess.check_output(['git','show','06a64751a92d1788e70c0efac12d09db43493a5b:Assets/Resources/Map/World.json'],cwd=root).decode('utf-8-sig'))
new=json.loads((root/'Assets/Resources/Map/World.json').read_text(encoding='utf-8-sig'))
assert len(new['regions'])==12
for i,(a,b) in enumerate(zip(old['regions'],new['regions'])):
 if i==9:
  a['artReady']=True
 assert a==b, f'Unexpected catalog change at island {i+1}'
assert all(hashlib.sha256((root/'Assets/Art/Environment/Map/Islands'/r['artKey']/'ProductionManifest.json').read_bytes()).hexdigest()==baseline[f"Assets/Art/Environment/Map/Islands/{r['artKey']}/ProductionManifest.json"] for r in new['regions'][10:] if f"Assets/Art/Environment/Map/Islands/{r['artKey']}/ProductionManifest.json" in baseline)
result={'success':not unexpected,'baselineTrackedFiles':len(baseline),'unchangedFiles':len(baseline)-len(changed),'intentionalChanges':changed,'unexpectedChanges':unexpected,'catalogChange':'Only starlight artReady false -> true; all level/reward/tutorial data identical','otherTrackedIslandManifestsUnchanged':True}
(out/'PRESERVATION.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps(result,ensure_ascii=True));assert not unexpected
