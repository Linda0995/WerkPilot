# Sprint 0.5.8 – Sitzungen und Zugriffsschutz

## Fertiggestellt

- verpflichtende Kennwortänderung beim Erststart
- Kennwortrichtlinie mit mindestens 12 Zeichen
- Prüfung auf Groß- und Kleinbuchstaben, Zahl und Sonderzeichen
- Prüfung des bisherigen Kennworts
- persistente Speicherung des neuen PBKDF2-Hashes
- Abmeldefunktion und Rückkehr zur Anmeldung
- Anzeige von Benutzer und Rolle im Hauptfenster
- rollenbasierte Sperre schreibender CRM-Aktionen
- zusätzliche Unit-Tests für Kennwortrichtlinie und Rollenprüfung

## Rollenwirkung im CRM

- Administrator, Geschäftsleitung und Vertrieb dürfen Kunden bearbeiten.
- Produktion und Nur-Lesen besitzen im CRM nur Leserechte.
- Nicht angemeldete Sitzungen erhalten keinen Zugriff.

## Abnahmekriterien

1. Ein Erststartkennwort muss vor Nutzung des Hauptfensters geändert werden.
2. Schwache oder nicht übereinstimmende Kennwörter werden abgewiesen.
3. Abmelden beendet die aktive Sitzung.
4. Schreibende CRM-Befehle beachten die Benutzerrolle.
5. Benutzername und Rolle sind in der Oberfläche sichtbar.
