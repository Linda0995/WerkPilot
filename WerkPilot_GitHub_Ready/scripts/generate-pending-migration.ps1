param(
    [string]$MigrationName = "RCModelSync_01218"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$env:WERKPILOT_CONNECTION_STRING = $env:WERKPILOT_CONNECTION_STRING

dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    throw "dotnet tool restore fehlgeschlagen."
}

dotnet ef migrations has-pending-model-changes `
    --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
    --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
    --context WerkPilotDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "Keine Pending Model Changes vorhanden." -ForegroundColor Green
    exit 0
}

dotnet ef migrations add $MigrationName `
    --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
    --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
    --context WerkPilotDbContext `
    --output-dir Persistence/Migrations

if ($LASTEXITCODE -ne 0) {
    throw "Migration konnte nicht erzeugt werden."
}

Write-Host "Migration $MigrationName wurde erzeugt." -ForegroundColor Green
