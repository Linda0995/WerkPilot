# Sprint 0.12.7 - CS0108 Build Fix

The first real `dotnet build` exposed four CS0108 warnings that are promoted to
errors by the repository build policy.

Affected members:

- `DocumentFile.MoveToTrash()`
- `DocumentFile.Restore()`
- `DocumentFolder.MoveToTrash()`
- `DocumentFolder.Restore()`

The derived members now explicitly use `new`, documenting the intended member
hiding and eliminating CS0108.

A regression check was added to `verify-source.ps1` so these four signatures
cannot silently regress.

Next real build:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\first-rc-build.ps1
```
