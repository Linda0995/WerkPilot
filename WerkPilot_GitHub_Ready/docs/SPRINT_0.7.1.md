# Sprint 0.7.1 – Digitale Projektakte

## Fertiggestellt

- persistente Dokumentordner
- Zuordnung zu Projekten
- Unterordnerstruktur über ParentFolderId
- lokaler Dateispeicher unter Dokumente/WerkPilot/Dateiablage
- Dateimetadaten in PostgreSQL
- Import bestehender Dateien
- frei wählbarer Anzeigename
- Öffnen mit dem Standardprogramm des Betriebssystems
- Dateien und Ordner in den Papierkorb verschieben
- Dateien und Ordner wiederherstellen
- keine endgültige Löschung in der Oberfläche
- eigenes Avalonia-Fenster für die Projektakte
- EF-Core-Migration `DigitalFileCabinet`
- Audit-Einträge bei Ordneranlage und Dateiimport
- zusätzliche Domänentests

## Speicherprinzip

Die Binärdateien liegen im lokalen WerkPilot-Dateispeicher. PostgreSQL speichert nur
Metadaten wie Anzeigename, relativen Pfad, Dateityp, Größe und Zuordnung.

## Fachliche Regeln

- Dateien werden beim Import kopiert, nicht nur verknüpft.
- Der gespeicherte physische Dateiname ist eine GUID und kollisionsfrei.
- Der sichtbare Anzeigename kann unabhängig vom physischen Dateinamen geändert werden.
- Papierkorb-Einträge bleiben erhalten und können wiederhergestellt werden.
- Endgültiges Löschen ist in dieser Version nicht vorgesehen.

## Abnahmekriterien

1. Ein Projekt kann Ordner und Dateien enthalten.
2. Dateien werden physisch in die WerkPilot-Ablage kopiert.
3. Metadaten werden in PostgreSQL gespeichert.
4. Dateien können geöffnet, in den Papierkorb verschoben und wiederhergestellt werden.
5. Ordner können ebenfalls in den Papierkorb verschoben und wiederhergestellt werden.
