# Sprint 0.12.14 - Avalonia ComboBox Watermark Fix

Der reale Windows-Build 0.12.13 hat 26 AVLN2000-Fehler gezeigt. Alle hatten dieselbe Ursache:
`ComboBox`-Elemente verwendeten das in Avalonia nicht unterstützte Attribut `Watermark`.

Die Bereinigung wurde global über alle `.axaml`-Dateien durchgeführt.

Betroffen waren 16 View-Dateien mit insgesamt 26 `ComboBox Watermark`-Attributen.
Alle diese Attribute wurden entfernt; die Bindings und Auswahlfunktionen der ComboBoxen
bleiben unverändert.

Zusätzlich prüft `verify-source.ps1` künftig, dass kein `ComboBox` mehr ein
`Watermark`-Attribut enthält.
