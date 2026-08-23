# Sprint 0.12.1 – First Build Runner

## Ziel

Der erste echte WerkPilot-RC-Build soll auf einem Windows-PC mit einem einzigen
Befehl reproduzierbar ausführbar sein.

## Neu

- `first-rc-build.ps1`
- vollständiges Transcript des Buildlaufs
- Maschinen-/Umgebungszusammenfassung als JSON
- automatisches Diagnosepaket bei Erfolg oder Fehler
- `.NET --info` im Diagnosepaket
- Docker-/Compose-Version im Diagnosepaket
- letzte WerkPilot-Logs im Diagnosepaket
- Release-/Source-Audit im Diagnosepaket
- `show-first-build-result.ps1`
- keine neue Datenbankmigration erforderlich

## Erster echter Build

Auf einem Windows-PC im entpackten WerkPilot-Quellordner:

```powershell
.\scripts\first-rc-build.ps1
```

Bei einem Fehler wird automatisch ein ZIP erzeugt:

```text
artifacts/first-build/<Zeitstempel>-diagnostic.zip
```

Dieses ZIP enthält die relevanten Buildinformationen, ohne dass die
Fehlersuche manuell zusammengesammelt werden muss.

## Ergebnis anzeigen

```powershell
.\scripts\show-first-build-result.ps1
```
