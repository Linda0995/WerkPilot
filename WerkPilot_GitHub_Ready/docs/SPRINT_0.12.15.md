# Sprint 0.12.15 - Unit Test Contract Alignment

Der reale Windows-Lauf von 0.12.14 hat 210 von 212 Tests bestanden.
Die zwei Fehlschläge waren veraltete Test-Erwartungen, nicht fachliche Fehler
der Implementierung.

1. SupplierInvoiceCsvExporter
   - Die reale CSV enthält `Bestellnummer` und die 3-Wege-Match-Spalte `Bestellt`.
   - Der Test erwartete den nicht vorhandenen Wortlaut `Bestellung`.
   - Der Test prüft nun die tatsächlichen fachlichen CSV-Felder.

2. CustomerDuplicateException
   - Die reale Meldung lautet `Es wurden mögliche Kundendubletten gefunden.`
   - Der Test erwartete nur `Dubletten`, was wegen Groß-/Kleinschreibung nicht traf.
   - Der Test prüft nun gezielt `Kundendubletten`.

Produktionslogik wurde für diese beiden Fälle bewusst nicht verändert.
