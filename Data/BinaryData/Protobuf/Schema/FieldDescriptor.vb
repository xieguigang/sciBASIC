Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Schema\FieldDescriptor.vb
' /********************************************************************************/

Imports ProtoWireType = Microsoft.VisualBasic.Data.IO.Protobuf.Wire.WireType

Namespace Schema

    ''' <summary>
    ''' 字段的出现规则（对应 .proto 中的 optional / required / repeated）。
    ''' </summary>
    Public Enum FieldLabel

        ''' <summary>
        ''' 可选字段（proto3 默认语义，也是本库的默认值）。
        ''' </summary>
        [Optional] = 1

        ''' <summary>
        ''' 必填字段（proto3 已移除，这里保留用于兼容 proto2 的 .proto 文件）。
        ''' </summary>
        Required = 2

        ''' <summary>
        ''' 可重复字段，映射到 .NET 的 List(Of T) 或 T()。
        ''' </summary>
        Repeated = 3

    End Enum

    ''' <summary>
    ''' 字段的标量类型。编号与 protobuf 的 <c>FieldDescriptorProto.Type</c> 保持一致，
    ''' 另外追加了若干 .NET 扩展类型的编号（&gt;= 100）。
    ''' </summary>
    Public Enum FieldTypeKind

        [Double] = 1
        Float = 2
        Int64 = 3
        UInt64 = 4
        Int32 = 5
        Fixed64 = 6
        Fixed32 = 7
        Bool = 8
        [String] = 9

        ''' <summary>
        ''' proto2 的已废弃分组语法，本库不支持序列化。
        ''' </summary>
        Group = 10

        Message = 11
        Bytes = 12
        UInt32 = 13
        [Enum] = 14
        SFixed32 = 15
        SFixed64 = 16
        SInt32 = 17
        SInt64 = 18

        ''' <summary>
        ''' 扩展类型：.NET <see cref="Decimal"/> 以 16 字节 length-delimited 编码。
        ''' </summary>
        Decimal128 = 100

        ''' <summary>
        ''' 扩展类型：<see cref="DateTime"/> 以 Unix 毫秒（int64 varint）编码。
        ''' </summary>
        DateTimeUnixMilliseconds = 101

        ''' <summary>
        ''' 扩展类型：<see cref="DateTime"/> 以 .NET Ticks（int64 varint）编码。
        ''' </summary>
        DateTimeTicks = 102

        ''' <summary>
        ''' 扩展类型：<see cref="Guid"/> 以 16 字节 length-delimited 编码。
        ''' </summary>
        Guid128 = 103

    End Enum

    ''' <summary>
    ''' 描述 .proto 文件中的一个字段。
    ''' </summary>
    Public Class FieldDescriptor

        ''' <summary>
        ''' 字段名（.proto 中的原始名称，通常为 snake_case）。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 字段号。
        ''' </summary>
        Public Property Number As Integer

        ''' <summary>
        ''' 出现规则。
        ''' </summary>
        Public Property Label As FieldLabel = FieldLabel.[Optional]

        ''' <summary>
        ''' 标量类型。
        ''' </summary>
        Public Property FieldType As FieldTypeKind = FieldTypeKind.Int32

        ''' <summary>
        ''' .proto 中书写的类型名（可能是相对名、限定名或前导点号限定的全名）。
        ''' </summary>
        Public Property TypeName As String

        ''' <summary>
        ''' 解析后的类型全名（message / enum）；类型解析失败时为 Nothing。
        ''' </summary>
        Public Property ResolvedTypeFullName As String

        ''' <summary>
        ''' <c>[packed=true|false]</c> 的显式声明；Nothing 表示按类型自动判定。
        ''' </summary>
        Public Property Packed As Boolean?

        ''' <summary>
        ''' <c>[default=...]</c> 的原始文本。
        ''' </summary>
        Public Property DefaultValue As String

        ''' <summary>
        ''' 所属 oneof 的下标；-1 表示不属于任何 oneof。
        ''' </summary>
        Public Property OneofIndex As Integer = -1

        ''' <summary>
        ''' JSON 名称（proto3 的 json_name 选项）。
        ''' </summary>
        Public Property JsonName As String

        ''' <summary>
        ''' 是否为 <c>map&lt;K,V&gt;</c> 字段。
        ''' </summary>
        Public Property IsMap As Boolean

        ''' <summary>
        ''' map 的键类型（仅当 <see cref="IsMap"/> 为 True 时有效）。
        ''' </summary>
        Public Property MapKeyType As FieldTypeKind = FieldTypeKind.String

        ''' <summary>
        ''' map 的值类型（仅当 <see cref="IsMap"/> 为 True 时有效）。
        ''' </summary>
        Public Property MapValueType As FieldTypeKind = FieldTypeKind.Message

        ''' <summary>
        ''' map 值类型的原始类型名（值类型为 message / enum 时有效）。
        ''' </summary>
        Public Property MapValueTypeName As String

        ''' <summary>
        ''' map 值类型解析后的全名。
        ''' </summary>
        Public Property MapValueResolvedTypeFullName As String

        ''' <summary>
        ''' map 字段所对应的合成 entry 消息的全名。
        ''' </summary>
        Public Property MapEntryTypeName As String

        ''' <summary>
        ''' 字段所属的消息描述符。
        ''' </summary>
        Public Property Parent As MessageDescriptor

        ''' <summary>
        ''' 字段全名（消息全名 + "." + 字段名）。
        ''' </summary>
        Public ReadOnly Property FullName As String
            Get
                If Parent Is Nothing Then
                    Return Name
                Else
                    Return $"{Parent.FullName}.{Name}"
                End If
            End Get
        End Property

        ''' <summary>
        ''' 由 <see cref="FieldType"/> 推导的线格式类型。
        ''' </summary>
        Public ReadOnly Property WireType As ProtoWireType
            Get
                Return GetWireType(FieldType)
            End Get
        End Property

        ''' <summary>
        ''' 该字段是否允许 packed 编码（数值与枚举类型可以，string/bytes/message 不可以）。
        ''' </summary>
        Public ReadOnly Property IsPackable As Boolean
            Get
                Return IsPackableKind(FieldType)
            End Get
        End Property

        ''' <summary>
        ''' 综合 <see cref="Packed"/> 与字段类型得到的最终 packed 结论。
        ''' </summary>
        Public ReadOnly Property EffectivePacked As Boolean
            Get
                If Packed.HasValue Then
                    Return Packed.Value
                End If

                Return Label = FieldLabel.Repeated AndAlso IsPackable AndAlso Not IsMap
            End Get
        End Property

        ''' <summary>
        ''' 判断某个标量类型是否可以 packed 编码。
        ''' </summary>
        Public Shared Function IsPackableKind(kind As FieldTypeKind) As Boolean
            Select Case kind
                Case FieldTypeKind.[String], FieldTypeKind.Bytes, FieldTypeKind.Message, FieldTypeKind.Group
                    Return False

                Case Else
                    Return True
            End Select
        End Function

        ''' <summary>
        ''' 由标量类型推导线格式类型。
        ''' </summary>
        Public Shared Function GetWireType(kind As FieldTypeKind) As ProtoWireType
            Select Case kind
                Case FieldTypeKind.[Double], FieldTypeKind.Fixed64, FieldTypeKind.SFixed64
                    Return ProtoWireType.Fixed64

                Case FieldTypeKind.Float, FieldTypeKind.Fixed32, FieldTypeKind.SFixed32
                    Return ProtoWireType.Fixed32

                Case FieldTypeKind.[String], FieldTypeKind.Bytes, FieldTypeKind.Message
                    Return ProtoWireType.LengthDelimited

                Case Else
                    Return Wire.WireType.Varint
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"{If(Label = FieldLabel.Repeated, "repeated ", "")}{FieldType.ToString().ToLowerInvariant()} {Name} = {Number}"
        End Function

    End Class

End Namespace
