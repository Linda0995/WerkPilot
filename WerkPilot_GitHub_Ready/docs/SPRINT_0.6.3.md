# Sprint 0.6.3 – Angebotsversand per E-Mail

## Fertiggestellt

- SMTP-basierter Angebotsversand
- PDF-Anhang wird unmittelbar vor dem Versand erzeugt
- Empfänger aus Kundenstammdaten
- editierbarer Betreff und Nachrichtentext
- persistente Firmenvorlage für Betreff und Nachricht
- Platzhalterauflösung
- SMTP-Secrets ausschließlich über Umgebungsvariablen
- Statusänderung auf „Gesendet“ erst nach erfolgreichem Versand
- Audit-Eintrag mit Empfängeradresse
- EF-Core-Migration `OfferEmailTemplate`
- zusätzliche Tests für E-Mail-Vorlagen

## Transaktionsregel

Ein fehlgeschlagener SMTP-Versand markiert das Angebot nicht als gesendet. Erst nach
erfolgreicher Übergabe an den SMTP-Server wird der Angebotsstatus geändert und der
Audit-Eintrag geschrieben.

## Abnahmekriterien

1. Eine E-Mail-Vorschau wird aus Firmenvorlage, Angebot und Kunde erzeugt.
2. Empfänger, Betreff und Text können vor dem Versand bearbeitet werden.
3. Das aktuelle Angebots-PDF wird automatisch angehängt.
4. Zugangsdaten stehen nicht in Code oder Datenbank.
5. Ein erfolgreicher Versand wird nachvollziehbar protokolliert.
6. Bereits gesendete oder abgeschlossene Angebote werden nicht erneut versendet.
