[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$solutionPath = Join-Path $repositoryRoot 'Forward Changes.sln'
$appProjectPath = Join-Path $repositoryRoot 'ForwardChanges.App\ForwardChanges.App.csproj'
$testProjectPath = Join-Path $repositoryRoot 'ForwardChanges.Tests\ForwardChanges.Tests.csproj'
$publishDirectory = Join-Path $repositoryRoot 'artifacts\ForwardChanges-win-x64'

function Resolve-SafeGeneratedPath {
    param([Parameter(Mandatory)][string]$Path)

    $resolvedPath = [System.IO.Path]::GetFullPath($Path)
    $repositoryPrefix = $repositoryRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedPath.StartsWith($repositoryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean a path outside the repository: $resolvedPath"
    }

    return $resolvedPath
}

function Invoke-DotNet {
    param([Parameter(Mandatory)][string[]]$Arguments)

    Write-Host "dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet exited with code $LASTEXITCODE."
    }
}

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Solution not found: $solutionPath"
}

$generatedDirectories = @(
    (Join-Path $repositoryRoot 'ForwardChanges\bin'),
    (Join-Path $repositoryRoot 'ForwardChanges\obj'),
    (Join-Path $repositoryRoot 'ForwardChanges.App\bin'),
    (Join-Path $repositoryRoot 'ForwardChanges.App\obj'),
    (Join-Path $repositoryRoot 'ForwardChanges.Tests\bin'),
    (Join-Path $repositoryRoot 'ForwardChanges.Tests\obj'),
    $publishDirectory
)

Write-Host 'Cleaning generated output...' -ForegroundColor Yellow
foreach ($directory in $generatedDirectories) {
    $safeDirectory = Resolve-SafeGeneratedPath -Path $directory
    if (Test-Path -LiteralPath $safeDirectory) {
        Remove-Item -LiteralPath $safeDirectory -Recurse -Force
    }
}

Invoke-DotNet -Arguments @('restore', $solutionPath)
Invoke-DotNet -Arguments @('build', $solutionPath, '-c', 'Release', '--no-restore')
Invoke-DotNet -Arguments @('test', $testProjectPath, '-c', 'Release', '--no-build', '--no-restore')
Invoke-DotNet -Arguments @(
    'publish',
    $appProjectPath,
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    '--no-restore',
    '-o', $publishDirectory
)

$executablePath = Join-Path $publishDirectory 'ForwardChanges.exe'
if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
    throw "Publish completed without producing the expected executable: $executablePath"
}

$executable = Get-Item -LiteralPath $executablePath
Write-Host ''
Write-Host 'Standalone build completed successfully.' -ForegroundColor Green
Write-Host "Executable: $($executable.FullName)"
Write-Host ("Size: {0:N1} MB" -f ($executable.Length / 1MB))
