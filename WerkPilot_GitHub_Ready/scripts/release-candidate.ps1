param(
    [switch]$SkipDatabaseSmokeTest
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

Write-Host "WerkPilot 0.12.11 RC - Release Candidate Pipeline" -ForegroundColor Cyan

Write-Host "STEP 1 - Check prerequisites and source" -ForegroundColor Cyan
& "$PSScriptRoot\check-prerequisites.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Prerequisite check failed."
}

& "$PSScriptRoot\test-powershell-syntax.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "PowerShell syntax validation failed."
}

& "$PSScriptRoot\verify-source.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Static source verification failed."
}

Write-Host "STEP 2 - Release build and unit tests" -ForegroundColor Cyan
& "$PSScriptRoot\build.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Release build or unit tests failed."
}

if (-not $SkipDatabaseSmokeTest) {
    Write-Host "STEP 3 - Database smoke test" -ForegroundColor Cyan
    & "$PSScriptRoot\database-smoke-test.ps1" -DatabaseName "werkpilot_rc_01211"
    if ($LASTEXITCODE -ne 0) {
        throw "Database smoke test failed."
    }
}
else {
    Write-Host "STEP 3 - Database smoke test skipped" -ForegroundColor Yellow
}

Write-Host "STEP 4 - Windows x64 publish" -ForegroundColor Cyan
& "$PSScriptRoot\publish-win-x64.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Windows publish failed."
}

Write-Host "STEP 5 - Create release package" -ForegroundColor Cyan
& "$PSScriptRoot\create-release-package.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Release package creation failed."
}

Write-Host "STEP 6 - Verify release package" -ForegroundColor Cyan
& "$PSScriptRoot\verify-release-package.ps1"
if ($LASTEXITCODE -ne 0) {
    throw "Release package verification failed."
}

Write-Host "RELEASE CANDIDATE PIPELINE SUCCESSFUL." -ForegroundColor Green
exit 0
