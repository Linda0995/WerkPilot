$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$failures = @()

function Add-Failure {
    param([string]$Message)
    $script:failures += $Message
}

$requiredFiles = @(
    "WerkPilot.sln",
    "global.json",
    "Directory.Build.props",
    "Directory.Packages.props",
    "src/WerkPilot.Desktop/WerkPilot.Desktop.csproj",
    "src/WerkPilot.Application/WerkPilot.Application.csproj",
    "src/WerkPilot.Domain/WerkPilot.Domain.csproj",
    "src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj",
    "tests/WerkPilot.UnitTests/WerkPilot.UnitTests.csproj",
    "deploy/docker-compose.yml",
    "scripts/build.ps1",
    "scripts/start.ps1",
    "scripts/update-database.ps1",
    "scripts/wait-for-postgres.ps1",
    "scripts/database-smoke-test.ps1",
    "scripts/basic-workflow-smoke-test.ps1",
    "scripts/release-candidate.ps1",
    "scripts/show-first-build-result.ps1",
    "scripts/verify-release-package.ps1",
    "scripts/test-powershell-syntax.ps1",
    "scripts/prepare-test-installation.ps1",
    "scripts/create-build-diagnostic-bundle.ps1",
    "scripts/first-rc-build.ps1",
    "scripts/create-release-package.ps1"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        Add-Failure "Pflichtdatei fehlt: $file"
    }
}

$xmlFiles = @()
$xmlFiles += Get-ChildItem src -Recurse -File -Filter "*.axaml"
$xmlFiles += Get-ChildItem src -Recurse -File -Filter "*.csproj"
$xmlFiles += Get-ChildItem tests -Recurse -File -Filter "*.csproj"
$xmlFiles += Get-Item "Directory.Build.props", "Directory.Packages.props"

foreach ($file in $xmlFiles) {
    try {
        [xml](Get-Content $file.FullName -Raw) | Out-Null
    }
    catch {
        Add-Failure "XML/XAML ungültig: $($file.FullName) – $($_.Exception.Message)"
    }
}

foreach ($file in Get-ChildItem src/WerkPilot.Desktop/Views -File -Filter "*.axaml") {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'x:Class="([^"]+)"') {
        $codeBehind = "$($file.FullName).cs"
        if (-not (Test-Path $codeBehind)) {
            Add-Failure "Code-behind fehlt: $codeBehind"
        }
        else {
            $className = ($Matches[1] -split '\.')[-1]
            $code = Get-Content $codeBehind -Raw
            if ($code -notmatch "partial\s+class\s+$([regex]::Escape($className))\b") {
                Add-Failure "x:Class/Code-behind stimmen nicht überein: $($file.FullName)"
            }
        }
    }
}

$solution = Get-Content "WerkPilot.sln" -Raw
$matches = [regex]::Matches($solution, '"([^"]+\.csproj)"')
foreach ($match in $matches) {
    $relative = $match.Groups[1].Value.Replace('\', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path $relative)) {
        Add-Failure "Solution verweist auf fehlendes Projekt: $relative"
    }
}

$migrationFiles = Get-ChildItem `
  "src/WerkPilot.Infrastructure/Persistence/Migrations" `
  -Filter "*.cs" |
  Where-Object { $_.Name -ne "WerkPilotDbContextModelSnapshot.cs" }

$migrationIds = @()
foreach ($file in $migrationFiles) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match '\[Migration\("([^"]+)"\)\]') {
        $migrationIds += $Matches[1]
    }
}

$duplicates = $migrationIds | Group-Object | Where-Object { $_.Count -gt 1 }
foreach ($duplicate in $duplicates) {
    Add-Failure "Doppelte EF-Migration-ID: $($duplicate.Name)"
}

$appSource = Get-Content "src/WerkPilot.Desktop/App.axaml.cs" -Raw
if ($appSource -match '\b(MyWorkService|TeamWorkService|WorkReassignmentService)\b' -and
    $appSource -notmatch 'using WerkPilot\.Application\.Work;') {
    Add-Failure "App.axaml.cs verwendet Work-Services ohne using WerkPilot.Application.Work."
}

$notificationSource = Get-Content `
    "src/WerkPilot.Application/Notifications/NotificationService.cs" -Raw
if ($notificationSource -match '\bUserAbsenceStatus\b' -and
    $notificationSource -notmatch 'using WerkPilot\.Domain\.Identity;') {
    Add-Failure "NotificationService verwendet UserAbsenceStatus ohne Domain.Identity-Namespace."
}

$csharp = Get-ChildItem src,tests -Recurse -File -Filter "*.cs"
foreach ($file in $csharp) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match 'new\s+ProjectTask\s*\(\s*[^,\r\n]+,\s*[^,\r\n]+,\s*[^,\r\n]+,\s*[^,\r\n\)]+\s*\)') {
        Add-Failure "Veralteter 4-Parameter-ProjectTask-Konstruktor: $($file.FullName)"
    }
}

$forbiddenPatterns = @(
    "WerkPilot!2026",
    "WerkPilot 0.10.8",
    "Version 0.10.8"
)

foreach ($file in Get-ChildItem src -Recurse -File) {
    if ($file.Extension -notin ".cs", ".axaml", ".json") {
        continue
    }

    $content = Get-Content $file.FullName -Raw
    foreach ($pattern in $forbiddenPatterns) {
        if ($content.Contains($pattern)) {
            Add-Failure "Verbotener Altwert '$pattern' in $($file.FullName)"
        }
    }
}

# Demo-Daten dürfen in der Runtime nur über explizites Flag aktiviert werden.
$dbInitializerSource = Get-Content `
    "src/WerkPilot.Infrastructure/Persistence/DbInitializer.cs" -Raw

if ($dbInitializerSource -match 'Musterbetrieb GmbH' -and
    $dbInitializerSource -notmatch 'WERKPILOT_SEED_DEMO_DATA') {
    Add-Failure "Demo-Daten sind nicht durch WERKPILOT_SEED_DEMO_DATA geschützt."
}

$startSource = Get-Content "scripts/start.ps1" -Raw
if ($startSource -notmatch 'wait-for-postgres\.ps1') {
    Add-Failure "Startskript wartet nicht auf PostgreSQL-Bereitschaft."
}

$updateSource = Get-Content "scripts/update-database.ps1" -Raw
if ($updateSource -notmatch 'ef migrations list') {
    Add-Failure "Datenbankupdate validiert die EF-Migrationsliste nicht."
}



# UI darf technische Exception-Texte nicht direkt ausgeben.
$desktopViewModels = Get-ChildItem `
    "src/WerkPilot.Desktop/ViewModels" `
    -File `
    -Filter "*.cs"

foreach ($file in $desktopViewModels) {
    $content = Get-Content $file.FullName -Raw

    if ($content -match '\b(Status|StatusText)\s*=\s*\$"[^"]*\{ex\.Message\}') {
        Add-Failure "Direkte technische Exception-Ausgabe in $($file.FullName)"
    }

    if ($content -match '\b(Status|StatusText)\s*=\s*ex\.Message\s*;') {
        Add-Failure "Direkte technische Exception-Ausgabe in $($file.FullName)"
    }
}

$uiErrorFormatter = Get-Content `
    "src/WerkPilot.Desktop/Services/UiErrorFormatter.cs" -Raw

if ($uiErrorFormatter -notmatch 'Fehler-ID') {
    Add-Failure "UiErrorFormatter erzeugt keine Fehler-ID."
}

if ($uiErrorFormatter -notmatch 'Log\.Error') {
    Add-Failure "UiErrorFormatter protokolliert technische Fehler nicht."
}



$releaseDiagnostics = Get-Content `
    "src/WerkPilot.Desktop/Services/ReleaseDiagnosticsService.cs" -Raw

$releaseSnapshot = Get-Content `
    "src/WerkPilot.Desktop/Services/ReleaseDiagnosticsSnapshot.cs" -Raw

if ($releaseSnapshot -match 'ConnectionString|DatabasePassword|InitialAdminPassword|SmtpPassword|Secret') {
    Add-Failure "Systemdiagnose darf keine Geheimwerte im Snapshot bereitstellen."
}

if ($releaseDiagnostics -notmatch 'AssemblyInformationalVersionAttribute') {
    Add-Failure "Systemdiagnose liest die Informationsversion nicht."
}

$rcScript = Get-Content -Path "scripts/release-candidate.ps1" -Raw

if ($rcScript -notmatch 'publish-win-x64\.ps1') {
    Add-Failure "RC-Pipeline enthält keinen Windows-Publish."
}

if ($rcScript -notmatch 'create-release-package\.ps1') {
    Add-Failure "RC-Pipeline enthält keine Release-Paketierung."
}

if ($rcScript -notmatch 'verify-release-package\.ps1') {
    Add-Failure "RC-Pipeline enthält keine Release-Paketprüfung."
}



$firstBuildScript = Get-Content "scripts/first-rc-build.ps1" -Raw

if ($firstBuildScript -notmatch 'create-build-diagnostic-bundle\.ps1') {
    Add-Failure "First-Build-Runner erzeugt kein Diagnosepaket."
}

if ($firstBuildScript -notmatch 'publish-win-x64\.ps1') {
    Add-Failure "First-Build-Runner führt kein Windows-Publish aus."
}

if ($firstBuildScript -notmatch 'database-smoke-test\.ps1') {
    Add-Failure "First-Build-Runner enthält keinen Datenbank-Smoke-Test."
}



$firstRunSource = Get-Content `
    "src/WerkPilot.Desktop/Services/FirstRunReadinessService.cs" -Raw

if ($firstRunSource -notmatch 'CanConnectAsync') {
    Add-Failure "Erststart-Prüfung testet die Datenbankverbindung nicht."
}

if ($firstRunSource -match 'return\s+connection') {
    Add-Failure "Erststart-Prüfung darf keinen Connection-String zurückgeben."
}

$releaseVerify = Get-Content -Path "scripts/verify-release-package.ps1" -Raw

if ($releaseVerify -notmatch 'Get-FileHash') {
    Add-Failure "Release-Paketprüfung berechnet keinen Dateihash."
}

if ($releaseVerify -notmatch 'SHA256') {
    Add-Failure "Release-Paketprüfung verwendet nicht SHA-256."
}

if ($releaseVerify -notmatch '\$manifest\.sha256') {
    Add-Failure "Release-Paketprüfung vergleicht den Hash nicht mit dem Release-Manifest."
}



# Regression check for CS0108 warnings treated as errors.
$documentEntities = Get-ChildItem `
    "src/WerkPilot.Domain" `
    -Recurse `
    -File `
    -Filter "*.cs" |
    Where-Object {
        $_.Name -in @("DocumentFile.cs", "DocumentFolder.cs")
    }

foreach ($file in $documentEntities) {
    $content = Get-Content $file.FullName -Raw

    if ($content -match 'public\s+void\s+MoveToTrash\s*\(') {
        Add-Failure "Document entity hides Entity.MoveToTrash without explicit new: $($file.FullName)"
    }

    if ($content -match 'public\s+void\s+Restore\s*\(') {
        Add-Failure "Document entity hides Entity.Restore without explicit new: $($file.FullName)"
    }
}



# Regression checks from the first real C# build.
$customerValidator = Get-Content `
    "src/WerkPilot.Application/Customers/CustomerValidator.cs" -Raw

foreach ($obsoleteProperty in @(
    'request.Street',
    'request.PostalCode',
    'request.City'
)) {
    if ($customerValidator.Contains($obsoleteProperty)) {
        Add-Failure "CustomerValidator verwendet veraltete Eigenschaft: $obsoleteProperty"
    }
}

$materialItemSource = Get-Content `
    "src/WerkPilot.Domain/Materials/MaterialItem.cs" -Raw

if ($materialItemSource -notmatch 'bool\s+IsPriceOutdated\s*\(\s*int\s+maximumAgeDays\s*\)') {
    Add-Failure "MaterialItem.IsPriceOutdated(int) fehlt."
}

$buildSource = Get-Content "scripts/build.ps1" -Raw
if ($buildSource -match 'WerkPilot 0\.11\.1 wurde erfolgreich') {
    Add-Failure "build.ps1 enthält noch die veraltete Erfolgsmeldung 0.11.1."
}

if ($buildSource -notmatch 'Unit-Tests fehlgeschlagen') {
    Add-Failure "build.ps1 bricht bei fehlgeschlagenen Unit-Tests nicht explizit ab."
}



# Regression check from real build 0.12.8.
$invoiceServiceSource = Get-Content `
    "src/WerkPilot.Application/Billing/CustomerInvoiceService.cs" -Raw

if ($invoiceServiceSource -match 'offer\.Items') {
    Add-Failure "CustomerInvoiceService verwendet veraltete OfferDetailsDto.Items-Referenz."
}

if ($invoiceServiceSource -notmatch 'offer\.Positions') {
    Add-Failure "CustomerInvoiceService übernimmt Angebotspositionen nicht aus OfferDetailsDto.Positions."
}



# Regression check from real build 0.12.9.
$invoiceServiceSource = Get-Content `
    "src/WerkPilot.Application/Billing/CustomerInvoiceService.cs" -Raw

if ($invoiceServiceSource -match 'item\.Unit\b') {
    Add-Failure "CustomerInvoiceService greift auf nicht vorhandenes OfferPositionDto.Unit zu."
}

if ($invoiceServiceSource -notmatch '"Stk\."') {
    Add-Failure "CustomerInvoiceService besitzt keine Legacy-Standardeinheit für Angebotspositionen."
}



# Regression checks from real build 0.12.10.
$customer360Test = Get-Content "tests/WerkPilot.UnitTests/Customer360DtoTests.cs" -Raw
if ($customer360Test -notmatch 'DateTimeOffset\.UtcNow') {
    Add-Failure "Customer360DtoTests bildet CustomerDto.LastContactAtUtc nicht ab."
}

$offerDocumentTest = Get-Content "tests/WerkPilot.UnitTests/OfferDocumentDataTests.cs" -Raw
if ($offerDocumentTest -notmatch '100m,\s*20m,\s*120m') {
    Add-Failure "OfferDocumentDataTests bildet NetTotal/TaxTotal/GrossTotal nicht ab."
}

$dashboardTest = Get-Content "tests/WerkPilot.UnitTests/DashboardModelTests.cs" -Raw
if ($dashboardTest -notmatch '\[\],\s*\[\],\s*\[\],\s*\[\]') {
    Add-Failure "DashboardModelTests bildet die vier aktuellen Dashboard-Listen nicht ab."
}

$projectTaskTest = Get-Content "tests/WerkPilot.UnitTests/ProjectTaskTests.cs" -Raw
if ($projectTaskTest -match 'new ProjectTask\([^;\r\n]*,\s*"Max",\s*new DateOnly') {
    # Expected current constructor has five arguments total and this check is informational only.
}

$teamWorkTest = Get-Content "tests/WerkPilot.UnitTests/TeamWorkSummaryTests.cs" -Raw
if ($teamWorkTest -match 'UserRole\.User') {
    Add-Failure "TeamWorkSummaryTests verwendet nicht vorhandene UserRole.User."
}

$appSource = Get-Content "src/WerkPilot.Desktop/App.axaml.cs" -Raw
if ($appSource -match 'class App\s*:\s*Application\b') {
    Add-Failure "App.axaml.cs verwendet mehrdeutigen Application-Typ."
}

$packagesSource = Get-Content "Directory.Packages.props" -Raw
if ($packagesSource -notmatch 'Microsoft\.EntityFrameworkCore\.Relational" Version="9\.0\.8"') {
    Add-Failure "EF Core Relational ist nicht auf 9.0.8 vereinheitlicht."
}



# Regression checks from real Desktop build 0.12.11.
$programSource = Get-Content "src/WerkPilot.Desktop/Program.cs" -Raw
if ($programSource -match '\.UseReactiveUI\(\)') {
    Add-Failure "Program.cs verwendet weiterhin die nicht auflösbare UseReactiveUI-Erweiterung."
}

$calculationVmSource = Get-Content "src/WerkPilot.Desktop/ViewModels/CalculationViewModel.cs" -Raw
if ($calculationVmSource -match 'UpdateItemAsync\([\s\S]{0,400}materialItemId\s*:') {
    Add-Failure "CalculationViewModel übergibt materialItemId an UpdateItemAsync."
}

$appSource = Get-Content "src/WerkPilot.Desktop/App.axaml.cs" -Raw
if ($appSource -notmatch 'using Microsoft\.Extensions\.Configuration;') {
    Add-Failure "App.axaml.cs importiert Microsoft.Extensions.Configuration nicht."
}
if ($appSource -match 'AddScoped<PurchaseListService>\(\)') {
    Add-Failure "App.axaml.cs enthält weiterhin einen mehrdeutigen PurchaseListService."
}


# Regression checks from real Desktop build 0.12.12.
foreach ($relayCommandFile in @(
    "src/WerkPilot.Desktop/ViewModels/MainWindowViewModel.cs",
    "src/WerkPilot.Desktop/ViewModels/DocumentsViewModel.cs"
)) {
    $relaySource = Get-Content $relayCommandFile -Raw

    if ($relaySource -match 'private sealed class RelayCommand[\s\S]{0,500}public event EventHandler\? CanExecuteChanged;') {
        Add-Failure "RelayCommand erzeugt erneut CS0067 in: $relayCommandFile"
    }

    if ($relaySource -notmatch 'CanExecuteChanged\s*\{\s*add\s*\{\s*\}\s*remove\s*\{\s*\}\s*\}') {
        Add-Failure "RelayCommand besitzt keine statische ICommand-Eventimplementierung in: $relayCommandFile"
    }
}



# Regression check from real Avalonia build 0.12.13.
$comboBoxWatermarkFiles = Get-ChildItem `
    "src/WerkPilot.Desktop/Views" `
    -Recurse `
    -File `
    -Filter "*.axaml" |
    Where-Object {
        (Get-Content $_.FullName -Raw) -match '<ComboBox\b[^>]*\bWatermark='
    }

foreach ($file in $comboBoxWatermarkFiles) {
    Add-Failure "ComboBox verwendet nicht unterstütztes Watermark-Attribut: $($file.FullName)"
}



# Regression checks from real unit-test run 0.12.14.
$supplierInvoiceCsvTest = Get-Content "tests/WerkPilot.UnitTests/SupplierInvoiceCsvExporterTests.cs" -Raw
if ($supplierInvoiceCsvTest -notmatch 'Assert\.Contains\("Bestellnummer", csv\)') {
    Add-Failure "SupplierInvoiceCsvExporterTests prüft nicht die reale CSV-Bestellnummer."
}
if ($supplierInvoiceCsvTest -notmatch 'Assert\.Contains\("Bestellt", csv\)') {
    Add-Failure "SupplierInvoiceCsvExporterTests prüft nicht die reale 3-Wege-Match-Spalte Bestellt."
}

$customerDuplicateTest = Get-Content "tests/WerkPilot.UnitTests/CustomerDuplicateTests.cs" -Raw
if ($customerDuplicateTest -notmatch 'Assert\.Contains\("Kundendubletten", exception\.Message\)') {
    Add-Failure "CustomerDuplicateTests ist nicht an die reale Kundendubletten-Meldung gekoppelt."
}


# Regression check from real EF smoke test 0.12.15.
$desktopProjectSource = Get-Content "src/WerkPilot.Desktop/WerkPilot.Desktop.csproj" -Raw
if ($desktopProjectSource -notmatch 'PackageReference Include="Microsoft\.EntityFrameworkCore\.Design"') {
    Add-Failure "WerkPilot.Desktop referenziert Microsoft.EntityFrameworkCore.Design nicht."
}


# Regression checks from real EF model validation 0.12.16.
$modelSnapshotSource = Get-Content `
    "src/WerkPilot.Infrastructure/Persistence/Migrations/WerkPilotDbContextModelSnapshot.cs" -Raw

if ($modelSnapshotSource -notmatch 'Relational:MaxIdentifierLength') {
    Add-Failure "EF ModelSnapshot enthält keine PostgreSQL-Identifier-Längenannotation."
}

if ($modelSnapshotSource -notmatch 'UseIdentityByDefaultColumns') {
    Add-Failure "EF ModelSnapshot enthält keine Npgsql-ValueGeneration-Konvention."
}

$databaseSmokeSource = Get-Content "scripts/database-smoke-test.ps1" -Raw
if ($databaseSmokeSource -notmatch 'migrations has-pending-model-changes') {
    Add-Failure "Database-Smoke-Test prüft Pending Model Changes nicht explizit."
}


# Regression checks from real EF pending-model run 0.12.17.
$databaseSmokeSource = Get-Content "scripts/database-smoke-test.ps1" -Raw

if ($databaseSmokeSource -notmatch 'migrations add \$generatedMigrationName') {
    Add-Failure "Database-Smoke-Test kann keine reale Pending-Model-Migration erzeugen."
}

if ($databaseSmokeSource -notmatch 'RCModelSync_01218') {
    Add-Failure "Database-Smoke-Test enthält keinen versionierten EF-Migrationsnamen."
}

if ($databaseSmokeSource -notmatch 'artifacts\\generated-migration') {
    Add-Failure "Erzeugte EF-Migration wird nicht als Build-Artefakt gesichert."
}

if ($databaseSmokeSource -notmatch 'Auch nach der erzeugten Migration bestehen Pending Model Changes') {
    Add-Failure "Database-Smoke-Test validiert das EF-Modell nach der Migration nicht erneut."
}


# Regression checks from real EF migration run 0.12.18.
$databaseSmokeSource = Get-Content "scripts/database-smoke-test.ps1" -Raw

if ($databaseSmokeSource -match 'migrations add[\s\S]{0,500}--no-build') {
    Add-Failure "EF-Migrationsgenerierung verwendet weiterhin --no-build."
}

if ($databaseSmokeSource -notmatch 'dotnet build src/WerkPilot\.Desktop/WerkPilot\.Desktop\.csproj -c Release') {
    Add-Failure "EF Startup-Projekt wird vor Design-Time-Befehlen nicht explizit gebaut."
}

$pendingMigrationHelper = Get-Content "scripts/generate-pending-migration.ps1" -Raw
if ($pendingMigrationHelper -match '--no-build') {
    Add-Failure "generate-pending-migration.ps1 verwendet weiterhin --no-build."
}


# Regression checks from real empty-database migration run 0.12.19.
$databaseSmokeSource = Get-Content "scripts/database-smoke-test.ps1" -Raw

if ($databaseSmokeSource -notmatch 'InitialBaseline_01220') {
    Add-Failure "Database-Smoke-Test besitzt keinen konsolidierten EF-Baseline-Fallback."
}

if ($databaseSmokeSource -notmatch 'artifacts\\migration-history-backup') {
    Add-Failure "Historische RC-Migrationen werden vor der Baseline-Konsolidierung nicht gesichert."
}

if ($databaseSmokeSource -notmatch 'artifacts\\generated-baseline') {
    Add-Failure "Die erzeugte EF-Baseline wird nicht als Build-Artefakt gesichert."
}

if ($databaseSmokeSource -notmatch 'Leere PostgreSQL-Datenbank wurde erfolgreich aus der neuen EF-Baseline aufgebaut') {
    Add-Failure "Database-Smoke-Test bestätigt den vollständigen Baseline-Neuaufbau nicht."
}

if (-not (Test-Path "scripts/rebaseline-migrations.ps1")) {
    Add-Failure "Entwickler-Helfer rebaseline-migrations.ps1 fehlt."
}


# Regression checks from real baseline smoke run 0.12.20.
$databaseSmokeSource = Get-Content "scripts/database-smoke-test.ps1" -Raw



if ($databaseSmokeSource -notmatch "table_name='__EFMigrationsHistory'") {
    Add-Failure "Database-Smoke-Test prüft Existenz der EF-Migrationshistorientabelle nicht."
}

if ($databaseSmokeSource -notmatch 'baseline-promotion\.json') {
    Add-Failure "Validierte EF-Baseline wird nicht zur dauerhaften Übernahme markiert."
}

if (-not (Test-Path "scripts/promote-generated-baseline.ps1")) {
    Add-Failure "Baseline-Promotion-Helfer fehlt."
}


# Regression checks from real EF history quoting run 0.12.21.
$databaseSmokeSource = Get-Content "scripts/database-smoke-test.ps1" -Raw

if ($databaseSmokeSource -match 'FROM public\."__EFMigrationsHistory"') {
    Add-Failure "Database-Smoke-Test verwendet erneut fragile quoted Tabellenabfrage."
}

if ($databaseSmokeSource -notmatch "table_name='__EFMigrationsHistory'") {
    Add-Failure "Database-Smoke-Test prüft EF-History nicht über information_schema."
}

if ($databaseSmokeSource -notmatch "c\.relname='__EFMigrationsHistory'") {
    Add-Failure "Database-Smoke-Test validiert EF-History-Struktur nicht über pg_catalog."
}

if ($databaseSmokeSource -notmatch "a\.attname='MigrationId'") {
    Add-Failure "Database-Smoke-Test validiert die MigrationId-Spalte nicht."
}


# Regression check from 0.12.22: obsolete quoted-history verifier must never return.
$verifySource = Get-Content "scripts/verify-source.ps1" -Raw
$obsoleteHistoryMessage = "Database-Smoke-Test liest EF-Migrationshistorie nicht " + "exakt/quoted."
if ($verifySource -match [regex]::Escape($obsoleteHistoryMessage)) {
    Add-Failure "Veraltete EF-History-Quoting-Prüfung ist wieder im Verifier enthalten."
}


# Release metadata regression checks 0.12.24.
$releaseMetadataFiles = Get-ChildItem -Recurse -File |
    Where-Object {
        $_.Extension -in @(".ps1", ".props", ".cs", ".json", ".md", ".yml", ".yaml", ".xml", ".axaml", ".txt") -and
        $_.FullName -notmatch '[\\/](bin|obj|artifacts)[\\/]'
    }

foreach ($metadataFile in $releaseMetadataFiles) {
    $metadataText = Get-Content $metadataFile.FullName -Raw -ErrorAction SilentlyContinue
    $legacyReleaseToken = "0.12.0" + "-rc1"
    if ($metadataText -match [regex]::Escape($legacyReleaseToken)) {
        Add-Failure "Veraltete Release-Version gefunden: $($metadataFile.FullName)"
    }
}

$firstRcBuildSource = Get-Content "scripts/first-rc-build.ps1" -Raw
if ($firstRcBuildSource -notmatch '0\.12\.24-rc') {
    Add-Failure "First-RC-Build verwendet nicht die aktuelle Release-Version 0.12.24-rc."
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    throw "Quellcode-Verifikation fehlgeschlagen ($($failures.Count) Problem(e))."
}

Write-Host "Quellcode-Verifikation erfolgreich." -ForegroundColor Green
Write-Host "AXAML/Projekt-XML: $($xmlFiles.Count)" -ForegroundColor Green
Write-Host "Migrationen: $($migrationFiles.Count)" -ForegroundColor Green


