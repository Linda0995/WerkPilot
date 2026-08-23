# Sprint 0.10.0 – Kunden-Wiedervorlagen und Aufgaben

## Fertiggestellt

- Wiedervorlagen pro Kunde
- Titel und ausführliche Notizen
- Fälligkeit
- Prioritäten Niedrig, Normal, Hoch und Dringend
- Status Offen, In Bearbeitung, Abgeschlossen und Storniert
- verantwortliche Person
- optionale Benutzer-ID für spätere Benutzerzuordnung
- automatische Überfälligkeitsprüfung
- Terminverschiebung
- Abschlussnotiz und Abschlusszeitpunkt
- Audit-Einträge bei Erstellung und Abschluss
- eigenes Avalonia-Fenster
- neuer Hauptmenüpunkt `Wiedervorlagen`
- EF-Core-Migration `CustomerFollowUps`
- zusätzliche Domänentests

## Fachliche Regeln

- abgeschlossene und stornierte Aufgaben können nicht verschoben werden
- erfolgreich abgeschlossene Aufgaben sind nicht mehr überfällig
- stornierte Aufgaben können nicht abgeschlossen werden
- jede Wiedervorlage bleibt dauerhaft mit dem Kunden verknüpft
