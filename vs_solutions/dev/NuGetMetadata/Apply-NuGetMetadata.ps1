<#
.SYNOPSIS
  Idempotently normalise NuGet package metadata across the sciBASIC# vbproj files.

.DESCRIPTION
  Reads the inventory (projects.json) plus the per-project content (metadata.json)
  and upserts, into the first unconditional PropertyGroup of every *library*
  project whose RootNamespace starts with "Microsoft.VisualBasic":

    Common    : Authors, Company, Copyright, PackageProjectUrl, RepositoryUrl,
                RepositoryType, PackageLicenseExpression, PackageIcon
    Per-proj  : Title, Description, PackageTags, AssemblyTitle  (from metadata.json)

  Legacy (non-SDK) projects only receive the assembly-level fields, because
  Package* properties are meaningless there.

  Safe to run repeatedly: values already equal to the target are left untouched,
  and a file is only rewritten when at least one real change was produced.
  Encoding (UTF-8 BOM) and line endings (CRLF/LF) are preserved.

.PARAMETER WhatIf
  Report what would change without writing anything.
#>
[CmdletBinding()]
param(
    [string]$Root          = 'g:\pixelArtist\src\framework',
    [string]$InventoryFile = (Join-Path $PSScriptRoot 'projects.json'),
    [string]$MetadataFile  = (Join-Path $PSScriptRoot 'metadata.json'),
    [string]$ProjectFilter = '*',
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Unified values
# ---------------------------------------------------------------------------
$Authors      = 'xieguigang <I@xieguigang.me>'
$Company      = 'sciBASIC.NET Foundation'
$CopyrightTxt = 'Copyright (c) sciBASIC.NET Foundation'
$LicenseExpr  = 'GPL-3.0-or-later'
$IconFile     = 'logo-knot.png'
$ProjectUrl   = 'http://scibasic.net/'
$RepoUrl      = 'https://github.com/xieguigang/sciBASIC'
$RepoType     = 'git'

# Properties that only make sense for SDK-style packable projects.
$PackOnlyProps = @('PackageLicenseExpression', 'PackageIcon', 'PackageTags')

# ---------------------------------------------------------------------------
# XML helpers
# ---------------------------------------------------------------------------

function Test-WhitespaceNode($node) {
    return ($null -ne $node) -and `
           ($node.NodeType -eq [System.Xml.XmlNodeType]::Whitespace -or `
            $node.NodeType -eq [System.Xml.XmlNodeType]::SignificantWhitespace)
}

function Find-RootNsPropertyGroup($doc) {
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.LocalName -ne 'PropertyGroup') { continue }
        if ($pg.GetAttribute('Condition')) { continue }
        foreach ($c in $pg.ChildNodes) {
            if ($c.NodeType -eq 'Element' -and $c.LocalName -eq 'RootNamespace') { return $pg }
        }
    }
    # Fall back to the very first unconditional PropertyGroup.
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.LocalName -eq 'PropertyGroup' -and -not $pg.GetAttribute('Condition')) { return $pg }
    }
    return $null
}

function Get-Indents($group) {
    $groupIndent = ''
    if (Test-WhitespaceNode $group.PreviousSibling) {
        $groupIndent = ($group.PreviousSibling.Value -split "`n")[-1]
    }
    $elemIndent = ''
    $first = $group.FirstChild
    if (Test-WhitespaceNode $first) {
        $elemIndent = ($first.Value -split "`n")[-1]
    }
    if (-not $elemIndent) { $elemIndent = $groupIndent + '  ' }
    return @{ Group = $groupIndent; Elem = $elemIndent }
}

function Append-Element($doc, $container, $elem, $indents) {
    # Strip trailing whitespace so the closing tag can be re-indented cleanly.
    $last = $container.LastChild
    while (Test-WhitespaceNode $last) {
        $prev = $last.PreviousSibling
        [void]$container.RemoveChild($last)
        $last = $prev
    }
    [void]$container.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem))
    [void]$container.AppendChild($elem)
    [void]$container.AppendChild($doc.CreateWhitespace("`n" + $indents.Group))
}

function Find-ChildElement($parent, [string]$name) {
    foreach ($c in $parent.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq $name) { return $c }
    }
    return $null
}

function Set-Property($doc, $group, [string]$name, [string]$value, $indents) {
    $existing = Find-ChildElement $group $name
    if ($existing) {
        if ($existing.InnerText -ne $value) {
            $existing.InnerText = $value
            return 'updated'
        }
        return 'unchanged'
    }
    $elem = $doc.CreateElement($name, $doc.DocumentElement.NamespaceURI)
    $elem.InnerText = $value
    Append-Element $doc $group $elem $indents
    return 'added'
}

function Remove-Property($group, [string]$name) {
    $targets = @()
    foreach ($c in $group.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq $name) { $targets += $c }
    }
    foreach ($c in $targets) {
        if (Test-WhitespaceNode $c.PreviousSibling) {
            [void]$group.RemoveChild($c.PreviousSibling)
        }
        [void]$group.RemoveChild($c)
    }
    return ($targets.Count -gt 0)
}

function Find-ItemGroup($doc) {
    foreach ($ig in $doc.DocumentElement.ChildNodes) {
        if ($ig.LocalName -eq 'ItemGroup' -and -not $ig.GetAttribute('Condition')) { return $ig }
    }
    return $null
}

function Find-IconItem($doc) {
    $stack = New-Object 'System.Collections.Generic.Stack[System.Xml.XmlNode]'
    $stack.Push($doc.DocumentElement)
    while ($stack.Count -gt 0) {
        $node = $stack.Pop()
        foreach ($c in $node.ChildNodes) {
            if ($c.NodeType -ne 'Element') { continue }
            if (@('None', 'Content', 'Resource') -contains $c.LocalName) {
                $inc = $c.GetAttribute('Include')
                if ($inc -and $inc -match 'logo-knot\.png$') { return $c }
            }
            $stack.Push($c)
        }
    }
    return $null
}

function Ensure-IconItem($doc, [string]$relPath) {
    if (Find-IconItem $doc) { return 'unchanged' }

    $itemGroup = Find-ItemGroup $doc
    $created = $false
    if ($null -eq $itemGroup) {
        $itemGroup = $doc.CreateElement('ItemGroup', $doc.DocumentElement.NamespaceURI)
        $last = $doc.DocumentElement.LastChild
        $indent = ''
        if (Test-WhitespaceNode $last) { $indent = ($last.Value -split "`n")[-1] }
        [void]$doc.DocumentElement.AppendChild($doc.CreateWhitespace("`n" + $indent))
        [void]$doc.DocumentElement.AppendChild($itemGroup)
        [void]$doc.DocumentElement.AppendChild($doc.CreateWhitespace("`n"))
        $created = $true
    }

    $indents = Get-Indents $itemGroup

    $item = $doc.CreateElement('None', $doc.DocumentElement.NamespaceURI)
    $item.SetAttribute('Include', $relPath)

    $pack = $doc.CreateElement('Pack', $doc.DocumentElement.NamespaceURI)
    $pack.InnerText = 'True'
    [void]$item.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem + '  '))
    [void]$item.AppendChild($pack)

    $pp = $doc.CreateElement('PackagePath', $doc.DocumentElement.NamespaceURI)
    $pp.InnerText = '\'
    [void]$item.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem + '  '))
    [void]$item.AppendChild($pp)
    [void]$item.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem))

    Append-Element $doc $itemGroup $item $indents
    if ($created) { return 'added(+ItemGroup)' }
    return 'added'
}

function Save-XmlPreserving($doc, [string]$path) {
    $origText = [System.IO.File]::ReadAllText($path)
    $bytes    = [System.IO.File]::ReadAllBytes($path)
    $hasBom   = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $nl       = if ($origText.Contains("`r`n")) { "`r`n" } else { "`n" }

    # Preserve the *original* XML declaration verbatim. XmlWriter would emit
    # encoding="utf-16" (the backing StringWriter's encoding), so we suppress
    # the declaration entirely and re-prepend the original one.
    $scan = $origText
    if ($scan.Length -gt 0 -and $scan[0] -eq [char]0xFEFF) { $scan = $scan.Substring(1) }
    $decl = ''
    if ($scan -match '^<\?xml[^>]*\?>') { $decl = $Matches[0] }

    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Indent             = $false
    $settings.OmitXmlDeclaration = $true
    $settings.NewLineHandling    = [System.Xml.NewLineHandling]::None
    $settings.Encoding           = New-Object System.Text.UTF8Encoding($false)

    $sw = New-Object System.IO.StringWriter
    $xw = [System.Xml.XmlWriter]::Create($sw, $settings)
    try { $doc.Save($xw) } finally { $xw.Close() }

    $text = $sw.ToString()
    $text = $text -replace "`r`n", "`n"
    $text = $text -replace "`n", $nl
    if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) { $text = $text.Substring(1) }

    $final = $decl + $text
    if ($final -eq $origText) { return $false }

    $enc = New-Object System.Text.UTF8Encoding($hasBom)
    [System.IO.File]::WriteAllText($path, $final, $enc)
    return $true
}

# ---------------------------------------------------------------------------
# Load inputs
# ---------------------------------------------------------------------------

$Root = (Resolve-Path $Root).Path.TrimEnd('\')
$inventory = Get-Content $InventoryFile -Raw | ConvertFrom-Json
$metadata  = Get-Content $MetadataFile  -Raw | ConvertFrom-Json

$metaByPath = @{}
foreach ($m in $metadata) { $metaByPath[$m.path] = $m }

$targets = @($inventory.projects | Where-Object {
        $_.isLibrary -and $_.path -like $ProjectFilter
    })

Write-Host "Root            : $Root" -ForegroundColor Cyan
Write-Host "Targets         : $($targets.Count) library project(s)" -ForegroundColor Cyan
Write-Host "Mode            : $(if ($WhatIf) { 'WhatIf (no write)' } else { 'APPLY' })" -ForegroundColor Yellow
Write-Host ""

$stats = @{ files = 0; changed = 0; added = 0; updated = 0; iconAdded = 0; licFileRemoved = 0; missing = 0 }
$changedFiles = @()

foreach ($p in $targets) {
    $meta = $metaByPath[$p.path]
    if ($null -eq $meta) {
        Write-Warning "No metadata entry for $($p.path) -- skipped"
        $stats.missing++
        continue
    }
    if (-not $meta.title -or -not $meta.description -or -not $meta.packageTags -or -not $meta.assemblyTitle) {
        Write-Warning "Incomplete metadata for $($p.path) -- skipped"
        $stats.missing++
        continue
    }

    $full = Join-Path $Root ($p.path -replace '/', '\')

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    try { $doc.Load($full) }
    catch { Write-Warning "Cannot parse $($p.path): $($_.Exception.Message)"; continue }

    $group = Find-RootNsPropertyGroup $doc
    if ($null -eq $group) { Write-Warning "No PropertyGroup found in $($p.path)"; continue }

    $indents = Get-Indents $group
    $isLegacy = $p.isLegacy

    $ops = @()

    # ---- per-project content ---------------------------------------------
    $ops += (Set-Property $doc $group 'Title'         $meta.title         $indents)
    $ops += (Set-Property $doc $group 'Description'   $meta.description   $indents)
    $ops += (Set-Property $doc $group 'AssemblyTitle' $meta.assemblyTitle $indents)

    # ---- identity ---------------------------------------------------------
    $ops += (Set-Property $doc $group 'Authors'   $Authors      $indents)
    $ops += (Set-Property $doc $group 'Company'   $Company      $indents)
    $ops += (Set-Property $doc $group 'Copyright' $CopyrightTxt $indents)

    # Repository / project URLs are plain strings with no side effects, so they
    # are applied to legacy (non-SDK) projects too, keeping the repo uniform.
    $ops += (Set-Property $doc $group 'PackageProjectUrl' $ProjectUrl $indents)
    $ops += (Set-Property $doc $group 'RepositoryUrl'     $RepoUrl    $indents)
    $ops += (Set-Property $doc $group 'RepositoryType'    $RepoType   $indents)

    if (-not $isLegacy) {
        # PackageTags is a pack-only concept.
        $ops += (Set-Property $doc $group 'PackageTags' $meta.packageTags $indents)

        # License: switch to a SPDX expression; PackageLicenseFile would
        # otherwise clash (NU5034).
        if (Remove-Property $group 'PackageLicenseFile') {
            $stats.licFileRemoved++
            $ops += 'removed(PackageLicenseFile)'
        }
        $ops += (Set-Property $doc $group 'PackageLicenseExpression' $LicenseExpr $indents)

        # Icon property + its <None Include> item.
        $ops += (Set-Property $doc $group 'PackageIcon' $IconFile $indents)

        $iconOp = Ensure-IconItem $doc $p.iconRelPath
        if ($iconOp -ne 'unchanged') { $stats.iconAdded++ }
        $ops += "icon:$iconOp"
    }
    else {
        Write-Host "  [legacy] $($p.path) -- pack-only metadata skipped" -ForegroundColor DarkGray
    }

    $real = @($ops | Where-Object { $_ -ne 'unchanged' -and $_ -ne 'icon:unchanged' })

    if ($real.Count -eq 0) {
        continue
    }

    $stats.files++
    $stats.added   += @($ops | Where-Object { $_ -eq 'added' }).Count
    $stats.updated += @($ops | Where-Object { $_ -eq 'updated' }).Count
    $changedFiles += $p.path

    Write-Host ("  {0}" -f $p.path) -ForegroundColor Green
    Write-Host ("      " + ($real -join ', ')) -ForegroundColor DarkGray

    if (-not $WhatIf) {
        [void](Save-XmlPreserving $doc $full)
    }
}

Write-Host ""
Write-Host "=========== SUMMARY ===========" -ForegroundColor Cyan
Write-Host ("projects with changes : {0}" -f $stats.files)
Write-Host ("properties added      : {0}" -f $stats.added)
Write-Host ("properties updated    : {0}" -f $stats.updated)
Write-Host ("icon items added      : {0}" -f $stats.iconAdded)
Write-Host ("PackageLicenseFile rm : {0}" -f $stats.licFileRemoved)
Write-Host ("skipped (no metadata) : {0}" -f $stats.missing)
