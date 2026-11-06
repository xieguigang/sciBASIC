Option Strict On
Option Explicit On
Option Infer On

Imports System.IO

Namespace Encodings

    ''' <summary>
    ''' PLAIN 编码（<see cref="Format.Encoding.PLAIN"/>）的读写实现。
    ''' </summary>
    Public Module PlainEncoding

        ''' <summary>
        ''' 布尔值采用 bit-packed、LSB first 的方式编码。
        ''' </summary>
        Public Sub WriteBooleans(stream As Stream, values As Boolean())
            Dim bytes As Integer = (values.Length + 7) \ 8
            Dim buffer As Byte() = New Byte(bytes - 1) {}

            For i As Integer = 0 To values.Length - 1
                If values(i) Then
                    buffer(i >> 3) = CByte(buffer(i >> 3) Or CByte(1 << (i And 7)))
                End If
            Next

            If bytes > 0 Then
                Call stream.Write(buffer, 0, bytes)
            End If
        End Sub

        Public Sub WriteInt32(stream As Stream, values As Integer())
            Dim buffer As Byte() = New Byte(values.Length * 4 - 1) {}

            For i As Integer = 0 To values.Length - 1
                System.Buffer.BlockCopy(BitConverter.GetBytes(values(i)), 0, buffer, i * 4, 4)
            Next

            If Not BitConverter.IsLittleEndian Then
                For i As Integer = 0 To values.Length - 1
                    Array.Reverse(buffer, i * 4, 4)
                Next
            End If

            If buffer.Length > 0 Then
                Call stream.Write(buffer, 0, buffer.Length)
            End If
        End Sub

        Public Sub WriteInt64(stream As Stream, values As Long())
            Dim buffer As Byte() = New Byte(values.Length * 8 - 1) {}

            For i As Integer = 0 To values.Length - 1
                System.Buffer.BlockCopy(BitConverter.GetBytes(values(i)), 0, buffer, i * 8, 8)
            Next

            If Not BitConverter.IsLittleEndian Then
                For i As Integer = 0 To values.Length - 1
                    Array.Reverse(buffer, i * 8, 8)
                Next
            End If

            If buffer.Length > 0 Then
                Call stream.Write(buffer, 0, buffer.Length)
            End If
        End Sub

        Public Sub WriteSingle(stream As Stream, values As Single())
            Dim data As Byte() = New Byte(values.Length * 4 - 1) {}

            For i As Integer = 0 To values.Length - 1
                System.Buffer.BlockCopy(BitConverter.GetBytes(values(i)), 0, data, i * 4, 4)
            Next

            If Not BitConverter.IsLittleEndian Then
                For i As Integer = 0 To values.Length - 1
                    Array.Reverse(data, i * 4, 4)
                Next
            End If

            If data.Length > 0 Then
                Call stream.Write(data, 0, data.Length)
            End If
        End Sub

        Public Sub WriteDouble(stream As Stream, values As Double())
            Dim data As Byte() = New Byte(values.Length * 8 - 1) {}

            For i As Integer = 0 To values.Length - 1
                System.Buffer.BlockCopy(BitConverter.GetBytes(values(i)), 0, data, i * 8, 8)
            Next

            If Not BitConverter.IsLittleEndian Then
                For i As Integer = 0 To values.Length - 1
                    Array.Reverse(data, i * 8, 8)
                Next
            End If

            If data.Length > 0 Then
                Call stream.Write(data, 0, data.Length)
            End If
        End Sub

        ''' <summary>
        ''' BYTE_ARRAY：4 字节小端长度前缀 + 数据。
        ''' </summary>
        Public Sub WriteByteArrays(stream As Stream, values As Byte()())
            For Each value As Byte() In values
                Dim buffer As Byte() = If(value, New Byte() {})

                Call ByteWriter.WriteInt32LE(stream, buffer.Length)
                Call stream.Write(buffer, 0, buffer.Length)
            Next
        End Sub

        ''' <summary>
        ''' FIXED_LEN_BYTE_ARRAY：定长字节序列。
        ''' </summary>
        Public Sub WriteFixedLenByteArrays(stream As Stream, values As Byte()(), typeLength As Integer)
            For Each value As Byte() In values
                Dim buffer As Byte() = If(value, New Byte() {})

                If buffer.Length <> typeLength Then
                    Throw New ParquetException($"the fixed length byte array value should be {typeLength} bytes, but got {buffer.Length} bytes!")
                End If

                Call stream.Write(buffer, 0, buffer.Length)
            Next
        End Sub

        ''' <summary>
        ''' INT96：8 字节纳秒 + 4 字节儒略日。
        ''' </summary>
        Public Sub WriteInt96(stream As Stream, values As Byte()())
            For Each value As Byte() In values
                Call stream.Write(value, 0, 12)
            Next
        End Sub

        ''' <summary>
        ''' 按物理类型读取 <paramref name="count"/> 个 PLAIN 编码的数值，返回对应的强类型数组。
        ''' </summary>
        Public Function ReadValues(reader As ByteReader, count As Integer, type As Format.ParquetType, Optional typeLength As Integer = 0) As Array
            If count = 0 Then
                Select Case type
                    Case Format.ParquetType.[BOOLEAN] : Return New Boolean() {}
                    Case Format.ParquetType.INT32 : Return New Integer() {}
                    Case Format.ParquetType.INT64 : Return New Long() {}
                    Case Format.ParquetType.FLOAT : Return New Single() {}
                    Case Format.ParquetType.[DOUBLE] : Return New Double() {}
                    Case Else : Return New Byte()() {}
                End Select
            End If

            Select Case type
                Case Format.ParquetType.[BOOLEAN]
                    Return ReadBooleans(reader, count)
                Case Format.ParquetType.INT32
                    Return ReadInt32(reader, count)
                Case Format.ParquetType.INT64
                    Return ReadInt64(reader, count)
                Case Format.ParquetType.FLOAT
                    Return ReadSingle(reader, count)
                Case Format.ParquetType.[DOUBLE]
                    Return ReadDouble(reader, count)
                Case Format.ParquetType.BYTE_ARRAY
                    Return ReadByteArrays(reader, count)
                Case Format.ParquetType.FIXED_LEN_BYTE_ARRAY
                    Return ReadFixedLenByteArrays(reader, count, typeLength)
                Case Format.ParquetType.INT96
                    Return ReadInt96(reader, count)
                Case Else
                    Throw New ParquetException($"unsupported parquet physical type for PLAIN decoding: {type}")
            End Select
        End Function

        Public Function ReadBooleans(reader As ByteReader, count As Integer) As Boolean()
            Dim buffer As Byte() = reader.ReadBytes((count + 7) \ 8)
            Dim result As Boolean() = New Boolean(count - 1) {}

            For i As Integer = 0 To count - 1
                result(i) = ((buffer(i >> 3) >> (i And 7)) And 1) <> 0
            Next

            Return result
        End Function

        Public Function ReadInt32(reader As ByteReader, count As Integer) As Integer()
            Dim buffer As Byte() = reader.ReadBytes(count * 4)
            Dim result As Integer() = New Integer(count - 1) {}

            For i As Integer = 0 To count - 1
                result(i) = ReadInt32At(buffer, i * 4)
            Next

            Return result
        End Function

        Public Function ReadInt64(reader As ByteReader, count As Integer) As Long()
            Dim buffer As Byte() = reader.ReadBytes(count * 8)
            Dim result As Long() = New Long(count - 1) {}

            For i As Integer = 0 To count - 1
                result(i) = ReadInt64At(buffer, i * 8)
            Next

            Return result
        End Function

        Public Function ReadSingle(reader As ByteReader, count As Integer) As Single()
            Dim buffer As Byte() = reader.ReadBytes(count * 4)
            Dim result As Single() = New Single(count - 1) {}

            For i As Integer = 0 To count - 1
                result(i) = ReadSingleAt(buffer, i * 4)
            Next

            Return result
        End Function

        Public Function ReadDouble(reader As ByteReader, count As Integer) As Double()
            Dim buffer As Byte() = reader.ReadBytes(count * 8)
            Dim result As Double() = New Double(count - 1) {}

            For i As Integer = 0 To count - 1
                result(i) = ReadDoubleAt(buffer, i * 8)
            Next

            Return result
        End Function

        Public Function ReadByteArrays(reader As ByteReader, count As Integer) As Byte()()
            Dim result As Byte()() = New Byte(count - 1)() {}

            For i As Integer = 0 To count - 1
                Dim size As Integer = reader.ReadInt32()

                If size < 0 Then
                    Throw New ParquetException($"invalid byte array length {size} in the PLAIN encoded page!")
                End If

                result(i) = reader.ReadBytes(size)
            Next

            Return result
        End Function

        Public Function ReadFixedLenByteArrays(reader As ByteReader, count As Integer, typeLength As Integer) As Byte()()
            If typeLength <= 0 Then
                Throw New ParquetException("the type_length of a FIXED_LEN_BYTE_ARRAY column is missing in the schema!")
            End If

            Dim result As Byte()() = New Byte(count - 1)() {}

            For i As Integer = 0 To count - 1
                result(i) = reader.ReadBytes(typeLength)
            Next

            Return result
        End Function

        Public Function ReadInt96(reader As ByteReader, count As Integer) As Byte()()
            Dim result As Byte()() = New Byte(count - 1)() {}

            For i As Integer = 0 To count - 1
                result(i) = reader.ReadBytes(12)
            Next

            Return result
        End Function

        Public Function ReadInt32At(buffer As Byte(), offset As Integer) As Integer
            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt32(buffer, offset)
            End If

            Return BitConverter.ToInt32(Reverse(buffer, offset, 4), 0)
        End Function

        Public Function ReadInt64At(buffer As Byte(), offset As Integer) As Long
            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToInt64(buffer, offset)
            End If

            Return BitConverter.ToInt64(Reverse(buffer, offset, 8), 0)
        End Function

        Public Function ReadSingleAt(buffer As Byte(), offset As Integer) As Single
            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToSingle(buffer, offset)
            End If

            Return BitConverter.ToSingle(Reverse(buffer, offset, 4), 0)
        End Function

        Public Function ReadDoubleAt(buffer As Byte(), offset As Integer) As Double
            If BitConverter.IsLittleEndian Then
                Return BitConverter.ToDouble(buffer, offset)
            End If

            Return BitConverter.ToDouble(Reverse(buffer, offset, 8), 0)
        End Function

        Private Function Reverse(buffer As Byte(), offset As Integer, size As Integer) As Byte()
            Dim temp As Byte() = New Byte(size - 1) {}

            System.Buffer.BlockCopy(buffer, offset, temp, 0, size)
            Array.Reverse(temp)

            Return temp
        End Function

    End Module

End Namespace
