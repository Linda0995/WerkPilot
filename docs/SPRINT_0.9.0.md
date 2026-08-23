# Sprint 0.9.0 – Eingangsrechnungsprüfung

## Fertiggestellt

- Eingangsrechnung aus Lieferantenbestellung erzeugen
- Rechnungsnummer, Rechnungsdatum und Fälligkeit
- Schutz vor doppelten Rechnungsnummern je Lieferant
- Drei-Wege-Abgleich Bestellung, Wareneingang und Rechnung
- Mengenabweichung je Position
- Preisabweichung je Position
- wertmäßige Abweichung
- Prüfstatus Exakt, Warnung und Kritisch
- Preiswarnung ab mehr als 2 Prozent Abweichung
- kritische Abweichung bei verrechneter Menge über Wareneingang
- Rechnungspositionen korrigieren
- Prüfung einreichen
- Freigabe mit bewusster Warnungsbestätigung
- Ablehnung mit Begründung
- Status Freigegeben, Bezahlt und Storniert
- CSV-Prüfprotokoll
- eigenes Avalonia-Fenster
- EF-Core-Migration `SupplierInvoiceMatching`
- zusätzliche Domänen- und Exporttests

## Freigaberegeln

- Kritische Abweichungen blockieren die Freigabe.
- Warnungen müssen ausdrücklich bestätigt werden.
- Nur Rechnungen im Status Prüfung können freigegeben oder abgelehnt werden.
- Nur freigegebene Rechnungen können als bezahlt markiert werden.
- Bezahlte Rechnungen können nicht storniert werden.
