# Materialimport

## Ablageort

```text
Dokumente/WerkPilot/ImportExport/material_import.csv
```

Beim ersten Klick auf „CSV importieren“ erstellt WerkPilot eine Beispielvorlage und
öffnet sie im Standardprogramm. Nach dem Ausfüllen wird dieselbe Datei erneut importiert.

## Verhalten

- Neue Artikelnummer: neuer Materialartikel
- Vorhandene Artikelnummer: Stammdaten und Preis werden aktualisiert
- Fehlerhafte Zeilen: werden übersprungen und gemeldet
- Gültige Zeilen: werden trotzdem verarbeitet

## Preisaktualität

Ein Einkaufspreis wird nach 90 Tagen als „Preis alt?“ markiert. Dies ist eine Warnung,
keine automatische Preisänderung.
