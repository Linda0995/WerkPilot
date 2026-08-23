$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$base = "artifacts/first-build"

if (-not (Test-Path $base)) {
    Write-Host "Noch kein First-Build-Lauf vorhanden." -ForegroundColor Yellow
    exit 0
}

$latest = Get-ChildItem $base -Directory |
    Sort-Object Name -Descending |
    Select-Object -First 1

if ($null -eq $latest) {
    Write-Host "Noch kein First-Build-Lauf vorhanden." -ForegroundColor Yellow
    exit 0
}

$summaryPath = Join-Path $latest.FullName "first-build-summary.json"

if (-not (Test-Path $summaryPath)) {
    Write-Host "Der letzte Lauf enthält keine Zusammenfassung." -ForegroundColor Yellow
    exit 1
}

$summary = Get-Content $summaryPath -Raw | ConvertFrom-Json

Write-Host "Letzter First-Build-Lauf" -ForegroundColor Cyan
Write-Host "Pfad: $($latest.FullName)"
Write-Host "Version: $($summary.version)"
Write-Host "Erfolg: $($summary.success)"
Write-Host ".NET: $($summary.dotnet)"
Write-Host "Docker: $($summary.docker)"

$diagZip = "$($latest.FullName)-diagnostic.zip"
if (Test-Path $diagZip) {
    Write-Host "Diagnose-ZIP: $diagZip"
}
