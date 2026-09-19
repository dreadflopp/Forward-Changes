param(
    [string]$AssetsFile = "DreadsMashedPatch/obj/project.assets.json",
    [string]$OutputRoot = "DecompiledMutagen",
    [string]$Framework = "net8.0",
    [string]$DecompilerVersion = "9.1.0.7988"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$toolDirectory = Join-Path $repoRoot ".tools"
$toolPath = Join-Path $toolDirectory "ilspycmd.exe"
$packagesRoot = Join-Path $env:USERPROFILE ".nuget\packages"

Push-Location $repoRoot
try {
    if (-not (Test-Path $toolPath)) {
        New-Item -ItemType Directory -Path $toolDirectory -Force | Out-Null
        dotnet tool install ilspycmd --tool-path $toolDirectory --version $DecompilerVersion | Out-Host
    }

    $assetsPath = (Resolve-Path $AssetsFile).ProviderPath
    $outputPath = Join-Path $repoRoot $OutputRoot

    $assets = Get-Content -Path $assetsPath -Raw | ConvertFrom-Json
    $libraries = @(
        $assets.libraries.PSObject.Properties |
        Where-Object { $_.Value.path -like "mutagen.bethesda*" } |
        Sort-Object Name
    )
    $referenceDirectories = @(
        $libraries |
        ForEach-Object {
            $referenceDirectory = Join-Path (Join-Path $packagesRoot $_.Value.path) (Join-Path "lib" $Framework)
            if (Test-Path $referenceDirectory) {
                $referenceDirectory
            }
        } |
        Sort-Object -Unique
    )

    if ($libraries.Count -eq 0) {
        throw "No Mutagen.Bethesda libraries were found in $AssetsFile. Run a restore/build first."
    }

    if (Test-Path $outputPath) {
        Remove-Item -Path $outputPath -Recurse -Force
    }

    New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

    foreach ($library in $libraries) {
        $packagePath = Join-Path $packagesRoot $library.Value.path
        $dllFiles = @(
            $library.Value.files |
            Where-Object { $_ -like "lib/$Framework/*.dll" -and $_ -notlike "*.resources.dll" }
        )

        foreach ($dllRelativePath in $dllFiles) {
            $dllPath = Join-Path $packagePath $dllRelativePath

            if (-not (Test-Path $dllPath)) {
                throw "Expected assembly not found: $dllPath"
            }

            $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($dllPath)
            $assemblyOutputPath = Join-Path $outputPath $assemblyName
            $decompilerArguments = @(
                "--disable-updatecheck",
                "--nested-directories",
                "-p",
                "-o",
                $assemblyOutputPath
            )

            foreach ($referenceDirectory in $referenceDirectories) {
                $decompilerArguments += @("-r", $referenceDirectory)
            }

            $decompilerArguments += $dllPath

            Write-Host "Decompiling $assemblyName"
            & $toolPath @decompilerArguments | Out-Host

            if ($LASTEXITCODE -ne 0) {
                throw "Decompilation failed for $assemblyName with exit code $LASTEXITCODE."
            }

            if (-not (Test-Path $assemblyOutputPath)) {
                throw "Decompilation produced no output folder for $assemblyName."
            }

            if (-not (Get-ChildItem -Path $assemblyOutputPath -Force | Select-Object -First 1)) {
                throw "Decompilation produced an empty output folder for $assemblyName."
            }
        }
    }
}
finally {
    Pop-Location
}