# Sprint 0.10.8 – Vertretungsvorschau und selektive Aufgabenübergabe

## Fertiggestellt

- konkrete Liste aller von einer Abwesenheit betroffenen offenen Aufgaben
- Kunden-Wiedervorlagen und Projektaufgaben gemeinsam
- Kennzeichnung, ob eine Aufgabe während des Abwesenheitszeitraums fällig wird
- Fälligkeit, Priorität, Referenz, Kontext und Titel in der Vorschau
- Übergabe nur der während der Abwesenheit fälligen Aufgaben
- alternativ Übergabe aller offenen Aufgaben
- hinterlegte Vertretung als Ziel
- Übergabegrund ist Pflicht
- keine automatische Umbuchung ohne Benutzeraktion
- Audit-Trail je einzelner Aufgabe
- zusätzlicher zusammenfassender Audit-Eintrag an der Abwesenheit
- Aktualisierung der Vorschau nach einer Übergabe
- zusätzliche DTO- und Ergebnis-Tests
- keine neue Datenbankmigration erforderlich

## Sicherheitsprinzip

WerkPilot darf erkennen und vorschlagen, aber die Verantwortlichkeit wird erst
durch eine explizite Benutzeraktion geändert.

Damit bleibt klar unterscheidbar zwischen:

1. Abwesenheit erfassen
2. Konflikte analysieren
3. Übergabeumfang prüfen
4. Aufgaben kontrolliert übertragen
