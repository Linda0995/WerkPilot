# Sprint 0.9.8 – Automatische Versandwarteschlange und SMTP-Diagnose

## Fertiggestellt

- automatische Verarbeitung fälliger Versandwiederholungen
- gehosteter Queue-Prozessor während der laufenden Anwendung
- Prüfintervall von einer Minute
- isolierte Verarbeitung jedes einzelnen Versandvorgangs
- maximale Stapelgröße pro Durchlauf
- manueller Start der Warteschlangenverarbeitung
- SMTP-Konfigurationsprüfung
- Netzwerk-Erreichbarkeit von SMTP-Host und Port
- Anzeige von Host, Port und SSL-Einstellung
- Queue-Ergebnis mit fälligen, erfolgreichen und fehlgeschlagenen Sendungen
- Protokollierung der Hintergrundverarbeitung
- zusätzliche DTO-Tests
- keine neue Datenbankmigration erforderlich

## Automatische Verarbeitung

Der Hintergrunddienst verarbeitet nur Sendungen mit:

```text
Status = Failed
NextRetryAtUtc <= aktuelle UTC-Zeit
```

Erfolgreiche Sendungen werden nicht erneut verarbeitet.

## SMTP-Diagnose

Die Diagnose prüft:

- erforderliche Umgebungsvariablen
- gültigen Port
- SSL-Einstellung
- Netzwerkverbindung zum SMTP-Server

Die SMTP-Anmeldung selbst wird weiterhin erst bei einem echten Versand geprüft.
