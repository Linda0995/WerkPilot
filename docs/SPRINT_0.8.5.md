# Sprint 0.8.5 – Lagerbestand und Inventur

## Fertiggestellt

- Lagerartikel auf Basis des Materialstamms
- eindeutige Zuordnung Materialartikel zu Lagerartikel
- Lagerort und Mindestbestand
- physischer Bestand
- reservierte Menge
- verfügbare Menge
- automatische Mindestbestandswarnung
- Wareneingang und Materialausgabe
- Bestandskorrektur nach oben und unten
- Projektreservierung und Reservierungsfreigabe
- projektbezogene Lagerbewegungen
- Buchungsgrund, Referenz und Benutzer
- vollständige Bewegungshistorie
- eigenes Avalonia-Lagerfenster
- EF-Core-Migration `InventoryFoundation`
- zusätzliche Domänentests

## Fachliche Regeln

- Ein Materialartikel besitzt höchstens einen Lagerdatensatz.
- Verfügbarer Bestand ist physischer Bestand minus reservierter Bestand.
- Reservierungen dürfen den verfügbaren Bestand nicht überschreiten.
- Materialausgaben und negative Korrekturen dürfen den physischen Bestand nicht unterschreiten.
- Mindestbestandswarnungen verwenden den verfügbaren Bestand.
- Jede Bestandsänderung erzeugt eine unveränderliche Lagerbewegung.
