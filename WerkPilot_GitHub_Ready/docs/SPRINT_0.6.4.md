# Sprint 0.6.4 – Angebotsbearbeitung und Versionierung

## Fertiggestellt

- Angebotspositionen auswählen und bearbeiten
- Angebotspositionen entfernen
- automatische Neunummerierung nach dem Entfernen
- Angebotsentwürfe duplizieren
- alle Positionen werden in die Kopie übernommen
- Kopien erhalten eine neue Angebotsnummer und bleiben Entwurf
- automatische Kennzeichnung abgelaufener gesendeter Angebote
- Audit-Einträge für Positionsänderungen und Duplikate
- zusätzliche Unit-Tests

## Fachliche Regeln

- Nur Angebotsentwürfe dürfen verändert werden.
- Gesendete, angenommene, abgelehnte und abgelaufene Angebote bleiben unverändert.
- Ein Duplikat beginnt immer als neuer Entwurf.
- Ein gesendetes Angebot wird nach Überschreiten des Gültigkeitsdatums als abgelaufen markiert.

## Abnahmekriterien

1. Positionen können innerhalb eines Entwurfs geändert werden.
2. Gelöschte Positionen hinterlassen keine Lücke in der Positionsnummerierung.
3. Ein Angebot kann vollständig kopiert werden.
4. Kopien erhalten eine neue fortlaufende Angebotsnummer.
5. Überfällige gesendete Angebote werden automatisch als abgelaufen markiert.
