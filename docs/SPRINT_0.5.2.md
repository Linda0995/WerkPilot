# Sprint 0.5.2 – CRM-Arbeitsbereich

## Fertiggestellt

- Kundendetailbereich direkt neben der Kundenliste
- Bearbeitung von Name, Ansprechpartner, Telefon und E-Mail
- Bearbeitung der Rechnungsadresse
- Bearbeitung von UID/ATU und Notizen
- Anzeige logisch gelöschter Kunden
- Wiederherstellung aus dem Papierkorb
- zustandsabhängige Befehle für Löschen und Wiederherstellen
- Erhalt der Auswahl nach Aktualisierungen
- versioniertes SQL-Schemascript
- zusätzliche Domänen- und Servicetests

## Technische Entscheidung

Die lokale Entwicklungsdatenbank wird in dieser Version noch über `EnsureCreated`
initialisiert. Das beiliegende Schema-Versionsscript bereitet die kontrollierte Umstellung
auf fortlaufende EF-Core-Migrationen vor.
