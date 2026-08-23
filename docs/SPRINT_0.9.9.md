# Sprint 0.9.9 – Kundenkommunikationsakte

## Fertiggestellt

- zentrale Kommunikationsakte je Kunde
- versendete Angebote aus dem Audit-Trail
- Ausgangsrechnungen aus dem Belegversand
- Gutschriften aus dem Belegversand
- Mahnungen aus dem Belegversand
- chronologische Kommunikationszeitachse
- Empfänger, Betreff, Status und Fehlertext
- Kennzahlen je Kunde
- Suche nach Kundenname, Kundennummer oder E-Mail
- eigenes Avalonia-Fenster
- neuer Hauptmenüpunkt `Kommunikationsakte`
- zusätzliche DTO-Tests
- keine neue Datenbankmigration erforderlich

## Architektur

Die Kommunikationsakte speichert keine Kopien der Belege. Sie aggregiert bestehende
Daten aus Kundenverwaltung, Angebots-Audit und Belegversandprotokoll.
