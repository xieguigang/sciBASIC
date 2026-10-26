' shared gif codec helpers: structure readers / writers, sub block framing,
' the median cut colour quantisation and the colour table helpers. the actual
' lzw compression is performed by the gif compatible lzw codec in
' Microsoft.VisualBasic.Serialization.LZWStream.

Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Serialization

Namespace Imaging.BitmapImage.Gif

    ''' <summary>
    ''' shared low level helpers for the gif codec.
    ''' </summary>
    Public Module GifCodec

        ' block / extension labels
        Public Const EXTENSION As Byte = &H21
        Public Const IMAGE_DESCRIPTOR As Byte = &H2C
        Public Const TRAILER As Byte = &H3B
        Public Const GRAPHIC_CONTROL As Byte = &HF9
        Public Const APPLICATION As Byte = &HFF
        Public Const COMMENT As Byte = &HFE
        Public Const PLAIN_TEXT As Byte = &H01

        Public Const NETSCAPE As String = "NETSCAPE2.0"

        ''' <summary>
        ''' round <paramref name="count"/> up to the next power of two and clamp to
        ''' the gif range [2, 256]. the returned value is the number of colour table
        ''' entries; the size field written to the file is log2 - 1.
        ''' </summary>
        Public Function ColorTableSize(count As Integer) As Integer
            Dim n As Integer = 1

            While n < count
                n <<= 1
            End While

            If n < 2 Then n = 2
            If n > 256 Then n = 256

            Return n
        End Function

        ''' <summary>
        ''' the number of bits required to represent <paramref name="entries"/> colours
        ''' (the gif "size of colour table" field is this value minus one).
        ''' </summary>
        Public Function ColorTableBits(entries As Integer) As Integer
            Dim bits As Integer = 1

            While (1 << bits) < entries
                bits += 1
            End While

            If bits > 8 Then bits = 8

            Return bits
        End Function

        Public Sub WriteUInt16(out As Stream, value As Integer)
            Call out.WriteByte(CByte(value And &HFF))
            Call out.WriteByte(CByte((value >> 8) And &HFF))
        End Sub

        Public Function ReadUInt16(input As Stream) As Integer
            Dim lo As Integer = input.ReadByte()
            Dim hi As Integer = input.ReadByte()
            Return lo Or (hi << 8)
        End Function

        ''' <summary>
        ''' write the gif colour table (3 bytes per entry, padded to <paramref name="entries"/>).
        ''' </summary>
        Public Sub WriteColorTable(out As Stream, palette As List(Of Color), entries As Integer)
            For i As Integer = 0 To entries - 1
                If i < palette.Count Then
                    Call out.WriteByte(palette(i).R)
                    Call out.WriteByte(palette(i).G)
                    Call out.WriteByte(palette(i).B)
                Else
                    Call out.WriteByte(0)
                    Call out.WriteByte(0)
                    Call out.WriteByte(0)
                End If
            Next
        End Sub

        ''' <summary>
        ''' read a gif colour table of <paramref name="entries"/> entries.
        ''' </summary>
        Public Function ReadColorTable(input As Stream, entries As Integer) As List(Of Color)
            Dim palette As New List(Of Color)()

            For i As Integer = 0 To entries - 1
                Dim r As Integer = input.ReadByte()
                Dim g As Integer = input.ReadByte()
                Dim b As Integer = input.ReadByte()
                palette.Add(Color.FromArgb(255, r, g, b))
            Next

            Return palette
        End Function

        ''' <summary>
        ''' write a length prefixed sequence of sub blocks terminated by a zero byte.
        ''' </summary>
        Public Sub WriteSubBlocks(out As Stream, data As Byte())
            Dim i As Integer = 0

            While i < data.Length
                Dim len As Integer = System.Math.Min(255, data.Length - i)
                Call out.WriteByte(CByte(len))
                Call out.Write(data, i, len)
                i += len
            End While

            Call out.WriteByte(0)
        End Sub

        ''' <summary>
        ''' read all the sub blocks following the current position into a single byte
        ''' array (the terminating zero byte is consumed).
        ''' </summary>
        Public Function ReadSubBlocks(input As Stream) As Byte()
            Using ms As New MemoryStream()
                Dim len As Integer = input.ReadByte()

                While len > 0
                    Dim buf(len - 1) As Byte
                    Call ReadExact(input, buf, len)
                    Call ms.Write(buf, 0, len)
                    len = input.ReadByte()
                End While

                Return ms.ToArray()
            End Using
        End Function

        Private Sub ReadExact(input As Stream, buf As Byte(), len As Integer)
            Dim read As Integer = 0

            While read < len
                Dim n As Integer = input.Read(buf, read, len - read)

                If n <= 0 Then
                    Throw New EndOfStreamException("unexpected end of gif stream")
                End If

                read += n
            End While
        End Sub

        Public Function Min(a As Integer, b As Integer) As Integer
            Return If(a < b, a, b)
        End Function

        ' ---- median cut colour quantisation ----

        ''' <summary>
        ''' build a palette of at most <paramref name="targetColors"/> colours from the
        ''' supplied pixel colours using the median cut algorithm.
        ''' </summary>
        Public Function MedianCut(colors As List(Of Color), targetColors As Integer) As List(Of Color)
            If colors Is Nothing OrElse colors.Count = 0 Then
                Return New List(Of Color) From {Color.Black, Color.White}
            End If

            Dim target As Integer = System.Math.Min(256, System.Math.Max(2, targetColors))
            Dim boxes As New List(Of MedianBox)()
            boxes.Add(New MedianBox(colors))

            While boxes.Count < target
                Dim best As MedianBox = Nothing

                For Each b As MedianBox In boxes
                    If b.CanSplit AndAlso (best Is Nothing OrElse b.Range > best.Range) Then
                        best = b
                    End If
                Next

                If best Is Nothing Then
                    Exit While
                End If

                Dim parts As MedianBox() = best.Split()
                boxes.Remove(best)
                boxes.Add(parts(0))
                boxes.Add(parts(1))
            End While

            Dim palette As New List(Of Color)()

            For Each b As MedianBox In boxes
                palette.Add(b.Average)
            Next

            Return palette
        End Function

        ''' <summary>
        ''' find the index of the palette colour nearest to (r, g, b) using a lookup
        ''' cache to avoid recomputing repeated colours.
        ''' </summary>
        Public Function NearestIndex(palette As List(Of Color), r As Integer, g As Integer, b As Integer, cache As Dictionary(Of Integer, Integer)) As Integer
            Dim key As Integer = (r << 16) Or (g << 8) Or b

            If cache IsNot Nothing AndAlso cache.ContainsKey(key) Then
                Return cache(key)
            End If

            Dim best As Integer = 0
            Dim bestDist As Integer = Integer.MaxValue

            For i As Integer = 0 To palette.Count - 1
                Dim dc As Integer = palette(i).R - r
                Dim dg As Integer = palette(i).G - g
                Dim db As Integer = palette(i).B - b
                Dim dist As Integer = dc * dc + dg * dg + db * db

                If dist < bestDist Then
                    bestDist = dist
                    best = i
                End If
            Next

            If cache IsNot Nothing Then
                cache(key) = best
            End If

            Return best
        End Function

    End Module

    Friend Class MedianBox
        Public colors As List(Of Color)

        Public Sub New(colors As List(Of Color))
            Me.colors = colors
        End Sub

        Public ReadOnly Property CanSplit As Boolean
            Get
                Return colors.Count > 1
            End Get
        End Property

        Public ReadOnly Property Range As Integer
            Get
                Dim rmin As Integer = 255, rmax As Integer = 0
                Dim gmin As Integer = 255, gmax As Integer = 0
                Dim bmin As Integer = 255, bmax As Integer = 0

                For Each c As Color In colors
                    If c.R < rmin Then rmin = c.R
                    If c.R > rmax Then rmax = c.R
                    If c.G < gmin Then gmin = c.G
                    If c.G > gmax Then gmax = c.G
                    If c.B < bmin Then bmin = c.B
                    If c.B > bmax Then bmax = c.B
                Next

                Dim rr As Integer = rmax - rmin
                Dim rg As Integer = gmax - gmin
                Dim rb As Integer = bmax - bmin

                Return System.Math.Max(rr, System.Math.Max(rg, rb))
            End Get
        End Property

        Public ReadOnly Property Average As Color
            Get
                Dim r As Integer = 0, g As Integer = 0, b As Integer = 0

                For Each c As Color In colors
                    r += c.R
                    g += c.G
                    b += c.B
                Next

                Dim n As Integer = colors.Count
                Return Color.FromArgb(255, CByte(r \ n), CByte(g \ n), CByte(b \ n))
            End Get
        End Property

        Public Function Split() As MedianBox()
            ' find the channel with the largest range
            Dim rmin As Integer = 255, rmax As Integer = 0
            Dim gmin As Integer = 255, gmax As Integer = 0
            Dim bmin As Integer = 255, bmax As Integer = 0

            For Each c As Color In colors
                If c.R < rmin Then rmin = c.R
                If c.R > rmax Then rmax = c.R
                If c.G < gmin Then gmin = c.G
                If c.G > gmax Then gmax = c.G
                If c.B < bmin Then bmin = c.B
                If c.B > bmax Then bmax = c.B
            Next

            Dim rr As Integer = rmax - rmin
            Dim rg As Integer = gmax - gmin
            Dim rb As Integer = bmax - bmin
            Dim channel As Integer

            If rr >= rg AndAlso rr >= rb Then
                channel = 0
            ElseIf rg >= rb Then
                channel = 1
            Else
                channel = 2
            End If

            colors.Sort(Function(a, b)
                            Dim va As Integer = If(channel = 0, a.R, If(channel = 1, a.G, a.B))
                            Dim vb As Integer = If(channel = 0, b.R, If(channel = 1, b.G, b.B))
                            Return va.CompareTo(vb)
                        End Function)

            Dim mid As Integer = colors.Count \ 2
            Dim left As New List(Of Color)()
            Dim right As New List(Of Color)()

            For i As Integer = 0 To colors.Count - 1
                If i < mid Then
                    left.Add(colors(i))
                Else
                    right.Add(colors(i))
                End If
            Next

            Return New MedianBox() {New MedianBox(left), New MedianBox(right)}
        End Function
    End Class

End Namespace
