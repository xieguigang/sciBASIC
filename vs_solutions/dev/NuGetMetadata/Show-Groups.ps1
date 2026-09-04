[CmdletBinding()]
param(
    [string]$OutFile = (Join-Path $PSScriptRoot 'projects.json'),
    [string]$Group = 'all'
)

$j = Get-Content $OutFile -Raw | ConvertFrom-Json
$libs = @($j.projects | Where-Object { $_.isLibrary })

$map = @{
    'mime'   = 'mime/'
    'nlp'    = 'nlp/'
    'gr'     = 'gr/'
    'vs'     = 'vs_solutions/'
    'www'    = 'www/'
    'core'   = 'Microsoft.VisualBasic.Core/'
    'data'   = 'Data/'
    'ds'     = 'Data_science/'
}

foreach ($p in $libs) {
    $hit = 'other'
    foreach ($k in $map.Keys) {
        if ($p.path.StartsWith($map[$k])) { $hit = $k; break }
    }
    if ($Group -ne 'all' -and $hit -ne $Group) { continue }
    Write-Host ("[" + $hit + "] " + $p.path + "   ns=" + $p.rootNamespace + "  asm=" + $p.assemblyName)
}
