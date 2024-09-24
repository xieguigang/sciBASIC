Imports Microsoft.VisualBasic.Drawing.ImageFormat

Module ImageUtils

    Friend Function GetImageWidthSize(width As Integer, colorBit As BitmapColorBit) As Integer
        Dim bitCount = width
        Select Case colorBit
            Case BitmapColorBit.Bit1
                bitCount *= 1
            Case BitmapColorBit.Bit4
                bitCount *= 4
            Case BitmapColorBit.Bit8
                bitCount *= 8
            Case BitmapColorBit.Bit24
                bitCount *= 24
            Case BitmapColorBit.Bit32
                bitCount *= 32
            Case Else
                Throw New InvalidOperationException($"Invalid color bit. : {colorBit}")
        End Select
        ' 8 bit
        Dim bitMod = bitCount Mod 8
        Dim bitTotal = bitCount + If(bitMod = 0, 0, 8 - bitMod)
        ' 4 byte
        Dim byteCount = bitTotal / 8
        Dim byteMod = byteCount Mod 4
        Return byteCount + If(byteMod = 0, 0, 4 - byteMod)
    End Function
End Module
