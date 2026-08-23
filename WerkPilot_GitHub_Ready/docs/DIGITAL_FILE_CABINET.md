# Digitale Projektakte

## Speicherort

```text
Dokumente/WerkPilot/Dateiablage
```

Dateien werden nach Jahr und Monat abgelegt. Der physische Dateiname besteht aus einer
GUID. Dadurch entstehen keine Konflikte bei identischen Originaldateinamen.

## Datenbankmetadaten

- Anzeigename
- gespeicherter Dateiname
- relativer Pfad
- Dateityp
- Dateigröße
- Projektzuordnung
- Ordnerzuordnung
- Importzeitpunkt
- Papierkorbstatus

## Papierkorb

WerkPilot löscht Dateien und Ordner nicht endgültig. Das Verschieben in den Papierkorb
setzt nur den Löschstatus. Die physischen Dateien bleiben unverändert erhalten.

## Aktuelle Grenze

Drag-and-Drop und ein nativer Dateiauswahldialog sind für einen weiteren UI-Sprint
vorgesehen. Version 0.11.0 importiert Dateien über ihren vollständigen Pfad.
