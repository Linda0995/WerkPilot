# WerkPilot 0.13.2 – gebündelter UI-/Bedien-Sprint

Basis: WerkPilot 0.13.1 UI Refresh (letzter grüner Stand).

Umgesetzt:
- Single-Window-Grundprinzip bleibt erhalten.
- Petrol/Weiß-Shell mit klarer linker Navigation.
- Dashboard oben, Einstellungen unten.
- Projekt-Zeiterfassung bleibt aus der Navigation entfernt.
- Kundenverlauf statt CRM-Kontaktjournal.
- Kalkulation & Angebote sowie Finanzen bleiben als sichtbare Hauptbereiche.
- Globale Suche ist ein echtes Eingabefeld im Kopfbereich und öffnet kein eigenes Fenster.
- ENTER startet die Suche; Treffer erscheinen als Overlay über der bestehenden Seite.
- Suchleiste und Trefferliste sind als eigene, typisierte Avalonia UserControls umgesetzt, damit ihre Bindings nicht auf MainWindowViewModel aufgelöst werden.
- Schnellzugriff bleibt direkt bei der Suche.
- Belege: Klick in den leeren manuellen Pfad bzw. „Durchsuchen …“ öffnet den nativen Dateidialog.
- Dateipfad und Dateiname werden übernommen.
- Dateiauswahl verwendet das aktive TopLevel und funktioniert damit auch bei eingebetteter Single-Window-Seite.
- Bestehende 0.13.1-Funktionen wie Kennwort anzeigen, deutsche Steuerprofile und deutsche Abwesenheitsarten bleiben erhalten.

Nicht als fertig ausgegeben:
- Drag & Drop für Belege.
- Externe UID-Firmendatenabfrage.
- UVA-Fachlogik.
- RZL/BMD-Exportspezifikationen.
Diese Punkte benötigen eigene fachlich getestete Implementierung.

Qualität:
- Neuaufbau ausschließlich auf 0.13.1.
- Alle XAML-Dateien werden vor Paketierung als XML geparst.
- Bindings der globalen Suche liegen nicht mehr im MainWindow-x:DataType.
- Keine Datenbankmigrationen oder Docker-Konfigurationen verändert.
