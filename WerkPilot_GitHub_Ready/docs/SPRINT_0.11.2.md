# Sprint 0.11.2 – Datenbank- und Startpfad-Härtung

## Ziel

Nach dem Build-Härtungssprint wird jetzt der komplette lokale Datenbank- und
Erststartpfad reproduzierbar gemacht.

## Behoben / verbessert

- Demo-Kunde wird nicht mehr automatisch in jeder Datenbank angelegt
- Demo-Daten nur noch mit `WERKPILOT_SEED_DEMO_DATA=true`
- PostgreSQL-Bereitschaft wird vor Migration und Start aktiv geprüft
- `update-database.ps1` prüft zuerst die EF-Migrationsliste
- Migrationsergebnis wird explizit auf Exitcode geprüft
- neues isoliertes Datenbank-Smoke-Test-Skript
- Build-/Start-Verifikation um Datenbankregeln erweitert
- keine fachliche Datenbankmigration nötig

## Lokaler Erststart

```powershell
$env:WERKPILOT_ADMIN_INITIAL_PASSWORD = "Ein-starkes-temporäres-Kennwort!"
.\scripts\start.ps1
```

Lokal werden Demo-Daten standardmäßig aktiviert. Ohne Demo-Daten:

```powershell
.\scripts\start.ps1 -NoDemoData
```

## Isolierter Datenbanktest

```powershell
.\scripts\database-smoke-test.ps1
```

Dieser Test erstellt `werkpilot_smoketest` neu, wendet alle Migrationen auf eine
leere Datenbank an und kontrolliert Tabellen sowie `__EFMigrationsHistory`.
