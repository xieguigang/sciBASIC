Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Wire\WireFormatException.vb
' /********************************************************************************/

Namespace Wire

''' <summary>
''' 当输入字节序列不是合法的 protobuf 线格式时抛出的异常。
''' </summary>
''' <remarks>
''' 异常对象携带出错时的绝对字节偏移量（<see cref="Position"/>）以及（在可知的情况下）
''' 正在解析的字段号（<see cref="FieldNumber"/>），便于定位损坏的数据。
''' 异常消息中不会包含大块 payload 内容。
''' </remarks>
Public Class WireFormatException : Inherits FormatException

    ''' <summary>
    ''' 出错位置在底层数据流中的绝对字节偏移量；未知时为 -1。
    ''' </summary>
    Public ReadOnly Property Position As Long

    ''' <summary>
    ''' 正在解析的字段号；未知时为 -1。
    ''' </summary>
    Public ReadOnly Property FieldNumber As Integer

    Public Sub New(message As String)
        MyBase.New(message)
        Position = -1
        FieldNumber = -1
    End Sub

    Public Sub New(message As String, innerException As Exception)
        MyBase.New(message, innerException)
        Position = -1
        FieldNumber = -1
    End Sub

    Public Sub New(message As String, position As Long)
        MyBase.New($"{message} (offset={position})")
        Position = position
        FieldNumber = -1
    End Sub

    Public Sub New(message As String, position As Long, fieldNumber As Integer)
        MyBase.New($"{message} (offset={position}, field={fieldNumber})")
        Position = position
        FieldNumber = fieldNumber
    End Sub

    Public Sub New(message As String, position As Long, fieldNumber As Integer, innerException As Exception)
        MyBase.New($"{message} (offset={position}, field={fieldNumber})", innerException)
        Position = position
        FieldNumber = fieldNumber
    End Sub

End Class

End Namespace
