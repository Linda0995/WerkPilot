# Sprint 0.9.7 – E-Mail-Vorlagen und Versandwarteschlange

## Fertiggestellt

- wiederverwendbare E-Mail-Vorlagen je Belegart
- Standardvorlage je Ausgangsrechnung, Gutschrift und Mahnung
- bearbeitbare Betreff- und Textvorlagen
- Platzhalter für Belegnummer, Kundenname, Firmenname und Anhang
- sofortige Wiederholung fehlgeschlagener Sendungen
- planbarer nächster Wiederholungszeitpunkt
- Anzahl der Versandversuche
- Zeitpunkt des letzten Versandversuchs
- automatisch vorgeschlagener Wiederholungszeitpunkt nach Fehlern
- erweiterte Versandwarteschlange im Avalonia-Fenster
- EF-Core-Migration `DocumentEmailTemplatesAndRetry`
- zusätzliche Vorlagen- und Wiederholungstests
- Korrektur ungültiger C#-Zeilenumbrüche in den Nachrichtenvorschlägen

## Platzhalter

```text
{{Belegnummer}}
{{Kundenname}}
{{Firmenname}}
{{Anhang}}
```

## Wiederholungslogik

Ein fehlgeschlagener Versand erhält automatisch einen neuen Vorschlagszeitpunkt.
Der Abstand wächst mit der Zahl der Versuche, maximal bis 60 Minuten.
