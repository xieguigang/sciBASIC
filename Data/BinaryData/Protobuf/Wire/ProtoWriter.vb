Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Wire\ProtoWriter.vb
' /********************************************************************************/

Imports System.Buffers
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Wire

    ''' <summary>
    ''' protobuf 线格式写入器，包装一个 <see cref="BinaryDataWriter"/> 并补齐 varint / zigzag /
    ''' 长度前缀等编码能力。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 构造时会强制把底层 <see cref="BinaryDataWriter.ByteOrder"/> 设置为
    ''' <c>ByteOrder.LittleEndian</c>：protobuf 的 fixed32 / fixed64 一律使用小端字节序，
    ''' 而 <see cref="BinaryDataWriter"/> 默认取系统字节序，不能依赖运行平台。
    ''' </para>
    ''' <para>
    ''' 长度前缀无法通过 <c>ReserveOffset()</c> 回填（该 API 只支持定长 4 字节 UInt32），
    ''' 因此所有 length-delimited 载荷（string / bytes / 嵌套消息 / packed）都采用
    ''' "先写入临时缓冲、再写 varint 长度、最后拷贝" 的策略。
    ''' </para>
    ''' </remarks>
    Public Class ProtoWriter : Implements IDisposable

        ''' <summary>
        ''' 允许的最大消息嵌套深度，用于防止恶意构造的超深递归消息。
        ''' </summary>
        Public Const MaxDepth As Integer = 64

        ReadOnly _writer As BinaryDataWriter
        ReadOnly _ownsWriter As Boolean
        Dim _depth As Integer
        Dim _buffer As MemoryStream

        ''' <summary>
        ''' 创建一个写入内部内存缓冲的写入器，可通过 <see cref="ToArray"/> 取回编码结果。
        ''' </summary>
        Public Sub New()
            _buffer = New MemoryStream()
            _writer = New BinaryDataWriter(_buffer, leaveOpen:=True)
            _writer.ByteOrder = ByteOrder.LittleEndian
            _ownsWriter = True
        End Sub

        ''' <summary>
        ''' 创建包装指定输出流的写入器。
        ''' </summary>
        ''' <param name="output">目标输出流。</param>
        ''' <param name="leaveOpen">释放本对象时是否保持 <paramref name="output"/> 打开（序列化到调用方提供的流时应设为 True）。</param>
        Public Sub New(output As Stream, Optional leaveOpen As Boolean = False)
            If output Is Nothing Then
                Throw New ArgumentNullException(NameOf(output))
            End If

            _writer = New BinaryDataWriter(output, leaveOpen)
            _writer.ByteOrder = ByteOrder.LittleEndian
            _ownsWriter = True
        End Sub

        ''' <summary>
        ''' 包装一个已经存在的 <see cref="BinaryDataWriter"/>。
        ''' </summary>
        ''' <param name="writer">底层写入器。</param>
        ''' <param name="ownsWriter">释放本对象时是否一并释放 <paramref name="writer"/>。</param>
        Public Sub New(writer As BinaryDataWriter, Optional ownsWriter As Boolean = False)
            If writer Is Nothing Then
                Throw New ArgumentNullException(NameOf(writer))
            End If

            _writer = writer
            _writer.ByteOrder = ByteOrder.LittleEndian
            _ownsWriter = ownsWriter
        End Sub

        ''' <summary>
        ''' 当前写入位置（相对于底层流的绝对偏移量）。
        ''' </summary>
        Public ReadOnly Property Position As Long
            Get
                Return _writer.Position
            End Get
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
        ''' 底层 <see cref="BinaryDataWriter"/> 实例，供高级场景直接操作原始字节。
        ''' </summary>
        Public ReadOnly Property BaseWriter As BinaryDataWriter
            Get
                Return _writer
            End Get
        End Property

#Region "VARINT"

        ''' <summary>
        ''' 写入无符号 varint（LEB128，每字节 7 位 + 续位）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteVarint(value As ULong)
            Do While value >= &H80UL
                _writer.Write(CByte((value And &H7FUL) Or &H80UL))
                value >>= 7
            Loop

            _writer.Write(CByte(value))
        End Sub

        ''' <summary>
        ''' 写入 int32。负值按 protobuf 规范做 64 位符号扩展，占用 10 字节。
        ''' </summary>
        Public Sub WriteInt32(value As Integer)
            If value < 0 Then
                ' 符号扩展：高 32 位全 1 与低 32 位原始位拼接，编码后恰好为 10 字节
                Dim low As ULong = CULng(value And &HFFFFFFFFL)
                WriteVarint(&HFFFFFFFF00000000UL Or low)
            Else
                WriteVarint(CULng(value))
            End If
        End Sub

        ''' <summary>
        ''' 写入 int64。负值直接按 64 位补码做 varint 编码，占用 10 字节。
        ''' </summary>
        Public Sub WriteInt64(value As Long)
            If value < 0 Then
                WriteVarint(CULng(value And Long.MaxValue) Or &H8000000000000000UL)
            Else
                WriteVarint(CULng(value))
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteUInt32(value As UInteger)
            WriteVarint(CULng(value))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteUInt64(value As ULong)
            WriteVarint(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteSInt32(value As Integer)
            WriteVarint(CULng(ProtoWireFormat.ZigZagEncode32(value)))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteSInt64(value As Long)
            WriteVarint(ProtoWireFormat.ZigZagEncode64(value))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteBoolean(value As Boolean)
            WriteVarint(If(value, 1UL, 0UL))
        End Sub

        ''' <summary>
        ''' 写入枚举值：protobuf 的枚举按 int32 varint 编码。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteEnumValue(value As Integer)
            WriteInt32(value)
        End Sub

#End Region

#Region "FIXED WIDTH"

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteFixed32(value As UInteger)
            _writer.Write(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteFixed64(value As ULong)
            _writer.Write(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteSFixed32(value As Integer)
            _writer.Write(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteSFixed64(value As Long)
            _writer.Write(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteSingle(value As Single)
            _writer.Write(value)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteDouble(value As Double)
            _writer.Write(value)
        End Sub

#End Region

#Region "TAG / LENGTH DELIMITED"

        ''' <summary>
        ''' 写入字段 tag：<c>(fieldNumber &lt;&lt; 3) | wireType</c>。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteTag(fieldNumber As Integer, wireType As WireType)
            WriteVarint(CULng(ProtoWireFormat.MakeTag(fieldNumber, wireType)))
        End Sub

        ''' <summary>
        ''' 写入 varint 长度前缀。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteLengthPrefix(length As Integer)
            If length < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(length), length, "长度前缀不能为负数")
            End If

            WriteVarint(CULng(length))
        End Sub

        ''' <summary>
        ''' 写入一段原始字节（不加长度前缀）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub WriteRawBytes(data As Byte())
            If data Is Nothing OrElse data.Length = 0 Then
                Return
            End If

            _writer.Write(data)
        End Sub

        ''' <summary>
        ''' 写入长度前缀 + 原始字节。
        ''' </summary>
        Public Sub WriteLengthDelimited(payload As Byte())
            Dim length As Integer = If(payload Is Nothing, 0, payload.Length)

            WriteLengthPrefix(length)

            If length > 0 Then
                _writer.Write(payload)
            End If
        End Sub

        ''' <summary>
        ''' 写入 UTF-8 字符串（varint 长度前缀）。
        ''' </summary>
        Public Sub WriteString(value As String)
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value), "protobuf 的 string 字段不允许写入 Nothing")
            End If

            Dim byteCount As Integer = Encoding.UTF8.GetByteCount(value)

            WriteLengthPrefix(byteCount)

            If byteCount = 0 Then
                Return
            End If

            Dim rented As Byte() = ArrayPool(Of Byte).Shared.Rent(byteCount)

            Try
                Call Encoding.UTF8.GetBytes(value, 0, value.Length, rented, 0)
                _writer.Write(rented, 0, byteCount)
            Finally
                ArrayPool(Of Byte).Shared.Return(rented)
            End Try
        End Sub

        ''' <summary>
        ''' 写入嵌套消息：先用临时缓冲收集 <paramref name="body"/> 生成的全部字节，
        ''' 再写出 <c>tag + varint 长度 + 载荷</c>。
        ''' </summary>
        ''' <param name="fieldNumber">字段号。</param>
        ''' <param name="body">负责写出嵌套消息内部字段的回调。</param>
        Public Sub WriteMessage(fieldNumber As Integer, body As Action(Of ProtoWriter))
            If body Is Nothing Then
                Throw New ArgumentNullException(NameOf(body))
            End If

            If _depth + 1 > MaxDepth Then
                Throw New InvalidOperationException($"消息嵌套深度超过上限 {MaxDepth}，疑似循环引用或恶意数据")
            End If

            Using buffer As New MemoryStream()
                Using sub As New ProtoWriter(buffer, leaveOpen:=True)
                    sub._depth = _depth + 1
                    Call body(sub)
                    Call sub.Flush()
                End Using

                WriteTag(fieldNumber, WireType.LengthDelimited)
                WriteLengthPrefix(CInt(buffer.Length))
                Call buffer.WriteTo(_writer.BaseStream)
            End Using
        End Sub

#End Region

        ''' <summary>
        ''' 把缓冲中的数据立即刷到底层流。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Flush()
            _writer.Flush()
        End Sub

        ''' <summary>
        ''' 取回写入内部内存缓冲的全部字节。仅当使用无参构造函数创建时可用。
        ''' </summary>
        Public Function ToArray() As Byte()
            If _buffer Is Nothing Then
                Throw New InvalidOperationException("当前写入器不是基于内部内存缓冲创建的，无法调用 ToArray")
            End If

            Call _writer.Flush()

            Return _buffer.ToArray()
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                Call _writer.Flush()
            Catch ex As Exception
                ' 释放阶段的刷盘失败不应掩盖调用方已经出现的异常
            End Try

            If _ownsWriter Then
                Call _writer.Dispose()
            End If

            _buffer = Nothing
        End Sub

    End Class

End Namespace
