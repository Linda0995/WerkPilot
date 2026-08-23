# Sprint 0.10.6 – Aufgabenübergabe und Vertretung

## Fertiggestellt

- Einzel-Neuzuweisung für Kunden-Wiedervorlagen
- Einzel-Neuzuweisung für Projektaufgaben
- Übergabegrund als Pflichtinformation
- Audit-Eintrag mit altem und neuem Verantwortlichen
- Sammelübergabe aller offenen Aufgaben zwischen zwei aktiven Benutzern
- getrennte Auswahl für Kunden-Wiedervorlagen und Projektaufgaben
- abgeschlossene Aufgaben werden nicht übertragen
- Fälligkeit, Priorität, Notizen und Status bleiben unverändert
- neue Ansicht `Aufgabenübergabe`
- zusätzlicher Hauptmenüpunkt
- zusätzliche Domänen- und Ergebnis-Tests
- keine neue Datenbankmigration erforderlich

## Typische Verwendung

- Urlaub
- Krankenstand
- Personalwechsel
- Kapazitätsausgleich
- Projektvertretung

## Audit

Kunden-Wiedervorlagen:

```text
EntityType = CustomerFollowUp
Action = Reassigned
```

Projektaufgaben:

```text
EntityType = Project
Action = TaskReassigned
```
