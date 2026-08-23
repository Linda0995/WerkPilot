# Sprint 0.6.9 – Persistente Bestelllisten

## Fertiggestellt

- dauerhafte Bestelllisten pro Angebot
- automatische Bestelllistennummern `BL-JJJJ-NNNN`
- Übernahme gruppierter Materialpositionen aus der Angebotskalkulation
- Lieferant, Artikelnummer, Menge, Einheit und aktueller Einkaufspreis
- Bestellstatus je Position
- manuelles Abhaken für Telefon-, E-Mail- und Portalbestellungen
- Zeitstempel für erledigte Bestellpositionen
- frei editierbare Bestellnotizen
- Status Entwurf, In Bearbeitung, Abgeschlossen und Storniert
- automatische Statusberechnung anhand der erledigten Positionen
- CSV-Export für Einkauf und Ablage
- eigenes Avalonia-Bestelllistenfenster
- EF-Core-Migration `PersistentPurchaseLists`
- Audit-Einträge für Erstellung und Statusänderungen
- zusätzliche Domänen- und CSV-Tests

## Fachliche Regeln

- Pro Angebot wird höchstens eine persistente Bestellliste erzeugt.
- Gleiche Materialartikel sind bereits vor der Persistierung mengenmäßig gruppiert.
- Ein erneuter Erzeugungsversuch öffnet die vorhandene Bestellliste.
- Das Abhaken kann rückgängig gemacht werden.
- Eine Position kann eine Notiz wie „telefonisch bestellt“ oder eine Auftragsnummer tragen.
- Sind alle Positionen erledigt, wechselt die Bestellliste automatisch auf „Abgeschlossen“.

## Abnahmekriterien

1. Eine Kalkulation kann als dauerhafte Bestellliste gespeichert werden.
2. Bestellpositionen können einzeln abgehakt und wieder geöffnet werden.
3. Bestellnotizen bleiben nach dem Neustart erhalten.
4. Status und Fortschritt werden automatisch berechnet.
5. Die vollständige Liste kann als CSV exportiert werden.
