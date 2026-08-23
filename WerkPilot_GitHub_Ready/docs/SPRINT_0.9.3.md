# Sprint 0.9.3 – Gutschriften und Rechnungskorrekturen

## Fertiggestellt

- Gutschriften als eigene Belegdokumente
- automatische Gutschriftsnummern `GS-JAHR-XXXX`
- Verknüpfung mit der Ausgangsrechnung
- Vollgutschrift über den offenen Rechnungsbetrag
- Teilgutschrift für einzelne Rechnungspositionen
- Korrekturgrund
- Netto-, Umsatzsteuer- und Bruttobetrag
- Status Entwurf, Ausgestellt, Verrechnet und Storniert
- Verrechnung mit dem offenen Rechnungsbetrag
- gutgeschriebener Betrag in der Ausgangsrechnung
- automatische Anpassung der Forderungsübersicht
- Schutz vor Überkorrektur
- CSV-Export
- eigenes Avalonia-Fenster
- EF-Core-Migration `CustomerCreditNotes`
- zusätzliche Domänen- und Exporttests

## Fachliche Regeln

- Gutschriften sind nur für ausgestellte, offene Rechnungen zulässig.
- Teilgutschriften dürfen die ursprüngliche Positionsmenge nicht überschreiten.
- Eine Gutschrift muss ausgestellt sein, bevor sie verrechnet werden kann.
- Verrechnete Gutschriften können nicht storniert werden.
- Der Gutschriftsbetrag darf den offenen Rechnungsbetrag nicht überschreiten.
