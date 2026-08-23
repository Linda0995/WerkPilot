# Sprint 0.9.4 – PDF-Belege und Belegarchiv

## Fertiggestellt

- PDF-Ausgangsrechnung
- PDF-Gutschrift
- Firmenkopf aus dem Unternehmensprofil
- Positionsdarstellung mit Menge, Preis und Umsatzsteuer
- Netto-, Steuer-, Brutto-, Zahlungs-, Gutschrifts- und Restbeträge
- Referenz auf die ursprüngliche Rechnung bei Gutschriften
- automatische Ablage im Belegarchiv
- SHA-256-Prüfsumme je PDF
- JSON-Manifest mit Belegart, Nummer, Dateiname und Archivzeitpunkt
- PDF-Schaltflächen in Ausgangsrechnungen und Gutschriften
- zusätzlicher Archivtest
- keine neue Datenbankmigration erforderlich

## Archivstruktur

```text
Dokumente/WerkPilot/Belegarchiv/Ausgangsrechnungen
Dokumente/WerkPilot/Belegarchiv/Gutschriften
```
