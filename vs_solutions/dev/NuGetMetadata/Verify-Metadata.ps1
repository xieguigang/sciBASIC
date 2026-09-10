<#
.SYNOPSIS
  Verify the NuGet metadata normalisation: XML validity, unified values,
  per-project content, icon items and idempotency.
#>
[CmdletBinding()]
param(
    [string]$Root          = '',
    [string]$InventoryFile = '',
    [string]$MetadataFile  = ''
)

$ErrorActionPreference = 'Stop'

# $PSScriptRoot is empty under some hosts (powershell -File from a wrapper),
# so fall back to the invocation path before deriving any defaults.
$ScriptDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($ScriptDir)) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition }
if ([string]::IsNullOrEmpty($Root))          { $Root          = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ScriptDir)) }
if ([string]::IsNullOrEmpty($InventoryFile)) { $InventoryFile = Join-Path $ScriptDir 'projects.json' }
if ([string]::IsNullOrEmpty($MetadataFile))  { $MetadataFile  = Join-Path $ScriptDir 'metadata.json' }

$Root = (Resolve-Path $Root).Path.TrimEnd('\')

$Expected = @{
    Authors                   = 'xieguigang <I@xieguigang.me>'
    Company                   = 'sciBASIC.NET Foundation'
    Copyright                 = 'Copyright (c) sciBASIC.NET Foundation'
    PackageLicenseExpression  = 'GPL-3.0-or-later'
    PackageIcon               = 'logo-knot.png'
    PackageProjectUrl         = 'http://scibasic.net/'
    RepositoryUrl             = 'https://github.com/xieguigang/sciBASIC'
}

$inventory = Get-Content $InventoryFile -Encoding UTF8 -Raw | ConvertFrom-Json
$metadata  = Get-Content $MetadataFile  -Encoding UTF8 -Raw | ConvertFrom-Json
$metaByPath = @{}
foreach ($m in $metadata) { $metaByPath[$m.path] = $m }

function Get-ChildText($parent, [string]$name) {
    foreach ($c in $parent.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq $name) { return $c.InnerText.Trim() }
    }
    return $null
}

function Find-ReadmeItem($doc, [string]$fileName) {
    # any None/Content/Resource item whose Include ends with the readme name
    $stack = New-Object 'System.Collections.Generic.Stack[System.Xml.XmlNode]'
    $stack.Push($doc.DocumentElement)
    while ($stack.Count -gt 0) {
        $node = $stack.Pop()
        foreach ($c in $node.ChildNodes) {
            if ($c.NodeType -ne 'Element') { continue }
            if (@('None', 'Content', 'Resource') -contains $c.LocalName) {
                $inc = $c.GetAttribute('Include')
                if ($inc -and $inc -match ([regex]::Escape($fileName) + '$')) { return $c }
            }
            $stack.Push($c)
        }
    }
    return $null
}

$libs = @($inventory.projects | Where-Object { $_.isLibrary })

$problems = New-Object System.Collections.Generic.List[string]
$checked  = 0
$xmlOk    = 0
$readmeOk = 0
$readmeSkippedLegacy = 0

foreach ($p in $libs) {
    $full = Join-Path $Root ($p.path -replace '/', '\')

    # 1) XML still parses
    try { $null = [xml](Get-Content $full -Raw); $xmlOk++ }
    catch { $problems.Add("XML INVALID: $($p.path) -> $($_.Exception.Message)"); continue }

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    $doc.Load($full)

    $props = @{}
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.LocalName -ne 'PropertyGroup') { continue }
        foreach ($c in $pg.ChildNodes) {
            if ($c.NodeType -ne 'Element') { continue }
            if (-not $props.ContainsKey($c.LocalName)) { $props[$c.LocalName] = $c.InnerText.Trim() }
        }
    }

    $meta = $metaByPath[$p.path]

    # 2) unified values
    #    Legacy (non-SDK) projects only receive the fields that need no pack
    #    infrastructure: PackageIcon / PackageTags / PackageLicenseExpression
    #    are meaningless there.
    foreach ($key in $Expected.Keys) {
        if ($p.isLegacy -and @('PackageIcon', 'PackageTags', 'PackageLicenseExpression') -contains $key) { continue }
        $actual = $props[$key]
        if ($actual -ne $Expected[$key]) {
            $problems.Add("UNIFIED MISMATCH [$key] in $($p.path): expected '$($Expected[$key])' got '$actual'")
        }
    }

    # 3) no PackageLicenseFile left alongside the expression
    if (-not $p.isLegacy -and $props.ContainsKey('PackageLicenseFile')) {
        $problems.Add("PackageLicenseFile still present in $($p.path)")
    }

    # 4) per-project content
    foreach ($key in @('Title', 'Description', 'PackageTags', 'AssemblyTitle')) {
        if ($p.isLegacy -and $key -eq 'PackageTags') { continue }
        if ([string]::IsNullOrWhiteSpace($props[$key])) {
            $problems.Add("MISSING $key in $($p.path)")
        }
        elseif ($meta -and $props[$key] -ne $meta.$key) {
            $problems.Add("CONTENT MISMATCH $key in $($p.path)")
        }
    }

    # 5) icon item present when PackageIcon is declared
    if (-not $p.isLegacy -and $props['PackageIcon'] -and -not $p.existingIconItem) {
        $problems.Add("PackageIcon without <None Include>: $($p.path)")
    }

    # 6) icon relative path must resolve to a real file
    if ($p.existingIconItem) {
        $resolved = [System.IO.Path]::GetFullPath((Join-Path (Split-Path $full -Parent) $p.existingIconItem))
        if (-not (Test-Path $resolved)) {
            $problems.Add("Icon path does not resolve: $($p.path) -> $($p.existingIconItem)")
        }
    }

    # 5b) readme wiring (packable SDK-style projects only)
    if (-not $p.isLegacy) {
        $readmeProp = $props['PackageReadmeFile']
        if ([string]::IsNullOrWhiteSpace($readmeProp)) {
            $problems.Add("MISSING PackageReadmeFile in $($p.path)")
        }
        else {
            # the file must live in the project folder
            $readmePath = Join-Path (Split-Path $full -Parent) $readmeProp
            if (-not (Test-Path -LiteralPath $readmePath)) {
                $problems.Add("PackageReadmeFile does not resolve in project folder: $($p.path) -> $readmeProp")
            }

            # an item must pack it under that exact package-relative name
            $item = Find-ReadmeItem $doc $readmeProp
            if ($null -eq $item) {
                $problems.Add("PackageReadmeFile without <None Include>: $($p.path) -> $readmeProp")
            }
            else {
                $inc  = $item.GetAttribute('Include')
                $pack = Get-ChildText $item 'Pack'
                $pp   = Get-ChildText $item 'PackagePath'
                if ([System.IO.Path]::GetFileName($inc) -ne $readmeProp) {
                    $problems.Add("readme item file name mismatch: $($p.path) -> Include=$inc vs PackageReadmeFile=$readmeProp")
                }
                $resolved = [System.IO.Path]::GetFullPath((Join-Path (Split-Path $full -Parent) $inc))
                if (-not (Test-Path -LiteralPath $resolved)) {
                    $problems.Add("readme item path does not resolve: $($p.path) -> $inc")
                }
                if ($pack -ne 'True') {
                    $problems.Add("readme item missing Pack=True: $($p.path)")
                }
                if ([string]::IsNullOrEmpty($pp)) {
                    $problems.Add("readme item missing PackagePath: $($p.path)")
                }
                $readmeOk++
            }
        }
    }
    else { $readmeSkippedLegacy++ }

    $checked++
}

# 7) per-project texts must be unique
$dupeTitles = @($metadata | Group-Object title        | Where-Object { $_.Count -gt 1 })
$dupeDesc   = @($metadata | Group-Object description  | Where-Object { $_.Count -gt 1 })
$dupeAsm    = @($metadata | Group-Object assemblyTitle| Where-Object { $_.Count -gt 1 })
foreach ($g in $dupeTitles) { $problems.Add("DUPLICATE title: $($g.Name)") }
foreach ($g in $dupeDesc)   { $problems.Add("DUPLICATE description: $($g.Name)") }
foreach ($g in $dupeAsm)    { $problems.Add("DUPLICATE assemblyTitle: $($g.Name)") }

# 8) metadata.json must cover every library project
foreach ($p in $libs) {
    if (-not $metaByPath.ContainsKey($p.path)) { $problems.Add("metadata.json missing entry: $($p.path)") }
}
foreach ($m in $metadata) {
    if (-not ($libs | Where-Object { $_.path -eq $m.path })) {
        $problems.Add("metadata.json has entry for non-library project: $($m.path)")
    }
}

Write-Host "=========== VERIFICATION ===========" -ForegroundColor Cyan
Write-Host ("library projects checked : {0}" -f $checked)
Write-Host ("XML parses cleanly       : {0}" -f $xmlOk)
Write-Host ("metadata.json entries    : {0}" -f $metadata.Count)
Write-Host ("needsIconAdd remaining   : {0}" -f $inventory.iconAddCount)
Write-Host ("needsIconFix remaining   : {0}" -f $inventory.iconFixCount)
Write-Host ("readme wiring OK         : {0}" -f $readmeOk)
Write-Host ("readme skipped (legacy)  : {0}" -f $readmeSkippedLegacy)
Write-Host ""

if ($problems.Count -eq 0) {
    Write-Host "ALL CHECKS PASSED" -ForegroundColor Green
}
else {
    Write-Host ("PROBLEMS: {0}" -f $problems.Count) -ForegroundColor Red
    foreach ($x in $problems) { Write-Host ("  - " + $x) -ForegroundColor Red }
    exit 1
}
