# WerkPilot-Sicherheitsmodell – Stand 0.5.7

## Authentifizierung

Passwörter werden niemals im Klartext gespeichert. WerkPilot nutzt PBKDF2 mit SHA-256,
einem kryptografisch zufälligen 128-Bit-Salt, 210.000 Iterationen und einem 256-Bit-Hash.
Der Vergleich erfolgt zeitkonstant.

## Schutz gegen Passwortversuche

Nach fünf Fehlversuchen wird das Benutzerkonto für 15 Minuten gesperrt. Erfolgreiche und
fehlgeschlagene Anmeldungen werden protokolliert.

## Autorisierung

Zentrale Berechtigungsregeln unterscheiden Administrator, Geschäftsleitung, Vertrieb,
Produktion und Nur-Lesen. Die vollständige Durchsetzung an jedem Modul wird schrittweise
mit den jeweiligen Fachmodulen ergänzt.

## Offene Sicherheitsaufgaben

- Dialog zum verpflichtenden Kennwortwechsel
- sichere Kennwort-Zurücksetzung durch Administratoren
- Sitzungs-Timeout
- optional Mehrfaktor-Authentifizierung
- Verschlüsselung besonders schützenswerter Konfigurationswerte


## Ergänzung 0.5.8

- Erststartkennwörter müssen vor der regulären Programmnutzung geändert werden.
- Kennwörter benötigen mindestens 12 Zeichen, Groß- und Kleinbuchstaben, Zahl und Sonderzeichen.
- Abmelden löscht den aktiven Sitzungskontext.
- CRM-Schreibrechte werden zentral anhand der Rolle geprüft.
