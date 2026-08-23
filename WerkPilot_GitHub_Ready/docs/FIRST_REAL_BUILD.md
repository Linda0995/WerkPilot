# Der erste echte WerkPilot-Build

## Voraussetzungen

- Windows 10/11
- .NET 9 SDK
- Docker Desktop
- PowerShell
- Internetzugriff für den ersten NuGet-Restore

## Ablauf

1. WerkPilot-Quell-ZIP entpacken.
2. PowerShell im WerkPilot-Verzeichnis öffnen.
3. Ausführen:

```powershell
.\scripts\first-rc-build.ps1
```

Der Runner übernimmt:

- statische Quellcodeprüfung
- NuGet Restore
- Release-Build
- Unit-Tests
- PostgreSQL-Smoke-Test
- Windows-x64 Publish
- Release-Paket
- Diagnoseprotokoll

## Falls der Build fehlschlägt

Nicht einzelne Fehlermeldungen abschreiben. Verwende das automatisch erzeugte:

```text
*-diagnostic.zip
```

Damit kann der konkrete Compiler-/Restore-/Migration-/Publishfehler direkt
analysiert werden.
