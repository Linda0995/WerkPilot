# Automatische E-Mail-Warteschlange

## Betriebsverhalten

WerkPilot prüft während der laufenden Desktop-Anwendung jede Minute, ob
fehlgeschlagene Belegsendungen erneut fällig sind.

Ein einzelner SMTP-Fehler stoppt nicht die Verarbeitung der restlichen Warteschlange.

## Manuelle Kontrolle

Im Fenster `Belegversand` stehen zusätzlich zur Verfügung:

- SMTP prüfen
- fällige Sendungen sofort verarbeiten
- Queue-Status
- SMTP-Status

## Sicherheitsregel

Die Diagnose versendet keine Test-E-Mail. Sie prüft Konfiguration und
Netzwerkerreichbarkeit. Dadurch wird kein unbeabsichtigter Beleg- oder Testversand
ausgelöst.
