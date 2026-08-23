# PDF-Belege und Belegarchiv

WerkPilot erzeugt druckfähige PDF-Belege mit QuestPDF.

Zu jedem PDF wird ein JSON-Manifest erzeugt. Es enthält:

- Belegart
- Belegnummer
- PDF-Dateiname
- SHA-256-Prüfsumme
- Archivzeitpunkt in UTC

Eine nachträgliche Veränderung der PDF-Datei kann über die Prüfsumme erkannt werden.
