$log = Join-Path $PSScriptRoot "build_log.txt"
$lines = Select-String -Path $log -Pattern ": warning "
$grouped = @{}
foreach ($l in $lines) {
    if ($l.Line -match ': warning ([A-Za-z0-9]+):') {
        $code = $matches[1]
        if (-not $grouped.ContainsKey($code)) { $grouped[$code] = 0 }
        $grouped[$code]++
    }
}
$grouped.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { "{0,-12} {1}" -f $_.Value, $_.Key }
