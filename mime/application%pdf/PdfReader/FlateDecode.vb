' ============================================================================
'  FlateDecode.vb  -  FlateDecode (zlib/deflate) 解压缩 + PNG 预测器
'  ----------------------------------------------------------------------------
'  PDF 的 FlateDecode 滤镜使用 zlib 压缩（RFC 1950）。
'  .NET 标准库 System.IO.Compression.DeflateStream 处理 raw deflate，
'  因此本模块跳过 zlib 头部（2 字节 CMF/FLG）后调用 DeflateStream。
'  注意：本模块不依赖任何第三方 PDF 库，仅使用 .NET BCL 的解压能力。
'
'  另外实现 PNG 预测器（Predictor 10~15），用于处理带 /DecodeParms 的流。
' ============================================================================

Imports System.IO
Imports System.IO.Compression
Imports std = System.Math

Public Class FlateDecode

    ''' <summary>解压 FlateDecode 数据（zlib 包装的 deflate）。</summary>
    Public Shared Function Decode(data As Byte()) As Byte()
        If data Is Nothing OrElse data.Length = 0 Then Return New Byte(-1) {}

        ' zlib 格式：2 字节头 (CMF, FLG) + raw deflate + 4 字节 adler32
        ' .NET DeflateStream 期望 raw deflate（无 zlib 头）
        Dim offset = 0
        If data.Length >= 2 AndAlso (data(0) And &HF) = &H8 Then
            ' 看起来是 zlib 头
            offset = 2
        End If

        Try
            Using ms As New MemoryStream(data, offset, data.Length - offset)
                Using ds As New DeflateStream(ms, CompressionMode.Decompress)
                    Using output As New MemoryStream()
                        Dim buffer(4095) As Byte
                        Dim read
                        Do
                            read = ds.Read(buffer, 0, buffer.Length)
                            If read > 0 Then output.Write(buffer, 0, read)
                        Loop While read > 0
                        Return output.ToArray()
                    End Using
                End Using
            End Using
        Catch
            ' 若带 zlib 头的尝试失败，尝试 raw deflate
            If offset = 2 Then
                Return DecodeRawDeflate(data)
            End If
            Throw
        End Try
    End Function

    Private Shared Function DecodeRawDeflate(data As Byte()) As Byte()
        Using ms As New MemoryStream(data)
            Using ds As New DeflateStream(ms, CompressionMode.Decompress)
                Using output As New MemoryStream()
                    Dim buffer(4095) As Byte
                    Dim read
                    Do
                        read = ds.Read(buffer, 0, buffer.Length)
                        If read > 0 Then output.Write(buffer, 0, read)
                    Loop While read > 0
                    Return output.ToArray()
                End Using
            End Using
        End Using
    End Function

    ''' <summary>
    ''' 应用 PNG 预测器解码（Predictor 10~15）。
    ''' 数据按行存储，每行首字节为预测器类型，其后为预测后的像素数据。
    ''' </summary>
    Public Shared Function ApplyPredictor(data As Byte(), columns As Integer,
                                          bitsPerComponent As Integer, colors As Integer) As Byte()
        If columns <= 0 Then Return data
        Dim bytesPerPixel = CInt(std.Ceiling(bitsPerComponent * colors / 8.0))
        If bytesPerPixel = 0 Then bytesPerPixel = 1
        Dim stride = bytesPerPixel * columns
        If stride = 0 Then Return data

        Dim rows = data.Length \ (stride + 1)
        Dim result As New List(Of Byte)()
        Dim prevRow(stride - 1) As Byte

        Dim pos = 0
        For r = 0 To rows - 1
            If pos >= data.Length Then Exit For
            Dim predictor = data(pos)
            pos += 1
            Dim currentRow(stride - 1) As Byte
            For c = 0 To stride - 1
                If pos >= data.Length Then Exit For
                Dim raw = data(pos)
                pos += 1
                Dim up = prevRow(c)
                Dim left = If(c >= bytesPerPixel, currentRow(c - bytesPerPixel), CByte(0))
                Dim upLeft = If(c >= bytesPerPixel, prevRow(c - bytesPerPixel), CByte(0))
                Dim val As Integer
                Select Case predictor
                    Case 0 ' None
                        val = raw
                    Case 1 ' Sub
                        val = (raw + left) And &HFF
                    Case 2 ' Up
                        val = (raw + up) And &HFF
                    Case 3 ' Average
                        val = (raw + (CInt(left) + CInt(up)) \ 2) And &HFF
                    Case 4 ' Paeth
                        val = (raw + PaethPredictor(left, up, upLeft)) And &HFF
                    Case Else
                        val = raw
                End Select
                currentRow(c) = CByte(val)
            Next
            result.AddRange(currentRow)
            prevRow = currentRow
        Next
        Return result.ToArray()
    End Function

    Private Shared Function PaethPredictor(a As Byte, b As Byte, c As Byte) As Integer
        Dim ai = CInt(a), bi = CInt(b), ci = CInt(c)
        Dim p = ai + bi - ci
        Dim pa = std.Abs(p - ai)
        Dim pb = std.Abs(p - bi)
        Dim pc = std.Abs(p - ci)
        If pa <= pb AndAlso pa <= pc Then Return ai
        If pb <= pc Then Return bi
        Return ci
    End Function

    ''' <summary>解码 ASCII85 (Adobe 变体) 数据。</summary>
    Public Shared Function DecodeAscii85(data As Byte()) As Byte()
        If data Is Nothing OrElse data.Length = 0 Then Return New Byte(-1) {}
        Dim result As New List(Of Byte)()
        Dim i = 0
        Dim n = data.Length
        ' 跳过前导 <~ (Adobe 变体的可选标记)
        If n >= 2 AndAlso data(0) = 60 AndAlso data(1) = 126 Then i = 2
        ' 跳过空白
        While i < n AndAlso IsAscii85Whitespace(data(i))
            i += 1
        End While

        Dim group As New List(Of Integer)()
        Do While i < n
            Dim b = data(i)
            If b = 126 Then ' ~
                ' 可能是结束标记 ~>
                If i + 1 < n AndAlso data(i + 1) = 62 Then Exit Do
                i += 1
                Continue Do
            End If
            If IsAscii85Whitespace(b) Then
                i += 1
                Continue Do
            End If
            If b = 122 Then ' 'z' = 4 个零字节
                If group.Count > 0 Then
                    ' z 不能出现在组中间，忽略
                Else
                    result.Add(0) : result.Add(0) : result.Add(0) : result.Add(0)
                End If
                i += 1
                Continue Do
            End If
            If b < 33 OrElse b > 117 Then
                ' 非法字符，跳过
                i += 1
                Continue Do
            End If
            group.Add(b - 33)
            i += 1
            If group.Count = 5 Then
                ' 完整组：5 个 base-85 数字 -> 4 字节
                Dim val As Long = 0
                For Each g In group
                    val = val * 85 + g
                Next
                result.Add(CByte((val >> 24) And &HFF))
                result.Add(CByte((val >> 16) And &HFF))
                result.Add(CByte((val >> 8) And &HFF))
                result.Add(CByte(val And &HFF))
                group.Clear()
            End If
        Loop

        ' 处理末尾不完整组（2~4 个字符）
        If group.Count > 0 Then
            Dim padCount = 5 - group.Count
            For j = 0 To padCount - 1
                group.Add(84) ' 'u' - 33 = 84
            Next
            Dim val As Long = 0
            For Each g In group
                val = val * 85 + g
            Next
            Dim outBytes(3) As Byte
            outBytes(0) = CByte((val >> 24) And &HFF)
            outBytes(1) = CByte((val >> 16) And &HFF)
            outBytes(2) = CByte((val >> 8) And &HFF)
            outBytes(3) = CByte(val And &HFF)
            ' 只输出原始字符数对应的字节数
            For k = 0 To (4 - padCount) - 1
                result.Add(outBytes(k))
            Next
        End If

        Return result.ToArray()
    End Function

    Private Shared Function IsAscii85Whitespace(b As Byte) As Boolean
        Return b = 32 OrElse b = 9 OrElse b = 10 OrElse b = 13 OrElse b = 12 OrElse b = 0
    End Function

End Class


