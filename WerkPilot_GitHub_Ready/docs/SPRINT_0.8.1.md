# Sprint 0.8.1 – Soll-/Ist-Zeitcontrolling

## Fertiggestellt

- Verknüpfung von Angebotskalkulation und Projektzeiterfassung
- kalkulierte Arbeitsstunden als Soll-Zeitbudget
- erfasste Projektstunden als Ist-Wert
- verbleibende Stunden
- positive beziehungsweise negative Stundenabweichung
- prozentualer Budgetverbrauch
- kalkulierte Arbeitskosten
- geschätzte Ist-Arbeitskosten zum Kalkulationsstundensatz
- Status Kein Budget, Im Budget, Warnung und Überschritten
- Warnschwelle bei 85 Prozent
- Überschreitung oberhalb von 100 Prozent
- Darstellung direkt im Zeiterfassungsfenster
- eigener Application-Service `ProjectTimeControllingService`
- zusätzliche Unit-Tests

## Berechnungslogik

```text
Soll-Stunden = Summe der Mengen aller Kalkulationspositionen vom Typ Arbeitszeit
Soll-Arbeitskosten = Summe der Arbeitszeit-Positionskosten
Kalkulationsstundensatz = Soll-Arbeitskosten / Soll-Stunden
Ist-Arbeitskosten = erfasste Stunden × Kalkulationsstundensatz
Budgetverbrauch = Ist-Stunden / Soll-Stunden × 100
Abweichung = Ist-Stunden - Soll-Stunden
```

## Fachliche Regeln

- Nur Projekte mit einem Ursprungsangebot können automatisch ein Zeitbudget erhalten.
- Fehlt eine Angebotskalkulation oder eine Arbeitszeitposition, wird „Kein Zeitbudget“ angezeigt.
- Laufende Zeiteinträge fließen mit ihrer aktuellen Laufzeit in die Ist-Stunden ein.
- Die Auswertung verändert weder Kalkulation noch Zeiteinträge.
