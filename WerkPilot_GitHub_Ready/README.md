# WerkPilot 0.12.11 RC

WerkPilot ist eine modular aufgebaute ERP-, CRM-, MES- und KI-Plattform für Handwerksbetriebe und mittelständische Unternehmen.

## Technologie

- C# / .NET 9
- Avalonia UI
- PostgreSQL
- Entity Framework Core
- xUnit
- Docker Compose

## Voraussetzungen

1. .NET 9 SDK
2. Docker Desktop oder eine lokale PostgreSQL-17-Instanz
3. Visual Studio 2022, Rider oder VS Code

## Start

```powershell
docker compose -f deploy/docker-compose.yml up -d
dotnet restore WerkPilot.sln
dotnet build WerkPilot.sln -c Release
dotnet test WerkPilot.sln -c Release
dotnet run --project src/WerkPilot.Desktop/WerkPilot.Desktop.csproj
```

## Funktionsstand

- Avalonia-Desktop-Shell
- PostgreSQL-Anbindung
- Kundenliste und Kundensuche
- Firmenkunden mit automatischer Kundennummer anlegen
- Favoriten, mehrere Kontakte sowie Rechnungs-/Lieferadressen
- Dashboard-Kennzahlen
- Papierkorb-Grundmodell
- Demo-Datensatz
- strukturierte Schichten
- Unit-Tests

## Sicherheit

Das enthaltene PostgreSQL-Passwort ist ausschließlich für die lokale Entwicklungsumgebung bestimmt.
Für Produktivbetrieb müssen Secrets außerhalb des Repositorys verwaltet werden.


## Neu in 0.5.4

Kunden können nun in einer Detailansicht bearbeitet, in den Papierkorb verschoben und
wiederhergestellt werden. Das Datenbankschema besitzt erstmals eine explizite Versionsdatei.


## Neu in 0.5.4

WerkPilot verwendet jetzt echte EF-Core-Migrationen. Der CRM-Workflow validiert
Kundenname, E-Mail-Adresse, Ländercode und Feldlängen vor dem Speichern.


## Neu in 0.5.4

Das CRM-Grundmodul unterstützt jetzt getrennte Rechnungs- und Lieferadressen,
Steuerprofile und mehrere Ansprechpartner mit definierbarem Hauptkontakt.


## Neu in 0.5.5

Das CRM erkennt mögliche Dubletten, durchsucht zusätzlich Kontakt- und Adressdaten
und protokolliert zentrale Kundenänderungen in einem persistenten Audit-Trail.


## Neu in 0.5.6
WerkPilot besitzt jetzt eine persistente Benutzer- und Rollenbasis. Passwortanmeldung und technische Berechtigungsprüfung folgen im nächsten Security-Ausbau.


## Neu in 0.5.7

WerkPilot startet jetzt mit einer sicheren Anmeldung. Der lokale Erststart erfolgt mit
`admin` / `WerkPilot!2026`. Das Kennwort ist als änderungspflichtig markiert.


## Neu in 0.5.8

Das Erststartkennwort muss nun vor der Nutzung geändert werden. WerkPilot unterstützt
Abmelden, zeigt die aktive Rolle an und sperrt schreibende CRM-Aktionen für Benutzer
ohne Bearbeitungsberechtigung.


## Neu in 0.5.9

WerkPilot besitzt jetzt das fachliche Angebotsgrundmodul mit fortlaufenden Nummern,
Positionen, Summenberechnung und kontrolliertem Statusworkflow.


## Neu in 0.6.0

Die Angebotsverwaltung ist erstmals vollständig bedienbar. Angebote können einem Kunden
zugeordnet, mit Positionen befüllt und durch den Statusworkflow geführt werden.


## Neu in 0.6.1

Angebote können als A4-PDF exportiert und im Standard-PDF-Programm als Vorschau
geöffnet werden. Die Dokumentengine ist über eine austauschbare Schnittstelle gekapselt.


## Neu in 0.6.2

Firmenstammdaten und Angebotsvorlagentexte können jetzt in WerkPilot gepflegt werden.
Der PDF-Export verwendet diese Einstellungen automatisch.


## Neu in 0.6.3

Angebote können mit automatisch erzeugtem PDF-Anhang per SMTP versendet werden.
Empfänger, Betreff und Nachricht sind vor dem Versand bearbeitbar; SMTP-Secrets werden
ausschließlich aus Umgebungsvariablen geladen.


## Neu in 0.6.4

Angebotspositionen können jetzt bearbeitet und entfernt werden. Bestehende Angebote
lassen sich als neuer Entwurf duplizieren; überfällige gesendete Angebote werden
automatisch als abgelaufen markiert.


## Neu in 0.6.5

Angebotsentwürfe unterstützen jetzt Gesamtrabatte und Alternativpositionen.
Alternativpositionen erscheinen im PDF, werden aber nicht in die verbindliche
Angebotssumme eingerechnet.


## Neu in 0.6.6

WerkPilot trennt jetzt interne Kosten und Kundenverkaufspreise. Material, Arbeitszeit,
Fremdleistungen und Gemeinkosten werden je Angebot kalkuliert; aus dem Firmenziel
entsteht ein empfohlener Nettoverkaufspreis.


## Neu in 0.6.7

WerkPilot besitzt jetzt einen zentralen Materialstamm. Einkaufsartikel können gepflegt,
gesucht und mit ihrem aktuellen Einkaufspreis direkt in Angebotskalkulationen übernommen
werden.


## Neu in 0.6.8

Materialstammdaten können per CSV importiert und exportiert werden. WerkPilot warnt vor
alten Einkaufspreisen und erzeugt aus verknüpften Kalkulationsmaterialien eine gruppierte
Bestellliste.


## Neu in 0.6.9

Bestelllisten werden jetzt dauerhaft gespeichert. Materialpositionen können nach
Lieferant bearbeitet, manuell abgehakt, mit Bestellnotizen versehen und als CSV für
Einkauf oder Telefonbestellungen exportiert werden.


## Neu in 0.7.0

Angenommene Angebote können jetzt in Projekte überführt werden. WerkPilot verwaltet
Projekttermine, Verantwortliche, Aufgaben, Status und den automatisch berechneten
Fortschritt.


## Neu in 0.7.1

WerkPilot besitzt jetzt eine digitale Projektakte. Dateien werden in eine kontrollierte
Ablage kopiert, Projekten und Ordnern zugeordnet und können über einen Papierkorb
wiederhergestellt werden.


## Neu in 0.7.2

Die digitale Projektakte besitzt jetzt einen nativen Dateiauswahldialog mit
Mehrfachimport. Dateien und Ordner können umbenannt sowie innerhalb der Projektakte
sicher verschoben werden.


## Neu in 0.7.3

Das Hauptfenster zeigt jetzt offene Angebote, aktive Projekte, fällige Aufgaben und aktuelle Vorgänge in einem operativen Dashboard.


## Neu in 0.7.4

WerkPilot bündelt fällige Angebote, Projektaufgaben und offene Bestelllisten in einem benutzerspezifischen Benachrichtigungscenter.


## Neu in 0.7.5

WerkPilot besitzt jetzt eine globale Suche mit Auto-Vorschlägen über Kunden, Angebote, Projekte, Material und Projektdokumente.


## Neu in 0.7.6

Globale Suchtreffer werden benutzerspezifisch als zuletzt verwendet gespeichert und können als Favoriten im Schnellzugriff abgelegt werden.


## Neu in 0.7.7

Kundenkontakte können als strukturiertes Journal mit Gesprächsnotizen und Wiedervorlagen erfasst werden. Der letzte Kontakt wird automatisch beim Kunden gepflegt.


## Neu in 0.7.8

CRM-Wiedervorlagen erscheinen jetzt direkt im operativen Dashboard und im
Benachrichtigungscenter. Überfällige Nachfassaktionen werden kritisch hervorgehoben.


## Neu in 0.7.9

Die Kundenübersicht 360° bündelt Angebote, Projekte, CRM-Kontakte, Wiedervorlagen und
Dokumente eines Kunden in einer einzigen Ansicht.


## Neu in 0.8.0

WerkPilot unterstützt jetzt eine optionale Projekt-Zeiterfassung mit Start/Stopp,
manueller Nachtragung, Aufgabenbezug und Projektstunden-Auswertung.


## Neu in 0.8.1

WerkPilot vergleicht jetzt die kalkulierten Arbeitsstunden eines Angebots mit den
tatsächlich erfassten Projektstunden. Abweichungen und drohende Überschreitungen werden
direkt in der Zeiterfassung sichtbar.


## Neu in 0.8.2

WerkPilot vergleicht nun kalkulierte Projektkosten mit tatsächlichen Material-, Arbeits-, Fremdleistungs- und Gemeinkosten.


## Neu in 0.8.3

Die Nachkalkulation zeigt Netto-Verkaufspreis, tatsächliche Projektkosten, Deckungsbeitrag, Marge und Ergebnisabweichung.


## Neu in 0.8.4

WerkPilot erzeugt nun einen Projektabschlussbericht mit Zeit-, Kosten- und
Ergebnisnachkalkulation sowie einer automatischen Beurteilung der Abschlussbereitschaft.


## Neu in 0.8.5

WerkPilot verwaltet nun Lagerbestand, Mindestbestand, Reservierungen,
Inventurkorrekturen und eine vollständige Bewegungshistorie.


## Neu in 0.8.6

WerkPilot erkennt Fehlmengen aus Lagerbestand, Reservierungen, Mindestbestand und
offenen Bestelllisten und erzeugt daraus einen lieferantenbezogenen
Nachbestellvorschlag.


## Neu in 0.8.7

WerkPilot unterstützt jetzt geführte Inventurläufe mit eingefrorenem Sollbestand,
Zählmengen, Differenzprüfung und automatischer Bestandskorrektur.


## Neu in 0.8.8

WerkPilot bewertet den physischen, reservierten und verfügbaren Lagerbestand mit den
Einkaufspreisen des Materialstamms. Inventurdifferenzen werden zusätzlich wertmäßig
ausgewiesen.


## Neu in 0.8.9

WerkPilot erzeugt Lieferantenbestellungen direkt aus Nachbestellvorschlägen und bucht
Teil- sowie Vollwareneingänge automatisch in den Lagerbestand.


## Neu in 0.9.0

WerkPilot prüft Eingangsrechnungen jetzt gegen Bestellung und Wareneingang. Mengen-,
Preis- und Wertabweichungen werden erkannt, bewertet und vor der Freigabe dokumentiert.


## Neu in 0.9.1

WerkPilot verwaltet Teil- und Vollzahlungen, offene Eingangsrechnungsbeträge,
Skontofristen und eine Liquiditätsvorschau für die nächsten 7, 14 und 30 Tage.


## Neu in 0.9.2

WerkPilot erstellt Ausgangsrechnungen aus Angeboten oder Projekten, verwaltet
Kundenzahlungen, offene Forderungen, Überfälligkeiten und Mahnstufen.


## Neu in 0.9.3

WerkPilot verwaltet Voll- und Teilgutschriften als eigene Belege und reduziert bei
der Verrechnung automatisch den offenen Forderungsbetrag.


## Neu in 0.9.4

WerkPilot erzeugt PDF-Ausgangsrechnungen und PDF-Gutschriften und archiviert jeden
Beleg gemeinsam mit einer SHA-256-Prüfsumme in einem JSON-Manifest.


## Neu in 0.9.5

WerkPilot erstellt Zahlungserinnerungen und Mahnungen mit Mahngebühren,
taggenauen Verzugszinsen, PDF-Ausgabe und revisionsfähigem Belegarchiv.


## Neu in 0.9.6

WerkPilot versendet Ausgangsrechnungen, Gutschriften und Mahnungen als PDF-Anhang
über SMTP und protokolliert erfolgreiche sowie fehlgeschlagene Versandversuche.


## Neu in 0.9.7

WerkPilot verwaltet wiederverwendbare E-Mail-Vorlagen und unterstützt die sofortige
oder terminierte Wiederholung fehlgeschlagener Belegsendungen.


## Neu in 0.9.8

WerkPilot verarbeitet fällige E-Mail-Wiederholungen automatisch im Hintergrund und
bietet eine SMTP-Konfigurations- und Netzwerkdiagnose ohne Testmail-Versand.


## Neu in 0.9.9

WerkPilot bündelt versendete Angebote, Rechnungen, Gutschriften und Mahnungen in
einer chronologischen Kommunikationsakte je Kunde.


## Neu in 0.10.0

WerkPilot verwaltet Kunden-Wiedervorlagen mit Fälligkeit, Priorität,
Verantwortlichem, Status, Überfälligkeitsanzeige und Abschlussnotiz.


## Neu in 0.10.1

Das WerkPilot-Hauptdashboard zeigt nun Kunden-Wiedervorlagen mit Priorität,
Überfälligkeit und Heute-Fälligkeit sowie die jüngste Kundenkommunikation.


## Neu in 0.10.2

Kunden-Wiedervorlagen erscheinen nun automatisch in der WerkPilot-
Benachrichtigungszentrale und werden abhängig von Fälligkeit und Priorität bis
zur kritischen Eskalationsstufe hochgestuft.


## Neu in 0.10.3

Die Ansicht `Meine Arbeit` bündelt persönliche Kunden-Wiedervorlagen und
zugewiesene Projektaufgaben des angemeldeten Benutzers in einer gemeinsamen,
priorisierten Arbeitsliste.


## Neu in 0.10.4

Kunden-Wiedervorlagen werden jetzt eindeutig an aktive WerkPilot-Benutzer
zugewiesen. Die Ansicht `Team-Arbeit` bündelt Kunden- und Projektaufgaben je
Mitarbeiter und macht Überlastung sowie kritische Fälligkeiten sichtbar.


## Neu in 0.10.5

Projektaufgaben besitzen jetzt wie Kunden-Wiedervorlagen eine stabile
`AssignedUserId`. Dadurch arbeiten `Meine Arbeit` und `Team-Arbeit` modulübergreifend
mit eindeutigen Benutzerzuordnungen statt ausschließlich mit Anzeigenamen.


## Neu in 0.10.6

WerkPilot kann offene Kunden-Wiedervorlagen und Projektaufgaben gesammelt von
einem Mitarbeiter an eine Vertretung übergeben. Jeder Verantwortungswechsel wird
mit Übergabegrund im Audit-Trail dokumentiert.


## Neu in 0.10.7

WerkPilot verwaltet Abwesenheiten und Vertretungen, erkennt offene Aufgaben im
Abwesenheitszeitraum und warnt vor fehlender Vertretung. Die eigentliche
Aufgabenübergabe bleibt kontrolliert und nachvollziehbar.


## Neu in 0.10.8

Die Abwesenheitsplanung zeigt jetzt eine konkrete Übergabevorschau. WerkPilot kann
wahlweise nur die während der Abwesenheit fälligen Aufgaben oder alle offenen
Aufgaben kontrolliert an die hinterlegte Vertretung übergeben.


## Neu in 0.11.0 – Basic-1.0-Readiness

Version 0.11.0 ist ein Stabilitäts- und Release-Sprint. Build, Tests,
Datenbankmigration und Windows-Publishing sind voneinander getrennt und über
reproduzierbare Skripte ausführbar. `global.json` fixiert die .NET-9-SDK-Linie,
ein lokales `dotnet-ef`-Toolmanifest fixiert die EF-Core-Werkzeugversion.

Der nächste Meilenstein ist nicht „mehr Module“, sondern ein vollständig
kompilierter, installierbarer und durchgetesteter WerkPilot-Basic-Release.


## Neu in 0.11.1

Der Build-Härtungssprint beseitigt konkrete Namespace-Compilerblocker und erweitert
das statische Release-Gate. `build.ps1` führt die Quellcode-Verifikation nun
automatisch vor Restore, Build und Tests aus.


## Neu in 0.11.2

Der Datenbank- und Erststartpfad ist jetzt reproduzierbarer: PostgreSQL wird auf
Bereitschaft geprüft, Migrationen werden vor dem Update validiert und ein
isolierter Smoke-Test kann alle Migrationen auf einer frischen Datenbank prüfen.
Demo-Daten werden ausschließlich über ein explizites Development-Flag aktiviert.


## Neu in 0.11.3

WerkPilot prüft seinen zentralen Basic-Geschäftsablauf jetzt erstmals
modulübergreifend. Der neue Menüpunkt `Basic-Prozessprüfung` zeigt, ob Angebot,
Kalkulation, Projekt, Rechnung, Zahlung und Mahnwesen fachlich sauber miteinander
verbunden sind und weist auf fehlende oder verwaiste Referenzen hin.


## Neu in 0.11.4

Die Desktop-Oberfläche behandelt unerwartete technische Fehler nun zentral.
Benutzer sehen eine verständliche Meldung mit Fehler-ID; Stacktrace und technische
Details werden für die Diagnose in Serilog protokolliert. Direkte `ex.Message`-
Ausgaben in Statuszeilen sind durch das statische Release-Gate untersagt.


## WerkPilot 0.12.0 RC1

0.12.0 ist der vorbereitete Basic Release Candidate. Die neue
`Systemdiagnose` zeigt die für Support und Abnahme relevanten Runtime-Daten ohne
Kennwörter oder Connection-Strings. `release-candidate.ps1` bündelt Build, Tests,
Datenbank-Smoke-Test, Windows-Publish und Release-Paketierung.


## WerkPilot 0.12.1 RC

Der erste reale Build kann jetzt auf einem geeigneten Windows-PC mit
`first-rc-build.ps1` vollständig ausgeführt werden. Jeder Lauf erzeugt ein
Transcript und automatisch ein Diagnose-ZIP, sodass reale Compiler- oder
Startprobleme anschließend gezielt behoben werden können.


## WerkPilot 0.12.2 RC

Der Release Candidate besitzt jetzt eine eigene Erststart-Prüfung und einen
kontrollierten Testinstallationspfad. Das erzeugte Release-ZIP kann gegen sein
SHA-256-Manifest verifiziert und anschließend in einen sauberen Testordner
entpackt werden.


## WerkPilot 0.12.3 RC

Behebt den beim ersten realen Windows-Test gefundenen Parserfehler und prüft künftig alle PowerShell-Skripte vor dem Build.


## WerkPilot 0.12.4 RC

Fixes the two additional PowerShell parser defects found by the first real Windows parser run.


## WerkPilot 0.12.5 RC

Fixes three false-positive release-gate checks found by the real Windows build run.


## WerkPilot 0.12.6 RC

Enthält die korrigierte Real-Build-Verifikation vollständig integriert. Keine manuelle Dateiersetzung erforderlich.


## WerkPilot 0.12.7 RC

Fixes the four real CS0108 compiler failures found by the first Windows dotnet build.


## WerkPilot 0.12.8 RC

Behebt die nächste im realen Windows-Build gefundene C#-Fehlergruppe in Kundenvalidierung, Material/Lager und Build-Exitcode-Behandlung.


## WerkPilot 0.12.9 RC

Behebt den im realen Build gefundenen OfferDetailsDto.Items/Positions-Compilerfehler in der Ausgangsrechnung.


## WerkPilot 0.12.10 RC

Behebt den real gefundenen `OfferPositionDto.Unit`-Compilerfehler ohne Schemaänderung; für bestehende Angebotspositionen wird bei der Rechnungsübernahme vorerst `Stk.` verwendet.


## WerkPilot 0.12.11 RC

Behebt die sieben im realen 0.12.10-Windows-Build gefundenen Test-/Compilerfehler und vereinheitlicht EF Core Relational auf 9.0.8.


## WerkPilot 0.12.13 RC

Behebt die zwei real bestätigten CS0067-Fehler in den statischen RelayCommand-Implementierungen des Desktop-Projekts.


## WerkPilot 0.12.14 RC

Behebt die 26 real gefundenen Avalonia-AVLN2000-Fehler durch Entfernung nicht unterstützter `Watermark`-Attribute auf `ComboBox`-Controls.


## WerkPilot 0.12.15 RC

Bereinigt die zwei verbleibenden veralteten Unit-Test-Erwartungen aus dem realen 0.12.14-Lauf.


## WerkPilot 0.12.17 RC

Korrigiert die fehlenden PostgreSQL-Provider-Annotationen im EF ModelSnapshot und prüft Pending Model Changes vor dem Datenbank-Smoke-Test explizit.


## WerkPilot 0.12.20 RC

Konsolidiert bei Bedarf die historische Pre-Release-Migrationskette automatisch zu einer sauberen EF-Baseline und validiert den vollständigen Neuaufbau einer leeren PostgreSQL-Datenbank.


## WerkPilot 0.12.21 RC

Korrigiert die abschließende EF-Migrationshistorienprüfung und markiert eine erfolgreich validierte Baseline sauber für die dauerhafte Übernahme in den Release-Quellstand.


## WerkPilot 0.12.22 RC

Ersetzt die letzte fragile EF-History-Abfrage durch robuste PostgreSQL-Katalogprüfungen ohne quoted-Identifier-Probleme.
