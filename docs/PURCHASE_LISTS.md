# Bestelllisten

## Erzeugung

Eine Bestellliste wird aus den mit dem Materialstamm verknüpften Positionen einer
Angebotskalkulation erzeugt. Artikel ohne Materialverknüpfung erscheinen nicht in der
Bestellliste.

## Manueller Bestellworkflow

1. Lieferant kontaktieren.
2. Position auswählen.
3. Optional eine Notiz eintragen, beispielsweise:
   - telefonisch bestellt
   - Bestellung per E-Mail
   - Auftragsnummer des Lieferanten
   - voraussichtlicher Liefertermin
4. „Bestellt / wieder offen“ betätigen.
5. Bei Bedarf den Status wieder zurücknehmen.

## Export

CSV-Dateien werden unter folgendem Ordner gespeichert:

```text
Dokumente/WerkPilot/Exporte/Bestelllisten
```

Der Export enthält Lieferant, Artikel, Menge, Preis, Schätzwert, Bestellstatus,
Zeitstempel und Notiz.
