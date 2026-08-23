param(
    [string]$DatabaseName = "werkpilot_smoketest"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

& "$PSScriptRoot/check-prerequisites.ps1" -RequireDocker
& "$PSScriptRoot/verify-source.ps1"

if ($DatabaseName -notmatch '^[a-zA-Z0-9_]+$') {
    throw "Ungültiger Testdatenbankname."
}

docker compose -f deploy/docker-compose.yml up -d
& "$PSScriptRoot/wait-for-postgres.ps1"

Write-Host "Isolierte Smoke-Test-Datenbank neu erstellen: $DatabaseName" -ForegroundColor Cyan

docker compose -f deploy/docker-compose.yml exec -T postgres `
    psql -U werkpilot -d postgres `
    -c "DROP DATABASE IF EXISTS $DatabaseName WITH (FORCE);"

if ($LASTEXITCODE -ne 0) {
    throw "Smoke-Test-Datenbank konnte nicht entfernt werden."
}

docker compose -f deploy/docker-compose.yml exec -T postgres `
    psql -U werkpilot -d postgres `
    -c "CREATE DATABASE $DatabaseName OWNER werkpilot;"

if ($LASTEXITCODE -ne 0) {
    throw "Smoke-Test-Datenbank konnte nicht erstellt werden."
}

$env:WERKPILOT_CONNECTION_STRING =
    "Host=localhost;Port=5432;Database=$DatabaseName;Username=werkpilot;Password=werkpilot_dev"

$desktopProject = "src/WerkPilot.Desktop/WerkPilot.Desktop.csproj"
$desktopProjectSource = Get-Content $desktopProject -Raw
if ($desktopProjectSource -notmatch 'PackageReference Include="Microsoft\.EntityFrameworkCore\.Design"') {
    throw "EF Core Design fehlt im Startup-Projekt WerkPilot.Desktop."
}

dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    throw "dotnet tool restore fehlgeschlagen."
}

Write-Host "EF Startup-Projekt für Design-Time vorbereiten..." -ForegroundColor Cyan
dotnet build src/WerkPilot.Desktop/WerkPilot.Desktop.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    throw "EF Startup-Projekt konnte nicht gebaut werden."
}

$efProject = "src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj"
$efStartupProject = "src/WerkPilot.Desktop/WerkPilot.Desktop.csproj"
$efContext = "WerkPilotDbContext"
$generatedMigrationName = "RCModelSync_01218"
$generatedMigrationArtifactDir = Join-Path $root "artifacts\generated-migration"

dotnet ef migrations has-pending-model-changes `
    --project $efProject `
    --startup-project $efStartupProject `
    --context $efContext

if ($LASTEXITCODE -ne 0) {
    Write-Host "EF hat echte Pending Model Changes erkannt." -ForegroundColor Yellow
    Write-Host "Erzeuge reale EF-Migration: $generatedMigrationName" -ForegroundColor Cyan

    $existingGeneratedMigration = Get-ChildItem `
        "src/WerkPilot.Infrastructure/Persistence/Migrations" `
        -File `
        -Filter "*_$generatedMigrationName.cs" `
        -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notlike "*.Designer.cs" } |
        Select-Object -First 1

    if ($null -eq $existingGeneratedMigration) {
        dotnet ef migrations add $generatedMigrationName `
            --project $efProject `
            --startup-project $efStartupProject `
            --context $efContext `
            --output-dir Persistence/Migrations

        if ($LASTEXITCODE -ne 0) {
            throw "Die reale EF-Migration konnte nicht erzeugt werden."
        }
    }
    else {
        Write-Host "Migration $generatedMigrationName ist bereits vorhanden." -ForegroundColor DarkYellow
    }

    New-Item -ItemType Directory -Force -Path $generatedMigrationArtifactDir | Out-Null

    Get-ChildItem `
        "src/WerkPilot.Infrastructure/Persistence/Migrations" `
        -File |
        Where-Object {
            $_.Name -like "*_$generatedMigrationName.cs" -or
            $_.Name -eq "WerkPilotDbContextModelSnapshot.cs"
        } |
        Copy-Item -Destination $generatedMigrationArtifactDir -Force

    $generatedMigration = Get-ChildItem `
        "src/WerkPilot.Infrastructure/Persistence/Migrations" `
        -File `
        -Filter "*_$generatedMigrationName.cs" |
        Where-Object { $_.Name -notlike "*.Designer.cs" } |
        Select-Object -First 1

    if ($null -eq $generatedMigration) {
        throw "EF meldete Pending Changes, aber es wurde keine Migration erzeugt."
    }

    $generatedMigrationSource = Get-Content $generatedMigration.FullName -Raw
    if ($generatedMigrationSource -notmatch 'partial class RCModelSync_01218') {
        throw "Die erzeugte EF-Migration ist nicht plausibel."
    }

    Write-Host "Erzeugte Migration: $($generatedMigration.Name)" -ForegroundColor Green

    dotnet ef migrations has-pending-model-changes `
        --project $efProject `
        --startup-project $efStartupProject `
        --context $efContext

    if ($LASTEXITCODE -ne 0) {
        throw "Auch nach der erzeugten Migration bestehen Pending Model Changes."
    }

    Write-Host "EF-Modell und ModelSnapshot sind nach der Migration synchron." -ForegroundColor Green
}
else {
    Write-Host "Keine Pending Model Changes vorhanden." -ForegroundColor Green
}

dotnet ef database update `
    --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
    --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
    --context WerkPilotDbContext

if ($LASTEXITCODE -ne 0) {
    Write-Host "Historische Migrationskette konnte eine leere Datenbank nicht vollständig aufbauen." -ForegroundColor Yellow
    Write-Host "WerkPilot ist noch vor dem ersten Produktiv-Release: Migrationen werden sicher auf einen aktuellen Baseline-Stand konsolidiert." -ForegroundColor Yellow

    $migrationDirectory = Join-Path $root "src\WerkPilot.Infrastructure\Persistence\Migrations"
    $migrationBackupDirectory = Join-Path $root "artifacts\migration-history-backup"
    $baselineArtifactDirectory = Join-Path $root "artifacts\generated-baseline"
    $baselineMigrationName = "InitialBaseline_01220"

    New-Item -ItemType Directory -Force -Path $migrationBackupDirectory | Out-Null
    New-Item -ItemType Directory -Force -Path $baselineArtifactDirectory | Out-Null

    Get-ChildItem $migrationDirectory -File -Filter "*.cs" |
        Copy-Item -Destination $migrationBackupDirectory -Force

    Write-Host "Alte RC-Migrationshistorie wurde unter artifacts\migration-history-backup gesichert." -ForegroundColor DarkYellow

    Get-ChildItem $migrationDirectory -File -Filter "*.cs" |
        Remove-Item -Force

    Write-Host "Erzeuge vollständige EF-Baseline aus dem aktuellen WerkPilot-Modell..." -ForegroundColor Cyan

    dotnet ef migrations add $baselineMigrationName `
        --project $efProject `
        --startup-project $efStartupProject `
        --context $efContext `
        --output-dir Persistence/Migrations

    if ($LASTEXITCODE -ne 0) {
        throw "Die konsolidierte EF-Baseline konnte nicht erzeugt werden."
    }

    Get-ChildItem $migrationDirectory -File -Filter "*.cs" |
        Copy-Item -Destination $baselineArtifactDirectory -Force

    $baselineMigration = Get-ChildItem $migrationDirectory `
        -File `
        -Filter "*_$baselineMigrationName.cs" |
        Where-Object { $_.Name -notlike "*.Designer.cs" } |
        Select-Object -First 1

    if ($null -eq $baselineMigration) {
        throw "Die erzeugte Baseline-Migration wurde nicht gefunden."
    }

    Write-Host "Baseline erzeugt: $($baselineMigration.Name)" -ForegroundColor Green

    dotnet ef migrations has-pending-model-changes `
        --project $efProject `
        --startup-project $efStartupProject `
        --context $efContext

    if ($LASTEXITCODE -ne 0) {
        throw "Die neue EF-Baseline stimmt nicht mit dem aktuellen Modell überein."
    }

    Write-Host "Smoke-Test-Datenbank für Baseline-Test erneut erstellen..." -ForegroundColor Cyan

    docker compose -f deploy/docker-compose.yml exec -T postgres `
        psql -U werkpilot -d postgres `
        -c "DROP DATABASE IF EXISTS $DatabaseName WITH (FORCE);"

    if ($LASTEXITCODE -ne 0) {
        throw "Smoke-Test-Datenbank konnte vor dem Baseline-Test nicht entfernt werden."
    }

    docker compose -f deploy/docker-compose.yml exec -T postgres `
        psql -U werkpilot -d postgres `
        -c "CREATE DATABASE $DatabaseName OWNER werkpilot;"

    if ($LASTEXITCODE -ne 0) {
        throw "Smoke-Test-Datenbank konnte vor dem Baseline-Test nicht neu erstellt werden."
    }

    dotnet ef database update `
        --project $efProject `
        --startup-project $efStartupProject `
        --context $efContext

    if ($LASTEXITCODE -ne 0) {
        throw "Auch die konsolidierte EF-Baseline konnte die leere Smoke-Test-Datenbank nicht aufbauen."
    }

    Write-Host "Leere PostgreSQL-Datenbank wurde erfolgreich aus der neuen EF-Baseline aufgebaut." -ForegroundColor Green

    $promotionManifest = [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
        baselineMigration = $baselineMigration.Name
        sourceDirectory = "src/WerkPilot.Infrastructure/Persistence/Migrations"
        artifactDirectory = "artifacts/generated-baseline"
        status = "validated-on-empty-postgresql"
        instruction = "Diese Baseline-Dateien in den nächsten festen Release-Quellstand übernehmen."
    }

    $promotionManifest |
        ConvertTo-Json -Depth 4 |
        Set-Content `
            (Join-Path $baselineArtifactDirectory "baseline-promotion.json") `
            -Encoding utf8

    Write-Host "Validierte Baseline ist unter artifacts\generated-baseline für die dauerhafte Übernahme markiert." -ForegroundColor Green
}

$tableCount = docker compose -f deploy/docker-compose.yml exec -T postgres `
    psql -U werkpilot -d $DatabaseName -t -A `
    -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public';"

if ($LASTEXITCODE -ne 0) {
    throw "Tabellenprüfung der Smoke-Test-Datenbank ist fehlgeschlagen."
}

$historyTableExists = docker compose -f deploy/docker-compose.yml exec -T postgres `
    psql -U werkpilot -d $DatabaseName -t -A `
    -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='__EFMigrationsHistory';"

if ($LASTEXITCODE -ne 0) {
    throw "EF-Migrationshistorie konnte nicht geprüft werden."
}

if ($historyTableExists.Trim() -ne "1") {
    $publicTables = docker compose -f deploy/docker-compose.yml exec -T postgres `
        psql -U werkpilot -d $DatabaseName -t -A `
        -c "SELECT table_name FROM information_schema.tables WHERE table_schema='public' ORDER BY table_name;"

    throw "EF-Migrationshistorientabelle wurde nicht gefunden. Öffentliche Tabellen: $($publicTables -join ', ')"
}

$migrationCount = docker compose -f deploy/docker-compose.yml exec -T postgres `
    psql -U werkpilot -d $DatabaseName -t -A `
    -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public' AND table_name='__EFMigrationsHistory';"

if ($LASTEXITCODE -ne 0) {
    throw "EF-Migrationshistorie konnte nicht gezählt werden."
}

if ([int]$migrationCount.Trim() -lt 1) {
    throw "EF-Migrationshistorie wurde nicht gefunden."
}

$appliedMigrationCount = docker compose -f deploy/docker-compose.yml exec -T postgres `
    psql -U werkpilot -d $DatabaseName -t -A `
    -c "SELECT COUNT(*) FROM pg_catalog.pg_attribute a JOIN pg_catalog.pg_class c ON a.attrelid = c.oid JOIN pg_catalog.pg_namespace n ON c.relnamespace = n.oid WHERE n.nspname='public' AND c.relname='__EFMigrationsHistory' AND a.attname='MigrationId' AND a.attnum > 0 AND NOT a.attisdropped;"

if ($LASTEXITCODE -ne 0) {
    throw "EF-Migrationshistorie konnte nicht strukturell validiert werden."
}

if ([int]$appliedMigrationCount.Trim() -lt 1) {
    throw "EF-Migrationshistorie besitzt keine MigrationId-Spalte."
}

Write-Host "Smoke-Test erfolgreich." -ForegroundColor Green
Write-Host "Tabellen: $($tableCount.Trim())" -ForegroundColor Green
Write-Host "EF-Migrationshistorie: vorhanden und strukturell gültig." -ForegroundColor Green
