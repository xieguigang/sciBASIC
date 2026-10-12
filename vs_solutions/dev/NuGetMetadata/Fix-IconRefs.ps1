<#
.SYNOPSIS
  Add the missing logo-knot.png <None Include> item to projects that declare
  <PackageIcon> but ship no icon file (would fail pack with NU5046).
#>
[CmdletBinding()]
param(
    [string]$Root          = '',
    [string]$InventoryFile = '',
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

function Test-WhitespaceNode($node) {
    return ($null -ne $node) -and `
           ($node.NodeType -eq [System.Xml.XmlNodeType]::Whitespace -or `
            $node.NodeType -eq [System.Xml.XmlNodeType]::SignificantWhitespace)
}

function Get-Indents($group) {
    $groupIndent = ''
    if (Test-WhitespaceNode $group.PreviousSibling) {
        $groupIndent = ($group.PreviousSibling.Value -split "`n")[-1]
    }
    $elemIndent = ''
    if (Test-WhitespaceNode $group.FirstChild) {
        $elemIndent = ($group.FirstChild.Value -split "`n")[-1]
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
    [System.IO.File]::WriteAllText($path, $final, (New-Object System.Text.UTF8Encoding($hasBom)))
    return $true
}

$inventory = Get-Content $InventoryFile -Raw | ConvertFrom-Json
$targets = @($inventory.projects | Where-Object { $_.needsIconFix })

Write-Host "Projects needing an icon <None Include>: $($targets.Count)" -ForegroundColor Cyan

foreach ($p in $targets) {
    $full = Join-Path $Root ($p.path -replace '/', '\')

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    try { $doc.Load($full) }
    catch { Write-Warning "Cannot parse $($p.path): $($_.Exception.Message)"; continue }

    # Find the first unconditional ItemGroup; the <None Remove="..."> groups of
    # SDK projects are unconditional and are a valid home for the new item.
    $itemGroup = $null
    foreach ($ig in $doc.DocumentElement.ChildNodes) {
        if ($ig.LocalName -eq 'ItemGroup' -and -not $ig.GetAttribute('Condition')) { $itemGroup = $ig; break }
    }
    if ($null -eq $itemGroup) {
        # Derive the child indent from the document body BEFORE building the
        # new group, so the element is not written flush-left. Prefer the last
        # real child's indentation; fall back to the first child's.
        $childIndent = ''
        foreach ($n in @($doc.DocumentElement.ChildNodes)) {
            if (Test-WhitespaceNode $n) { continue }
            $prev = $n.PreviousSibling
            if (Test-WhitespaceNode $prev) { $childIndent = ($prev.Value -split "`n")[-1] }
            break
        }
        if ([string]::IsNullOrEmpty($childIndent)) {
            $first = $doc.DocumentElement.FirstChild
            if (Test-WhitespaceNode $first) { $childIndent = ($first.Value -split "`n")[-1] }
        }
        if ([string]::IsNullOrEmpty($childIndent)) { $childIndent = '  ' }

        $itemGroup = $doc.CreateElement('ItemGroup', $doc.DocumentElement.NamespaceURI)
        [void]$doc.DocumentElement.AppendChild($doc.CreateWhitespace("`n" + $childIndent))
        [void]$doc.DocumentElement.AppendChild($itemGroup)
        [void]$doc.DocumentElement.AppendChild($doc.CreateWhitespace("`n"))
    }

    $indents = Get-Indents $itemGroup
    $ns = $doc.DocumentElement.NamespaceURI

    $item = $doc.CreateElement('None', $ns)
    $item.SetAttribute('Include', $p.iconRelPath)
    $pack = $doc.CreateElement('Pack', $ns); $pack.InnerText = 'True'
    $pp   = $doc.CreateElement('PackagePath', $ns); $pp.InnerText = '\'
    [void]$item.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem + '  '))
    [void]$item.AppendChild($pack)
    [void]$item.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem + '  '))
    [void]$item.AppendChild($pp)
    [void]$item.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem))

    # The path must actually resolve, otherwise pack still fails.
    $resolved = [System.IO.Path]::GetFullPath((Join-Path (Split-Path $full -Parent) $p.iconRelPath))
    if (-not (Test-Path $resolved)) {
        Write-Warning "Icon path does not resolve for $($p.path): $resolved"
        continue
    }

    Append-Element $doc $itemGroup $item $indents

    Write-Host ("  " + $p.path + "  ->  " + $p.iconRelPath) -ForegroundColor Green
    if (-not $WhatIf) { [void](Save-XmlPreserving $doc $full) }
}
