param(
    [string]$ReleaseDirectory = "artifacts/release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$manifestPath = Join-Path $ReleaseDirectory "release-manifest.json"

if (-not (Test-Path $manifestPath)) {
    throw "Release-Manifest fehlt."
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

$zipPath = Join-Path $ReleaseDirectory $manifest.zipFile

if (-not (Test-Path $zipPath)) {
    throw "Release-ZIP fehlt: $zipPath"
}

$actual = (Get-FileHash $zipPath -Algorithm SHA256).Hash

if ($actual -ne $manifest.sha256) {
    throw "SHA-256-Prüfung fehlgeschlagen."
}

Write-Host "Release-ZIP ist unverändert und Prüfsumme stimmt." -ForegroundColor Green
Write-Host "Version: $($manifest.version)" -ForegroundColor Green
Write-Host "SHA256: $actual" -ForegroundColor Green
