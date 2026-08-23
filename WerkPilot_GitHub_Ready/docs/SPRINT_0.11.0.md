# Sprint 0.11.0 – Stabilisierung und Basic-1.0-Readiness

## Ziel

WerkPilot wird ab diesem Sprint nicht mehr primär durch neue Einzelmodule
vergrößert. Der Schwerpunkt liegt auf einer tatsächlich auslieferbaren Basic 1.0.

## Fertiggestellt

- .NET-9-SDK-Linie über `global.json` festgelegt
- Assembly-, Datei- und Produktversion zentral in `Directory.Build.props`
- `dotnet-ef` als lokales Tool versioniert
- Build-Prozess von PostgreSQL/Docker getrennt
- Datenbank-Update als eigener reproduzierbarer Schritt
- Startskript startet Infrastruktur und aktualisiert standardmäßig die Datenbank
- Windows-x64 Self-contained Publish-Skript
- statische Quellcode-Verifikation
- Runtime-Versionsangaben auf 0.11.0 vereinheitlicht
- Release-Checkliste
- Basic-1.0-Definition-of-Done

## Wichtige Änderung

Ein Build benötigt keine laufende PostgreSQL-Instanz. Dadurch kann ein Entwickler
oder CI-System WerkPilot kompilieren und Unit-Tests ausführen, ohne vorher Docker
zu starten.

## Lokaler Prüfpfad

```powershell
.\scripts\verify-source.ps1
.\scripts\build.ps1
.\scripts\update-database.ps1
.\scripts\start.ps1 -SkipDatabaseUpdate
```

## Windows-Paket

```powershell
.\scripts\publish-win-x64.ps1
```

Ausgabe:

```text
artifacts/publish/win-x64
```


## Während der Härtung behobene Altfehler

- fester Standard-Login `WerkPilot!2026` aus Quellcode und UI entfernt
- Erstadministrator verwendet jetzt `WERKPILOT_ADMIN_INITIAL_PASSWORD`
- Kennwortrichtlinie wird beim Bootstrap geprüft
- fehlende Klammern im Erstbenutzer-Block von `DbInitializer` korrigiert
- Produktionsstart besitzt keine fest eingebaute PostgreSQL-Verbindung mehr
- Development-Datenbankkonfiguration vom Publish getrennt
