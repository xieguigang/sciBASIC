' a dependency free gif encoder. it builds a global colour table from the frames
' using median cut quantisation, maps every pixel to a palette index and writes the
' gif89a stream including graphic control extensions and the netscape loop
' extension for animations. the lzw compression is delegated to the gif compatible
' lzw codec.

Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Serialization

Namespace Imaging.BitmapImage.Gif

    ''' <summary>
    ''' a dependency free gif encoder (static and animated).
    ''' </summary>
    Public Module GifWriter

        ''' <summary>
        ''' encode a single frame into the target stream as a static gif.
        ''' </summary>
        Public Sub Save(s As Stream, bmp As BitmapBuffer)
            Dim frames As New List(Of BitmapBuffer) From {bmp}
            Call Save(s, frames, 0, Nothing, Nothing)
        End Sub

        ''' <summary>
        ''' encode one or more frames into the target stream as an animated gif.
        ''' </summary>
        ''' <param name="loopCount">
        ''' the number of additional loops (0 = loop forever). the loop extension is
        ''' only written when there is more than one frame.
        ''' </param>
        ''' <param name="delays">per frame delay in centiseconds.</param>
        ''' <param name="disposals">per frame disposal method (0..3).</param>
        Public Sub Save(s As Stream,
                        frames As IEnumerable(Of BitmapBuffer),
                        Optional loopCount As Integer = 0,
                        Optional delays As Integer() = Nothing,
                        Optional disposals As Integer() = Nothing)
            If s Is Nothing Then
                Throw New ArgumentNullException(NameOf(s))
            End If

            Dim frameList As List(Of BitmapBuffer) = frames.ToList()

            If frameList.Count = 0 Then
                Throw New ArgumentException("no frames to encode")
            End If

            ' detect per frame transparency (alpha < 128)
            Dim frameTransparent As New List(Of Boolean)()

            For Each f As BitmapBuffer In frameList
                frameTransparent.Add(FrameHasTransparency(f))
            Next

            Dim anyTransparent As Boolean = frameTransparent.Contains(True)

            ' build the global palette from all frames
            Dim samples As List(Of Color) = CollectColors(frameList)
            Dim targetColors As Integer = If(anyTransparent, 255, 256)
            Dim palette As List(Of Color) = GifCodec.MedianCut(samples, targetColors)

            Dim transparentIndex As Integer = -1

            If anyTransparent Then
                palette.Add(Color.FromArgb(255, 0, 0, 0))
                transparentIndex = palette.Count - 1
            End If

            Dim entries As Integer = GifCodec.ColorTableSize(palette.Count)
            Dim bits As Integer = GifCodec.ColorTableBits(entries)
            Dim minCodeSize As Integer = System.Math.Max(2, bits)

            ' ---- header ----
            Dim header As Byte() = System.Text.Encoding.ASCII.GetBytes("GIF89a")
            Call s.Write(header, 0, 6)

            ' ---- logical screen descriptor ----
            Dim logicalW As Integer = frameList(0).Width
            Dim logicalH As Integer = frameList(0).Height

            Call GifCodec.WriteUInt16(s, logicalW)
            Call GifCodec.WriteUInt16(s, logicalH)

            Dim packed As Integer = &H80 Or ((bits - 1) << 4) Or (bits - 1)
            Call s.WriteByte(CByte(packed))
            Call s.WriteByte(0) ' background colour index
            Call s.WriteByte(0) ' pixel aspect ratio

            ' ---- global colour table ----
            Call GifCodec.WriteColorTable(s, palette, entries)

            ' ---- netscape loop extension (animation only) ----
            If frameList.Count > 1 Then
                Call WriteLoopExtension(s, loopCount)
            End If

            ' ---- frames ----
            Dim cache As New Dictionary(Of Integer, Integer)()

            For i As Integer = 0 To frameList.Count - 1
                Dim f As BitmapBuffer = frameList(i)
                Dim delay As Integer = If(delays IsNot Nothing AndAlso i < delays.Length, delays(i), 10)
                Dim disposal As Integer = If(disposals IsNot Nothing AndAlso i < disposals.Length, disposals(i), If(frameTransparent(i), 2, 1))
                Dim transparent As Boolean = frameTransparent(i)

                If frameList.Count > 1 OrElse transparent Then
                    Call WriteGraphicControl(s, delay, disposal, If(transparent, transparentIndex, -1))
                End If

                Call WriteImageDescriptor(s, f.Width, f.Height)

                ' the minimum code size byte precedes the lzw sub block stream
                Call s.WriteByte(CByte(minCodeSize))

                Dim indices As Byte() = BuildIndices(f, palette, transparent, transparentIndex, cache)

                Using lzw As New MemoryStream()
                    Call GifLzwEncoder.Encode(indices, lzw, minCodeSize)
                    Call GifCodec.WriteSubBlocks(s, lzw.ToArray())
                End Using
            Next

            ' ---- trailer ----
            Call s.WriteByte(GifCodec.TRAILER)
            Call s.Flush()
        End Sub

        Public Function Encode(frames As IEnumerable(Of BitmapBuffer), Optional loopCount As Integer = 0) As Byte()
            Using ms As New MemoryStream()
                Call Save(ms, frames, loopCount, Nothing, Nothing)
                Return ms.ToArray()
            End Using
        End Function

        Private Function FrameHasTransparency(f As BitmapBuffer) As Boolean
            Dim raw As Byte() = f.RawBuffer
            Dim channels As Integer = f.GetPixelChannels()

            If channels < 4 Then
                Return False
            End If

            For p As Integer = 3 To raw.Length - 1 Step channels
                If raw(p) < 128 Then
                    Return True
                End If
            Next

            Return False
        End Function

        Private Function CollectColors(frames As List(Of BitmapBuffer)) As List(Of Color)
            Dim list As New List(Of Color)()
            Dim cap As Integer = 30000

            For Each f As BitmapBuffer In frames
                Dim raw As Byte() = f.RawBuffer
                Dim channels As Integer = f.GetPixelChannels()
                Dim stride As Integer = System.Math.Max(1, (raw.Length \ channels) \ cap + 1)

                For p As Integer = 0 To raw.Length - 1 Step channels * stride
                    Dim b As Integer = raw(p)
                    Dim g As Integer = raw(p + 1)
                    Dim r As Integer = raw(p + 2)
                    list.Add(Color.FromArgb(255, r, g, b))

                    If list.Count >= cap Then
                        Return list
                    End If
                Next
            Next

            Return list
        End Function

        Private Function BuildIndices(f As BitmapBuffer, palette As List(Of Color), transparent As Boolean, transparentIndex As Integer, cache As Dictionary(Of Integer, Integer)) As Byte()
            Dim raw As Byte() = f.RawBuffer
            Dim channels As Integer = f.GetPixelChannels()
            Dim n As Integer = f.Width * f.Height
            Dim indices(n - 1) As Byte

            For p As Integer = 0 To raw.Length - 1 Step channels
                Dim b As Integer = raw(p)
                Dim g As Integer = raw(p + 1)
                Dim r As Integer = raw(p + 2)
                Dim a As Integer = If(channels >= 4, raw(p + 3), 255)

                Dim idx As Integer

                If transparent AndAlso a < 128 Then
                    idx = transparentIndex
                Else
                    idx = GifCodec.NearestIndex(palette, r, g, b, cache)
                End If

                indices(p \ channels) = CByte(idx)
            Next

            Return indices
        End Function

        Private Sub WriteLoopExtension(s As Stream, loopCount As Integer)
            Call s.WriteByte(GifCodec.EXTENSION)
            Call s.WriteByte(GifCodec.APPLICATION)
            Call s.WriteByte(11)

            Dim name As Byte() = System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0")
            Call s.Write(name, 0, 11)

            Call s.WriteByte(3)
            Call s.WriteByte(&H1)
            Call GifCodec.WriteUInt16(s, loopCount)
            Call s.WriteByte(0)
        End Sub

        Private Sub WriteGraphicControl(s As Stream, delay As Integer, disposal As Integer, transparentIndex As Integer)
            Call s.WriteByte(GifCodec.EXTENSION)
            Call s.WriteByte(GifCodec.GRAPHIC_CONTROL)
            Call s.WriteByte(4)

            Dim hasTransparent As Boolean = transparentIndex >= 0
            Dim packed As Integer = (disposal << 2) Or If(hasTransparent, 1, 0)

            Call s.WriteByte(CByte(packed))
            Call GifCodec.WriteUInt16(s, delay)
            Call s.WriteByte(CByte(If(hasTransparent, transparentIndex, 0)))
            Call s.WriteByte(0)
        End Sub

        Private Sub WriteImageDescriptor(s As Stream, w As Integer, h As Integer)
            Call s.WriteByte(GifCodec.IMAGE_DESCRIPTOR)
            Call GifCodec.WriteUInt16(s, 0) ' left
            Call GifCodec.WriteUInt16(s, 0) ' top
            Call GifCodec.WriteUInt16(s, w)
            Call GifCodec.WriteUInt16(s, h)

            ' no local colour table, no interlace
            Call s.WriteByte(0)
        End Sub

    End Module

End Namespace
