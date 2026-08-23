# Sprint 0.12.0 – Basic Release Candidate

## Ziel

Erste releasefähige Paketstruktur für WerkPilot Basic vorbereiten.

## Umgesetzt

- Systemdiagnose im Hauptmenü
- Anzeige von Produktversion, Informationsversion, .NET Runtime, Betriebssystem,
  Architektur und Environment
- Diagnose von DB-Konfiguration, Admin-Bootstrap und Demo-Modus ohne Geheimnisse
- Anzeige von Programm- und Logverzeichnis
- Windows-Publish prüft auf tatsächlich erzeugte EXE
- Release-Candidate-Pipeline
- Release-ZIP mit Dokumentation
- SHA-256-Dateiprüfsummen
- SHA-256-Prüfsumme des Release-ZIPs
- maschinenlesbares `release-manifest.json`
- manuelles RC-Smoke-Test-Protokoll
- keine neue Datenbankmigration erforderlich

## Noch nicht behauptet

0.12.0 ist vorbereitet als Release Candidate, aber in der aktuellen
Ausführungsumgebung noch nicht tatsächlich kompiliert, veröffentlicht oder auf
Windows gestartet worden. Diese Gates müssen auf einem System mit .NET 9,
Docker/PostgreSQL und Windows ausgeführt werden.
