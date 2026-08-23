param(
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)

Write-Host "Warte auf PostgreSQL..." -ForegroundColor Cyan

do {
    docker compose -f deploy/docker-compose.yml exec -T postgres `
        pg_isready -U werkpilot -d werkpilot *> $null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "PostgreSQL ist bereit." -ForegroundColor Green
        exit 0
    }

    Start-Sleep -Seconds 2
}
while ((Get-Date) -lt $deadline)

throw "PostgreSQL war nach $TimeoutSeconds Sekunden noch nicht bereit."
