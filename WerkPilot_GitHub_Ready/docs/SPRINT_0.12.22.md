# Sprint 0.12.22 - Robust EF History Verification

Der reale 0.12.21-Lauf hat erneut bestätigt, dass die neue EF-Baseline eine
komplett leere PostgreSQL-Datenbank erfolgreich aufbauen kann.

Der einzige verbleibende Fehler lag in der direkten SQL-Abfrage auf
`public."__EFMigrationsHistory"`. Beim Weg PowerShell -> Docker -> psql gingen
die notwendigen Anführungszeichen verloren.

0.12.22 vermeidet diese fragile Abfrage vollständig.

Stattdessen:
- wird die Tabelle über `information_schema.tables` gefunden,
- wird ihre Struktur über `pg_catalog` geprüft,
- wird die vorhandene `MigrationId`-Spalte validiert.

Damit ist keine problematische quoted identifier Übergabe mehr notwendig.
