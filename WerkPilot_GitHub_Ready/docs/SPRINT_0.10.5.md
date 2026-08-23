# Sprint 0.10.5 – Eindeutige Benutzerzuordnung für Projektaufgaben

## Fertiggestellt

- `AssignedUserId` für Projektaufgaben
- Anzeigename `AssignedTo` bleibt für Lesbarkeit und Altbestände erhalten
- Benutzer-Dropdown im Projektfenster
- nur aktive WerkPilot-Benutzer auswählbar
- `Meine Arbeit` verwendet bei Projektaufgaben bevorzugt die Benutzer-ID
- `Team-Arbeit` verwendet bei Projektaufgaben bevorzugt die Benutzer-ID
- Fallback auf bisherigen Namensvergleich für Altbestände
- Datenmigration versucht bestehende Namenszuordnungen auf Benutzer-IDs zu übertragen
- Index auf `project_tasks.AssignedUserId`
- zusätzliche Domänentests
- EF-Core-Migration `ProjectTaskUserAssignments`

## Zuordnung

```text
AssignedUserId -> stabile technische Zuordnung
AssignedTo     -> lesbarer Anzeigename und Legacy-Fallback
```

Damit bleiben Aufgaben auch dann korrekt einem Benutzer zugeordnet, wenn dessen
Anzeigename später geändert wird.
