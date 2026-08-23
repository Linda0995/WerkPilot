# Sprint 0.12.12 - Desktop Build Fix

Der reale Windows-Build 0.12.11 hat Domain, Application, Infrastructure und
UnitTests bereits erfolgreich kompiliert. Die verbleibende Fehlergruppe lag im
Desktop-Projekt.

Korrigiert:
- nicht auflösbarer `UseReactiveUI()`-Aufruf aus dem Bootstrap entfernt
- `CalculationViewModel.UpdateItemAsync` an die aktuelle Service-Signatur angepasst
- `Microsoft.Extensions.Configuration` für `GetConnectionString` importiert
- mehrdeutigen `PurchaseListService` in DI explizit auf den Calculation-Service qualifiziert

Hinweis: Die im Build ausgegebenen CS0067-Meldungen sind Warnungen und nicht die
Ursache des Build-Abbruchs; sie werden daher nicht durch semantisch falsche
Dummy-Event-Aufrufe kaschiert.
