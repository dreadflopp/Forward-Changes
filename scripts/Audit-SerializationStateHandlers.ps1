param(
    [string]$OutputPath,
    [switch]$FailOnCandidates
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$handlersRoot = Join-Path $repoRoot "ForwardChanges/RecordHandlers"
$decompiledRoot = Join-Path $repoRoot "DecompiledMutagen/Mutagen.Bethesda.Skyrim/Mutagen/Bethesda/Skyrim"
$registrationPattern = '\{\s*"(?<property>[^"]+)"\s*,\s*new\s+(?<handler>[A-Za-z0-9_.]+)'
$candidates = [System.Collections.Generic.List[object]]::new()

function Get-SerializationStateReason {
    param([Parameter(Mandatory)][string]$PropertyPath)

    $leaf = ($PropertyPath -split '\.')[-1]
    if ($leaf -match '^ActsLike\d+$') {
        return "Mutagen compatibility/layout switch; verify the generated binary translator and exclude unless it represents an xEdit field."
    }
    if ($leaf -match 'DataTypeState$') {
        return "Mutagen subrecord-layout discriminator; verify the generated binary translator and exclude unless it represents an xEdit field."
    }
    if ($leaf -eq 'Versioning' -or $leaf -match 'VersioningBreaks$') {
        return "Mutagen versioning/layout state; verify the generated binary translator and exclude unless it represents an xEdit field."
    }

    return $null
}

function Get-BinaryControlFlowEvidence {
    param(
        [Parameter(Mandatory)][string]$RecordName,
        [Parameter(Mandatory)][string]$PropertyPath
    )

    # Dotted paths belong to nested generated types; the explicit naming rules above
    # cover known nested serialization state such as BodyTemplate.ActsLike44.
    if ($PropertyPath.Contains('.')) {
        return $null
    }

    $createPath = Join-Path $decompiledRoot ($RecordName + 'BinaryCreateTranslation.cs')
    $writePath = Join-Path $decompiledRoot ($RecordName + 'BinaryWriteTranslation.cs')
    if (-not (Test-Path -LiteralPath $createPath) -or -not (Test-Path -LiteralPath $writePath)) {
        return $null
    }

    $escapedProperty = [regex]::Escape($PropertyPath)
    $createLines = Get-Content -LiteralPath $createPath
    $writeLines = Get-Content -LiteralPath $writePath
    $parseDerived = @($createLines | Where-Object {
        $_ -match "item\.$escapedProperty\s*(\|=|=)\s*(true|false)"
    })
    $writerControl = @($writeLines | Where-Object {
        $_ -match "if\s*\(.*item\.$escapedProperty|switch\s*\(.*item\.$escapedProperty|item\.$escapedProperty\.HasFlag"
    })
    $writerDirect = @($writeLines | Where-Object {
        $_ -match "(Translation|writer\.Write).*item\.$escapedProperty"
    })

    if ($parseDerived.Count -eq 0 -or $writerControl.Count -eq 0 -or $writerDirect.Count -gt 0) {
        return $null
    }

    return [pscustomobject]@{
        Reason = "Parser-derived value used only to control binary output; verify against xEdit and exclude if it is not independently editable."
        ParserEvidence = $parseDerived[0].Trim()
        WriterEvidence = $writerControl[0].Trim()
    }
}

foreach ($sourcePath in Get-ChildItem -LiteralPath $handlersRoot -Filter '*RecordHandler.cs' -File | Sort-Object Name) {
    $source = Get-Content -LiteralPath $sourcePath.FullName -Raw
    $recordName = $sourcePath.BaseName -replace 'RecordHandler$', ''
    foreach ($match in [regex]::Matches($source, $registrationPattern)) {
        $property = $match.Groups['property'].Value
        $reason = Get-SerializationStateReason -PropertyPath $property
        $binaryEvidence = $null
        if ($null -eq $reason) {
            $binaryEvidence = Get-BinaryControlFlowEvidence -RecordName $recordName -PropertyPath $property
            if ($null -eq $binaryEvidence) {
                continue
            }
            $reason = $binaryEvidence.Reason
        }

        $line = ([regex]::Matches($source.Substring(0, $match.Index), "`n")).Count + 1
        $parserEvidence = if ($null -eq $binaryEvidence) { $null } else { $binaryEvidence.ParserEvidence }
        $writerEvidence = if ($null -eq $binaryEvidence) { $null } else { $binaryEvidence.WriterEvidence }
        $candidates.Add([pscustomobject]@{
            Handler = $sourcePath.Name
            Line = $line
            Property = $property
            HandlerType = $match.Groups['handler'].Value
            Reason = $reason
            ParserEvidence = $parserEvidence
            WriterEvidence = $writerEvidence
        })
    }
}

if ($candidates.Count -eq 0) {
    Write-Host "No registered serialization-state candidates found."
}
else {
    $candidates | Format-Table Handler, Line, Property, HandlerType -AutoSize
    Write-Host "$($candidates.Count) registered serialization-state candidate(s) require review."
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $resolvedOutput = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
        $OutputPath
    }
    else {
        Join-Path $repoRoot $OutputPath
    }
    $outputDirectory = Split-Path -Parent $resolvedOutput
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }
    $json = ConvertTo-Json -InputObject $candidates.ToArray() -Depth 3
    [System.IO.File]::WriteAllText(
        $resolvedOutput,
        $json,
        [System.Text.UTF8Encoding]::new($false))
    Write-Host "JSON report: $resolvedOutput"
}

if ($FailOnCandidates -and $candidates.Count -gt 0) {
    exit 1
}
