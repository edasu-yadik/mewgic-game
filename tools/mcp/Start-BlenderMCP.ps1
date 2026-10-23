$ErrorActionPreference = 'Stop'
$blender = 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
if (!(Test-Path -LiteralPath $blender)) { throw 'Blender 5.2 executable not found.' }
$listener = Get-NetTCPConnection -LocalPort 9876 -State Listen -ErrorAction SilentlyContinue
if ($listener) {
    Write-Host 'Port 9876 is already in use. Reuse the existing test session or close its Blender window before starting another.'
    exit 1
}
$bootstrap = Join-Path $PSScriptRoot 'BootstrapBlender.py'
Start-Process -FilePath $blender -ArgumentList @('--factory-startup', '--online-mode', '--python', ('"' + $bootstrap + '"')) -WindowStyle Normal
Write-Host 'A separate temporary Blender scene is starting. The MCP bridge listens only on 127.0.0.1:9876.'
