Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Public Class GDIPlusImage : Inherits Microsoft.VisualBasic.Imaging.Image

    Public Overrides ReadOnly Property Size As Size
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return bitmap.Size
        End Get
    End Property

    ReadOnly bitmap As System.Drawing.Bitmap

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(img As System.Drawing.Image)
        bitmap = New System.Drawing.Bitmap(img)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(img As System.Drawing.Bitmap)
        bitmap = img
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(img As Microsoft.VisualBasic.Imaging.Bitmap)
        Call Me.New(DirectCast(img, Microsoft.VisualBasic.Imaging.Image))
    End Sub

    Sub New(img As Microsoft.VisualBasic.Imaging.Image)
        bitmap = New System.Drawing.Bitmap(
            width:=img.Width,
            height:=img.Height,
            format:=System.Drawing.Imaging.PixelFormat.Format32bppArgb
        )

        Using buffer As BitmapBuffer = BitmapBuffer.FromBitmap(bitmap)
            Array.ConstrainedCopy(
                img.GetMemoryBitmap.RawBuffer, Scan0,
                buffer.RawBuffer, Scan0,
                buffer.RawBuffer.Length
            )
        End Using
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Sub Save(s As IO.Stream, format As ImageFormats)
        Call bitmap.Save(s, format.GetFormat)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function GetMemoryBitmap() As BitmapImage.BitmapBuffer
        Return BitmapBuffer.FromBitmap(bitmap)
    End Function

    Protected Overrides Function ConvertToBitmapStream() As IO.MemoryStream
        Dim s As Stream = New MemoryStream
        Call Save(s, ImageFormats.Png)
        Call s.Flush()
        Return s
    End Function
End Class
