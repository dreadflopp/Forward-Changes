param(
    [string]$OutputPath = "docs/record-handler-coverage.md",
    [string]$JsonOutputPath = "docs/record-handler-coverage.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$toolProject = Join-Path $repoRoot "tools/RecordHandlerCoverage/RecordHandlerCoverage.csproj"
$overridesPath = Join-Path $repoRoot "tools/RecordHandlerCoverage/coverage-overrides.json"
$markdownPath = Join-Path $repoRoot $OutputPath
$jsonPath = Join-Path $repoRoot $JsonOutputPath

Push-Location $repoRoot
try {
    dotnet run --project $toolProject --configuration Release -- $repoRoot $markdownPath $jsonPath $overridesPath
    if ($LASTEXITCODE -ne 0) {
        throw "Record handler coverage audit failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
