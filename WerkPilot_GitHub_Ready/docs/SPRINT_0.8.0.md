# Sprint 0.8.0 – Projekt-Zeiterfassung

## Fertiggestellt

- optionale Zeiterfassung je Projekt
- optionale Zuordnung zu einer Projektaufgabe
- Start- und Stoppfunktion
- nur eine laufende Zeiterfassung pro Benutzer
- manueller Zeiteintrag
- nachträgliche Korrektur eigener Einträge
- Tätigkeitsbeschreibung
- Beginn, Ende und berechnete Dauer
- Projektzeitsumme
- abgeschlossene und laufende Stunden
- eigenes Avalonia-Fenster
- benutzerspezifische Zeiteinträge
- EF-Core-Migration `ProjectTimeTracking`
- Audit-Einträge bei Start und Stopp
- zusätzliche Domänentests

## Fachliche Regeln

- Pro Benutzer darf höchstens ein Zeiteintrag gleichzeitig laufen.
- Das Ende muss nach dem Beginn liegen.
- Nur eigene Zeiteinträge dürfen nachträglich geändert werden.
- Laufende Einträge verwenden für die aktuelle Dauer den momentanen Zeitpunkt.
- Die Zeiterfassung ist projektbezogen und optional einer Aufgabe zugeordnet.
