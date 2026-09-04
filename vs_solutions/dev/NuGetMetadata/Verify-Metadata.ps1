<#
.SYNOPSIS
  Verify the NuGet metadata normalisation: XML validity, unified values,
  per-project content, icon items and idempotency.
#>
[CmdletBinding()]
param(
    [string]$Root          = 'g:\pixelArtist\src\framework',
    [string]$InventoryFile = (Join-Path $PSScriptRoot 'projects.json'),
    [string]$MetadataFile  = (Join-Path $PSScriptRoot 'metadata.json')
)

$ErrorActionPreference = 'Stop'
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

$inventory = Get-Content $InventoryFile -Raw | ConvertFrom-Json
$metadata  = Get-Content $MetadataFile  -Raw | ConvertFrom-Json
$metaByPath = @{}
foreach ($m in $metadata) { $metaByPath[$m.path] = $m }

$libs = @($inventory.projects | Where-Object { $_.isLibrary })

$problems = New-Object System.Collections.Generic.List[string]
$checked  = 0
$xmlOk    = 0

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
Write-Host ""

if ($problems.Count -eq 0) {
    Write-Host "ALL CHECKS PASSED" -ForegroundColor Green
}
else {
    Write-Host ("PROBLEMS: {0}" -f $problems.Count) -ForegroundColor Red
    foreach ($x in $problems) { Write-Host ("  - " + $x) -ForegroundColor Red }
    exit 1
}
