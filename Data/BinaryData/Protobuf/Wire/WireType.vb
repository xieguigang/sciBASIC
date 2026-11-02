Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf
'  Data\BinaryData\Protobuf\Wire\WireType.vb
' /********************************************************************************/

''' <summary>
''' protobuf 的线格式类型（wire type），对应 tag 的最低 3 位。
''' </summary>
''' <remarks>
''' 参考 <see href="https://protobuf.dev/programming-guides/encoding/"/>。
''' 其中的 <see cref="StartGroup"/> 与 <see cref="EndGroup"/> 属于 proto2 的已废弃分组语法，
''' 本库不支持，读取到时会抛出 <see cref="NotSupportedException"/>。
''' </remarks>
Public Enum WireType As Byte

    ''' <summary>
    ''' 变长整数编码（int32/int64/uint32/uint64/sint32/sint64/bool/enum）。
    ''' </summary>
    Varint = 0

    ''' <summary>
    ''' 固定 8 字节（fixed64/sfixed64/double）。
    ''' </summary>
    Fixed64 = 1

    ''' <summary>
    ''' 长度前缀 + 载荷（string/bytes/嵌套消息/packed 数组）。
    ''' </summary>
    LengthDelimited = 2

    ''' <summary>
    ''' 已废弃的分组起始标记，本库不支持。
    ''' </summary>
    StartGroup = 3

    ''' <summary>
    ''' 已废弃的分组结束标记，本库不支持。
    ''' </summary>
    EndGroup = 4

    ''' <summary>
    ''' 固定 4 字节（fixed32/sfixed32/float）。
    ''' </summary>
    Fixed32 = 5

End Enum
