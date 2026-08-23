# E-Mail-Konfiguration

WerkPilot 0.11.0 versendet Angebots-E-Mails über SMTP. Zugangsdaten werden bewusst
nicht in der Datenbank und nicht im Quellcode gespeichert.

## Erforderliche Umgebungsvariablen

```text
WERKPILOT_SMTP_HOST=smtp.example.com
WERKPILOT_SMTP_PORT=587
WERKPILOT_SMTP_SSL=true
WERKPILOT_SMTP_USERNAME=office@example.com
WERKPILOT_SMTP_PASSWORD=[über Secret Store setzen]
WERKPILOT_SMTP_FROM=office@example.com
WERKPILOT_SMTP_FROM_NAME=Firmenname
```

`WERKPILOT_SMTP_FROM` und `WERKPILOT_SMTP_FROM_NAME` sind optional. Ohne FROM-Adresse
wird der SMTP-Benutzername verwendet.

## Sicherheitsregeln

- SMTP-Kennwörter gehören niemals in `appsettings.json`.
- Für Microsoft 365, Gmail und vergleichbare Anbieter sind deren aktuelle
  Sicherheitsvorgaben und gegebenenfalls App-Kennwörter zu beachten.
- Produktivbetrieb sollte Secrets über das Betriebssystem oder einen Secret Store laden.
- Versandfehler verändern den Angebotsstatus nicht.

## Vorlagenplatzhalter

- `{OfferNumber}`
- `{OfferTitle}`
- `{CompanyName}`
- `{CustomerName}`
- `{ContactPerson}`
- `{ValidUntil}`
- `{GrossTotal}`
