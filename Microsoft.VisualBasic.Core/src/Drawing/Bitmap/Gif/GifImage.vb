#If NET8_0_OR_GREATER Or NETSTANDARD2_0_OR_GREATER Then

Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.BitmapImage.Gif

Namespace Imaging

    ''' <summary>
    ''' a gif image object that wraps one or more decoded frames together with the
    ''' animation metadata (per frame delay / disposal / transparent colour and the
    ''' netscape loop count).
    ''' </summary>
    Public Class GifImage : Inherits Image

        Private ReadOnly frames As List(Of BitmapBuffer)
        Private ReadOnly logicalW As Integer
        Private ReadOnly logicalH As Integer

        ''' <summary>
        ''' the loop count (0 = loop forever, -1 = no loop extension present).
        ''' </summary>
        Public ReadOnly Property LoopCount As Integer

        ''' <summary>
        ''' number of frames in this gif image.
        ''' </summary>
        Public ReadOnly Property FrameCount As Integer

        ''' <summary>
        ''' per frame delay, in centiseconds.
        ''' </summary>
        Public ReadOnly Property Delays As Integer()

        ''' <summary>
        ''' per frame disposal method (0..3).
        ''' </summary>
        Public ReadOnly Property Disposals As Integer()

        ''' <summary>
        ''' per frame transparent colour index, or -1 when the frame is opaque.
        ''' </summary>
        Public ReadOnly Property TransparentIndices As Integer()

        ''' <summary>
        ''' the logical screen size of the gif image.
        ''' </summary>
        Public ReadOnly Property LogicalSize As Size
            Get
                Return New Size(logicalW, logicalH)
            End Get
        End Property

        Sub New(frames As List(Of BitmapBuffer), logicalWidth As Integer, logicalHeight As Integer, loopCount As Integer, delays As List(Of Integer), disposals As List(Of Integer), transparentIndices As List(Of Integer))
            Me.frames = frames
            logicalW = logicalWidth
            logicalH = logicalHeight
            Me.LoopCount = loopCount
            Me.FrameCount = frames.Count
            Me.Delays = delays.ToArray()
            Me.Disposals = disposals.ToArray()
            Me.TransparentIndices = transparentIndices.ToArray()
        End Sub

        ''' <summary>
        ''' get a single frame buffer by index.
        ''' </summary>
        Public Function Frame(index As Integer) As BitmapBuffer
            Return frames(index)
        End Function

        Public Overrides ReadOnly Property Size As Size
            Get
                Return New Size(logicalW, logicalH)
            End Get
        End Property

        Public Overrides Function GetMemoryBitmap() As BitmapBuffer
            If frames.Count > 0 Then
                Return frames(0)
            Else
                Return Nothing
            End If
        End Function

        Protected Friend Overrides Function ConvertToBitmapStream() As MemoryStream
            Return New MemoryStream(GifWriter.Encode(frames, LoopCount))
        End Function

        ''' <summary>
        ''' save the image into the target stream. when <paramref name="format"/> is
        ''' <see cref="ImageFormats.Gif"/> the (possibly animated) gif encoder is used,
        ''' any other format falls back to the default <see cref="Bitmap"/> routing.
        ''' </summary>
        Public Overrides Sub Save(s As Stream, format As ImageFormats)
            Select Case format
                Case ImageFormats.Gif
                    Call GifWriter.Save(s, frames, LoopCount, Delays, Disposals)
                Case Else
                    Dim bmp As New Bitmap(GetMemoryBitmap())
                    Call bmp.Save(s, format)
            End Select
        End Sub

        ''' <summary>
        ''' load a gif image from a file path.
        ''' </summary>
        Public Shared Function Load(path As String) As GifImage
            Return GifReader.Load(path)
        End Function

        ''' <summary>
        ''' load a gif image from a stream.
        ''' </summary>
        Public Shared Function Load(s As Stream) As GifImage
            Return GifReader.Load(s)
        End Function

        ''' <summary>
        ''' decode a gif byte array into a <see cref="GifImage"/>.
        ''' </summary>
        Public Shared Function Decode(data As Byte()) As GifImage
            Return GifReader.Decode(data)
        End Function

    End Class

End Namespace
#End If
