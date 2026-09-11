<#
.SYNOPSIS
  Extract a compact code digest (namespaces / public types / member signatures /
  XML summaries) for every library project listed in projects.json.

.DESCRIPTION
  Reads the inventory produced by Export-ProjectInventory.ps1 and, for each
  library project, scans the .vb sources that physically live under the project
  directory (obj / bin / test / "My Project" are excluded) and writes one
  Markdown digest per project into the output directory.

  The digest is intentionally small (a few KB per project): it lets a reviewer
  understand what a project actually does without opening thousands of source
  files. Only declarations and XML <summary> comments are captured -- method
  bodies are never emitted.

  An _index.md file is written as well, listing every project grouped by area
  with its source file count, so the digests can be handed out in batches.

.PARAMETER Root
  Repository root. Defaults to three levels above this script.

.PARAMETER InventoryFile
  projects.json produced by Export-ProjectInventory.ps1.

.PARAMETER OutDir
  Digest output directory. Defaults to <script dir>\digest.

.PARAMETER ProjectFilter
  Wildcard filter applied to the project relative path.
#>
[CmdletBinding()]
param(
    [string]$Root = '',
    [string]$InventoryFile = '',
    [string]$OutDir = '',
    [string]$ProjectFilter = '*'
)

$ErrorActionPreference = 'Stop'

# $PSScriptRoot is empty under some hosts (powershell -File from a wrapper),
# so fall back to the invocation path before deriving any defaults.
$ScriptDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($ScriptDir)) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition }
if ([string]::IsNullOrEmpty($Root))          { $Root          = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ScriptDir)) }
if ([string]::IsNullOrEmpty($InventoryFile)) { $InventoryFile = Join-Path $ScriptDir 'projects.json' }
if ([string]::IsNullOrEmpty($OutDir))        { $OutDir        = Join-Path $ScriptDir 'digest' }

$Root = (Resolve-Path $Root).Path.TrimEnd('\')

# ---------------------------------------------------------------------------
# Limits -- keep each digest small enough to be read comfortably
# ---------------------------------------------------------------------------
$MaxFilesListed   = 150
$MaxFilesScanned  = 1200
$MaxTypes         = 120
$MaxMembers       = 60
$MaxImports       = 25
$MaxSummaryLines  = 3
$MaxSignatureLen  = 150

$ExcludeDirPattern = '(^|[\\])(obj|bin|\.git|\.vs|packages|test|tests|My Project)([\\]|$)'

function Read-TextFile([string]$path) {
    # Honour a BOM when present; fall back to the ANSI code page otherwise so
    # that legacy GBK-encoded sources still yield usable text.
    $sr = New-Object System.IO.StreamReader($path, [System.Text.Encoding]::Default, $true)
    try { return $sr.ReadToEnd() }
    finally { $sr.Close() }
}

function Get-Summary([string[]]$lines, [int]$declIndex) {
    # Walk backwards over the XML doc comment block that precedes the declaration.
    $block = New-Object System.Collections.Generic.List[string]
    for ($i = $declIndex - 1; $i -ge 0; $i--) {
        $t = $lines[$i].Trim()
        if ($t -like "'''*") {
            $block.Insert(0, $t)
            if ($block.Count -gt 12) { break }
        }
        else { break }
    }
    if ($block.Count -eq 0) { return '' }

    $text = New-Object System.Collections.Generic.List[string]
    foreach ($l in $block) {
        $c = $l -replace "^'''", ''
        $c = $c.Trim()
        if (-not $c) { continue }
        if ($c -match '^</?summary>' -or $c -match '^</?remarks>' -or $c -match '^</?param' -or $c -match '^</?returns?>' -or $c -match '^</?example>') { continue }
        $text.Add($c)
        if ($text.Count -ge $MaxSummaryLines) { break }
    }
    if ($text.Count -eq 0) { return '' }
    return (($text -join ' ') -replace '\s+', ' ').Trim()
}

function Get-FileFacts([string]$path, [string]$relPath) {
    $text = Read-TextFile $path
    $lines = $text -split "`r`n|`n|`r"

    $namespaces = New-Object System.Collections.Generic.List[string]
    $types      = New-Object System.Collections.Generic.List[object]
    $members    = New-Object System.Collections.Generic.List[string]
    $imports    = New-Object System.Collections.Generic.List[string]

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $t = $line.Trim()

        if ($t -eq '' -or $t.StartsWith("'")) { continue }

        if ($t -match '^Imports\s+(.+)$') {
            $imports.Add($Matches[1].Trim())
            continue
        }

        if ($t -match '^Namespace\s+(.+)$') {
            $namespaces.Add($Matches[1].Trim())
            continue
        }

        if ($t -match '^(?:(Public|Friend|Private|Protected|Protected Friend)\s+)?(?:Partial\s+)?(Module|Class|Structure|Interface|Enum)\s+([A-Za-z_][A-Za-z0-9_]*)') {
            $mod  = $Matches[1]
            $kind = $Matches[2]
            $name = $Matches[3]
            if ($mod -eq 'Private') { continue }
            $types.Add([ordered]@{
                kind    = $kind
                name    = $name
                summary = (Get-Summary $lines $i)
            })
            continue
        }

        if ($t -match '^(?:Public|Friend|Protected|Protected Friend)\s+(?:Shared\s+|Overloads\s+|Overrides\s+|Overridable\s+|MustOverride\s+|ReadOnly\s+|WriteOnly\s+|Default\s+|Async\s+|Iterator\s+)*(Function|Sub|Property|Const|Enum|Delegate|Event|Operator)\s+([A-Za-z_][A-Za-z0-9_]*)') {
            $sig = $t
            if ($sig.Length -gt $MaxSignatureLen) { $sig = $sig.Substring(0, $MaxSignatureLen) + '…' }
            $members.Add($sig)
            continue
        }
    }

    return [ordered]@{
        relPath    = $relPath
        namespaces = $namespaces
        types      = $types
        members    = $members
        imports    = $imports
    }
}

# ---------------------------------------------------------------------------
# Load inventory
# ---------------------------------------------------------------------------
$inventory = Get-Content $InventoryFile -Encoding UTF8 -Raw | ConvertFrom-Json
$targets = @($inventory.projects | Where-Object { $_.isLibrary -and $_.path -like $ProjectFilter })

if (-not (Test-Path -LiteralPath $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }

Write-Host "Repository root : $Root" -ForegroundColor Cyan
Write-Host "Library projects: $($targets.Count)" -ForegroundColor Cyan
Write-Host "Digest output   : $OutDir" -ForegroundColor Cyan
Write-Host ""

$index          = New-Object System.Collections.Generic.List[string]
$totalFiles     = 0
$totalTypes     = 0
$emptyProjects  = New-Object System.Collections.Generic.List[string]

foreach ($p in $targets) {
    $projFull = Join-Path $Root ($p.path -replace '/', '\')
    $projDir  = Split-Path $projFull -Parent

    $allFiles = @(Get-ChildItem -LiteralPath $projDir -Filter '*.vb' -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $rel = $_.FullName.Substring($projDir.Length + 1)
            $rel -notmatch $ExcludeDirPattern
        } | Sort-Object FullName)

    $scanned   = 0
    $namespaces = @{}
    $typeLines  = New-Object System.Collections.Generic.List[string]
    $memberLines = New-Object System.Collections.Generic.List[string]
    $importSet  = @{}

    foreach ($f in $allFiles) {
        if ($scanned -ge $MaxFilesScanned) { break }
        $rel = $f.FullName.Substring($projDir.Length + 1)
        try { $facts = Get-FileFacts $f.FullName $rel }
        catch { Write-Warning "  cannot read $rel : $($_.Exception.Message)"; continue }
        $scanned++

        foreach ($ns in $facts.namespaces) {
            if (-not $namespaces.ContainsKey($ns)) { $namespaces[$ns] = 0 }
            $namespaces[$ns] = $namespaces[$ns] + 1
        }
        foreach ($tp in $facts.types) {
            $suffix = if ($tp.summary) { " - " + $tp.summary } else { '' }
            $typeLines.Add(("- {0} {1} ({2}){3}" -f $tp.kind, $tp.name, $rel, $suffix))
        }
        foreach ($m in $facts.members) { $memberLines.Add("- " + $m) }
        foreach ($im in $facts.imports) { $importSet[$im] = $true }
    }

    if ($scanned -eq 0) { $emptyProjects.Add($p.path) }

    $nsSorted = @($namespaces.GetEnumerator() | Sort-Object { -not $_.Name.StartsWith('Microsoft.VisualBasic') }, Name)
    $importSorted = @($importSet.Keys | Sort-Object)

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine("# $($p.path)")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("- RootNamespace : $($p.rootNamespace)")
    [void]$sb.AppendLine("- AssemblyName  : $($p.assemblyName)")
    [void]$sb.AppendLine("- TargetFramework: $($p.targetFramework)")
    [void]$sb.AppendLine("- Source files  : $($allFiles.Count)")
    [void]$sb.AppendLine("- Existing Title: $($p.snapshot.Title)")
    [void]$sb.AppendLine("- Existing Desc : $($p.snapshot.Description)")
    [void]$sb.AppendLine("- Existing Tags : $($p.snapshot.PackageTags)")
    [void]$sb.AppendLine("")

    [void]$sb.AppendLine("## Namespaces")
    if ($nsSorted.Count -eq 0) {
        [void]$sb.AppendLine("- (no explicit `Namespace` statement; every type lives directly under the RootNamespace $($p.rootNamespace))")
    }
    else {
        foreach ($e in ($nsSorted | Select-Object -First 40)) {
            [void]$sb.AppendLine("- $($e.Name)  [files: $($e.Value)]")
        }
    }
    [void]$sb.AppendLine("")

    [void]$sb.AppendLine("## Public types")
    if ($typeLines.Count -eq 0) { [void]$sb.AppendLine("- (none detected)") }
    else {
        foreach ($l in ($typeLines | Select-Object -First $MaxTypes)) { [void]$sb.AppendLine($l) }
        if ($typeLines.Count -gt $MaxTypes) { [void]$sb.AppendLine("- ... and $($typeLines.Count - $MaxTypes) more") }
    }
    [void]$sb.AppendLine("")

    [void]$sb.AppendLine("## Notable public members")
    if ($memberLines.Count -eq 0) { [void]$sb.AppendLine("- (none detected)") }
    else {
        foreach ($l in ($memberLines | Select-Object -First $MaxMembers)) { [void]$sb.AppendLine($l) }
        if ($memberLines.Count -gt $MaxMembers) { [void]$sb.AppendLine("- ... and $($memberLines.Count - $MaxMembers) more") }
    }
    [void]$sb.AppendLine("")

    [void]$sb.AppendLine("## Imports")
    foreach ($im in ($importSorted | Select-Object -First $MaxImports)) { [void]$sb.AppendLine("- $im") }
    [void]$sb.AppendLine("")

    if ($allFiles.Count -gt 0) {
        [void]$sb.AppendLine("## File tree")
        foreach ($f in ($allFiles | Select-Object -First $MaxFilesListed)) {
            [void]$sb.AppendLine("- " + $f.FullName.Substring($projDir.Length + 1))
        }
        if ($allFiles.Count -gt $MaxFilesListed) {
            [void]$sb.AppendLine("- ... and $($allFiles.Count - $MaxFilesListed) more files")
        }
        [void]$sb.AppendLine("")
    }

    $safe = $p.path -replace '[\\/]', '__' -replace '%', '_pct_' -replace '[<>:"|?* ]', '_'
    $outFile = Join-Path $OutDir ($safe + '.md')
    [System.IO.File]::WriteAllText($outFile, $sb.ToString(), (New-Object System.Text.UTF8Encoding($false)))

    $totalFiles += $allFiles.Count
    $totalTypes += $typeLines.Count
    $index.Add("- $($p.path) | files=$($allFiles.Count) | types=$($typeLines.Count) | digest=digest/$($safe).md")

    $short = $p.path
    if ($short.Length -gt 78) { $short = '...' + $short.Substring($short.Length - 75) }
    Write-Host ("  {0,-78} files={1,-5} types={2}" -f $short, $allFiles.Count, $typeLines.Count) -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# Index (grouped by area = first path segment)
# ---------------------------------------------------------------------------
$idxSb = New-Object System.Text.StringBuilder
[void]$idxSb.AppendLine("# Code digest index")
[void]$idxSb.AppendLine("")
[void]$idxSb.AppendLine("Generated : $((Get-Date).ToString('o'))")
[void]$idxSb.AppendLine("Root      : $Root")
[void]$idxSb.AppendLine("Projects  : $($targets.Count)")
[void]$idxSb.AppendLine("VB files  : $totalFiles")
[void]$idxSb.AppendLine("Types     : $totalTypes")
[void]$idxSb.AppendLine("")

foreach ($grp in ($targets | Group-Object { ($_.path -split '/')[0] } | Sort-Object Name)) {
    [void]$idxSb.AppendLine("## $($grp.Name)  ($($grp.Count) projects)")
    foreach ($e in $grp.Group) {
        $m = $index | Where-Object { $_ -like "- $($e.path) |*" }
        if ($m) { [void]$idxSb.AppendLine($m) }
    }
    [void]$idxSb.AppendLine("")
}

if ($emptyProjects.Count -gt 0) {
    [void]$idxSb.AppendLine("## Projects with no .vb source found")
    foreach ($e in $emptyProjects) { [void]$idxSb.AppendLine("- $e") }
}

[System.IO.File]::WriteAllText((Join-Path $OutDir '_index.md'), $idxSb.ToString(), (New-Object System.Text.UTF8Encoding($false)))

Write-Host ""
Write-Host "=========== DIGEST SUMMARY ===========" -ForegroundColor Cyan
Write-Host ("projects digested : {0}" -f $targets.Count)
Write-Host ("vb files scanned  : {0}" -f $totalFiles)
Write-Host ("public types      : {0}" -f $totalTypes)
Write-Host ("no-source projects: {0}" -f $emptyProjects.Count)
foreach ($e in $emptyProjects) { Write-Host ("   - " + $e) -ForegroundColor Yellow }
Write-Host ("index             : {0}" -f (Join-Path $OutDir '_index.md')) -ForegroundColor Cyan
