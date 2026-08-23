# Sprint 0.7.5 – Globale Suche

## Fertiggestellt

- zentrale Suche über Kunden, Angebote, Projekte, Material und Projektdokumente
- Auto-Suche nach 300 Millisekunden Eingabepause
- Mindestlänge von zwei Zeichen
- typisierte Treffer mit Bereich, Nummer, Titel und Zusatzinformation
- Relevanzsortierung für exakte Treffer, Präfixe und Teiltreffer
- Begrenzung auf maximal 60 sichtbare Treffer
- eigenes Avalonia-Suchfenster
- Doppelklick und Schaltfläche zum Öffnen eines Treffers
- Navigation in den passenden WerkPilot-Bereich
- zentrale Application-Komponente `GlobalSearchService`
- zusätzliche Modelltests

## Suchfelder

- Kunden: Kundennummer, Name, E-Mail und Ort
- Angebote: Angebotsnummer, Titel und Status
- Projekte: Projektnummer, Titel, Projektleitung und Status
- Material: Artikelnummer, Beschreibung, Lieferant und Lieferantenartikelnummer
- Dokumente: Anzeigename, Dateityp und zugehöriges Projekt

## Abnahmekriterien

1. Ab zwei Zeichen startet automatisch eine Suche.
2. Treffer verschiedener Module werden gemeinsam angezeigt.
3. Exakte Nummerntreffer stehen vor allgemeinen Teiltreffern.
4. Ein Treffer kann den passenden Fachbereich öffnen.
5. Die Suchlogik ist unabhängig von Avalonia wiederverwendbar.
