from pathlib import Path
import json,hashlib
root=Path(__file__).resolve().parents[2];o=root/'docs/ProductionReports/Part13J';base=json.loads((o/'BASELINE.json').read_text(encoding='utf-8-sig'))
allowed={'docs/PRODUCTION_PROGRESS.md'}
changed=[p for p,h in base.items() if not(root/p).is_file() or hashlib.sha256((root/p).read_bytes()).hexdigest()!=h]
unexpected=[p for p in changed if p not in allowed]
r={'success':not unexpected,'baselineTrackedFiles':len(base),'unchangedFiles':len(base)-len(changed),'intentionalChanges':changed,'unexpectedChanges':unexpected,'scope':'Existing production art, scenes, generator, localization, save, gameplay and Pati files unchanged. Preexisting TMP fallback bytes included.'}
(o/'PRESERVATION.json').write_text(json.dumps(r,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps(r));assert not unexpected
