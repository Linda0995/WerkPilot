# Sprint 0.6.5 – Rabatte und Alternativpositionen

## Fertiggestellt

- prozentualer Gesamtrabatt für Angebotsentwürfe
- Rabattbetrag sowie Netto-, Steuer- und Bruttosumme nach Rabatt
- Alternativpositionen
- Alternativpositionen werden im PDF gekennzeichnet
- Alternativpositionen sind nicht Bestandteil der verbindlichen Angebotssumme
- Bearbeiten und Duplizieren übernimmt den Alternativstatus
- Angebotsduplikate übernehmen den Rabatt
- EF-Core-Migration `OfferDiscountsAndOptions`
- zusätzliche Domänentests

## Fachliche Regeln

- Rabatte liegen zwischen 0 und 100 Prozent.
- Rabatte können nur im Entwurfsstatus geändert werden.
- Alternativpositionen werden angezeigt, aber nicht in Netto, Steuer und Brutto eingerechnet.
- Der Einzelwert einer Alternativposition bleibt sichtbar.
- Kopien übernehmen Rabatt und Alternativkennzeichen.

## Abnahmekriterien

1. Ein Entwurf kann einen Gesamtrabatt erhalten.
2. Summen berücksichtigen den Rabatt kaufmännisch gerundet.
3. Alternativpositionen sind eindeutig gekennzeichnet.
4. Alternativpositionen verändern die verbindliche Angebotssumme nicht.
5. Datenbank und PDF bilden beide Eigenschaften ab.
