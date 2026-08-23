param(
    [string]$ArtifactDirectory = "artifacts\generated-baseline"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$artifactPath = Join-Path $root $ArtifactDirectory
$migrationPath = Join-Path $root "src\WerkPilot.Infrastructure\Persistence\Migrations"

if (-not (Test-Path $artifactPath)) {
    throw "Baseline-Artefaktordner wurde nicht gefunden: $artifactPath"
}

$baselineFiles = Get-ChildItem $artifactPath -File -Filter "*.cs"
if ($baselineFiles.Count -lt 2) {
    throw "Im Baseline-Artefaktordner fehlen Migration/Designer/Snapshot-Dateien."
}

New-Item -ItemType Directory -Force -Path $migrationPath | Out-Null
Get-ChildItem $migrationPath -File -Filter "*.cs" | Remove-Item -Force
$baselineFiles | Copy-Item -Destination $migrationPath -Force

Write-Host "Validierte EF-Baseline wurde in den Source-Tree übernommen." -ForegroundColor Green
