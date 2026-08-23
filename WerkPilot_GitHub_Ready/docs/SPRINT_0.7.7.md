# Sprint 0.7.7 – CRM-Kontaktjournal

## Fertiggestellt

- Kontaktjournal pro Kunde
- Kontaktarten Telefon, E-Mail, Besprechung, Besuch und Notiz
- Betreff, ausführliche Notiz und Ansprechpartner
- tatsächlicher Kontaktzeitpunkt
- Ersteller des Journaleintrags
- Wiedervorlagedatum und Verantwortlicher
- Wiedervorlage erledigen und wieder öffnen
- Übersicht offener Wiedervorlagen für die nächsten 30 Tage
- automatische Pflege des letzten Kundenkontakts
- eigenes Avalonia-Fenster
- EF-Core-Migration `CrmContactJournal`
- Audit-Einträge bei neuen Kontakten
- zusätzliche Domänentests

## Fachliche Regeln

- Der letzte Kundenkontakt wird automatisch aus dem neuesten Journaleintrag ermittelt.
- Ältere nachgetragene Kontakte überschreiben keinen neueren Kontaktzeitpunkt.
- Eine Wiedervorlage kann nur erledigt werden, wenn ein Wiedervorlagedatum vorhanden ist.
- Gesprächsnotizen werden nicht auf eine kurze Zeichenanzahl begrenzt; die Datenbank erlaubt bis zu 8000 Zeichen pro Eintrag.
