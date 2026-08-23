# Lokaler Build und Release

## Voraussetzungen

- Windows 10/11 oder geeignete Entwicklungsumgebung
- .NET 9 SDK
- Docker Desktop / Docker Engine mit Compose
- PowerShell

## 1. Nur Quellcode prüfen

```powershell
.\scripts\verify-source.ps1
```

## 2. Kompilieren und testen

```powershell
.\scripts\build.ps1
```

Docker und PostgreSQL werden dafür nicht benötigt.

## 3. Datenbank starten und migrieren

```powershell
.\scripts\update-database.ps1
```

## 4. Anwendung starten

```powershell
.\scripts\start.ps1
```

## 5. Windows-Paket erstellen

```powershell
.\scripts\publish-win-x64.ps1
```

Das Publish ist `self-contained`; auf dem späteren Testrechner muss deshalb
nicht zwingend separat das .NET Runtime-Paket installiert sein.


## Erstinstallation – Administrator

Vor dem ersten Start muss ein starkes temporäres Kennwort gesetzt werden:

```powershell
$env:WERKPILOT_ADMIN_INITIAL_PASSWORD = "Ein-starkes-temporäres-Kennwort!"
```

WerkPilot legt den Benutzer `admin` nur dann an, wenn noch kein Benutzer existiert.
Das Klartextkennwort wird nicht gespeichert oder protokolliert. Beim ersten Login
ist ein Kennwortwechsel erforderlich.

## Produktionsdatenbank

Für ein Release wird die Verbindung nicht aus einer eingebauten
Entwicklungsverbindung bezogen. Setze:

```powershell
$env:ConnectionStrings__WerkPilot = "Host=...;Database=...;Username=...;Password=..."
```

`appsettings.Development.json` wird nicht in das Windows-Publish kopiert.
