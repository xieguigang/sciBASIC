#Region "Microsoft.VisualBasic::9f0a1b2c3d4e5f60718293a4b5c6d7e8, Microsoft.VisualBasic.Core\src\Drawing\Bitmap\Png\PngWriter.vb"

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

    '     Module PngWriter
    ' 
    '         Function: Encode, ColorTypeOf, ToGrayBuffer
    ' 
    '         Sub: EncodeFile, Save, WriteGray, WriteImage, WritePgm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression

Namespace Imaging.BitmapImage.Png

    ''' <summary>
    ''' A dependency free PNG encoder, the gray scale, the gray scale with alpha, 
    ''' the true color and the true color with alpha image are all supported, 
    ''' the scan line filter is selected for each row automatically so that the 
    ''' output file would be smaller than the raw scan line storage.
    ''' </summary>
    Public Module PngWriter

        Private ReadOnly AdaptiveFilters As PngFilterType() = {
            PngFilterType.None,
            PngFilterType.[Sub],
            PngFilterType.Up,
            PngFilterType.Average,
            PngFilterType.Paeth
        }

        ''' <summary>
        ''' save a raw pixel buffer as a png file
        ''' </summary>
        ''' <param name="path">the output png file path</param>
        ''' <param name="pixels">the row major raw pixel data</param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="channels">1 = gray, 2 = gray+alpha, 3 = rgb, 4 = rgba</param>
        ''' <param name="rowFilter">
        ''' the scan line filter, <see cref="PngFilterType.Adaptive"/> selects the 
        ''' best filter of each scan line automatically.
        ''' </param>
        ''' <param name="level">the compression level of the deflate stream.</param>
        Public Sub Save(path As String, pixels As Byte(), width As Integer, height As Integer, channels As Integer,
                        Optional rowFilter As PngFilterType = PngFilterType.Adaptive,
                        Optional level As CompressionLevel = CompressionLevel.Optimal)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Using s As Stream = File.Create(path)
                Call Save(s, pixels, width, height, channels, rowFilter, level)
                Call s.Flush()
            End Using
        End Sub

        ''' <summary>
        ''' write a raw pixel buffer as a png image data into the target stream
        ''' </summary>
        ''' <param name="s">the output stream, it is not closed by this function.</param>
        ''' <param name="pixels">the row major raw pixel data</param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="channels">1 = gray, 2 = gray+alpha, 3 = rgb, 4 = rgba</param>
        ''' <param name="rowFilter"></param>
        ''' <param name="level"></param>
        Public Sub Save(s As Stream, pixels As Byte(), width As Integer, height As Integer, channels As Integer,
                        Optional rowFilter As PngFilterType = PngFilterType.Adaptive,
                        Optional level As CompressionLevel = CompressionLevel.Optimal)
            If s Is Nothing Then
                Throw New ArgumentNullException(NameOf(s))
            End If

            Call WriteImage(s, pixels, width, height, channels, rowFilter, level)
        End Sub

        ''' <summary>
        ''' encode a raw pixel buffer as the png image file bytes
        ''' </summary>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="pixels">the row major raw pixel data</param>
        ''' <param name="channels">1 = gray, 2 = gray+alpha, 3 = rgb, 4 = rgba</param>
        ''' <param name="rowFilter"></param>
        ''' <param name="level"></param>
        ''' <returns>the png image file bytes.</returns>
        Public Function Encode(width As Integer, height As Integer, pixels As Byte(), channels As Integer,
                               Optional rowFilter As PngFilterType = PngFilterType.Adaptive,
                               Optional level As CompressionLevel = CompressionLevel.Optimal) As Byte()
            Using buffer As New MemoryStream()
                Call WriteImage(buffer, pixels, width, height, channels, rowFilter, level)

                Return buffer.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' encode a raw rgb pixel buffer as a png image file, the directory of 
        ''' the target file path is created when it does not exist.
        ''' </summary>
        ''' <param name="path">the output png file path</param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="rgb">the row major rgb pixel data</param>
        ''' <param name="rowFilter"></param>
        ''' <param name="level"></param>
        Public Sub EncodeFile(path As String, width As Integer, height As Integer, rgb As Byte(),
                              Optional rowFilter As PngFilterType = PngFilterType.Adaptive,
                              Optional level As CompressionLevel = CompressionLevel.Optimal)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If

            Dim dir As String = IO.Path.GetDirectoryName(path)

            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Call Directory.CreateDirectory(dir)
            End If

            Call Save(path, rgb, width, height, 3, rowFilter, level)
        End Sub

        ''' <summary>
        ''' save a normalized pixel buffer as a 8 bits gray scale png image file
        ''' </summary>
        ''' <param name="path">the output png file path</param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="pixels">
        ''' the normalized pixel values in the range [0,1], or the 0/1 mask 
        ''' values, laid out as <c>y * width + x</c>.
        ''' </param>
        ''' <param name="scale255">the non zero pixels are scaled by this factor, 255 by default.</param>
        ''' <param name="rowFilter"></param>
        ''' <param name="level"></param>
        Public Sub WriteGray(path As String, width As Integer, height As Integer, pixels As Double(),
                             Optional scale255 As Double = 255.0,
                             Optional rowFilter As PngFilterType = PngFilterType.Adaptive,
                             Optional level As CompressionLevel = CompressionLevel.Optimal)
            Call Save(path, ToGrayBuffer(pixels, width, height, scale255), width, height, 1, rowFilter, level)
        End Sub

        ''' <summary>
        ''' save a normalized pixel buffer as a PGM(P5) gray scale image file, 
        ''' it is a much more simpler file format than the png image file.
        ''' </summary>
        ''' <param name="path">the output pgm file path</param>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="pixels">the normalized pixel values in the range [0,1].</param>
        ''' <param name="scale255">the non zero pixels are scaled by this factor, 255 by default.</param>
        Public Sub WritePgm(path As String, width As Integer, height As Integer, pixels As Double(),
                            Optional scale255 As Double = 255.0)
            If String.IsNullOrEmpty(path) Then
                Throw New ArgumentNullException(NameOf(path))
            End If
            If pixels Is Nothing Then
                Throw New ArgumentNullException(NameOf(pixels))
            End If
            If width <= 0 OrElse height <= 0 Then
                Throw New ArgumentOutOfRangeException($"{width}x{height} is not a valid image size")
            End If
            If CLng(width) * height > pixels.Length Then
                Throw New ArgumentException($"the pixel buffer is too small: {pixels.Length} values for a {width}x{height} image")
            End If

            Using fs As New FileStream(path, FileMode.Create, FileAccess.Write)
                Using writer As New StreamWriter(fs)
                    Call writer.Write($"P5{Chr(10)}{width} {height}{Chr(10)}255{Chr(10)}")
                    Call writer.Flush()

                    For i As Integer = 0 To width * height - 1
                        Call fs.WriteByte(ToGrayByte(pixels(i) * scale255))
                    Next
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' the png image encoding pipeline: the scan line filter, the zlib 
        ''' compression and the chunk layout.
        ''' </summary>
        Private Sub WriteImage(s As Stream, pixels As Byte(), width As Integer, height As Integer,
                               channels As Integer, rowFilter As PngFilterType, level As CompressionLevel)
            If pixels Is Nothing Then
                Throw New ArgumentNullException(NameOf(pixels))
            End If
            If width <= 0 OrElse height <= 0 Then
                Throw New ArgumentOutOfRangeException($"{width}x{height} is not a valid image size")
            End If
            If channels < 1 OrElse channels > 4 Then
                Throw New NotSupportedException($"unsupported png channel number {channels}!")
            End If
            If CLng(width) * height * channels > pixels.Length Then
                Throw New ArgumentException($"the pixel buffer is too small: {pixels.Length} bytes for a {width}x{height}x{channels} image")
            End If

            Dim stride As Integer = width * channels
            Dim size As Long = CLng(stride + 1) * height

            If size > Integer.MaxValue Then
                Throw New NotSupportedException($"the image {width}x{height}x{channels} is too large to be encoded at once")
            End If

            Dim raw As Byte() = New Byte(CInt(size) - 1) {}
            Dim scratch As Byte() = If(rowFilter = PngFilterType.Adaptive, New Byte(stride - 1) {}, Nothing)

            For y As Integer = 0 To height - 1
                Dim source As Integer = y * stride
                Dim target As Integer = y * (stride + 1)

                raw(target) = FilterRow(pixels, source, (y - 1) * stride, y > 0,
                                        scratch, raw, target + 1, stride, channels, rowFilter)
            Next

            Call s.Write(PngCodec.Signature, 0, 8)
            Call PngCodec.WriteChunk(s, "IHDR", Ihdr(width, height, ColorTypeOf(channels)))
            Call PngCodec.WriteChunk(s, "IDAT", PngCodec.ZlibDeflate(raw, level))
            Call PngCodec.WriteChunk(s, "IEND", Nothing)
        End Sub

        ''' <summary>
        ''' filter a scan line into the output buffer, the filter type that is 
        ''' used is returned as the filter byte of the scan line.
        ''' </summary>
        Private Function FilterRow(pixels As Byte(), source As Integer, prev As Integer, hasPrev As Boolean,
                                   scratch As Byte(), raw As Byte(), target As Integer,
                                   stride As Integer, bpp As Integer, mode As PngFilterType) As Byte
            If mode <> PngFilterType.Adaptive Then
                Call EncodeRow(mode, pixels, source, prev, hasPrev, raw, target, stride, bpp)

                Return CByte(mode)
            End If

            Dim best As PngFilterType = PngFilterType.None
            Dim bestSum As Long = Long.MaxValue

            For Each candidate As PngFilterType In AdaptiveFilters
                Dim sum As Long = EncodeRow(candidate, pixels, source, prev, hasPrev, scratch, 0, stride, bpp)

                If sum < bestSum Then
                    bestSum = sum
                    best = candidate
                End If
            Next

            Call EncodeRow(best, pixels, source, prev, hasPrev, raw, target, stride, bpp)

            Return CByte(best)
        End Function

        ''' <summary>
        ''' encode a scan line with the given filter, the sum of the absolute 
        ''' values of the encoded bytes is returned as the cost of the filter.
        ''' </summary>
        Private Function EncodeRow(mode As PngFilterType, pixels As Byte(), source As Integer,
                                   prev As Integer, hasPrev As Boolean, output As Byte(),
                                   target As Integer, stride As Integer, bpp As Integer) As Long
            Dim sum As Long = 0

            Select Case mode
                Case PngFilterType.None
                    For i As Integer = 0 To stride - 1
                        Dim v As Integer = CInt(pixels(source + i))

                        output(target + i) = CByte(v)
                        sum += If(v < 128, v, 256 - v)
                    Next
                Case PngFilterType.[Sub]
                    For i As Integer = 0 To stride - 1
                        Dim left As Integer = If(i >= bpp, CInt(pixels(source + i - bpp)), 0)
                        Dim v As Integer = (CInt(pixels(source + i)) - left) And &HFF

                        output(target + i) = CByte(v)
                        sum += If(v < 128, v, 256 - v)
                    Next
                Case PngFilterType.Up
                    For i As Integer = 0 To stride - 1
                        Dim up As Integer = If(hasPrev, CInt(pixels(prev + i)), 0)
                        Dim v As Integer = (CInt(pixels(source + i)) - up) And &HFF

                        output(target + i) = CByte(v)
                        sum += If(v < 128, v, 256 - v)
                    Next
                Case PngFilterType.Average
                    For i As Integer = 0 To stride - 1
                        Dim left As Integer = If(i >= bpp, CInt(pixels(source + i - bpp)), 0)
                        Dim up As Integer = If(hasPrev, CInt(pixels(prev + i)), 0)
                        Dim v As Integer = (CInt(pixels(source + i)) - ((left + up) >> 1)) And &HFF

                        output(target + i) = CByte(v)
                        sum += If(v < 128, v, 256 - v)
                    Next
                Case PngFilterType.Paeth
                    For i As Integer = 0 To stride - 1
                        Dim a As Integer = If(i >= bpp, CInt(pixels(source + i - bpp)), 0)
                        Dim b As Integer = If(hasPrev, CInt(pixels(prev + i)), 0)
                        Dim c As Integer = If(hasPrev AndAlso i >= bpp, CInt(pixels(prev + i - bpp)), 0)
                        Dim v As Integer = (CInt(pixels(source + i)) - PngCodec.Paeth(a, b, c)) And &HFF

                        output(target + i) = CByte(v)
                        sum += If(v < 128, v, 256 - v)
                    Next
            End Select

            Return sum
        End Function

        ''' <summary>
        ''' the png color type of the given number of the color channels
        ''' </summary>
        Private Function ColorTypeOf(channels As Integer) As Byte
            Select Case channels
                Case 1 : Return 0
                Case 2 : Return 4
                Case 3 : Return 2
                Case 4 : Return 6
                Case Else
                    Throw New NotSupportedException($"unsupported png channel number {channels}!")
            End Select
        End Function

        Private Function Ihdr(width As Integer, height As Integer, colorType As Byte) As Byte()
            Dim data As Byte() = New Byte(12) {}

            Call PngCodec.WriteInt32BE(data, 0, width)
            Call PngCodec.WriteInt32BE(data, 4, height)

            data(8) = 8               ' the bit depth
            data(9) = colorType       ' the color type
            data(10) = 0              ' the compression method
            data(11) = 0              ' the filter method
            data(12) = 0              ' the interlace method

            Return data
        End Function

        ''' <summary>
        ''' clamp the normalized pixel value into the 8 bits gray scale buffer
        ''' </summary>
        Private Function ToGrayBuffer(pixels As Double(), width As Integer, height As Integer, scale255 As Double) As Byte()
            If pixels Is Nothing Then
                Throw New ArgumentNullException(NameOf(pixels))
            End If
            If width <= 0 OrElse height <= 0 Then
                Throw New ArgumentOutOfRangeException($"{width}x{height} is not a valid image size")
            End If
            If CLng(width) * height > pixels.Length Then
                Throw New ArgumentException($"the pixel buffer is too small: {pixels.Length} values for a {width}x{height} image")
            End If

            Dim buffer As Byte() = New Byte(width * height - 1) {}

            For i As Integer = 0 To buffer.Length - 1
                buffer(i) = ToGrayByte(pixels(i) * scale255)
            Next

            Return buffer
        End Function

        Private Function ToGrayByte(v As Double) As Byte
            If Double.IsNaN(v) Then
                Return 0
            End If
            If v < 0 Then
                v = 0
            End If
            If v > 255 Then
                v = 255
            End If

            Return CByte(CInt(v))
        End Function

    End Module

End Namespace
