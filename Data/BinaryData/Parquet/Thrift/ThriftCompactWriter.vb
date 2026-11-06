Option Strict On
Option Explicit On
Option Infer On

Imports System.IO
Imports System.Text

Namespace Thrift

    ''' <summary>
    ''' Thrift compact protocol 写入器。
    ''' <para>
    ''' 只实现 Parquet 文件元数据（footer / page header）所使用到的子集：
    ''' struct / field id delta / zigzag varint / list / map / double / binary。
    ''' </para>
    ''' </summary>
    Public NotInheritable Class ThriftCompactWriter

        Private ReadOnly _stream As Stream
        Private ReadOnly _fieldStack As New Stack(Of Integer)
        Private _lastFieldId As Integer

        Public Sub New(stream As Stream)
            If stream Is Nothing Then
                Throw New ArgumentNullException(NameOf(stream))
            End If
            If Not stream.CanWrite Then
                Throw New ArgumentException("the output stream is not writable!", NameOf(stream))
            End If

            _stream = stream
        End Sub

        ''' <summary>
        ''' 当前已经写入的字节数量。
        ''' </summary>
        Public ReadOnly Property Position As Long
            Get
                Return _stream.Position
            End Get
        End Property

        Public Sub WriteStructBegin()
            Call _fieldStack.Push(_lastFieldId)
            _lastFieldId = 0
        End Sub

        Public Sub WriteStructEnd()
            Call _stream.WriteByte(0) ' STOP
            _lastFieldId = _fieldStack.Pop()
        End Sub

        Public Sub WriteFieldHeader(id As Integer, type As ThriftType)
            Dim delta As Integer = id - _lastFieldId

            If delta > 0 AndAlso delta <= 15 Then
                Call _stream.WriteByte(CByte((delta << 4) Or CInt(type)))
            Else
                Call _stream.WriteByte(CByte(CInt(type)))
                Call WriteZigZag(id)
            End If

            _lastFieldId = id
        End Sub

        ''' <summary>
        ''' 写入布尔字段：布尔值直接编码在字段头的低四位类型标记内。
        ''' </summary>
        Public Sub WriteBoolField(id As Integer, value As Boolean)
            Call WriteFieldHeader(id, If(value, ThriftType.True, ThriftType.False))
        End Sub

        Public Sub WriteI32Field(id As Integer, value As Integer)
            Call WriteFieldHeader(id, ThriftType.I32)
            Call WriteZigZag(value)
        End Sub

        Public Sub WriteI64Field(id As Integer, value As Long)
            Call WriteFieldHeader(id, ThriftType.I64)
            Call WriteZigZag(value)
        End Sub

        Public Sub WriteBinaryField(id As Integer, value As Byte())
            Call WriteFieldHeader(id, ThriftType.Binary)
            Call WriteBinary(value)
        End Sub

        Public Sub WriteStringField(id As Integer, value As String)
            Call WriteFieldHeader(id, ThriftType.Binary)
            Call WriteBinary(If(value Is Nothing, New Byte() {}, Encoding.UTF8.GetBytes(value)))
        End Sub

        ''' <summary>
        ''' 写入一个元素为 struct 的 list 字段（例如 schema / row_groups / columns）。
        ''' </summary>
        Public Sub WriteStructListField(id As Integer, count As Integer)
            Call WriteFieldHeader(id, ThriftType.List)
            Call WriteListHeader(count, ThriftType.Struct)
        End Sub

        Public Sub WriteListHeader(size As Integer, elementType As ThriftType)
            If size <= 14 Then
                Call _stream.WriteByte(CByte((size << 4) Or CInt(elementType)))
            Else
                Call _stream.WriteByte(CByte(&HF0 Or CInt(elementType)))
                Call WriteVarint(CULng(size))
            End If
        End Sub

        Public Sub WriteMapHeader(size As Integer, keyType As ThriftType, valueType As ThriftType)
            If size <= 0 Then
                Call _stream.WriteByte(0)
            Else
                Call WriteVarint(CULng(size))
                Call _stream.WriteByte(CByte((CInt(keyType) << 4) Or CInt(valueType)))
            End If
        End Sub

        Public Sub WriteByte(value As Byte)
            Call _stream.WriteByte(value)
        End Sub

        ''' <summary>
        ''' 写入列表内的布尔值（1 = True，2 = False）。
        ''' </summary>
        Public Sub WriteBoolValue(value As Boolean)
            Call _stream.WriteByte(If(value, CByte(1), CByte(2)))
        End Sub

        Public Sub WriteVarint(value As ULong)
            Dim current As ULong = value

            Do
                Dim b As Byte = CByte(current And &H7FUL)

                current >>= 7

                If current <> 0UL Then
                    b = CByte(b Or &H80)
                End If

                Call _stream.WriteByte(b)
            Loop While current <> 0UL
        End Sub

        Public Sub WriteZigZag(value As Long)
            ' (value << 1) ^ (value >> 63) 并以 64 位无符号整数重新解释
            Dim zz As Long = (value << 1) Xor (value >> 63)

            Call WriteVarint(AsUnsigned(zz))
        End Sub

        Public Sub WriteBinary(value As Byte())
            If value Is Nothing Then
                Call WriteVarint(0UL)
                Return
            End If

            Call WriteVarint(CULng(value.Length))
            Call _stream.Write(value, 0, value.Length)
        End Sub

        Public Sub WriteDoubleValue(value As Double)
            Dim bytes As Byte() = BitConverter.GetBytes(value)

            If Not BitConverter.IsLittleEndian Then
                Array.Reverse(bytes)
            End If

            Call _stream.Write(bytes, 0, bytes.Length)
        End Sub

        Private Shared Function AsUnsigned(value As Long) As ULong
            Return BitConverter.ToUInt64(BitConverter.GetBytes(value), 0)
        End Function

    End Class

End Namespace
