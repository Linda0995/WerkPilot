# Sprint 0.12.19 - EF Design-Time Build Fix

Der reale 0.12.18-Lauf hat gezeigt, dass `dotnet ef migrations add` wegen
`WerkPilot.Desktop.deps.json does not exist` abbricht.

Ursache:
- Der neue EF-Workflow verwendete `--no-build`.
- Dadurch fehlten dem Startup-Projekt die Design-Time-Buildartefakte.

Korrektur:
- `--no-build` aus den EF-Migrationsbefehlen entfernt.
- `WerkPilot.Desktop` wird vor den EF-Design-Time-Befehlen explizit in Release gebaut.
- Auch das manuelle Helper-Skript verwendet kein `--no-build` mehr.
- Die echte Migration `RCModelSync_01218` wird weiterhin nur dann erzeugt,
  wenn EF tatsächliche Pending Model Changes meldet.
