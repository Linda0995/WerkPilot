# Basic 1.0 – Geschäftsablauf-Abnahmetest

## Happy Path

1. Kunden anlegen.
2. Angebot für den Kunden erstellen.
3. Mindestens eine Angebotsposition anlegen.
4. Kalkulation öffnen und Kostenpositionen erfassen.
5. Firmenziel / Gewinnziel setzen.
6. Angebot versenden bzw. Status `Sent`.
7. Angebot annehmen.
8. Projekt aus angenommenem Angebot erzeugen.
9. Projektaufgabe anlegen.
10. Ausgangsrechnung aus Angebot oder Projekt erzeugen.
11. Rechnung ausstellen.
12. Zahlung vollständig erfassen.
13. `Basic-Prozessprüfung` öffnen.

Erwartung:

- Kalkulation = vorhanden
- Projekt = vorhanden
- Rechnung = vorhanden
- Zahlung = vorhanden
- Phase = `Abgeschlossen`
- Fortschritt = 100 %
- kein Problemhinweis
- keine verwaiste Referenz

## Mahnfall

1. Rechnung ausstellen.
2. Fälligkeit überschreiten lassen bzw. Testdatum entsprechend wählen.
3. Zahlung offen lassen.
4. Mahnung erstellen und ausstellen.
5. Basic-Prozessprüfung aktualisieren.

Erwartung:

- Mahnung = vorhanden
- Phase = `Mahnung`
- Rechnungs- und Kundenbezug konsistent

## Fehlerfälle, die automatisch erkannt werden

- angenommenes Angebot ohne Kalkulation
- angenommenes Angebot ohne Projekt
- Projekt mit anderem Kunden als Angebot
- Rechnung mit anderem Kunden als Angebot
- Rechnung ohne gültigen Angebots-/Projektbezug
- überfällige offene Rechnung ohne Mahnung
- Projekt mit ungültigem `SourceOfferId`
- Rechnung mit ungültigem `SourceOfferId`
- Rechnung mit ungültigem `SourceProjectId`
- Mahnung mit ungültiger `CustomerInvoiceId`
