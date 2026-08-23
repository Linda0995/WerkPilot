param(
    [Parameter(Mandatory = $true)]
    [string]$Name
)

$ErrorActionPreference = "Stop"

dotnet ef migrations add $Name `
  --project src/WerkPilot.Infrastructure/WerkPilot.Infrastructure.csproj `
  --startup-project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj `
  --context WerkPilotDbContext `
  --output-dir Persistence/Migrations

Write-Host "Migration '$Name' wurde erstellt." -ForegroundColor Green
