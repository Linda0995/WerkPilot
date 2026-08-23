# Sprint 0.12.2 – Testinstallation und Erststart

## Ziel

Nach dem ersten echten Build soll eine WerkPilot-Testinstallation kontrolliert
vorbereitet und ihre Startbereitschaft überprüft werden können.

## Neu

- `FirstRunReadinessService`
- Prüfung, ob DB-Verbindung konfiguriert ist
- Prüfung, ob PostgreSQL tatsächlich erreichbar ist
- Prüfung des Admin-Bootstrap-Status
- Prüfung des Demo-Modus
- neue Oberfläche `Erststart-Prüfung`
- `verify-release-package.ps1`
- SHA-256-Prüfung gegen Release-Manifest
- `prepare-test-installation.ps1`
- EXE-Prüfung nach Entpacken des Release-ZIPs
- RC-Pipeline prüft das erzeugte Release-ZIP zusätzlich
- keine neue Datenbankmigration erforderlich

## Erststart-Prüfung

Die Oberfläche zeigt nur Zustände und keine Kennwörter oder Connection-Strings.

`Startbereit` bedeutet:

```text
DB konfiguriert
+ DB erreichbar
+ Admin-Bootstrap vorhanden
```

## Release verifizieren

```powershell
.\scripts\verify-release-package.ps1
```

## Testinstallation vorbereiten

```powershell
.\scripts\prepare-test-installation.ps1
```
