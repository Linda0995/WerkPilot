# Sprint 0.12.11 - Test/API and EF Core Alignment

The real Windows build of 0.12.10 exposed seven build errors and two warnings.

Fixed:

- Customer360DtoTests updated to the current CustomerDto constructor
- OfferDocumentDataTests updated to current OfferDetailsDto totals
- DashboardModelTests updated to current DashboardDto counters/lists
- ProjectTaskTests updated to current ProjectTask constructor
- TeamWorkSummaryTests no longer references the removed UserRole.User
- App now explicitly derives from Avalonia.Application
- Microsoft.EntityFrameworkCore.Relational is explicitly aligned to 9.0.8
  for the test and desktop projects

The EF Core change removes the 9.0.1 / 9.0.8 assembly-reference conflict seen
in the real build.

Next Windows run:

```powershell
Get-ChildItem -Recurse -File | Unblock-File
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\first-rc-build.ps1"
```
