# Sprint 0.6.0 – Bedienbare Angebotsverwaltung

## Fertiggestellt

- Angebotsfenster in Avalonia
- Kundenauswahl
- Angebotstitel und Gültigkeitsdatum
- automatische Angebotsanlage
- Angebotsliste
- Positionseditor für Beschreibung, Menge und Netto-Einzelpreis
- Live-Anzeige von Netto, Umsatzsteuer und Brutto
- Statusaktionen Gesendet, Angenommen und Abgelehnt
- rollenbasierte Schreibberechtigung
- zusätzliche Tests für Positionsberechnung und Eingabegrenzen

## Rollen

Administrator, Geschäftsleitung und Vertrieb dürfen Angebote bearbeiten.
Produktion und Nur-Lesen können Angebote ansehen, aber nicht verändern.

## Abnahmekriterien

1. Ein Angebot kann aus einem bestehenden Kunden heraus angelegt werden.
2. Positionen können einem Entwurf hinzugefügt werden.
3. Summen werden nach jeder Änderung aktualisiert.
4. Statuswechsel folgen dem Domänenworkflow.
5. Schreibaktionen beachten die aktive Benutzerrolle.
