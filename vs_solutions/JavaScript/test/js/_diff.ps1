# Dev helper: run a .js through both Node.js and LiteJs.Runner and show a
# normalized diff. LiteJs's JSON.stringify emits "key": value (space after
# colon) while Node is compact; we collapse ": " -> ":" so the diff reflects
# actual VALUE differences, not serialization cosmetics.
param(
    [Parameter(Mandatory=$true)] [string] $Script
)

$ErrorActionPreference = 'Continue'
$runner = "..\..\LiteJs.Runner\bin\Debug\net10.0\LiteJs.Runner.dll"

function Normalize($s) {
    # collapse the JSON "key": value space, then unify line endings
    return ($s -replace ': ', ':') -replace "`r`n", "`n"
}

$nodeOut = & node $Script 2>&1 | Out-String
$liteOut = & dotnet $runner $Script 2>&1 | Out-String

$n = Normalize $nodeOut
$l = Normalize $liteOut

Write-Host "================ NODE (baseline) ================"
Write-Host $nodeOut
Write-Host "================ LiteJs ========================"
Write-Host $liteOut
Write-Host "================ DIFF (normalized) ============="

$diffs = 0
$na = $n.Split("`n")
$la = $l.Split("`n")
$max = [Math]::Max($na.Length, $la.Length)
for ($i = 0; $i -lt $max; $i++) {
    $a = if ($i -lt $na.Length) { $na[$i] } else { "<none>" }
    $b = if ($i -lt $la.Length) { $la[$i] } else { "<none>" }
    if ($a -ne $b) {
        $diffs++
        Write-Host ("LINE {0}  NODE : {1}" -f ($i+1), $a)
        Write-Host ("LINE {0}  LITE : {1}" -f ($i+1), $b)
    }
}
if ($diffs -eq 0) { Write-Host "NO DIFFERENCES (after normalization)" }
else { Write-Host ("{0} differing line(s)" -f $diffs) }
