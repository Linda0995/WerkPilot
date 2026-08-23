# Sprint 0.11.3 – Basic-Geschäftsablauf schließen

## Ziel

Die bereits vorhandenen Module werden als ein zusammenhängender Basic-Prozess
behandelt:

```text
Kunde
  -> Angebot
  -> Kalkulation
  -> Angebot angenommen
  -> Projekt
  -> Rechnung
  -> Zahlung
  -> optional Mahnung bei Überfälligkeit
```

## Neu

- `BasicWorkflowAuditService`
- Workflow-Prüfung pro Angebot
- Prüfung der Kalkulation über `OfferId`
- Prüfung Projektbezug über `SourceOfferId`
- Prüfung Rechnungsbezug über `SourceOfferId` / `SourceProjectId`
- Prüfung Zahlung über `PaidAmount`
- Prüfung Mahnungen über `CustomerInvoiceId`
- Erkennung von Kundenabweichungen
- Erkennung verwaister Projekt-, Rechnungs- und Mahnreferenzen
- Fortschrittswert pro Geschäftsablauf
- neue Oberfläche `Basic-Prozessprüfung`
- Filter `Nur Probleme`
- Unit-Tests für Workflow-Phasen
- technisches Smoke-Test-Skript

## Release-Bedeutung

Ab 0.11.3 wird nicht mehr nur geprüft, ob einzelne Module existieren. WerkPilot
kann selbst anzeigen, ob seine Kernmodule fachlich zu einem konsistenten
Geschäftsprozess verbunden sind.
