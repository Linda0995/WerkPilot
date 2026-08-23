# Sprint 0.5.9 – Angebotsgrundmodul

## Fertiggestellt

- Angebotsdomäne
- automatische Angebotsnummern `AN-JJJJ-NNNN`
- Angebotsstatus Entwurf, Gesendet, Angenommen, Abgelehnt und Abgelaufen
- Angebotspositionen
- Netto-, Steuer- und Bruttosummen
- Bearbeitungsschutz nach dem Senden
- Application Service und Repository
- PostgreSQL-Mapping und EF-Core-Migration `OfferFoundation`
- Audit-Eintrag bei Anlage und Versand
- Unit-Tests für Summen und Statusworkflow

## Abnahmekriterien

1. Ein Angebot ist eindeutig einem Kunden zugeordnet.
2. Angebotsnummern werden jährlich fortlaufend erzeugt.
3. Positionen berechnen ihre Nettosumme nachvollziehbar.
4. Steuer und Bruttosumme werden kaufmännisch gerundet.
5. Ein Angebot ohne Positionen kann nicht gesendet werden.
6. Gesendete Angebote sind nicht mehr frei bearbeitbar.
