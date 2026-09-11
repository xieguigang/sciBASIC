<#
.SYNOPSIS
  Keep every "Microsoft.VisualBasic.*" SDK-style vbproj registered in nuget.slnx
  and wired up for the "nuget_release|x64" packaging configuration.

.DESCRIPTION
  Scans the repository for *.vbproj files and processes the ones that are

    * SDK style       -- <Project Sdk="Microsoft.NET.Sdk">
    * package worthy  -- <RootNamespace> starts with "Microsoft.VisualBasic"

  For every matching project the script performs five idempotent checks:

    1. Solution membership
       If the project is not referenced by nuget.slnx it is appended to the
       solution folder given by -SolutionFolder (created on demand), together
       with the "*|x64 -> x64" platform mapping used by all existing entries.
       Membership is decided on *resolved absolute paths*, so the different
       spellings already present in the file ("cuda/ILCuda/ILCuda.vbproj",
       "../../../../Microsoft.VisualBasic.Drawing/...") never cause duplicates.
       Immediately before a project is registered its <RootNamespace> is read
       again from disk and must still start with "Microsoft.VisualBasic";
       otherwise the project is reported as skipped and never added.

    2. Configuration declaration
       <Configurations> gains "nuget_release" and <Platforms> gains "x64" when
       missing, otherwise MSBuild/Visual Studio would not offer the config.

    3. Conditional property group
       <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='nuget_release|x64'">
       is created when absent, seeded with <PlatformTarget>x64</PlatformTarget>.

    4. Output path
       <OutputPath> inside that group is forced to the relative path pointing at
       the repository level ".nuget" directory, e.g. "../../.nuget/" for
       gr/avi/AVI.NET5.vbproj. Missing values are inserted, wrong ones rewritten
       (this also normalises sloppy values such as a missing trailing slash).

    5. Packaging properties
       The four properties that make MSBuild actually emit a .nupkg on build are
       upserted into the first unconditional PropertyGroup of every matching
       project (a project missing them silently produces no package):

           GeneratePackageOnBuild          = True
           PackageRequireLicenseAcceptance = True
           IncludeSymbols                  = True
           SymbolPackageFormat             = snupkg

       Properties that are already present with the target value are left alone;
       a present but different value is rewritten. Use -SkipPackagingProps to
       turn step 5 off.

  The script is safe to run repeatedly: a file is only rewritten when at least
  one real change was produced, and encoding (UTF-8 BOM or not), line endings
  (CRLF/LF) and the surrounding indentation are preserved. No backup copies are
  created -- use git to review or revert the result.

  Requires Windows PowerShell 5.1 or newer; no external modules.

.PARAMETER Root
  Repository root to scan. Defaults to the parent of the folder holding this
  script, i.e. the sciBASIC# repository root.

.PARAMETER Solution
  The .slnx solution file to synchronise. Defaults to "<Root>\nuget.slnx".

.PARAMETER SolutionFolder
  Solution folder that receives newly registered projects. Must start and end
  with '/'. Defaults to "/packages/new/" so the additions can be filed away by
  hand afterwards.

.PARAMETER NuGetDirName
  Name of the package output directory located at the repository root.
  Defaults to ".nuget".

.PARAMETER ProjectFilter
  Wildcard applied to the repository relative project path (forward slashes),
  e.g. '*Imaging*'. Defaults to '*' (everything).

.PARAMETER ExcludePattern
  Regular expression matched against the relative project path. Any project
  whose path contains one of these directory segments is ignored.

.PARAMETER ReportFile
  Optional CSV path receiving one row per matched project with the individual
  operations that were applied.

.PARAMETER SkipPackagingProps
  Do not touch GeneratePackageOnBuild / PackageRequireLicenseAcceptance /
  IncludeSymbols / SymbolPackageFormat.

.PARAMETER WhatIf
  Run every check and print the resulting operations without writing anything.

.EXAMPLE
  .\Sync-NuGetSolution.ps1 -WhatIf

  Preview mode: reports what would be changed, touches no file.

.EXAMPLE
  .\Sync-NuGetSolution.ps1

  Apply the changes to nuget.slnx and to the project files.

.EXAMPLE
  .\Sync-NuGetSolution.ps1 -ProjectFilter '*Imaging*' -ReportFile .\report.csv -Verbose

  Restrict the run to the imaging projects, dump a CSV report and trace every
  decision.
#>
[CmdletBinding()]
param(
    [string]$Root           = '',
    [string]$Solution       = '',
    [string]$SolutionFolder = '/packages/new/',
    [string]$NuGetDirName   = '.nuget',
    [string]$ProjectFilter  = '*',
    [string]$ExcludePattern = '(^|[\\/])(obj|bin|\.git|packages|package-install-cache|package-install-SetupFiles)([\\/]|$)',
    [string]$ReportFile     = '',
    [switch]$SkipPackagingProps,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

# Windows PowerShell does not populate $PSScriptRoot while binding the default
# values of an advanced (CmdletBinding) script, so the location aware defaults
# are resolved here instead of in the param block.
$scriptDir = $PSScriptRoot
if (-not $scriptDir) { $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $Root)      { $Root      = Split-Path -Parent $scriptDir }

# The MSBuild condition we are looking for / creating. Built by concatenation so
# that PowerShell never tries to expand the $(...) MSBuild placeholders.
$TargetConfiguration = 'nuget_release'
$TargetPlatform      = 'x64'
$TargetCondition     = "'" + '$(Configuration)|$(Platform)' + "'=='" + `
                       $TargetConfiguration + '|' + $TargetPlatform + "'"

# Defaults used when a project declares no <Configurations> / <Platforms> at all.
$DefaultConfigurations = 'Debug;Release'
$DefaultPlatforms      = 'AnyCPU'

# Only projects whose <RootNamespace> starts with this prefix are picked up, and
# only such projects are ever registered in the solution.
$NamespacePrefix = 'Microsoft.VisualBasic'

# Without these four properties MSBuild never emits a .nupkg for a project, so
# they are upserted into the first unconditional PropertyGroup of every project
# that is picked up. Values are forced, matching the convention already used by
# the packed projects in this repository (e.g. gr/avi/AVI.NET5.vbproj).
$PackagingProps = [ordered]@{
    GeneratePackageOnBuild          = 'True'
    PackageRequireLicenseAcceptance = 'True'
    IncludeSymbols                  = 'True'
    SymbolPackageFormat             = 'snupkg'
}

# ---------------------------------------------------------------------------
# Generic XML helpers (kept in sync with dev/NuGetMetadata/Apply-NuGetMetadata.ps1)
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

function Get-NodeText($parent, [string]$name) {
    $elem = Find-ChildElement $parent $name
    if ($null -eq $elem) { return $null }
    return $elem.InnerText.Trim()
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

function Add-PropertyIfMissing($doc, $group, [string]$name, [string]$value, $indents) {
    # Unlike Set-Property this never overwrites a value the author chose
    # deliberately -- used for PlatformTarget.
    if (Find-ChildElement $group $name) { return 'unchanged' }
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
# Path helpers
# ---------------------------------------------------------------------------

function Get-RelativePathUnix([string]$baseDir, [string]$targetPath) {
    # [System.IO.Path]::GetRelativePath is .NET Core only, and Uri.MakeRelativeUri
    # mangles the '%' characters present in directory names such as
    # "mime/text%html". Plain string arithmetic on the path segments is both
    # portable to PowerShell 5.1 and '%' safe.
    $base   = [System.IO.Path]::GetFullPath($baseDir.TrimEnd('\', '/') + '\').TrimEnd('\')
    $target = [System.IO.Path]::GetFullPath($targetPath).TrimEnd('\')

    $baseParts   = @($base   -split '\\')
    $targetParts = @($target -split '\\')

    $common = 0
    while ($common -lt $baseParts.Count -and $common -lt $targetParts.Count -and
           [string]::Equals($baseParts[$common], $targetParts[$common], [System.StringComparison]::OrdinalIgnoreCase)) {
        $common++
    }
    if ($common -eq 0) {
        # Different volume: a relative path does not exist, keep it absolute.
        return ($target -replace '\\', '/')
    }

    $segments = @()
    for ($i = $common; $i -lt $baseParts.Count;   $i++) { $segments += '..' }
    for ($i = $common; $i -lt $targetParts.Count; $i++) { $segments += $targetParts[$i] }
    if ($segments.Count -eq 0) { return '.' }
    return ($segments -join '/')
}

function Get-AbsolutePathKey([string]$baseDir, [string]$relativePath) {
    $combined = [System.IO.Path]::Combine($baseDir, ($relativePath -replace '/', '\'))
    return ([System.IO.Path]::GetFullPath($combined)).TrimEnd('\').ToLowerInvariant()
}

# ---------------------------------------------------------------------------
# vbproj helpers
# ---------------------------------------------------------------------------

function Find-RootNsPropertyGroup($doc) {
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -ne 'Element' -or $pg.LocalName -ne 'PropertyGroup') { continue }
        if ($pg.GetAttribute('Condition')) { continue }
        if (Find-ChildElement $pg 'RootNamespace') { return $pg }
    }
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -eq 'Element' -and $pg.LocalName -eq 'PropertyGroup' -and -not $pg.GetAttribute('Condition')) {
            return $pg
        }
    }
    return $null
}

function Get-RootNamespace($doc) {
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -ne 'Element' -or $pg.LocalName -ne 'PropertyGroup') { continue }
        $v = Get-NodeText $pg 'RootNamespace'
        if ($v) { return $v }
    }
    return $null
}

function Read-RootNamespaceFromDisk([string]$path) {
    # Independent re-read straight from disk. Used as a second line of defence
    # right before a project is registered in the solution, so a future change
    # to the filtering logic can never leak a foreign project into nuget.slnx.
    # $null is returned for anything unreadable -- the caller then declines.
    $probe = New-Object System.Xml.XmlDocument
    $probe.PreserveWhitespace = $true
    try { $probe.Load($path) }
    catch { return $null }
    if ([string]::IsNullOrEmpty($probe.DocumentElement.GetAttribute('Sdk'))) { return $null }
    return (Get-RootNamespace $probe)
}

function Test-RootNamespaceAllowed([string]$rootNs) {
    if (-not $rootNs) { return $false }
    return $rootNs.StartsWith($NamespacePrefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Find-PropertyOwnerGroup($doc, [string]$name) {
    # The property may live in any unconditional PropertyGroup, not necessarily
    # the one carrying RootNamespace. Patch it where the author put it.
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -ne 'Element' -or $pg.LocalName -ne 'PropertyGroup') { continue }
        if ($pg.GetAttribute('Condition')) { continue }
        if (Find-ChildElement $pg $name) { return $pg }
    }
    return $null
}

function Remove-DuplicateProperty($group, [string]$name) {
    # MSBuild honours the *last* declaration of a property, so a file carrying
    # two copies of e.g. PackageRequireLicenseAcceptance keeps the trailing one.
    # Drop every earlier copy so the value that is actually in effect is also the
    # only one a reader sees.
    $copies = @()
    foreach ($c in $group.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq $name) { $copies += $c }
    }
    if ($copies.Count -le 1) { return 0 }

    for ($i = 0; $i -lt $copies.Count - 1; $i++) {
        $node = $copies[$i]
        $prev = $node.PreviousSibling
        [void]$group.RemoveChild($node)
        if (Test-WhitespaceNode $prev) { [void]$group.RemoveChild($prev) }
    }
    return ($copies.Count - 1)
}

function Test-ShouldPatchPackagingProp($doc, [string]$name, [string]$value) {
    # Only an *unconditional* definition counts as "already declared": a value
    # that merely exists inside a conditional PropertyGroup (for instance the
    # nuget_release|x64 group) is not a project wide setting and must still be
    # added, otherwise every other configuration builds without a package.
    $owner = Find-PropertyOwnerGroup $doc $name
    if ($null -eq $owner) { return $true }
    $existing = Find-ChildElement $owner $name
    if ($null -eq $existing) { return $true }
    if ($existing.InnerText.Trim() -eq $value) { return $false }
    # Declared unconditionally, but with another value -- rewrite it in place.
    $mainGroup = Find-RootNsPropertyGroup $doc
    return ($null -ne $mainGroup) -and ($owner -eq $mainGroup)
}

function Test-TargetCondition([string]$condition) {
    if (-not $condition) { return $false }
    $a = $condition       -replace '\s', ''
    $b = $TargetCondition -replace '\s', ''
    return [string]::Equals($a, $b, [System.StringComparison]::OrdinalIgnoreCase)
}

function Find-TargetGroups($doc) {
    $found = @()
    foreach ($pg in $doc.DocumentElement.ChildNodes) {
        if ($pg.NodeType -ne 'Element' -or $pg.LocalName -ne 'PropertyGroup') { continue }
        if (Test-TargetCondition $pg.GetAttribute('Condition')) { $found += $pg }
    }
    return $found
}

function New-ConditionalPropertyGroup($doc, [string]$condition) {
    $root = $doc.DocumentElement

    # Anchor after the last top level PropertyGroup so the new group stays with
    # its peers and above the ItemGroup section.
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

function Set-SemicolonToken($doc, $group, [string]$name, [string]$token, [string]$seed, $indents) {
    $existing = Find-ChildElement $group $name
    if ($null -eq $existing) {
        $value = @($seed -split ';' | Where-Object { $_ }) + $token
        $elem  = $doc.CreateElement($name, $doc.DocumentElement.NamespaceURI)
        $elem.InnerText = ($value -join ';')
        Append-Element $doc $group $elem $indents
        return 'added'
    }
    $tokens = @($existing.InnerText -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    foreach ($t in $tokens) {
        if ([string]::Equals($t, $token, [System.StringComparison]::OrdinalIgnoreCase)) { return 'unchanged' }
    }
    $existing.InnerText = (($tokens + $token) -join ';')
    return 'updated'
}

# ---------------------------------------------------------------------------
# slnx helpers
# ---------------------------------------------------------------------------

function Find-SolutionFolder($doc, [string]$name) {
    foreach ($c in $doc.DocumentElement.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq 'Folder' -and $c.GetAttribute('Name') -eq $name) {
            return $c
        }
    }
    return $null
}

function New-SolutionFolder($doc, [string]$name) {
    $root = $doc.DocumentElement

    # Keep the file's layout: <Configurations>, then every <Folder>, then the
    # root level <Project> entries.
    $anchor = $null
    foreach ($c in $root.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq 'Folder') { $anchor = $c }
    }
    if ($null -eq $anchor) {
        foreach ($c in $root.ChildNodes) {
            if ($c.NodeType -eq 'Element' -and $c.LocalName -eq 'Configurations') { $anchor = $c }
        }
    }

    $folderIndent = '  '
    if ($null -ne $anchor -and (Test-WhitespaceNode $anchor.PreviousSibling)) {
        $probe = ($anchor.PreviousSibling.Value -split "`n")[-1]
        if ($probe) { $folderIndent = $probe }
    }

    $folder = $doc.CreateElement('Folder', $root.NamespaceURI)
    $folder.SetAttribute('Name', $name)
    [void]$folder.AppendChild($doc.CreateWhitespace("`n" + $folderIndent + '  '))

    if ($null -ne $anchor) {
        [void]$root.InsertAfter($folder, $anchor)
        [void]$root.InsertAfter($doc.CreateWhitespace("`n" + $folderIndent), $anchor)
    }
    else {
        $last = $root.LastChild
        while (Test-WhitespaceNode $last) {
            $prev = $last.PreviousSibling
            [void]$root.RemoveChild($last)
            $last = $prev
        }
        [void]$root.AppendChild($doc.CreateWhitespace("`n" + $folderIndent))
        [void]$root.AppendChild($folder)
        [void]$root.AppendChild($doc.CreateWhitespace("`n"))
    }
    return $folder
}

function Add-SolutionProject($doc, $folder, [string]$relativePath) {
    $indents = Get-Indents $folder
    $ns      = $doc.DocumentElement.NamespaceURI

    $project = $doc.CreateElement('Project', $ns)
    $project.SetAttribute('Path', $relativePath)

    # Every existing entry maps the solution x64 platform onto the project x64
    # platform; build type mappings are left to Visual Studio, which resolves
    # same named configurations automatically.
    $platform = $doc.CreateElement('Platform', $ns)
    $platform.SetAttribute('Solution', '*|' + $TargetPlatform)
    $platform.SetAttribute('Project', $TargetPlatform)

    [void]$project.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem + '  '))
    [void]$project.AppendChild($platform)
    [void]$project.AppendChild($doc.CreateWhitespace("`n" + $indents.Elem))

    Append-Element $doc $folder $project $indents
}

# ---------------------------------------------------------------------------
# Resolve inputs
# ---------------------------------------------------------------------------

if (-not (Test-Path -LiteralPath $Root)) { throw "Repository root not found: $Root" }
$Root = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\')

if (-not $Solution) { $Solution = Join-Path $Root 'nuget.slnx' }
if (-not (Test-Path -LiteralPath $Solution)) { throw "Solution file not found: $Solution" }
$Solution    = (Resolve-Path -LiteralPath $Solution).Path
$solutionDir = Split-Path -Parent $Solution

if ($SolutionFolder -notmatch '^/.*/$') {
    throw "SolutionFolder must start and end with '/', got: $SolutionFolder"
}

$nugetDir = Join-Path $Root $NuGetDirName

Write-Host "Repository root : $Root"          -ForegroundColor Cyan
Write-Host "Solution        : $Solution"      -ForegroundColor Cyan
Write-Host "Target folder   : $SolutionFolder" -ForegroundColor Cyan
Write-Host "Package output  : $nugetDir"      -ForegroundColor Cyan
Write-Host "Configuration   : $TargetConfiguration|$TargetPlatform" -ForegroundColor Cyan
Write-Host "Mode            : $(if ($WhatIf) { 'WhatIf (no write)' } else { 'APPLY' })" -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path -LiteralPath $nugetDir)) {
    Write-Warning "Package output directory does not exist yet: $nugetDir (it is created by the build)"
}

# ---------------------------------------------------------------------------
# Load the solution and index its projects by absolute path
# ---------------------------------------------------------------------------

$slnx = New-Object System.Xml.XmlDocument
$slnx.PreserveWhitespace = $true
$slnx.Load($Solution)

$registered = @{}
foreach ($node in $slnx.SelectNodes('//Project[@Path]')) {
    $key = Get-AbsolutePathKey $solutionDir $node.GetAttribute('Path')
    $registered[$key] = $node.GetAttribute('Path')
}
Write-Host ("Solution entries: {0} project reference(s)" -f $registered.Count) -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# Discover candidate project files
# ---------------------------------------------------------------------------

$allProjects = @(Get-ChildItem -LiteralPath $Root -Filter '*.vbproj' -Recurse -File | Where-Object {
        $rel = $_.FullName.Substring($Root.Length + 1)
        ($rel -notmatch $ExcludePattern) -and (($rel -replace '\\', '/') -like $ProjectFilter)
    } | Sort-Object FullName)

Write-Host ("Discovered      : {0} *.vbproj after filtering" -f $allProjects.Count) -ForegroundColor Cyan
Write-Host ""

# ---------------------------------------------------------------------------
# Process
# ---------------------------------------------------------------------------

$stats = @{
    scanned               = 0
    matched               = 0
    skippedLegacy         = 0
    skippedRootNamespace  = 0
    skippedByGuard        = 0
    failed                = 0
    slnxAdded             = 0
    projectsChanged       = 0
    configurationsPatched = 0
    platformsPatched      = 0
    groupsAdded           = 0
    platformTargetAdded   = 0
    outputPathAdded       = 0
    outputPathUpdated     = 0
    packagingAdded        = 0
    packagingUpdated      = 0
    packagingDupesRemoved = 0
}
$report        = New-Object System.Collections.Generic.List[object]
$slnxDirty     = $false
$newFolderMade = $false
$targetFolder  = $null

foreach ($file in $allProjects) {
    $stats.scanned++
    $relUnix = ($file.FullName.Substring($Root.Length + 1)) -replace '\\', '/'

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    try { $doc.Load($file.FullName) }
    catch {
        Write-Warning "Unparsable: $relUnix -> $($_.Exception.Message)"
        $stats.failed++
        continue
    }

    if ([string]::IsNullOrEmpty($doc.DocumentElement.GetAttribute('Sdk'))) {
        Write-Verbose "skip (legacy, no Sdk attribute): $relUnix"
        $stats.skippedLegacy++
        continue
    }
    if ($doc.DocumentElement.GetAttribute('Sdk') -ne 'Microsoft.NET.Sdk') {
        Write-Verbose "skip (Sdk=$($doc.DocumentElement.GetAttribute('Sdk'))): $relUnix"
        $stats.skippedLegacy++
        continue
    }

    $rootNs = Get-RootNamespace $doc
    if (-not $rootNs) {
        Write-Verbose "skip (no RootNamespace): $relUnix"
        $stats.skippedRootNamespace++
        continue
    }
    if (-not (Test-RootNamespaceAllowed $rootNs)) {
        Write-Verbose "skip (RootNamespace=$rootNs): $relUnix"
        $stats.skippedRootNamespace++
        continue
    }

    $stats.matched++
    $ops = @()

    # ---- 1. solution membership -------------------------------------------
    $slnxOp  = 'present'
    $absKey  = $file.FullName.TrimEnd('\').ToLowerInvariant()
    if (-not $registered.ContainsKey($absKey)) {
        # Second line of defence: re-read <RootNamespace> straight from disk and
        # refuse to register the project unless it really starts with the
        # required prefix. Anything unreadable is declined as well.
        $guardNs = Read-RootNamespaceFromDisk $file.FullName
        if (-not (Test-RootNamespaceAllowed $guardNs)) {
            Write-Host ("  {0}" -f $relUnix) -ForegroundColor Yellow
            Write-Host ("      slnx:NOT added -- RootNamespace " +
                        $(if ($guardNs) { "'$guardNs' does" } else { "is missing or unreadable and" }) +
                        " not start with '$NamespacePrefix' (skipped)") -ForegroundColor DarkYellow
            $stats.skippedByGuard++
            $slnxOp = 'skipped(namespace guard)'
        }
        else {
            $slnxRel = Get-RelativePathUnix $solutionDir $file.FullName
            if ($null -eq $targetFolder) {
                $targetFolder = Find-SolutionFolder $slnx $SolutionFolder
                if ($null -eq $targetFolder) {
                    $targetFolder  = New-SolutionFolder $slnx $SolutionFolder
                    $newFolderMade = $true
                }
            }
            Add-SolutionProject $slnx $targetFolder $slnxRel
            $registered[$absKey] = $slnxRel
            $slnxDirty = $true
            $stats.slnxAdded++
            $slnxOp = 'added'
            $ops += "slnx:added($slnxRel)"
        }
    }

    # ---- 2. configuration declarations ------------------------------------
    $mainGroup = Find-RootNsPropertyGroup $doc
    if ($null -eq $mainGroup) {
        Write-Warning "No unconditional PropertyGroup in $relUnix -- project settings skipped"
        $stats.failed++
        continue
    }

    $cfgGroup = Find-PropertyOwnerGroup $doc 'Configurations'
    if ($null -eq $cfgGroup) { $cfgGroup = $mainGroup }
    $cfgOp = Set-SemicolonToken $doc $cfgGroup 'Configurations' $TargetConfiguration `
                                $DefaultConfigurations (Get-Indents $cfgGroup)
    if ($cfgOp -ne 'unchanged') { $stats.configurationsPatched++; $ops += "Configurations:$cfgOp" }

    $platGroup = Find-PropertyOwnerGroup $doc 'Platforms'
    if ($null -eq $platGroup) { $platGroup = $mainGroup }
    $platOp = Set-SemicolonToken $doc $platGroup 'Platforms' $TargetPlatform `
                                 $DefaultPlatforms (Get-Indents $platGroup)
    if ($platOp -ne 'unchanged') { $stats.platformsPatched++; $ops += "Platforms:$platOp" }

    # ---- 3. packaging properties ------------------------------------------
    # Without GeneratePackageOnBuild the build silently produces no .nupkg, so
    # these belong to the same "make this project packable" check as step 4.
    $packIndents = Get-Indents $mainGroup
    if (-not $SkipPackagingProps) {
        foreach ($name in $PackagingProps.Keys) {
            $value = $PackagingProps[$name]
            $op    = 'present'
            if (Test-ShouldPatchPackagingProp $doc $name $value) {
                # Collapse repeated declarations first: the surviving (last) copy
                # is then compared and patched, leaving exactly one entry.
                if ((Remove-DuplicateProperty $mainGroup $name) -gt 0) {
                    $stats.packagingDupesRemoved++
                    $ops += "$name`:de-duplicated"
                }
                $op = Set-Property $doc $mainGroup $name $value $packIndents
            }
            if ($op -eq 'added')   { $stats.packagingAdded++ }
            if ($op -eq 'updated') { $stats.packagingUpdated++ }
            if ($op -ne 'present' -and $op -ne 'unchanged') { $ops += "$name`:$op" }
        }
    }

    # ---- 4. conditional property group ------------------------------------
    $groups  = @(Find-TargetGroups $doc)
    $groupOp = 'present'
    if ($groups.Count -eq 0) {
        $groups  = @(New-ConditionalPropertyGroup $doc $TargetCondition)
        $groupOp = 'added'
        $stats.groupsAdded++
        $ops += 'propertyGroup:added'
    }

    # ---- 4. PlatformTarget + OutputPath ------------------------------------
    $outputPath = (Get-RelativePathUnix (Split-Path -Parent $file.FullName) $nugetDir)
    if (-not $outputPath.EndsWith('/')) { $outputPath += '/' }

    $pathOps = @()
    foreach ($group in $groups) {
        $indents = Get-Indents $group

        $ptOp = Add-PropertyIfMissing $doc $group 'PlatformTarget' $TargetPlatform $indents
        if ($ptOp -ne 'unchanged') { $stats.platformTargetAdded++; $ops += "PlatformTarget:$ptOp" }

        $opOp = Set-Property $doc $group 'OutputPath' $outputPath $indents
        if ($opOp -eq 'added')   { $stats.outputPathAdded++ }
        if ($opOp -eq 'updated') { $stats.outputPathUpdated++ }
        if ($opOp -ne 'unchanged') { $ops += "OutputPath:$opOp($outputPath)" }
        $pathOps += $opOp
    }

    # ---- persist -----------------------------------------------------------
    $projectOps = @($ops | Where-Object { $_ -notlike 'slnx:*' })
    $written    = $false
    if ($projectOps.Count -gt 0) {
        $stats.projectsChanged++
        if (-not $WhatIf) { $written = Save-XmlPreserving $doc $file.FullName }
    }

    if ($ops.Count -gt 0) {
        Write-Host ("  {0}" -f $relUnix) -ForegroundColor Green
        Write-Host ("      " + ($ops -join ', ')) -ForegroundColor DarkGray
    }
    else {
        Write-Verbose "ok (nothing to do): $relUnix"
    }

    $report.Add([ordered]@{
        path           = $relUnix
        rootNamespace  = $rootNs
        solution       = $slnxOp
        configurations = $cfgOp
        platforms      = $platOp
        propertyGroup  = $groupOp
        outputPath     = ($pathOps -join ';')
        outputPathValue= $outputPath
        rewritten      = $written
    })
}

# ---------------------------------------------------------------------------
# Persist the solution once
# ---------------------------------------------------------------------------

if ($slnxDirty) {
    $slnxName  = Split-Path -Leaf $Solution
    $folderNote = ''
    if ($newFolderMade) { $folderNote = " (+ solution folder $SolutionFolder)" }

    Write-Host ""
    if ($WhatIf) {
        Write-Host ("  {0} -- would register {1} project(s){2}" -f $slnxName, $stats.slnxAdded, $folderNote) -ForegroundColor Green
    }
    else {
        [void](Save-XmlPreserving $slnx $Solution)
        Write-Host ("  {0} -- registered {1} project(s){2}" -f $slnxName, $stats.slnxAdded, $folderNote) -ForegroundColor Green
    }
}

if ($ReportFile) {
    $rows = @($report | ForEach-Object { [pscustomobject]$_ })
    $rows | Export-Csv -LiteralPath $ReportFile -NoTypeInformation -Encoding UTF8
    Write-Host ""
    Write-Host "Report          : $ReportFile" -ForegroundColor Cyan
}

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------

Write-Host ""
Write-Host "=========== SUMMARY ===========" -ForegroundColor Cyan
Write-Host ("vbproj scanned          : {0}" -f $stats.scanned)
Write-Host ("matched (Microsoft.VB*) : {0}" -f $stats.matched)
Write-Host ("added to solution       : {0}" -f $stats.slnxAdded)          -ForegroundColor Green
Write-Host ("projects modified       : {0}" -f $stats.projectsChanged)    -ForegroundColor Green
Write-Host ("  Configurations patched: {0}" -f $stats.configurationsPatched)
Write-Host ("  Platforms patched     : {0}" -f $stats.platformsPatched)
Write-Host ("  PropertyGroups added  : {0}" -f $stats.groupsAdded)
Write-Host ("  PlatformTarget added  : {0}" -f $stats.platformTargetAdded)
Write-Host ("  OutputPath added      : {0}" -f $stats.outputPathAdded)
Write-Host ("  OutputPath rewritten  : {0}" -f $stats.outputPathUpdated)
Write-Host ("  Packaging props added : {0}" -f $stats.packagingAdded)
Write-Host ("  Packaging props fixed : {0}" -f $stats.packagingUpdated)
Write-Host ("  Packaging dupes merged: {0}" -f $stats.packagingDupesRemoved)
Write-Host ("skipped (non SDK style) : {0}" -f $stats.skippedLegacy)      -ForegroundColor DarkGray
Write-Host ("skipped (RootNamespace) : {0}" -f $stats.skippedRootNamespace) -ForegroundColor DarkGray
Write-Host ("blocked by ns guard     : {0}" -f $stats.skippedByGuard)     -ForegroundColor $(if ($stats.skippedByGuard) { 'Yellow' } else { 'DarkGray' })
Write-Host ("failed                  : {0}" -f $stats.failed)             -ForegroundColor $(if ($stats.failed) { 'Red' } else { 'DarkGray' })
if ($WhatIf) {
    Write-Host ""
    Write-Host "WhatIf mode -- no file was written. Re-run without -WhatIf to apply." -ForegroundColor Yellow
}
