' a dependency free gif decoder. it supports the logical screen descriptor, the
' global / local colour tables, the graphic control extension, the netscape loop
' extension and comments, and decodes every frame through the gif compatible lzw
' codec, producing a <see cref="GifImage"/>.

Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Serialization

Namespace Imaging.BitmapImage.Gif

    ''' <summary>
    ''' a dependency free gif decoder.
    ''' </summary>
    Public Module GifReader

        Public Function Load(path As String) As GifImage
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Return Decode(File.ReadAllBytes(path))
        End Function

        Public Function Load(stream As Stream) As GifImage
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            Using buffer As New MemoryStream()
                Call stream.CopyTo(buffer)
                Return Decode(buffer.ToArray())
            End Using
        End Function

        Public Function Decode(data As Byte()) As GifImage
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            If data.Length < 6 OrElse data(0) <> AscW("G"c) OrElse data(1) <> AscW("I"c) OrElse data(2) <> AscW("F"c) Then
                Throw New InvalidDataException("not a gif image")
            End If

            Dim decoder As New GifDecodeContext(data)
            Call decoder.Parse()

            Return decoder.BuildImage()
        End Function

    End Module

    Friend Class GifDecodeContext

        Private ReadOnly data As Byte()
        Private pos As Integer

        Public logicalWidth As Integer = 0
        Public logicalHeight As Integer = 0
        Public globalPalette As List(Of Color)
        Public hasGCT As Boolean = False
        Public bgColorIndex As Integer = 0

        Public loopCount As Integer = -1
        Public frames As New List(Of BitmapBuffer)()
        Public frameDelays As New List(Of Integer)()
        Public frameDisposals As New List(Of Integer)()
        Public transparentIndices As New List(Of Integer)()

        ' pending graphic control values (applied to the next image)
        Private pendingDelay As Integer = 0
        Private pendingDisposal As Integer = 0
        Private pendingTransparent As Integer = -1

        Public Sub New(data As Byte())
            Me.data = data
            Me.pos = 6
        End Sub

        Public Sub Parse()
            Call ParseLSD()

            While pos < data.Length
                Dim block As Integer = data(pos)
                pos += 1

                If block = GifCodec.TRAILER Then
                    Exit While
                ElseIf block = GifCodec.IMAGE_DESCRIPTOR Then
                    Call ParseImage()
                ElseIf block = GifCodec.EXTENSION Then
                    Call ParseExtension()
                Else
                    ' unknown block : stop to avoid corrupting the stream
                    Exit While
                End If
            End While
        End Sub

        Private Function ReadU16() As Integer
            Dim lo As Integer = data(pos)
            Dim hi As Integer = data(pos + 1)
            pos += 2
            Return lo Or (hi << 8)
        End Function

        Private Function ReadColorTableAdvance(entries As Integer) As List(Of Color)
            Dim palette As New List(Of Color)()

            For i As Integer = 0 To entries - 1
                Dim r As Integer = data(pos)
                Dim g As Integer = data(pos + 1)
                Dim b As Integer = data(pos + 2)
                pos += 3
                palette.Add(Color.FromArgb(255, r, g, b))
            Next

            Return palette
        End Function

        Private Function ReadSubBlocksAdvance() As Byte()
            Using ms As New MemoryStream()
                Dim len As Integer = data(pos)
                pos += 1

                While len > 0
                    For k As Integer = 0 To len - 1
                        Call ms.WriteByte(data(pos))
                        pos += 1
                    Next

                    len = data(pos)
                    pos += 1
                End While

                Return ms.ToArray()
            End Using
        End Function

        Private Sub ParseLSD()
            logicalWidth = ReadU16()
            logicalHeight = ReadU16()

            Dim packed As Integer = data(pos)
            pos += 1
            bgColorIndex = data(pos)
            pos += 1
            pos += 1 ' pixel aspect ratio

            hasGCT = (packed And &H80) <> 0

            If hasGCT Then
                Dim bits As Integer = (packed And 7) + 1
                Dim entries As Integer = 1 << bits
                globalPalette = ReadColorTableAdvance(entries)
            End If
        End Sub

        Private Sub ParseExtension()
            Dim label As Integer = data(pos)
            pos += 1

            Select Case label
                Case GifCodec.GRAPHIC_CONTROL
                    Call ParseGraphicControl()
                Case GifCodec.APPLICATION
                    Call ParseApplication()
                Case Else
                    ' comment / plain text / unknown : skip sub blocks
                    Call ReadSubBlocksAdvance()
            End Select
        End Sub

        Private Sub ParseGraphicControl()
            Dim subData As Byte() = ReadSubBlocksAdvance()

            If subData.Length >= 4 Then
                Dim packed As Integer = subData(1)
                pendingDelay = subData(2) Or (subData(3) << 8)
                pendingDisposal = (packed >> 2) And 7

                If (packed And 1) <> 0 Then
                    pendingTransparent = subData(4)
                Else
                    pendingTransparent = -1
                End If
            End If
        End Sub

        Private Sub ParseApplication()
            Dim subData As Byte() = ReadSubBlocksAdvance()

            If subData.Length >= 11 Then
                Dim name As String = System.Text.Encoding.ASCII.GetString(subData, 0, 11)

                If name = GifCodec.NETSCAPE Then
                    Dim i As Integer = 11

                    While i < subData.Length
                        Dim blockLen As Integer = subData(i)
                        i += 1

                        If blockLen = 3 AndAlso i + 2 < subData.Length Then
                            loopCount = subData(i + 1) Or (subData(i + 2) << 8)
                        End If

                        i += blockLen
                    End While
                End If
            End If
        End Sub

        Private Sub ParseImage()
            Dim left As Integer = ReadU16()
            Dim top As Integer = ReadU16()
            Dim w As Integer = ReadU16()
            Dim h As Integer = ReadU16()
            Dim packed As Integer = data(pos)
            pos += 1

            Dim hasLCT As Boolean = (packed And &H80) <> 0
            Dim interlaced As Boolean = (packed And &H40) <> 0
            Dim lct As List(Of Color) = Nothing

            If hasLCT Then
                Dim bits As Integer = (packed And 7) + 1
                Dim entries As Integer = 1 << bits
                lct = ReadColorTableAdvance(entries)
            End If

            Dim palette As List(Of Color) = If(lct IsNot Nothing, lct, globalPalette)
            Dim transparentIndex As Integer = pendingTransparent

            Dim minCodeSize As Integer = data(pos)
            pos += 1

            Dim lzwData As Byte() = ReadSubBlocksAdvance()

            Using decoded As New MemoryStream()
                Call GifLzwDecoder.Decode(New MemoryStream(lzwData), decoded, minCodeSize)
                Dim indices As Byte() = decoded.ToArray()

                If interlaced Then
                    indices = Deinterlace(indices, w, h)
                End If

                frames.Add(BuildFrame(indices, w, h, palette, transparentIndex))
                frameDelays.Add(pendingDelay)
                frameDisposals.Add(pendingDisposal)
                transparentIndices.Add(transparentIndex)
            End Using

            ' reset pending graphic control
            pendingDelay = 0
            pendingDisposal = 0
            pendingTransparent = -1
        End Sub

        Private Function BuildFrame(indices As Byte(), w As Integer, h As Integer, palette As List(Of Color), transparentIndex As Integer) As BitmapBuffer
            Dim buffer(w * h * 4 - 1) As Byte
            Dim total As Integer = w * h

            For i As Integer = 0 To total - 1
                Dim idx As Integer = If(i < indices.Length, indices(i), 0)
                Dim col As Color

                If idx >= 0 AndAlso idx < palette.Count Then
                    col = palette(idx)
                Else
                    col = Color.Black
                End If

                Dim dst As Integer = i * 4
                buffer(dst) = col.B
                buffer(dst + 1) = col.G
                buffer(dst + 2) = col.R
                buffer(dst + 3) = If(idx = transparentIndex, 0, 255)
            Next

            Return New BitmapBuffer(buffer, New Size(w, h), 4)
        End Function

        Private Function Deinterlace(indices As Byte(), w As Integer, h As Integer) As Byte()
            Dim outArr(w * h - 1) As Byte
            Dim src As Integer = 0

            For y As Integer = 0 To h - 1 Step 8
                Array.Copy(indices, src, outArr, y * w, w)
                src += w
            Next

            For y As Integer = 4 To h - 1 Step 8
                Array.Copy(indices, src, outArr, y * w, w)
                src += w
            Next

            For y As Integer = 2 To h - 1 Step 4
                Array.Copy(indices, src, outArr, y * w, w)
                src += w
            Next

            For y As Integer = 1 To h - 1 Step 2
                Array.Copy(indices, src, outArr, y * w, w)
                src += w
            Next

            Return outArr
        End Function

        Public Function BuildImage() As GifImage
            Return New GifImage(frames, logicalWidth, logicalHeight, loopCount, frameDelays, frameDisposals, transparentIndices)
        End Function

    End Class

End Namespace
