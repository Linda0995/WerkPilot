# WerkPilot 0.12.0 RC1 – Release Candidate

## Zweck

0.12.0 ist der erste vorbereitete Basic-Release-Candidate. Ab hier werden keine
neuen Fachmodule mehr in den RC aufgenommen, außer sie beseitigen einen
Blocker für Basic 1.0.

## Technische RC-Pipeline

```powershell
.\scripts\release-candidate.ps1
```

Die Pipeline führt aus:

1. Voraussetzungen und statisches Release-Gate
2. Release-Build
3. Unit-Tests
4. isolierter PostgreSQL-Migrations-Smoke-Test
5. Windows-x64 Self-contained Publish
6. Release-ZIP
7. SHA-256-Prüfsummen
8. Release-Manifest

## Ausgaben

```text
artifacts/publish/win-x64
artifacts/release/WerkPilot-0.12.24-rc-win-x64.zip
artifacts/release/release-manifest.json
```

## RC-Freigabe

RC1 darf erst zu Basic 1.0 hochgestuft werden, wenn:

- Build erfolgreich
- Unit-Tests erfolgreich
- DB-Smoke-Test erfolgreich
- Windows-Publish erfolgreich
- Anwendung auf sauberem Windows-Testsystem startet
- Login funktioniert
- Basic-Geschäftsablauf vollständig getestet
- keine Blocker im Smoke-Test-Protokoll
