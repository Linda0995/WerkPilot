# Sprint 0.10.2 – Aufgabenwarnungen und Eskalationen

## Fertiggestellt

- Kunden-Wiedervorlagen in der bestehenden Benachrichtigungszentrale
- automatische Hinweise bis sieben Tage vor Fälligkeit
- Eskalation nach Fälligkeit und Priorität
- dringende Aufgaben werden sofort kritisch behandelt
- heute fällige Aufgaben werden als Warnung angezeigt
- ab drei Tagen Überfälligkeit wird die Meldung kritisch
- Verantwortliche Person in der Meldungsbeschreibung
- Überfälligkeitstage in der Meldung
- stabiler Benachrichtigungsschlüssel je Aufgabe, Fälligkeit und Priorität
- bestehende Gelesen/Ungelesen-Logik wird wiederverwendet
- Hauptdashboard-Zähler berücksichtigt die neuen Hinweise automatisch
- zusätzliche Policy-Tests
- keine neue Datenbankmigration erforderlich

## Eskalationsregeln

### Information

- normale oder niedrige Priorität
- innerhalb der nächsten sieben Tage fällig
- noch nicht heute fällig

### Warning

- heute fällig
- ein oder zwei Tage überfällig
- Priorität `High`

### Critical

- mindestens drei Tage überfällig
- Priorität `Urgent`

Abgeschlossene und stornierte Wiedervorlagen erzeugen keine Benachrichtigungen.
