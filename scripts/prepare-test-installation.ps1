param(
    [string]$ReleaseZip = "artifacts/release/WerkPilot-0.12.24-rc-win-x64.zip",
    [string]$TargetDirectory = "artifacts/test-installation/WerkPilot"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not (Test-Path $ReleaseZip)) {
    throw "Release-ZIP wurde nicht gefunden: $ReleaseZip"
}

if (Test-Path $TargetDirectory) {
    Remove-Item $TargetDirectory -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $TargetDirectory | Out-Null

Expand-Archive `
    -Path $ReleaseZip `
    -DestinationPath $TargetDirectory `
    -Force

$exe = Join-Path $TargetDirectory "WerkPilot.Desktop.exe"

if (-not (Test-Path $exe)) {
    throw "Testinstallation enthält keine WerkPilot.Desktop.exe."
}

Write-Host "Testinstallation vorbereitet: $TargetDirectory" -ForegroundColor Green
Write-Host "EXE: $exe" -ForegroundColor Green
