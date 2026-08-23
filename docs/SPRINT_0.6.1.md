# Sprint 0.6.1 – Angebots-PDF

## Fertiggestellt

- PDF-Exportservice als Application-Abstraktion
- QuestPDF-Implementierung in der Infrastruktur
- A4-Angebotslayout
- Kundenanschrift und UID/ATU
- Angebotsnummer, Datum, Gültigkeit und Status
- Positionstabelle
- Netto-, Steuer- und Bruttosummen
- Seitenzahlen
- Export in den Dokumentenordner
- Vorschau über das Standard-PDF-Programm
- dokumentierte Lizenzkonfiguration
- zusätzliche Dokumentdatentests

## Architektur

Die Fachlogik kennt QuestPDF nicht. Sie arbeitet ausschließlich gegen
`IOfferDocumentExporter`. Dadurch kann die PDF-Engine später ausgetauscht oder durch
firmenspezifische Layout-Renderer ergänzt werden.

## Abnahmekriterien

1. Ein gespeichertes Angebot kann als PDF exportiert werden.
2. Die PDF enthält Kunde, Angebotskopf, Positionen und Summen.
3. Die Vorschau öffnet die erzeugte Datei im Standardprogramm.
4. Der Dateiname basiert auf der Angebotsnummer.
5. Die PDF-Engine bleibt hinter einer Application-Schnittstelle gekapselt.
