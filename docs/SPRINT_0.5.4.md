# Sprint 0.5.4 – CRM-Komplettierung

## Fertiggestellt

- getrennte Rechnungs- und Lieferadresse
- Option „Lieferadresse entspricht Rechnungsadresse“
- auswählbares Steuerprofil
- mehrere Ansprechpartner je Kunde
- Hauptansprechpartner setzen
- Ansprechpartner entfernen
- Übernahme des Hauptansprechpartners in die Kundenstammdaten
- zusätzliche Suchindizes für Kundenname und UID/ATU
- zweite fortlaufende EF-Core-Migration
- zusätzliche Domänen- und Validierungstests

## Abnahmekriterien

1. Ein Kunde kann eine eigenständige Lieferadresse besitzen.
2. Das Steuerprofil kann im Kundenformular ausgewählt und gespeichert werden.
3. Mehrere Ansprechpartner können angelegt werden.
4. Genau ein Ansprechpartner kann als Hauptkontakt markiert werden.
5. Kundenname und UID/ATU sind datenbankseitig indiziert.
