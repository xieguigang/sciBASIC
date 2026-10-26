' A dependency free baseline sequential jpeg encoder. the input pixel data is a
' <see cref="BitmapBuffer"/> (bgr / bgra layout), the output is a baseline 8 bits
' jpeg byte stream. the encoder uses the annex k standard huffman tables and
' scales the standard quantisation tables by the requested quality.

Imports System.IO
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Namespace Imaging.BitmapImage.Jpeg

    ''' <summary>
    ''' a dependency free baseline jpeg encoder (8 bits).
    ''' </summary>
    Public Module JpegWriter

        ''' <summary>
        ''' encode a <see cref="BitmapBuffer"/> into a baseline jpeg byte array.
        ''' </summary>
        ''' <param name="bmp">the source pixel data, bgr or bgra layout.</param>
        ''' <param name="quality">the encode quality, 1 (worst) .. 100 (best), 75 by default.</param>
        Public Function Encode(bmp As BitmapBuffer, Optional quality As Integer = 75) As Byte()
            Using ms As New MemoryStream()
                Call Save(ms, bmp, quality)
                Return ms.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' encode a <see cref="BitmapBuffer"/> into the target stream as a baseline jpeg.
        ''' </summary>
        Public Sub Save(s As Stream, bmp As BitmapBuffer, Optional quality As Integer = 75)
            If s Is Nothing Then
                Throw New ArgumentNullException(NameOf(s))
            End If
            If bmp Is Nothing Then
                Throw New ArgumentNullException(NameOf(bmp))
            End If

            Dim width As Integer = bmp.Width
            Dim height As Integer = bmp.Height
            Dim channels As Integer = bmp.GetPixelChannels()
            Dim raw As Byte() = bmp.RawBuffer

            ' build the ycbcr planes
            Dim color As Boolean = (channels >= 3)
            Dim cw As Integer = (width + 1) \ 2
            Dim ch As Integer = (height + 1) \ 2
            Dim yPlane As Double() = New Double(width * height - 1) {}
            Dim cbPlane As Double() = New Double(cw * ch - 1) {}
            Dim crPlane As Double() = New Double(cw * ch - 1) {}

            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    Dim p As Integer = (y * width + x) * channels
                    Dim r As Integer = raw(p + 2)
                    Dim g As Integer = raw(p + 1)
                    Dim b As Integer = raw(p)

                    Dim yy As Integer = 0, cb As Integer = 0, cr As Integer = 0
                    Call JpegCodec.RGBToYCbCr(r, g, b, yy, cb, cr)

                    yPlane(y * width + x) = yy
                    cbPlane((y \ 2) * cw + (x \ 2)) = cb
                    crPlane((y \ 2) * cw + (x \ 2)) = cr
                Next
            Next

            ' build the component descriptors
            Dim dcLumaTbl As New HuffmanTable(JpegCodec.DcLumaBits, JpegCodec.DcLumaVals)
            Dim acLumaTbl As New HuffmanTable(JpegCodec.AcLumaBits, JpegCodec.AcLumaVals)
            Dim dcChromaTbl As New HuffmanTable(JpegCodec.DcChromaBits, JpegCodec.DcChromaVals)
            Dim acChromaTbl As New HuffmanTable(JpegCodec.AcChromaBits, JpegCodec.AcChromaVals)

            Dim comps As New List(Of JpegEncodeComponent)()
            comps.Add(New JpegEncodeComponent With {
                .plane = yPlane, .planeW = width, .planeH = height,
                .h = 2, .v = 2, .tq = 0, .id = 1,
                .dcTable = dcLumaTbl, .acTable = acLumaTbl
            })

            If color Then
                comps.Add(New JpegEncodeComponent With {
                    .plane = cbPlane, .planeW = cw, .planeH = ch,
                    .h = 1, .v = 1, .tq = 1, .id = 2,
                    .dcTable = dcChromaTbl, .acTable = acChromaTbl
                })
                comps.Add(New JpegEncodeComponent With {
                    .plane = crPlane, .planeW = cw, .planeH = ch,
                    .h = 1, .v = 1, .tq = 1, .id = 3,
                    .dcTable = dcChromaTbl, .acTable = acChromaTbl
                })
            End If

            Dim maxH As Integer = 2
            Dim maxV As Integer = 2

            If Not color Then
                comps(0).h = 1
                comps(0).v = 1
                maxH = 1
                maxV = 1
            End If

            Dim mcusX As Integer = (width + maxH * 8 - 1) \ (maxH * 8)
            Dim mcusY As Integer = (height + maxV * 8 - 1) \ (maxV * 8)

            ' quantisation tables (zigzag order for the dqt segment, natural order
            ' for the coefficient division).
            Dim lumaZig As Integer() = ScaleQuant(JpegCodec.StdLuminanceQuant, quality)
            Dim chromaZig As Integer() = ScaleQuant(JpegCodec.StdChrominanceQuant, quality)
            Dim lumaNat As Integer() = Unzig(lumaZig)
            Dim chromaNat As Integer() = Unzig(chromaZig)

            For Each c As JpegEncodeComponent In comps
                c.qnat = If(c.tq = 0, lumaNat, chromaNat)
                c.td = c.tq
                c.ta = c.tq
            Next

            ' ---- write the file skeleton ----
            Call WriteMarker(s, &HD8) ' SOI
            Call WriteApp0(s)
            Call WriteDQT(s, 0, lumaZig)
            Call WriteDQT(s, 1, chromaZig)
            Call WriteSOF0(s, width, height, comps)
            Call WriteDHT(s)
            Call WriteSOS(s, comps)

            ' ---- entropy coded data ----
            Using e As New MemoryStream()
                Dim writer As New JpegBitWriter(e)

                For Each c As JpegEncodeComponent In comps
                    c.prevDC = 0
                Next

                For mcuY As Integer = 0 To mcusY - 1
                    For mcuX As Integer = 0 To mcusX - 1
                        For Each c As JpegEncodeComponent In comps
                            For by As Integer = 0 To c.v - 1
                                For bx As Integer = 0 To c.h - 1
                                    Dim cx As Integer = (mcuX * c.h + bx) * 8
                                    Dim cy As Integer = (mcuY * c.v + by) * 8

                                    Call EncodeBlock(writer, c, cx, cy)
                                Next
                            Next
                        Next
                    Next
                Next

                Call writer.Flush()
                e.WriteTo(s)
            End Using

            Call WriteMarker(s, &HD9) ' EOI
            Call s.Flush()
        End Sub

        Private Sub EncodeBlock(writer As JpegBitWriter, c As JpegEncodeComponent, cx As Integer, cy As Integer)
            Dim block As Double() = New Double(63) {}

            For yy As Integer = 0 To 7
                Dim py As Integer = cy + yy

                If py >= c.planeH Then
                    py = c.planeH - 1
                End If

                For xx As Integer = 0 To 7
                    Dim px As Integer = cx + xx

                    If px >= c.planeW Then
                        px = c.planeW - 1
                    End If

                    block(yy * 8 + xx) = c.plane(py * c.planeW + px) - 128.0
                Next
            Next

            Call JpegCodec.FDCT(block)

            ' quantise into zigzag order
            Dim zz As Integer() = New Integer(63) {}

            For k As Integer = 0 To 63
                Dim val As Integer = CInt(System.Math.Round(block(k) / c.qnat(k)))
                ' place the natural order coefficient at its zigzag scan position
                zz(JpegCodec.UnZigZag(k)) = val
            Next

            ' dc
            Dim diff As Integer = zz(0) - c.prevDC
            c.prevDC = zz(0)

            Dim t As Integer = Category(diff)
            Call c.dcTable.Encode(writer, t)

            If t > 0 Then
                Call WriteMagnitude(writer, diff, t)
            End If

            ' ac
            Dim ac As Integer = 1
            Dim run As Integer = 0

            While ac <= 63
                Dim v As Integer = zz(ac)

                If v = 0 Then
                    run += 1

                    If run = 16 Then
                        Call c.acTable.Encode(writer, &HF0) ' ZRL
                        run = 0
                    End If

                    ac += 1
                    Continue While
                End If

                While run > 15
                    Call c.acTable.Encode(writer, &HF0)
                    run -= 16
                End While

                Dim sz As Integer = Category(v)
                Dim rs As Integer = (run << 4) Or sz

                Call c.acTable.Encode(writer, rs)
                Call WriteMagnitude(writer, v, sz)
                run = 0
                ac += 1
            End While

            If run > 0 Then
                Call c.acTable.Encode(writer, &H0) ' EOB
            End If
        End Sub

        Private Sub WriteMagnitude(writer As JpegBitWriter, val As Integer, t As Integer)
            Dim bits As Integer = val

            If val < 0 Then
                bits = val + (1 << t) - 1
            End If

            Call writer.WriteBits(bits, t)
        End Sub

        Private Function Category(v As Integer) As Integer
            Dim t As Integer = 0
            Dim n As Integer = System.Math.Abs(v)

            While n > 0
                n >>= 1
                t += 1
            End While

            Return t
        End Function

        Private Function ScaleQuant(base() As Integer, quality As Integer) As Integer()
            Dim q As Integer = quality

            If q <= 0 Then q = 1
            If q > 100 Then q = 100

            Dim scale As Double = If(q < 50, 5000.0 / q, 200.0 - 2.0 * q)
            Dim outArr(63) As Integer

            For i As Integer = 0 To 63
                Dim v As Integer = CInt((base(i) * scale) / 100.0)

                If v < 1 Then v = 1
                If v > 255 Then v = 255

                outArr(i) = v
            Next

            Return outArr
        End Function

        Private Function Unzig(zig() As Integer) As Integer()
            Dim nat(63) As Integer

            For i As Integer = 0 To 63
                nat(i) = zig(JpegCodec.UnZigZag(i))
            Next

            Return nat
        End Function

        ' ---- low level segment writers ----

        Private Sub WriteMarker(s As Stream, b As Integer)
            Call s.WriteByte(&HFF)
            Call s.WriteByte(CByte(b))
        End Sub

        Private Sub WriteShort(s As Stream, v As Integer)
            Call s.WriteByte(CByte((v >> 8) And &HFF))
            Call s.WriteByte(CByte(v And &HFF))
        End Sub

        Private Sub WriteApp0(s As Stream)
            Call WriteMarker(s, &HE0)
            Call WriteShort(s, 16)
            Dim jfif As Byte() = {AscW("J"c), AscW("F"c), AscW("I"c), AscW("F"c), 0}
            Call s.Write(jfif, 0, jfif.Length)
            Call s.WriteByte(1) ' major version
            Call s.WriteByte(1) ' minor version
            Call s.WriteByte(0) ' units
            Call WriteShort(s, 1) ' xdensity
            Call WriteShort(s, 1) ' ydensity
            Call s.WriteByte(0) ' xthumbnail
            Call s.WriteByte(0) ' ythumbnail
        End Sub

        Private Sub WriteSOS(s As Stream, comps As List(Of JpegEncodeComponent))
            Call WriteMarker(s, &HDA)
            Call WriteShort(s, 6 + 2 * comps.Count)

            Call s.WriteByte(CByte(comps.Count))

            For Each c As JpegEncodeComponent In comps
                Call s.WriteByte(CByte(c.id))
                Call s.WriteByte(CByte((c.td << 4) Or c.ta))
            Next

            ' spectral selection / successive approximation (baseline : 0 / 63 / 0)
            Call s.WriteByte(0)
            Call s.WriteByte(63)
            Call s.WriteByte(0)
        End Sub

        Private Sub WriteDQT(s As Stream, tq As Integer, table() As Integer)
            Call WriteMarker(s, &HDB)
            Call WriteShort(s, 2 + 1 + 64)
            Call s.WriteByte(CByte(tq)) ' pq = 0, tq

            For i As Integer = 0 To 63
                Call s.WriteByte(CByte(table(i)))
            Next
        End Sub

        Private Sub WriteSOF0(s As Stream, width As Integer, height As Integer, comps As List(Of JpegEncodeComponent))
            Call WriteMarker(s, &HC0)
            Call WriteShort(s, 8 + 3 * comps.Count)
            Call s.WriteByte(8) ' precision
            Call WriteShort(s, height)
            Call WriteShort(s, width)
            Call s.WriteByte(CByte(comps.Count))

            For Each c As JpegEncodeComponent In comps
                Call s.WriteByte(CByte(c.id))
                Call s.WriteByte(CByte((c.h << 4) Or c.v))
                Call s.WriteByte(CByte(c.tq))
            Next
        End Sub

        Private Sub WriteDHT(s As Stream)
            Dim dcLuma As New HuffmanTable(JpegCodec.DcLumaBits, JpegCodec.DcLumaVals)
            Dim acLuma As New HuffmanTable(JpegCodec.AcLumaBits, JpegCodec.AcLumaVals)
            Dim dcChroma As New HuffmanTable(JpegCodec.DcChromaBits, JpegCodec.DcChromaVals)
            Dim acChroma As New HuffmanTable(JpegCodec.AcChromaBits, JpegCodec.AcChromaVals)

            Dim tables As (Integer, Integer, HuffmanTable)() = {
                (0, 0, dcLuma),
                (1, 0, acLuma),
                (0, 1, dcChroma),
                (1, 1, acChroma)
            }

            For Each t In tables
                Using p As New MemoryStream()
                    Call t.Item3.WriteDHT(p, t.Item1, t.Item2)
                    Dim payload As Byte() = p.ToArray()

                    Call WriteMarker(s, &HC4)
                    Call WriteShort(s, 2 + payload.Length)
                    Call s.Write(payload, 0, payload.Length)
                End Using
            Next
        End Sub

    End Module

    Friend Class JpegEncodeComponent
        Public id As Integer
        Public h As Integer
        Public v As Integer
        Public tq As Integer
        Public td As Integer
        Public ta As Integer
        Public plane As Double()
        Public planeW As Integer
        Public planeH As Integer
        Public qnat As Integer()
        Public prevDC As Integer
        Public dcTable As HuffmanTable
        Public acTable As HuffmanTable
    End Class

End Namespace
