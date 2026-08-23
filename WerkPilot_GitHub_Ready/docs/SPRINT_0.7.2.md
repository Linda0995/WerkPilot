# Sprint 0.7.2 – Komfortable Dateiablage

## Fertiggestellt

- nativer Avalonia-Dateiauswahldialog
- Mehrfachauswahl und Mehrfachimport
- Filter für PDF, Bilder, CAD- und Office-Dateien
- manueller Pfadimport bleibt als Ausweichmöglichkeit erhalten
- Dateien umbenennen
- Ordner umbenennen
- Dateien zwischen Ordnern und Hauptbereich verschieben
- Ordner zwischen Ordnern und Hauptbereich verschieben
- Schutz vor Verschieben in fremde Akten
- Schutz vor Verschieben in Papierkorbordner
- Schutz vor zyklischen Ordnerstrukturen
- zusätzliche Tests für Verschiebevorgänge
- aktualisierte Bedienoberfläche und Dokumentation

## Bedienung

1. Projekt auswählen.
2. Optional einen Zielordner auswählen.
3. „Dateien auswählen …“ betätigen.
4. Eine oder mehrere Dateien markieren.
5. WerkPilot kopiert alle ausgewählten Dateien in die kontrollierte Ablage.

## Fachliche Regeln

- Umbenennen ändert nur den sichtbaren Namen, nicht den physischen Speichernamen.
- Dateien und Ordner können in den Hauptbereich verschoben werden.
- Ein Ordner kann nicht in sich selbst oder einen eigenen Unterordner verschoben werden.
- Ein Zielordner muss zur selben Projektakte gehören.
- In Papierkorbordner kann nicht verschoben werden.
