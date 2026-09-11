<#
.SYNOPSIS
  Wire the per-project README.md into every packable sciBASIC# vbproj.

.DESCRIPTION
  For each library project listed in projects.json this script

    1. locates the readme file that physically sits in the project folder
       (README.md, readme.md, PROJECT_SUMMARY.md, project_summary.md),
    2. writes <PackageReadmeFile>{fileName}</PackageReadmeFile> into the same
       unconditional PropertyGroup that declares RootNamespace, and
    3. makes sure an item

           <None Include="{fileName}">
             <Pack>True</Pack>
             <PackagePath>\</PackagePath>
           </None>

       exists so that NuGet actually packs the file.

  Existing items that point at a readme outside the project folder (for example
  `..\README.md` or `..\..\README.md`) are re-pointed to the local file instead
  of being duplicated, so no NU5118-style duplicate warnings appear.

  Legacy (non SDK-style) projects are reported and skipped: they have no
  `dotnet pack` pipeline, so PackageReadmeFile would be inert there.

  Idempotent: unchanged values are not rewritten and a file is only saved when
  at least one real change was produced. Encoding (BOM) and line endings
  (CRLF/LF) are preserved.

.PARAMETER WhatIf
  Report what would change without writing anything.
#>
[CmdletBinding()]
param(
    [string]$Root = '',
    [string]$InventoryFile = '',
    [string]$ProjectFilter = '*',
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

# $PSScriptRoot is empty under some hosts (powershell -File from a wrapper),
# so fall back to the invocation path before deriving any defaults.
$ScriptDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($ScriptDir)) { $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition }
if ([string]::IsNullOrEmpty($Root))          { $Root          = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ScriptDir)) }
if ([string]::IsNullOrEmpty($InventoryFile)) { $InventoryFile = Join-Path $ScriptDir 'projects.json' }

$Root = (Resolve-Path $Root).Path.TrimEnd('\')

# Candidate readme file names, in priority order.
$ReadmeCandidates = @('README.md', 'readme.md', 'PROJECT_SUMMARY.md', 'project_summary.md')

# ---------------------------------------------------------------------------
# XML helpers (same proven approach as Apply-NuGetMetadata.ps1)
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

function Set-ChildText($doc, $parent, [string]$name, [string]$value, $indents) {
    $existing = Find-ChildElement $parent $name
    if ($existing) {
        if ($existing.InnerText -ne $value) {
            $existing.InnerText = $value
            return 'updated'
        }
        return 'unchanged'
    }
    $elem = $doc.CreateElement($name, $doc.DocumentElement.NamespaceURI)
    $elem.InnerText = $value
    $subIndent = @{ Group = $indents.Elem; Elem = $indents.Elem + '  ' }
    Append-Element $doc $parent $elem $subIndent
    return 'added'
}

function Find-ReadmeItem($doc, [string]$fileName) {
    # Match any None/Content/Resource item whose Include ends with the readme
    # file name, so `..\README.md` and `README.md` are treated as the same item.
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

function Find-ItemGroup($doc) {
    foreach ($ig in $doc.DocumentElement.ChildNodes) {
        if ($ig.LocalName -eq 'ItemGroup' -and -not $ig.GetAttribute('Condition')) { return $ig }
    }
    return $null
}

function Save-XmlPreserving($doc, [string]$path) {
    $origText = [System.IO.File]::ReadAllText($path)
    $bytes    = [System.IO.File]::ReadAllBytes($path)
    $hasBom   = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $nl       = if ($origText.Contains("`r`n")) { "`r`n" } else { "`n" }

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
# Main loop
# ---------------------------------------------------------------------------
$inventory = Get-Content $InventoryFile -Encoding UTF8 -Raw | ConvertFrom-Json
$targets = @($inventory.projects | Where-Object { $_.isLibrary -and $_.path -like $ProjectFilter })

Write-Host "Repository root : $Root" -ForegroundColor Cyan
Write-Host "Library projects: $($targets.Count)" -ForegroundColor Cyan
Write-Host "Mode            : $(if ($WhatIf) { 'WhatIf (no write)' } else { 'APPLY' })" -ForegroundColor Yellow
Write-Host ""

$stats = @{ changed = 0; propAdded = 0; propUpdated = 0; itemAdded = 0; itemRepointed = 0; itemFixed = 0; noReadme = 0; legacy = 0; failed = 0 }
$missing = New-Object System.Collections.Generic.List[string]

foreach ($p in $targets) {
    $projFull = Join-Path $Root ($p.path -replace '/', '\')
    $projDir  = Split-Path $projFull -Parent

    # 1) which readme file actually lives in the project folder?
    $fileName = $null
    foreach ($cand in $ReadmeCandidates) {
        if (Test-Path -LiteralPath (Join-Path $projDir $cand)) { $fileName = $cand; break }
    }
    if ($null -eq $fileName) {
        Write-Warning "No readme file in project folder: $($p.path)"
        $missing.Add($p.path)
        $stats.noReadme++
        continue
    }

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    try { $doc.Load($projFull) }
    catch { Write-Warning "Cannot parse $($p.path): $($_.Exception.Message)"; $stats.failed++; continue }

    $isLegacy = [string]::IsNullOrEmpty($doc.DocumentElement.GetAttribute('Sdk'))
    if ($isLegacy) {
        Write-Host "  [legacy] $($p.path) -- no pack pipeline, skipped" -ForegroundColor DarkGray
        $stats.legacy++
        continue
    }

    $group = Find-RootNsPropertyGroup $doc
    if ($null -eq $group) { Write-Warning "No PropertyGroup found in $($p.path)"; $stats.failed++; continue }

    $indents = Get-Indents $group
    $ops = New-Object System.Collections.Generic.List[string]

    # 2) <PackageReadmeFile>
    $op = Set-Property $doc $group 'PackageReadmeFile' $fileName $indents
    $ops.Add("readmeFile:$op")
    if ($op -eq 'added')   { $stats.propAdded++ }
    if ($op -eq 'updated') { $stats.propUpdated++ }

    # 3) <None Include="{file}"> with Pack/PackagePath
    $item = Find-ReadmeItem $doc $fileName
    if ($null -eq $item) {
        $itemGroup = Find-ItemGroup $doc
        if ($null -eq $itemGroup) {
            $itemGroup = $doc.CreateElement('ItemGroup', $doc.DocumentElement.NamespaceURI)
            $last = $doc.DocumentElement.LastChild
            $indent = ''
            if (Test-WhitespaceNode $last) { $indent = ($last.Value -split "`n")[-1] }
            [void]$doc.DocumentElement.AppendChild($doc.CreateWhitespace("`n" + $indent))
            [void]$doc.DocumentElement.AppendChild($itemGroup)
            [void]$doc.DocumentElement.AppendChild($doc.CreateWhitespace("`n"))
        }
        $igIndents = Get-Indents $itemGroup

        $item = $doc.CreateElement('None', $doc.DocumentElement.NamespaceURI)
        $item.SetAttribute('Include', $fileName)
        Append-Element $doc $itemGroup $item $igIndents
        $ops.Add('readmeItem:added')
        $stats.itemAdded++
    }
    else {
        $current = $item.GetAttribute('Include')
        if ($current -ne $fileName) {
            $item.SetAttribute('Include', $fileName)
            $ops.Add("readmeItem:repointed($current -> $fileName)")
            $stats.itemRepointed++
        }
        else {
            $ops.Add('readmeItem:exists')
        }
    }

    # The item may have just been created, so re-derive its indentation from
    # the parent ItemGroup (Get-Indents needs the element to be in the tree).
    $parentIg = $item.ParentNode
    $igIndents2 = Get-Indents $parentIg

    $o1 = Set-ChildText $doc $item 'Pack' 'True' $igIndents2
    $o2 = Set-ChildText $doc $item 'PackagePath' '\' $igIndents2
    if (@($o1, $o2) -contains 'added' -or @($o1, $o2) -contains 'updated') {
        $ops.Add("itemMeta:$o1/$o2")
        $stats.itemFixed++
    }

    $real = @($ops | Where-Object {
            $_ -notmatch ':(unchanged|exists)' -and $_ -notmatch 'itemMeta:unchanged/unchanged'
        })

    if ($real.Count -eq 0) { continue }

    $stats.changed++
    Write-Host ("  {0}" -f $p.path) -ForegroundColor Green
    Write-Host ("      " + ($real -join ', ')) -ForegroundColor DarkGray

    if (-not $WhatIf) {
        [void](Save-XmlPreserving $doc $projFull)
    }
}

Write-Host ""
Write-Host "=========== SUMMARY ===========" -ForegroundColor Cyan
Write-Host ("projects changed       : {0}" -f $stats.changed)
Write-Host ("PackageReadmeFile added: {0}" -f $stats.propAdded)
Write-Host ("PackageReadmeFile upd. : {0}" -f $stats.propUpdated)
Write-Host ("readme items added     : {0}" -f $stats.itemAdded)
Write-Host ("readme items repointed : {0}" -f $stats.itemRepointed)
Write-Host ("readme items fixed     : {0}" -f $stats.itemFixed)
Write-Host ("legacy skipped         : {0}" -f $stats.legacy)
Write-Host ("no readme in folder    : {0}" -f $stats.noReadme)
Write-Host ("failed                 : {0}" -f $stats.failed)
foreach ($m in $missing) { Write-Host ("   - " + $m) -ForegroundColor Yellow }
