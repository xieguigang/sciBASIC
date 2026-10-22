<#
.SYNOPSIS
  Map the git history of a time window onto the projects listed in projects.json.

.DESCRIPTION
  Reads the inventory written by Export-ProjectInventory.ps1 and walks the git log
  from a lower bound (defaults to the inventory's previousGeneratedAt, i.e. the
  timestamp of the previous metadata refresh) up to HEAD. Every touched file is
  attributed to the deepest project directory that contains it, so the resulting
  changes.json tells you, per project, which commits landed and which files moved.

  This file is the input used to write the per-package <PackageReleaseNotes>:
  the notes must summarise what actually changed in the window, not the whole
  history of the assembly.

.PARAMETER Since
  Lower bound of the window (inclusive), e.g. '2026-10-12 13:20:54'. Defaults to
  previousGeneratedAt from the inventory so repeated runs stay consistent.

.PARAMETER OutFile
  Defaults to <script dir>\changes.json.
#>
[CmdletBinding()]
param(
    [string]$Root          = '',
    [string]$InventoryFile = '',
    [string]$Since         = '',
    [string]$OutFile       = ''
)

$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($ScriptDir)) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition }
if ([string]::IsNullOrEmpty($Root))          { $Root          = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ScriptDir)) }
if ([string]::IsNullOrEmpty($InventoryFile)) { $InventoryFile = Join-Path $ScriptDir 'projects.json' }
if ([string]::IsNullOrEmpty($OutFile))       { $OutFile       = Join-Path $ScriptDir 'changes.json' }

$Root = (Resolve-Path $Root).Path.TrimEnd('\')

$inventory = Get-Content $InventoryFile -Encoding UTF8 -Raw | ConvertFrom-Json
if ([string]::IsNullOrEmpty($Since)) {
    $Since = $inventory.previousGeneratedAt
    if ([string]::IsNullOrEmpty($Since)) { throw 'No -Since given and the inventory carries no previousGeneratedAt.' }
}

$projects = @($inventory.projects)
$byDir = @{}
foreach ($p in $projects) {
    $dir = ($p.path -replace '/', '\')
    $dir = $dir.Substring(0, $dir.LastIndexOf('\'))
    $byDir[$dir.ToLowerInvariant()] = $p
}

function Resolve-Owner([string]$relPath) {
    $probe = $relPath
    while ($true) {
        $idx = $probe.LastIndexOf('\')
        if ($idx -lt 0) { return $null }
        $probe = $probe.Substring(0, $idx)
        $key = $probe.ToLowerInvariant()
        if ($byDir.ContainsKey($key)) { return $byDir[$key] }
    }
}

Write-Host "Repository root : $Root" -ForegroundColor Cyan
Write-Host "Window since    : $Since" -ForegroundColor Cyan

Push-Location $Root
try {
    $raw = & git --no-pager log --since="$Since" --numstat --pretty=format:'@@%h|%aI|%s'
}
finally { Pop-Location }

$agg = @{}
$commitCount = 0
$current = $null

foreach ($line in $raw) {
    if ($line -like '@@*') {
        $commitCount++
        $parts = $line.Substring(2).Split('|')
        $current = [ordered]@{ hash = $parts[0]; date = $parts[1]; subject = $parts[2] }
        continue
    }
    if ([string]::IsNullOrWhiteSpace($line) -or $null -eq $current) { continue }

    $cols = $line -split "`t"
    if ($cols.Count -lt 3) { continue }
    $add = $cols[0]; $del = $cols[1]; $file = ($cols[2] -replace '/', '\')
    # Binary diffs show '-' in both counters.
    $addN = 0; $delN = 0
    [void][int]::TryParse($add, [ref]$addN)
    [void][int]::TryParse($del, [ref]$delN)

    $owner = Resolve-Owner $file
    if ($null -eq $owner) { continue }

    if (-not $agg.ContainsKey($owner.path)) {
        $agg[$owner.path] = [ordered]@{
            path        = $owner.path
            rootNamespace = $owner.rootNamespace
            assemblyName  = $owner.assemblyName
            isLibrary     = $owner.isLibrary
            commits     = New-Object System.Collections.Generic.List[object]
            files       = @{}
            insertions  = 0
            deletions   = 0
        }
    }
    $e = $agg[$owner.path]
    if (-not ($e.commits | Where-Object { $_.hash -eq $current.hash })) {
        $e.commits.Add($current)
    }
    if (-not $e.files.ContainsKey($file)) { $e.files[$file] = @{ adds = 0; dels = 0 } }
    $e.files[$file].adds += $addN
    $e.files[$file].dels += $delN
    $e.insertions += $addN
    $e.deletions += $delN
}

$records = New-Object System.Collections.Generic.List[object]
foreach ($e in ($agg.Values | Sort-Object { -$_.commits.Count })) {
    $top = $e.files.GetEnumerator() |
        Sort-Object { - ($_.Value.adds + $_.Value.dels) } |
        Select-Object -First 12 |
        ForEach-Object { "{0} (+{1}/-{2})" -f $_.Key, $_.Value.adds, $_.Value.dels }

    $records.Add([ordered]@{
        path         = $e.path
        rootNamespace = $e.rootNamespace
        assemblyName = $e.assemblyName
        isLibrary    = $e.isLibrary
        commitCount  = $e.commits.Count
        insertions   = $e.insertions
        deletions    = $e.deletions
        subjects     = @($e.commits | ForEach-Object { $_.subject } | Select-Object -Unique)
        files        = @($e.files.Keys | Sort-Object)
        topFiles     = @($top)
    })
}

# Projects with no change inside the window are still listed, so the release-note
# step can tell "nothing happened" apart from "project not scanned".
$untouched = @($projects | Where-Object { $_.isLibrary -and -not $agg.ContainsKey($_.path) } |
    ForEach-Object { $_.path })

$payload = [ordered]@{
    generatedAt   = (Get-Date).ToString('o')
    since         = $Since
    head          = (& git -C $Root rev-parse HEAD)
    totalCommits  = $commitCount
    changedCount  = $records.Count
    untouchedCount = $untouched.Count
    projects      = $records
    untouched     = $untouched
}

$payload | ConvertTo-Json -Depth 8 | Set-Content -Path $OutFile -Encoding UTF8

Write-Host ("Commits in window : {0}" -f $commitCount) -ForegroundColor Green
Write-Host ("Projects touched  : {0}" -f $records.Count) -ForegroundColor Green
Write-Host ("Projects untouched: {0}" -f $untouched.Count) -ForegroundColor Yellow
Write-Host ("Changes           : $OutFile") -ForegroundColor Cyan
foreach ($r in $records) {
    Write-Host ("  {0,-70} commits={1,-4} +{2}/-{3}" -f $r.path, $r.commitCount, $r.insertions, $r.deletions) -ForegroundColor DarkGray
}
