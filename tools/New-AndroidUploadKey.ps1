param(
    [Parameter(Mandatory = $true)][string]$Keytool,
    [Parameter(Mandatory = $true)][string]$Destination
)

$ErrorActionPreference = 'Stop'
if (-not [IO.Path]::IsPathRooted($Destination)) {
    throw 'Anahtar için depo dışında mutlak bir yol belirtin.'
}
$repository = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..')).TrimEnd('\')
$keyPath = [IO.Path]::GetFullPath($Destination)
if ($keyPath.StartsWith($repository + '\', [StringComparison]::OrdinalIgnoreCase) -or
    $keyPath.Equals($repository, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'İmzalama anahtarı Git deposunun dışında tutulmalı.'
}
if (Test-Path -LiteralPath $keyPath) {
    throw 'Hedef dosya zaten var. Mevcut imzalama anahtarı korunmalı.'
}
if (-not (Test-Path -LiteralPath $Keytool -PathType Leaf) -or
    -not (Test-Path -LiteralPath (Split-Path -Parent $keyPath) -PathType Container)) {
    throw 'Geçerli keytool ve var olan güvenli hedef klasörü gerekli.'
}

Write-Host 'Parola ve sertifika bilgilerini keytool istemine kendiniz girin. Bu oturumu kaydetmeyin.'
& $Keytool -genkeypair -keystore $keyPath -storetype JKS -alias upload -keyalg RSA -keysize 2048 -validity 10000
if ($LASTEXITCODE -ne 0) {
    throw 'Anahtar oluşturulamadı. Mevcut dosyayı inceleyin ve üzerine yazmayın.'
}
Write-Host 'Anahtarı ve parolasını güvenli, ayrı yedeklerde saklayın. Git veya sohbet üzerinden paylaşmayın.'
