Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Public Class GDIPlusImage : Inherits Microsoft.VisualBasic.Imaging.Image

    Public Overrides ReadOnly Property Size As Size
        Get
            Return bitmap.Size
        End Get
    End Property

    Dim bitmap As System.Drawing.Bitmap

    Public Overrides Sub Save(s As IO.Stream, format As ImageFormats)
        Call bitmap.Save(s, format.GetFormat)
    End Sub

    Public Overrides Function GetMemoryBitmap() As BitmapImage.BitmapBuffer
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function ConvertToBitmapStream() As IO.MemoryStream
        Dim s As Stream = New MemoryStream
        Call Save(s, ImageFormats.Png)
        Call s.Flush()
        Return s
    End Function
End Class
