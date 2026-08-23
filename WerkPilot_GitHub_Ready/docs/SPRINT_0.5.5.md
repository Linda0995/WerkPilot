# Sprint 0.5.5 – CRM-Qualitätssicherung

## Fertiggestellt

- Dublettenprüfung für Kundenname, E-Mail und UID/ATU
- blockierende Prüfung bei gleicher E-Mail oder UID/ATU
- erweiterte Suche nach Telefon, E-Mail, Ort, PLZ und Ansprechpartnern
- persistenter Änderungsverlauf für Kunden
- Protokollierung von Anlage, Bearbeitung, Favoritenstatus, Papierkorb und Kontakten
- Anzeige des Änderungsverlaufs in der CRM-Oberfläche
- dritte fortlaufende EF-Core-Migration
- zusätzliche Unit-Tests

## Fachliche Regeln

- Gleiche UID/ATU oder gleiche primäre E-Mail-Adresse blockieren das Speichern.
- Ein gleicher Kundenname wird bei der Neuanlage als mögliche Dublette gemeldet.
- Der Änderungsverlauf wird nicht gemeinsam mit einem Kunden gelöscht.
- Suchabfragen berücksichtigen auch Ansprechpartner und Adressdaten.

## Abnahmekriterien

1. Eine offensichtliche Dublette kann nicht unbemerkt angelegt werden.
2. Kunden sind über Kontakt- und Adressdaten auffindbar.
3. Zentrale CRM-Aktionen erscheinen im Änderungsverlauf.
4. Das Audit-Schema wird über eine EF-Core-Migration angelegt.
