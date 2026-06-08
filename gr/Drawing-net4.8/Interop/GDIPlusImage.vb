Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging

Namespace Interop

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
            bitmap = CTypeGDIPlusImage(img)
        End Sub

        Public Shared Function CTypeGDIPlusImage(img As Microsoft.VisualBasic.Imaging.Image) As System.Drawing.Bitmap
            Dim bitmap As New System.Drawing.Bitmap(
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

            Return bitmap
        End Function

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

        Public Function CreateCanvas2D(direct_access As Boolean) As IGraphics
            Return bitmap.CreateCanvas2D(direct_access)
        End Function

        Public Shared Narrowing Operator CType(img As GDIPlusImage) As System.Drawing.Bitmap
            Return img.bitmap
        End Operator

        Public Shared Narrowing Operator CType(img As GDIPlusImage) As System.Drawing.Image
            Return img.bitmap
        End Operator

        Public Shared Function LoadImage(path As String, Optional base64 As Boolean = False, Optional throwEx As Boolean = True) As System.Drawing.Image
            Return path.LoadImage(base64, throwEx)
        End Function
    End Class
End Namespace