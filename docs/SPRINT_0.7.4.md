# Sprint 0.7.4 – Benachrichtigungscenter

## Fertiggestellt
- zentrale Benachrichtigungen aus Angeboten, Projektaufgaben und Bestelllisten
- Warnung sieben Tage vor Angebotsablauf
- kritischer Hinweis für abgelaufene gesendete Angebote
- Warnung für fällige und überfällige Projektaufgaben
- Hinweis für offene persistente Bestelllisten
- Prioritäten Information, Warnung und Kritisch
- dauerhaft gespeicherter Gelesen-Status pro Benutzer
- einzelne oder alle Hinweise als gelesen markieren
- Anzeige ungelesener Hinweise im Hauptmenü
- eigenes Avalonia-Benachrichtigungsfenster
- EF-Core-Migration `NotificationCenter`
- zusätzliche Domänentests

## Architektur
Benachrichtigungen werden aus den aktuellen Fachdaten berechnet. Nur der Gelesen-Status wird gespeichert. Dadurch entstehen keine veralteten Kopien von Angeboten, Aufgaben oder Bestelllisten.
