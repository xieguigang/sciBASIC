Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Wire\ProtoReader.vb
' /********************************************************************************/

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Wire

    ''' <summary>
    ''' protobuf 线格式读取器，包装一个 <see cref="BinaryDataReader"/> 并补齐 varint / zigzag /
    ''' 长度前缀等解码能力。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 构造时会强制把底层 <see cref="BinaryDataReader.ByteOrder"/> 设置为
    ''' <c>ByteOrder.LittleEndian</c>：protobuf 的 fixed32 / fixed64 一律使用小端字节序。
    ''' </para>
    ''' <para>
    ''' 嵌套消息通过 <see cref="PushLimit"/> / <see cref="PopLimit"/> 维护一个读取边界栈来实现，
    ''' 不为每个子消息分配新的流或数组拷贝。
    ''' </para>
    ''' </remarks>
    Public Class ProtoReader : Implements IDisposable

        ''' <summary>
        ''' 允许的最大消息嵌套深度。
        ''' </summary>
        Public Const MaxDepth As Integer = 64

        ReadOnly _reader As BinaryDataReader
        ReadOnly _ownsReader As Boolean
        ReadOnly _limits As New Stack(Of Long)
        ReadOnly _sourceLength As Long

        Dim _limit As Long
        Dim _depth As Integer

        ''' <summary>
        ''' 包装指定的输入流。
        ''' </summary>
        ''' <param name="input">输入流。</param>
        ''' <param name="leaveOpen">释放本对象时是否保持 <paramref name="input"/> 打开。</param>
        Public Sub New(input As Stream, Optional leaveOpen As Boolean = False)
            If input Is Nothing Then
                Throw New ArgumentNullException(NameOf(input))
            End If

            _reader = New BinaryDataReader(input, leaveOpen)
            _reader.ByteOrder = ByteOrder.LittleEndian
            _ownsReader = True
            _limit = Long.MaxValue
            _sourceLength = If(input.CanSeek, input.Length, -1)
        End Sub

        ''' <summary>
        ''' 包装一个已经存在的 <see cref="BinaryDataReader"/>。
        ''' </summary>
        Public Sub New(reader As BinaryDataReader, Optional ownsReader As Boolean = False)
            If reader Is Nothing Then
                Throw New ArgumentNullException(NameOf(reader))
            End If

            _reader = reader
            _reader.ByteOrder = ByteOrder.LittleEndian
            _ownsReader = ownsReader
            _limit = Long.MaxValue

            If reader.BaseStream.CanSeek Then
                _sourceLength = reader.BaseStream.Length
            Else
                _sourceLength = -1
            End If
        End Sub

        ''' <summary>
        ''' 当前读取位置（相对于底层流的绝对偏移量）。
        ''' </summary>
        Public Property Position As Long
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _reader.Position
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As Long)
                _reader.Position = value
            End Set
        End Property

        ''' <summary>
        ''' 当前的嵌套消息深度。
        ''' </summary>
        Public ReadOnly Property Depth As Integer
            Get
                Return _depth
            End Get
        End Property

        ''' <summary>
        ''' 当前消息的读取边界（绝对偏移量）。
        ''' </summary>
        Public ReadOnly Property Limit As Long
            Get
                Return _limit
            End Get
        End Property

        ''' <summary>
        ''' 底层 <see cref="BinaryDataReader"/> 实例。
        ''' </summary>
        Public ReadOnly Property BaseReader As BinaryDataReader
            Get
                Return _reader
            End Get
        End Property

        ''' <summary>
        ''' 是否已经到达当前消息（或数据流）的末尾。
        ''' </summary>
        Public ReadOnly Property EndOfStream As Boolean
            Get
                If _reader.Position >= _limit Then
                    Return True
                End If

                If _sourceLength >= 0 AndAlso _reader.Position >= _sourceLength Then
                    Return True
                End If

                Return False
            End Get
        End Property

#Region "LIMIT"

        ''' <summary>
        ''' 在当前位置压入一个新的读取边界，后续 <see cref="ReadTag"/> 与读取操作不得越界。
        ''' </summary>
        ''' <param name="byteCount">子消息（或 packed 块）占用的字节数。</param>
        Public Sub PushLimit(byteCount As Integer)
            If byteCount < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(byteCount), byteCount, "长度不能为负数")
            End If

            If _depth + 1 > MaxDepth Then
                Throw New InvalidOperationException($"消息嵌套深度超过上限 {MaxDepth}，疑似循环引用或恶意数据")
            End If

            Dim newLimit As Long = _reader.Position + byteCount

            If newLimit > _limit Then
                Throw New WireFormatException($"长度前缀 {byteCount} 超过了当前消息剩余的可用字节数", _reader.Position)
            End If

            _limits.Push(_limit)
            _limit = newLimit
            _depth += 1
        End Sub

        ''' <summary>
        ''' 弹出当前读取边界，并跳过子消息中尚未读取的字节。
        ''' </summary>
        Public Sub PopLimit()
            If _limits.Count = 0 Then
                Throw New InvalidOperationException("PopLimit 调用次数超过了 PushLimit")
            End If

            If _reader.Position < _limit Then
                _reader.Position = _limit
            End If

            _limit = _limits.Pop()
            _depth -= 1
        End Sub

        ''' <summary>
        ''' 跳过指定字节数。
        ''' </summary>
        Public Sub SkipBytes(count As Integer)
            If count <= 0 Then
                Return
            End If

            If _reader.Position + count > _limit Then
                Throw New WireFormatException($"跳过 {count} 字节时越过了当前消息的边界", _reader.Position)
            End If

            _reader.Position += count
        End Sub

#End Region

#Region "TAG"

        ''' <summary>
        ''' 读取下一个字段 tag。到达当前消息或数据流末尾时返回 False。
        ''' </summary>
        ''' <param name="fieldNumber">输出参数：字段号。</param>
        ''' <param name="wireType">输出参数：线格式类型。</param>
        Public Function ReadTag(ByRef fieldNumber As Integer, ByRef wireType As WireType) As Boolean
            If EndOfStream Then
                Return False
            End If

            Dim tag As ULong = ReadVarint()

            If tag > UInteger.MaxValue Then
                Throw New WireFormatException("字段 tag 超过 32 位，数据已损坏", _reader.Position)
            End If

            Dim number As Integer = CInt(tag >> 3)

            If number < 1 OrElse number > ProtoWireFormat.MaxFieldNumber Then
                Throw New WireFormatException($"非法的字段号 {number}", _reader.Position, -1)
            End If

            Dim kind As WireType = CType(tag And 7UL, WireType)

            If kind = WireType.StartGroup OrElse kind = WireType.EndGroup Then
                Throw New NotSupportedException($"暂不支持 protobuf 的已废弃分组语法（wire type={(CByte(kind))}）")
            End If

            fieldNumber = number
            wireType = kind

            Return True
        End Function

#End Region

#Region "VARINT"

        ''' <summary>
        ''' 读取一个 varint（最多 10 字节）。
        ''' </summary>
        Public Function ReadVarint() As ULong
            Dim result As ULong = 0UL
            Dim shift As Integer = 0
            Dim count As Integer = 0

            Do
                Dim b As Byte = ReadRawByte()

                count += 1

                If shift = 63 AndAlso b > 1 Then
                    Throw New WireFormatException("varint 数值超过 64 位表示范围", _reader.Position)
                End If

                result = result Or (CULng(b And &H7F) << shift)

                If (b And &H80) = 0 Then
                    Return result
                End If

                shift += 7

                If count >= ProtoWireFormat.MaxVarintBytes Then
                    Throw New WireFormatException("varint 编码超过 10 字节", _reader.Position)
                End If
            Loop

        End Function

        ''' <summary>
        ''' 读取一个 varint，并同时返回它占用的原始字节（用于未知字段的原样保留）。
        ''' </summary>
        Public Function ReadVarintRaw(ByRef value As ULong) As Byte()
            Dim buffer(0 To ProtoWireFormat.MaxVarintBytes - 1) As Byte
            Dim count As Integer = 0
            Dim result As ULong = 0UL
            Dim shift As Integer = 0

            Do
                Dim b As Byte = ReadRawByte()

                buffer(count) = b
                count += 1

                If shift = 63 AndAlso b > 1 Then
                    Throw New WireFormatException("varint 数值超过 64 位表示范围", _reader.Position)
                End If

                result = result Or (CULng(b And &H7F) << shift)

                If (b And &H80) = 0 Then
                    value = result

                    Dim raw(count - 1) As Byte
                    Call Array.Copy(buffer, raw, count)

                    Return raw
                End If

                shift += 7

                If count >= ProtoWireFormat.MaxVarintBytes Then
                    Throw New WireFormatException("varint 编码超过 10 字节", _reader.Position)
                End If
            Loop

        End Function

        ''' <summary>
        ''' 读取 int32（负值为 10 字节符号扩展编码）。
        ''' </summary>
        Public Function ReadInt32() As Integer
            Dim raw As ULong = ReadVarint() And &HFFFFFFFFUL

            If (raw And &H80000000UL) <> 0UL Then
                Return CInt(raw And &H7FFFFFFFUL) Or Integer.MinValue
            Else
                Return CInt(raw)
            End If
        End Function

        ''' <summary>
        ''' 读取 int64。
        ''' </summary>
        Public Function ReadInt64() As Long
            Dim raw As ULong = ReadVarint()

            If (raw And &H8000000000000000UL) <> 0UL Then
                Return CLng(raw And &H7FFFFFFFFFFFFFFFUL) Or Long.MinValue
            Else
                Return CLng(raw)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadUInt32() As UInteger
            Return CUInt(ReadVarint() And &HFFFFFFFFUL)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadUInt64() As ULong
            Return ReadVarint()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadSInt32() As Integer
            Return ProtoWireFormat.ZigZagDecode32(CUInt(ReadVarint() And &HFFFFFFFFUL))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadSInt64() As Long
            Return ProtoWireFormat.ZigZagDecode64(ReadVarint())
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadBoolean() As Boolean
            Return ReadVarint() <> 0UL
        End Function

        ''' <summary>
        ''' 读取枚举值。protobuf 的枚举按 int32 varint 编码，未知值同样会被接受。
        ''' </summary>
        Public Function ReadEnumValue(Of TEnum As Structure)() As TEnum
            Return CType(System.Enum.ToObject(GetType(TEnum), ReadInt32()), TEnum)
        End Function

#End Region

#Region "FIXED WIDTH"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadFixed32() As UInteger
            Return _reader.ReadUInt32()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadFixed64() As ULong
            Return _reader.ReadUInt64()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadSFixed32() As Integer
            Return _reader.ReadInt32()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadSFixed64() As Long
            Return _reader.ReadInt64()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadSingle() As Single
            Return _reader.ReadSingle()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadDouble() As Double
            Return _reader.ReadDouble()
        End Function

#End Region

#Region "LENGTH DELIMITED"

        ''' <summary>
        ''' 读取长度前缀并校验它在当前消息边界内。
        ''' </summary>
        Public Function ReadLengthPrefix() As Integer
            Dim length As ULong = ReadVarint()

            If length > CUInt(Integer.MaxValue) Then
                Throw New WireFormatException($"长度前缀 {length} 超出可支持范围", _reader.Position)
            End If

            Dim count As Integer = CInt(length)

            If _reader.Position + count > _limit Then
                Throw New WireFormatException($"长度前缀 {count} 超出了当前消息剩余的可用字节数", _reader.Position)
            End If

            If _sourceLength >= 0 AndAlso _reader.Position + count > _sourceLength Then
                Throw New WireFormatException($"长度前缀 {count} 超出了数据流的实际长度", _reader.Position)
            End If

            Return count
        End Function

        ''' <summary>
        ''' 读取指定数量的原始字节。数据不足时抛出 <see cref="WireFormatException"/>。
        ''' </summary>
        Public Function ReadRawBytes(count As Integer) As Byte()
            If count = 0 Then
                Return Array.Empty(Of Byte)()
            End If

            If count < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(count), count, "读取长度不能为负数")
            End If

            If _reader.Position + count > _limit Then
                Throw New WireFormatException($"读取 {count} 字节时越过了当前消息的边界", _reader.Position)
            End If

            Dim buffer As Byte()

            Try
                buffer = _reader.ReadBytes(count)
            Catch ex As EndOfStreamException
                Throw New WireFormatException("读取字节时在流末尾被截断", _reader.Position, -1, ex)
            End Try

            If buffer Is Nothing OrElse buffer.Length <> count Then
                Dim actual As Integer = If(buffer Is Nothing, 0, buffer.Length)
                Throw New WireFormatException($"期望读取 {count} 字节，但流中只剩 {actual} 字节", _reader.Position)
            End If

            Return buffer
        End Function

        ''' <summary>
        ''' 读取一个原始字节（不经过 BinaryReader 的缓冲）。
        ''' </summary>
        Public Function ReadRawByte() As Byte
            If _reader.Position >= _limit Then
                Throw New WireFormatException("读取字节时越过了当前消息的边界", _reader.Position)
            End If

            Try
                Return _reader.ReadByte()
            Catch ex As EndOfStreamException
                Throw New WireFormatException("读取字节时在流末尾被截断", _reader.Position, -1, ex)
            End Try
        End Function

        ''' <summary>
        ''' 读取 length-delimited 的字节数组（bytes 字段）。
        ''' </summary>
        Public Function ReadBytes() As Byte()
            Return ReadRawBytes(ReadLengthPrefix())
        End Function

        ''' <summary>
        ''' 读取 length-delimited 的 UTF-8 字符串。
        ''' </summary>
        Public Function ReadString() As String
            Return Encoding.UTF8.GetString(ReadBytes())
        End Function

#End Region

#Region "SKIP"

        ''' <summary>
        ''' 按线格式类型跳过当前字段的值。
        ''' </summary>
        Public Sub SkipField(wireType As WireType)
            Select Case wireType
                Case WireType.Varint
                    Call ReadVarint()

                Case WireType.Fixed64
                    Call SkipBytes(8)

                Case WireType.Fixed32
                    Call SkipBytes(4)

                Case WireType.LengthDelimited
                    Call SkipBytes(ReadLengthPrefix())

                Case Else
                    Throw New NotSupportedException($"暂不支持跳过 wire type={(CByte(wireType))} 的字段")
            End Select
        End Sub

        ''' <summary>
        ''' 原样读取当前字段的"值"部分（不含 tag），用于未知字段的保留与重放。
        ''' </summary>
        Public Function CaptureValueBytes(wireType As WireType) As Byte()
            Select Case wireType
                Case WireType.Varint
                    Dim value As ULong = 0UL
                    Return ReadVarintRaw(value)

                Case WireType.Fixed64
                    Return ReadRawBytes(8)

                Case WireType.Fixed32
                    Return ReadRawBytes(4)

                Case WireType.LengthDelimited
                    Dim length As ULong = 0UL
                    Dim prefix As Byte() = ReadVarintRaw(length)

                    If length > CUInt(Integer.MaxValue) Then
                        Throw New WireFormatException($"长度前缀 {length} 超出可支持范围", _reader.Position)
                    End If

                    Dim payload As Byte() = ReadRawBytes(CInt(length))
                    Dim raw(0 To prefix.Length + payload.Length - 1) As Byte

                    Call Array.Copy(prefix, 0, raw, 0, prefix.Length)
                    Call Array.Copy(payload, 0, raw, prefix.Length, payload.Length)

                    Return raw

                Case Else
                    Throw New NotSupportedException($"暂不支持保留 wire type={(CByte(wireType))} 的未知字段")
            End Select
        End Function

#End Region

        Public Sub Dispose() Implements IDisposable.Dispose
            If _ownsReader Then
                Call _reader.Dispose()
            End If
        End Sub

    End Class

End Namespace
