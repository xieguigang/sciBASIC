Option Strict On
Option Explicit On
Option Infer On

' /********************************************************************************/
'  Microsoft.VisualBasic.Data.IO.Protobuf.test
'  Data\BinaryData\Protobuf\test\SchemaTests.vb
' /********************************************************************************/

Imports System
Imports System.IO
Imports Microsoft.VisualBasic.Data.IO.Protobuf.Schema

''' <summary>
''' .proto 解析测试：语法版本、package、import、嵌套类型、map、oneof、reserved、错误定位。
''' </summary>
Friend Module SchemaTests

    Friend Sub Run()
        Dim file As ProtoFile = ProtoFile.LoadText(SampleProto.Text, "sample.proto")

        Call FileLevel(file)
        Call MessageLevel(file)
        Call MapAndOneofLevel(file)
        Call EnumAndServiceLevel(file)
        Call LoadFromFile()
        Call ParseErrors()
    End Sub

#Region "CASES"

    Private Sub FileLevel(file As ProtoFile)
        Call AssertEqual(ProtoSyntax.Proto3, file.Syntax, "syntax = proto3")
        Call AssertEqual("demo.shop", file.Package, "package 解析")
        Call AssertEqual(1, file.ImportFiles.Count, "import 数量")
        Call AssertEqual("common/types.proto", file.ImportFiles(0), "import 文件名")
        Call AssertEqual("Demo.Shop", If(file.Options.ContainsKey("csharp_namespace"), file.Options("csharp_namespace"), ""),
                         "文件级 option 解析")
        Call AssertEqual(3, file.Messages.Count, "顶层消息数量")
        Call AssertEqual(1, file.Enums.Count, "顶层枚举数量")
        Call AssertEqual(1, file.Services.Count, "service 数量")
        Call AssertEqual(0, file.UnresolvedTypes.Count, "全部类型引用均可解析")
    End Sub

    Private Sub MessageLevel(file As ProtoFile)
        Dim customer As MessageDescriptor = file.FindMessageByFullName("demo.shop.Customer")

        Call AssertTrue(customer IsNot Nothing, "按全名查找消息")
        Call AssertEqual(6, customer.Fields.Count, "Customer 字段数量")
        Call AssertEqual(1, customer.NestedMessages.Count, "Customer 嵌套消息数量")
        Call AssertEqual(1, customer.NestedEnums.Count, "Customer 嵌套枚举数量")
        Call AssertEqual(2, customer.ReservedRanges.Count, "reserved 声明数量")

        Call AssertEqual("name", customer.Fields(0).Name, "字段顺序保持声明顺序")
        Call AssertEqual(FieldTypeKind.[String], customer.FindFieldByNumber(1).FieldType, "string 字段类型")
        Call AssertEqual(FieldTypeKind.Int32, customer.FindFieldByNumber(2).FieldType, "int32 字段类型")

        Dim tags As FieldDescriptor = customer.FindField("tags")
        Call AssertEqual(FieldLabel.Repeated, tags.Label, "repeated 标记")
        Call AssertEqual(FieldTypeKind.[String], tags.FieldType, "repeated string 元素类型")
        Call AssertFalse(tags.EffectivePacked, "repeated string 不 packed")

        Dim scores As FieldDescriptor = customer.FindField("scores")
        Call AssertTrue(scores.Packed.HasValue, "显式 packed 选项被记录")
        Call AssertTrue(scores.EffectivePacked, "repeated int32 默认 packed")

        Dim phones As FieldDescriptor = customer.FindField("phones")
        Call AssertEqual(FieldTypeKind.Message, phones.FieldType, "repeated 消息字段类型")
        Call AssertEqual("demo.shop.Customer.PhoneNumber", phones.ResolvedTypeFullName, "嵌套消息类型解析")
        Call AssertEqual("demo.shop.Customer.PhoneNumber", phones.Parent.FullName & ".PhoneNumber", "Parent 回填")

        Dim phoneNumber As MessageDescriptor = file.FindMessageByFullName("demo.shop.Customer.PhoneNumber")
        Call AssertTrue(phoneNumber IsNot Nothing, "嵌套消息可被全名查找")
        Call AssertEqual(FieldTypeKind.[Enum], phoneNumber.FindField("kind").FieldType, "嵌套枚举字段类型")
        Call AssertEqual("demo.shop.Customer.PhoneKind", phoneNumber.FindField("kind").ResolvedTypeFullName, "嵌套枚举类型解析")

        Dim keywordSample As MessageDescriptor = file.FindMessageByFullName("demo.shop.KeywordSample")
        Call AssertEqual(3, keywordSample.Fields.Count, "关键字样例消息字段数量")
        Call AssertEqual("class", keywordSample.Fields(0).Name, "字段名保持 .proto 原文")
    End Sub

    Private Sub MapAndOneofLevel(file As ProtoFile)
        Dim order As MessageDescriptor = file.FindMessageByFullName("demo.shop.Order")

        Call AssertTrue(order IsNot Nothing, "Order 消息存在")
        Call AssertEqual(3, order.NestedMessages.Count, "Order 嵌套消息数量（含 map entry）")
        Call AssertEqual("demo.shop.Order.Item", order.FindNestedMessage("Item").FullName, "普通嵌套消息全名")

        Dim attributes As FieldDescriptor = order.FindField("attributes")
        Call AssertTrue(attributes.IsMap, "map 字段标记")
        Call AssertEqual(FieldTypeKind.[String], attributes.MapKeyType, "map 键类型")
        Call AssertEqual(FieldTypeKind.[String], attributes.MapValueType, "map 值类型")
        Call AssertEqual("demo.shop.Order.AttributesEntry", attributes.MapEntryTypeName, "map entry 全名")
        Call AssertEqual(FieldLabel.Repeated, attributes.Label, "map 字段按 repeated 处理")

        Dim quantities As FieldDescriptor = order.FindField("quantities")
        Call AssertTrue(quantities.IsMap, "map<string,int32> 标记")
        Call AssertEqual(FieldTypeKind.Int32, quantities.MapValueType, "map<string,int32> 值类型")

        Dim entry As MessageDescriptor = file.FindMessageByFullName("demo.shop.Order.QuantitiesEntry")
        Call AssertTrue(entry IsNot Nothing, "map entry 消息已合成")
        Call AssertTrue(entry.MapEntry, "map entry 标记")
        Call AssertEqual(1, entry.FindField("key").Number, "map entry key 字段号")
        Call AssertEqual(2, entry.FindField("value").Number, "map entry value 字段号")

        Call AssertEqual(1, order.Oneofs.Count, "oneof 分组数量")
        Call AssertEqual("payment", order.Oneofs(0), "oneof 名称")
        Call AssertEqual(0, order.FindField("card_number").OneofIndex, "oneof 字段归属")
        Call AssertEqual(0, order.FindField("wallet_id").OneofIndex, "oneof 第二个字段归属")
        Call AssertEqual(-1, order.FindField("total").OneofIndex, "非 oneof 字段标记为 -1")
    End Sub

    Private Sub EnumAndServiceLevel(file As ProtoFile)
        Dim status As EnumDescriptor = file.FindEnumByFullName("demo.shop.OrderStatus")

        Call AssertTrue(status IsNot Nothing, "按全名查找枚举")
        Call AssertEqual(4, status.Values.Count, "枚举取值数量")
        Call AssertEqual("STATUS_UNKNOWN", status.Values(0).Name, "枚举取值名称")
        Call AssertEqual(0, status.Values(0).Number, "枚举取值编号")
        Call AssertEqual(9, status.FindValue("STATUS_CLOSED").Number, "按名称查找枚举取值")
        Call AssertEqual("STATUS_PAID", status.FindValue(1).Name, "按编号查找枚举取值")

        Dim service As ServiceDescriptor = file.Services(0)

        Call AssertEqual("OrderService", service.Name, "service 名称")
        Call AssertEqual("demo.shop.OrderService", service.FullName, "service 全名")
        Call AssertEqual(2, service.Methods.Count, "rpc 方法数量")
        Call AssertEqual("GetOrder", service.Methods(0).Name, "rpc 方法名")
        Call AssertEqual("Order", service.Methods(0).InputType, "rpc 请求类型")
        Call AssertFalse(service.Methods(0).ServerStreaming, "一元 rpc")
        Call AssertTrue(service.Methods(1).ServerStreaming, "服务端流 rpc")
    End Sub

    Private Sub LoadFromFile()
        Dim dir As String = TempDir()
        Dim protoPath As String = Path.Combine(dir, "sample.proto")

        Call IO.File.WriteAllText(protoPath, SampleProto.Text)

        Dim parsed As ProtoFile = ProtoFile.Load(protoPath)

        Call AssertEqual("sample.proto", parsed.SourceFile, "从文件解析时记录文件名")
        Call AssertEqual(3, parsed.Messages.Count, "从文件解析的消息数量")
    End Sub

    Private Sub ParseErrors()
        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("message A { int32 x = 1 }"),
            "缺少分号时抛出 ProtoParseException")

        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("message A { int32 x = 1;"),
            "花括号未闭合时抛出 ProtoParseException")

        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("message A { int32 x = 0; }"),
            "字段号 0 非法")

        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("message A { int32 x = 19000; }"),
            "字段号落在保留区间时非法")

        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("message A { int32 x = 1; } @@@"),
            "无法识别的字符抛出 ProtoParseException")

        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("message A { map<float, string> bad = 1; }"),
            "map 键类型非法")

        Call AssertThrows(Of ProtoParseException)(
            Sub() ProtoFile.LoadText("/* 未闭合的注释"),
            "块注释未闭合")

        Call AssertThrows(Of FileNotFoundException)(
            Sub() ProtoFile.Load(Path.Combine(TempDir(), "not-exists.proto")),
            "文件不存在时抛出 FileNotFoundException")
    End Sub

#End Region

End Module
