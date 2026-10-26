#Region "Microsoft.VisualBasic::01f2c1b80e3a4e2fa8a1f3b4c5d6e7f80, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Png\PngCodec.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Enumeration PngFilterType
    ' 
    '         Adaptive, Average, None, Paeth, Sub, Up
    ' 
    '     Module PngCodec
    ' 
    '         Function: Adler32, BitsToBytes, ChannelsOf, Crc32, ImageTag,
    '                   IsValidImageFormat, PassHeight, PassWidth, Paeth,
    '                   ReadUInt32BE, ZlibDeflate, ZlibInflate
    ' 
    '         Sub: WriteChunk, WriteInt32BE
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Text

Namespace Imaging.BitmapImage.Png

    ''' <summary>
    ''' The scanline filter type of the png image data, the filter is applied 
    ''' on the raw scanline data before the zlib compression.
    ''' </summary>
    Public Enum PngFilterType As Byte
        ''' <summary>
        ''' The raw scanline data is stored as it is.
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Each byte is stored as the difference to the byte on its left.
        ''' </summary>
        [Sub] = 1
        ''' <summary>
        ''' Each byte is stored as the difference to the byte above it.
        ''' </summary>
        Up = 2
        ''' <summary>
        ''' Each byte is stored as the difference to the average of the byte 
        ''' on its left and the byte above it.
        ''' </summary>
        Average = 3
        ''' <summary>
        ''' Each byte is stored as the difference to the paeth predictor of the 
        ''' left, the above and the upper left byte.
        ''' </summary>
        Paeth = 4
        ''' <summary>
        ''' Select the best filter of each scanline automatically via the 
        ''' minimum sum of the absolute differences heuristic.
        ''' </summary>
        Adaptive = 5
    End Enum

    ''' <summary>
    ''' The shared low level helper of the png image codec: the file signature, 
    ''' the big endian integer reader/writer, the CRC32 and the Adler32 checksum, 
    ''' the zlib stream inflate/deflate, the chunk writer and the Adam7 
    ''' interlace grid.
    ''' </summary>
    ''' <remarks>
    ''' The <see cref="PngReader"/> and the <see cref="PngWriter"/> are built 
    ''' on top of this module, so that the checksum and the zlib stream would 
    ''' not be implemented twice.
    ''' </remarks>
    Public Module PngCodec

        ''' <summary>
        ''' The 8 bytes png file signature
        ''' </summary>
        Friend ReadOnly Signature As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}

        ' the Adam7 interlace grid: {xStart, yStart, xStep, yStep} of the 7 passes
        Friend ReadOnly Adam7XStart As Integer() = {0, 4, 0, 2, 0, 1, 0}
        Friend ReadOnly Adam7YStart As Integer() = {0, 0, 4, 0, 2, 0, 1}
        Friend ReadOnly Adam7XStep As Integer() = {8, 8, 4, 4, 2, 2, 1}
        Friend ReadOnly Adam7YStep As Integer() = {8, 8, 8, 4, 4, 2, 2}

        Private ReadOnly CrcTable As UInteger() = BuildCrcTable()

        ''' <summary>
        ''' The CRC32 checksum (aka. the CRC-32/ISO-HDLC) of the input data buffer.
        ''' </summary>
        ''' <param name="data">the input data buffer, could be nothing.</param>
        ''' <returns>the 32bits unsigned CRC32 checksum value.</returns>
        Public Function Crc32(data As Byte()) As UInteger
            Return Crc32(data, 0, If(data Is Nothing, 0, data.Length))
        End Function

        ''' <summary>
        ''' The CRC32 checksum of a slice of the input data buffer.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="offset">the zero based offset of the first byte.</param>
        ''' <param name="count">the number of the bytes that would be scanned.</param>
        ''' <returns></returns>
        Public Function Crc32(data As Byte(), offset As Integer, count As Integer) As UInteger
            Dim crc As UInteger = &HFFFFFFFFUI

            For i As Integer = offset To offset + count - 1
                crc = CrcTable(CInt((crc Xor CUInt(data(i))) And &HFFUI)) Xor (crc >> 8)
            Next

            Return crc Xor &HFFFFFFFFUI
        End Function

        ''' <summary>
        ''' The Adler32 checksum of the input data buffer.
        ''' </summary>
        ''' <param name="data">the input data buffer, could be nothing.</param>
        ''' <returns>the 32bits unsigned Adler32 checksum value.</returns>
        Public Function Adler32(data As Byte()) As UInteger
            Return Adler32(data, 0, If(data Is Nothing, 0, data.Length))
        End Function

        ''' <summary>
        ''' The Adler32 checksum of a slice of the input data buffer.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="offset"></param>
        ''' <param name="count"></param>
        ''' <returns></returns>
        Public Function Adler32(data As Byte(), offset As Integer, count As Integer) As UInteger
            Dim a As UInteger = 1UI
            Dim b As UInteger = 0UI

            For i As Integer = offset To offset + count - 1
                a = (a + CUInt(data(i))) Mod 65521UI
                b = (b + a) Mod 65521UI
            Next

            Return (b << 16) Or a
        End Function

        ''' <summary>
        ''' Read a big endian 32bits unsigned integer from the data buffer.
        ''' </summary>
        ''' <remarks>
        ''' the png file stores all of the 32bits integer in the big endian 
        ''' byte order, this reader is required for the image data that is 
        ''' created on a big endian platform as well.
        ''' </remarks>
        Friend Function ReadUInt32BE(data As Byte(), offset As Integer) As UInteger
            Return (CUInt(data(offset)) << 24) Or
                   (CUInt(data(offset + 1)) << 16) Or
                   (CUInt(data(offset + 2)) << 8) Or
                   CUInt(data(offset + 3))
        End Function

        ''' <summary>
        ''' Write a big endian 32bits integer into the target buffer.
        ''' </summary>
        Friend Sub WriteInt32BE(buffer As Byte(), offset As Integer, value As Integer)
            buffer(offset) = CByte((value >> 24) And &HFF)
            buffer(offset + 1) = CByte((value >> 16) And &HFF)
            buffer(offset + 2) = CByte((value >> 8) And &HFF)
            buffer(offset + 3) = CByte(value And &HFF)
        End Sub

        ''' <summary>
        ''' Write a png chunk: the 4 bytes length, the 4 bytes chunk type, 
        ''' the payload data and the 4 bytes CRC32 checksum of the chunk type 
        ''' plus the payload data.
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <param name="type">the 4 chars chunk type name, for example IHDR, IDAT.</param>
        ''' <param name="payload">the chunk data, could be nothing for the empty chunk.</param>
        Friend Sub WriteChunk(stream As Stream, type As String, payload As Byte())
            Dim typeBytes As Byte() = Encoding.ASCII.GetBytes(type)
            Dim length As Byte() = New Byte(3) {}
            Dim size As Integer = If(payload Is Nothing, 0, payload.Length)

            Call WriteInt32BE(length, 0, size)
            Call stream.Write(length, 0, 4)
            Call stream.Write(typeBytes, 0, 4)

            If size > 0 Then
                Call stream.Write(payload, 0, size)
            End If

            Dim crc As UInteger = &HFFFFFFFFUI

            For i As Integer = 0 To 3
                crc = CrcTable(CInt((crc Xor CUInt(typeBytes(i))) And &HFFUI)) Xor (crc >> 8)
            Next
            For i As Integer = 0 To size - 1
                crc = CrcTable(CInt((crc Xor CUInt(payload(i))) And &HFFUI)) Xor (crc >> 8)
            Next

            crc = crc Xor &HFFFFFFFFUI

            Dim crcBytes As Byte() = New Byte(3) {}

            crcBytes(0) = CByte((crc >> 24) And &HFFUI)
            crcBytes(1) = CByte((crc >> 16) And &HFFUI)
            crcBytes(2) = CByte((crc >> 8) And &HFFUI)
            crcBytes(3) = CByte(crc And &HFFUI)

            Call stream.Write(crcBytes, 0, 4)
        End Sub

        ''' <summary>
        ''' Inflate the zlib stream of the IDAT chunk payload.
        ''' </summary>
        ''' <param name="compressed">the concatenated payload of all IDAT chunks.</param>
        ''' <param name="expected">the exact byte length of the inflated scanline data.</param>
        ''' <param name="tag">the image file path or the "&lt;stream&gt;" tag, used by the error message.</param>
        ''' <returns>the inflated raw scanline data, could not be nothing.</returns>
        Friend Function ZlibInflate(compressed As Byte(), expected As Integer, tag As String) As Byte()
            Dim raw As Byte() = New Byte(expected - 1) {}
            Dim read As Integer = 0

            Using input As New MemoryStream(compressed, writable:=False)
                Using zlib As New ZLibStream(input, CompressionMode.Decompress)
                    While read < expected
                        Dim n As Integer = zlib.Read(raw, read, expected - read)

                        If n <= 0 Then
                            Exit While
                        End If

                        read += n
                    End While
                End Using
            End Using

            If read <> expected Then
                Throw New InvalidDataException($"the inflated png image data is truncated: {read} of {expected} bytes, {ImageTag(tag)}")
            End If

            Return raw
        End Function

        ''' <summary>
        ''' Deflate the raw scanline data into a zlib stream.
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="level">the compression level of the deflate stream.</param>
        ''' <returns>the zlib stream bytes, includes the 2 bytes zlib header and the 4 bytes Adler32 checksum.</returns>
        Friend Function ZlibDeflate(raw As Byte(), level As CompressionLevel) As Byte()
            Using output As New MemoryStream()
                Using zlib As New ZLibStream(output, level, leaveOpen:=True)
                    Call zlib.Write(raw, 0, raw.Length)
                End Using

                Return output.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' The paeth predictor of the png scanline filter.
        ''' </summary>
        Friend Function Paeth(a As Integer, b As Integer, c As Integer) As Integer
            Dim p As Integer = a + b - c
            Dim pa As Integer = System.Math.Abs(p - a)
            Dim pb As Integer = System.Math.Abs(p - b)
            Dim pc As Integer = System.Math.Abs(p - c)

            If pa <= pb AndAlso pa <= pc Then
                Return a
            ElseIf pb <= pc Then
                Return b
            Else
                Return c
            End If
        End Function

        ''' <summary>
        ''' The number of the color channels that are stored in the png file 
        ''' for the given color type.
        ''' </summary>
        ''' <param name="colorType">
        ''' 0 = gray, 2 = rgb, 3 = the palette index, 4 = gray + alpha, 6 = rgba.
        ''' </param>
        ''' <returns></returns>
        Friend Function ChannelsOf(colorType As Integer) As Integer
            Select Case colorType
                Case 0 : Return 1
                Case 2 : Return 3
                Case 3 : Return 1
                Case 4 : Return 2
                Case 6 : Return 4
                Case Else
                    Throw New NotSupportedException($"unsupported png color type {colorType}!")
            End Select
        End Function

        ''' <summary>
        ''' Checks that the bit depth and the color type of the IHDR chunk is a 
        ''' legal combination of the png specification.
        ''' </summary>
        Friend Function IsValidImageFormat(colorType As Integer, bitDepth As Integer) As Boolean
            Select Case colorType
                Case 0
                    Return bitDepth = 1 OrElse bitDepth = 2 OrElse bitDepth = 4 OrElse bitDepth = 8 OrElse bitDepth = 16
                Case 3
                    Return bitDepth = 1 OrElse bitDepth = 2 OrElse bitDepth = 4 OrElse bitDepth = 8
                Case 2, 4, 6
                    Return bitDepth = 8 OrElse bitDepth = 16
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' The width of an Adam7 interlace pass
        ''' </summary>
        ''' <param name="imageWidth">the full image width.</param>
        ''' <param name="xStart">the x offset of the first pixel of the pass.</param>
        ''' <param name="xStep">the x distance between the pixels of the pass.</param>
        ''' <returns>zero when the pass does not contain any pixel.</returns>
        Friend Function PassWidth(imageWidth As Integer, xStart As Integer, xStep As Integer) As Integer
            If imageWidth <= xStart Then Return 0

            Return (imageWidth - xStart + xStep - 1) \ xStep
        End Function

        ''' <summary>
        ''' The height of an Adam7 interlace pass
        ''' </summary>
        ''' <param name="imageHeight">the full image height.</param>
        ''' <param name="yStart">the y offset of the first pixel of the pass.</param>
        ''' <param name="yStep">the y distance between the pixels of the pass.</param>
        ''' <returns>zero when the pass does not contain any pixel.</returns>
        Friend Function PassHeight(imageHeight As Integer, yStart As Integer, yStep As Integer) As Integer
            If imageHeight <= yStart Then Return 0

            Return (imageHeight - yStart + yStep - 1) \ yStep
        End Function

        ''' <summary>
        ''' The number of the bytes that are required for storing the given bits.
        ''' </summary>
        Friend Function BitsToBytes(bits As Long) As Integer
            Return CInt((bits + 7) \ 8)
        End Function

        ''' <summary>
        ''' The image file path or the "&lt;stream&gt;" tag of an image that is 
        ''' loaded from a stream.
        ''' </summary>
        Friend Function ImageTag(tag As String) As String
            Return If(String.IsNullOrEmpty(tag), "<stream>", tag)
        End Function

        Private Function BuildCrcTable() As UInteger()
            Dim table As UInteger() = New UInteger(255) {}

            For i As Integer = 0 To 255
                Dim c As UInteger = CUInt(i)

                For k As Integer = 0 To 7
                    If (c And 1UI) <> 0UI Then
                        c = &HEDB88320UI Xor (c >> 1)
                    Else
                        c >>= 1
                    End If
                Next

                table(i) = c
            Next

            Return table
        End Function
    End Module

End Namespace
