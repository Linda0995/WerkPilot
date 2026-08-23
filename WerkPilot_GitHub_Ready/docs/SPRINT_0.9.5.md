# Sprint 0.9.5 – Professionelles Mahnwesen

## Fertiggestellt

- Mahnungen als eigenständige Belegdokumente
- automatische Mahnnummern `MA-JAHR-XXXX`
- Auswahl überfälliger Ausgangsrechnungen
- automatische nächste Mahnstufe
- Zahlungserinnerung
- erste Mahnung
- zweite Mahnung
- letzte Mahnung
- frei definierbare Zahlungsfrist
- Mahngebühr
- jährlicher Verzugszinssatz
- taggenaue Verzugszinsberechnung
- festgeschriebene Hauptforderung, Gebühren und Zinsen
- PDF-Mahnschreiben je Mahnstufe
- verschärfter Text bei letzter Mahnung
- SHA-256-Manifest im Belegarchiv
- automatische Aktualisierung der Mahnstufe der Ausgangsrechnung
- eigenes Avalonia-Fenster
- EF-Core-Migration `DunningNotices`
- zusätzliche Domänen- und Berechnungstests

## Berechnung

```text
Verzugszinsen =
offener Rechnungsbetrag × Zinssatz / 100 × überfällige Tage / 365

Gesamtforderung =
Hauptforderung + Mahngebühr + Verzugszinsen
```

## Fachliche Regeln

- Mahnungen können nur für tatsächlich überfällige offene Rechnungen erstellt werden.
- Die Berechnungswerte werden beim Erstellen festgeschrieben.
- Erst das Ausstellen erhöht die Mahnstufe der Ausgangsrechnung.
- Ausgestellte Mahnungen können nicht gelöscht oder storniert werden.
- Eine letzte Mahnung enthält einen deutlichen Hinweis auf mögliche weitere Schritte.
