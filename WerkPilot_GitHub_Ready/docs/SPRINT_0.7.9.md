# Sprint 0.7.9 – Kundenübersicht 360°

## Fertiggestellt

- zentrale Gesamtansicht pro Kunde
- Kundenstammdaten als Ausgangspunkt
- alle Angebote des Kunden
- alle Projekte des Kunden
- vollständige CRM-Kontakthistorie
- offene Wiedervorlagen
- direkte und projektbezogene Dokumente
- offenes Netto-Angebotsvolumen
- Anzahl aktiver Projekte
- Anzahl offener Wiedervorlagen
- letzter Kontakt
- Dokumente direkt aus der Übersicht öffnen
- eigenes Avalonia-Fenster
- eigener Application-Service `Customer360Service`
- zusätzliche Unit-Tests

## Fachliche Regeln

- Offenes Angebotsvolumen berücksichtigt Entwürfe und gesendete Angebote.
- Aktive Projekte umfassen geplante, aktive und angehaltene Projekte.
- Offene Wiedervorlagen sind nicht erledigte CRM-Einträge mit Wiedervorlagedatum.
- Dokumente werden aus der Kundenakte und allen zugehörigen Projektakten zusammengeführt.
- Die 360°-Ansicht erzeugt keine redundanten Daten, sondern aggregiert bestehende Module.
