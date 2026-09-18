param(
    [string]$Repository = "https://github.com/TES5Edit/TES5Edit.git",
    [string]$Revision = "93cc0bc5a1251936c3c7859eee3150eda12a62d7",
    [string]$OutputRoot = "XEditSource"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Split-Path -Parent $PSScriptRoot)).ProviderPath
$outputPath = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputRoot))
$repoPrefix = $repoRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) +
    [System.IO.Path]::DirectorySeparatorChar

if (-not $outputPath.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Output path must remain inside the repository: $outputPath"
}

if (Test-Path $outputPath) {
    if (-not (Test-Path (Join-Path $outputPath ".git"))) {
        throw "Output path exists but is not a Git checkout: $outputPath"
    }

    git -C $outputPath fetch origin $Revision --depth 1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to fetch xEdit revision $Revision."
    }

    git -C $outputPath checkout --detach FETCH_HEAD
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to check out xEdit revision $Revision."
    }
}
else {
    New-Item -ItemType Directory -Path $outputPath | Out-Null
    git -C $outputPath init
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to initialize the xEdit checkout."
    }

    git -C $outputPath remote add origin $Repository
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to configure the xEdit remote."
    }

    git -C $outputPath fetch origin $Revision --depth 1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to fetch xEdit revision $Revision."
    }

    git -C $outputPath checkout --detach FETCH_HEAD
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to check out xEdit revision $Revision."
    }
}

$commit = git -C $outputPath rev-parse HEAD
if ($LASTEXITCODE -ne 0) {
    throw "Failed to determine the checked-out xEdit commit."
}

Write-Host "xEdit source available at $outputPath"
Write-Host "Revision: $Revision ($commit)"
