Imports System.Runtime.InteropServices

Namespace Imaging.BitmapImage.StreamWriter

    <StructLayout(LayoutKind.Explicit, Pack:=1)>
    Friend Structure BitmapInfoHeader

        <FieldOffset(0)>
        Public Size As UInteger
        <FieldOffset(4)>
        Public Width As Integer
        <FieldOffset(8)>
        Public Height As Integer
        <FieldOffset(12)>
        Public Planes As Short
        <FieldOffset(14)>
        Public BitCount As UShort
        <FieldOffset(16)>
        Public Compression As UInteger
        <FieldOffset(20)>
        Public SizeImage As UInteger
        <FieldOffset(24)>
        Public XPixPerMeter As Integer
        <FieldOffset(28)>
        Public YPixPerMeter As Integer
        <FieldOffset(32)>
        Public ClrUsed As UInteger
        <FieldOffset(36)>
        Public CirImportant As UInteger

        Public Shared Function GetDefault() As BitmapInfoHeader
            Return New BitmapInfoHeader With {
                .Size = 40,
                .Width = 0,
                .Height = 0,
                .Planes = 1,
                .BitCount = BitmapColorBit.Bit1,
                .Compression = BitmapCompression.Rgb,
                .SizeImage = 0,
                .XPixPerMeter = 0,
                .YPixPerMeter = 0,
                .ClrUsed = 0,
                .CirImportant = 0
            }
        End Function

    End Structure
End Namespace