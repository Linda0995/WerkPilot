# Sprint 0.8.2 – Projektkosten-Controlling

## Fertiggestellt

- Soll-/Ist-Vergleich der Projektkosten
- Kostenarten Material, Arbeitszeit, Fremdleistung und Gemeinkosten
- Arbeitszeit-Ist-Kosten automatisch aus der Zeiterfassung
- manuelle Ist-Kostenbuchungen für Material, Fremdleistung und Gemeinkosten
- Rechnungs-, Lieferschein- oder Referenzfeld
- Kostenbuchungsdatum
- Soll-Kosten aus der Angebotskalkulation
- Ist-Gesamtkosten, Abweichung und Restbudget
- prozentualer Budgetverbrauch
- Frühwarnstatus ab 85 Prozent
- eigenes Avalonia-Fenster
- EF-Core-Migration `ProjectCostControlling`
- zusätzliche Unit-Tests

## Fachliche Regeln

- Arbeitszeit wird nicht doppelt manuell gebucht, sondern aus der Zeiterfassung berechnet.
- Material, Fremdleistung und Gemeinkosten werden projektbezogen als Netto-Ist-Kosten erfasst.
- Fehlt eine Angebotskalkulation, zeigt WerkPilot „Kein Kostenbudget“.
- Ist-Kosten verändern die Angebotskalkulation nicht.
