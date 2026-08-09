$paths = @(
    'C:\Users\Administrator\Downloads\Visium_HD_6p5mm_Rat_Liver_molecule_info.h5',
    'C:\Users\Administrator\Downloads\Visium_HD_6p5mm_Rat_Liver_feature_slice.h5'
)
foreach ($p in $paths) {
    $fs = [System.IO.File]::OpenRead($p)
    $hdr = New-Object byte[] 48
    $null = $fs.Read($hdr, 0, 48)
    # superblock v0 offsets: version@8, sizeOfOffsets@10, sizeOfLengths@11,
    # rootGroupSymbolTableAddress @ 36 (for O=8): actually base@20(8), rootGroupST@20+8=28? need compute.
    $soo = $hdr[10]
    $ver = $hdr[8]
    # base address @ 20 (O bytes), root group symbol table address @ 20 + O
    $rootOff = 20 + $soo
    $rootAddr = 0L
    for ($i = 0; $i -lt $soo; $i++) { $rootAddr = ($rootAddr -shl 8) -bor $hdr[$rootOff + $i] }
    # read 4 bytes at rootAddr
    $fs.Seek($rootAddr, [System.IO.SeekOrigin]::Begin) | Out-Null
    $sig = New-Object byte[] 4
    $null = $fs.Read($sig, 0, 4)
    $fs.Close()
    $sigStr = [System.Text.Encoding]::ASCII.GetString($sig)
    Write-Output ("FILE=" + [System.IO.Path]::GetFileName($p))
    Write-Output ("  version=$ver sizeOfOffsets=$soo rootGroupSTaddr=$rootAddr sigAtRoot=$sigStr")
    Write-Output ("  hdr(0..15)= " + (($hdr[0..15] | ForEach-Object { $_.ToString('x2') }) -join ' '))
}
