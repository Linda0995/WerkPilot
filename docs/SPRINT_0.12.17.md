# Sprint 0.12.17 - EF ModelSnapshot Provider Fix

Der reale 0.12.16-Lauf hat `PendingModelChangesWarning` ausgelöst.

Die Prüfung des Repositories zeigt, dass `WerkPilotDbContextModelSnapshot` kein
normal vollständig generierter Npgsql-Snapshot ist. Er delegiert zwar an dieselbe
`WerkPilotModelConfiguration`, lässt aber die PostgreSQL-Provider-Annotationen
weg, die das Design-Time-Modell automatisch besitzt.

Das kann EF Core als Modellabweichung interpretieren, obwohl keine fachliche
Schemaänderung vorliegt.

Korrektur:
- `Relational:MaxIdentifierLength = 63` im Snapshot ergänzt.
- Npgsql `UseIdentityByDefaultColumns` im Snapshot ergänzt.
- Database-Smoke-Test führt vor `database update` jetzt explizit
  `dotnet ef migrations has-pending-model-changes` aus.
- Keine künstliche/leere Datenbankmigration erzeugt: Eine Migration ohne echte
  Schemaänderung würde das Problem nur verdecken.

Falls der reale Windows-Lauf weiterhin echte Pending Changes meldet, liefert der
neue Preflight einen klaren isolierten Fehlerpunkt; dann wird die konkrete
Schemaänderung als echte Migration ergänzt.
