Option Strict On
Option Explicit On
Option Infer On

Imports System.Text
Imports System.Text.Json.Nodes

Namespace Nested

    ''' <summary>
    ''' 嵌套列写入端的 Dremel shredding 实现：把每一行的 JSON 文本按照 schema 树
    ''' 拆分成叶子列的 <c>(repetition_level, definition_level, value)</c> 三元流。
    ''' </summary>
    Public Module JsonShredder

        ''' <summary>
        ''' <c>FeatureVector.attributes</c> 中保存嵌套类型描述的键名。
        ''' </summary>
        Public Const NestedTypeAttribute As String = "parquet.type"

        ''' <summary>
        ''' 将嵌套列在指定行区间内的 JSON 文本拆分为叶子列数据。
        ''' </summary>
        Public Function ShredRange(plan As ColumnPlan, rowStart As Integer, rowCount As Integer) As List(Of Columns.DataColumn)
            Dim buffers As New Dictionary(Of Schema.ParquetSchemaNode, Columns.ColumnBuffer)

            For Each leaf As Schema.ParquetSchemaNode In plan.Node.Leaves()
                buffers(leaf) = New Columns.ColumnBuffer(leaf)
            Next

            For i As Integer = 0 To rowCount - 1
                Dim row As Integer = rowStart + i
                Dim json As String = If(plan.JsonValues Is Nothing, Nothing, plan.JsonValues(row))
                Dim document As JsonNode = Nothing

                If Not String.IsNullOrWhiteSpace(json) Then
                    Try
                        document = JsonNode.Parse(json)
                    Catch ex As Exception
                        Throw New ParquetException($"invalid json text of the nested column '{plan.Name}' at row {row}: {ex.Message}", ex)
                    End Try
                End If

                Call ShredNode(plan.Node, document, 0, 0, buffers)
            Next

            Dim result As New List(Of Columns.DataColumn)

            For Each leaf As Schema.ParquetSchemaNode In plan.Node.Leaves()
                Call result.Add(buffers(leaf).ToDataColumn())
            Next

            Return result
        End Function

        ''' <summary>
        ''' 将单个 JSON 数值按照 schema 节点写入各个叶子列的缓冲区。
        ''' </summary>
        ''' <param name="node">当前 schema 节点</param>
        ''' <param name="value">当前 JSON 数值</param>
        ''' <param name="parentDef">父节点的定义级别</param>
        ''' <param name="rep">当前的重复级别</param>
        Public Sub ShredNode(node As Schema.ParquetSchemaNode,
                             value As JsonNode,
                             parentDef As Integer,
                             rep As Integer,
                             buffers As Dictionary(Of Schema.ParquetSchemaNode, Columns.ColumnBuffer))

            If node.IsLeaf Then
                Dim buffer As Columns.ColumnBuffer = buffers(node)

                If value Is Nothing Then
                    buffer.Add(rep, parentDef, Nothing)
                Else
                    buffer.Add(rep, node.DefinitionLevel, ToClrValue(node, value))
                End If

                Return
            End If

            If node.IsListGroup Then
                Dim array As JsonArray = TryCast(value, JsonArray)

                If value Is Nothing Then
                    Call EmitNull(node, rep, parentDef, buffers)
                    Return
                End If
                If array Is Nothing Then
                    Throw New ParquetException($"the column '{node.Name}' is declared as LIST but the json value is not an array!")
                End If
                If array.Count = 0 Then
                    Call EmitNull(node, rep, node.DefinitionLevel, buffers)
                    Return
                End If

                Dim middle As Schema.ParquetSchemaNode = node.Children(0)
                Dim element As Schema.ParquetSchemaNode = middle.Children(0)

                For i As Integer = 0 To array.Count - 1
                    Dim elementRep As Integer = If(i = 0, rep, middle.RepetitionLevel)

                    Call ShredNode(element, array(i), middle.DefinitionLevel, elementRep, buffers)
                Next

                Return
            End If

            If node.IsMapGroup Then
                Dim map As JsonObject = TryCast(value, JsonObject)

                If value Is Nothing Then
                    Call EmitNull(node, rep, parentDef, buffers)
                    Return
                End If
                If map Is Nothing Then
                    Throw New ParquetException($"the column '{node.Name}' is declared as MAP but the json value is not an object!")
                End If
                If map.Count = 0 Then
                    Call EmitNull(node, rep, node.DefinitionLevel, buffers)
                    Return
                End If

                Dim keyValue As Schema.ParquetSchemaNode = node.Children(0)
                Dim keyNode As Schema.ParquetSchemaNode = keyValue.Children(0)
                Dim valueNode As Schema.ParquetSchemaNode = If(keyValue.Children.Count > 1, keyValue.Children(1), Nothing)
                Dim index As Integer = 0

                For Each pair As KeyValuePair(Of String, JsonNode) In map
                    Dim elementRep As Integer = If(index = 0, rep, keyValue.RepetitionLevel)

                    Call ShredNode(keyNode, JsonValue.Create(pair.Key), keyValue.DefinitionLevel, elementRep, buffers)

                    If valueNode IsNot Nothing Then
                        Call ShredNode(valueNode, pair.Value, keyValue.DefinitionLevel, elementRep, buffers)
                    End If

                    index += 1
                Next

                Return
            End If

            If node.IsRepeated Then
                Dim array As JsonArray = TryCast(value, JsonArray)

                If value Is Nothing Then
                    Call EmitNull(node, rep, parentDef, buffers)
                    Return
                End If
                If array Is Nothing Then
                    Throw New ParquetException($"the repeated field '{node.Name}' requires a json array value!")
                End If
                If array.Count = 0 Then
                    Call EmitNull(node, rep, parentDef, buffers)
                    Return
                End If

                For i As Integer = 0 To array.Count - 1
                    Dim elementRep As Integer = If(i = 0, rep, node.RepetitionLevel)

                    Call ShredStructChildren(node, array(i), node.DefinitionLevel, elementRep, buffers)
                Next

                Return
            End If

            If node.IsOptional AndAlso value Is Nothing Then
                Call EmitNull(node, rep, parentDef, buffers)
                Return
            End If

            Call ShredStructChildren(node, value, node.DefinitionLevel, rep, buffers)
        End Sub

        Private Sub ShredStructChildren(node As Schema.ParquetSchemaNode,
                                        value As JsonNode,
                                        nodeDef As Integer,
                                        rep As Integer,
                                        buffers As Dictionary(Of Schema.ParquetSchemaNode, Columns.ColumnBuffer))

            Dim structNode As JsonObject = TryCast(value, JsonObject)

            If structNode Is Nothing Then
                Throw New ParquetException($"the struct field '{node.Name}' requires a json object value!")
            End If

            For Each child As Schema.ParquetSchemaNode In node.Children
                Call ShredNode(child, structNode(child.Name), nodeDef, rep, buffers)
            Next
        End Sub

        ''' <summary>
        ''' 为某个节点下面的所有叶子写入空值。
        ''' </summary>
        Private Sub EmitNull(node As Schema.ParquetSchemaNode,
                             rep As Integer,
                             def As Integer,
                             buffers As Dictionary(Of Schema.ParquetSchemaNode, Columns.ColumnBuffer))

            For Each leaf As Schema.ParquetSchemaNode In node.Leaves()
                buffers(leaf).Add(rep, def, Nothing)
            Next
        End Sub

        ''' <summary>
        ''' 将 JSON 数值转换为列物理类型对应的 CLR 数值。
        ''' </summary>
        Friend Function ToClrValue(node As Schema.ParquetSchemaNode, value As JsonNode) As Object
            If node.Logical IsNot Nothing Then
                Select Case node.Logical.Kind
                    Case Format.LogicalType.KindTimestamp
                        Return ToTimestampMillis(value)
                    Case Format.LogicalType.KindDate
                        Return ToDateDays(value)
                End Select
            End If

            Select Case node.PhysicalType.Value
                Case Format.ParquetType.[BOOLEAN]
                    Return ToBoolean(value)
                Case Format.ParquetType.INT32, Format.ParquetType.INT64
                    Return ToInt64(value)
                Case Format.ParquetType.FLOAT, Format.ParquetType.[DOUBLE]
                    Return ToDouble(value)
                Case Else
                    Return Encoding.UTF8.GetBytes(JsonText(value))
            End Select
        End Function

        ''' <summary>
        ''' 取得 JSON 数值的文本形式（字符串节点返回反序列化之后的原文）。
        ''' </summary>
        Friend Function JsonText(value As JsonNode) As String
            If value Is Nothing Then
                Return ""
            End If

            Try
                Return value.GetValue(Of String)()
            Catch
                Return value.ToJsonString()
            End Try
        End Function

        Friend Function ToBoolean(value As JsonNode) As Boolean
            Dim text As String = JsonText(value)

            Select Case LCase(text)
                Case "true"
                    Return True
                Case "false", "null", ""
                    Return False
                Case Else
                    Return Convert.ToBoolean(Double.Parse(text, Globalization.CultureInfo.InvariantCulture))
            End Select
        End Function

        Friend Function ToInt64(value As JsonNode) As Long
            Dim text As String = JsonText(value)

            If Long.TryParse(text, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, Nothing) Then
                Return Long.Parse(text, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            End If

            Return CLng(Double.Parse(text, Globalization.CultureInfo.InvariantCulture))
        End Function

        Friend Function ToDouble(value As JsonNode) As Double
            Dim text As String = JsonText(value)

            Return Double.Parse(text, Globalization.CultureInfo.InvariantCulture)
        End Function

        Friend Function ToTimestampMillis(value As JsonNode) As Long
            Dim text As String = JsonText(value)

            If Long.TryParse(text, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, Nothing) Then
                Return Long.Parse(text, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            End If

            Dim time As DateTime = DateTime.Parse(text, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind)

            Return CLng((time.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds)
        End Function

        Friend Function ToDateDays(value As JsonNode) As Integer
            Dim text As String = JsonText(value)

            If Integer.TryParse(text, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, Nothing) Then
                Return Integer.Parse(text, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
            End If

            Dim time As DateTime = DateTime.Parse(text, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.RoundtripKind)

            Return CInt((time.Date - DateTime.UnixEpoch.Date).TotalDays)
        End Function

    End Module

End Namespace
