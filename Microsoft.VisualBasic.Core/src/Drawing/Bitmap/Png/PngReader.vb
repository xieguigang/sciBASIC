#Region "Microsoft.VisualBasic::7d8e9f0a1b2c3d4e5f60718293a4b5c6, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Png\PngReader.vb"

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

    '     Module PngReader
    ' 
    '         Function: Decode, DecodeImage, IsUsableTrns, Load, OutputChannels,
    '                   ScaleTrnsValue
    ' 
    '         Sub: ConvertPass, UnfilterPass
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text

Namespace Imaging.BitmapImage.Png

    ''' <summary>
    ''' A dependency free PNG decoder, the 1/2/4/8/16 bits per channel image, 
    ''' the gray/rgb/palette/gray alpha/rgba color type and the Adam7 interlaced 
    ''' image are all supported.
    ''' </summary>
    Public Module PngReader

        ''' <summary>
        ''' load a png image file
        ''' </summary>
        ''' <param name="path">the file path of the png image file.</param>
        ''' <param name="verifyCrc">
        ''' verify the CRC32 checksum of every png chunk or not, a corrupted 
        ''' image is rejected with an <see cref="InvalidDataException"/> when 
        ''' this option is enabled.
        ''' </param>
        ''' <param name="expandTransparency">
        ''' expand the transparency of the tRNS chunk into an alpha channel or 
        ''' not, it is disabled by default so that the image that is rendered 
        ''' with the alpha channel would not be broken by the auto detection of 
        ''' the <see cref="PngImage.GrayInAlpha"/> flag.
        ''' </param>
        ''' <returns>the decoded raster image data.</returns>
        Public Function Load(path As String, Optional verifyCrc As Boolean = False,
                             Optional expandTransparency As Boolean = False) As PngImage
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Return DecodeImage(File.ReadAllBytes(path), verifyCrc, expandTransparency, path)
        End Function

        ''' <summary>
        ''' load a png image from a stream
        ''' </summary>
        ''' <param name="stream">the png image data stream, the stream is read 
        ''' into the memory at once.</param>
        ''' <param name="verifyCrc">verify the CRC32 checksum of every png chunk or not.</param>
        ''' <param name="expandTransparency">expand the transparency of the tRNS chunk into an alpha channel or not.</param>
        ''' <returns>the decoded raster image data.</returns>
        Public Function Load(stream As Stream, Optional verifyCrc As Boolean = False,
                             Optional expandTransparency As Boolean = False) As PngImage
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If

            Using buffer As New MemoryStream()
                Call stream.CopyTo(buffer)

                Return DecodeImage(buffer.ToArray(), verifyCrc, expandTransparency, Nothing)
            End Using
        End Function

        ''' <summary>
        ''' decode the png image file data
        ''' </summary>
        ''' <param name="data">the png image file bytes.</param>
        ''' <param name="verifyCrc"></param>
        ''' <param name="expandTransparency"></param>
        ''' <returns>the decoded raster image data.</returns>
        Public Function Decode(data As Byte(), Optional verifyCrc As Boolean = False,
                               Optional expandTransparency As Boolean = False) As PngImage
            Return DecodeImage(data, verifyCrc, expandTransparency, Nothing)
        End Function

        ''' <summary>
        ''' decode a png image file into the normalized raster image data
        ''' </summary>
        ''' <param name="path">the file path of the png image file.</param>
        ''' <param name="toGray">true for converting the rgb color into the gray 
        ''' scale intensity, false for keeping the 3 channels rgb color.</param>
        ''' <returns>
        ''' a <see cref="RasterImage"/> object, the pixel values are normalized 
        ''' into the range [0,1], it could be feed into the numeric model 
        ''' directly.
        ''' </returns>
        Public Function Decode(path As String, Optional toGray As Boolean = True) As RasterImage
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Return Load(path).ToRaster(toGray)
        End Function

        ''' <summary>
        ''' the png image decoding pipeline: the signature check, the chunk 
        ''' walking, the IHDR validation, the IDAT inflate, the scan line 
        ''' unfilter, the Adam7 de-interlace and the pixel expansion.
        ''' </summary>
        Private Function DecodeImage(data As Byte(), verifyCrc As Boolean,
                                     expandTransparency As Boolean, tag As String) As PngImage
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If data.Length < 8 Then
                Throw New InvalidDataException($"too short to be a png image data: {PngCodec.ImageTag(tag)}")
            End If

            For i As Integer = 0 To 7
                If data(i) <> PngCodec.Signature(i) Then
                    Throw New InvalidDataException($"invalid png image signature: {PngCodec.ImageTag(tag)}")
                End If
            Next

            Dim width As Long = 0
            Dim height As Long = 0
            Dim bitDepth As Integer = 0
            Dim colorType As Integer = -1
            Dim interlace As Integer = 0
            Dim compression As Integer = 0
            Dim filterMethod As Integer = 0
            Dim palette As Byte() = Nothing
            Dim trns As Byte() = Nothing
            Dim hasIhdr As Boolean = False
            Dim idat As New MemoryStream()
            Dim pos As Integer = 8

            While pos + 8 <= data.Length
                Dim length As Long = CLng(PngCodec.ReadUInt32BE(data, pos))
                Dim type As String = Encoding.ASCII.GetString(data, pos + 4, 4)
                Dim payload As Integer = pos + 8

                If length > Integer.MaxValue Then
                    Throw New InvalidDataException($"the png chunk [{type}] is too large: {length} bytes, {PngCodec.ImageTag(tag)}")
                End If
                If payload + length + 4 > data.Length Then
                    Throw New InvalidDataException($"the png image data is truncated at the chunk [{type}]: {PngCodec.ImageTag(tag)}")
                End If

                If verifyCrc Then
                    Dim stored As UInteger = PngCodec.ReadUInt32BE(data, payload + CInt(length))
                    Dim actual As UInteger = PngCodec.Crc32(data, pos + 4, CInt(length) + 4)

                    If stored <> actual Then
                        Throw New InvalidDataException($"the CRC32 checksum of the png chunk [{type}] is mismatched: {PngCodec.ImageTag(tag)}")
                    End If
                End If

                Select Case type
                    Case "IHDR"
                        If length <> 13 Then
                            Throw New InvalidDataException($"illegal IHDR chunk size {length} bytes, {PngCodec.ImageTag(tag)}")
                        End If

                        width = CLng(PngCodec.ReadUInt32BE(data, payload))
                        height = CLng(PngCodec.ReadUInt32BE(data, payload + 4))
                        bitDepth = data(payload + 8)
                        colorType = data(payload + 9)
                        compression = data(payload + 10)
                        filterMethod = data(payload + 11)
                        interlace = data(payload + 12)
                        hasIhdr = True
                    Case "PLTE"
                        palette = New Byte(CInt(length) - 1) {}
                        Call Array.Copy(data, payload, palette, 0, CInt(length))
                    Case "tRNS"
                        trns = New Byte(CInt(length) - 1) {}
                        Call Array.Copy(data, payload, trns, 0, CInt(length))
                    Case "IDAT"
                        Call idat.Write(data, payload, CInt(length))
                    Case "IEND"
                        Exit While
                End Select

                pos = payload + CInt(length) + 4
            End While

            If Not hasIhdr Then
                Throw New InvalidDataException($"no IHDR chunk was found in {PngCodec.ImageTag(tag)}")
            End If
            If width <= 0 OrElse height <= 0 OrElse width > Integer.MaxValue OrElse height > Integer.MaxValue Then
                Throw New InvalidDataException($"illegal png image size {width}x{height}: {PngCodec.ImageTag(tag)}")
            End If
            If compression <> 0 Then
                Throw New NotSupportedException($"the png compression method {compression} is not supported: {PngCodec.ImageTag(tag)}")
            End If
            If filterMethod <> 0 Then
                Throw New NotSupportedException($"the png filter method {filterMethod} is not supported: {PngCodec.ImageTag(tag)}")
            End If
            If Not PngCodec.IsValidImageFormat(colorType, bitDepth) Then
                Throw New InvalidDataException($"illegal png image format: bit depth = {bitDepth}, color type = {colorType}, {PngCodec.ImageTag(tag)}")
            End If
            If interlace <> 0 AndAlso interlace <> 1 Then
                Throw New NotSupportedException($"the png interlace method {interlace} is not supported: {PngCodec.ImageTag(tag)}")
            End If
            If colorType = 3 AndAlso palette Is Nothing Then
                Throw New InvalidDataException($"the palette png image does not have a PLTE chunk: {PngCodec.ImageTag(tag)}")
            End If
            If idat.Length = 0 Then
                Throw New InvalidDataException($"no IDAT chunk was found in {PngCodec.ImageTag(tag)}")
            End If

            Dim sourceChannels As Integer = PngCodec.ChannelsOf(colorType)
            Dim bitsPerPixel As Integer = bitDepth * sourceChannels
            Dim bpp As Integer = If(bitsPerPixel >= 8, bitsPerPixel \ 8, 1)
            Dim useTrns As Boolean = expandTransparency AndAlso IsUsableTrns(trns, colorType)
            Dim outChannels As Integer = OutputChannels(colorType, useTrns)
            Dim pixelCount As Long = width * height

            If pixelCount * outChannels > Integer.MaxValue Then
                Throw New NotSupportedException($"the png image is too large to be loaded into a single byte buffer: {width}x{height}")
            End If

            Dim w As Integer = CInt(width)
            Dim h As Integer = CInt(height)
            Dim passWidths As Integer() = New Integer(6) {}
            Dim passHeights As Integer() = New Integer(6) {}
            Dim passStrides As Integer() = New Integer(6) {}
            Dim passCount As Integer = 1

            If interlace = 1 Then
                passCount = 7

                For p As Integer = 0 To 6
                    passWidths(p) = PngCodec.PassWidth(w, PngCodec.Adam7XStart(p), PngCodec.Adam7XStep(p))
                    passHeights(p) = PngCodec.PassHeight(h, PngCodec.Adam7YStart(p), PngCodec.Adam7YStep(p))
                    passStrides(p) = PngCodec.BitsToBytes(CLng(passWidths(p)) * bitsPerPixel)
                Next
            Else
                passWidths(0) = w
                passHeights(0) = h
                passStrides(0) = PngCodec.BitsToBytes(CLng(w) * bitsPerPixel)
            End If

            Dim expected As Long = 0

            For p As Integer = 0 To passCount - 1
                If passWidths(p) = 0 OrElse passHeights(p) = 0 Then
                    Continue For
                End If

                expected += CLng(passStrides(p) + 1) * passHeights(p)
            Next

            If expected <= 0 OrElse expected > Integer.MaxValue Then
                Throw New NotSupportedException($"the png image is too large to be loaded into a single byte buffer: {width}x{height}")
            End If

            Dim raw As Byte() = PngCodec.ZlibInflate(idat.ToArray(), CInt(expected), tag)
            Dim decoded As Byte() = New Byte(CInt(pixelCount * outChannels) - 1) {}
            Dim trnsGray As Integer = 0
            Dim trnsRgb As Integer() = Nothing

            If useTrns Then
                Select Case colorType
                    Case 0
                        trnsGray = ScaleTrnsValue((CInt(trns(0)) << 8) Or CInt(trns(1)), bitDepth)
                    Case 2
                        trnsRgb = {
                            ScaleTrnsValue((CInt(trns(0)) << 8) Or CInt(trns(1)), bitDepth),
                            ScaleTrnsValue((CInt(trns(2)) << 8) Or CInt(trns(3)), bitDepth),
                            ScaleTrnsValue((CInt(trns(4)) << 8) Or CInt(trns(5)), bitDepth)
                        }
                End Select
            End If

            Dim cursor As Integer = 0

            For p As Integer = 0 To passCount - 1
                Dim passW As Integer = passWidths(p)
                Dim passH As Integer = passHeights(p)

                If passW = 0 OrElse passH = 0 Then
                    Continue For
                End If

                Dim x0 As Integer = 0
                Dim y0 As Integer = 0
                Dim xStep As Integer = 1
                Dim yStep As Integer = 1

                If interlace = 1 Then
                    x0 = PngCodec.Adam7XStart(p)
                    y0 = PngCodec.Adam7YStart(p)
                    xStep = PngCodec.Adam7XStep(p)
                    yStep = PngCodec.Adam7YStep(p)
                End If

                Call UnfilterPass(raw, cursor, passH, passStrides(p), bpp)
                Call ConvertPass(raw, cursor, passH, passStrides(p), bitDepth, sourceChannels,
                                 colorType, palette, useTrns, trnsGray, trnsRgb, trns,
                                 decoded, w, outChannels, x0, y0, xStep, yStep, passW)

                cursor += (passStrides(p) + 1) * passH
            Next

            Return New PngImage With {
                .Width = w,
                .Height = h,
                .Channels = outChannels,
                .Pixels = decoded,
                .BitDepth = bitDepth,
                .ColorType = colorType,
                .Interlace = interlace,
                .IdatBytes = If(idat.Length > Integer.MaxValue, Integer.MaxValue, CInt(idat.Length)),
                .RawBytes = raw.Length
            }
        End Function

        ''' <summary>
        ''' remove the scan line filter of one image pass, the data is unfiltered 
        ''' in place so that no extra buffer allocation is required.
        ''' </summary>
        ''' <param name="raw">the inflated raw image data.</param>
        ''' <param name="rowBase">the offset of the first scan line of current pass.</param>
        ''' <param name="rows">the number of the scan lines of current pass.</param>
        ''' <param name="stride">the byte length of a scan line of current pass.</param>
        ''' <param name="bpp">the bytes per pixel, at least 1.</param>
        Private Sub UnfilterPass(raw As Byte(), rowBase As Integer, rows As Integer,
                                 stride As Integer, bpp As Integer)
            For y As Integer = 0 To rows - 1
                Dim rowStart As Integer = rowBase + y * (stride + 1)
                Dim filterType As Integer = raw(rowStart)
                Dim cur As Integer = rowStart + 1
                Dim prev As Integer = cur - stride - 1
                Dim hasPrev As Boolean = (y > 0)

                Select Case filterType
                    Case 0
                        ' the scan line data is stored as it is
                    Case 1
                        For i As Integer = bpp To stride - 1
                            raw(cur + i) = CByte((CInt(raw(cur + i)) + CInt(raw(cur + i - bpp))) And &HFF)
                        Next
                    Case 2
                        If hasPrev Then
                            For i As Integer = 0 To stride - 1
                                raw(cur + i) = CByte((CInt(raw(cur + i)) + CInt(raw(prev + i))) And &HFF)
                            Next
                        End If
                    Case 3
                        For i As Integer = 0 To stride - 1
                            Dim left As Integer = If(i >= bpp, CInt(raw(cur + i - bpp)), 0)
                            Dim up As Integer = If(hasPrev, CInt(raw(prev + i)), 0)

                            raw(cur + i) = CByte((CInt(raw(cur + i)) + ((left + up) >> 1)) And &HFF)
                        Next
                    Case 4
                        For i As Integer = 0 To stride - 1
                            Dim a As Integer = If(i >= bpp, CInt(raw(cur + i - bpp)), 0)
                            Dim b As Integer = If(hasPrev, CInt(raw(prev + i)), 0)
                            Dim c As Integer = If(hasPrev AndAlso i >= bpp, CInt(raw(prev + i - bpp)), 0)

                            raw(cur + i) = CByte((CInt(raw(cur + i)) + PngCodec.Paeth(a, b, c)) And &HFF)
                        Next
                    Case Else
                        Throw New InvalidDataException($"unknown png scan line filter {filterType}")
                End Select
            Next
        End Sub

        ''' <summary>
        ''' expand the samples of one image pass into the 8 bits per channel 
        ''' output buffer at the position that is described by the Adam7 grid.
        ''' </summary>
        Private Sub ConvertPass(raw As Byte(), rowBase As Integer, rows As Integer, stride As Integer,
                                bitDepth As Integer, sourceChannels As Integer, colorType As Integer,
                                palette As Byte(), useTrns As Boolean, trnsGray As Integer,
                                trnsRgb As Integer(), trns As Byte(),
                                decoded As Byte(), width As Integer, outChannels As Integer,
                                x0 As Integer, y0 As Integer, xStep As Integer, yStep As Integer,
                                passWidth As Integer)
            Dim isPalette As Boolean = (colorType = 3)
            Dim mask As Integer = If(bitDepth < 8, (1 << bitDepth) - 1, &HFF)
            Dim scale As Integer = If(bitDepth < 8, 255 \ If(mask = 0, 1, mask), 1)
            Dim sample As Integer() = New Integer(3) {}
            Dim canCopyRow As Boolean = (bitDepth = 8) AndAlso (outChannels = sourceChannels)

            For y As Integer = 0 To rows - 1
                Dim source As Integer = rowBase + y * (stride + 1) + 1
                Dim dstRow As Integer = (y0 + y * yStep) * width

                If canCopyRow AndAlso x0 = 0 AndAlso xStep = 1 Then
                    Call Array.Copy(raw, source, decoded, dstRow * outChannels, stride)
                    Continue For
                End If

                For i As Integer = 0 To passWidth - 1
                    ' the byte offset of the first sample of current pixel: the 
                    ' 16 bits sample occupies 2 bytes but the sample of the bit 
                    ' depth that is less than 8 is packed into the shared bytes
                    Dim sampleBase As Integer = If(bitDepth = 16,
                                                   source + i * sourceChannels * 2,
                                                   source + i * sourceChannels)

                    For c As Integer = 0 To sourceChannels - 1
                        Select Case bitDepth
                            Case 8
                                sample(c) = raw(sampleBase + c)
                            Case 16
                                sample(c) = (CInt(raw(sampleBase + c * 2)) << 8) Or CInt(raw(sampleBase + c * 2 + 1))
                            Case Else
                                Dim bitPos As Integer = (i * sourceChannels + c) * bitDepth
                                Dim shift As Integer = 8 - bitDepth - (bitPos And 7)

                                sample(c) = (CInt(raw(source + (bitPos >> 3))) >> shift) And mask
                        End Select
                    Next

                    If bitDepth = 16 Then
                        For c As Integer = 0 To sourceChannels - 1
                            sample(c) = (sample(c) * 255 + 32895) >> 16
                        Next
                    ElseIf bitDepth < 8 AndAlso Not isPalette Then
                        For c As Integer = 0 To sourceChannels - 1
                            sample(c) = sample(c) * scale
                        Next
                    End If

                    Dim dst As Integer = (dstRow + x0 + i * xStep) * outChannels

                    Select Case colorType
                        Case 0
                            decoded(dst) = CByte(sample(0))

                            If useTrns Then
                                decoded(dst + 1) = If(sample(0) = trnsGray, CByte(0), CByte(255))
                            End If
                        Case 2
                            decoded(dst) = CByte(sample(0))
                            decoded(dst + 1) = CByte(sample(1))
                            decoded(dst + 2) = CByte(sample(2))

                            If useTrns Then
                                Dim hit As Boolean = sample(0) = trnsRgb(0) AndAlso
                                                     sample(1) = trnsRgb(1) AndAlso
                                                     sample(2) = trnsRgb(2)

                                decoded(dst + 3) = If(hit, CByte(0), CByte(255))
                            End If
                        Case 3
                            Dim index As Integer = sample(0) * 3

                            If index < 0 OrElse index + 2 >= palette.Length Then
                                Throw New InvalidDataException($"the png palette index {sample(0)} is out of range of the PLTE chunk")
                            End If

                            decoded(dst) = palette(index)
                            decoded(dst + 1) = palette(index + 1)
                            decoded(dst + 2) = palette(index + 2)

                            If useTrns Then
                                decoded(dst + 3) = If(sample(0) < trns.Length, trns(sample(0)), CByte(255))
                            End If
                        Case 4
                            decoded(dst) = CByte(sample(0))
                            decoded(dst + 1) = CByte(sample(1))
                        Case 6
                            decoded(dst) = CByte(sample(0))
                            decoded(dst + 1) = CByte(sample(1))
                            decoded(dst + 2) = CByte(sample(2))
                            decoded(dst + 3) = CByte(sample(3))
                    End Select
                Next
            Next
        End Sub

        ''' <summary>
        ''' the number of the color channels of the decoded output buffer
        ''' </summary>
        Private Function OutputChannels(colorType As Integer, useTrns As Boolean) As Integer
            Select Case colorType
                Case 0 : Return If(useTrns, 2, 1)
                Case 2 : Return If(useTrns, 4, 3)
                Case 3 : Return If(useTrns, 4, 3)
                Case 4 : Return 2
                Case 6 : Return 4
                Case Else
                    Throw New NotSupportedException($"unsupported png color type {colorType}!")
            End Select
        End Function

        ''' <summary>
        ''' the tRNS chunk could only be applied on the gray, the rgb and the 
        ''' palette image, and the payload size should be large enough.
        ''' </summary>
        Private Function IsUsableTrns(trns As Byte(), colorType As Integer) As Boolean
            If trns Is Nothing Then
                Return False
            End If

            Select Case colorType
                Case 0 : Return trns.Length >= 2
                Case 2 : Return trns.Length >= 6
                Case 3 : Return trns.Length >= 1
                Case Else
                    Return False
            End Select
        End Function

        ''' <summary>
        ''' scale the transparency reference value of the tRNS chunk into the 
        ''' 8 bits sample space.
        ''' </summary>
        Private Function ScaleTrnsValue(value As Integer, bitDepth As Integer) As Integer
            If bitDepth = 16 Then
                Return (value * 255 + 32895) >> 16
            ElseIf bitDepth < 8 Then
                Return value * (255 \ ((1 << bitDepth) - 1))
            Else
                Return value
            End If
        End Function

    End Module

End Namespace
