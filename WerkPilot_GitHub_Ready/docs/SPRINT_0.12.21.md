# Sprint 0.12.21 - EF History Verification Fix

Der reale Windows-Lauf von 0.12.20 hat erfolgreich eine neue EF-Baseline erzeugt
und eine komplett leere PostgreSQL-Datenbank daraus aufgebaut.

Der einzige Fehler lag anschließend in der eigenen Verifikation der
EF-Migrationshistorie.

Korrekturen:

- Existenz von `__EFMigrationsHistory` wird über `information_schema.tables`
  geprüft.
- Der eigentliche Zähler verwendet exakt:
  `public."__EFMigrationsHistory"`.
- Eine leere History-Tabelle gilt als Fehler.
- Bei fehlender History-Tabelle werden alle öffentlichen Tabellen für die
  Diagnose ausgegeben.
- Eine erfolgreich validierte Baseline erhält jetzt zusätzlich
  `baseline-promotion.json`.
- `promote-generated-baseline.ps1` ermöglicht einem Entwickler, die validierten
  Baseline-Dateien aus dem Artifact-Ordner in einen festen Source-Stand zu
  übernehmen.

Wichtig:
Die konkrete auf dem Windows-Rechner erzeugte `InitialBaseline_01220` kann in
diesem Paket nicht vorab enthalten sein, weil sie erst dort von EF generiert
wurde. Der Build sichert und markiert sie deshalb reproduzierbar als Artefakt.
