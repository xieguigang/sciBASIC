[CmdletBinding()]
param([string]$OutFile = (Join-Path $PSScriptRoot 'projects.json'))

$j = Get-Content $OutFile -Raw | ConvertFrom-Json

Write-Host "=== EXCLUDED (not library) ===" -ForegroundColor Yellow
foreach ($p in $j.projects) {
    if (-not $p.isLibrary) { Write-Host ("  " + $p.path + "   [" + $p.excludeReason + "]") }
}

Write-Host ""
Write-Host "=== NEEDS ICON FIX ($($j.iconFixCount)) ===" -ForegroundColor Yellow
foreach ($p in $j.projects) {
    if ($p.needsIconFix) { Write-Host ("  " + $p.path + "   depth=" + $p.depth + "   -> " + $p.iconRelPath) }
}

Write-Host ""
Write-Host "=== LEGACY PROJECTS ===" -ForegroundColor Yellow
foreach ($p in $j.projects) {
    if ($p.isLegacy) { Write-Host ("  " + $p.path) }
}

Write-Host ""
Write-Host "=== ALL LIBRARY PROJECTS ($($j.libraryCount)) ===" -ForegroundColor Cyan
foreach ($p in $j.projects) {
    if ($p.isLibrary) {
        Write-Host ("  " + $p.path)
        Write-Host ("      ns=" + $p.rootNamespace + "   asm=" + $p.assemblyName + "   tfm=" + $p.targetFramework + "   legacy=" + $p.isLegacy)
    }
}
