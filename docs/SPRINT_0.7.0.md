# Sprint 0.7.0 – Projektmanagement

## Fertiggestellt

- persistentes Projektmodul
- automatische Projektnummern `PR-JJJJ-NNNN`
- Überführung angenommener Angebote in Projekte
- eindeutige Verbindung zwischen Angebot und Projekt
- Projekttitel, Beschreibung und Projektverantwortlicher
- geplanter Start und geplantes Ende
- Projektstatus Geplant, Aktiv, Angehalten, Abgeschlossen und Storniert
- Projektaufgaben mit Verantwortlichem, Fälligkeit und Status
- automatische Fortschrittsberechnung
- Anzahl offener Aufgaben
- Abschluss nur bei vollständig erledigten Aufgaben
- eigenes Avalonia-Projektfenster
- EF-Core-Migration `ProjectFoundation`
- Audit-Einträge für Projektanlage, Aufgaben und Statusänderungen
- zusätzliche Domänen- und Aufgabentests

## Fachliche Regeln

- Nur angenommene Angebote können in Projekte überführt werden.
- Pro Angebot wird höchstens ein Projekt angelegt.
- Ein Projekt kann erst abgeschlossen werden, wenn alle Aufgaben erledigt sind.
- Abgeschlossene und stornierte Projekte können nicht mehr bearbeitet werden.
- Der Fortschritt ergibt sich aus dem Anteil erledigter Aufgaben.
- Aufgaben werden nach dem Entfernen automatisch neu nummeriert.

## Abnahmekriterien

1. Ein angenommenes Angebot kann in ein Projekt überführt werden.
2. Projektstammdaten und Termine werden dauerhaft gespeichert.
3. Aufgaben können angelegt, bearbeitet, abgeschlossen und entfernt werden.
4. Fortschritt und offene Aufgaben werden automatisch berechnet.
5. Ein Projekt mit offenen Aufgaben kann nicht abgeschlossen werden.
