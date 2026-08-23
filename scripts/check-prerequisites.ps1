param(
    [switch]$RequireDocker
)

$ErrorActionPreference = "Stop"

function Require-Command {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$InstallHint
    )

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name wurde nicht gefunden. $InstallHint"
    }
}

Require-Command "dotnet" "Installiere das .NET 9 SDK von Microsoft."

$versionText = & dotnet --version
if (-not $versionText.StartsWith("9.")) {
    throw "WerkPilot benötigt .NET 9. Gefunden wurde: $versionText"
}

if ($RequireDocker) {
    Require-Command "docker" "Installiere Docker Desktop oder Docker Engine mit Compose-Unterstützung."

    & docker compose version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker Compose ist nicht verfügbar."
    }
}

Write-Host "Voraussetzungen erfüllt (.NET $versionText)." -ForegroundColor Green
