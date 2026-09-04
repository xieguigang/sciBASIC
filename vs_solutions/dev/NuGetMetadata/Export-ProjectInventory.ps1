<#
.SYNOPSIS
  Scan every *.vbproj under the repository and emit projects.json inventory.
.DESCRIPTION
  Read-only w.r.t. project files; only writes the inventory report.
#>
[CmdletBinding()]
param(
    [string]$Root    = (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))),
    [string]$OutFile = (Join-Path $PSScriptRoot 'projects.json')
)

$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path $Root).Path.TrimEnd('\')

$TestPathSegments = @(
    'test', 'tests', 'test5', 'testing', 'demo', 'demos',
    'example', 'examples', '_smoketest', 'smoketest', 'sanitycheck',
    'biff_class_test', 'verify_escape', 'chartingbase.test',
    'markdownrender.tests', 'layoutsanitycheck'
)

$MetaProps = @(
    'Title', 'Description', 'PackageTags', 'AssemblyTitle',
    'Authors', 'Company', 'Copyright', 'PackageIcon',
    'PackageProjectUrl', 'RepositoryUrl', 'RepositoryType',
    'PackageLicenseExpression', 'PackageLicenseFile', 'PackageReadmeFile',
    'GeneratePackageOnBuild', 'IsPackable', 'Version', 'PackageId'
)

function Get-NodeText($ParentNode, [string]$Name) {
    if ($null -eq $ParentNode) { return $null }
    foreach ($c in $ParentNode.ChildNodes) {
        if ($c.NodeType -eq 'Element' -and $c.LocalName -eq $Name) { return $c.InnerText.Trim() }
    }
    return $null
}

function Test-IsTestPath([string]$RelativePath) {
    foreach ($seg in ($RelativePath -split '[\\/]')) {
        if ($TestPathSegments -contains $seg.ToLowerInvariant()) { return $true }
    }
    return $false
}

function Get-IconInclude($Doc) {
    # SDK-style projects carry no xmlns; legacy ones use the MSBuild 2003 ns.
    # Walk every element and match on LocalName so both forms work.
    $stack = New-Object System.Collections.Generic.Stack[System.Xml.XmlNode]
    $stack.Push($Doc.DocumentElement)
    while ($stack.Count -gt 0) {
        $node = $stack.Pop()
        foreach ($c in $node.ChildNodes) {
            if ($c.NodeType -ne 'Element') { continue }
            if (@('None', 'Content', 'Resource') -contains $c.LocalName) {
                $inc = $c.GetAttribute('Include')
                if ($inc -and $inc -match 'logo-knot\.png$') { return $inc }
            }
            $stack.Push($c)
        }
    }
    return $null
}

Write-Host "Repository root : $Root" -ForegroundColor Cyan

$allProjects = Get-ChildItem -Path $Root -Filter '*.vbproj' -Recurse -File | Where-Object {
    $rel = $_.FullName.Substring($Root.Length + 1)
    $rel -notmatch '(^|[\\/])(obj|bin|\.git|packages)([\\/]|$)'
}
Write-Host "Discovered      : $($allProjects.Count) *.vbproj" -ForegroundColor Cyan

$records = New-Object System.Collections.Generic.List[object]
$skipped = New-Object System.Collections.Generic.List[object]
$matchCount = 0

foreach ($file in $allProjects) {
    $relUnix = ($file.FullName.Substring($Root.Length + 1)) -replace '\\', '/'

    $doc = New-Object System.Xml.XmlDocument
    $doc.PreserveWhitespace = $true
    try { $doc.Load($file.FullName) }
    catch {
        Write-Warning "Unparsable: $relUnix -> $($_.Exception.Message)"
        $skipped.Add([ordered]@{ path = $relUnix; reason = 'XML parse failure' })
        continue
    }

    $projectNode = $doc.DocumentElement
    $isLegacy    = [string]::IsNullOrEmpty($projectNode.GetAttribute('Sdk'))

    $rootNs = $null
    $firstGroup = $null
    foreach ($pg in $projectNode.ChildNodes) {
        if ($pg.LocalName -ne 'PropertyGroup') { continue }
        if ($null -eq $firstGroup) { $firstGroup = $pg }
        $v = Get-NodeText $pg 'RootNamespace'
        if ($v -and -not $rootNs) { $rootNs = $v }
    }

    if (-not $rootNs) { $skipped.Add([ordered]@{ path = $relUnix; reason = 'no RootNamespace' }); continue }
    if (-not $rootNs.StartsWith('Microsoft.VisualBasic')) {
        $skipped.Add([ordered]@{ path = $relUnix; reason = "RootNamespace=$rootNs" }); continue
    }
    $matchCount++

    $props = @{}
    foreach ($n in $MetaProps) { $props[$n] = $null }
    $outputType = 'Library'

    foreach ($pg in $projectNode.ChildNodes) {
        if ($pg.LocalName -ne 'PropertyGroup') { continue }
        foreach ($c in $pg.ChildNodes) {
            if ($c.NodeType -ne 'Element') { continue }
            if ($MetaProps -contains $c.LocalName) {
                if ([string]::IsNullOrEmpty($props[$c.LocalName])) { $props[$c.LocalName] = $c.InnerText.Trim() }
            }
            elseif ($c.LocalName -eq 'OutputType') { $outputType = $c.InnerText.Trim() }
        }
    }

    $asmName = Get-NodeText $firstGroup 'AssemblyName'
    if (-not $asmName) { $asmName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name) }

    $tfm = Get-NodeText $firstGroup 'TargetFrameworks'
    if (-not $tfm) { $tfm = Get-NodeText $firstGroup 'TargetFramework' }
    if (-not $tfm) { $tfm = Get-NodeText $firstGroup 'TargetFrameworkVersion' }

    $pathLooksTest = Test-IsTestPath $relUnix
    $isExe         = $outputType -ieq 'Exe' -or $outputType -ieq 'WinExe'
    $notPackable   = $props['IsPackable'] -ieq 'false'
    $isLibrary     = (-not $pathLooksTest) -and (-not $isExe) -and (-not $notPackable)

    $depth       = ($relUnix -split '/').Count - 1
    $iconRelPath = ('..\' * $depth) + 'vs_solutions\logo-knot.png'
    $existingIcon = Get-IconInclude $doc

    $reasons = @()
    if ($pathLooksTest) { $reasons += 'test/demo path' }
    if ($isExe)         { $reasons += "OutputType=$outputType" }
    if ($notPackable)   { $reasons += 'IsPackable=false' }

    $records.Add([ordered]@{
        path             = $relUnix
        depth            = $depth
        rootNamespace    = $rootNs
        assemblyName     = $asmName
        targetFramework  = $tfm
        outputType       = $outputType
        isLibrary        = $isLibrary
        isLegacy         = $isLegacy
        excludeReason    = ($reasons -join '; ')
        iconRelPath      = $iconRelPath
        existingIconItem = $existingIcon
        hasPackageIcon   = [bool]$props['PackageIcon']
        needsIconFix     = ([bool]$props['PackageIcon']) -and (-not $existingIcon)
        needsIconAdd     = $isLibrary -and (-not $existingIcon) -and (-not $isLegacy)
        title            = ''
        description      = ''
        packageTags      = ''
        assemblyTitle    = ''
        notes            = ''
        snapshot         = $props
    })
}

$payload = [ordered]@{
    generatedAt    = (Get-Date).ToString('o')
    root           = $Root
    totalVbproj    = $allProjects.Count
    matchedCount   = $matchCount
    libraryCount   = @($records | Where-Object { $_.isLibrary }).Count
    excludedCount  = @($records | Where-Object { -not $_.isLibrary }).Count
    legacyCount    = @($records | Where-Object { $_.isLegacy }).Count
    iconFixCount   = @($records | Where-Object { $_.needsIconFix }).Count
    iconAddCount   = @($records | Where-Object { $_.needsIconAdd }).Count
    projects       = $records
    skipped        = $skipped
}

$payload | ConvertTo-Json -Depth 8 | Set-Content -Path $OutFile -Encoding UTF8

Write-Host "Matched         : $matchCount (RootNamespace starts with Microsoft.VisualBasic)" -ForegroundColor Green
Write-Host "  library       : $($payload.libraryCount)" -ForegroundColor Green
Write-Host "  excluded      : $($payload.excludedCount)" -ForegroundColor Yellow
Write-Host "  legacy        : $($payload.legacyCount)" -ForegroundColor Yellow
Write-Host "  needsIconFix  : $($payload.iconFixCount)" -ForegroundColor Yellow
Write-Host "  needsIconAdd  : $($payload.iconAddCount)" -ForegroundColor Yellow
Write-Host "Inventory       : $OutFile" -ForegroundColor Cyan
