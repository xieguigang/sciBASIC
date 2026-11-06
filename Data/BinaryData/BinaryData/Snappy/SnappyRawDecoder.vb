Option Strict On
Option Explicit On

Imports System.IO

Namespace Snappy

    ''' <summary>
    ''' Snappy 原始块格式的解码器。解析 varint 长度头之后线性扫描
    ''' 字面量与三种复制指令，并对所有边界条件做严格校验。
    ''' </summary>
    Friend Module SnappyRawDecoder

        ''' <summary>
        ''' 从 <paramref name="src"/> 的 <paramref name="pos"/> 位置解码一个完整的原始块。
        ''' 方法返回后 <paramref name="pos"/> 指向该块之后的第一个字节。
        ''' </summary>
        Friend Function Decode(src As Byte(), ByRef pos As Integer, limit As Integer) As Byte()
            If src Is Nothing Then Throw New ArgumentNullException(NameOf(src))
            If pos < 0 OrElse limit < pos OrElse limit > src.Length Then
                Throw New ArgumentOutOfRangeException("pos/limit 超出了数据范围。")
            End If

            Dim length As UInteger = Varint.Read(src, pos, limit)

            If length > Integer.MaxValue Then
                Throw New InvalidDataException($"Snappy: 声明的未压缩长度 {length} 字节超出 .NET 数组上限。")
            End If

            ' 单条复制指令最多用 3 字节产生 64 字节输出，字面量至多 1:1，
            ' 因此任何合法块的长度都不可能超过剩余输入规模的 64/3 倍。
            ' 这里用于阻止恶意长度前缀触发巨量内存分配。
            Dim maxOutput As ULong = CULng(CLng(limit - pos) * 64L \ 3L)

            If CULng(length) > maxOutput Then
                Throw New InvalidDataException("Snappy: 声明的未压缩长度与压缩数据规模不匹配。")
            End If

            Dim dst As Byte()

            If length = 0UI Then
                dst = New Byte() {}
            Else
                dst = New Byte(CInt(length) - 1) {}
            End If

            DecodeBlock(src, pos, limit, dst)
            Return dst
        End Function

        ''' <summary>
        ''' 只读取原始块开头的 varint 未压缩长度，不执行解码。
        ''' </summary>
        Friend Function UncompressedLength(src As Byte(), ByRef pos As Integer, limit As Integer) As Integer
            Dim length As UInteger = Varint.Read(src, pos, limit)

            If length > Integer.MaxValue Then
                Throw New InvalidDataException($"Snappy: 声明的未压缩长度 {length} 字节超出 .NET 数组上限。")
            End If

            Return CInt(length)
        End Function

        Private Sub DecodeBlock(src As Byte(), ByRef ip As Integer, srcEnd As Integer, dst As Byte())
            Dim op As Integer = 0
            Dim dstEnd As Integer = dst.Length

            While op < dstEnd
                If ip >= srcEnd Then
                    Throw New InvalidDataException("Snappy: 压缩数据被截断。")
                End If

                Dim tag As Integer = CInt(src(ip))
                ip += 1

                Select Case tag And 3
                    Case 0
                        ' 字面量
                        Dim literalLength As Integer = tag >> 2

                        If literalLength < 60 Then
                            literalLength += 1
                        Else
                            Dim bytes As Integer = literalLength - 59

                            If ip > srcEnd - bytes Then
                                Throw New InvalidDataException("Snappy: 字面量长度字段被截断。")
                            End If

                            Dim value As ULong = 0UL

                            For i As Integer = 0 To bytes - 1
                                value = value Or (CULng(src(ip + i)) << (8 * i))
                            Next

                            ip += bytes

                            If value >= CUInt(Integer.MaxValue) Then
                                Throw New InvalidDataException($"Snappy: 字面量长度 {value} 超出支持范围。")
                            End If

                            literalLength = CInt(value) + 1
                        End If

                        If literalLength > dstEnd - op Then
                            Throw New InvalidDataException("Snappy: 字面量长度超出声明的未压缩长度。")
                        End If
                        If literalLength > srcEnd - ip Then
                            Throw New InvalidDataException("Snappy: 字面量数据被截断。")
                        End If

                        Buffer.BlockCopy(src, ip, dst, op, literalLength)
                        ip += literalLength
                        op += literalLength

                    Case 1
                        ' copy with 1-byte offset
                        If ip >= srcEnd Then
                            Throw New InvalidDataException("Snappy: 一字节偏移复制指令被截断。")
                        End If

                        Dim copyLength As Integer = ((tag >> 2) And 7) + 4
                        Dim distance As Integer = ((tag >> 5) << 8) Or CInt(src(ip))
                        ip += 1
                        op = Copy(dst, op, distance, copyLength)

                    Case 2
                        ' copy with 2-byte offset
                        If ip > srcEnd - 2 Then
                            Throw New InvalidDataException("Snappy: 两字节偏移复制指令被截断。")
                        End If

                        Dim copyLength As Integer = (tag >> 2) + 1
                        Dim distance As Integer = CInt(src(ip)) Or (CInt(src(ip + 1)) << 8)
                        ip += 2
                        op = Copy(dst, op, distance, copyLength)

                    Case Else
                        ' copy with 4-byte offset
                        If ip > srcEnd - 4 Then
                            Throw New InvalidDataException("Snappy: 四字节偏移复制指令被截断。")
                        End If

                        Dim copyLength As Integer = (tag >> 2) + 1
                        Dim rawDistance As ULong = CULng(src(ip)) Or
                                                   (CULng(src(ip + 1)) << 8) Or
                                                   (CULng(src(ip + 2)) << 16) Or
                                                   (CULng(src(ip + 3)) << 24)
                        ip += 4

                        If rawDistance > CUInt(Integer.MaxValue) Then
                            Throw New InvalidDataException($"Snappy: 复制距离 {rawDistance} 超出支持范围。")
                        End If

                        op = Copy(dst, op, CInt(rawDistance), copyLength)

                End Select
            End While
        End Sub

        Private Function Copy(dst As Byte(), op As Integer, distance As Integer, length As Integer) As Integer
            If length > dst.Length - op Then
                Throw New InvalidDataException("Snappy: 复制长度超出声明的未压缩长度。")
            End If
            If distance <= 0 OrElse distance > op Then
                Throw New InvalidDataException($"Snappy: 非法回退距离 {distance}（当前已输出 {op} 字节）。")
            End If

            Dim from As Integer = op - distance

            If distance >= length Then
                Buffer.BlockCopy(dst, from, dst, op, length)
            Else
                ' 重叠复制（例如 distance = 1 的行程编码）必须逐字节进行。
                For i As Integer = 0 To length - 1
                    dst(op + i) = dst(from + i)
                Next
            End If

            Return op + length
        End Function

    End Module
End Namespace
