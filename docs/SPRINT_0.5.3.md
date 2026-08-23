# Sprint 0.5.3 – CRM-Stabilisierung

## Fertiggestellt

- echte EF-Core-Initialmigration für PostgreSQL
- `Database.MigrateAsync` statt `EnsureCreated`
- Design-Time-DbContext-Factory für EF CLI
- Validierung von Kundenname, E-Mail, Land und Feldlängen
- strukturierte Validierungsfehler
- fehlertolerante Kundenanlage und Kundenbearbeitung
- sichtbare Fehlermeldung bei einem Datenbank-Startfehler
- PowerShell-Skripte für Migrationserstellung und Datenbankupdate
- zusätzliche Validierungstests

## Architekturentscheidung

Ab Version 0.5.3 wird jede Änderung des relationalen Schemas als fortlaufende
EF-Core-Migration auf derselben Codebasis dokumentiert. `EnsureCreated` ist nicht mehr
Teil des Anwendungsstarts.

## Abnahmekriterien

1. Eine leere PostgreSQL-Datenbank wird durch Migrationen aufgebaut.
2. Demo-Daten werden nur bei einer leeren Kundentabelle angelegt.
3. Ungültige E-Mail-Adressen und Ländercodes werden vor dem Speichern abgewiesen.
4. Ein Datenbankfehler beendet den Start nicht kommentarlos.
5. Tests decken die neuen Validierungsregeln ab.
