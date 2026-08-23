# Sprint 0.11.4 – Fehlerfälle und Bedienung

## Ziel

WerkPilot darf im normalen Betrieb keine ungefilterten technischen Exceptions
in Statuszeilen anzeigen. Fachliche Fehler sollen verständlich sein, technische
Fehler sollen sicher protokolliert werden.

## Umgesetzt

- zentraler `UiErrorFormatter`
- technische Fehler erhalten eine kurze Fehler-ID
- vollständige Exception inklusive Stacktrace wird über Serilog protokolliert
- Validierungs-/Eingabefehler bleiben für den Benutzer lesbar
- technische Exception-Nachrichten werden nicht direkt angezeigt
- Startup-Datenbankfehler verwenden ebenfalls eine sichere Fehler-ID
- globale Protokollierung für unbehandelte AppDomain-Exceptions
- globale Protokollierung für unbeobachtete Task-Exceptions
- statisches Release-Gate verhindert neue direkte `ex.Message`-Statusausgaben
- 154 direkte Status-Exception-Ausgaben wurden ersetzt
- 35 ViewModel-Dateien wurden auf die zentrale Behandlung umgestellt
- keine Datenbankmigration erforderlich

## Verhalten

### Erwarteter Eingabefehler

```text
Wiedervorlage konnte nicht erstellt werden: Fälligkeit ist ungültig.
```

### Technischer Fehler

```text
Wiedervorlage konnte nicht erstellt werden. Bitte erneut versuchen. Fehler-ID: A1B2C3D4
```

Im Log befindet sich zur Fehler-ID der vollständige technische Fehler.

## Sicherheitsnutzen

Datenbankverbindungen, interne Hosts, Stacktraces oder andere technische Details
werden bei unerwarteten Fehlern nicht mehr ungefiltert in der Oberfläche gezeigt.
