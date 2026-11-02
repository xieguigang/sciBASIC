Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf.test
'  Data\BinaryData\Protobuf\test\SerializationTests.vb
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.Protobuf
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Serialization
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Wire

''' <summary>
''' CLR 对象序列化测试：黄金字节、嵌套、repeated/packed、map、枚举、可空、未知字段、合并语义。
''' </summary>
Friend Module SerializationTests

    Friend Sub Run()
        Call GoldenVectors()
        Call DefaultValueElimination()
        Call RoundTripPerson()
        Call PackedAndUnpackedInterop()
        Call MapFields()
        Call EnumAndNullable()
        Call UnknownFieldHandling()
        Call MergeIntoSExisting()
        Call DepthGuard()
        Call LargePayload()
    End Sub

#Region "GOLDEN VECTORS"

    Private Sub GoldenVectors()
        Dim data As Byte() = ProtoSerializer.Serialize(New WireVector With {.VarintValue = 150})

        Call AssertBytesEqual(HexBytes("08 96 01"), data, "字段 1 varint 150")

        data = ProtoSerializer.Serialize(New WireVector With {.Text = "testing"})
        Call AssertBytesEqual(HexBytes("12 07 74 65 73 74 69 6E 67"), data, "字段 2 字符串 ""testing""")

        data = ProtoSerializer.Serialize(New WireVector With {
            .Numbers = New List(Of Integer) From {3, 270, 86942}
        })
        Call AssertBytesEqual(HexBytes("22 06 03 8E 02 9E A7 05"), data, "字段 4 packed [3, 270, 86942]")

        data = ProtoSerializer.Serialize(New WireVector With {.Extra = "x"})
        Call AssertBytesEqual(HexBytes("82 01 01 78"), data, "字段 16 字符串 ""x""")

        data = ProtoSerializer.Serialize(New WireVector With {.VarintValue = 1, .Text = "a"})
        Call AssertBytesEqual(HexBytes("08 01 12 01 61"), data, "字段按字段号升序写出")

        Dim copy As WireVector = ProtoSerializer.Deserialize(Of WireVector)(
            HexBytes("08 96 01 12 07 74 65 73 74 69 6E 67 22 06 03 8E 02 9E A7 05"))
        Call AssertEqual(150, copy.VarintValue, "黄金向量回读 varint")
        Call AssertEqual("testing", copy.Text, "黄金向量回读字符串")
        Call AssertEqual(3, copy.Numbers.Count, "黄金向量回读 packed 元素个数")
        Call AssertEqual(86942, copy.Numbers(2), "黄金向量回读 packed 最后一个元素")
    End Sub

    Private Sub DefaultValueElimination()
        Dim data As Byte() = ProtoSerializer.Serialize(New Person With {.Name = "a"})

        Call AssertBytesEqual(HexBytes("0A 01 61"), data, "默认值与 Nothing 字段被省略")

        Dim empty As Byte() = ProtoSerializer.Serialize(New PhoneNumber())

        Call AssertEqual(0, empty.Length, "全默认值消息序列化为 0 字节")

        Dim restored As PhoneNumber = ProtoSerializer.Deserialize(Of PhoneNumber)(Array.Empty(Of Byte)())
        Call AssertEqual("", If(restored.Number, ""), "空消息反序列化得到默认实例")
        Call AssertEqual(PhoneKind.Mobile, restored.Kind, "空消息反序列化得到默认枚举值")

        ' 关闭 SkipDefaults 后，0 值字段也会被写出
        Dim context As New SerializationContext With {.SkipDefaults = False}
        Dim old As SerializationContext = ProtoSerializer.Context

        Try
            ProtoSerializer.Context = context
            data = ProtoSerializer.Serialize(New PhoneNumber With {.Number = "", .Kind = PhoneKind.Mobile})
            Call AssertBytesEqual(HexBytes("0A 00 10 00"), data, "SkipDefaults=False 时写出默认值")
        Finally
            ProtoSerializer.Context = old
        End Try
    End Sub

#End Region

#Region "OBJECTS"

    Private Sub RoundTripPerson()
        Dim person As New Person With {
            .Name = "Ada",
            .Id = 7,
            .Email = "ada@example.com",
            .Tags = New List(Of String) From {"math", "code"},
            .Scores = New List(Of Integer) From {100, -5, 0},
            .Phones = New List(Of PhoneNumber) From {
                New PhoneNumber With {.Number = "1234", .Kind = PhoneKind.Home},
                New PhoneNumber With {.Number = "5678", .Kind = PhoneKind.Work}
            },
            .NickNames = New String() {"A", "Addy"}
        }

        Dim data As Byte() = ProtoSerializer.Serialize(person)
        Dim copy As Person = ProtoSerializer.Deserialize(Of Person)(data)

        Call AssertPersonsEqual(person, copy, "Person 深度往返")
        Call AssertEqual(person.NickNames.Length, copy.NickNames.Length, "数组字段往返元素个数")

        ' 字符串流入口
        Using stream As New MemoryStream()

            Call ProtoSerializer.Serialize(person, stream)
            Call AssertEqual(CLng(data.Length), stream.Length, "序列化到 Stream 的字节数与字节数组一致")

            stream.Position = 0

            Dim fromStream As Person = ProtoSerializer.Deserialize(Of Person)(stream)
            Call AssertEqual(person.Name, fromStream.Name, "从 Stream 反序列化")
        End Using

        ' 非泛型入口
        Dim boxed As Object = ProtoSerializer.Deserialize(GetType(Person), data)
        Call AssertTrue(TypeOf boxed Is Person, "非泛型反序列化返回正确类型")
        Call AssertEqual(person.Id, DirectCast(boxed, Person).Id, "非泛型反序列化字段值")

        ' 文件往返
        Dim path As String = IO.Path.Combine(TempDir(), "person.bin")

        Call ProtoSerializer.SerializeToFile(person, path)

        Dim fromFile As Person = ProtoSerializer.DeserializeFromFile(Of Person)(path)
        Call AssertEqual(person.Email, fromFile.Email, "文件往返")

        ' 扩展方法
        Dim viaExtension As Person = person.ToProtobuf().FromProtobuf(Of Person)()
        Call AssertEqual(person.Name, viaExtension.Name, "扩展方法往返")
    End Sub

    Private Shared Sub AssertPersonsEqual(expected As Person, actual As Person, message As String)
        Call AssertEqual(expected.Name, actual.Name, $"{message}：Name")
        Call AssertEqual(expected.Id, actual.Id, $"{message}：Id")
        Call AssertEqual(expected.Email, actual.Email, $"{message}：Email")
        Call AssertEqual(expected.Tags.Count, actual.Tags.Count, $"{message}：Tags 数量")

        For i As Integer = 0 To expected.Tags.Count - 1
            Call AssertEqual(expected.Tags(i), actual.Tags(i), $"{message}：Tags[{i}]")
        Next

        Call AssertEqual(expected.Scores.Count, actual.Scores.Count, $"{message}：Scores 数量")

        For i As Integer = 0 To expected.Scores.Count - 1
            Call AssertEqual(expected.Scores(i), actual.Scores(i), $"{message}：Scores[{i}]")
        Next

        Call AssertEqual(expected.Phones.Count, actual.Phones.Count, $"{message}：Phones 数量")

        For i As Integer = 0 To expected.Phones.Count - 1
            Call AssertEqual(expected.Phones(i).Number, actual.Phones(i).Number, $"{message}：Phones[{i}].Number")
            Call AssertEqual(expected.Phones(i).Kind, actual.Phones(i).Kind, $"{message}：Phones[{i}].Kind")
        Next
    End Sub

    Private Sub PackedAndUnpackedInterop()
        ' 声明为 packed 的字段，读取 unpacked 编码（每个元素独立 tag）
        Dim unpacked As Byte() = HexBytes("20 03 20 8E 02 20 9E A7 05")
        Dim fromUnpacked As PackedVector = ProtoSerializer.Deserialize(Of PackedVector)(unpacked)

        Call AssertEqual(3, fromUnpacked.Values.Count, "unpacked 数据读入 packed 字段的元素个数")
        Call AssertEqual(3, fromUnpacked.Values(0), "unpacked 数据第一个元素")
        Call AssertEqual(86942, fromUnpacked.Values(2), "unpacked 数据第三个元素")

        ' packed 编码再次写出
        Dim reserialized As Byte() = ProtoSerializer.Serialize(fromUnpacked)
        Call AssertBytesEqual(HexBytes("22 06 03 8E 02 9E A7 05"), reserialized, "unpacked 输入被规范化为 packed 输出")
    End Sub

    Private Sub MapFields()
        Dim profile As New OrderProfile With {
            .Attributes = New Dictionary(Of String, String) From {
                {"channel", "web"},
                {"coupon", "SPRING"}
            },
            .Quantities = New Dictionary(Of String, Integer) From {
                {"sku-1", 2},
                {"sku-2", 5}
            },
            .Status = OrderStatus.Shipped,
            .Score = 9.75R,
            .Discount = 12.5D,
            .CreatedAt = New DateTime(2026, 11, 2, 8, 30, 0, DateTimeKind.Utc),
            .Payload = New Byte() {1, 2, 3, 4, 5},
            .Owner = New Person With {.Name = "Grace", .Id = 42}
        }

        Dim data As Byte() = ProtoSerializer.Serialize(profile)
        Dim copy As OrderProfile = ProtoSerializer.Deserialize(Of OrderProfile)(data)

        Call AssertEqual(2, copy.Attributes.Count, "map<string,string> 元素个数")
        Call AssertEqual("web", copy.Attributes("channel"), "map<string,string> 取值")
        Call AssertEqual(5, copy.Quantities("sku-2"), "map<string,int32> 取值")
        Call AssertEqual(OrderStatus.Shipped, copy.Status, "枚举字段往返")
        Call AssertEqual(9.75R, CDbl(copy.Score), "Nullable(Of Double) 往返")
        Call AssertEqual(12.5D, CDec(copy.Discount), "Nullable(Of Decimal) 往返")
        Call AssertEqual(profile.CreatedAt, copy.CreatedAt, "DateTime（Unix 毫秒）往返")
        Call AssertBytesEqual(profile.Payload, copy.Payload, "bytes 字段往返")
        Call AssertEqual("Grace", copy.Owner.Name, "嵌套消息往返")
        Call AssertEqual(42, copy.Owner.Id, "嵌套消息字段往返")

        ' 空字典不产生任何字节
        Dim empty As Byte() = ProtoSerializer.Serialize(New OrderProfile())
        Call AssertEqual(0, empty.Length, "空 map 与默认值字段不产生字节")
    End Sub

    Private Sub EnumAndNullable()
        ' 未知枚举值按 varint 读取（前向兼容）
        Dim data As Byte() = HexBytes("10 63")
        Dim phone As PhoneNumber = ProtoSerializer.Deserialize(Of PhoneNumber)(data)

        Call AssertEqual(99, CInt(phone.Kind), "未知枚举值被接受")

        Dim profile As New OrderProfile With {.Status = CType(77, OrderStatus)}
        Dim copy As OrderProfile = ProtoSerializer.Deserialize(Of OrderProfile)(ProtoSerializer.Serialize(profile))

        Call AssertEqual(77, CInt(copy.Status), "未知枚举值往返")

        ' Nothing 表示字段缺失
        Dim restored As OrderProfile = ProtoSerializer.Deserialize(Of OrderProfile)(
            ProtoSerializer.Serialize(New OrderProfile()))

        Call AssertTrue(restored.Score Is Nothing, "缺失的 Nullable 字段保持 Nothing")
        Call AssertTrue(restored.Discount Is Nothing, "缺失的 Nullable(Of Decimal) 保持 Nothing")
        Call AssertTrue(restored.Owner Is Nothing, "缺失的嵌套消息保持 Nothing")
    End Sub

    Private Sub UnknownFieldHandling()
        ' 字段 1 = 42（已知），字段 3 = 7（未知）
        Dim data As Byte() = HexBytes("08 2A 18 07")
        Dim message As UnknownTolerantMessage = ProtoSerializer.Deserialize(Of UnknownTolerantMessage)(data)

        Call AssertEqual(42, message.Id, "已知字段正常读取")
        Call AssertEqual(1, message.UnknownFields.Count, "未知字段被保留")

        Dim reserialized As Byte() = ProtoSerializer.Serialize(message)
        Call AssertBytesEqual(HexBytes("08 2A 18 07"), reserialized, "未知字段被原样重放")

        Dim old As SerializationContext = ProtoSerializer.Context

        Try
            ProtoSerializer.Context = New SerializationContext With {
                .UnknownFieldBehavior = UnknownFieldBehavior.Skip
            }

            Dim skipped As UnknownTolerantMessage = ProtoSerializer.Deserialize(Of UnknownTolerantMessage)(data)
            Call AssertEqual(0, skipped.UnknownFields.Count, "Skip 模式不保留未知字段")
            Call AssertBytesEqual(HexBytes("08 2A"), ProtoSerializer.Serialize(skipped), "Skip 模式丢弃未知字段")
        Finally
            ProtoSerializer.Context = old
        End Try

        ' 未知的 length-delimited 字段也应被保留（字段 2，长度 3）
        Dim withBlob As Byte() = HexBytes("08 2A 12 03 41 42 43")
        Dim kept As UnknownTolerantMessage = ProtoSerializer.Deserialize(Of UnknownTolerantMessage)(withBlob)

        Call AssertBytesEqual(withBlob, ProtoSerializer.Serialize(kept), "length-delimited 未知字段原样重放")
    End Sub

    Private Sub MergeIntoSExisting()
        Dim person As New Person With {.Name = "Linus", .Id = 1}
        Dim patch As Byte() = ProtoSerializer.Serialize(New Person With {.Id = 2, .Email = "linus@example.com"})

        Call ProtoSerializer.Populate(person, patch)

        Call AssertEqual("Linus", person.Name, "合并语义：未出现在数据中的字段保持不变")
        Call AssertEqual(2, person.Id, "合并语义：标量字段被覆盖")
        Call AssertEqual("linus@example.com", person.Email, "合并语义：新字段被写入")
    End Sub

    Private Sub DepthGuard()
        Dim old As SerializationContext = ProtoSerializer.Context

        Try
            ProtoSerializer.Context = New SerializationContext With {.MaxDepth = 1}

            Dim deep As New OrderProfile With {
                .Owner = New Person With {
                    .Name = "deep",
                    .Phones = New List(Of PhoneNumber) From {New PhoneNumber With {.Number = "1"}}
                }
            }

            Call AssertThrows(Of InvalidOperationException)(
                Sub()
                    Call ProtoSerializer.Serialize(deep)
                End Sub,
                "超过 MaxDepth 时抛出 InvalidOperationException")
        Finally
            ProtoSerializer.Context = old
        End Try
    End Sub

    Private Sub LargePayload()
        Dim text As String = New String("A"c, 1024 * 1024)
        Dim data As Byte() = ProtoSerializer.Serialize(New Person With {.Name = text})

        ' tag(1) + varint 长度(3) + 1MB 载荷
        Call AssertEqual(text.Length + 4, data.Length, "1MB 字符串的字节长度")

        Dim copy As Person = ProtoSerializer.Deserialize(Of Person)(data)
        Call AssertEqual(text.Length, copy.Name.Length, "1MB 字符串往返长度")

        Dim utf8 As New UTF8Encoding(False)
        Call AssertTrue(utf8.GetByteCount(text) = utf8.GetByteCount(copy.Name), "1MB 字符串往返字节数一致")
    End Sub

#End Region

End Module
