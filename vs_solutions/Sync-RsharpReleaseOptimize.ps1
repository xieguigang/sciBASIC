<#
.SYNOPSIS
  Enable <Optimize>true</Optimize> for the "Rsharp_app_release|x64" build
  configuration of every *.vbproj registered in a .slnx solution file.

.DESCRIPTION
  The script reads the Visual Studio XML solution format (*.slnx), collects
  every <Project Path="..."/> entry, keeps the *.vbproj ones and patches each
  project file in place:

    1. Target property group
       A <PropertyGroup> whose Condition selects "Rsharp_app_release|x64" is
       reused when present. The condition is matched structurally (whitespace
       removed, case folded), so all of these forms are recognised:

           Condition="'$(Configuration)|$(Platform)'=='Rsharp_app_release|x64'"
           Condition=" '$(Configuration)|$(Platform)' == 'Rsharp_app_release|x64' "
           Condition="'$(Configuration)'=='Rsharp_app_release' And '$(Platform)'=='x64'"

       Preference order: an exact "configuration|platform" group first, then a
       bare "configuration only" group. A group that pins another platform
       (for example "...|x86") is never touched.

    2. Optimize property
       <Optimize>true</Optimize> is upserted into that group. An existing
       <Optimize>false</Optimize> is rewritten in place; duplicates are not
       created.

    3. Missing group
       When no matching group exists a new one is appended after the last
       top-level <PropertyGroup> of the project:

           <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Rsharp_app_release|x64'">
             <Optimize>true</Optimize>
           </PropertyGroup>

       Placing it last is deliberate: MSBuild lets the *last* declaration of a
       property win, so this also overrides an unconditional
       <Optimize>false</Optimize> that an SDK style project inherits from the
       Debug defaults.

  A file is only rewritten when the value actually changes, so the script is
  safe to run repeatedly and does not disturb incremental builds (an untouched
  timestamp means MSBuild keeps its cached outputs). Encoding (UTF-8 with or
  without BOM), line endings (CRLF/LF) and the surrounding indentation are
  preserved, and no backup copies are created -- use git to review the diff.

  Note: the script does NOT add "Rsharp_app_release" to <Configurations>. That
  element only controls which configurations Visual Studio offers in the UI;
  `dotnet build -c Rsharp_app_release` works regardless. The CSV report tells
  you for each project whether the configuration is declared.

  Requires Windows PowerShell 5.1 or newer; no external modules.

.PARAMETER Solution
  Path to the .slnx solution file, e.g. "G:\GCModeller\src\GCModeller.slnx".

.PARAMETER Configuration
  Build configuration to target. Defaults to "Rsharp_app_release".

.PARAMETER Platform
  Build platform to target. Defaults to "x64".

.PARAMETER Optimize
  Value written into <Optimize>. Defaults to $true. Pass -Optimize $false (or
  -Optimize 0) to switch the optimization back off for the same configuration.
  Declared as [object] because `powershell -File` hands every argument over as
  a string, which the [bool] binder refuses to convert for 0/1.

.PARAMETER ProjectFilter
  Wildcard applied to the project file name. Defaults to "*.vbproj"; use "*"
  to process every project type referenced by the solution.

.PARAMETER ReportFile
  Optional CSV path receiving one row per processed project.

.PARAMETER WhatIf
  Run the whole analysis and print the planned operations without writing
  anything.

.EXAMPLE
  .\Sync-RsharpReleaseOptimize.ps1 G:\GCModeller\src\GCModeller.slnx -WhatIf

  Preview which projects would be modified.

.EXAMPLE
  .\Sync-RsharpReleaseOptimize.ps1 G:\GCModeller\src\GCModeller.slnx

  Apply the change to every *.vbproj of the GCModeller solution.

.EXAMPLE
  .\Sync-RsharpReleaseOptimize.ps1 G:\GCModeller\src\GCModeller.slnx -ReportFile .\optimize-report.csv -Verbose

  Apply and dump a CSV report of every decision.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Solution,

    [string]$Configuration = 'Rsharp_app_release',
    [string]$Platform      = 'x64',
    [object]$Optimize      = $true,
    [string]$ProjectFilter = '*.vbproj',
    [string]$ReportFile    = '',
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Target condition / value
# ---------------------------------------------------------------------------

# Built by concatenation so that PowerShell never tries to expand the $(...)
# MSBuild placeholders.
$TargetCondition = "'" + '$(Configuration)|$(Platform)' + "'=='" + $Configuration + '|' + $Platform + "'"
$TargetValue     = if ($Optimize) { 'true' } else { 'false' }
$PairToken       = ($Configuration + '|' + $Platform).ToLowerInvariant()
$ConfigToken     = $Configuration.ToLowerInvariant()

# ---------------------------------------------------------------------------
# Generic XML helpers (kept in sync with Sync-NuGetSolution.ps1)
# ---------------------------------------------------------------------------

function Test-WhitespaceNode($node) {
    return ($null -ne $node) -and `
           ($node.NodeType -eq [System.Xml.XmlNodeType]::Whitespace -or `
            $node.NodeType -eq [System.Xml.XmlNodeType]::SignificantWhitespace)
}

function Get-Indents($container) {
    $groupIndent = ''
    if (Test-WhitespaceNode $container.PreviousSibling) {
        $groupIndent = ($container.PreviousSibling.Value -split "`n")[-1]
    }
    $elemIndent = ''
    $first = $container.FirstChild
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
        if ($existing.InnerText.Trim() -ne $value) {
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

function Save-XmlPreserving($doc, [string]$path) {
    $origText = [System.IO.File]::ReadAllText($path)
    $bytes    = [System.IO.File]::ReadAllBytes($path)
    $hasBom   = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $nl       = if ($origText.Contains("`r`n")) { "`r`n" } else { "`n" }

    # Preserve the *original* XML declaration verbatim. XmlWriter would emit
    # encoding="utf-16" (the backing StringWriter's encoding), so the declaration
    # is suppressed and re-prepended instead.
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
# Condition matching
# ---------------------------------------------------------------------------

function Get-ConditionKind([string]$condition) {
    # Whitespace is irrelevant in MSBuild conditions, so it is stripped before
    # matching; the comparison is case insensitive (VB projects are not
    # consistent about the casing of configuration and platform names).
    if ([string]::IsNullOrEmpty($condition)) { return 'none' }

    $norm = ($condition -replace '\s', '').ToLowerInvariant()

    # "config|platform" written as a single token: '<cfg>|<plat>'
    if ($norm.Contains($PairToken)) { return 'pair' }

    # Configuration only (no '|' at all): applies to every platform, x64 included.
    if ($norm.Contains($ConfigToken) -and -not $norm.Contains('|')) { return 'config' }

    return 'none'
}

function Find-TargetGroups($doc) {
    $pairs   = @()
    $configs = @()

    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -ne 'Element' -or $pg.LocalName -ne 'PropertyGroup') { continue }

        switch (Get-ConditionKind $pg.GetAttribute('Condition')) {
            'pair'   { $pairs   += $pg }
            'config' { $configs += $pg }
        }
    }

    # The platform specific group is the authoritative one; a configuration-only
    # group is used only when no platform pinning group exists.
    if ($pairs.Count -gt 0) { return @($pairs) }
    return @($configs)
}

function New-ConditionalPropertyGroup($doc, [string]$condition) {
    $root = $doc.DocumentElement

    # Anchor after the last top level PropertyGroup so the new group stays with
    # its peers, above the ItemGroup section, and wins over any earlier
    # unconditional <Optimize> (MSBuild honours the last declaration).
    $anchor = $null
    foreach ($c in $root.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq 'PropertyGroup') { $anchor = $c }
    }

    $groupIndent = '  '
    if ($null -ne $anchor -and (Test-WhitespaceNode $anchor.PreviousSibling)) {
        $probe = ($anchor.PreviousSibling.Value -split "`n")[-1]
        if ($probe) { $groupIndent = $probe }
    }
    $elemIndent = $groupIndent + '  '

    $group = $doc.CreateElement('PropertyGroup', $root.NamespaceURI)
    $group.SetAttribute('Condition', $condition)
    # Seed the whitespace so Get-Indents can derive the child indentation.
    [void]$group.AppendChild($doc.CreateWhitespace("`n" + $elemIndent))

    if ($null -ne $anchor) {
        [void]$root.InsertAfter($group, $anchor)
        [void]$root.InsertAfter($doc.CreateWhitespace("`n`n" + $groupIndent), $anchor)
    }
    else {
        $last = $root.LastChild
        while (Test-WhitespaceNode $last) {
            $prev = $last.PreviousSibling
            [void]$root.RemoveChild($last)
            $last = $prev
        }
        [void]$root.AppendChild($doc.CreateWhitespace("`n`n" + $groupIndent))
        [void]$root.AppendChild($group)
        [void]$root.AppendChild($doc.CreateWhitespace("`n"))
    }
    return $group
}

function Test-ConfigurationDeclared($doc) {
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -ne 'Element' -or $pg.LocalName -ne 'PropertyGroup') { continue }
        $elem = Find-ChildElement $pg 'Configurations'
        if ($null -eq $elem) { continue }

        foreach ($token in ($elem.InnerText -split ';')) {
            if ([string]::Equals($token.Trim(), $Configuration, [System.StringComparison]::OrdinalIgnoreCase)) {
                return $true
            }
        }
    }
    return $false
}

# ---------------------------------------------------------------------------
# Resolve the solution file
# ---------------------------------------------------------------------------

if ([string]::IsNullOrWhiteSpace($Solution)) { throw 'A .slnx solution path is required.' }
if (-not (Test-Path -LiteralPath $Solution)) { throw "Solution file not found: $Solution" }

$Solution = (Resolve-Path -LiteralPath $Solution).Path
if ([System.IO.Path]::GetExtension($Solution) -ne '.slnx') {
    Write-Warning "The given file is not a .slnx solution: $Solution"
}

$solutionDir = Split-Path -Parent $Solution

Write-Host "Solution        : $Solution" -ForegroundColor Cyan
Write-Host "Configuration   : $Configuration|$Platform" -ForegroundColor Cyan
Write-Host "Optimize        : $TargetValue" -ForegroundColor Cyan
Write-Host "Condition       : $TargetCondition" -ForegroundColor Cyan
Write-Host "Project filter  : $ProjectFilter" -ForegroundColor Cyan
Write-Host "Mode            : $(if ($WhatIf) { 'WhatIf (no write)' } else { 'APPLY' })" -ForegroundColor Yellow
Write-Host ""

# ---------------------------------------------------------------------------
# Collect the projects referenced by the solution
# ---------------------------------------------------------------------------

$slnx = New-Object System.Xml.XmlDocument
$slnx.PreserveWhitespace = $true
$slnx.Load($Solution)

$allReferences = @($slnx.SelectNodes('//Project[@Path]'))
$selected      = New-Object System.Collections.Generic.List[object]
$missing       = New-Object System.Collections.Generic.List[string]
$seen          = New-Object 'System.Collections.Generic.HashSet[string]'

foreach ($node in $allReferences) {
    $rel = $node.GetAttribute('Path')
    if ([string]::IsNullOrWhiteSpace($rel)) { continue }

    $abs = [System.IO.Path]::GetFullPath((Join-Path $solutionDir ($rel -replace '/', '\')))

    if (-not (Test-Path -LiteralPath $abs -PathType Leaf)) {
        $missing.Add($rel) | Out-Null
        continue
    }
    if (-not ((Split-Path -Leaf $abs) -like $ProjectFilter)) { continue }
    if (-not $seen.Add($abs.ToLowerInvariant())) { continue }   # duplicated entry

    $selected.Add([pscustomobject]@{ FullName = $abs; Relative = $rel })
}

Write-Host ("Solution entries: {0} project reference(s)" -f $allReferences.Count) -ForegroundColor Cyan
Write-Host ("Selected        : {0} project(s) matching '{1}'" -f $selected.Count, $ProjectFilter) -ForegroundColor Cyan
if ($missing.Count -gt 0) {
    Write-Host ("Missing files   : {0}" -f $missing.Count) -ForegroundColor DarkYellow
}
Write-Host ""

# ---------------------------------------------------------------------------
# Patch every selected project
# ---------------------------------------------------------------------------

$stats = @{
    scanned   = 0
    added     = 0
    updated   = 0
    created   = 0
    unchanged = 0
    failed    = 0
    rewritten = 0
}

$report = New-Object System.Collections.Generic.List[object]

foreach ($item in $selected) {
    $stats.scanned++
    $path = $item.FullName

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    try { $doc.Load($path) }
    catch {
        Write-Warning "Unparsable: $($item.Relative) -> $($_.Exception.Message)"
        $stats.failed++
        continue
    }

    if ($null -eq $doc.DocumentElement -or $doc.DocumentElement.LocalName -ne 'Project') {
        Write-Warning "Not an MSBuild project: $($item.Relative)"
        $stats.failed++
        continue
    }

    $configDeclared = Test-ConfigurationDeclared $doc
    $groups         = @(Find-TargetGroups $doc)
    $groupOp        = 'present'
    $conditionUsed  = ''

    if ($groups.Count -eq 0) {
        $groups        = @(New-ConditionalPropertyGroup $doc $TargetCondition)
        $groupOp       = 'created'
        $stats.created++
    }

    $valueOp = 'unchanged'

    foreach ($group in $groups) {
        $op = Set-Property $doc $group 'Optimize' $TargetValue (Get-Indents $group)

        if ($op -eq 'added')   { $valueOp = 'added' }
        if ($op -eq 'updated') { $valueOp = 'updated' }

        $conditionUsed = $group.GetAttribute('Condition')
    }

    switch ($groupOp) {
        'created' { $stats.added++ }
        default {
            if ($valueOp -eq 'added')   { $stats.added++ }
            if ($valueOp -eq 'updated') { $stats.updated++ }
        }
    }
    if ($groupOp -eq 'present' -and $valueOp -eq 'unchanged') { $stats.unchanged++ }

    $changed = ($groupOp -ne 'present') -or ($valueOp -ne 'unchanged')
    $written = $false

    if ($changed) {
        if (-not $WhatIf) { $written = Save-XmlPreserving $doc $path }
        if ($written) { $stats.rewritten++ }

        $label = if ($groupOp -eq 'created') { 'propertyGroup+optimize' } else { "optimize:$valueOp" }

        Write-Host ("  {0}" -f $item.Relative) -ForegroundColor Green
        Write-Host ("      {0}; condition: {1}" -f $label, $conditionUsed) -ForegroundColor DarkGray
    }
    else {
        Write-Verbose "ok (already optimized): $($item.Relative)"
    }

    $report.Add([ordered]@{
        path                    = $item.Relative
        configurationDeclared   = $configDeclared
        propertyGroup           = $groupOp
        optimize                = $valueOp
        optimizeValue           = $TargetValue
        condition               = $conditionUsed
        rewritten               = $written
    })
}

# ---------------------------------------------------------------------------
# Report + summary
# ---------------------------------------------------------------------------

if ($ReportFile) {
    $rows = @($report | ForEach-Object { [pscustomobject]$_ })
    $rows | Export-Csv -LiteralPath $ReportFile -NoTypeInformation -Encoding UTF8
    Write-Host ""
    Write-Host "Report          : $ReportFile" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=========== SUMMARY ===========" -ForegroundColor Cyan
Write-Host ("projects scanned            : {0}" -f $stats.scanned)
Write-Host ("  property group created    : {0}" -f $stats.created)  -ForegroundColor Green
Write-Host ("  Optimize added            : {0}" -f $stats.added)    -ForegroundColor Green
Write-Host ("  Optimize rewritten        : {0}" -f $stats.updated)  -ForegroundColor Green
Write-Host ("  already up to date        : {0}" -f $stats.unchanged) -ForegroundColor DarkGray
Write-Host ("files written               : {0}" -f $stats.rewritten)
Write-Host ("failed                      : {0}" -f $stats.failed) -ForegroundColor $(if ($stats.failed) { 'Red' } else { 'DarkGray' })

$notDeclared = @($report | Where-Object { -not $_.configurationDeclared }).Count
if ($notDeclared -gt 0) {
    Write-Host ""
    Write-Host ("note: {0} project(s) do not declare '{1}' in <Configurations>; the property group still applies to 'dotnet build -c {1}' but Visual Studio will not offer the configuration." -f $notDeclared, $Configuration) -ForegroundColor DarkYellow
}

if ($missing.Count -gt 0) {
    Write-Host ""
    Write-Host "missing project files referenced by the solution:" -ForegroundColor DarkYellow
    foreach ($m in $missing) { Write-Host ("  - {0}" -f $m) -ForegroundColor DarkYellow }
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "WhatIf mode -- no file was written. Re-run without -WhatIf to apply." -ForegroundColor Yellow
}
