param(
    [string]$MigrationName = "InitialBaseline_01220"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$migrationDirectory = Join-Path $root "src\WerkPilot.Infrastructure\Persistence\Migrations"
$backupDirectory = Join-Path $root "artifacts\migration-history-backup"

New-Item -ItemType Directory -Force -Path $backupDirectory | Out-Null

Get-ChildItem $migrationDirectory -File -Filter "*.cs" |
    Copy-Item -Destination $backupDirectory -Force

Get-ChildItem $migrationDirectory -File -Filter "*.cs" |
    Remove-Item -Force

dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    throw "dotnet tool restore fehlgeschlagen."
}

dotnet ef migrations add $MigrationName `
    --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
    --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
    --context WerkPilotDbContext `
    --output-dir Persistence/Migrations

if ($LASTEXITCODE -ne 0) {
    throw "Baseline-Migration konnte nicht erzeugt werden."
}

dotnet ef migrations has-pending-model-changes `
    --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
    --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
    --context WerkPilotDbContext

if ($LASTEXITCODE -ne 0) {
    throw "Baseline und aktuelles EF-Modell stimmen nicht überein."
}

Write-Host "EF-Migrationshistorie wurde erfolgreich auf $MigrationName konsolidiert." -ForegroundColor Green
