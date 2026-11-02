$ErrorActionPreference='Stop'
for($n=0;$n -lt 12;$n++){
 $code='UnityEngine.Object.FindFirstObjectByType<Mewgic.Map.MapWorld>().FocusRegion('+$n+',false);'
 $focus=unity command script eval $code --project-path C:/Dev/Mewgic --json | ConvertFrom-Json
 if(-not $focus.success -or -not $focus.data.result.success){throw 'Focus failed'}
 Start-Sleep -Milliseconds 1250
 $shot=unity command capture game_view --width 900 --height 1600 --source screen --save_path Part13J_Native.png --project-path C:/Dev/Mewgic --json | ConvertFrom-Json
 if(-not $shot.success){throw 'Capture failed'}
 $target='docs/ProductionReports/Part13J/Native_'+($n+1).ToString('00')+'.png'
 $code='System.IO.File.Copy("Assets/Part13J_Native.png","'+$target+'",true);UnityEditor.AssetDatabase.DeleteAsset("Assets/Part13J_Native.png");'
 $copy=unity command script eval $code --project-path C:/Dev/Mewgic --json | ConvertFrom-Json
 if(-not $copy.success -or -not $copy.data.result.success){throw 'Copy failed'}
 Write-Output ('Native Game View '+($n+1)+'/12')
}
unity command script eval 'UnityEngine.Object.FindFirstObjectByType<Mewgic.Map.MapWorld>().FocusRegion(0,false);' --project-path C:/Dev/Mewgic --json
