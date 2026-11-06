Option Strict On
Option Explicit On
Option Infer On

Namespace Encodings

    ''' <summary>
    ''' DELTA_BINARY_PACKED / DELTA_LENGTH_BYTE_ARRAY / DELTA_BYTE_ARRAY 的解码实现。
    ''' <para>
    ''' 本库写入时不使用这些编码，仅用于读取外部实现（parquet-mr / pyarrow 等）生成的文件。
    ''' </para>
    ''' </summary>
    Public Module DeltaEncoding

        ''' <summary>
        ''' 解码 DELTA_BINARY_PACKED 序列。INT32 与 INT64 物理类型共用该解码器。
        ''' </summary>
        Public Function ReadDeltaBinaryPacked(reader As ByteReader) As Long()
            Dim blockSize As Integer = CInt(reader.ReadVarint())
            Dim miniBlocks As Integer = CInt(reader.ReadVarint())
            Dim total As Integer = CInt(reader.ReadVarint())

            If total = 0 Then
                Return New Long() {}
            End If
            If total < 0 Then
                Throw New ParquetException($"invalid delta binary packed value count: {total}")
            End If
            If blockSize <= 0 OrElse miniBlocks <= 0 Then
                Throw New ParquetException($"invalid delta binary packed block header: block_size={blockSize}, mini_blocks={miniBlocks}")
            End If

            Dim miniSize As Integer = blockSize \ miniBlocks

            If miniSize <= 0 Then
                Throw New ParquetException($"invalid delta binary packed mini block size: block_size={blockSize}, mini_blocks={miniBlocks}")
            End If

            Dim result As Long() = New Long(total - 1) {}
            Dim first As Long = reader.ReadZigZag()
            Dim index As Integer = 1
            Dim previous As Long = first
            Dim widths As Integer() = New Integer(miniBlocks - 1) {}

            result(0) = first

            While index < total
                Dim minDelta As Long = reader.ReadZigZag()
                Dim remaining As Integer = total - index
                Dim needed As Integer = System.Math.Min(miniBlocks, (remaining + miniSize - 1) \ miniSize)

                For m As Integer = 0 To needed - 1
                    widths(m) = CInt(reader.ReadByte())
                Next

                For m As Integer = 0 To needed - 1
                    Dim width As Integer = widths(m)
                    Dim countInMiniBlock As Integer = System.Math.Min(miniSize, total - index)

                    If width = 0 Then
                        For k As Integer = 0 To countInMiniBlock - 1
                            previous = previous + minDelta
                            result(index) = previous
                            index += 1
                        Next
                    Else
                        Dim bytes As Integer = (miniSize * width + 7) \ 8
                        Dim buffer As Byte() = reader.ReadBytes(bytes)

                        For k As Integer = 0 To countInMiniBlock - 1
                            Dim delta As Long = BitPacking.UnpackValue(buffer, k * width, width)

                            previous = previous + minDelta + delta
                            result(index) = previous
                            index += 1
                        Next
                    End If
                Next
            End While

            Return result
        End Function

        ''' <summary>
        ''' 解码 DELTA_LENGTH_BYTE_ARRAY 序列。
        ''' </summary>
        Public Function ReadDeltaLengthByteArray(reader As ByteReader) As Byte()()
            Dim lengths As Long() = ReadDeltaBinaryPacked(reader)
            Dim result As Byte()() = New Byte(lengths.Length - 1)() {}

            For i As Integer = 0 To lengths.Length - 1
                Dim size As Long = lengths(i)

                If size < 0 OrElse size > reader.Remaining Then
                    Throw New ParquetException($"invalid byte array length {size} in the DELTA_LENGTH_BYTE_ARRAY encoded page!")
                End If

                result(i) = reader.ReadBytes(CInt(size))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 解码 DELTA_BYTE_ARRAY 序列（前缀长度 + 后缀的 DELTA_LENGTH_BYTE_ARRAY）。
        ''' </summary>
        Public Function ReadDeltaByteArray(reader As ByteReader) As Byte()()
            Dim prefixes As Long() = ReadDeltaBinaryPacked(reader)
            Dim suffixes As Byte()() = ReadDeltaLengthByteArray(reader)

            If prefixes.Length <> suffixes.Length Then
                Throw New ParquetException($"the prefix length count ({prefixes.Length}) and the suffix count ({suffixes.Length}) are not equal in the DELTA_BYTE_ARRAY encoded page!")
            End If

            Dim result As Byte()() = New Byte(prefixes.Length - 1)() {}
            Dim previous As Byte() = New Byte() {}

            For i As Integer = 0 To prefixes.Length - 1
                Dim prefixLength As Integer = CInt(prefixes(i))
                Dim suffix As Byte() = suffixes(i)

                If prefixLength < 0 OrElse prefixLength > previous.Length Then
                    Throw New ParquetException($"invalid prefix length {prefixLength} in the DELTA_BYTE_ARRAY encoded page!")
                End If

                Dim value As Byte() = New Byte(prefixLength + suffix.Length - 1) {}

                If prefixLength > 0 Then
                    System.Buffer.BlockCopy(previous, 0, value, 0, prefixLength)
                End If
                If suffix.Length > 0 Then
                    System.Buffer.BlockCopy(suffix, 0, value, prefixLength, suffix.Length)
                End If

                result(i) = value
                previous = value
            Next

            Return result
        End Function

    End Module

End Namespace
