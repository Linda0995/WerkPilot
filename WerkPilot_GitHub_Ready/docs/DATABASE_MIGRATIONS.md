# Datenbankmigrationen

## Datenbank aktualisieren

```powershell
.\scripts\update-database.ps1
```

Beim normalen Programmstart ruft WerkPilot ebenfalls `Database.MigrateAsync` auf.

## Neue Migration erstellen

```powershell
.\scripts\add-migration.ps1 -Name MeaningfulMigrationName
```

Danach werden die erzeugten Migrationsdateien zusammen mit der restlichen Codebasis
versioniert und im nächsten Sprint-ZIP ausgeliefert.

## Produktionsregel

Vor einem Produktivupdate ist eine Datenbanksicherung verpflichtend. Migrationen werden
zuerst in einer Testumgebung ausgeführt und anschließend mit dem Release freigegeben.

Die Initialmigration trägt explizite EF-Metadaten und wird durch einen automatisierten Metadatentest abgesichert.
