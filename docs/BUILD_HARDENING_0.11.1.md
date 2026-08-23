# Build-Härtung 0.11.1

Auf einem Rechner mit .NET 9 SDK:

```powershell
.\scripts\build.ps1
```

Ablauf:

1. Voraussetzungen prüfen
2. statisches Quellcode-Gate
3. NuGet Restore
4. Release-Build
5. Unit-Tests

Der Build benötigt keine laufende PostgreSQL-Datenbank.
