# Sprint 0.12.3 – First Build Parser Fix

Der erste reale Windows-Test hat einen Parserfehler in `first-rc-build.ps1`
aufgedeckt. Version 0.12.3 baut den Runner neu auf, verwendet Windows-PowerShell-
kompatible Syntax und speichert PowerShell-Skripte als UTF-8 mit BOM.

Neu ist `test-powershell-syntax.ps1`. Vor dem eigentlichen Build werden alle
PowerShell-Skripte mit dem eingebauten PowerShell-Parser geprüft.

Ausführen:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\first-rc-build.ps1
```
