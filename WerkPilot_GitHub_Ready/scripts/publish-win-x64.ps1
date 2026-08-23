param(
    [string]$OutputPath = "artifacts/publish/win-x64"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

& "$PSScriptRoot/check-prerequisites.ps1"
& "$PSScriptRoot/verify-source.ps1"

if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

dotnet restore WerkPilot.sln

dotnet publish src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=false `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o $OutputPath

if (-not (Test-Path (Join-Path $OutputPath "WerkPilot.Desktop.exe"))) {
    throw "Publish wurde ausgeführt, aber WerkPilot.Desktop.exe fehlt."
}

Write-Host "Windows-Paket erstellt: $OutputPath" -ForegroundColor Green
