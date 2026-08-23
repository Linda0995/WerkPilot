# Sprint 0.8.7 – Geführte Inventur

## Fertiggestellt

- Inventurläufe mit automatischer Nummer
- Inventur über alle Lagerorte oder einen einzelnen Lagerort
- Sollbestand zum Zeitpunkt der Inventuranlage
- Zählfreigabe
- gezählter Bestand je Artikel
- Zählnotiz, Benutzer und Zeitstempel
- automatische Soll-/Ist-Differenz
- Fortschritt mit offenen und gezählten Positionen
- Freigabe zur Buchung erst bei vollständiger Zählung
- automatische Bestandskorrektur bei Buchung
- unveränderliche Korrekturbuchungen in der Lagerhistorie
- Stornierung nicht gebuchter Inventuren
- CSV-Export
- eigenes Avalonia-Fenster
- EF-Core-Migration `InventoryCounting`
- zusätzliche Domänen- und Exporttests

## Fachliche Regeln

- Der Sollbestand wird beim Anlegen der Inventur eingefroren.
- Nur gestartete Inventuren können gezählt werden.
- Erst vollständig gezählte Inventuren können gebucht werden.
- Differenzen werden als Bestandskorrektur Plus oder Minus gebucht.
- Gebuchte Inventuren können nicht mehr storniert werden.
- Eine Inventur ohne Lagerpositionen kann nicht gestartet werden.
