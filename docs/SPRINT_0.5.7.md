# Sprint 0.5.7 – Anmeldung und Sicherheit

## Fertiggestellt

- separates Login-Fenster vor dem Hauptprogramm
- PBKDF2-SHA256-Passwort-Hashing mit zufälligem Salt und 210.000 Iterationen
- zeitkonstanter Hashvergleich
- fünf zulässige Fehlversuche
- automatische Sperre für 15 Minuten
- persistenter letzter Login und Sperrstatus
- Sitzungsstatus für den angemeldeten Benutzer
- zentrale Rollenprüfungen als Authorization Service
- Audit-Einträge für erfolgreiche und fehlgeschlagene Anmeldungen
- EF-Core-Migration `AuthenticationSecurity`
- Unit-Tests für Hashing und Sperrlogik

## Erststart

- Benutzername: `admin`
- vorläufiges Kennwort: `WerkPilot!2026`
- Kennwortwechsel ist als verpflichtend markiert

Das Erststartkennwort ist ausschließlich für lokale Entwicklung und Tests vorgesehen.
