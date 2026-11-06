Option Strict On
Option Explicit On
Option Infer On

Imports System.IO

Namespace Encodings

    ''' <summary>
    ''' RLE / bit-packed hybrid 编码（<see cref="Format.Encoding.RLE"/>），
    ''' 以及已废弃的旧式 <see cref="Format.Encoding.BIT_PACKED"/> 解码。
    ''' <para>
    ''' 位序为 LSB first（bit-packed run）；旧式 BIT_PACKED 级别编码为 MSB first。
    ''' </para>
    ''' </summary>
    Public Module BitPacking

        ''' <summary>
        ''' 计算表示 <paramref name="maxValue"/> 所需要的位数。
        ''' </summary>
        Public Function BitWidth(maxValue As Integer) As Integer
            If maxValue <= 0 Then
                Return 0
            End If

            Dim width As Integer = 0
            Dim value As Integer = maxValue

            While value > 0
                width += 1
                value >>= 1
            End While

            Return width
        End Function

        ''' <summary>
        ''' 以 RLE / bit-packed hybrid 格式写入整数序列（不带长度前缀）。
        ''' </summary>
        Public Sub WriteRleBitPackedHybrid(stream As Stream, values As Integer(), bitWidth As Integer)
            Dim n As Integer = values.Length

            If n = 0 Then
                Return
            End If

            If bitWidth <= 0 Then
                ' 所有数值均为 0：一个 RLE run 即可
                Call ByteWriter.WriteVarint(stream, CULng(CLng(n) << 1))
                Return
            End If

            Dim i As Integer = 0

            While i < n
                Dim runLength As Integer = CountRun(values, i)

                If runLength >= 8 Then
                    Call ByteWriter.WriteVarint(stream, CULng(CLng(runLength) << 1))
                    Call WriteRunValue(stream, values(i), bitWidth)
                    i += runLength
                Else
                    Dim start As Integer = i

                    While i < n
                        Dim run As Integer = CountRun(values, i)

                        If run >= 8 Then
                            Exit While
                        End If

                        i += run
                    End While

                    Dim length As Integer = i - start
                    Dim fullGroups As Integer = (length \ 8) * 8

                    If i >= n Then
                        ' 数据流末尾的 bit-packed run 允许使用 0 填充到 8 的整数倍
                        Call WriteBitPackedRun(stream, values, start, length, bitWidth)
                    Else
                        If fullGroups > 0 Then
                            Call WriteBitPackedRun(stream, values, start, fullGroups, bitWidth)
                        End If

                        ' 中间位置的 bit-packed run 必须是 8 的整数倍，剩余尾部数值使用单值 RLE run 编码
                        For k As Integer = fullGroups To length - 1
                            Call ByteWriter.WriteVarint(stream, CULng(1L << 1))
                            Call WriteRunValue(stream, values(start + k), bitWidth)
                        Next
                    End If
                End If
            End While
        End Sub

        ''' <summary>
        ''' 读取 RLE / bit-packed hybrid 格式的整数序列。
        ''' </summary>
        ''' <param name="reader">字节游标</param>
        ''' <param name="count">需要读取的数值个数</param>
        ''' <param name="bitWidth">每个数值的位宽（0 ~ 32）</param>
        Public Function ReadRleBitPackedHybrid(reader As ByteReader, count As Integer, bitWidth As Integer) As Integer()
            If count <= 0 Then
                Return New Integer() {}
            End If

            Dim result As Integer() = New Integer(count - 1) {}

            If bitWidth <= 0 Then
                ' bit width 为 0 时全部数值均为 0，数据流中通常只有一个不带数值的 RLE run
                If Not reader.EndOfData Then
                    Call reader.ReadVarint()
                End If

                Return result
            End If

            Dim produced As Integer = 0

            While produced < count
                Dim header As Long = CLng(reader.ReadVarint())
                Dim isBitPacked As Boolean = (header And 1L) <> 0L
                Dim runLength As Integer = CInt(header >> 1)

                If isBitPacked Then
                    Dim groups As Integer = runLength

                    If groups <= 0 Then
                        Throw New ParquetException("invalid bit-packed run in the rle encoded stream!")
                    End If

                    Dim buffer As Byte() = reader.ReadBytes(groups * bitWidth)
                    Dim total As Integer = groups * 8
                    Dim limit As Integer = System.Math.Min(total, count - produced)

                    For k As Integer = 0 To limit - 1
                        result(produced) = UnpackValue(buffer, k * bitWidth, bitWidth)
                        produced += 1
                    Next
                Else
                    If runLength <= 0 Then
                        Throw New ParquetException("invalid rle run in the rle encoded stream!")
                    End If

                    Dim value As Integer = ReadRunValue(reader, bitWidth)
                    Dim limit As Integer = System.Math.Min(runLength, count - produced)

                    For k As Integer = 0 To limit - 1
                        result(produced) = value
                        produced += 1
                    Next
                End If
            End While

            Return result
        End Function

        ''' <summary>
        ''' 读取旧式（已废弃）BIT_PACKED 编码的级别数据，MSB first，按照 8 个数值一组打包。
        ''' </summary>
        Public Function ReadBitPackedLegacy(reader As ByteReader, count As Integer, bitWidth As Integer) As Integer()
            If count <= 0 Then
                Return New Integer() {}
            End If

            Dim result As Integer() = New Integer(count - 1) {}

            If bitWidth <= 0 Then
                Return result
            End If

            Dim totalBits As Integer = count * bitWidth
            Dim totalBytes As Integer = (totalBits + 7) \ 8
            Dim buffer As Byte() = reader.ReadBytes(totalBytes)
            Dim bitPosition As Integer = 0

            For k As Integer = 0 To count - 1
                Dim value As Integer = 0

                For b As Integer = 0 To bitWidth - 1
                    Dim byteIndex As Integer = (bitPosition + b) \ 8
                    Dim bitIndex As Integer = 7 - ((bitPosition + b) Mod 8)
                    Dim bit As Integer = (buffer(byteIndex) >> bitIndex) And 1

                    value = (value << 1) Or bit
                Next

                result(k) = value
                bitPosition += bitWidth
            Next

            Return result
        End Function

        Private Function CountRun(values As Integer(), start As Integer) As Integer
            Dim count As Integer = 1
            Dim value As Integer = values(start)

            While start + count < values.Length AndAlso values(start + count) = value
                count += 1
            End While

            Return count
        End Function

        Private Sub WriteRunValue(stream As Stream, value As Integer, bitWidth As Integer)
            Dim bytes As Integer = (bitWidth + 7) \ 8
            Dim buffer As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(buffer)
            End If

            Call stream.Write(buffer, 0, bytes)
        End Sub

        Private Function ReadRunValue(reader As ByteReader, bitWidth As Integer) As Integer
            Dim bytes As Integer = (bitWidth + 7) \ 8
            Dim buffer As Byte() = reader.ReadBytes(bytes)
            Dim full As Byte() = New Byte(3) {}

            System.Buffer.BlockCopy(buffer, 0, full, 0, bytes)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(full)
            End If

            Return BitConverter.ToInt32(full, 0)
        End Function

        Private Sub WriteBitPackedRun(stream As Stream, values As Integer(), start As Integer, count As Integer, bitWidth As Integer)
            Dim groups As Integer = (count + 7) \ 8
            Dim totalBits As Integer = groups * 8 * bitWidth
            Dim buffer As Byte() = New Byte((totalBits + 7) \ 8 - 1) {}

            For k As Integer = 0 To count - 1
                Dim value As Integer = values(start + k)
                Dim bitOffset As Integer = k * bitWidth

                For b As Integer = 0 To bitWidth - 1
                    If ((value >> b) And 1) <> 0 Then
                        Dim position As Integer = bitOffset + b
                        Dim index As Integer = position >> 3
                        buffer(index) = CByte(buffer(index) Or CByte(1 << (position And 7)))
                    End If
                Next
            Next

            Call ByteWriter.WriteVarint(stream, CULng((CLng(groups) << 1) Or 1L))
            Call stream.Write(buffer, 0, buffer.Length)
        End Sub

        ''' <summary>
        ''' 从 bit-packed 缓冲区中解出指定偏移处的数值（LSB first）。
        ''' </summary>
        Public Function UnpackValue(buffer As Byte(), bitOffset As Integer, bitWidth As Integer) As Integer
            Dim value As Integer = 0

            For b As Integer = 0 To bitWidth - 1
                Dim position As Integer = bitOffset + b
                Dim index As Integer = position >> 3

                If index >= buffer.Length Then
                    Exit For
                End If

                If ((buffer(index) >> (position And 7)) And 1) <> 0 Then
                    value = value Or (1 << b)
                End If
            Next

            Return value
        End Function

    End Module

End Namespace
