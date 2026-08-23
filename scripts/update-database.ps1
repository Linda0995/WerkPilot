$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

& "$PSScriptRoot/check-prerequisites.ps1" -RequireDocker
& "$PSScriptRoot/verify-source.ps1"

docker compose -f deploy/docker-compose.yml up -d
& "$PSScriptRoot/wait-for-postgres.ps1"

if (-not $env:WERKPILOT_CONNECTION_STRING) {
    $env:WERKPILOT_CONNECTION_STRING =
        "Host=localhost;Port=5432;Database=werkpilot;Username=werkpilot;Password=werkpilot_dev"
}

dotnet tool restore

Write-Host "Verfügbare EF-Core-Migrationen prüfen..." -ForegroundColor Cyan
dotnet ef migrations list `
  --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
  --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
  --context WerkPilotDbContext

if ($LASTEXITCODE -ne 0) {
    throw "EF-Core-Migrationen konnten nicht geladen werden."
}

Write-Host "Datenbank aktualisieren..." -ForegroundColor Cyan
dotnet ef database update `
  --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
  --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
  --context WerkPilotDbContext

if ($LASTEXITCODE -ne 0) {
    throw "EF-Core-Datenbankmigration ist fehlgeschlagen."
}

Write-Host "WerkPilot-Datenbank ist auf dem aktuellen Migrationsstand." -ForegroundColor Green
