# Fehlerbehandlung und Support

## Grundprinzip

WerkPilot trennt zwischen:

1. fachlichen / erwarteten Fehlern
2. technischen / unerwarteten Fehlern

Fachliche Fehler dürfen eine verständliche Meldung anzeigen. Technische Fehler
werden geloggt und erhalten eine Fehler-ID.

## Supportablauf

Wenn ein Benutzer eine Meldung wie

```text
Vorgang konnte nicht abgeschlossen werden. Fehler-ID: 91A24FBC
```

sieht, wird für die Fehlersuche die Fehler-ID gemeinsam mit dem Zeitpunkt
verwendet. Die technischen Details bleiben im WerkPilot-Log.

## Logs

Standardmäßig schreibt die Desktop-Anwendung rollierende Logs nach:

```text
logs/werkpilot-YYYYMMDD.log
```

Passwörter und andere geheime Werte dürfen nicht absichtlich in Logs geschrieben
werden.
