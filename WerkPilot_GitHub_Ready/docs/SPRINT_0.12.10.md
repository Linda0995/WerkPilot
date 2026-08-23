# Sprint 0.12.10 - Offer Position Unit Build Fix

Der reale Windows-Build von 0.12.9 hat gezeigt, dass `OfferPositionDto` aktuell
keine `Unit`-Eigenschaft besitzt. Das Angebots-Domainmodell führt derzeit
Beschreibung, Menge und Preis, aber keine Einheit.

Für die RC-Stabilisierung wird beim Erzeugen einer Ausgangsrechnung aus einem
bestehenden Angebot deshalb die Legacy-Standardeinheit `Stk.` verwendet.

Diese Lösung:
- beseitigt den aktuellen CS1061-Compilerfehler,
- verändert das Datenbankschema nicht,
- vermeidet eine zusätzliche Migration mitten in der RC-Bereinigung.

Eine echte frei wählbare Einheit pro Angebotsposition ist ein separates
Fachfeature und sollte nach dem stabilen Basic-Build sauber durch Domain, DTO,
UI und Migration geführt werden.

Nächster realer Lauf:

```powershell
Get-ChildItem -Recurse -File | Unblock-File
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\first-rc-build.ps1"
```
