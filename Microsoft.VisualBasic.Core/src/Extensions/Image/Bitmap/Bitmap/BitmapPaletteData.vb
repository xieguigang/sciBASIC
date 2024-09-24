Imports System.Runtime.InteropServices

Namespace ImageFormat

    <StructLayout(LayoutKind.Explicit, Pack:=1)>
    Friend Structure BitmapPaletteData

        <FieldOffset(0)>
        Public Red As Byte
        <FieldOffset(1)>
        Public Green As Byte
        <FieldOffset(2)>
        Public Blue As Byte
        <FieldOffset(3)>
        Public Reserve As Byte

        Public Sub New(red As Byte, green As Byte, blue As Byte)
            Me.Red = red
            Me.Green = green
            Me.Blue = blue
        End Sub
    End Structure
End Namespace