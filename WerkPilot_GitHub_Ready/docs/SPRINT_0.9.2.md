# Sprint 0.9.2 – Ausgangsrechnungen und Forderungsmanagement

## Fertiggestellt

- Ausgangsrechnung aus Angebot
- Ausgangsrechnung aus Projekt mit Ursprungsangebot
- automatische Rechnungsnummern `RE-JAHR-XXXX`
- Rechnungsdatum und Fälligkeit
- Netto-, Umsatzsteuer- und Bruttobeträge
- Rechnung ausstellen
- Teil- und Vollzahlungen
- offener Rechnungsbetrag
- Status Entwurf, Ausgestellt, Teilbezahlt, Bezahlt und Storniert
- Überfälligkeitsprüfung
- Mahnstufen Erinnerung, erste, zweite und letzte Mahnung
- Datum der letzten Mahnung
- Forderungsübersicht
- offene und überfällige Beträge
- Fälligkeiten innerhalb 7, 14 und 30 Tagen
- CSV-Export mit Positionen und Zahlungshistorie
- eigenes Avalonia-Fenster Ausgangsrechnungen
- eigenes Avalonia-Fenster Forderungsmanagement
- EF-Core-Migration `CustomerInvoices`
- zusätzliche Domänen-, Export- und DTO-Tests

## Fachliche Regeln

- Rechnungen können nur aus Angeboten oder Projekten mit Ursprungsangebot erzeugt werden.
- Nur Entwürfe können ausgestellt werden.
- Zahlungen sind erst nach dem Ausstellen zulässig.
- Teilzahlungen setzen den Status auf Teilbezahlt.
- Vollzahlungen setzen den Status auf Bezahlt.
- Mahnstufen können nur für überfällige offene Rechnungen erhöht werden.
- Bezahlte Rechnungen können nicht storniert werden.
