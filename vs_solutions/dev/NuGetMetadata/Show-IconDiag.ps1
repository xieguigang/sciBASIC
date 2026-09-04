[CmdletBinding()]
param([string]$OutFile = (Join-Path $PSScriptRoot 'projects.json'))

$j = Get-Content $OutFile -Raw | ConvertFrom-Json

Write-Host "total=$($j.totalVbproj)  matched=$($j.matchedCount)  library=$($j.libraryCount)  excluded=$($j.excludedCount)  iconFix=$($j.iconFixCount)"
Write-Host "projects array size = $($j.projects.Count)"

$hasIcon   = @($j.projects | Where-Object { $_.hasPackageIcon  -eq $true }).Count
$hasItem   = @($j.projects | Where-Object { $_.existingIconItem }).Count
$needsFix  = @($j.projects | Where-Object { $_.needsIconFix -eq $true }).Count

Write-Host "hasPackageIcon=true : $hasIcon"
Write-Host "existingIconItem   : $hasItem"
Write-Host "needsIconFix=true  : $needsFix"

Write-Host ""
Write-Host "--- hasPackageIcon=true AND existingIconItem empty ---"
foreach ($p in $j.projects) {
    if ($p.hasPackageIcon -and -not $p.existingIconItem) {
        Write-Host ("  " + $p.path + "  depth=" + $p.depth + "  lib=" + $p.isLibrary)
    }
}
