Imports System.Runtime.InteropServices

Namespace ImageFormat

    <StructLayout(LayoutKind.Explicit, Pack:=1)>
    Friend Structure BitmapFileHeader

        <FieldOffset(0)>
        Public Type As UShort
        <FieldOffset(2)>
        Public Size As UInteger
        <FieldOffset(6)>
        Public Reserved1 As UShort
        <FieldOffset(8)>
        Public Reserved2 As UShort
        <FieldOffset(10)>
        Public OffBits As UInteger

        Public Shared Function GetDefault() As BitmapFileHeader
            Return New BitmapFileHeader With {
                .Type = &H4D42,
                .Size = 0,
                .Reserved1 = 0,
                .Reserved2 = 0,
                .OffBits = 0
            }
        End Function

    End Structure
End Namespace