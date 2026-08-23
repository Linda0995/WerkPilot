# Datenbank- und Startup-Smoke-Test

## Was geprüft wird

1. Docker / Compose verfügbar
2. PostgreSQL-Container startet
3. `pg_isready` meldet bereit
4. isolierte Testdatenbank wird erstellt
5. EF-Core-Migrationen laufen von Null bis aktuell
6. öffentliche Tabellen sind vorhanden
7. `__EFMigrationsHistory` ist lesbar

## Warum eine getrennte Testdatenbank?

Der Smoke-Test greift nicht auf die normale lokale WerkPilot-Datenbank zu. Damit
kann der Migrationspfad wiederholt getestet werden, ohne Entwicklungsdaten zu
zerstören.

## Demo-Daten

Produktiv werden niemals automatisch Demo-Kunden angelegt. Demo-Daten sind nur
aktiv, wenn ausdrücklich gesetzt wird:

```text
WERKPILOT_SEED_DEMO_DATA=true
```
