param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

& "$PSScriptRoot\check-prerequisites.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Voraussetzungsprüfung fehlgeschlagen."
}

& "$PSScriptRoot\verify-source.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Quellcode-Verifikation fehlgeschlagen."
}

Write-Host "1/3 NuGet-Pakete wiederherstellen..." -ForegroundColor Cyan
& dotnet restore WerkPilot.sln
if ($LASTEXITCODE -ne 0) {
    throw "NuGet Restore fehlgeschlagen."
}

Write-Host "2/3 WerkPilot kompilieren..." -ForegroundColor Cyan
& dotnet build WerkPilot.sln -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    throw "WerkPilot Build fehlgeschlagen."
}

Write-Host "3/3 Unit-Tests ausführen..." -ForegroundColor Cyan
& dotnet test tests/WerkPilot.UnitTests/WerkPilot.UnitTests.csproj `
  -c $Configuration `
  --no-build `
  --logger "console;verbosity=normal"

if ($LASTEXITCODE -ne 0) {
    throw "WerkPilot Unit-Tests fehlgeschlagen."
}

Write-Host "WerkPilot 0.12.24 wurde erfolgreich gebaut und getestet." -ForegroundColor Green
exit 0
