# Sprint 0.6.6 – Angebotskalkulation

## Fertiggestellt

- eigenständiges Kalkulationsmodul pro Angebot
- Kostenarten Material, Arbeitszeit, Fremdleistung und Gemeinkosten
- Kalkulationspositionen anlegen, bearbeiten und entfernen
- automatische Positionsnummerierung
- Kostenübersicht nach Kostenart
- frei einstellbares Firmenziel in Prozent
- Zielgewinn und empfohlener Nettoverkaufspreis
- eigenes Avalonia-Kalkulationsfenster
- PostgreSQL-Persistenz
- EF-Core-Migration `OfferCalculationFoundation`
- Audit-Einträge für Positionen und Firmenziel
- zusätzliche Domänentests

## Fachliche Trennung

Die Angebotspositionen stellen den Verkaufspreis für den Kunden dar.
Die Kalkulationspositionen bilden die internen Kosten ab und erscheinen nicht im
Angebots-PDF. Dadurch bleiben interne Stundensätze, Einstandspreise und Gewinnziele
vertraulich.

## Abnahmekriterien

1. Jedes Angebot besitzt höchstens eine Kalkulation.
2. Material, Arbeitszeit, Fremdleistung und Gemeinkosten werden getrennt summiert.
3. Das Firmenziel wird auf die Gesamtkosten angewendet.
4. Der empfohlene Nettoverkaufspreis ist nachvollziehbar berechnet.
5. Interne Kalkulationswerte werden nicht in Kundendokumente übernommen.
