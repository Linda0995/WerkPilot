# Sprint 0.10.4 – Benutzerzuordnung und Team-Arbeit

## Fertiggestellt

- Kunden-Wiedervorlagen werden über echte aktive WerkPilot-Benutzer zugewiesen
- `AssignedUserId` wird beim Erstellen und Verschieben gespeichert
- Anzeigename bleibt für Lesbarkeit und Altbestände erhalten
- bestehende ältere Zuordnungen per Namen bleiben kompatibel
- neue Ansicht `Team-Arbeit`
- Auslastung pro aktivem Benutzer
- gemeinsame Kunden- und Projektaufgaben pro Benutzer
- Team-Kennzahlen für offen, heute fällig, überfällig und dringend
- Sortierung nach Überfälligkeit, Dringlichkeit und Arbeitsmenge
- eigener Hauptmenüpunkt
- zusätzliche Team-DTO-Tests
- keine neue Datenbankmigration erforderlich

## Zuordnungsstrategie

Kunden-Wiedervorlagen:

```text
AssignedUserId -> eindeutige Benutzerzuordnung
AssignedTo     -> lesbarer Anzeigename / Kompatibilität
```

Projektaufgaben besitzen weiterhin eine textuelle `AssignedTo`-Zuordnung. Diese
werden in `Team-Arbeit` gegen den aktuellen Anzeigenamen der Benutzer aufgelöst.
