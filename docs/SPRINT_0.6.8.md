# Sprint 0.6.8 – Materialimport und Bestellliste

## Fertiggestellt

- CSV-Import für Materialstammdaten
- CSV-Export des vollständigen Materialstamms
- Importvorlage im Dokumentenordner
- Aktualisierung vorhandener Artikel über die Artikelnummer
- Fehlerprotokoll pro CSV-Zeile
- Kennzeichnung veralteter Einkaufspreise nach 90 Tagen
- dauerhafte Verknüpfung zwischen Kalkulationsposition und Materialartikel
- Bestellliste aus den Materialpositionen einer Angebotskalkulation
- Gruppierung gleicher Materialartikel
- aktueller Einkaufspreis und geschätzter Bestellwert
- Preiswarnung in Materialstamm und Bestellliste
- EF-Core-Migration `MaterialImportAndPurchaseList`
- zusätzliche CSV- und Verknüpfungstests

## CSV-Spalten

```text
Artikelnummer;Beschreibung;Einheit;Einkaufspreis;Lieferant;LieferantenArtikelnummer
```

Die erste Zeile ist die Kopfzeile. Dezimalzahlen werden im österreichischen Format
mit Komma unterstützt.

## Fachliche Regeln

- Die Artikelnummer entscheidet, ob ein Datensatz neu angelegt oder aktualisiert wird.
- Ein Preis gilt nach 90 Tagen als prüfungsbedürftig.
- Bestelllisten verwenden den aktuellen Materialstammpreis.
- Historische Kalkulationskosten bleiben unverändert.
- Gleiche Materialien werden in der Bestellliste mengenmäßig zusammengeführt.
