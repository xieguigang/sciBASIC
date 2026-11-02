Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\IProtoMessage.vb
' /********************************************************************************/

Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

Namespace Serialization

    ''' <summary>
    ''' 未知字段的处理策略。
    ''' </summary>
    Public Enum UnknownFieldBehavior

        ''' <summary>
        ''' 直接跳过未知字段。
        ''' </summary>
        Skip = 0

        ''' <summary>
        ''' 保留未知字段，并在下次序列化时原样重放（需要消息类型实现 <see cref="IProtoMessage"/>）。
        ''' </summary>
        Preserve = 1

    End Enum

    ''' <summary>
    ''' 由消息类型实现后，可以接收并保留反序列化过程中遇到的未知字段。
    ''' </summary>
    ''' <remarks>
    ''' 只有实现了本接口的实例才会收集未知字段；未实现时按
    ''' <see cref="SerializationContext.UnknownFieldBehavior"/> 的配置决定跳过或丢弃。
    ''' </remarks>
    Public Interface IProtoMessage

        ''' <summary>
        ''' 未知字段集合。序列化时其中的内容会被原样追加到消息末尾。
        ''' </summary>
        ReadOnly Property UnknownFields As UnknownFieldSet

    End Interface

    ''' <summary>
    ''' 未知字段集合：按"字段号 + 线格式类型 + 原始值字节"保存，保证重放时字节完全一致。
    ''' </summary>
    Public Class UnknownFieldSet

        ''' <summary>
        ''' 单条未知字段记录。
        ''' </summary>
        Friend Class Entry

            Friend ReadOnly FieldNumber As Integer
            Friend ReadOnly WireType As WireType
            Friend ReadOnly ValueBytes As Byte()

            Friend Sub New(fieldNumber As Integer, wireType As WireType, valueBytes As Byte())
                Me.FieldNumber = fieldNumber
                Me.WireType = wireType
                Me.ValueBytes = valueBytes
            End Sub

        End Class

        Private ReadOnly _entries As New List(Of Entry)

        ''' <summary>
        ''' 已保留的未知字段数量。
        ''' </summary>
        Public ReadOnly Property Count As Integer
            Get
                Return _entries.Count
            End Get
        End Property

        ''' <summary>
        ''' 追加一条未知字段。
        ''' </summary>
        ''' <param name="fieldNumber">字段号。</param>
        ''' <param name="wireType">线格式类型。</param>
        ''' <param name="valueBytes">值的原始编码字节（不含 tag，含 length-delimited 的长度前缀）。</param>
        Public Function Add(fieldNumber As Integer, wireType As WireType, valueBytes As Byte()) As UnknownFieldSet
            Call _entries.Add(New Entry(fieldNumber, wireType, If(valueBytes, Array.Empty(Of Byte)())))

            Return Me
        End Function

        ''' <summary>
        ''' 从读取器当前位置原样捕获一条未知字段。
        ''' </summary>
        Public Function AddFrom(reader As ProtoReader, fieldNumber As Integer, wireType As WireType) As UnknownFieldSet
            If reader Is Nothing Then
                Throw New ArgumentNullException(NameOf(reader))
            End If

            Return Add(fieldNumber, wireType, reader.CaptureValueBytes(wireType))
        End Function

        ''' <summary>
        ''' 把全部未知字段原样写回消息。
        ''' </summary>
        Public Sub WriteTo(writer As ProtoWriter)
            If writer Is Nothing Then
                Throw New ArgumentNullException(NameOf(writer))
            End If

            For Each entry As Entry In _entries
                Call writer.WriteTag(entry.FieldNumber, entry.WireType)
                Call writer.WriteRawBytes(entry.ValueBytes)
            Next
        End Sub

        ''' <summary>
        ''' 清空全部未知字段。
        ''' </summary>
        Public Sub Clear()
            Call _entries.Clear()
        End Sub

        ''' <summary>
        ''' 枚举已保留的未知字段（用于诊断）。
        ''' </summary>
        Public Iterator Function Enumerate() As IEnumerable(Of (fieldNumber As Integer, wireType As WireType, size As Integer))
            For Each entry As Entry In _entries
                Yield (entry.FieldNumber, entry.WireType, entry.ValueBytes.Length)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"unknown fields: {_entries.Count}"
        End Function

    End Class

End Namespace
