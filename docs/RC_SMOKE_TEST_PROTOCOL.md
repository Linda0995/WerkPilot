# WerkPilot 0.12.0 RC – Smoke-Test-Protokoll

Testdatum: ____________________

Tester: _______________________

Testrechner / Windows-Version: ______________________________

Release-ZIP SHA256: _________________________________________

## Installation / Start

- [ ] Release-ZIP entpackt
- [ ] PostgreSQL-Verbindung konfiguriert
- [ ] temporäres Admin-Kennwort gesetzt
- [ ] Datenbankmigration erfolgreich
- [ ] WerkPilot startet
- [ ] Login als `admin` funktioniert
- [ ] Kennwortwechsel funktioniert
- [ ] Hauptfenster öffnet ohne Fehler

## Systemdiagnose

- [ ] Version zeigt 0.12.0
- [ ] .NET Runtime wird angezeigt
- [ ] Betriebssystem wird angezeigt
- [ ] Architektur wird angezeigt
- [ ] DB-Konfiguration wird als vorhanden angezeigt
- [ ] Logpfad wird angezeigt
- [ ] keine Geheimnisse werden angezeigt

## Basic-Geschäftsablauf

- [ ] Kunde anlegen
- [ ] Angebot erstellen
- [ ] Angebotsposition erfassen
- [ ] Kalkulation erfassen
- [ ] Angebot annehmen
- [ ] Projekt erzeugen / öffnen
- [ ] Projektaufgabe erfassen
- [ ] Ausgangsrechnung erzeugen
- [ ] Rechnung ausstellen
- [ ] Zahlung erfassen
- [ ] Basic-Prozessprüfung zeigt `Abgeschlossen`
- [ ] Fortschritt = 100 %
- [ ] keine verwaisten Referenzen

## Mahnfall

- [ ] offene überfällige Rechnung vorhanden
- [ ] Mahnung erzeugen
- [ ] Basic-Prozessprüfung zeigt `Mahnung`
- [ ] Kunden-/Rechnungsbezug stimmt

## Bedienung / Fehlerfälle

- [ ] ungültige Eingabe erzeugt verständliche Meldung
- [ ] technischer Fehler zeigt Fehler-ID statt Stacktrace
- [ ] Logdatei enthält technischen Fehler zur Fehler-ID
- [ ] keine Passwörter / Connection-Strings in UI sichtbar
- [ ] Abbruch eines Dialogs beschädigt keine Daten

## Ergebnis

Blocker: _________________________________________________

Major: ___________________________________________________

Minor: ___________________________________________________

Gesamtergebnis:

- [ ] RC bestanden
- [ ] RC nicht bestanden

Freigabe Basic 1.0:

- [ ] Ja
- [ ] Nein
