param(
    [string]$PublishPath = "artifacts/publish/win-x64",
    [string]$ReleasePath = "artifacts/release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not (Test-Path $PublishPath)) {
    throw "Publish-Verzeichnis fehlt: $PublishPath"
}

New-Item -ItemType Directory -Force -Path $ReleasePath | Out-Null

$version = "0.12.24-rc"
$packageRoot = Join-Path $ReleasePath "WerkPilot-$version-win-x64"

if (Test-Path $packageRoot) {
    Remove-Item $packageRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null

Copy-Item "$PublishPath\*" $packageRoot -Recurse -Force

$docs = @(
    "README.md",
    "CHANGELOG.md",
    "docs/BASIC_1.0_RELEASE_GATE.md",
    "docs/BASIC_WORKFLOW_ACCEPTANCE_TEST.md",
    "docs/ERROR_HANDLING_AND_SUPPORT.md",
    "docs/LOCAL_BUILD_AND_RELEASE.md",
    "docs/RELEASE_CANDIDATE_0.12.0.md",
    "docs/RC_SMOKE_TEST_PROTOCOL.md"
)

$docsTarget = Join-Path $packageRoot "docs"
New-Item -ItemType Directory -Force -Path $docsTarget | Out-Null

foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Copy-Item $doc $docsTarget -Force
    }
}

$checksums = @()
Get-ChildItem $packageRoot -Recurse -File |
    Sort-Object FullName |
    ForEach-Object {
        $relative = $_.FullName.Substring(
            $packageRoot.Length + 1)

        $hash = Get-FileHash $_.FullName -Algorithm SHA256

        $checksums += "$($hash.Hash)  $relative"
    }

$checksums |
    Set-Content `
        (Join-Path $packageRoot "SHA256SUMS.txt") `
        -Encoding UTF8

$zipPath = Join-Path $ReleasePath "WerkPilot-$version-win-x64.zip"

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive `
    -Path "$packageRoot\*" `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal

$zipHash = Get-FileHash $zipPath -Algorithm SHA256

$manifest = @{
    product = "WerkPilot"
    version = $version
    runtime = "win-x64"
    selfContained = $true
    createdAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    zipFile = (Split-Path $zipPath -Leaf)
    sha256 = $zipHash.Hash
}

$manifest |
    ConvertTo-Json |
    Set-Content `
        (Join-Path $ReleasePath "release-manifest.json") `
        -Encoding UTF8

Write-Host "Release-Paket erstellt: $zipPath" -ForegroundColor Green
Write-Host "SHA256: $($zipHash.Hash)" -ForegroundColor Green
