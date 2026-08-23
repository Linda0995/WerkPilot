# Sprint 0.12.18 - Real EF Migration Capture

Der reale Windows-Lauf von 0.12.17 hat bestätigt, dass tatsächlich Pending Model
Changes existieren.

Die exakte EF-Migration kann nur in einer Umgebung erzeugt werden, in der das
.NET-9-SDK, dotnet-ef, Npgsql und das vollständige Projekt geladen werden. Das
ist die reale Windows-Buildmaschine.

0.12.18 automatisiert diesen Schritt:

1. `dotnet ef migrations has-pending-model-changes`
2. Falls Änderungen vorliegen, erzeugt EF selbst:
   `RCModelSync_01218`
3. Die erzeugte Migration und der aktualisierte ModelSnapshot werden nach
   `artifacts/generated-migration` kopiert.
4. Pending Model Changes werden erneut geprüft.
5. Erst wenn das Modell synchron ist, läuft `database update` gegen die leere
   PostgreSQL-Smoke-Test-Datenbank.

Damit wird weder eine Warnung unterdrückt noch eine Migration geraten. Die
Migration stammt direkt aus dem tatsächlichen EF-Modell.

Hinweis:
Der Source-Tree wird bei diesem ersten Lauf absichtlich um die von EF generierte
Migration ergänzt. Ein anschließender Release-/Übergabestand kann diese Dateien
dann dauerhaft übernehmen.
