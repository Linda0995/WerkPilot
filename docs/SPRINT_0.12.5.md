# Sprint 0.12.5 - Release Gate Fix

The real Windows run passed PowerShell syntax validation for all 18 scripts.
The next blocker was three false-positive static release-gate checks.

Fixed:

- system diagnostics secret exposure check
- RC pipeline completeness check
- release SHA-256 verification check

Expected next milestone:

```text
PowerShell syntax validation: PASS
Static source verification: PASS
dotnet restore/build: NEXT
```
