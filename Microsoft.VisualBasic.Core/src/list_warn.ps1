$log = Join-Path $PSScriptRoot "build_log.txt"
$lines = Select-String -Path $log -Pattern ': warning ([A-Za-z0-9]+):'
$rows = @{}
foreach ($l in $lines) {
    # line format: path(line,col): warning CODE: message
    $m = [regex]::Match($l.Line, '^(.*?)\(\d+,\d+\): warning ([A-Za-z0-9]+):')
    if ($m.Success) {
        $file = Split-Path $m.Groups[1].Value -Leaf
        $code = $m.Groups[2].Value
        $key = "$code`t$file"
        if (-not $rows.ContainsKey($key)) { $rows[$key] = 0 }
        $rows[$key]++
    }
}
$rows.GetEnumerator() | Sort-Object Key | ForEach-Object { "{0,-12} {1,4}  {2}" -f ($_.Key -split "`t")[0], $_.Value, ($_.Key -split "`t")[1] }
