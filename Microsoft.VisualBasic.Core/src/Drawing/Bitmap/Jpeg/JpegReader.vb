' A dependency free jpeg decoder that supports both baseline sequential and
' progressive 8 bits images, the gray scale and the ycbcr color image are
' both supported. the decoded pixel data is returned as a <see cref="JpegImage"/>.

Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Namespace Imaging.BitmapImage.Jpeg

    ''' <summary>
    ''' a dependency free jpeg decoder (baseline + progressive, 8 bits).
    ''' </summary>
    Public Module JpegReader

        ''' <summary>
        ''' load a jpeg image file from disk.
        ''' </summary>
        Public Function Load(path As String) As JpegImage
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Return Decode(File.ReadAllBytes(path))
        End Function

        ''' <summary>
        ''' load a jpeg image from a stream, the stream is read into memory at once.
        ''' </summary>
        Public Function Load(stream As Stream) As JpegImage
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            Using buffer As New MemoryStream()
                Call stream.CopyTo(buffer)
                Return Decode(buffer.ToArray())
            End Using
        End Function

        ''' <summary>
        ''' decode the jpeg image bytes into a <see cref="JpegImage"/>.
        ''' </summary>
        Public Function Decode(data As Byte()) As JpegImage
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If data.Length < 2 OrElse data(0) <> &HFF OrElse data(1) <> CByte(JpegCodec.C_SOI) Then
                Throw New InvalidDataException("not a jpeg image (missing SOI marker)")
            End If

            Dim decoder As New JpegDecodeContext(data)

            Call decoder.Parse()

            If Not decoder.HaveImage Then
                Throw New InvalidDataException("the jpeg stream does not contain a complete image")
            End If

            Return decoder.BuildImage()
        End Function

    End Module

    Friend Class JpegDecodeContext

        Private ReadOnly data As Byte()
        Private pos As Integer

        ' scan state
        Public width As Integer = 0
        Public height As Integer = 0
        Public precision As Integer = 8
        Public progressive As Boolean = False
        Public maxH As Integer = 1
        Public maxV As Integer = 1
        Public mcusX As Integer = 0
        Public mcusY As Integer = 0
        Public restartInterval As Integer = 0

        Public components As New List(Of JpegComponent)()
        Private compById As New Dictionary(Of Integer, JpegComponent)()

        ' quantisation tables (natural order), index 0..3
        Private qtables(3)() As Integer

        ' huffman tables
        Private dcTables(3) As HuffmanTable
        Private acTables(3) As HuffmanTable

        ' standard fallback tables
        Private stdDc(1) As HuffmanTable
        Private stdAc(1) As HuffmanTable

        Public HaveImage As Boolean = False

        Public Sub New(data As Byte())
            Me.data = data
            Me.pos = 2
            stdDc(0) = New HuffmanTable(JpegCodec.DcLumaBits, JpegCodec.DcLumaVals)
            stdDc(1) = New HuffmanTable(JpegCodec.DcChromaBits, JpegCodec.DcChromaVals)
            stdAc(0) = New HuffmanTable(JpegCodec.AcLumaBits, JpegCodec.AcLumaVals)
            stdAc(1) = New HuffmanTable(JpegCodec.AcChromaBits, JpegCodec.AcChromaVals)
        End Sub

        ' ---- big endian helpers ----
        Private Function ReadBE16() As Integer
            Dim v As Integer = (CInt(data(pos)) << 8) Or data(pos + 1)
            pos += 2
            Return v
        End Function

        ''' <summary>
        ''' read a marker segment payload (the length field is consumed), returns
        ''' the payload bytes.
        ''' </summary>
        Private Function ReadSegmentPayload() As Byte()
            Dim len As Integer = ReadBE16()
            If len < 2 Then
                Throw New InvalidDataException("invalid jpeg segment length")
            End If

            Dim payload As Byte() = New Byte(len - 3) {}
            Call Array.Copy(data, pos, payload, 0, len - 2)
            pos += len - 2

            Return payload
        End Function

        Public Sub Parse()
            While pos < data.Length
                If data(pos) <> &HFF Then
                    pos += 1
                    Continue While
                End If

                pos += 1

                If pos >= data.Length Then
                    Exit While
                End If

                Dim marker As Integer = data(pos)
                pos += 1

                Select Case marker
                    Case JpegCodec.C_SOI, &H1, &HFF
                        ' ignore
                    Case JpegCodec.C_EOI
                        Exit While
                    Case JpegCodec.C_DQT
                        Call ParseDQT()
                    Case JpegCodec.C_SOF0, JpegCodec.C_SOF2
                        progressive = (marker = JpegCodec.C_SOF2)
                        Call ParseSOF()
                    Case JpegCodec.C_DHT
                        Call ParseDHT()
                    Case JpegCodec.C_DRI
                        Dim dri As Byte() = ReadSegmentPayload()
                        restartInterval = (dri(0) << 8) Or dri(1)
                    Case JpegCodec.C_SOS
                        Call ParseSOS()
                    Case Else
                        ' app / com / unknown markers : skip the segment
                        Call ReadSegmentPayload()
                End Select
            End While
        End Sub

        Private Sub ParseDQT()
            Dim payload As Byte() = ReadSegmentPayload()
            Dim i As Integer = 0

            While i < payload.Length
                Dim pq As Integer = payload(i) >> 4
                Dim tq As Integer = payload(i) And 15
                i += 1

                If tq < 0 OrElse tq > 3 Then
                    Throw New InvalidDataException($"invalid dqt table id {tq}")
                End If

                Dim table As Integer() = New Integer(63) {}

                If pq = 0 Then
                    For k As Integer = 0 To 63
                        ' payload is in zigzag order : map each zigzag position to its
                        ' natural index.
                        table(JpegCodec.ZigZag(k)) = payload(i)
                        i += 1
                    Next
                Else
                    For k As Integer = 0 To 63
                        table(JpegCodec.ZigZag(k)) = (payload(i) << 8) Or payload(i + 1)
                        i += 2
                    Next
                End If

                qtables(tq) = table
            End While
        End Sub

        Private Sub ParseSOF()
            Dim payload As Byte() = ReadSegmentPayload()

            precision = payload(0)
            height = (payload(1) << 8) Or payload(2)
            width = (payload(3) << 8) Or payload(4)

            Dim ncomp As Integer = payload(5)
            Dim p As Integer = 6

            For c As Integer = 0 To ncomp - 1
                Dim id As Integer = payload(p)
                Dim samp As Integer = payload(p + 1)
                Dim tq As Integer = payload(p + 2)
                p += 3

                Dim comp As New JpegComponent With {
                    .id = id,
                    .h = samp >> 4,
                    .v = samp And 15,
                    .tq = tq
                }

                If comp.h < 1 OrElse comp.v < 1 Then
                    comp.h = 1
                    comp.v = 1
                End If

                maxH = System.Math.Max(maxH, comp.h)
                maxV = System.Math.Max(maxV, comp.v)

                components.Add(comp)
                compById(id) = comp
            Next

            mcusX = (width + maxH * 8 - 1) \ (maxH * 8)
            mcusY = (height + maxV * 8 - 1) \ (maxV * 8)

            For Each comp As JpegComponent In components
                comp.compWBlocks = mcusX * comp.h
                comp.compHBlocks = mcusY * comp.v
                comp.compW = comp.compWBlocks * 8
                comp.compH = comp.compHBlocks * 8
                comp.coefs = New Double(comp.compWBlocks * comp.compHBlocks * 64 - 1) {}
                comp.samples = New Byte(comp.compW * comp.compH - 1) {}
            Next
        End Sub

        Private Sub ParseDHT()
            Dim payload As Byte() = ReadSegmentPayload()
            Dim i As Integer = 0

            While i < payload.Length
                Dim tc As Integer = payload(i) >> 4
                Dim th As Integer = payload(i) And 15
                i += 1

                Dim counts(15) As Integer
                Dim total As Integer = 0

                For k As Integer = 0 To 15
                    counts(k) = payload(i)
                    total += counts(k)
                    i += 1
                Next

                Dim vals(total - 1) As Integer

                For k As Integer = 0 To total - 1
                    vals(k) = payload(i)
                    i += 1
                Next

                Dim table As New HuffmanTable(counts, vals)

                If tc = 0 Then
                    dcTables(th) = table
                Else
                    acTables(th) = table
                End If
            End While
        End Sub

        Private Sub ParseSOS()
            Dim payload As Byte() = ReadSegmentPayload()
            Dim ns As Integer = payload(0)
            Dim p As Integer = 1
            Dim scanIds As New List(Of Integer)()

            For c As Integer = 0 To ns - 1
                Dim id As Integer = payload(p)
                Dim tdta As Integer = payload(p + 1)
                p += 2

                Dim comp As JpegComponent = compById(id)

                If comp Is Nothing Then
                    Throw New InvalidDataException($"sos references unknown component {id}")
                End If

                comp.td = tdta >> 4
                comp.ta = tdta And 15
                scanIds.Add(id)
            Next

            Dim Ss As Integer = payload(p)
            Dim Se As Integer = payload(p + 1)
            Dim AhAl As Integer = payload(p + 2)
            Dim Ah As Integer = AhAl >> 4
            Dim Al As Integer = AhAl And 15

            ' reset dc predictors for the components that carry dc (Ss = 0)
            If Ss = 0 Then
                For Each id As Integer In scanIds
                    compById(id).pred = 0
                Next
            End If

            ' decode the entropy coded data that follows the sos header
            Dim reader As New JpegBitReader(data, pos, data.Length - pos)
            Call DecodeScan(reader, scanIds, Ss, Se, Ah, Al)

            ' advance the file position to the marker that terminated this scan so
            ' that the outer marker loop can parse the next segment (another sos
            ' for progressive images, or the eoi marker).
            If reader.MarkerStart >= 0 Then
                pos = reader.MarkerStart
            Else
                Call reader.SyncToMarker()
                pos = reader.MarkerStart
            End If
        End Sub

        Private Sub DecodeScan(reader As JpegBitReader, scanIds As List(Of Integer), Ss As Integer, Se As Integer, Ah As Integer, Al As Integer)
            Dim comps As New List(Of JpegComponent)()

            For Each id As Integer In scanIds
                comps.Add(compById(id))
            Next

            Dim totalMcus As Integer = mcusX * mcusY
            Dim mcuIndex As Integer = 0
            Dim ri As Integer = restartInterval

            For mcuY As Integer = 0 To mcusY - 1
                For mcuX As Integer = 0 To mcusX - 1
                    ' a marker surfaced by the bit reader means we reached a scan
                    ' boundary (a restart, eoi or the next sos). handle it first.
                    If reader.Marker >= 0 Then
                        If reader.Marker >= JpegCodec.C_RST0 AndAlso reader.Marker <= JpegCodec.C_RST7 Then
                            If ri > 0 AndAlso mcuIndex > 0 AndAlso (mcuIndex Mod ri) = 0 Then
                                For Each comp As JpegComponent In comps
                                    comp.pred = 0
                                Next
                            End If

                            Call reader.ConsumeMarker()
                        Else
                            ' eoi or the next sos terminates this scan
                            HaveImage = True
                            Return
                        End If
                    End If

                    For Each comp As JpegComponent In comps
                        For by As Integer = 0 To comp.v - 1
                            For bx As Integer = 0 To comp.h - 1
                                Dim blockCol As Integer = mcuX * comp.h + bx
                                Dim blockRow As Integer = mcuY * comp.v + by
                                Dim blockIndex As Integer = blockRow * comp.compWBlocks + blockCol
                                Dim offset As Integer = blockIndex * 64

                                Call DecodeBlock(reader, comp, offset, Ss, Se, Ah, Al)
                            Next
                        Next
                    Next

                    mcuIndex += 1

                    If mcuIndex >= totalMcus Then
                        HaveImage = True
                        Return
                    End If
                Next
            Next

            HaveImage = True
        End Sub

        Private Sub DecodeBlock(reader As JpegBitReader, comp As JpegComponent, offset As Integer, Ss As Integer, Se As Integer, Ah As Integer, Al As Integer)
            If qtables(comp.tq) Is Nothing Then
                Throw New InvalidDataException($"missing quantisation table {comp.tq}")
            End If

            If Ss = 0 Then
                ' ---- dc ----
                Dim dcTable As HuffmanTable = If(dcTables(comp.td) Is Nothing, stdDc(System.Math.Min(comp.td, 1)), dcTables(comp.td))
                Dim t As Integer = dcTable.Decode(reader)
                Dim diff As Integer = JpegCodec.Extend(reader.ReadBits(t), t)

                comp.pred += diff

                If Ah = 0 Then
                    comp.coefs(offset) = comp.pred << Al
                Else
                    Dim bit As Integer = reader.ReadBit()
                    comp.coefs(offset) = comp.coefs(offset) Or (bit << Al)
                End If
            End If

            If Se > 0 Then
                ' ---- ac ----
                Dim acTable As HuffmanTable = If(acTables(comp.ta) Is Nothing, stdAc(System.Math.Min(comp.ta, 1)), acTables(comp.ta))
                ' in a sequential scan (Ss = 0) the dc coefficient is handled above, so
                ' the ac coefficients start at zigzag position 1.
                Dim k As Integer = If(Ss = 0, 1, Ss)

                Do While k <= Se
                    Dim rs As Integer = acTable.Decode(reader)
                    Dim r As Integer = rs >> 4
                    Dim s As Integer = rs And 15

                    If s = 0 Then
                        If r = 15 Then
                            k += 16
                            Continue Do
                        Else
                            Exit Do
                        End If
                    End If

                    k += r

                    If k > Se Then
                        Exit Do
                    End If

                    If Ah = 0 Then
                        Dim v As Integer = JpegCodec.Extend(reader.ReadBits(s), s)
                        ' k is the zigzag scan position : store at the natural index
                        comp.coefs(offset + JpegCodec.ZigZag(k)) = v << Al
                    Else
                        Dim bit As Integer = reader.ReadBit()
                        comp.coefs(offset + JpegCodec.ZigZag(k)) = comp.coefs(offset + JpegCodec.ZigZag(k)) Or (bit << Al)
                    End If

                    k += 1
                Loop
            End If

            ' the dequantisation is deferred to ConvertToSamples so that the
            ' progressive refinement scans do not multiply the coefficients twice.
        End Sub

        Public Function BuildImage() As JpegImage
            ' convert coefficient blocks into sample planes
            For Each comp As JpegComponent In components
                Call ConvertToSamples(comp)
            Next

            Dim color As Boolean = (components.Count >= 3)
            Dim channels As Integer = 3
            Dim buffer As Byte() = New Byte(width * height * channels - 1) {}
            Dim yComp As JpegComponent = components(0)
            Dim cbComp As JpegComponent = If(color, components(1), Nothing)
            Dim crComp As JpegComponent = If(color, components(2), Nothing)

            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    Dim yy As Integer, cb As Integer, cr As Integer

                    yy = SampleAt(yComp, x, y)
                    If color Then
                        cb = SampleAt(cbComp, x, y)
                        cr = SampleAt(crComp, x, y)
                    Else
                        cb = 128
                        cr = 128
                    End If

                    Dim r As Integer = 0, g As Integer = 0, b As Integer = 0
                    Call JpegCodec.YCbCrToRGB(yy, cb, cr, r, g, b)

                    Dim dst As Integer = (y * width + x) * channels
                    buffer(dst) = JpegCodec.Clamp8(b)
                    buffer(dst + 1) = JpegCodec.Clamp8(g)
                    buffer(dst + 2) = JpegCodec.Clamp8(r)
                Next
            Next

            Dim bmp As New BitmapBuffer(buffer, New Size(width, height), channels)

            Return New JpegImage(bmp, progressive)
        End Function

        Private Function SampleAt(comp As JpegComponent, x As Integer, y As Integer) As Integer
            Dim sx As Integer = (x * comp.h) \ maxH
            Dim sy As Integer = (y * comp.v) \ maxV

            If sx >= comp.compW Then sx = comp.compW - 1
            If sy >= comp.compH Then sy = comp.compH - 1

            Return comp.samples(sy * comp.compW + sx)
        End Function

        Private Sub ConvertToSamples(comp As JpegComponent)
            Dim block As Double() = New Double(63) {}
            Dim qt As Integer() = qtables(comp.tq)

            For by As Integer = 0 To comp.compHBlocks - 1
                For bx As Integer = 0 To comp.compWBlocks - 1
                    Dim offset As Integer = (by * comp.compWBlocks + bx) * 64

                    For i As Integer = 0 To 63
                        block(i) = comp.coefs(offset + i) * qt(i)
                    Next

                    Dim dstOff As Integer = (by * 8) * comp.compW + (bx * 8)

                    Call JpegCodec.IDCT(block, comp.samples, dstOff, comp.compW)
                Next
            Next
        End Sub

    End Class

    Friend Class JpegComponent
        Public id As Integer
        Public h As Integer
        Public v As Integer
        Public tq As Integer
        Public td As Integer
        Public ta As Integer
        Public pred As Integer
        Public compWBlocks As Integer
        Public compHBlocks As Integer
        Public compW As Integer
        Public compH As Integer
        Public coefs As Double()
        Public samples As Byte()
    End Class

End Namespace
