# Sprint 0.11.1 – Build-Härtung und Compilerbereinigung

## Ziel

Keine neuen Fachmodule. Die fortgeführte WerkPilot-Codebasis wird gezielt auf
Compiler- und Releaseblocker geprüft.

## Behobene Buildblocker

- `App.axaml.cs`: fehlendes `using WerkPilot.Application.Work`
  - `MyWorkService`
  - `TeamWorkService`
  - `WorkReassignmentService`
- `NotificationService`: fehlendes `using WerkPilot.Domain.Identity`
  - `UserAbsenceStatus`
- `GlobalSearchWindow`: ViewModel-Namespace explizit importiert

## Build-Gate erweitert

`verify-source.ps1` prüft jetzt zusätzlich XML-/AXAML-Syntax, x:Class/Code-behind,
Solution-Projektpfade, doppelte EF-Migration-IDs, bekannte Namespace-Regressionen,
veraltete `ProjectTask`-Signaturen, Default-Geheimnisse und alte Runtime-Werte.

`build.ps1` führt dieses Gate automatisch vor Restore, Build und Tests aus.

## Noch offen

Ein echter .NET-9-Compilerlauf ist in der aktuellen Ausführungsumgebung weiterhin
nicht möglich, weil dort kein `dotnet` installiert ist.
