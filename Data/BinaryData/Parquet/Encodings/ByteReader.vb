Option Strict On
Option Explicit On
Option Infer On

Imports System.IO

Namespace Encodings

    ''' <summary>
    ''' 针对 Parquet 页数据的字节读取游标，所有多字节整数均为<b>小端</b>字节序。
    ''' </summary>
    Public NotInheritable Class ByteReader

        Private Const MaxVarintBytes As Integer = 10

        Private ReadOnly _data As Byte()
        Private _position As Integer

        Public Sub New(data As Byte(), Optional position As Integer = 0)
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            _data = data
            _position = position
        End Sub

        ''' <summary>
        ''' 底层缓冲区。
        ''' </summary>
        Public ReadOnly Property Data As Byte()
            Get
                Return _data
            End Get
        End Property

        Public Property Position As Integer
            Get
                Return _position
            End Get
            Set(value As Integer)
                _position = value
            End Set
        End Property

        Public ReadOnly Property Length As Integer
            Get
                Return _data.Length
            End Get
        End Property

        Public ReadOnly Property Remaining As Integer
            Get
                Return _data.Length - _position
            End Get
        End Property

        Public ReadOnly Property EndOfData As Boolean
            Get
                Return _position >= _data.Length
            End Get
        End Property

        Public Sub Ensure(count As Integer)
            If count < 0 Then
                Throw New ParquetException($"invalid data length: {count}")
            End If
            If _position + count > _data.Length Then
                Throw New ParquetException($"unexpected end of the parquet page data: required {count} bytes at position {_position}, but only {_data.Length - _position} bytes are available!")
            End If
        End Sub

        Public Sub Skip(count As Integer)
            Call Ensure(count)
            _position += count
        End Sub

        Public Function ReadByte() As Byte
            Call Ensure(1)

            Dim value As Byte = _data(_position)
            _position += 1

            Return value
        End Function

        Public Function ReadInt16() As Short
            Call Ensure(2)

            Dim value As Short = BitConverter.ToInt16(ToLittleEndian(_data, _position, 2), 0)
            _position += 2

            Return value
        End Function

        Public Function ReadInt32() As Integer
            Call Ensure(4)

            Dim value As Integer = BitConverter.ToInt32(ToLittleEndian(_data, _position, 4), 0)
            _position += 4

            Return value
        End Function

        Public Function ReadInt64() As Long
            Call Ensure(8)

            Dim value As Long = BitConverter.ToInt64(ToLittleEndian(_data, _position, 8), 0)
            _position += 8

            Return value
        End Function

        Public Function ReadSingle() As Single
            Call Ensure(4)

            Dim value As Single = BitConverter.ToSingle(ToLittleEndian(_data, _position, 4), 0)
            _position += 4

            Return value
        End Function

        Public Function ReadDouble() As Double
            Call Ensure(8)

            Dim value As Double = BitConverter.ToDouble(ToLittleEndian(_data, _position, 8), 0)
            _position += 8

            Return value
        End Function

        Public Function ReadBytes(count As Integer) As Byte()
            Call Ensure(count)

            Dim buffer As Byte() = New Byte(count - 1) {}

            If count > 0 Then
                System.Buffer.BlockCopy(_data, _position, buffer, 0, count)
            End If

            _position += count

            Return buffer
        End Function

        Public Function ReadVarint() As ULong
            Dim result As ULong = 0UL
            Dim shift As Integer = 0
            Dim count As Integer = 0

            Do
                Dim b As Byte = ReadByte()

                result = result Or (CULng(b And &H7F) << shift)
                count += 1

                If (b And &H80) = 0 Then
                    Return result
                End If

                shift += 7

                If count >= MaxVarintBytes Then
                    Throw New ParquetException("the parquet varint value is too long, the page data is corrupted!")
                End If
            Loop
        End Function

        Public Function ReadZigZag() As Long
            Dim value As ULong = ReadVarint()
            Dim raw As Long = BitConverter.ToInt64(BitConverter.GetBytes(value >> 1), 0)

            If (value And 1UL) <> 0UL Then
                raw = Not raw
            End If

            Return raw
        End Function

        Private Shared Function ToLittleEndian(data As Byte(), offset As Integer, size As Integer) As Byte()
            Dim buffer As Byte() = New Byte(size - 1) {}

            System.Buffer.BlockCopy(data, offset, buffer, 0, size)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(buffer)
            End If

            Return buffer
        End Function

    End Class

    ''' <summary>
    ''' 面向输出流的字节写入辅助函数（小端字节序）。
    ''' </summary>
    Public Module ByteWriter

        Public Sub WriteInt32LE(stream As Stream, value As Integer)
            Dim bytes As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(bytes)
            End If

            stream.Write(bytes, 0, 4)
        End Sub

        Public Sub WriteInt64LE(stream As Stream, value As Long)
            Dim bytes As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(bytes)
            End If

            stream.Write(bytes, 0, 8)
        End Sub

        Public Sub WriteSingleLE(stream As Stream, value As Single)
            Dim bytes As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(bytes)
            End If

            stream.Write(bytes, 0, 4)
        End Sub

        Public Sub WriteDoubleLE(stream As Stream, value As Double)
            Dim bytes As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(bytes)
            End If

            stream.Write(bytes, 0, 8)
        End Sub

        Public Sub WriteVarint(stream As Stream, value As ULong)
            Dim current As ULong = value

            Do
                Dim b As Byte = CByte(current And &H7FUL)

                current >>= 7

                If current <> 0UL Then
                    b = CByte(b Or &H80)
                End If

                stream.WriteByte(b)
            Loop While current <> 0UL
        End Sub

        Public Sub WriteZigZag(stream As Stream, value As Long)
            Dim zz As Long = (value << 1) Xor (value >> 63)

            Call WriteVarint(stream, BitConverter.ToUInt64(BitConverter.GetBytes(zz), 0))
        End Sub

    End Module

End Namespace
