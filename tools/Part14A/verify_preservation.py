from pathlib import Path
import json,hashlib
root=Path(__file__).resolve().parents[2];out=root/'docs/ProductionReports/Part14A'
b=json.loads((out/'BASELINE.json').read_text(encoding='utf-8-sig'))
allowed=set(['Assets/Localization/Source/Mewgic.json', 'Assets/Localization/Tables/Mewgic Shared Data.asset', 'Assets/Localization/Tables/Mewgic_tr.asset', 'Assets/Localization/Tables/Mewgic_en.asset', 'Assets/Localization/Tables/Mewgic_de.asset', 'Assets/Localization/Tables/Mewgic_fr.asset', 'Assets/Localization/Tables/Mewgic_es.asset', 'Assets/Localization/Tables/Mewgic_pt-BR.asset', 'Assets/Localization/Tables/Mewgic_it.asset', 'Assets/Localization/Tables/Mewgic_pl.asset', 'Assets/Scripts/Core/BoosterActions.cs', 'Assets/Scripts/Core/DailyActivities.cs', 'Assets/Scripts/Daily/DailyActivitiesPanel.cs', 'Assets/Scripts/Map/MapWorld.cs', 'Assets/Scripts/Map/MapWorldProgression.cs', 'Assets/Scripts/Presentation/GameplayEndScreen.cs', 'Assets/Scripts/Presentation/HomeResourceModel.cs', 'Assets/Scripts/Presentation/HomeTopBar.cs', 'Assets/Scripts/Presentation/PrototypeApp.cs', 'Assets/Scripts/Rewards/RewardHubPanel.cs', 'docs/PRODUCTION_PROGRESS.md'])
changed=[p for p,h in b.items() if not(root/p).is_file() or hashlib.sha256((root/p).read_bytes()).hexdigest()!=h]
unexpected=[p for p in changed if p not in allowed]
r={'success':not unexpected,'baselineTrackedFiles':len(b),'unchanged':len(b)-len(changed),'intentionalChanges':changed,'unexpected':unexpected,'note':'Ada sanati, sahneler, prefablar, puzzle/uretici/Pati ve onceki raporlar korundu; baslangicta degismis font byte olarak ayni.'}
(out/'PRESERVATION.json').write_text(json.dumps(r,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');print(json.dumps(r));assert not unexpected
