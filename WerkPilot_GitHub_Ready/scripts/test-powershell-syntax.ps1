$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

$Failures = @()
$Scripts = Get-ChildItem "scripts" -File -Filter "*.ps1"

foreach ($Script in $Scripts) {
    $Tokens = $null
    $Errors = $null

    [System.Management.Automation.Language.Parser]::ParseFile(
        $Script.FullName,
        [ref]$Tokens,
        [ref]$Errors
    ) | Out-Null

    if ($Errors.Count -gt 0) {
        foreach ($ErrorItem in $Errors) {
            $Failures += "$($Script.Name): $($ErrorItem.Message) at line $($ErrorItem.Extent.StartLineNumber)"
        }
    }
}

if ($Failures.Count -gt 0) {
    $Failures | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    throw "PowerShell syntax validation failed."
}

Write-Host "PowerShell syntax validation successful for $($Scripts.Count) scripts." -ForegroundColor Green
