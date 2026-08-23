$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

Write-Host "WerkPilot Basic Workflow - Technical Smoke Test" -ForegroundColor Cyan

Write-Host "STEP 1 - Release build and unit tests" -ForegroundColor Cyan
& "$PSScriptRoot\build.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Release build or unit tests failed."
}

Write-Host "STEP 2 - Database migration on isolated test database" -ForegroundColor Cyan
& "$PSScriptRoot\database-smoke-test.ps1" -DatabaseName "werkpilot_basic_workflow"
if ($LASTEXITCODE -ne 0) {
    throw "Database smoke test failed."
}

Write-Host ""
Write-Host "STEP 3 - Manual business workflow required" -ForegroundColor Yellow
Write-Host "Start WerkPilot and open the menu item Basic-Prozesspruefung." -ForegroundColor Yellow
Write-Host "Test case:" -ForegroundColor Yellow
Write-Host "Customer -> Offer -> Calculation -> Accept offer -> Project -> Invoice -> Issue invoice -> Payment" -ForegroundColor Yellow
Write-Host "Additional case: overdue open invoice -> Dunning notice" -ForegroundColor Yellow
Write-Host "Expected result: no orphan references and stage Abgeschlossen with 100 percent." -ForegroundColor Green

exit 0
