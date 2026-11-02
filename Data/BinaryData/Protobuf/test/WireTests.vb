Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf.test
'  Data\BinaryData\Protobuf\test\WireTests.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

''' <summary>
''' Wire 层测试：varint / zigzag / 定长 / tag / 长度前缀 / 边界保护。
''' </summary>
Friend Module WireTests

    Friend Sub Run()
        Call VarintVectors()
        Call ZigZagVectors()
        Call TagVectors()
        Call FixedWidthVectors()
        Call LengthDelimitedVectors()
        Call ReaderGuards()
        Call LimitStack()
        Call SkipFields()
        Call HexHelper()
    End Sub

#Region "VARINT"

    Private Sub VarintVectors()
        Call AssertBytesEqual(HexBytes("00"), Write(Sub(w) w.WriteVarint(0UL)), "varint 0")
        Call AssertBytesEqual(HexBytes("01"), Write(Sub(w) w.WriteVarint(1UL)), "varint 1")
        Call AssertBytesEqual(HexBytes("7F"), Write(Sub(w) w.WriteVarint(127UL)), "varint 127")
        Call AssertBytesEqual(HexBytes("80 01"), Write(Sub(w) w.WriteVarint(128UL)), "varint 128")
        Call AssertBytesEqual(HexBytes("AC 02"), Write(Sub(w) w.WriteVarint(300UL)), "varint 300")
        Call AssertBytesEqual(HexBytes("FF FF FF FF 0F"), Write(Sub(w) w.WriteVarint(4294967295UL)), "varint uint32 最大值")
        Call AssertBytesEqual(HexBytes("FF FF FF FF FF FF FF FF FF 01"),
                              Write(Sub(w) w.WriteVarint(ULong.MaxValue)), "varint uint64 最大值")

        ' 负 int32 必须做 64 位符号扩展（10 字节）
        Call AssertBytesEqual(HexBytes("FF FF FF FF FF FF FF FF FF 01"),
                              Write(Sub(w) w.WriteInt32(-1)), "int32 -1 符号扩展")
        Call AssertBytesEqual(HexBytes("FF FF FF FF FF FF FF FF FF 01"),
                              Write(Sub(w) w.WriteInt64(-1L)), "int64 -1")

        Call AssertEqual(1, ProtoWireFormat.GetVarintSize(0UL), "varint size 0")
        Call AssertEqual(1, ProtoWireFormat.GetVarintSize(127UL), "varint size 127")
        Call AssertEqual(2, ProtoWireFormat.GetVarintSize(128UL), "varint size 128")
        Call AssertEqual(10, ProtoWireFormat.GetVarintSize(ULong.MaxValue), "varint size 最大值")

        ' 往返
        Dim samples As ULong() = {0UL, 1UL, 127UL, 128UL, 300UL, 4294967295UL, ULong.MaxValue}
        Dim buffer As Byte() = Write(
            Sub(w)
                For Each value As ULong In samples
                    Call w.WriteVarint(value)
                Next
            End Sub)

        Using reader As New ProtoReader(New MemoryStream(buffer))

            For Each expected As ULong In samples
                Call AssertEqual(expected, reader.ReadVarint(), $"varint 往返 {expected}")
            Next

            Call AssertTrue(reader.EndOfStream, "varint 往返后已到流末尾")
        End Using
    End Sub

    Private Sub ZigZagVectors()
        Call AssertEqual(0UI, ProtoWireFormat.ZigZagEncode32(0), "zigzag 0")
        Call AssertEqual(1UI, ProtoWireFormat.ZigZagEncode32(-1), "zigzag -1")
        Call AssertEqual(2UI, ProtoWireFormat.ZigZagEncode32(1), "zigzag 1")
        Call AssertEqual(3UI, ProtoWireFormat.ZigZagEncode32(-2), "zigzag -2")
        Call AssertEqual(4294967295UI, ProtoWireFormat.ZigZagEncode32(Integer.MinValue), "zigzag int32 最小值")

        Call AssertEqual(0, ProtoWireFormat.ZigZagDecode32(0UI), "zigzag 解码 0")
        Call AssertEqual(-1, ProtoWireFormat.ZigZagDecode32(1UI), "zigzag 解码 -1")
        Call AssertEqual(1, ProtoWireFormat.ZigZagDecode32(2UI), "zigzag 解码 1")
        Call AssertEqual(Integer.MinValue, ProtoWireFormat.ZigZagDecode32(4294967295UI), "zigzag 解码 int32 最小值")

        Call AssertEqual(1UL, ProtoWireFormat.ZigZagEncode64(-1L), "zigzag64 -1")
        Call AssertEqual(-1L, ProtoWireFormat.ZigZagDecode64(1UL), "zigzag64 解码 -1")
        Call AssertEqual(Long.MinValue, ProtoWireFormat.ZigZagDecode64(ProtoWireFormat.ZigZagEncode64(Long.MinValue)),
                         "zigzag64 往返 int64 最小值")

        Call AssertBytesEqual(HexBytes("01"), Write(Sub(w) w.WriteSInt32(-1)), "sint32 -1")
        Call AssertBytesEqual(HexBytes("02"), Write(Sub(w) w.WriteSInt32(1)), "sint32 1")
        Call AssertBytesEqual(HexBytes("FF FF FF FF 0F"),
                              Write(Sub(w) w.WriteSInt32(Integer.MinValue)), "sint32 最小值")
    End Sub

#End Region

#Region "TAG / FIXED"

    Private Sub TagVectors()
        Call AssertBytesEqual(HexBytes("08"), Write(Sub(w) w.WriteTag(1, WireType.Varint)), "tag field=1 varint")
        Call AssertBytesEqual(HexBytes("12"), Write(Sub(w) w.WriteTag(2, WireType.LengthDelimited)), "tag field=2 length-delimited")
        Call AssertBytesEqual(HexBytes("0D"), Write(Sub(w) w.WriteTag(1, WireType.Fixed32)), "tag field=1 fixed32")
        Call AssertBytesEqual(HexBytes("09"), Write(Sub(w) w.WriteTag(1, WireType.Fixed64)), "tag field=1 fixed64")
        Call AssertBytesEqual(HexBytes("82 01"), Write(Sub(w) w.WriteTag(16, WireType.LengthDelimited)), "tag field=16 length-delimited")

        Call AssertEqual(1, ProtoWireFormat.GetFieldNumber(ProtoWireFormat.MakeTag(1, WireType.Varint)), "tag 解析字段号")
        Call AssertTrue(ProtoWireFormat.GetWireType(ProtoWireFormat.MakeTag(16, WireType.LengthDelimited)) = WireType.LengthDelimited,
                        "tag 解析 wire type")
        Call AssertFalse(ProtoWireFormat.IsValidFieldNumber(0), "字段号 0 非法")
        Call AssertFalse(ProtoWireFormat.IsValidFieldNumber(19000), "字段号 19000 落在保留区间")
        Call AssertTrue(ProtoWireFormat.IsValidFieldNumber(19999 + 1), "字段号 20000 合法")
        Call AssertFalse(ProtoWireFormat.IsValidFieldNumber(ProtoWireFormat.MaxFieldNumber + 1), "字段号超出上限")
    End Sub

    Private Sub FixedWidthVectors()
        Call AssertBytesEqual(HexBytes("0D 00 00 80 3F"),
                              Write(Sub(w)
                                        w.WriteTag(1, WireType.Fixed32)
                                        w.WriteSingle(1.0F)
                                    End Sub),
                              "fixed32 1.0f")

        Call AssertBytesEqual(HexBytes("0D 01 00 00 00"),
                              Write(Sub(w)
                                        w.WriteTag(1, WireType.Fixed32)
                                        w.WriteFixed32(1UI)
                                    End Sub),
                              "fixed32 小端无符号 1")

        Call AssertBytesEqual(HexBytes("09 00 00 00 00 00 00 F0 3F"),
                              Write(Sub(w)
                                        w.WriteTag(1, WireType.Fixed64)
                                        w.WriteDouble(1.0)
                                    End Sub),
                              "fixed64 1.0")

        Call AssertBytesEqual(HexBytes("09 FF FF FF FF FF FF FF FF"),
                              Write(Sub(w)
                                        w.WriteTag(1, WireType.Fixed64)
                                        w.WriteSFixed64(-1L)
                                    End Sub),
                              "sfixed64 -1")

        Dim buffer As Byte() = Write(
            Sub(w)
                w.WriteFixed32(4294967295UI)
                w.WriteFixed64(ULong.MaxValue)
                w.WriteDouble(-1.5)
                w.WriteSingle(0.5F)
            End Sub)

        Using reader As New ProtoReader(New MemoryStream(buffer))
            Call AssertEqual(UInteger.MaxValue, reader.ReadFixed32(), "fixed32 往返")
            Call AssertEqual(ULong.MaxValue, reader.ReadFixed64(), "fixed64 往返")
            Call AssertEqual(-1.5, reader.ReadDouble(), "double 往返")
            Call AssertEqual(0.5F, reader.ReadSingle(), "single 往返")
        End Using
    End Sub

    Private Sub LengthDelimitedVectors()
        Call AssertBytesEqual(HexBytes("07 74 65 73 74 69 6E 67"),
                              Write(Sub(w) w.WriteString("testing")), "string ""testing""")

        Call AssertBytesEqual(HexBytes("00"), Write(Sub(w) w.WriteString("")), "空字符串")

        Call AssertBytesEqual(HexBytes("03 01 02 03"),
                              Write(Sub(w) w.WriteBytes(New Byte() {1, 2, 3})), "bytes 1,2,3")

        Dim text As String = "中文 protobuf ✓"
        Dim encoded As Byte() = Write(Sub(w) w.WriteString(text))

        Using reader As New ProtoReader(New MemoryStream(encoded))
            Call AssertEqual(text, reader.ReadString(), "UTF-8 字符串往返")
        End Using

        ' 嵌套消息
        Dim nested As Byte() = Write(
            Sub(w)
                w.WriteMessage(2,
                    Sub(child)
                        Call child.WriteTag(1, WireType.Varint)
                        Call child.WriteInt32(42)
                    End Sub)
            End Sub)

        Call AssertBytesEqual(HexBytes("12 02 08 2A"), nested, "嵌套消息 tag+长度+载荷")
    End Sub

#End Region

#Region "GUARDS"

    Private Sub ReaderGuards()
        Call AssertThrows(Of WireFormatException)(
            Sub()
                Using reader As New ProtoReader(New MemoryStream(HexBytes("80")))
                    Call reader.ReadVarint()
                End Using
            End Sub,
            "varint 被截断时抛出 WireFormatException")

        Call AssertThrows(Of WireFormatException)(
            Sub()
                Using reader As New ProtoReader(New MemoryStream(HexBytes("FF FF FF FF 0F")))
                    Call reader.ReadLengthPrefix()
                End Using
            End Sub,
            "长度前缀超出数据流时抛出 WireFormatException")

        Call AssertThrows(Of WireFormatException)(
            Sub()
                Using reader As New ProtoReader(New MemoryStream(HexBytes("0A 05 41")))
                    Call reader.ReadString()
                End Using
            End Sub,
            "长度前缀大于剩余字节时抛出 WireFormatException")

        Call AssertThrows(Of NotSupportedException)(
            Sub()
                Using reader As New ProtoReader(New MemoryStream(HexBytes("0B")))
                    Dim fieldNumber As Integer
                    Dim wireType As WireType

                    Call reader.ReadTag(fieldNumber, wireType)
                End Using
            End Sub,
            "分组语法（wire type=3）抛出 NotSupportedException")

        Using reader As New ProtoReader(New MemoryStream(Array.Empty(Of Byte)()))
            Dim fieldNumber As Integer
            Dim wireType As WireType

            Call AssertFalse(reader.ReadTag(fieldNumber, wireType), "空数据流没有 tag")
        End Using
    End Sub

    Private Sub LimitStack()
        ' 子消息 [08 2A][10 07] 长 4 字节，只读取第一个字段后弹出边界应跳过剩余字节
        Dim payload As Byte() = HexBytes("08 2A 10 07")

        Using reader As New ProtoReader(New MemoryStream(payload))
            Call reader.PushLimit(4)

            Dim fieldNumber As Integer
            Dim wireType As WireType

            Call AssertTrue(reader.ReadTag(fieldNumber, wireType), "limit 内可以读取 tag")
            Call AssertEqual(1, fieldNumber, "limit 内字段号")
            Call AssertEqual(42, reader.ReadInt32(), "limit 内读取值")

            Call reader.PopLimit()
            Call AssertEqual(4L, reader.Position, "PopLimit 跳过子消息剩余字节")
        End Using

        Call AssertThrows(Of WireFormatException)(
            Sub()
                Using reader As New ProtoReader(New MemoryStream(HexBytes("08 2A")))
                    Call reader.PushLimit(10)
                End Using
            End Sub,
            "PushLimit 超出可用字节时抛出 WireFormatException")
    End Sub

    Private Sub SkipFields()
        Call AssertSkip(HexBytes("08 96 01"), WireType.Varint, "跳过 varint")
        Call AssertSkip(HexBytes("01 02 03 04 05 06 07 08"), WireType.Fixed64, "跳过 fixed64")
        Call AssertSkip(HexBytes("01 02 03 04"), WireType.Fixed32, "跳过 fixed32")
        Call AssertSkip(HexBytes("03 41 42 43"), WireType.LengthDelimited, "跳过 length-delimited")
    End Sub

    Private Sub AssertSkip(payload As Byte(), wireType As WireType, message As String)
        Using reader As New ProtoReader(New MemoryStream(payload))
            Call reader.SkipField(wireType)
            Call AssertTrue(reader.EndOfStream, message)
        End Using
    End Sub

    Private Sub HexHelper()
        Call AssertBytesEqual(New Byte() {&HA, 1, &H61}, HexBytes("0A 01 61"), "十六进制辅助函数")
        Call AssertEqual("0A 01 61", ToHex(New Byte() {&HA, 1, &H61}), "十六进制格式化")
    End Sub

#End Region

#Region "HELPERS"

    Private Function Write(action As Action(Of ProtoWriter)) As Byte()
        Using writer As New ProtoWriter()
            Call action(writer)
            Call writer.Flush()

            Return writer.ToArray()
        End Using
    End Function

#End Region

End Module
