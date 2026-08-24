# WerkPilot 0.13.0 – Pilot-Sprint

## In diesem Sprint umgesetzt
- Kompakte, gruppierte linke Navigation mit Hauptpunkten und deutschen Bezeichnungen.
- Linke Navigation heller, klar abgegrenzt, schwarze Schrift, Hauptpunkte fett, ohne Schatten.
- Globale Suche und Schnellzugriff dauerhaft oben rechts; aus der linken Navigation entfernt.
- Single-Window-Navigation: bestehende Fachmasken werden in die WerkPilot-Hauptoberfläche eingebettet statt als separate Fenster geöffnet.
- Projekt-Zeiterfassung aus der Navigation entfernt (technisch für spätere optionale Aktivierung erhalten).
- CRM-Kontaktjournal in der Oberfläche zu „Kundenverlauf“ umbenannt.
- Kalkulation als eigener sichtbarer Kernpunkt unter „Kalkulation & Angebote“.
- Rechnungswesen unter „Finanzen“ zusammengefasst: Eingangsrechnungen/Ausgaben, Ausgangsrechnungen/Einnahmen, Forderungen, Gutschriften, Mahnwesen, Liquidität und Belege.
- „Kennwort anzeigen“ im Login sowie beim Kennwortwechsel.
- Enter bestätigt Login und Kennwortwechsel über Standardbuttons.
- Steuerprofile in der deutschen Oberfläche/Domain verständlich benannt: Inland, EU-Unternehmen mit UID, EU ohne UID, Drittland, Reverse Charge.
- Abwesenheitsarten vollständig deutsch: Urlaub, Krankenstand, Schulung, Dienstreise, Sonstige.

## Fachlicher Ausbau – nächste technische Ausbaustufe innerhalb Finanzen
Die bestehende Codebasis hat bereits Ausgangsrechnungen, Eingangsrechnungen, Zahlungen, Gutschriften, Mahnwesen, Belege/Dokumente und Liquidität. Für eine steuerlich belastbare UVA sowie RZL/BMD-Importe fehlen im aktuellen Datenmodell jedoch noch einzelne Buchungsmerkmale (insbesondere Vorsteuer/USt-Schlüssel bei Eingangsbelegen, igL/igE-Kennzeichen und Kontierung). Diese werden nicht mit erfundenen Exportformaten simuliert. Vor produktivem Export werden die gültigen RZL- und BMD-Importspezifikationen als verbindliche Felddefinition implementiert. Zielausgaben: XLSX und CSV.

## Verbindliche Bedienregeln
- Deutsche Oberfläche verwendet deutsche Begriffe.
- Fachmasken öffnen innerhalb des Hauptfensters; separate Fenster nur für echte Dialoge/Vorschau.
- Globale Suche und Schnellzugriff bleiben auf jeder Hauptseite erreichbar.
- Keine doppelte Dateneingabe zwischen operativen Belegen und Finanzbereich.
