# Sprint 0.12.20 - EF Baseline Rebuild

Der reale 0.12.19-Lauf hat bestätigt:

- Build erfolgreich
- 212/212 Unit-Tests erfolgreich
- EF erkennt und erzeugt die reale Modell-Synchronisationsmigration
- ModelSnapshot und aktuelles Modell sind anschließend synchron

Der verbleibende Fehler liegt in der historischen RC-Migrationskette:
Eine komplett leere PostgreSQL-Datenbank kann nicht zuverlässig von Migration 1
bis heute aufgebaut werden.

Da WerkPilot noch vor dem ersten Produktiv-Release steht, ist jetzt der richtige
Zeitpunkt, die während der Entwicklung entstandenen historischen Migrationen zu
einer sauberen Baseline zu konsolidieren.

0.12.20 macht dies sicher und reproduzierbar:

1. Zuerst wird die bestehende Migrationskette normal getestet.
2. Scheitert der Neuaufbau einer leeren Datenbank, werden alle bisherigen
   RC-Migrationen nach `artifacts/migration-history-backup` gesichert.
3. EF erzeugt direkt aus dem aktuellen Modell eine vollständige
   `InitialBaseline_01220`.
4. Baseline und aktuelles Modell werden mit
   `has-pending-model-changes` gegengeprüft.
5. Die Smoke-Test-Datenbank wird erneut komplett leer erstellt.
6. Die neue Baseline muss diese Datenbank vollständig aufbauen.
7. Die erzeugte Baseline wird zusätzlich unter
   `artifacts/generated-baseline` gesichert.

Es werden keine Produktivdaten gelöscht. Der Vorgang betrifft die
Migrations-Quelldateien und die isolierte `werkpilot_first_rc_build`
Smoke-Test-Datenbank.

Für einen späteren öffentlichen Release ist diese konsolidierte Baseline
wesentlich sauberer als die während der Entwicklung gewachsene RC-Historie.
