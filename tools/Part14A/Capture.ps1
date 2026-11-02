param([string]$Name,[int]$Height=1600)
$shot=unity command capture game_view --width 900 --height $Height --source screen --save_path Part14A_Capture.png --project-path C:/Dev/Mewgic --json | ConvertFrom-Json
if(-not $shot.success){throw 'Unity yakalama hatasi'}
$code='System.IO.File.Copy("Assets/Part14A_Capture.png","docs/ProductionReports/Part14A/'+$Name+'.png",true);UnityEditor.AssetDatabase.DeleteAsset("Assets/Part14A_Capture.png");'
unity command script eval $code --project-path C:/Dev/Mewgic --json
