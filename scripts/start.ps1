param(
    [switch]$SkipDatabaseUpdate,
    [switch]$NoDemoData
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

& "$PSScriptRoot/check-prerequisites.ps1" -RequireDocker
& "$PSScriptRoot/verify-source.ps1"

docker compose -f deploy/docker-compose.yml up -d
& "$PSScriptRoot/wait-for-postgres.ps1"

$env:DOTNET_ENVIRONMENT = "Development"
$env:WERKPILOT_CONNECTION_STRING =
    "Host=localhost;Port=5432;Database=werkpilot;Username=werkpilot;Password=werkpilot_dev"

$env:WERKPILOT_SEED_DEMO_DATA =
    if ($NoDemoData) { "false" } else { "true" }

if (-not $env:WERKPILOT_ADMIN_INITIAL_PASSWORD) {
    Write-Warning "Erstinstallation: Setze WERKPILOT_ADMIN_INITIAL_PASSWORD vor dem ersten Start."
}

if (-not $SkipDatabaseUpdate) {
    & "$PSScriptRoot/update-database.ps1"
}

dotnet run `
  --project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
  --configuration Debug
