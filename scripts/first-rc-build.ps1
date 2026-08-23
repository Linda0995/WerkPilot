param([switch]$SkipDatabaseSmokeTest)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$RunDir = Join-Path "artifacts\first-build" $Timestamp
New-Item -ItemType Directory -Force -Path $RunDir | Out-Null

$TranscriptPath = Join-Path $RunDir "first-build-transcript.txt"
$SummaryPath = Join-Path $RunDir "first-build-summary.json"
$ErrorPath = Join-Path $RunDir "powershell-error.txt"

$BuildSucceeded = $false
$TranscriptStarted = $false

try {
    Start-Transcript -Path $TranscriptPath -Force | Out-Null
    $TranscriptStarted = $true

    Write-Host "WerkPilot 0.12.24 RC - First RC Build" -ForegroundColor Cyan

    $DotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $DotnetCommand) {
        throw "dotnet was not found. Install the .NET 9 SDK."
    }

    $DotnetVersion = (& dotnet --version).Trim()
    Write-Host ".NET SDK: $DotnetVersion"

    if (-not $DotnetVersion.StartsWith("9.")) {
        throw "WerkPilot requires .NET 9. Found: $DotnetVersion"
    }

    $DockerCommand = Get-Command docker -ErrorAction SilentlyContinue
    if ($null -eq $DockerCommand) {
        throw "docker was not found. Install and start Docker Desktop."
    }

    & docker --version
    if ($LASTEXITCODE -ne 0) { throw "docker --version failed." }

    & docker compose version
    if ($LASTEXITCODE -ne 0) { throw "docker compose is not available." }

    Write-Host "STEP 1 - PowerShell syntax validation" -ForegroundColor Cyan
    & "$PSScriptRoot\test-powershell-syntax.ps1"

    Write-Host "STEP 2 - Static source verification" -ForegroundColor Cyan
    & "$PSScriptRoot\verify-source.ps1"

    Write-Host "STEP 3 - Release build and unit tests" -ForegroundColor Cyan
    & "$PSScriptRoot\build.ps1"

    if (-not $SkipDatabaseSmokeTest) {
        Write-Host "STEP 4 - Database smoke test" -ForegroundColor Cyan
        & "$PSScriptRoot\database-smoke-test.ps1" -DatabaseName "werkpilot_first_rc_build"
    }
    else {
        Write-Host "STEP 4 - Database smoke test skipped" -ForegroundColor Yellow
    }

    Write-Host "STEP 5 - Windows x64 publish" -ForegroundColor Cyan
    & "$PSScriptRoot\publish-win-x64.ps1"

    Write-Host "STEP 6 - Create release package" -ForegroundColor Cyan
    & "$PSScriptRoot\create-release-package.ps1"

    Write-Host "STEP 7 - Verify release package" -ForegroundColor Cyan
    & "$PSScriptRoot\verify-release-package.ps1"

    $BuildSucceeded = $true
    Write-Host "FIRST RC BUILD SUCCESSFUL." -ForegroundColor Green
}
catch {
    Write-Host "FIRST RC BUILD FAILED." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    $_ | Format-List * -Force | Out-File -FilePath $ErrorPath -Encoding UTF8
}
finally {
    $DotnetSummary = "not-found"
    if ($null -ne (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        try { $DotnetSummary = (& dotnet --version).Trim() } catch { $DotnetSummary = "error" }
    }

    $DockerSummary = "not-found"
    if ($null -ne (Get-Command docker -ErrorAction SilentlyContinue)) {
        try { $DockerSummary = (& docker --version | Out-String).Trim() } catch { $DockerSummary = "error" }
    }

    $Summary = [ordered]@{
        version = "0.12.24-rc"
        completedAt = [DateTimeOffset]::Now.ToString("O")
        success = $BuildSucceeded
        machine = $env:COMPUTERNAME
        user = $env:USERNAME
        os = [System.Environment]::OSVersion.VersionString
        dotnet = $DotnetSummary
        docker = $DockerSummary
    }

    $Summary | ConvertTo-Json -Depth 4 | Set-Content -Path $SummaryPath -Encoding UTF8

    if ($TranscriptStarted) {
        try { Stop-Transcript | Out-Null } catch { }
    }

    try {
        & "$PSScriptRoot\create-build-diagnostic-bundle.ps1" -RunDirectory $RunDir
    }
    catch {
        Write-Host "Diagnostic bundle creation failed: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    if (-not $BuildSucceeded) {
        Write-Host "Build failed. Send the generated diagnostic ZIP for analysis." -ForegroundColor Yellow
        exit 1
    }
}

exit 0
