# Angebots-PDF und Lizenzkonfiguration

WerkPilot 0.11.0 verwendet QuestPDF als codebasierte PDF-Engine.

## Exportziel

PDF-Dateien werden standardmäßig unter folgendem Ordner gespeichert:

```text
Dokumente/WerkPilot/Exporte/Angebote
```

Die Funktion „PDF-Vorschau“ erzeugt dieselbe PDF-Datei und öffnet sie anschließend
mit dem im Betriebssystem hinterlegten Standardprogramm.

## QuestPDF-Lizenz

Die passende Lizenzart muss vor einer kommerziellen Veröffentlichung rechtlich geprüft
und über die Umgebungsvariable `WERKPILOT_QUESTPDF_LICENSE` konfiguriert werden:

```text
community
professional
enterprise
```

Ohne Konfiguration verwendet die Entwicklungsfassung `community`.

Diese Einstellung ist eine technische Auswahl und ersetzt keine rechtliche Lizenzprüfung.
