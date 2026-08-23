# Buildstatus

## Lieferumgebung
In der zur Erstellung verwendeten Ausführungsumgebung war kein .NET-SDK installiert.
Zusätzlich bestand kein Internetzugang für die NuGet-Wiederherstellung.

Daher konnte der reale `dotnet restore/build/test`-Lauf in dieser Umgebung nicht ausgeführt werden.

## Reproduzierbare Prüfung
Auf einem Rechner mit .NET 9 SDK und Internetzugang:

```powershell
docker compose -f deploy/docker-compose.yml up -d
dotnet restore WerkPilot.sln
dotnet build WerkPilot.sln -c Release
dotnet test WerkPilot.sln -c Release
```

Erst ein erfolgreicher Lauf dieser Befehle bestätigt den Buildstatus verbindlich.
