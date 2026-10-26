#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Jpeg

Namespace Imaging

    ''' <summary>
    ''' a jpeg image object that wraps a <see cref="BitmapBuffer"/> decoded from /
    ''' encoded into a baseline jpeg byte stream.
    ''' </summary>
    Public Class JpegImage : Inherits Microsoft.VisualBasic.Imaging.Image

        Private ReadOnly buffer As BitmapBuffer

        ''' <summary>
        ''' the quality factor (1..100) that was used when the image was encoded.
        ''' </summary>
        Public ReadOnly Property Quality As Integer

        ''' <summary>
        ''' create a jpeg image object from a decoded pixel buffer.
        ''' </summary>
        ''' <param name="bmp">the decoded pixel data (bgr / bgra layout).</param>
        ''' <param name="quality">the quality factor to use when re-encoding.</param>
        Sub New(bmp As BitmapBuffer, Optional quality As Integer = 75)
            buffer = bmp
            Me.Quality = quality
        End Sub

        Public Overrides ReadOnly Property Size As Size
            Get
                Return New Size(buffer.Width, buffer.Height)
            End Get
        End Property

        Public Overrides Function GetMemoryBitmap() As BitmapBuffer
            Return buffer
        End Function

        Protected Friend Overrides Function ConvertToBitmapStream() As MemoryStream
            Return New MemoryStream(JpegWriter.Encode(buffer, Quality))
        End Function

        ''' <summary>
        ''' save the image into the target stream. when <paramref name="format"/> is
        ''' <see cref="ImageFormats.Jpeg"/> the baseline jpeg encoder is used, any
        ''' other format falls back to the default <see cref="Bitmap"/> routing.
        ''' </summary>
        Public Overrides Sub Save(s As Stream, format As ImageFormats)
            Select Case format
                Case ImageFormats.Jpeg
                    Call JpegWriter.Save(s, buffer, Quality)
                Case Else
                    Dim bmp As New Bitmap(buffer)
                    Call bmp.Save(s, format)
            End Select
        End Sub

        ''' <summary>
        ''' load a jpeg image from a file path.
        ''' </summary>
        Public Shared Function Load(path As String) As JpegImage
            Return JpegReader.Load(path)
        End Function

        ''' <summary>
        ''' load a jpeg image from a stream.
        ''' </summary>
        Public Shared Function Load(s As Stream) As JpegImage
            Return JpegReader.Load(s)
        End Function

        ''' <summary>
        ''' decode a jpeg byte array into a <see cref="JpegImage"/>.
        ''' </summary>
        Public Shared Function Decode(data As Byte()) As JpegImage
            Return JpegReader.Decode(data)
        End Function

    End Class

End Namespace
#End If
