# Sprint 0.12.4 - PowerShell Parser Cleanup

The real Windows parser in 0.12.3 found two additional syntax defects:

- `basic-workflow-smoke-test.ps1`
- `release-candidate.ps1`

Both scripts were rebuilt in conservative Windows PowerShell 5.1-compatible syntax.
The first-build runner remains the entry point:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\first-rc-build.ps1
```
