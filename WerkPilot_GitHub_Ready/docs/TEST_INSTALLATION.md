# WerkPilot RC – Testinstallation

## Nach erfolgreichem RC-Build

1. Release-Prüfsumme kontrollieren:

```powershell
.\scripts\verify-release-package.ps1
```

2. Testinstallation in einen sauberen Ordner entpacken:

```powershell
.\scripts\prepare-test-installation.ps1
```

3. Datenbankverbindung und Admin-Bootstrap konfigurieren.

4. WerkPilot starten.

5. Im Hauptmenü `Erststart-Prüfung` öffnen.

Erwartung:

- Datenbank konfiguriert = Ja
- Datenbank erreichbar = Ja
- Admin-Bootstrap gesetzt = Ja
- Startbereit = Ja

Danach kann das RC-Smoke-Test-Protokoll abgearbeitet werden.
