' ---------------------------------------------------------------------------
' 极简 PNG 编解码器（8 位、非隔行）
'
' 为什么不用 GDI+ / Microsoft.VisualBasic.Imaging：
'   * Core 的 BitmapReader 只认 BMP 魔数（"BM"），对 PNG 直接抛
'     InvalidDataException("invalid magic number!")；
'   * 走 GDI+ 需要 System.Drawing.Common，而本库的目标框架是非 Windows 专用的
'     net10.0，加上会引入平台绑定。
'
' 这里用 .NET 内置的 System.IO.Compression.ZLibStream 自行实现：
'   解码：IHDR + PLTE + IDAT → 反滤波（None/Sub/Up/Average/Paeth）→ RGB
'   编码：RGB → 滤波 0 → zlib → IHDR/IDAT/IEND + CRC32
' 支持 colorType 0/2/3/4/6（灰/彩色/调色板/带 alpha），位深 8。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports std = System.Math

''' <summary>解码结果（始终是 8 位 RGB）。</summary>
Public Class PngImage

        Public Property Width As Integer
        Public Property Height As Integer

        ''' <summary>行优先 RGB，长度 = Width·Height·3</summary>
        Public Property Rgb As Byte()

    End Class

    ''' <summary>PNG 读写。</summary>
    Public Module PngCodec

        Private ReadOnly Signature As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}

#Region "解码"

        Public Function DecodeFile(path As String) As PngImage
            Return Decode(File.ReadAllBytes(path))
        End Function

        Public Function Decode(data As Byte()) As PngImage
            If data Is Nothing OrElse data.Length < 8 Then Throw New InvalidDataException("PNG 数据过短")

            For i = 0 To 7
                If data(i) <> Signature(i) Then Throw New InvalidDataException("不是 PNG 文件（签名不匹配）")
            Next

            Dim width = 0, height = 0, bitDepth = 0, colorType = -1, interlace = 0
            Dim palette As Byte() = Nothing
            Dim idat As New MemoryStream()

            Dim pos = 8

            While pos + 8 <= data.Length
                Dim length = ReadInt32BE(data, pos)
                Dim type = Encoding.ASCII.GetString(data, pos + 4, 4)
                Dim payload = pos + 8

                Select Case type
                    Case "IHDR"
                        width = ReadInt32BE(data, payload)
                        height = ReadInt32BE(data, payload + 4)
                        bitDepth = data(payload + 8)
                        colorType = data(payload + 9)
                        interlace = data(payload + 12)
                    Case "PLTE"
                        palette = New Byte(length - 1) {}
                        Call Array.Copy(data, payload, palette, 0, length)
                    Case "IDAT"
                        idat.Write(data, payload, length)
                    Case "IEND"
                        pos = data.Length
                        Exit While
                End Select

                pos = payload + length + 4 ' 跳过 CRC
            End While

            If width <= 0 OrElse height <= 0 Then Throw New InvalidDataException("PNG 缺少有效 IHDR")
            If bitDepth <> 8 Then Throw New NotSupportedException($"仅支持 8 位/通道的 PNG（当前 {bitDepth} 位）")
            If interlace <> 0 Then Throw New NotSupportedException("不支持隔行扫描 PNG")

            Dim channels As Integer

            Select Case colorType
                Case 0 : channels = 1
                Case 2 : channels = 3
                Case 3 : channels = 1
                Case 4 : channels = 2
                Case 6 : channels = 4
                Case Else
                    Throw New NotSupportedException($"不支持的 PNG colorType={colorType}")
            End Select

            Dim stride = width * channels
            Dim raw = Inflate(idat.ToArray(), height * (stride + 1))

            Dim image As New PngImage With {.Width = width, .Height = height}
            Dim rgb(width * height * 3 - 1) As Byte
            Dim prev(stride - 1) As Byte
            Dim cur(stride - 1) As Byte

            Dim offset = 0
            Dim dst = 0

            For y = 0 To height - 1
                Dim filter = raw(offset)
                offset += 1

                Call Array.Copy(raw, offset, cur, 0, stride)
                offset += stride

                Call Unfilter(filter, cur, prev, channels)

                For x = 0 To width - 1
                    Dim src = x * channels

                    Select Case colorType
                        Case 0
                            Dim g = cur(src)
                            rgb(dst) = g : rgb(dst + 1) = g : rgb(dst + 2) = g
                        Case 2
                            rgb(dst) = cur(src) : rgb(dst + 1) = cur(src + 1) : rgb(dst + 2) = cur(src + 2)
                        Case 3
                            Dim p = CInt(cur(src)) * 3
                            rgb(dst) = palette(p) : rgb(dst + 1) = palette(p + 1) : rgb(dst + 2) = palette(p + 2)
                        Case 4
                            Dim g = cur(src)
                            rgb(dst) = g : rgb(dst + 1) = g : rgb(dst + 2) = g
                        Case 6
                            rgb(dst) = cur(src) : rgb(dst + 1) = cur(src + 1) : rgb(dst + 2) = cur(src + 2)
                    End Select

                    dst += 3
                Next

                Call Array.Copy(cur, prev, stride)
            Next

            image.Rgb = rgb

            Return image
        End Function

        Private Sub Unfilter(filter As Byte, cur As Byte(), prev As Byte(), bpp As Integer)
            Dim n = cur.Length

            Select Case filter
                Case 0
                    ' None
                Case 1
                    For i = bpp To n - 1
                        cur(i) = CByte((CInt(cur(i)) + CInt(cur(i - bpp))) And &HFF)
                    Next
                Case 2
                    For i = 0 To n - 1
                        cur(i) = CByte((CInt(cur(i)) + CInt(prev(i))) And &HFF)
                    Next
                Case 3
                    For i = 0 To n - 1
                        Dim left = If(i >= bpp, CInt(cur(i - bpp)), 0)
                        cur(i) = CByte((CInt(cur(i)) + ((left + CInt(prev(i))) >> 1)) And &HFF)
                    Next
                Case 4
                    For i = 0 To n - 1
                        Dim a = If(i >= bpp, CInt(cur(i - bpp)), 0)
                        Dim b = CInt(prev(i))
                        Dim c = If(i >= bpp, CInt(prev(i - bpp)), 0)
                        cur(i) = CByte((CInt(cur(i)) + Paeth(a, b, c)) And &HFF)
                    Next
                Case Else
                    Throw New NotSupportedException($"未知 PNG 滤波类型 {filter}")
            End Select
        End Sub

        Private Function Paeth(a As Integer, b As Integer, c As Integer) As Integer
            Dim p = a + b - c
            Dim pa = std.Abs(p - a)
            Dim pb = std.Abs(p - b)
            Dim pc = std.Abs(p - c)

            If pa <= pb AndAlso pa <= pc Then Return a
            If pb <= pc Then Return b

            Return c
        End Function

        Private Function Inflate(compressed As Byte(), expected As Integer) As Byte()
            Using input As New MemoryStream(compressed)
                Using z As New ZLibStream(input, CompressionMode.Decompress)
                    Dim buffer(expected - 1) As Byte
                    Dim read = 0

                    While read < expected
                        Dim n = z.Read(buffer, read, expected - read)

                        If n <= 0 Then Exit While

                        read += n
                    End While

                    If read <> expected Then
                        Throw New InvalidDataException($"PNG 数据长度不符: 期望 {expected}, 实际 {read}")
                    End If

                    Return buffer
                End Using
            End Using
        End Function

#End Region

#Region "编码"

        ''' <summary>把 RGB（行优先、8 位）编码为 PNG 文件。</summary>
        Public Sub EncodeFile(path As String, width As Integer, height As Integer, rgb As Byte())
            Dim dir = IO.Path.GetDirectoryName(path)

            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Call Directory.CreateDirectory(dir)
            End If

            Call File.WriteAllBytes(path, Encode(width, height, rgb))
        End Sub

        Public Function Encode(width As Integer, height As Integer, rgb As Byte()) As Byte()
            Dim stride = width * 3
            Dim raw(height * (stride + 1) - 1) As Byte

            Dim src = 0
            Dim dst = 0

            For y = 0 To height - 1
                raw(dst) = 0 ' 滤波 None
                dst += 1

                Call Array.Copy(rgb, src, raw, dst, stride)

                src += stride
                dst += stride
            Next

            Dim compressed As Byte()

            Using output As New MemoryStream()
                Using z As New ZLibStream(output, CompressionLevel.Optimal, leaveOpen:=True)
                    z.Write(raw, 0, raw.Length)
                End Using

                compressed = output.ToArray()
            End Using

            Using png As New MemoryStream()
                png.Write(Signature, 0, Signature.Length)

                Dim ihdr(12) As Byte
                WriteInt32BE(ihdr, 0, width)
                WriteInt32BE(ihdr, 4, height)
                ihdr(8) = 8    ' bit depth
                ihdr(9) = 2    ' color type = RGB
                ihdr(10) = 0   ' compression
                ihdr(11) = 0   ' filter
                ihdr(12) = 0   ' interlace

                Call WriteChunk(png, "IHDR", ihdr)
                Call WriteChunk(png, "IDAT", compressed)
                Call WriteChunk(png, "IEND", New Byte() {})

                Return png.ToArray()
            End Using
        End Function

        Private Sub WriteChunk(stream As Stream, type As String, payload As Byte())
            Dim typeBytes = Encoding.ASCII.GetBytes(type)
            Dim length(3) As Byte

            WriteInt32BE(length, 0, payload.Length)
            stream.Write(length, 0, 4)
            stream.Write(typeBytes, 0, 4)
            stream.Write(payload, 0, payload.Length)

            Dim crc As UInteger = &HFFFFFFFFUI

            For Each b In typeBytes
                crc = CrcTable(CInt((crc Xor b) And &HFFUI)) Xor (crc >> 8)
            Next

            For Each b In payload
                crc = CrcTable(CInt((crc Xor b) And &HFFUI)) Xor (crc >> 8)
            Next

            crc = crc Xor &HFFFFFFFFUI

            Dim crcBytes(3) As Byte
            crcBytes(0) = CByte((crc >> 24) And &HFFUI)
            crcBytes(1) = CByte((crc >> 16) And &HFFUI)
            crcBytes(2) = CByte((crc >> 8) And &HFFUI)
            crcBytes(3) = CByte(crc And &HFFUI)

            stream.Write(crcBytes, 0, 4)
        End Sub

        Private ReadOnly CrcTable As UInteger() = BuildCrcTable()

        Private Function BuildCrcTable() As UInteger()
            Dim table(255) As UInteger

            For n = 0 To 255
                Dim c As UInteger = CUInt(n)

                For k = 0 To 7
                    If (c And 1UI) <> 0UI Then
                        c = &HEDB88320UI Xor (c >> 1)
                    Else
                        c >>= 1
                    End If
                Next

                table(n) = c
            Next

            Return table
        End Function

#End Region

        Private Function ReadInt32BE(data As Byte(), offset As Integer) As Integer
            Return (CInt(data(offset)) << 24) Or (CInt(data(offset + 1)) << 16) Or
                   (CInt(data(offset + 2)) << 8) Or CInt(data(offset + 3))
        End Function

        Private Sub WriteInt32BE(buffer As Byte(), offset As Integer, value As Integer)
            buffer(offset) = CByte((value >> 24) And &HFF)
            buffer(offset + 1) = CByte((value >> 16) And &HFF)
            buffer(offset + 2) = CByte((value >> 8) And &HFF)
            buffer(offset + 3) = CByte(value And &HFF)
        End Sub

End Module
