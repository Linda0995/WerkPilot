# Sprint 0.8.6 – Lagerbedarf und Nachbestellung

## Fertiggestellt

- automatische Ermittlung offener Materialbedarfe
- offene Bedarfe aus nicht bestellten Bestelllistenpositionen
- Berücksichtigung des verfügbaren Lagerbestands
- Berücksichtigung bestehender Reservierungen
- Berücksichtigung des Mindestbestands
- automatisch berechnete Fehlmenge
- Nachbestellvorschlag je Materialartikel
- Gruppierung nach Lieferant
- Lieferantenartikelnummer
- aktueller Einkaufspreis und geschätzter Bestellwert
- Warnung bei veraltetem Einkaufspreis
- eigene Registerkarte im Lagerfenster
- CSV-Export für den Einkauf
- zusätzliche Modell- und Exporttests

## Berechnung

```text
Verfügbarer Bestand = physischer Bestand - Reservierungen
Zielmenge = offener Bedarf + Mindestbestand
Bestellvorschlag = max(0, Zielmenge - verfügbarer Bestand)
```

## Regeln

- Bereits bestellte Positionen erzeugen keinen offenen Bedarf.
- Stornierte Bestelllisten werden nicht berücksichtigt.
- Angezeigt werden nur Artikel mit tatsächlicher Fehlmenge.
- Preise älter als 90 Tage werden zur Prüfung markiert.
