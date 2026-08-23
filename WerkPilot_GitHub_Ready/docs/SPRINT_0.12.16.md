# Sprint 0.12.16 - EF Core Design-Time Startup Fix

Der reale 0.12.15-Lauf hat den kompletten Build und alle 212 Unit-Tests
erfolgreich abgeschlossen. Der anschließende PostgreSQL-Smoke-Test scheiterte
bei `dotnet ef database update`, weil das Startup-Projekt `WerkPilot.Desktop`
`Microsoft.EntityFrameworkCore.Design` nicht direkt referenziert hat.

Korrektur:
- `Microsoft.EntityFrameworkCore.Design` im Desktop-Startup-Projekt ergänzt.
- Package bleibt mit `PrivateAssets=all` eine reine Entwicklungs-/Design-Time-Abhängigkeit.
- Database-Smoke-Test prüft die Voraussetzung vor `dotnet ef`.
- Source-Verifikation enthält einen Regression-Check.

Die vorhandene EF-Design-Abhängigkeit im Infrastructure-Projekt bleibt erhalten,
da dort DbContext und Migrationen liegen.
