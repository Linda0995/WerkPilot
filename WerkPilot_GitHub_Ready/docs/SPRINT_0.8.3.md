# Sprint 0.8.3 – Nachkalkulation und Deckungsbeitrag

## Fertiggestellt

- Netto-Verkaufspreis aus dem Ursprungsangebot
- geplante Kosten aus der Angebotskalkulation
- tatsächliche Projektkosten aus dem Kostencontrolling
- geplanter Deckungsbeitrag
- aktueller Deckungsbeitrag
- geplante und aktuelle Marge
- Ergebnisabweichung zwischen Plan und Ist
- Status Profitabel, Niedrige Marge, Verlust und Kein Verkaufspreis
- Warnung bei einer Marge unter 10 Prozent
- Integration in das bestehende Projektkostenfenster
- eigener Application-Service `ProjectProfitabilityService`
- zusätzliche Unit-Tests

## Berechnung

```text
Geplanter Deckungsbeitrag = Netto-Verkaufspreis - Soll-Kosten
Aktueller Deckungsbeitrag = Netto-Verkaufspreis - Ist-Kosten
Marge in Prozent = Deckungsbeitrag / Netto-Verkaufspreis × 100
Ergebnisabweichung = aktueller Deckungsbeitrag - geplanter Deckungsbeitrag
```

## Fachliche Regeln

- Der Verkaufspreis stammt aus dem angenommenen Ursprungsangebot.
- Ist-Kosten werden nicht in das Angebot zurückgeschrieben.
- Eine negative aktuelle Deckung führt zum Status Verlust.
- Eine positive Marge unter 10 Prozent wird als niedrige Marge markiert.
- Für diesen Sprint ist keine neue Datenbankmigration erforderlich.
