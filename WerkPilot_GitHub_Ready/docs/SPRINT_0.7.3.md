# Sprint 0.7.3 – Operatives Dashboard

## Fertiggestellt

- eigener Application-Service für Dashboard-Kennzahlen
- offene Angebote und offenes Netto-Angebotsvolumen
- aktive Projekte
- offene Projektaufgaben
- fällige Aufgaben der nächsten 14 Tage
- gesonderte Überfälligkeitskennzeichnung
- aktuelle Angebote und Projekte in einer gemeinsamen Vorgangsliste
- Dashboard-Daten werden bei jeder Aktualisierung neu geladen
- zusätzliche Modelltests
- überarbeitete Dashboard-Oberfläche

## Kennzahlen

- offene Angebote: Entwurf und Gesendet
- aktive Projekte: Geplant, Aktiv und Angehalten
- offene Aufgaben: alle nicht erledigten Aufgaben aktiver Projekte
- fällige Aufgaben: Fälligkeit bis einschließlich 14 Tage ab heute
- überfällige Aufgaben: Fälligkeit liegt vor dem heutigen Tag

## Architektur

Die Dashboardlogik liegt im Application-Projekt. Die Avalonia-Oberfläche bindet nur
fertige DTOs. Dadurch kann dieselbe Datenquelle später für Benachrichtigungen, mobile
Ansichten und periodische Zusammenfassungen verwendet werden.
