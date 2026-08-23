# Sprint 0.6.7 – Materialstamm

## Fertiggestellt

- zentraler Materialstamm
- Artikelnummer, Beschreibung und Einheit
- Einkaufspreis
- Lieferant und Lieferanten-Artikelnummer
- Preisaktualisierungszeitpunkt
- Aktivieren und Deaktivieren von Artikeln
- Suche nach Artikelnummer, Beschreibung und Lieferant
- Übernahme eines Materialartikels in eine Angebotskalkulation
- Einkaufspreis wird als Kalkulations-Einstandspreis übernommen
- eigenes Avalonia-Materialfenster
- PostgreSQL-Persistenz
- EF-Core-Migration `MaterialMaster`
- zusätzliche Domänentests

## Fachliche Regeln

- Artikelnummern sind eindeutig.
- Inaktive Artikel bleiben historisch erhalten, können aber nicht neu übernommen werden.
- Änderungen am Materialstamm verändern bereits gespeicherte Kalkulationspositionen nicht rückwirkend.
- Der bei der Übernahme aktuelle Einkaufspreis wird in die Kalkulation kopiert.

## Abnahmekriterien

1. Materialartikel können angelegt, geändert und deaktiviert werden.
2. Die Suche berücksichtigt Lieferanteninformationen.
3. Material kann mit Menge in eine Kalkulation übernommen werden.
4. Der Einkaufspreis wird als interner Einstandspreis verwendet.
5. Bestehende Kalkulationen bleiben trotz späterer Preisänderungen nachvollziehbar.
