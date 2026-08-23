# Sprint 0.12.6 - Real Build Gate Fix

Der reale Windows-Test hat erfolgreich bestätigt:

- Docker/WSL funktioniert.
- alle PowerShell-Skripte bestehen die Parserprüfung.
- der First-Build-Runner startet korrekt.

Die verbleibenden zwei Blocker lagen ausschließlich in `verify-source.ps1`.
Dort wurden fälschlicherweise mehrere Skripte gemeinsam mit `Get-Content -Raw`
eingelesen. Diese Prüfungen wurden jetzt auf einzelne Dateien umgestellt.

Behoben:

- RC-Pipeline-Prüfung
- SHA-256-Releaseprüfung

Im Gesamtpaket ist die korrigierte Datei bereits vollständig integriert.
Manuelles Ersetzen einzelner Dateien ist nicht mehr nötig.

Nächster Build:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\first-rc-build.ps1
```
