' JPEG shared low level codec helpers: markers, standard quantisation / huffman
' tables, bit reader / writer, huffman table model, IDCT / FDCT and color conversion.
'
' This module is dependency free and mirrors the style of the PNG codec
' (Imaging.BitmapImage.Png.PngCodec) so that the jpeg reader / writer can
' reuse the same building blocks.

Imports System.IO

Namespace Imaging.BitmapImage.Jpeg

    ''' <summary>
    ''' shared low level helpers for the baseline / progressive jpeg codec.
    ''' </summary>
    Public Module JpegCodec

        ' ---- marker constants (the second byte of a FFxx marker) ----
        Public Const C_Marker As Byte = &HFF
        Public Const C_SOI As Byte = &HD8
        Public Const C_EOI As Byte = &HD9
        Public Const C_DQT As Byte = &HDB
        Public Const C_SOF0 As Byte = &HC0 ' baseline
        Public Const C_SOF2 As Byte = &HC2 ' progressive
        Public Const C_DHT As Byte = &HC4
        Public Const C_DRI As Byte = &HDD
        Public Const C_SOS As Byte = &HDA
        Public Const C_APP0 As Byte = &HE0
        Public Const C_COM As Byte = &HFE
        Public Const C_RST0 As Byte = &HD0
        Public Const C_RST7 As Byte = &HD7

        ' ---- zigzag natural order (index in natural 8x8 -> zigzag position) ----
        Public ReadOnly ZigZag As Integer() = {
            0, 1, 8, 16, 9, 2, 3, 10,
            17, 24, 32, 25, 18, 11, 4, 5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13, 6, 7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63
        }

        ' inverse of ZigZag
        Public ReadOnly UnZigZag As Integer() = {
            0, 1, 5, 6, 14, 15, 27, 28,
            2, 4, 7, 13, 16, 26, 29, 42,
            3, 8, 12, 17, 25, 30, 41, 43,
            9, 11, 18, 24, 31, 40, 44, 53,
            10, 19, 23, 32, 39, 45, 52, 54,
            20, 22, 33, 38, 46, 51, 55, 60,
            21, 34, 37, 47, 50, 56, 59, 61,
            35, 36, 48, 49, 57, 58, 62, 63
        }

        ' ---- standard (annex K) quantisation tables in zigzag order ----
        Public ReadOnly StdLuminanceQuant As Integer() = {
            16, 11, 12, 14, 12, 10, 16, 14,
            13, 14, 18, 17, 16, 19, 24, 40,
            26, 24, 22, 22, 24, 49, 35, 37,
            29, 40, 58, 51, 61, 60, 57, 51,
            56, 55, 64, 72, 92, 78, 64, 68,
            87, 69, 55, 56, 80, 109, 81, 87,
            95, 98, 103, 104, 103, 62, 77, 113,
            121, 112, 100, 120, 92, 101, 103, 99
        }

        Public ReadOnly StdChrominanceQuant As Integer() = {
            17, 18, 18, 24, 21, 24, 47, 26,
            26, 47, 99, 66, 56, 66, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99
        }

        ' ---- standard (annex K) huffman tables ----
        ' dc luminance
        Public ReadOnly DcLumaBits As Integer() = {0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0}
        Public ReadOnly DcLumaVals As Integer() = {&H0, &H1, &H2, &H3, &H4, &H5, &H6, &H7, &H8, &H9, &HA, &HB}
        ' dc chrominance
        Public ReadOnly DcChromaBits As Integer() = {0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0}
        Public ReadOnly DcChromaVals As Integer() = {&H0, &H1, &H2, &H3, &H4, &H5, &H6, &H7, &H8, &H9, &HA, &HB}
        ' ac luminance
        Public ReadOnly AcLumaBits As Integer() = {0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 125}
        Public ReadOnly AcLumaVals As Integer() = {
            &H1, &H2, &H3, &H0, &H4, &H11, &H5, &H12, &H21, &H31, &H41, &H6, &H13, &H51, &H61, &H7,
            &H22, &H71, &H14, &H32, &H81, &H91, &HA1, &H8, &H23, &H42, &HB1, &HC1, &H15, &H52, &HD1, &HF0,
            &H24, &H33, &H62, &H72, &H82, &H9, &HA, &H16, &H17, &H18, &H19, &H1A, &H25, &H26, &H27, &H28,
            &H29, &H2A, &H34, &H35, &H36, &H37, &H38, &H39, &H3A, &H43, &H44, &H45, &H46, &H47, &H48, &H49,
            &H4A, &H53, &H54, &H55, &H56, &H57, &H58, &H59, &H5A, &H63, &H64, &H65, &H66, &H67, &H68, &H69,
            &H6A, &H73, &H74, &H75, &H76, &H77, &H78, &H79, &H7A, &H83, &H84, &H85, &H86, &H87, &H88, &H89,
            &H8A, &H92, &H93, &H94, &H95, &H96, &H97, &H98, &H99, &H9A, &HA2, &HA3, &HA4, &HA5, &HA6, &HA7,
            &HA8, &HA9, &HAA, &HB2, &HB3, &HB4, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HC2, &HC3, &HC4, &HC5,
            &HC6, &HC7, &HC8, &HC9, &HCA, &HD2, &HD3, &HD4, &HD5, &HD6, &HD7, &HD8, &HD9, &HDA, &HE1, &HE2,
            &HE3, &HE4, &HE5, &HE6, &HE7, &HE8, &HE9, &HEA, &HF1, &HF2, &HF3, &HF4, &HF5, &HF6, &HF7, &HF8, &HF9, &HFA
        }
        ' ac chrominance
        Public ReadOnly AcChromaBits As Integer() = {0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 119}
        Public ReadOnly AcChromaVals As Integer() = {
            &H0, &H1, &H2, &H3, &H11, &H4, &H5, &H21, &H31, &H6, &H12, &H41, &H51, &H7, &H61, &H71,
            &H13, &H22, &H32, &H81, &H8, &H14, &H42, &H91, &HA1, &HB1, &HC1, &H9, &H23, &H33, &H52, &HF0,
            &H15, &H62, &H72, &HD1, &HA, &H16, &H24, &H34, &HE1, &H25, &HF1, &H17, &H18, &H19, &H1A, &H26,
            &H27, &H28, &H29, &H2A, &H35, &H36, &H37, &H38, &H39, &H3A, &H43, &H44, &H45, &H46, &H47, &H48,
            &H49, &H4A, &H53, &H54, &H55, &H56, &H57, &H58, &H59, &H5A, &H63, &H64, &H65, &H66, &H67, &H68,
            &H69, &H6A, &H73, &H74, &H75, &H76, &H77, &H78, &H79, &H7A, &H82, &H83, &H84, &H85, &H86, &H87,
            &H88, &H89, &H8A, &H92, &H93, &H94, &H95, &H96, &H97, &H98, &H99, &H9A, &HA2, &HA3, &HA4, &HA5,
            &HA6, &HA7, &HA8, &HA9, &HAA, &HB2, &HB3, &HB4, &HB5, &HB6, &HB7, &HB8, &HB9, &HBA, &HC2, &HC3,
            &HC4, &HC5, &HC6, &HC7, &HC8, &HC9, &HCA, &HD2, &HD3, &HD4, &HD5, &HD6, &HD7, &HD8, &HD9, &HDA,
            &HE2, &HE3, &HE4, &HE5, &HE6, &HE7, &HE8, &HE9, &HEA, &HF2, &HF3, &HF4, &HF5, &HF6, &HF7, &HF8, &HF9, &HFA
        }

        Public Function Clamp8(v As Integer) As Byte
            If v < 0 Then Return 0
            If v > 255 Then Return 255
            Return CByte(v)
        End Function

        ''' <summary>
        ''' extend the received <paramref name="v"/> of <paramref name="n"/> bits to a
        ''' signed integer, following the jpeg EXTEND procedure.
        ''' </summary>
        Public Function Extend(v As Integer, n As Integer) As Integer
            If n = 0 Then
                Return 0
            End If

            Dim vt As Integer = 1 << (n - 1)

            If v < vt Then
                v = v + (-1 << n) + 1
            End If

            Return v
        End Function

        ' ---- color conversion ----
        Public Sub YCbCrToRGB(y As Integer, cb As Integer, cr As Integer, ByRef r As Integer, ByRef g As Integer, ByRef b As Integer)
            Dim c As Integer = y - 128
            Dim d As Integer = cb - 128
            Dim e As Integer = cr - 128

            r = y + CInt(1.402 * e)
            g = y - CInt(0.344136 * d) - CInt(0.714136 * e)
            b = y + CInt(1.772 * d)
        End Sub

        Public Sub RGBToYCbCr(r As Integer, g As Integer, b As Integer, ByRef y As Integer, ByRef cb As Integer, ByRef cr As Integer)
            y = CInt(0.299 * r + 0.587 * g + 0.114 * b)
            cb = CInt(-0.168736 * r - 0.331264 * g + 0.5 * b) + 128
            cr = CInt(0.5 * r - 0.418688 * g - 0.081312 * b) + 128
        End Sub

        ' ---- DCT ----
        ''' <summary>
        ''' forward 2D 8x8 dct, in place on <paramref name="block"/>.
        ''' </summary>
        Public Sub FDCT(block() As Double)
            Dim tmp(63) As Double
            Dim col(7) As Double
            Dim work(7) As Double

            For y As Integer = 0 To 7
                For x As Integer = 0 To 7
                    work(x) = block(y * 8 + x)
                Next

                Call dct1d(work, 0, tmp, y * 8)
            Next

            For x As Integer = 0 To 7
                For y As Integer = 0 To 7
                    col(y) = tmp(y * 8 + x)
                Next

                Call dct1d(col, 0, work, 0)

                For y As Integer = 0 To 7
                    block(y * 8 + x) = work(y)
                Next
            Next
        End Sub

        ''' <summary>
        ''' inverse 2D 8x8 dct, in place on <paramref name="block"/>, the result is
        ''' level shifted by +128 and clamped into the destination byte buffer.
        ''' </summary>
        Public Sub IDCT(block() As Double, dst() As Byte, dstOff As Integer)
            Dim tmp(63) As Double
            Dim col(7) As Double
            Dim work(7) As Double

            For y As Integer = 0 To 7
                For x As Integer = 0 To 7
                    work(x) = block(y * 8 + x)
                Next

                Call idct1d(work, 0, tmp, y * 8)
            Next

            For x As Integer = 0 To 7
                For y As Integer = 0 To 7
                    col(y) = tmp(y * 8 + x)
                Next

                Call idct1d(col, 0, work, 0)

                For y As Integer = 0 To 7
                    dst(dstOff + y * 8 + x) = Clamp8(CInt(work(y)) + 128)
                Next
            Next
        End Sub

        Private Sub dct1d(inp() As Double, inOff As Integer, out() As Double, outOff As Integer)
            For x As Integer = 0 To 7
                Dim sum As Double = 0.0

                For u As Integer = 0 To 7
                    Dim cu As Double = If(u = 0, 0.7071067811865476, 1.0)
                    sum += cu * inp(inOff + u) * System.Math.Cos((2 * x + 1) * u * System.Math.PI / 16.0)
                Next

                out(outOff + x) = sum / 2.0
            Next
        End Sub

        Private Sub idct1d(inp() As Double, inOff As Integer, out() As Double, outOff As Integer)
            For x As Integer = 0 To 7
                Dim sum As Double = 0.0

                For u As Integer = 0 To 7
                    Dim cu As Double = If(u = 0, 0.7071067811865476, 1.0)
                    sum += cu * inp(inOff + u) * System.Math.Cos((2 * x + 1) * u * System.Math.PI / 16.0)
                Next

                out(outOff + x) = sum / 2.0
            Next
        End Sub

    End Module

    ''' <summary>
    ''' msb first bit reader for the jpeg entropy coded data. it transparently
    ''' removes the 0xff00 byte stuffing and surfaces restart / eoi markers
    ''' through the <see cref="Marker"/> property.
    ''' </summary>
    Public Class JpegBitReader

        Private ReadOnly data As Byte()
        Private pos As Integer
        Private ReadOnly endPos As Integer
        Private bitBuffer As Integer
        Private bitCount As Integer

        ''' <summary>
        ''' the marker byte following a 0xff escape, or -1 when none is pending.
        ''' a restart / eoi / sos marker is surfaced here when the entropy
        ''' decoder reaches the byte boundary of the enclosing scan.
        ''' </summary>
        Public Marker As Integer = -1

        ''' <summary>
        ''' the index of the 0xff byte of the currently surfaced <see cref="Marker"/>.
        ''' </summary>
        Public MarkerStart As Integer = -1

        Public Sub New(data As Byte(), start As Integer, length As Integer)
            Me.data = data
            Me.pos = start
            Me.endPos = start + length
            Me.bitBuffer = 0
            Me.bitCount = 0
        End Sub

        ''' <summary>
        ''' the current byte index into the source data (right after the last
        ''' consumed byte).
        ''' </summary>
        Public ReadOnly Property Position As Integer
            Get
                Return pos
            End Get
        End Property

        ''' <summary>
        ''' advance the reader past the currently surfaced <see cref="Marker"/> (a
        ''' restart marker). call this after resetting the dc predictors so the next
        ''' call to the entropy decoder continues on the byte after the marker.
        ''' </summary>
        Public Sub ConsumeMarker()
            If MarkerStart >= 0 Then
                pos = MarkerStart + 2
            End If

            Marker = -1
        End Sub

        Public Function ReadBit() As Integer
            If bitCount = 0 Then
                Call FillByte()
            End If

            bitCount -= 1

            Return (bitBuffer >> bitCount) And 1
        End Function

        ''' <summary>
        ''' read <paramref name="count"/> bits as an unsigned integer (msb first).
        ''' </summary>
        Public Function ReadBits(count As Integer) As Integer
            Dim v As Integer = 0

            For i As Integer = 0 To count - 1
                v = (v << 1) Or ReadBit()
            Next

            Return v
        End Function

        ''' <summary>
        ''' advance to the next real marker (handling 0xff00 byte stuffing) and
        ''' surface it through <see cref="Marker"/> / <see cref="MarkerStart"/>.
        ''' used to locate the end of a scan when the exact byte count is unknown.
        ''' </summary>
        Public Sub SyncToMarker()
            While pos < endPos
                Dim b As Integer = data(pos)

                If b <> &HFF Then
                    pos += 1
                    Continue While
                End If

                If pos + 1 >= endPos Then
                    Marker = -1
                    MarkerStart = -1
                    Return
                End If

                Dim b2 As Integer = data(pos + 1)

                If b2 = &H0 Then
                    ' stuffed 0xff, skip both bytes
                    pos += 2
                    Continue While
                End If

                Marker = b2
                MarkerStart = pos
                pos += 2
                Return
            End While

            Marker = -1
            MarkerStart = -1
        End Sub

        Private Sub FillByte()
            Dim b As Integer

            If pos >= endPos Then
                ' no more data: behave as if fill bits (all ones)
                bitBuffer = &HFF
                bitCount = 8
                Return
            End If

            b = data(pos)
            pos += 1

            If b = &HFF Then
                If pos >= endPos Then
                    bitBuffer = &HFF
                    bitCount = 8
                    Return
                End If

                Dim b2 As Integer = data(pos)

                If b2 = &H0 Then
                    ' stuffed 0xff
                    b = &HFF
                    pos += 1
                Else
                    ' marker (rst / eoi / next sos). we are byte aligned, surface
                    ' it and resume decoding from the byte after the marker.
                    Marker = b2
                    MarkerStart = pos - 1
                    bitBuffer = 0
                    bitCount = 0
                    Return
                End If
            End If

            bitBuffer = b
            bitCount = 8
        End Sub

    End Class

    ''' <summary>
    ''' msb first bit writer for the jpeg entropy coded data, the 0xff00 byte
    ''' stuffing is performed automatically.
    ''' </summary>
    Public Class JpegBitWriter

        Private ReadOnly out As Stream
        Private bitBuffer As Integer
        Private bitCount As Integer

        Public Sub New(out As Stream)
            Me.out = out
            Me.bitBuffer = 0
            Me.bitCount = 0
        End Sub

        Public Sub WriteBits(value As Integer, length As Integer)
            bitBuffer = (bitBuffer << length) Or (value And ((1 << length) - 1))
            bitCount += length

            While bitCount >= 8
                Dim b As Integer = (bitBuffer >> (bitCount - 8)) And &HFF
                bitCount -= 8
                Call out.WriteByte(CByte(b))

                If b = &HFF Then
                    Call out.WriteByte(0)
                End If
            End While

            bitBuffer = bitBuffer And ((1 << bitCount) - 1)
        End Sub

        Public Sub Flush()
            If bitCount > 0 Then
                Dim b As Integer = (bitBuffer << (8 - bitCount)) And &HFF
                Call out.WriteByte(CByte(b))

                If b = &HFF Then
                    Call out.WriteByte(0)
                End If

                bitCount = 0
            End If
        End Sub

    End Class

    ''' <summary>
    ''' a huffman table that supports both decoding (from the entropy stream)
    ''' and encoding (to the entropy stream) built from the standard jpeg
    ''' (bits, values) description.
    ''' </summary>
    Public Class HuffmanTable

        Private ReadOnly bits(15) As Integer
        Private ReadOnly vals() As Integer

        ' decode tables
        Private ReadOnly mincode(16) As Integer
        Private ReadOnly maxcode(16) As Integer
        Private ReadOnly valptr(16) As Integer

        ' encode tables
        Private ReadOnly ehufco(255) As Integer
        Private ReadOnly ehufsi(255) As Integer

        ''' <summary>
        ''' build a huffman table from the (bits, values) arrays. the index of
        ''' <paramref name="bits"/> is the code length - 1.
        ''' </summary>
        Public Sub New(bits() As Integer, vals() As Integer)
            For i As Integer = 0 To 15
                Me.bits(i) = bits(i)
            Next

            Me.vals = vals

            Call BuildDecode()
            Call BuildEncode()
        End Sub

        Private Sub BuildDecode()
            Dim code As Integer = 0
            Dim k As Integer = 0

            For l As Integer = 1 To 16
                If bits(l - 1) = 0 Then
                    maxcode(l) = -1
                Else
                    valptr(l) = k
                    mincode(l) = code
                    code += bits(l - 1)
                    maxcode(l) = code - 1
                    k += bits(l - 1)
                End If

                code <<= 1
            Next
        End Sub

        Private Sub BuildEncode()
            Dim code As Integer = 0
            Dim k As Integer = 0

            For l As Integer = 1 To 16
                For i As Integer = 0 To bits(l - 1) - 1
                    Dim symbol As Integer = vals(k)
                    ehufco(symbol) = code
                    ehufsi(symbol) = l
                    code += 1
                    k += 1
                Next

                code <<= 1
            Next
        End Sub

        ''' <summary>
        ''' decode one symbol from the reader.
        ''' </summary>
        Public Function Decode(reader As JpegBitReader) As Integer
            Dim code As Integer = 0

            For l As Integer = 1 To 16
                code = (code << 1) Or reader.ReadBit()

                If maxcode(l) >= 0 AndAlso code >= mincode(l) AndAlso code <= maxcode(l) Then
                    Return vals(valptr(l) + (code - mincode(l)))
                End If
            Next

            Throw New InvalidDataException("jpeg huffman decode failure")
        End Function

        ''' <summary>
        ''' write the code for the given <paramref name="symbol"/> to the writer.
        ''' </summary>
        Public Sub Encode(writer As JpegBitWriter, symbol As Integer)
            Call writer.WriteBits(ehufco(symbol), ehufsi(symbol))
        End Sub

        ''' <summary>
        ''' the number of bits used to encode the symbol, 0 when unknown.
        ''' </summary>
        Public Function CodeLength(symbol As Integer) As Integer
            Return ehufsi(symbol)
        End Function

        ''' <summary>
        ''' write the dht segment payload (without the ffc4 marker and the length
        ''' field) for this table into <paramref name="out"/>.
        ''' </summary>
        ''' <param name="tc">0 = dc, 1 = ac</param>
        ''' <param name="th">the table id (0..3)</param>
        Public Sub WriteDHT(out As Stream, tc As Integer, th As Integer)
            Call out.WriteByte(CByte((tc << 4) Or th))

            For i As Integer = 0 To 15
                Call out.WriteByte(CByte(bits(i)))
            Next

            For i As Integer = 0 To vals.Length - 1
                Call out.WriteByte(CByte(vals(i)))
            Next
        End Sub

    End Class

End Namespace
