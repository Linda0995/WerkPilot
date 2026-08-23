# Sprint 0.8.9 – Lieferantenbestellungen und Wareneingang

## Fertiggestellt

- Lieferantenbestellungen aus Nachbestellvorschlägen
- automatische Bestellnummern `BE-JAHR-XXXX`
- Gruppierung nach Lieferant
- Bestellpositionen mit Menge und Einkaufspreis
- Bestellentwurf
- Status Bestellt, Teilgeliefert, Geliefert und Storniert
- erwarteter Liefertermin
- Lieferantenreferenz
- Teilwareneingänge
- vollständige Wareneingänge
- automatische Lagererhöhung beim Wareneingang
- automatisches Anlegen eines Lagerartikels bei fehlendem Lagerdatensatz
- Wareneingangsreferenz oder Lieferschein
- CSV-Export
- eigenes Avalonia-Fenster
- EF-Core-Migration `SupplierOrders`
- zusätzliche Domänen- und Exporttests

## Fachliche Regeln

- Bestellungen werden aus aktuellen Nachbestellvorschlägen eines Lieferanten erzeugt.
- Nur Bestellentwürfe können als bestellt markiert werden.
- Wareneingänge sind erst nach dem Bestellen zulässig.
- Die Wareneingangsmenge darf die offene Bestellmenge nicht überschreiten.
- Teilwareneingänge halten die Bestellung offen.
- Vollständige Wareneingänge setzen den Status auf Geliefert.
- Teilweise oder vollständig gelieferte Bestellungen können nicht storniert werden.
