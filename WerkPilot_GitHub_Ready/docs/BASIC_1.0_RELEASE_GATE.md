# WerkPilot Basic 1.0 – Release Gate

Basic 1.0 gilt erst als freigabefähig, wenn alle Punkte erfüllt sind.

## Gate A – Build

- `dotnet restore` erfolgreich
- Release-Build ohne Compilerfehler
- Warnungen sind Fehler (`TreatWarningsAsErrors=true`)
- alle Unit-Tests erfolgreich

## Gate B – Datenbank

- PostgreSQL startet reproduzierbar
- alle EF-Core-Migrationen laufen auf leerer Datenbank
- bestehende Testdatenbank lässt sich auf aktuellen Stand migrieren
- Anwendung startet nach Migration

## Gate C – Kernabläufe

- Anmeldung
- Kunden anlegen und ändern
- Angebot erstellen
- Kalkulation
- Angebot als PDF
- Projekt aus Angebot / Projektverwaltung
- Projektaufgaben
- Material und Bestellliste
- Ausgangsrechnung
- Zahlung / Forderungsstatus
- Gutschrift
- Mahnwesen
- Belegversand
- Wiedervorlagen / Meine Arbeit
- Dokumentenakte

## Gate D – Bedienung und Sicherheit

- keine blockierenden UI-Fehler
- keine unverständlichen technischen Fehlermeldungen im Normalbetrieb
- Benutzerrechte geprüft
- keine SMTP-/DB-Passwörter im Quellcode
- Audit-Trail für kritische Änderungen
- Daten bleiben beim Lizenz-/Programmwechsel erhalten

## Gate E – Release

- Windows-x64-Publish erfolgreich
- sauberes Testsystem installiert
- Smoke-Test durchgeführt
- Versionsnummer sichtbar
- Release-ZIP archiviert
- Quellcode-ZIP archiviert
- Changelog und Bedienungsanleitung aktuell
