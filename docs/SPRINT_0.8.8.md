# Sprint 0.8.8 – Lagerbewertung

## Fertiggestellt

- Lagerbewertung auf Basis des Einkaufspreises
- gesamter physischer Lagerwert
- Wert reservierter Bestände
- Wert verfügbarer Bestände
- Bewertung je Lagerartikel
- Preisalter in Tagen
- Warnung bei Einkaufspreisen älter als 90 Tage
- eigene Registerkarte im Lagerfenster
- CSV-Export der Lagerbewertung
- wertmäßige Bewertung von Inventurdifferenzen
- Preiswarnungen in Inventurpositionen
- absoluter Inventurdifferenzwert
- zusätzliche Modell- und Exporttests

## Berechnung

```text
Lagerwert = physischer Bestand × Einkaufspreis
Reservierungswert = reservierte Menge × Einkaufspreis
Verfügbarer Lagerwert = verfügbare Menge × Einkaufspreis
Inventurdifferenzwert = Mengendifferenz × Einkaufspreis
```

## Fachliche Regeln

- Die Bewertung verwendet den aktuellen Einkaufspreis aus dem Materialstamm.
- Preise älter als 90 Tage werden sichtbar als prüfbedürftig markiert.
- Negative Inventurdifferenzen erzeugen negative Differenzwerte.
- Der absolute Inventurdifferenzwert summiert die betragsmäßigen Abweichungen.
- Die Bewertung verändert weder Bestand noch Einkaufspreise.
- Für diesen Sprint ist keine neue Datenbankmigration erforderlich.
