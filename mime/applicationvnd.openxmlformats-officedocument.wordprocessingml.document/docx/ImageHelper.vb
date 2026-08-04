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
    Public Function ReadPngDimensions(data As Byte()) As ImageDimensions
        ' PNG 签名: 89 50 4E 47 0D 0A 1A 0A (8 bytes)
        ' IHDR: width at offset 16-19, height at offset 20-23 (big-endian)
        If data Is Nothing OrElse data.Length < 24 Then
            Return New ImageDimensions With {.Width = 600, .Height = 400}
        End If

        ' 验证 PNG 签名
        If data(0) <> &H89 OrElse data(1) <> &H50 OrElse data(2) <> &H4E OrElse data(3) <> &H47 Then
            Return New ImageDimensions With {.Width = 600, .Height = 400}
        End If

        Dim w As Integer = (data(16) << 24) Or (data(17) << 16) Or (data(18) << 8) Or data(19)
        Dim h As Integer = (data(20) << 24) Or (data(21) << 16) Or (data(22) << 8) Or data(23)

        Return New ImageDimensions With {.Width = w, .Height = h}
    End Function

    ''' <summary>
    ''' 从 JPEG 文件读取图像尺寸。
    ''' 通过扫描 SOF0/SOF2 标记获取尺寸。
    ''' </summary>
    Public Function ReadJpegDimensions(data As Byte()) As ImageDimensions
        If data Is Nothing OrElse data.Length < 4 Then
            Return New ImageDimensions With {.Width = 600, .Height = 400}
        End If

        ' 验证 JPEG 签名: FF D8
        If data(0) <> &HFF OrElse data(1) <> &HD8 Then
            Return New ImageDimensions With {.Width = 600, .Height = 400}
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

            ' 读取段长度 (big-endian)
            If pos + 3 >= data.Length Then Exit Do
            Dim length As Integer = (data(pos + 2) << 8) Or data(pos + 3)

            ' SOF0 (FFC0) 或 SOF2 (FFC2): 包含图像尺寸
            If marker = &HC0 OrElse marker = &HC2 Then
                If pos + 8 < data.Length Then
                    Dim h As Integer = (data(pos + 5) << 8) Or data(pos + 6)
                    Dim w As Integer = (data(pos + 7) << 8) Or data(pos + 8)
                    Return New ImageDimensions With {.Width = w, .Height = h}
                End If
                Exit Do
            End If

            pos += 2 + length
        Loop

        Return New ImageDimensions With {.Width = 600, .Height = 400}
    End Function

    ''' <summary>
    ''' 从文件读取图像尺寸。
    ''' </summary>
    Public Function ReadImageDimensions(filePath As String) As ImageDimensions
        Try
            Dim data As Byte() = File.ReadAllBytes(filePath)
            Dim ext As String = Path.GetExtension(filePath).TrimStart("."c).ToLower()

            Select Case ext
                Case "png"
                    Return ReadPngDimensions(data)
                Case "jpg", "jpeg"
                    Return ReadJpegDimensions(data)
                Case Else
                    Return New ImageDimensions With {.Width = 600, .Height = 400}
            End Select
        Catch
            Return New ImageDimensions With {.Width = 600, .Height = 400}
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
            crcData = New Byte(3 + dataLen - 1) {}
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
