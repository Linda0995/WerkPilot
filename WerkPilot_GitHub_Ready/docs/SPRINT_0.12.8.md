# Sprint 0.12.8 - Real C# Build Fix II

Der reale Windows-Build hat die nächste zusammenhängende Compilerfehlergruppe
sichtbar gemacht.

## Behoben

### Kundenvalidierung
- veraltete `request.Street`-Referenz auf `BillingStreet` umgestellt
- veraltete `request.PostalCode`-Referenz auf `BillingPostalCode` umgestellt
- veraltete `request.City`-Referenz auf `BillingCity` umgestellt
- `ValidateNewCustomer` an die vollständige `UpdateCustomerRequest`-Signatur
  angepasst

### Material / Lager
- `MaterialItem.IsPriceOutdated(int)` wieder als Domain-Methode eingeführt
- negative Altersgrenzen werden validiert
- Regressionstests für frische Preise und ungültige Altersgrenzen ergänzt

### Build-Pipeline
- `build.ps1` prüft nun Restore-, Build- und Test-Exitcodes
- bei Fehlern wird sofort abgebrochen
- die falsche Erfolgsmeldung `WerkPilot 0.11.1 wurde erfolgreich gebaut und getestet`
  wurde entfernt
- Erfolg wird nur noch nach tatsächlich erfolgreichen Unit-Tests gemeldet

## Nächster realer Lauf

Nach dem Entpacken zunächst Windows-Dateiblock entfernen:

```powershell
Get-ChildItem -Recurse -File | Unblock-File
```

Danach:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\first-rc-build.ps1"
```
