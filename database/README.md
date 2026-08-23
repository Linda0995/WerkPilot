# Datenbank

Seit WerkPilot 0.5.3 werden Datenbankänderungen ausschließlich über EF-Core-Migrationen
unter `src/WerkPilot.Infrastructure/Persistence/Migrations` verwaltet.

Die Anwendung führt ausstehende Migrationen beim Start aus. Für kontrollierte manuelle
Updates steht `scripts/update-database.ps1` zur Verfügung.
