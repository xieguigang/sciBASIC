Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.IO
Imports System.Text.Json.Nodes
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.DataStorage.Parquet

''' <summary>
''' 嵌套类型（LIST / MAP / LIST&lt;STRUCT&gt; / STRUCT 内含 LIST）的写入与读取测试。
''' </summary>
Friend Module NestedTests

    Friend Sub Run()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "nested.parquet")
        Dim df As DataFrame = Frames.MakeNested()

        Call pq.write_table(df, file)

        Dim df2 As DataFrame = pq.read_table(file)

        Call TestSupport.AssertEqual(5, df2.nsamples, "嵌套表行数一致")
        Call TestSupport.AssertEqual(4, df2.nfeatures, "嵌套表列数一致")
        Call TestSupport.AssertTrue(DescribeColumn(df2, "values").Contains("list"), "列表列读取为 JSON 文本列并记录类型描述")
        Call TestSupport.AssertTrue(df2("values").attributes IsNot Nothing, "列表列带有类型描述 attributes")

        Dim values As String() = DirectCast(df2("values").vector, String())
        Dim attrs As String() = DirectCast(df2("attrs").vector, String())
        Dim points As String() = DirectCast(df2("points").vector, String())
        Dim record As String() = DirectCast(df2("record").vector, String())

        Call TestSupport.AssertEqual("null", Normalize(values(3)), "空列表以 null 表示")

        Dim firstList As JsonArray = DirectCast(JsonNode.Parse(values(0)), JsonArray)

        Call TestSupport.AssertEqual(3, firstList.Count, "第一个列表元素数量正确")
        Call TestSupport.AssertEqual(3.0R, DoubleOf(firstList(2)), "第一个列表最后一个元素正确")
        Call TestSupport.AssertEqual(0, DirectCast(JsonNode.Parse(values(2)), JsonArray).Count, "空列表读取为空数组")

        Dim map As JsonObject = DirectCast(JsonNode.Parse(attrs(0)), JsonObject)

        Call TestSupport.AssertEqual(2, map.Count, "map 元素数量正确")
        Call TestSupport.AssertEqual(1L, LongOf(map("a")), "map 键 a 的值正确")
        Call TestSupport.AssertEqual(0, DirectCast(JsonNode.Parse(attrs(1)), JsonObject).Count, "空 map 读取为空对象")

        Dim structList As JsonArray = DirectCast(JsonNode.Parse(points(0)), JsonArray)

        Call TestSupport.AssertEqual(2, structList.Count, "list<struct> 元素数量正确")
        Call TestSupport.AssertEqual(2L, LongOf(DirectCast(structList(1), JsonObject)("x")), "list<struct> 中 x 字段正确")
        Call TestSupport.AssertEqual(2.5R, DoubleOf(DirectCast(structList(1), JsonObject)("y")), "list<struct> 中 y 字段正确")

        Dim recordNode As JsonObject = DirectCast(JsonNode.Parse(record(0)), JsonObject)

        Call TestSupport.AssertEqual(1L, LongOf(recordNode("a")), "struct 内标量字段正确")
        Call TestSupport.AssertEqual(2, DirectCast(recordNode("l"), JsonArray).Count, "struct 内 list 字段正确")
        Call TestSupport.AssertEqual(0, DirectCast(DirectCast(JsonNode.Parse(record(1)), JsonObject)("l"), JsonArray).Count, "struct 内空 list 字段正确")
    End Sub

    ''' <summary>
    ''' 读取 → 再次写入 → 再次读取，验证嵌套列的往返稳定性。
    ''' </summary>
    Friend Sub RunStableRoundtrip()
        Dim dir As String = TestSupport.TempDir()
        Dim first As String = System.IO.Path.Combine(dir, "nested-1.parquet")
        Dim second As String = System.IO.Path.Combine(dir, "nested-2.parquet")
        Dim df As DataFrame = Frames.MakeNested()

        Call pq.write_table(df, first)

        Dim df2 As DataFrame = pq.read_table(first)

        Call pq.write_table(df2, second)

        Dim df3 As DataFrame = pq.read_table(second)

        For Each name As String In df2.featureNames
            Call TestSupport.AssertArray(TestSupport.ColumnArray(df2, name), TestSupport.ColumnArray(df3, name), $"嵌套列 {name} 二次往返稳定")
        Next

        Call TestSupport.AssertEqual(df2.nsamples, df3.nsamples, "嵌套列二次往返行数一致")
    End Sub

    Friend Sub RunNestedSchema()
        Dim dir As String = TestSupport.TempDir()
        Dim file As String = System.IO.Path.Combine(dir, "nested-schema.parquet")

        Call pq.write_table(Frames.MakeNested(), file)

        Dim schema As Schema.ParquetSchema = pq.read_schema(file)
        Dim values As Schema.ParquetSchemaNode = schema.Root.Select("values")

        Call TestSupport.AssertTrue(values IsNot Nothing AndAlso values.IsListGroup, "values 列在 schema 中带有 LIST 注解")
        Call TestSupport.AssertEqual(1, values.Children.Count, "LIST 只有一个 repeated 包装节点")
        Call TestSupport.AssertTrue(values.Children(0).IsRepeated, "LIST 的包装节点为 REPEATED")
        Call TestSupport.AssertEqual(1, values.Children(0).Children.Count, "LIST 包装节点下面只有一个 element 节点")
        Call TestSupport.AssertEqual(Format.ParquetType.[DOUBLE], values.Children(0).Children(0).PhysicalType.Value, "LIST 元素物理类型为 DOUBLE")

        Dim attrs As Schema.ParquetSchemaNode = schema.Root.Select("attrs")

        Call TestSupport.AssertTrue(attrs IsNot Nothing AndAlso attrs.IsMapGroup, "attrs 列在 schema 中带有 MAP 注解")

        Dim record As Schema.ParquetSchemaNode = schema.Root.Select("record")

        Call TestSupport.AssertTrue(record IsNot Nothing AndAlso record.IsGroup AndAlso Not record.IsListGroup, "record 列在 schema 中是 struct group")
    End Sub

    Private Function Normalize(text As String) As String
        If String.IsNullOrEmpty(text) Then
            Return "null"
        End If

        Return text.Trim()
    End Function

    Private Function DoubleOf(node As JsonNode) As Double
        If node Is Nothing Then
            Return Double.NaN
        End If

        Return Double.Parse(node.ToJsonString(), Globalization.CultureInfo.InvariantCulture)
    End Function

    Private Function LongOf(node As JsonNode) As Long
        If node Is Nothing Then
            Return Long.MinValue
        End If

        Return Long.Parse(node.ToJsonString(), Globalization.CultureInfo.InvariantCulture)
    End Function

    Private Function DescribeColumn(df As DataFrame, name As String) As String
        If Not df.features.ContainsKey(name) Then
            Return "<missing>"
        End If

        Dim column As FeatureVector = df.features(name)

        If column.attributes Is Nothing OrElse Not column.attributes.ContainsKey("parquet.type") Then
            Return column.type.Name
        End If

        Return column.attributes("parquet.type")
    End Function

End Module
