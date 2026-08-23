# Sprint 0.10.7 – Abwesenheiten und Vertretungsplanung

## Fertiggestellt

- Abwesenheitszeiträume je aktivem Benutzer
- Abwesenheitsarten Urlaub, Krankenstand, Schulung, Dienstreise und Sonstiges
- Start- und Enddatum
- optionale Vertretung über eindeutige Benutzer-ID
- Status Geplant, Aktiv, Abgeschlossen und Storniert
- automatische Statusaktualisierung anhand des Tagesdatums
- Schutz vor überlappenden Abwesenheiten
- Schutz vor Selbstvertretung
- Konfliktprüfung gegen offene Kunden-Wiedervorlagen
- Konfliktprüfung gegen offene Projektaufgaben
- Ermittlung der im Abwesenheitszeitraum fälligen Aufgaben
- Benachrichtigung für eigene bevorstehende oder aktive Abwesenheiten
- Warnung, wenn keine Vertretung hinterlegt ist
- neues Avalonia-Fenster und Hauptmenüpunkt
- EF-Core-Migration `UserAbsences`
- zusätzliche Tests

## Prinzip

Die Abwesenheitsplanung übergibt Aufgaben nicht automatisch. WerkPilot erkennt
Konflikte und weist auf fehlende Vertretungen hin. Die tatsächliche Änderung der
Verantwortlichkeit bleibt bewusst im kontrollierten Modul `Aufgabenübergabe`.
