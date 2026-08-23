param(
    [Parameter(Mandatory = $true)]
    [string]$RunDirectory
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

if (-not (Test-Path $RunDirectory)) {
    throw "Diagnostic run directory not found: $RunDirectory"
}

$DiagnosticDir = Join-Path $RunDirectory "diagnostic"
if (Test-Path $DiagnosticDir) {
    Remove-Item $DiagnosticDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $DiagnosticDir | Out-Null

$FilesToCopy = @(
    "global.json",
    "Directory.Build.props",
    "Directory.Packages.props",
    "WerkPilot.sln",
    "CHANGELOG.md"
)

foreach ($File in $FilesToCopy) {
    if (Test-Path $File) {
        Copy-Item $File $DiagnosticDir -Force
    }
}

if ($null -ne (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    & dotnet --info | Out-File -FilePath (Join-Path $DiagnosticDir "dotnet-info.txt") -Encoding UTF8
}

if ($null -ne (Get-Command docker -ErrorAction SilentlyContinue)) {
    & docker version | Out-File -FilePath (Join-Path $DiagnosticDir "docker-version.txt") -Encoding UTF8
    & docker compose version | Out-File -FilePath (Join-Path $DiagnosticDir "docker-compose-version.txt") -Encoding UTF8
}

Get-ChildItem $RunDirectory -File | ForEach-Object {
    Copy-Item $_.FullName $DiagnosticDir -Force
}

$ZipPath = "$RunDirectory-diagnostic.zip"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Compress-Archive -Path (Join-Path $DiagnosticDir "*") -DestinationPath $ZipPath -CompressionLevel Optimal
Write-Host "Diagnostic package: $ZipPath" -ForegroundColor Yellow
