$paths = @(
    'C:\Users\Administrator\Downloads\Visium_HD_6p5mm_Rat_Liver_molecule_info.h5',
    'C:\Users\Administrator\Downloads\Visium_HD_6p5mm_Rat_Liver_feature_slice.h5'
)
foreach ($p in $paths) {
    $fs = [System.IO.File]::OpenRead($p)
    $b = New-Object byte[] 1
    function ReadByteAt([long]$pos) {
        $fs.Seek($pos, [System.IO.SeekOrigin]::Begin) | Out-Null
        $null = $fs.Read($b, 0, 1)
        return [int]$b[0]
    }
    $soo = ReadByteAt 10
    $sol = ReadByteAt 11
    $rootOff = 20 + $soo
    $rootAddr = 0L
    for ($i = 0; $i -lt $soo; $i++) { $rootAddr = ($rootAddr -shl 8) -bor [int](ReadByteAt ($rootOff + $i)) }
    # signature at rootAddr
    $sigBytes = New-Object byte[] 8
    $fs.Seek($rootAddr, [System.IO.SeekOrigin]::Begin) | Out-Null
    $null = $fs.Read($sigBytes, 0, 8)
    $fs.Close()
    $sigStr = [System.Text.Encoding]::ASCII.GetString($sigBytes)
    Write-Output ("FILE=" + [System.IO.Path]::GetFileName($p))
    Write-Output ("  version=" + (ReadByteAt 8) + " soo=" + $soo + " sol=" + $sol + " rootGroupSTaddr=" + $rootAddr + " sigAtRoot=" + $sigStr)
}
