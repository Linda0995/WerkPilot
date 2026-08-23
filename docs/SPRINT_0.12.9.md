# Sprint 0.12.9 - Offer/Invoice Build Fix

Der reale Windows-Build von 0.12.8 hat einen verbleibenden C#-Fehler sichtbar gemacht:

```text
CustomerInvoiceService.cs: OfferDetailsDto enthält keine Definition für Items
```

`OfferDetailsDto` stellt die Angebotspositionen unter `Positions` bereit.
`CustomerInvoiceService.CreateFromOfferAsync` wurde entsprechend korrigiert.

Zusätzlich verhindert das statische Release-Gate künftig wieder eine
`offer.Items`-Referenz in `CustomerInvoiceService`.

Nächster realer Lauf:

```powershell
Get-ChildItem -Recurse -File | Unblock-File
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\first-rc-build.ps1"
```
