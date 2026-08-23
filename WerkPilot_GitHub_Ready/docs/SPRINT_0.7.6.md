# Sprint 0.7.6 – Schnellzugriff und zuletzt verwendet

## Fertiggestellt

- benutzerspezifische Liste zuletzt geöffneter Datensätze
- Favoriten für Kunden, Angebote, Projekte, Material und Dokumente
- automatische Aktualisierung beim Öffnen eines globalen Suchtreffers
- persistenter Zeitstempel der letzten Verwendung
- eigenes Avalonia-Fenster „Schnellzugriff“
- Weiterleitung in den passenden Fachbereich
- getrennte Ansichten für Favoriten und zuletzt verwendet
- EF-Core-Migration `UserWorkbench`
- zusätzliche Domänentests

## Fachliche Regeln

- Schnellzugriffe sind pro Benutzer getrennt.
- Pro Benutzer, Bereich und Datensatz existiert höchstens ein Eintrag.
- Erneutes Öffnen aktualisiert Titel, Zusatzinformation und Zeitstempel.
- Favoriten bleiben unabhängig von der Sortierung „zuletzt verwendet“ erhalten.
