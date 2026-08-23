# Sprint 0.6.2 – Firmenstammdaten und Angebotsvorlage

## Fertiggestellt

- persistente Firmenstammdaten
- Firmenname, Anschrift, E-Mail, Telefon, UID/ATU und Website
- konfigurierbarer Angebots-Einleitungstext
- konfigurierbarer Angebots-Abschlusstext
- konfigurierbarer ISO-Währungscode
- eigenes Avalonia-Einstellungsfenster
- PDF-Kopf und PDF-Fußzeile verwenden die Firmenstammdaten
- Angebots-PDF verwendet die gespeicherten Vorlagentexte
- EF-Core-Migration `CompanyProfileAndOfferTemplate`
- Audit-Eintrag bei Änderungen
- zusätzliche Domänentests

## Architektur

Firmenstammdaten sind ein eigenständiges Settings-Modul. Der PDF-Exporter erhält die
fertig aufbereiteten Daten über `OfferDocumentData` und greift nicht selbst auf die
Datenbank zu.

## Abnahmekriterien

1. Firmenstammdaten können gespeichert und erneut geladen werden.
2. Angebots-Einleitungs- und Abschlusstext sind frei konfigurierbar.
3. Neue PDFs verwenden die gespeicherten Unternehmensdaten.
4. Änderungen werden im Audit-Trail protokolliert.
5. Das Schema wird über eine fortlaufende EF-Core-Migration erweitert.
