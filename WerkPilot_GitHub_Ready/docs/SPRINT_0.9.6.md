# Sprint 0.9.6 – Belegversand und Versandprotokoll

## Fertiggestellt

- gemeinsamer E-Mail-Versand für Ausgangsrechnungen
- E-Mail-Versand für Gutschriften
- E-Mail-Versand für Mahnungen
- automatische Empfängerermittlung aus dem Kundenstamm
- automatisch erzeugte Betreff- und Nachrichtenvorschläge
- bearbeitbare Empfängeradresse, Betreffzeile und Nachricht
- aktueller PDF-Beleg als Anhang
- Versand über die bestehende SMTP-Infrastruktur
- Protokollierung erfolgreicher Versandvorgänge
- Protokollierung fehlgeschlagener Versandvorgänge
- Fehlertext, Empfänger, Betreff, Belegnummer und Anhang im Prüfpfad
- eigenes Avalonia-Fenster `Belegversand`
- EF-Core-Migration `DocumentEmailDispatches`
- zusätzliche Domänen- und DTO-Tests

## Versandfähige Belege

- ausgestellte Ausgangsrechnungen
- ausgestellte oder verrechnete Gutschriften
- ausgestellte Mahnungen

Entwürfe und stornierte Belege werden nicht zur Auswahl angeboten.
