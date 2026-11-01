from pathlib import Path
import hashlib, json
root=Path(__file__).resolve().parents[2]
out=root/'docs/ProductionReports/Part13'
world=json.loads((root/'Assets/Resources/Map/World.json').read_text(encoding='utf-8-sig'))
layers=['Background','IslandBase','Architecture','Vegetation','WaterOrSpecialEffects','Decorations','LevelNodes','Rewards','ForegroundEffects']
assert len(world['regions'])==12
for i,r in enumerate(world['regions']):
 assert r['firstLevel']==i*50+1 and r['levelCount']==50
 assert r['artReady']==(i<4)
 if i>=4:
  d=root/'Assets/Art/Environment/Map/Islands'/r['artKey']
  m=json.loads((d/'ProductionManifest.json').read_text(encoding='utf-8'))
  assert m['islandKey']==r['key'] and m['layers']==layers and not m['artReady']
  assert all((d/l).is_dir() for l in layers)
  assert all((root/'SourceArt/Map/Islands'/r['artKey']/l).is_dir() for l in ['Blender','Renders','Paint','Masks'])
baseline=json.loads((out/'PRESERVATION_BASELINE.json').read_text())
changed=[p for p,h in baseline.items() if not (root/p).is_file() or hashlib.sha256((root/p).read_bytes()).hexdigest()!=h]
result={'success':not changed,'protectedTrackedFiles':len(baseline),'changed':changed,'catalogLevels':600,'preparedIslands':8,'layersPerIsland':9}
(out/'PRESERVATION.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(result,ensure_ascii=True))
assert not changed
