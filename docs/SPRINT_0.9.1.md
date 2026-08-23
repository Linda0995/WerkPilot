# Sprint 0.9.1 – Zahlungs- und Liquiditätsmanagement

## Fertiggestellt

- Teilzahlungen für Eingangsrechnungen
- Vollzahlungen
- Zahlungsdatum, Referenz und Benutzer
- bezahlter und offener Rechnungsbetrag
- automatische Statusänderung auf Bezahlt
- Schutz vor Überzahlung
- Skontosatz und Skontofrist
- automatisch berechneter Skontobetrag
- reduzierter Zahlbetrag bei verfügbarem Skonto
- Übersicht offener Eingangsrechnungen
- überfällige Rechnungen
- Liquiditätsbedarf für 7, 14 und 30 Tage
- gesamtes verfügbares Skontopotenzial
- eigenes Avalonia-Fenster Liquiditätsvorschau
- erweiterter CSV-Export mit Zahlungshistorie
- EF-Core-Migration `SupplierInvoicePayments`
- zusätzliche Domänen- und DTO-Tests

## Fachliche Regeln

- Zahlungen sind erst nach Rechnungsfreigabe zulässig.
- Teilzahlungen reduzieren den offenen Betrag.
- Eine vollständige Zahlung setzt den Status automatisch auf Bezahlt.
- Der Zahlungsbetrag darf den offenen Betrag nicht überschreiten.
- Skonto wird nur bis einschließlich Skontofrist als verfügbar ausgewiesen.
- Überfälligkeit beginnt am Tag nach der Rechnungsfälligkeit.
