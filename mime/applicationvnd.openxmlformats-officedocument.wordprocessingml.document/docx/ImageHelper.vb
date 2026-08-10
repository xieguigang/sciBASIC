#Region "Microsoft.VisualBasic::1b2fc888d609061add0a3fe7829d61dc, mime\applicationvnd.openxmlformats-officedocument.wordprocessingml.document\docx\ImageHelper.vb"

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


    ' Code Statistics:

    '   Total Lines: 325
    '    Code Lines: 210 (64.62%)
    ' Comment Lines: 69 (21.23%)
    '    - Xml Docs: 50.72%
    ' 
    '   Blank Lines: 46 (14.15%)
    '     File Size: 12.23 KB


    ' Module ImageHelper
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Adler32, BE16, BE32, Crc32, IsStartOfFrame
    '               ReadImageDimensions, ReadJpegDimensions, ReadPngDimensions, ZlibCompress
    ' 
    '     Sub: CreateTestPng, WriteChunk
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' ImageHelper.vb - 图像工具
'
' 提供 PNG/JPEG 尺寸读取和测试用 PNG 生成功能。
' 仅依赖 BCL，不使用 System.Drawing.Common。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports ImageDimensions = System.Drawing.Size

''' <summary>
''' 图像工具：读取图像尺寸、生成测试 PNG。
''' </summary>
Public Module ImageHelper

    ''' <summary>
    ''' 从 PNG 文件读取图像尺寸。
    ''' </summary>
    ''' <returns>解析成功返回真实像素尺寸；无法识别时返回 <see cref="ImageDimensions.Empty"/> 表示尺寸未知。</returns>
    Public Function ReadPngDimensions(data As Byte()) As ImageDimensions
        ' PNG 签名: 89 50 4E 47 0D 0A 1A 0A (8 bytes)
        ' IHDR: width at offset 16-19, height at offset 20-23 (big-endian)
        If data Is Nothing OrElse data.Length < 24 Then
            Return ImageDimensions.Empty
        End If

        ' 验证 PNG 签名
        If data(0) <> &H89 OrElse data(1) <> &H50 OrElse data(2) <> &H4E OrElse data(3) <> &H47 Then
            Return ImageDimensions.Empty
        End If

        Dim w As Integer = BE32(data, 16)
        Dim h As Integer = BE32(data, 20)

        ' 畸形 IHDR：尺寸为 0 或负数（最高位被置位）一律视为未知
        If w <= 0 OrElse h <= 0 Then
            Return ImageDimensions.Empty
        End If

        Return New ImageDimensions With {.Width = w, .Height = h}
    End Function

    ''' <summary>
    ''' 从 JPEG 文件读取图像尺寸。
    ''' 通过扫描 SOFn 标记获取尺寸（涵盖基线、扩展顺序、渐进式及算术编码等变体）。
    ''' </summary>
    ''' <returns>解析成功返回真实像素尺寸；无法识别时返回 <see cref="ImageDimensions.Empty"/> 表示尺寸未知。</returns>
    Public Function ReadJpegDimensions(data As Byte()) As ImageDimensions
        If data Is Nothing OrElse data.Length < 4 Then
            Return ImageDimensions.Empty
        End If

        ' 验证 JPEG 签名: FF D8
        If data(0) <> &HFF OrElse data(1) <> &HD8 Then
            Return ImageDimensions.Empty
        End If

        Dim pos As Integer = 2
        Do While pos < data.Length - 1
            ' 查找标记: FF xx
            If data(pos) <> &HFF Then
                pos += 1
                Continue Do
            End If

            Dim marker As Integer = data(pos + 1)

            ' SOI / EOI
            If marker = &HD8 OrElse marker = &HD9 Then
                Exit Do
            End If

            ' 跳过填充字节
            If marker = &HFF Then
                pos += 1
                Continue Do
            End If

            ' SOS(DA) 之后是熵编码数据，不再有可解析的段头
            If marker = &HDA Then
                Exit Do
            End If

            ' 读取段长度 (big-endian)
            If pos + 3 >= data.Length Then Exit Do
            Dim length As Integer = BE16(data, pos + 2)

            ' 段长度自身占 2 字节，小于 2 说明文件损坏；
            ' 若不拦截会导致 pos 不前进甚至回退，形成死循环
            If length < 2 Then Exit Do

            ' SOFn: 包含图像尺寸。
            ' C0-C3、C5-C7、C9-CB、CD-CF 均为 SOF 变体；
            ' 需排除 C4(DHT)、C8(JPG 保留)、CC(DAC)，它们不携带尺寸。
            If IsStartOfFrame(marker) Then
                If pos + 8 < data.Length Then
                    Dim h As Integer = BE16(data, pos + 5)
                    Dim w As Integer = BE16(data, pos + 7)

                    If w > 0 AndAlso h > 0 Then
                        Return New ImageDimensions With {.Width = w, .Height = h}
                    End If
                End If
                Exit Do
            End If

            pos += 2 + length
        Loop

        Return ImageDimensions.Empty
    End Function

    ''' <summary>
    ''' 读取大端序 32 位无符号整数（以 Integer 承载）。
    ''' </summary>
    ''' <remarks>
    ''' 必须先 <c>CInt</c> 提升再移位：VB.NET 中 <c>Byte</c> 参与 <c>&lt;&lt;</c> 时结果仍为 <c>Byte</c>，
    ''' 且移位数会被掩码为 <c>count And 7</c>，直接对 Byte 左移 8/16/24 位会把高位字节全部丢弃，
    ''' 导致解析出的尺寸严重偏小（例如 2400x1800 会被读成 105x15）。
    ''' </remarks>
    Private Function BE32(data As Byte(), offset As Integer) As Integer
        Return (CInt(data(offset)) << 24) Or
               (CInt(data(offset + 1)) << 16) Or
               (CInt(data(offset + 2)) << 8) Or
               CInt(data(offset + 3))
    End Function

    ''' <summary>
    ''' 读取大端序 16 位无符号整数。
    ''' </summary>
    ''' <remarks>同 <see cref="BE32"/>，必须先 <c>CInt</c> 提升再移位。</remarks>
    Private Function BE16(data As Byte(), offset As Integer) As Integer
        Return (CInt(data(offset)) << 8) Or CInt(data(offset + 1))
    End Function

    ''' <summary>
    ''' 判断 JPEG 标记是否为携带尺寸信息的 SOFn 段。
    ''' </summary>
    Private Function IsStartOfFrame(marker As Integer) As Boolean
        Select Case marker
            Case &HC4, &HC8, &HCC
                ' DHT / JPG 保留 / DAC：不携带尺寸
                Return False
            Case &HC0 To &HCF
                Return True
            Case Else
                Return False
        End Select
    End Function

    ''' <summary>
    ''' 从文件读取图像尺寸。
    ''' </summary>
    ''' <returns>解析成功返回真实像素尺寸；无法识别时返回 <see cref="ImageDimensions.Empty"/> 表示尺寸未知。</returns>
    Public Function ReadImageDimensions(filePath As String) As ImageDimensions
        Try
            Dim data As Byte() = File.ReadAllBytes(filePath)
            Dim ext As String = Path.GetExtension(filePath).TrimStart("."c).ToLower()
            Dim dims As ImageDimensions

            Select Case ext
                Case "png"
                    dims = ReadPngDimensions(data)
                Case "jpg", "jpeg"
                    dims = ReadJpegDimensions(data)
                Case Else
                    Console.Error.WriteLine($"[警告] 不支持读取该格式的图像尺寸，将按默认比例呈现: {filePath}")
                    Return ImageDimensions.Empty
            End Select

            If dims.IsEmpty Then
                Console.Error.WriteLine($"[警告] 无法解析图像尺寸（文件可能已损坏或格式不符），将按默认比例呈现: {filePath}")
            End If

            Return dims
        Catch ex As Exception
            Console.Error.WriteLine($"[警告] 读取图像尺寸失败，将按默认比例呈现: {filePath} - {ex.Message}")
            Return ImageDimensions.Empty
        End Try
    End Function

    ''' <summary>
    ''' 创建测试用 PNG 图片（纯色矩形）。
    ''' 用于在没有图像资源时演示 Image 功能。
    ''' </summary>
    Public Sub CreateTestPng(filePath As String, width As Integer, height As Integer,
                              r As Byte, g As Byte, b As Byte)
        Using fs As New FileStream(filePath, FileMode.Create)
            ' PNG 签名
            Dim sig As Byte() = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}
            fs.Write(sig, 0, 8)

            ' IHDR 数据 (13 bytes)
            Dim ihdr As Byte() = New Byte(12) {}
            ihdr(0) = CByte((width >> 24) And &HFF)
            ihdr(1) = CByte((width >> 16) And &HFF)
            ihdr(2) = CByte((width >> 8) And &HFF)
            ihdr(3) = CByte(width And &HFF)
            ihdr(4) = CByte((height >> 24) And &HFF)
            ihdr(5) = CByte((height >> 16) And &HFF)
            ihdr(6) = CByte((height >> 8) And &HFF)
            ihdr(7) = CByte(height And &HFF)
            ihdr(8) = 8     ' bit depth
            ihdr(9) = 2     ' color type: RGB
            ihdr(10) = 0    ' compression
            ihdr(11) = 0    ' filter
            ihdr(12) = 0    ' interlace
            WriteChunk(fs, "IHDR", ihdr)

            ' IDAT: zlib 压缩的扫描线数据
            Dim rawSize As Integer = height * (1 + width * 3)
            Dim rawData As Byte() = New Byte(rawSize - 1) {}
            Dim off As Integer = 0
            For y As Integer = 0 To height - 1
                rawData(off) = 0  ' filter: none
                off += 1
                For x As Integer = 0 To width - 1
                    rawData(off) = r
                    rawData(off + 1) = g
                    rawData(off + 2) = b
                    off += 3
                Next
            Next

            Dim compressed As Byte() = ZlibCompress(rawData)
            WriteChunk(fs, "IDAT", compressed)

            ' IEND
            WriteChunk(fs, "IEND", Nothing)
        End Using
    End Sub

    Private Sub WriteChunk(fs As FileStream, type As String, data As Byte())
        Dim dataLen As Integer = If(data IsNot Nothing, data.Length, 0)

        ' 长度 (big-endian)
        fs.WriteByte(CByte((dataLen >> 24) And &HFF))
        fs.WriteByte(CByte((dataLen >> 16) And &HFF))
        fs.WriteByte(CByte((dataLen >> 8) And &HFF))
        fs.WriteByte(CByte(dataLen And &HFF))

        ' 类型
        Dim typeBytes As Byte() = System.Text.Encoding.ASCII.GetBytes(type)
        fs.Write(typeBytes, 0, 4)

        ' 数据
        If dataLen > 0 Then
            fs.Write(data, 0, dataLen)
        End If

        ' CRC (over type + data)
        Dim crcData As Byte()
        If dataLen > 0 Then
            crcData = New Byte(4 + dataLen - 1) {}
            Array.Copy(typeBytes, crcData, 4)
            Array.Copy(data, 0, crcData, 4, dataLen)
        Else
            crcData = typeBytes
        End If
        Dim crc As UInteger = Crc32(crcData)
        fs.WriteByte(CByte((crc >> 24) And &HFF))
        fs.WriteByte(CByte((crc >> 16) And &HFF))
        fs.WriteByte(CByte((crc >> 8) And &HFF))
        fs.WriteByte(CByte(crc And &HFF))
    End Sub

    Private Function ZlibCompress(data As Byte()) As Byte()
        Using ms As New MemoryStream()
            ' zlib 头: 0x78 0x01 (deflate, level 1)
            ms.WriteByte(&H78)
            ms.WriteByte(&H1)

            ' deflate 压缩
            Using ds As New DeflateStream(ms, CompressionMode.Compress, leaveOpen:=True)
                ds.Write(data, 0, data.Length)
            End Using

            ' Adler32 校验和
            Dim adler As UInteger = Adler32(data)
            ms.WriteByte(CByte((adler >> 24) And &HFF))
            ms.WriteByte(CByte((adler >> 16) And &HFF))
            ms.WriteByte(CByte((adler >> 8) And &HFF))
            ms.WriteByte(CByte(adler And &HFF))

            Return ms.ToArray()
        End Using
    End Function

    Private Function Adler32(data As Byte()) As UInteger
        Dim a As UInteger = 1
        Dim b As UInteger = 0
        For Each c As Byte In data
            a = (a + c) Mod 65521
            b = (b + a) Mod 65521
        Next
        Return (b << 16) Or a
    End Function

    Private crcTable(255) As UInteger

    Sub New()
        ' 初始化 CRC32 表
        For i As UInteger = 0 To 255
            Dim c As UInteger = i
            For j As Integer = 0 To 7
                If (c And 1) = 1 Then
                    c = &HEDB88320UI Xor (c >> 1)
                Else
                    c = c >> 1
                End If
            Next
            crcTable(i) = c
        Next
    End Sub

    Private Function Crc32(data As Byte()) As UInteger
        Dim crc As UInteger = &HFFFFFFFFUI
        For Each b As Byte In data
            crc = crcTable((crc Xor b) And &HFF) Xor (crc >> 8)
        Next
        Return crc Xor &HFFFFFFFFUI
    End Function

End Module
