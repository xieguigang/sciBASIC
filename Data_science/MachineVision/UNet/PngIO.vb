' ---------------------------------------------------------------------------
' 自包含的 PNG 读写
'
' 为什么自己写：本工程（以及它的依赖链）没有引用任何图形库，
' 而框架里的 Microsoft.VisualBasic.Imaging 是一个面向 net10.0-windows 的
' 大型图形引擎，为了读一批 256×256 的灰度图把它引进来并不划算，
' 也不该为一个 demo 引入 NuGet 包。DSB2018 数据集的 PNG 都是
' 非隔行、位深 8（少量 16）、颜色类型 0/2/3/6 的常见形态，
' 用 .NET 自带的 DeflateStream 就足以完整解码。
'
' 解码流程：签名校验 → 遍历 chunk 取 IHDR/PLTE/IDAT → 拼接 IDAT 并 inflate
'          → 逐扫描线反过滤(None/Sub/Up/Average/Paeth) → 按位深展开为每通道样本
'          → 转灰度（或保留 RGB）并归一化到 [0,1]。
'
' 编码流程：只输出 8 位灰度 PNG，IDAT 用 stored（不压缩）deflate 块，
'          省去 Huffman 编码，换来的是实现简单且解码端零风险。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.IO.Compression
Imports std = System.Math

Namespace UNet

    ''' <summary>
    ''' 解码出来的一张位图。
    ''' </summary>
    Public Class RasterImage

        ''' <summary>宽度。</summary>
        Public Property Width As Integer
        ''' <summary>高度。</summary>
        Public Property Height As Integer
        ''' <summary>通道数（1 = 灰度，3 = RGB）。</summary>
        Public Property Channels As Integer
        ''' <summary>归一化到 [0,1] 的像素，按 <c>(y*Width + x)*Channels + c</c> 排列。</summary>
        Public Property Pixels As Double()

        ''' <summary>像素值是否全为 0（解码失败的典型症状，用于"防假通过"校验）。</summary>
        Public ReadOnly Property IsBlank As Boolean
            Get
                If Pixels Is Nothing Then Return True

                For Each v In Pixels
                    If v > 0 Then Return False
                Next

                Return True
            End Get
        End Property

        ''' <summary>单行描述。</summary>
        Public Overrides Function ToString() As String
            Return $"{Width}x{Height}x{Channels}"
        End Function

    End Class

    ''' <summary>
    ''' PNG 位图的读与写（无外部依赖）。
    ''' </summary>
    Public Module PngIO

        Private ReadOnly Signature As Byte() = New Byte() {137, 80, 78, 71, 13, 10, 26, 10}

        ''' <summary>
        ''' 解码一个 PNG 文件。
        ''' </summary>
        ''' <param name="filePath">文件路径。</param>
        ''' <param name="toGray">True 时转成单通道灰度；False 时保留 3 通道 RGB。</param>
        ''' <returns>解码出的位图。</returns>
        Public Function Decode(filePath As String, Optional toGray As Boolean = True) As RasterImage
            Dim raw = System.IO.File.ReadAllBytes(filePath)

            If raw.Length < 8 Then
                Throw New InvalidDataException($"文件过短, 不是合法的 PNG: {filePath}")
            End If
            For i As Integer = 0 To 7
                If raw(i) <> Signature(i) Then
                    Throw New InvalidDataException($"PNG 签名不匹配: {filePath}")
                End If
            Next

            Dim width As Integer = 0, height As Integer = 0
            Dim bitDepth As Integer = 0, colorType As Integer = 0, interlace As Integer = 0
            Dim idat As New List(Of Byte())
            Dim palette As Byte() = Nothing
            Dim pos As Integer = 8

            While pos + 8 <= raw.Length
                Dim length = ReadBE(raw, pos)
                Dim type = System.Text.Encoding.ASCII.GetString(raw, pos + 4, 4)
                Dim dataStart = pos + 8

                If dataStart + length > raw.Length Then
                    Throw New InvalidDataException($"PNG chunk 长度越界: {type}")
                End If

                Select Case type
                    Case "IHDR"
                        width = ReadBE(raw, dataStart)
                        height = ReadBE(raw, dataStart + 4)
                        bitDepth = raw(dataStart + 8)
                        colorType = raw(dataStart + 9)
                        interlace = raw(dataStart + 12)
                    Case "PLTE"
                        palette = New Byte(length - 1) {}
                        Array.Copy(raw, dataStart, palette, 0, length)
                    Case "IDAT"
                        Dim buf(length - 1) As Byte
                        Array.Copy(raw, dataStart, buf, 0, length)
                        idat.Add(buf)
                    Case "IEND"
                        Exit While
                End Select

                pos = dataStart + length + 4 ' 跳过 CRC
            End While

            If interlace <> 0 Then
                Throw New NotSupportedException($"暂不支持隔行(Adam7)编码的 PNG: {filePath}")
            End If
            If width <= 0 OrElse height <= 0 Then
                Throw New InvalidDataException($"PNG 尺寸非法 {width}x{height}: {filePath}")
            End If

            Dim channels = ChannelsOf(colorType)
            Dim samples = UnpackSamples(idat, width, height, bitDepth, channels)
            Dim pixels = ToPixels(samples, width, height, colorType, palette, toGray)
            Dim outChannels = If(toGray, 1, 3)

            Return New RasterImage With {
                .Width = width,
                .Height = height,
                .Channels = outChannels,
                .Pixels = pixels
            }
        End Function

        ''' <summary>颜色类型对应的通道数。</summary>
        Private Function ChannelsOf(colorType As Integer) As Integer
            Select Case colorType
                Case 0 : Return 1 ' gray
                Case 2 : Return 3 ' rgb
                Case 3 : Return 1 ' palette index
                Case 4 : Return 2 ' gray + alpha
                Case 6 : Return 4 ' rgba
                Case Else
                    Throw New NotSupportedException($"不支持的 PNG 颜色类型: {colorType}")
            End Select
        End Function

        ''' <summary>拼接 IDAT → inflate → 反过滤 → 展开为每通道 8 位样本。</summary>
        Private Function UnpackSamples(idat As List(Of Byte()), width As Integer, height As Integer,
                                       bitDepth As Integer, channels As Integer) As Byte()
            Dim total As Integer = 0

            For Each buf In idat
                total += buf.Length
            Next

            Dim compressed(total - 1) As Byte
            Dim offset As Integer = 0

            For Each buf In idat
                Array.Copy(buf, 0, compressed, offset, buf.Length)
                offset += buf.Length
            Next

            ' zlib 流 = 2 字节头 + 原始 deflate + 4 字节 Adler32
            If compressed.Length < 6 Then
                Throw New InvalidDataException("IDAT 数据过短")
            End If

            Dim raw As Byte()

            Using ms As New MemoryStream(compressed, 2, compressed.Length - 2)
                Using ds As New DeflateStream(ms, CompressionMode.Decompress)
                    Using out As New MemoryStream()
                        Call ds.CopyTo(out)
                        raw = out.ToArray()
                    End Using
                End Using
            End Using

            ' 位深小于 8 时一行不是整字节对齐的，因此按 bit 计算步长
            Dim bitsPerPixel = bitDepth * channels
            Dim stride = (width * bitsPerPixel + 7) \ 8
            Dim expected = (stride + 1) * height

            If raw.Length < expected Then
                Throw New InvalidDataException($"解压后的数据长度不足: {raw.Length} < {expected}")
            End If

            ' 反过滤（就地）
            Dim bpp = std.Max(1, bitsPerPixel \ 8)

            For y As Integer = 0 To height - 1
                Dim rowStart = y * (stride + 1)
                Dim filterType = raw(rowStart)
                Dim cur = rowStart + 1
                Dim prev = cur - stride - 1

                For i As Integer = 0 To stride - 1
                    Dim a As Integer = If(i >= bpp, CInt(raw(cur + i - bpp)), 0)
                    Dim b As Integer = If(y > 0, CInt(raw(prev + i)), 0)
                    Dim c As Integer = If(y > 0 AndAlso i >= bpp, CInt(raw(prev + i - bpp)), 0)
                    Dim x = raw(cur + i)

                    ' 注意: VB 里 Byte + Byte 的结果**仍然是 Byte**并且受溢出检查保护,
                    ' 所以这里必须先显式拓宽到 Integer 再做加法, 否则 255+1 直接抛 OverflowException
                    Select Case filterType
                        Case 0 ' None
                        Case 1 ' Sub
                            x = CByte((CInt(x) + CInt(a)) And &HFF)
                        Case 2 ' Up
                            x = CByte((CInt(x) + CInt(b)) And &HFF)
                        Case 3 ' Average
                            x = CByte((CInt(x) + ((CInt(a) + CInt(b)) \ 2)) And &HFF)
                        Case 4 ' Paeth
                            x = CByte((CInt(x) + Paeth(a, b, c)) And &HFF)
                        Case Else
                            Throw New InvalidDataException($"未知的 PNG 行过滤器: {filterType}")
                    End Select

                    raw(cur + i) = x
                Next
            Next

            ' 展开为每通道一个字节（位深 16 取高字节）
            Dim samples(width * height * channels - 1) As Byte

            If bitDepth = 8 Then
                For y As Integer = 0 To height - 1
                    Array.Copy(raw, y * (stride + 1) + 1, samples, y * width * channels, width * channels)
                Next
            ElseIf bitDepth = 16 Then
                For y As Integer = 0 To height - 1
                    Dim src = y * (stride + 1) + 1

                    For i As Integer = 0 To width * channels - 1
                        samples(y * width * channels + i) = raw(src + i * 2)
                    Next
                Next
            ElseIf bitDepth = 1 OrElse bitDepth = 2 OrElse bitDepth = 4 Then
                Dim mask = (1 << bitDepth) - 1
                Dim perByte = 8 \ bitDepth
                Dim scale = 255 \ mask

                For y As Integer = 0 To height - 1
                    Dim src = y * (stride + 1) + 1

                    For i As Integer = 0 To width * channels - 1
                        Dim b As Integer = CInt(raw(src + i \ perByte))
                        Dim shift = 8 - bitDepth * ((i Mod perByte) + 1)
                        Dim value = (b >> shift) And mask

                        samples(y * width * channels + i) = CByte(value * scale)
                    Next
                Next
            Else
                Throw New NotSupportedException($"不支持的 PNG 位深: {bitDepth}")
            End If

            Return samples
        End Function

        ''' <summary>PNG 规范里的 Paeth 预测器。</summary>
        Private Function Paeth(a As Integer, b As Integer, c As Integer) As Integer
            Dim p = a + b - c
            Dim pa = std.Abs(p - a)
            Dim pb = std.Abs(p - b)
            Dim pc = std.Abs(p - c)

            If pa <= pb AndAlso pa <= pc Then Return a
            If pb <= pc Then Return b

            Return c
        End Function

        ''' <summary>把样本转成归一化像素（灰度或 RGB）。</summary>
        Private Function ToPixels(samples As Byte(), width As Integer, height As Integer,
                                  colorType As Integer, palette As Byte(), toGray As Boolean) As Double()
            Dim n = width * height
            Dim channels = ChannelsOf(colorType)
            Dim outChannels = If(toGray, 1, 3)
            Dim pixels(n * outChannels - 1) As Double

            For p As Integer = 0 To n - 1
                Dim baseIndex = p * channels
                Dim r As Double, g As Double, b As Double

                Select Case colorType
                    Case 0, 4 ' 灰度（带 alpha 时忽略 alpha）
                        r = samples(baseIndex) / 255.0
                        g = r
                        b = r
                    Case 2, 6 ' RGB（带 alpha 时忽略 alpha）
                        r = samples(baseIndex) / 255.0
                        g = samples(baseIndex + 1) / 255.0
                        b = samples(baseIndex + 2) / 255.0
                    Case 3 ' 调色板
                        Dim idx = samples(baseIndex) * 3

                        If palette IsNot Nothing AndAlso idx + 2 < palette.Length Then
                            r = palette(idx) / 255.0
                            g = palette(idx + 1) / 255.0
                            b = palette(idx + 2) / 255.0
                        Else
                            r = 0.0 : g = 0.0 : b = 0.0
                        End If
                    Case Else
                        Throw New NotSupportedException($"不支持的 PNG 颜色类型: {colorType}")
                End Select

                If toGray Then
                    pixels(p) = 0.299 * r + 0.587 * g + 0.114 * b
                Else
                    pixels(p * 3) = r
                    pixels(p * 3 + 1) = g
                    pixels(p * 3 + 2) = b
                End If
            Next

            Return pixels
        End Function

        ''' <summary>大端读取一个 32 位整数。</summary>
        Private Function ReadBE(data As Byte(), offset As Integer) As Integer
            Return (CInt(data(offset)) << 24) Or (CInt(data(offset + 1)) << 16) Or
                   (CInt(data(offset + 2)) << 8) Or CInt(data(offset + 3))
        End Function

        ' ---------------- 写出 ----------------

        ''' <summary>
        ''' 写出一张 8 位灰度 PNG。
        ''' </summary>
        ''' <param name="filePath">目标路径。</param>
        ''' <param name="width">宽度。</param>
        ''' <param name="height">高度。</param>
        ''' <param name="pixels">像素值，取值 [0,1] 或 0/1 掩膜，按 <c>y*width + x</c> 排列。</param>
        ''' <param name="scale255">
        ''' 掩膜模式：非 0 像素按 <paramref name="scale255"/> 缩放（默认 255）。
        ''' </param>
        Public Sub WriteGray(filePath As String, width As Integer, height As Integer,
                             pixels As Double(), Optional scale255 As Double = 255.0)
            Dim raw((strideOf(width) + 1) * height - 1) As Byte
            Dim stride = width

            For y As Integer = 0 To height - 1
                Dim rowStart = y * (stride + 1)

                raw(rowStart) = 0 ' 过滤器：None

                For x As Integer = 0 To width - 1
                    Dim v = pixels(y * width + x) * scale255

                    If v < 0 Then v = 0
                    If v > 255 Then v = 255

                    raw(rowStart + 1 + x) = CByte(CInt(v))
                Next
            Next

            Dim idat = ZlibStore(raw)

            Using fs As New FileStream(filePath, FileMode.Create, FileAccess.Write)
                Call fs.Write(Signature, 0, Signature.Length)
                Call WriteChunk(fs, "IHDR", Ihdr(width, height))
                Call WriteChunk(fs, "IDAT", idat)
                Call WriteChunk(fs, "IEND", New Byte() {})
            End Using
        End Sub

        ''' <summary>
        ''' 写出 PGM(P5) 灰度图；比 PNG 更简单，作为可视化兜底。
        ''' </summary>
        Public Sub WritePgm(filePath As String, width As Integer, height As Integer,
                            pixels As Double(), Optional scale255 As Double = 255.0)
            Using fs As New FileStream(filePath, FileMode.Create, FileAccess.Write)
                Using sw As New StreamWriter(fs)
                    Call sw.Write($"P5{Chr(10)}{width} {height}{Chr(10)}255{Chr(10)}")
                    Call sw.Flush()

                    For i As Integer = 0 To width * height - 1
                        Dim v = pixels(i) * scale255

                        If v < 0 Then v = 0
                        If v > 255 Then v = 255

                        fs.WriteByte(CByte(CInt(v)))
                    Next
                End Using
            End Using
        End Sub

        Private Function strideOf(width As Integer) As Integer
            Return width
        End Function

        Private Function Ihdr(width As Integer, height As Integer) As Byte()
            Dim data(12) As Byte

            Call WriteBE(data, 0, width)
            Call WriteBE(data, 4, height)

            data(8) = 8  ' bit depth
            data(9) = 0  ' color type: gray
            data(10) = 0 ' compression
            data(11) = 0 ' filter
            data(12) = 0 ' interlace

            Return data
        End Function

        Private Sub WriteBE(data As Byte(), offset As Integer, value As Integer)
            data(offset) = CByte((value >> 24) And &HFF)
            data(offset + 1) = CByte((value >> 16) And &HFF)
            data(offset + 2) = CByte((value >> 8) And &HFF)
            data(offset + 3) = CByte(value And &HFF)
        End Sub

        ''' <summary>把原始数据打包成 zlib 流（stored deflate 块 + Adler32）。</summary>
        Private Function ZlibStore(raw As Byte()) As Byte()
            Dim chunks As New List(Of Byte())

            Dim pos As Integer = 0

            Do
                Dim blockSize = std.Min(65535, raw.Length - pos)
                Dim finalBlock = (pos + blockSize >= raw.Length)
                Dim buf(blockSize + 4) As Byte

                buf(0) = If(finalBlock, CByte(1), CByte(0)) ' BFINAL=1 表示最后一块, BTYPE=00 表示 stored
                buf(1) = CByte(blockSize And &HFF)
                buf(2) = CByte((blockSize >> 8) And &HFF)
                buf(3) = CByte((Not blockSize) And &HFF)
                buf(4) = CByte(((Not blockSize) >> 8) And &HFF)

                Array.Copy(raw, pos, buf, 5, blockSize)
                chunks.Add(buf)

                pos += blockSize
            Loop While pos < raw.Length

            Dim total As Integer = 2 + 4

            For Each c In chunks
                total += c.Length
            Next

            Dim out(total - 1) As Byte

            out(0) = &H78
            out(1) = &H1

            Dim p As Integer = 2

            For Each c In chunks
                Array.Copy(c, 0, out, p, c.Length)
                p += c.Length
            Next

            Dim adler = Adler32(raw)

            out(p) = CByte((adler >> 24) And &HFF)
            out(p + 1) = CByte((adler >> 16) And &HFF)
            out(p + 2) = CByte((adler >> 8) And &HFF)
            out(p + 3) = CByte(adler And &HFF)

            Return out
        End Function

        Private Function Adler32(data As Byte()) As UInteger
            Dim a As UInteger = 1
            Dim b As UInteger = 0

            For Each value In data
                a = (a + value) Mod 65521UI
                b = (b + a) Mod 65521UI
            Next

            Return (b << 16) Or a
        End Function

        Private Sub WriteChunk(fs As Stream, type As String, data As Byte())
            Dim name = System.Text.Encoding.ASCII.GetBytes(type)
            ' 4 字节长度 + 4 字节类型名 + 数据
            Dim buffer(4 + name.Length + data.Length - 1) As Byte

            Call WriteBE(buffer, 0, data.Length)
            Array.Copy(name, 0, buffer, 4, name.Length)
            Array.Copy(data, 0, buffer, 8, data.Length)

            Call fs.Write(buffer, 0, buffer.Length)

            Dim crc = Crc32(name, data)
            Dim crcBytes(3) As Byte

            crcBytes(0) = CByte((crc >> 24) And &HFF)
            crcBytes(1) = CByte((crc >> 16) And &HFF)
            crcBytes(2) = CByte((crc >> 8) And &HFF)
            crcBytes(3) = CByte(crc And &HFF)

            Call fs.Write(crcBytes, 0, 4)
        End Sub

        Private Function Crc32(name As Byte(), data As Byte()) As UInteger
            Dim crc As UInteger = &HFFFFFFFFUI

            For Each b In name
                crc = CrcStep(crc, b)
            Next
            For Each b In data
                crc = CrcStep(crc, b)
            Next

            Return crc Xor &HFFFFFFFFUI
        End Function

        Private Function CrcStep(crc As UInteger, b As Byte) As UInteger
            crc = crc Xor b

            For k As Integer = 0 To 7
                If (crc And 1UI) <> 0 Then
                    crc = (crc >> 1) Xor &HEDB88320UI
                Else
                    crc >>= 1
                End If
            Next

            Return crc
        End Function

    End Module

End Namespace
