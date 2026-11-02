Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Wire\ProtoWireFormat.vb
' /********************************************************************************/

Imports System.Runtime.CompilerServices

''' <summary>
''' protobuf 线格式的纯函数辅助工具：tag 拼装、varint 尺寸预估、zigzag 变换。
''' </summary>
''' <remarks>
''' 该模块不涉及任何 IO，可安全地在序列化前的尺寸预估、动态消息构造等场景中复用。
''' </remarks>
Public Module ProtoWireFormat

    ''' <summary>
    ''' 单个 varint 最多占用的字节数（64 位整数）。
    ''' </summary>
    Public Const MaxVarintBytes As Integer = 10

    ''' <summary>
    ''' protobuf 允许的最大字段号（2^29 - 1）。
    ''' </summary>
    Public Const MaxFieldNumber As Integer = 536870911

    ''' <summary>
    ''' 保留字段号区间的起始值（含）。
    ''' </summary>
    Public Const MinReservedFieldNumber As Integer = 19000

    ''' <summary>
    ''' 保留字段号区间的结束值（含）。
    ''' </summary>
    Public Const MaxReservedFieldNumber As Integer = 19999

    ''' <summary>
    ''' 计算 <paramref name="value"/> 用 varint 编码后占用的字节数。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetVarintSize(value As ULong) As Integer
        Dim size As Integer = 1

        Do While value >= &H80UL
            value >>= 7
            size += 1
        Loop

        Return size
    End Function

    ''' <summary>
    ''' 判断字段号是否合法：必须位于 [1, <see cref="MaxFieldNumber"/>]，且不得落在保留区间。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function IsValidFieldNumber(fieldNumber As Integer) As Boolean
        If fieldNumber < 1 OrElse fieldNumber > MaxFieldNumber Then
            Return False
        End If

        Return fieldNumber < MinReservedFieldNumber OrElse fieldNumber > MaxReservedFieldNumber
    End Function

    ''' <summary>
    ''' 将字段号与线格式类型打包成 32 位 tag：<c>(fieldNumber &lt;&lt; 3) | wireType</c>。
    ''' </summary>
    Public Function MakeTag(fieldNumber As Integer, wireType As WireType) As UInteger
        If fieldNumber < 1 OrElse fieldNumber > MaxFieldNumber Then
            Throw New ArgumentOutOfRangeException(NameOf(fieldNumber), fieldNumber, $"非法的 protobuf 字段号，有效范围为 1..{MaxFieldNumber}")
        End If

        Return (CUInt(fieldNumber) << 3) Or CUInt(CByte(wireType))
    End Function

    ''' <summary>
    ''' 从 tag 中提取字段号。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetFieldNumber(tag As UInteger) As Integer
        Return CInt(tag >> 3)
    End Function

    ''' <summary>
    ''' 从 tag 中提取线格式类型。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetWireType(tag As UInteger) As WireType
        Return CType(tag And 7UI, WireType)
    End Function

    ''' <summary>
    ''' 计算某个字段号与线格式类型组合后的 tag 占用的字节数。
    ''' </summary>
    Public Function GetTagSize(fieldNumber As Integer, wireType As WireType) As Integer
        Return GetVarintSize(CULng(MakeTag(fieldNumber, wireType)))
    End Function

    ''' <summary>
    ''' zigzag 编码 32 位有符号整数（sint32）。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ZigZagEncode32(value As Integer) As UInteger
        Dim wide As Long = value
        Return CUInt((wide << 1) Xor (wide >> 31))
    End Function

    ''' <summary>
    ''' zigzag 解码 32 位有符号整数（sint32）。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ZigZagDecode32(value As UInteger) As Integer
        Dim wide As Long = CLng(value)
        Return CInt((wide >> 1) Xor -(wide And 1L))
    End Function

    ''' <summary>
    ''' zigzag 编码 64 位有符号整数（sint64）。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ZigZagEncode64(value As Long) As ULong
        Dim encoded As Long = (value << 1) Xor (value >> 63)

        If encoded < 0 Then
            Return CULng(encoded And Long.MaxValue) Or &H8000000000000000UL
        Else
            Return CULng(encoded)
        End If
    End Function

    ''' <summary>
    ''' zigzag 解码 64 位有符号整数（sint64）。
    ''' </summary>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ZigZagDecode64(value As ULong) As Long
        Dim wide As Long

        If (value And &H8000000000000000UL) <> 0UL Then
            wide = CLng(value And &H7FFFFFFFFFFFFFFFUL) Or Long.MinValue
        Else
            wide = CLng(value)
        End If

        Return (wide >> 1) Xor -(wide And 1L)
    End Function

End Module
