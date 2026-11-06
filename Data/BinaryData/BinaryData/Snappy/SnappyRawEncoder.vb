Option Strict On
Option Explicit On

Imports System.IO
Imports std = System.Math

Namespace Snappy

    ''' <summary>
    ''' Snappy 原始块格式的压缩器。输入被切成不超过 64 KiB 的分片，
    ''' 每个分片使用一张 4 字节哈希表查找匹配，并输出标准的字面量/复制指令流。
    ''' </summary>
    Friend Module SnappyRawEncoder

        ''' <summary>
        ''' 压缩指定范围的原始数据，返回「varint 未压缩长度 + 指令流」的完整原始块。
        ''' </summary>
        Friend Function Encode(input As Byte(), offset As Integer, count As Integer) As Byte()
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If offset < 0 OrElse count < 0 OrElse offset > input.Length OrElse count > input.Length - offset Then
                Throw New ArgumentOutOfRangeException("offset/count 超出了数据范围。")
            End If

            Dim output As Byte() = New Byte(Snappy.MaxCompressedLength(count) - 1) {}
            Dim op As Integer = 0
            Varint.Write(CUInt(count), output, op)

            Dim pos As Integer = offset
            Dim finish As Integer = offset + count

            While pos < finish
                Dim block As Integer = std.Min(SnappyConstants.BlockSize, finish - pos)
                EncodeFragment(input, pos, pos + block, output, op)
                pos += block
            End While

            If op = output.Length Then Return output

            Dim result(op - 1) As Byte
            Buffer.BlockCopy(output, 0, result, 0, op)
            Return result
        End Function

        Private Sub EncodeFragment(input As Byte(), start As Integer, finish As Integer, output As Byte(), ByRef op As Integer)
            Dim table(SnappyConstants.HashTableSize - 1) As Integer

            For i As Integer = 0 To table.Length - 1
                table(i) = -1
            Next

            Dim ip As Integer = start
            Dim nextEmit As Integer = start
            Dim ipLimit As Integer = finish - 4

            While ip <= ipLimit
                Dim h As Integer = Hash(Load32(input, ip))
                Dim candidate As Integer = table(h)
                table(h) = ip

                If candidate >= start Then
                    Dim distance As Integer = ip - candidate

                    If distance <= SnappyConstants.MaxTwoByteOffset AndAlso Load32(input, candidate) = Load32(input, ip) Then
                        Dim matched As Integer = 4
                        Dim maxMatch As Integer = finish - ip

                        While matched < maxMatch AndAlso input(candidate + matched) = input(ip + matched)
                            matched += 1
                        End While

                        EmitLiteral(input, nextEmit, ip - nextEmit, output, op)
                        EmitCopy(distance, matched, output, op)

                        ip += matched
                        nextEmit = ip
                        Continue While
                    End If
                End If

                ip += 1
            End While

            EmitLiteral(input, nextEmit, finish - nextEmit, output, op)
        End Sub

        Private Function Hash(value As UInteger) As Integer
            Dim product As ULong = (CULng(value) * &H1E35A7BDUL) And &HFFFFFFFFUL
            Return CInt((product >> (32 - SnappyConstants.HashTableBits)) And &HFFFFFFFFUL)
        End Function

        Private Function Load32(data As Byte(), index As Integer) As UInteger
            Return CUInt(data(index)) Or
                   (CUInt(data(index + 1)) << 8) Or
                   (CUInt(data(index + 2)) << 16) Or
                   (CUInt(data(index + 3)) << 24)
        End Function

        Private Sub EmitLiteral(input As Byte(), start As Integer, count As Integer, output As Byte(), ByRef op As Integer)
            If count <= 0 Then Return

            Dim stored As Integer = count - 1

            If stored < 60 Then
                output(op) = CByte(stored << 2)
                op += 1
            Else
                Dim bytes As Integer = 1

                If stored >= (1 << 8) Then bytes = 2
                If stored >= (1 << 16) Then bytes = 3
                If stored >= (1 << 24) Then bytes = 4

                output(op) = CByte((59 + bytes) << 2)
                op += 1

                Dim value As Integer = stored

                For i As Integer = 0 To bytes - 1
                    output(op) = CByte(value And &HFF)
                    op += 1
                    value >>= 8
                Next
            End If

            Buffer.BlockCopy(input, start, output, op, count)
            op += count
        End Sub

        Private Sub EmitCopy(distance As Integer, length As Integer, output As Byte(), ByRef op As Integer)
            While length >= 68
                EmitCopyLessThan64(distance, SnappyConstants.MaxCopyLength, output, op)
                length -= SnappyConstants.MaxCopyLength
            End While

            If length > SnappyConstants.MaxCopyLength Then
                EmitCopyLessThan64(distance, 60, output, op)
                length -= 60
            End If

            EmitCopyLessThan64(distance, length, output, op)
        End Sub

        Private Sub EmitCopyLessThan64(distance As Integer, length As Integer, output As Byte(), ByRef op As Integer)
            If length >= 4 AndAlso length < 12 AndAlso distance < 2048 Then
                ' copy with 1-byte offset: 低 3 位长度-4，高 3 位偏移高位
                output(op) = CByte(&H1 Or ((length - 4) << 2) Or ((distance >> 8) << 5))
                op += 1
                output(op) = CByte(distance And &HFF)
                op += 1
            ElseIf distance <= SnappyConstants.MaxTwoByteOffset Then
                ' copy with 2-byte offset
                output(op) = CByte(&H2 Or ((length - 1) << 2))
                op += 1
                output(op) = CByte(distance And &HFF)
                output(op + 1) = CByte((distance >> 8) And &HFF)
                op += 2
            Else
                ' copy with 4-byte offset
                output(op) = CByte(&H3 Or ((length - 1) << 2))
                op += 1
                output(op) = CByte(distance And &HFF)
                output(op + 1) = CByte((distance >> 8) And &HFF)
                output(op + 2) = CByte((distance >> 16) And &HFF)
                output(op + 3) = CByte((distance >> 24) And &HFF)
                op += 4
            End If
        End Sub

    End Module
End Namespace
