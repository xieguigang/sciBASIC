Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Serialization\ProtoDataFormat.vb
' /********************************************************************************/

Namespace Serialization

    ''' <summary>
    ''' 字段数值的编码方式，用于在 protobuf 原生的若干等价类型之间做选择，
    ''' 或者指定 .NET 扩展类型（decimal / DateTime / Guid）的编码方案。
    ''' </summary>
    Public Enum ProtoDataFormat

        ''' <summary>
        ''' 按 CLR 类型自动推导（默认行为）。
        ''' </summary>
        Auto = 0

        ''' <summary>
        ''' 有符号整数采用 zigzag 编码，对应 protobuf 的 sint32 / sint64。
        ''' </summary>
        ZigZag = 1

        ''' <summary>
        ''' 4 字节小端定长，对应 protobuf 的 fixed32 / sfixed32。
        ''' </summary>
        Fixed32 = 2

        ''' <summary>
        ''' 8 字节小端定长，对应 protobuf 的 fixed64 / sfixed64。
        ''' </summary>
        Fixed64 = 3

        ''' <summary>
        ''' .NET <see cref="Decimal"/> 以 16 字节（4 个 Int32 位段）length-delimited 编码。
        ''' 这是本库的扩展类型，不是 protobuf 标准类型。
        ''' </summary>
        Decimal128 = 4

        ''' <summary>
        ''' <see cref="DateTime"/> 以 Unix 毫秒（int64 varint）编码。
        ''' 这是本库的扩展类型，不是 protobuf 标准类型。
        ''' </summary>
        DateTimeUnixMilliseconds = 5

        ''' <summary>
        ''' <see cref="DateTime"/> 以 .NET Ticks（int64 varint）编码。
        ''' 这是本库的扩展类型，不是 protobuf 标准类型。
        ''' </summary>
        DateTimeTicks = 6

        ''' <summary>
        ''' <see cref="Guid"/> 以 16 字节 length-delimited 编码。
        ''' 这是本库的扩展类型，不是 protobuf 标准类型。
        ''' </summary>
        Guid128 = 7

    End Enum

End Namespace
