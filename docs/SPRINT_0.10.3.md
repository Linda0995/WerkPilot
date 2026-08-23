# Sprint 0.10.3 – Persönliche Arbeitsliste „Meine Arbeit“

## Fertiggestellt

- neue persönliche Arbeitsansicht
- angemeldeter Benutzer als Filterbasis
- Kunden-Wiedervorlagen über Benutzer-ID oder Anzeigename
- Projektaufgaben über `AssignedTo`
- gemeinsame chronologische/priorisierte Liste
- Kennzahl für offene persönliche Aufgaben
- Kennzahl für heute fällige Aufgaben
- Kennzahl für überfällige Aufgaben
- Kennzahl für dringende Aufgaben
- getrennte Kennzahlen für Kunden- und Projektaufgaben
- Filter `Nur kritisch`
- eigener Hauptmenüpunkt `Meine Arbeit`
- zusätzliche DTO-Tests
- keine neue Datenbankmigration erforderlich

## Sortierung

1. überfällig
2. heute fällig
3. Priorität
4. Fälligkeit
5. Kontext

## Zuordnung

Kunden-Wiedervorlagen bevorzugen `AssignedUserId`, sofern vorhanden.
Als Rückfall wird der Anzeigename verwendet.

Projektaufgaben besitzen aktuell eine textuelle Zuordnung über `AssignedTo`.
Diese wird gegen den Anzeigenamen des angemeldeten Benutzers verglichen.
