Option Strict On
Option Explicit On
Option Infer On

Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Linq

Namespace Nested

    ''' <summary>
    ''' 嵌套列读取端的 record assembly 实现：把叶子列的 <c>(repetition_level, definition_level, value)</c>
    ''' 三元流按照 schema 树组装回每一行的 JSON 文本。
    ''' <para>
    ''' 支持的结构：struct、LIST、LIST&lt;STRUCT&gt;、LIST&lt;LIST&gt;、MAP&lt;PRIMITIVE, PRIMITIVE&gt;；
    ''' 更复杂的组合（例如 map 的 value 为 struct / list）会抛出明确的异常。
    ''' </para>
    ''' </summary>
    Public Module JsonAssembler

        ''' <summary>
        ''' 组装顶层嵌套列，返回每一行对应的 JSON 文本（空值为 <c>null</c>）。
        ''' </summary>
        Public Function Assemble(top As Schema.ParquetSchemaNode,
                                 columns As Dictionary(Of Schema.ParquetSchemaNode, Columns.DataColumn),
                                 numRows As Integer) As String()
            If numRows <= 0 Then
                Return New String() {}
            End If

            If top.IsMapGroup Then
                Return AssembleMap(top, columns, numRows)
            End If

            Dim leaves As List(Of Schema.ParquetSchemaNode) = top.Leaves().ToList
            Dim data As New List(Of Columns.DataColumn)
            Dim starts As New List(Of Integer())
            Dim valueOffsets As New List(Of Integer())

            For Each leaf As Schema.ParquetSchemaNode In leaves
                Dim column As Columns.DataColumn = columns(leaf)
                Dim rowSlots As Integer() = RowStarts(column, numRows)

                Call data.Add(column)
                Call starts.Add(rowSlots)
                Call valueOffsets.Add(ValueStarts(column, rowSlots, numRows))
            Next

            Dim result As String() = New String(numRows - 1) {}

            For row As Integer = 0 To numRows - 1
                Dim holder As JsonNode() = New JsonNode(0) {}

                For k As Integer = 0 To leaves.Count - 1
                    Call AssembleLeafRow(holder, leaves(k), data(k), starts(k), valueOffsets(k), row)
                Next

                result(row) = If(holder(0) Is Nothing, "null", holder(0).ToJsonString())
            Next

            Return result
        End Function

        Private Sub AssembleLeafRow(holder As JsonNode(),
                                    leaf As Schema.ParquetSchemaNode,
                                    column As Columns.DataColumn,
                                    starts As Integer(),
                                    valueStarts As Integer(),
                                    row As Integer)
            Dim path As List(Of Schema.ParquetSchemaNode) = PathOf(leaf)
            Dim repeats As New List(Of Schema.ParquetSchemaNode)

            For Each node As Schema.ParquetSchemaNode In path
                If node.IsRepeated Then
                    Call repeats.Add(node)
                End If
            Next

            Dim counters As Integer() = New Integer(repeats.Count) {}
            Dim valueIndex As Integer = valueStarts(row)
            Dim first As Boolean = True
            

            For slot As Integer = starts(row) To starts(row + 1) - 1
                Dim definition As Integer = column.DefinitionLevels(slot)
                Dim repetition As Integer = If(column.RepetitionLevels.Length = 0, 0, column.RepetitionLevels(slot))

                If first Then
                    first = False

                    For d As Integer = 1 To repeats.Count
                        counters(d) = 0
                    Next
                ElseIf repetition >= 1 Then
                    counters(repetition) += 1

                    For d As Integer = repetition + 1 To repeats.Count
                        counters(d) = 0
                    Next
                End If

                Dim depth As Integer = 0

                For d As Integer = 0 To repeats.Count - 1
                    If definition >= repeats(d).DefinitionLevel Then
                        depth += 1
                    Else
                        Exit For
                    End If
                Next

                Dim positions As Integer() = New Integer(depth - 1) {}

                For d As Integer = 0 To depth - 1
                    positions(d) = counters(d + 1)
                Next

                Dim hasValue As Boolean = definition >= column.MaxDefinitionLevel
                Dim value As Object = Nothing

                If hasValue Then
                    value = column.Values.GetValue(valueIndex)
                    valueIndex += 1
                End If

                Call InsertValue(holder, path, positions, value, hasValue, definition)
            Next
        End Sub

        ''' <summary>
        ''' 把单个数值插入到行内的 JSON 结构当中。
        ''' </summary>
        Private Sub InsertValue(holder As JsonNode(),
                                path As List(Of Schema.ParquetSchemaNode),
                                positions As Integer(),
                                value As Object,
                                hasValue As Boolean,
                                definition As Integer)

            Dim parent As JsonNode = Nothing
            Dim slot As Object = Nothing
            Dim pendingArray As JsonArray = Nothing
            Dim posIndex As Integer = 0

            For i As Integer = 0 To path.Count - 1
                Dim node As Schema.ParquetSchemaNode = path(i)

                If node.IsLeaf Then
                    Dim key As Object = KeyFor(parent, slot, node.Name)

                    Call AssignNode(holder, parent, key, If(hasValue, ToJsonValue(node, value), Nothing))
                    Return
                End If

                If node.IsListGroup Then
                    If definition < node.DefinitionLevel Then
                        ' 列表本身为空值（而不是空列表）
                        Call AssignNode(holder, parent, KeyFor(parent, slot, node.Name), Nothing)
                        Return
                    End If

                    Dim arrayNode As JsonArray = TryCast(GetNodeAt(holder, parent, KeyFor(parent, slot, node.Name)), JsonArray)

                    If arrayNode Is Nothing Then
                        arrayNode = New JsonArray()
                        Call AssignNode(holder, parent, KeyFor(parent, slot, node.Name), arrayNode)
                    End If

                    pendingArray = arrayNode
                    parent = Nothing
                    slot = Nothing
                    Continue For
                End If

                If node.IsRepeated Then
                    Dim arrayNode As JsonArray = pendingArray

                    If arrayNode Is Nothing Then
                        arrayNode = New JsonArray()
                        Call AssignNode(holder, parent, KeyFor(parent, slot, node.Name), arrayNode)
                    End If

                    pendingArray = Nothing

                    If posIndex >= positions.Length Then
                        Return
                    End If

                    Dim index As Integer = positions(posIndex)
                    posIndex += 1

                    Call EnsureArraySize(arrayNode, index + 1)

                    parent = arrayNode
                    slot = index
                    Continue For
                End If

                ' struct / element 包装节点
                Dim objectKey As Object = KeyFor(parent, slot, node.Name)

                If definition < node.DefinitionLevel Then
                    ' 结构体本身为空值
                    Call AssignNode(holder, parent, objectKey, Nothing)
                    Return
                End If

                Dim objectNode As JsonObject = TryCast(GetNodeAt(holder, parent, objectKey), JsonObject)

                If objectNode Is Nothing Then
                    objectNode = New JsonObject()
                    Call AssignNode(holder, parent, objectKey, objectNode)
                End If

                parent = objectNode
                slot = Nothing
            Next
        End Sub

        Private Function AssembleMap(top As Schema.ParquetSchemaNode,
                                     columns As Dictionary(Of Schema.ParquetSchemaNode, Columns.DataColumn),
                                     numRows As Integer) As String()
            Dim keyValue As Schema.ParquetSchemaNode = top.Children(0)
            Dim keyNode As Schema.ParquetSchemaNode = keyValue.Children(0)

            If keyNode.IsGroup Then
                Throw New ParquetException($"the map column '{top.Name}' uses a complex key type which is not supported!")
            End If

            Dim valueNode As Schema.ParquetSchemaNode = Nothing
            Dim keyColumn As Columns.DataColumn = columns(keyNode)
            Dim valueColumn As Columns.DataColumn = Nothing
            Dim keyRowStarts As Integer() = RowStarts(keyColumn, numRows)
            Dim keyValueStarts As Integer() = ValueStarts(keyColumn, keyRowStarts, numRows)
            Dim valueRowStarts As Integer() = Nothing
            Dim valueValueStarts As Integer() = Nothing

            If keyValue.Children.Count > 1 Then
                valueNode = keyValue.Children(1)

                If valueNode.IsGroup Then
                    Throw New ParquetException($"the map column '{top.Name}' uses a complex value type which is not supported by this implementation!")
                End If

                valueColumn = columns(valueNode)
                valueRowStarts = RowStarts(valueColumn, numRows)
                valueValueStarts = ValueStarts(valueColumn, valueRowStarts, numRows)
            End If

            Dim result As String() = New String(numRows - 1) {}

            For row As Integer = 0 To numRows - 1
                Dim map As New JsonObject()
                Dim keyIndex As Integer = keyValueStarts(row)
                Dim valueIndex As Integer = If(valueColumn Is Nothing, 0, valueValueStarts(row))
                Dim valueEnd As Integer = If(valueColumn Is Nothing, 0, valueValueStarts(row + 1))
                Dim valueSlot As Integer = 0

                For slot As Integer = keyRowStarts(row) To keyRowStarts(row + 1) - 1
                    If keyColumn.DefinitionLevels(slot) < keyColumn.MaxDefinitionLevel Then
                        Continue For
                    End If

                    Dim key As String = ToKeyText(keyColumn.Values.GetValue(keyIndex))

                    keyIndex += 1

                    Dim item As JsonNode = Nothing

                    If valueColumn IsNot Nothing AndAlso valueIndex < valueEnd Then
                        Dim valueSlotIndex As Integer = valueRowStarts(row) + valueSlot

                        If valueSlotIndex < valueColumn.Count AndAlso valueColumn.DefinitionLevels(valueSlotIndex) >= valueColumn.MaxDefinitionLevel Then
                            item = ToJsonValue(valueNode, valueColumn.Values.GetValue(valueIndex))
                        End If

                        valueIndex += 1
                        valueSlot += 1
                    End If

                    map(key) = item
                Next

                result(row) = map.ToJsonString()
            Next

            Return result
        End Function

        Private Function ToKeyText(value As Object) As String
            If value Is Nothing Then
                Return ""
            End If
            If TypeOf value Is Byte() Then
                Return Encoding.UTF8.GetString(CType(value, Byte()))
            End If

            Return Convert.ToString(value, Globalization.CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' 计算某个叶子列每一行第一个已定义数值在 <c>Values</c> 中的下标（长度为 numRows + 1）。
        ''' </summary>
        Private Function ValueStarts(column As Columns.DataColumn, starts As Integer(), numRows As Integer) As Integer()
            Dim result As Integer() = New Integer(numRows) {}
            Dim count As Integer = 0

            For row As Integer = 0 To numRows - 1
                result(row) = count

                For slot As Integer = starts(row) To starts(row + 1) - 1
                    If slot >= 0 AndAlso slot < column.Count AndAlso column.DefinitionLevels(slot) >= column.MaxDefinitionLevel Then
                        count += 1
                    End If
                Next
            Next

            result(numRows) = count

            Return result
        End Function

        ''' <summary>
        ''' 计算某个叶子列每一行的槽位起点（长度为 numRows + 1）。
        ''' </summary>
        Private Function RowStarts(column As Columns.DataColumn, numRows As Integer) As Integer()
            Dim starts As Integer() = New Integer(numRows) {}
            Dim row As Integer = 0

            starts(0) = 0

            For slot As Integer = 0 To column.Count - 1
                If slot > 0 AndAlso column.RepetitionLevels(slot) = 0 Then
                    row += 1

                    If row > numRows Then
                        Exit For
                    End If

                    starts(row) = slot
                End If
            Next

            For i As Integer = row + 1 To numRows
                starts(i) = column.Count
            Next

            Return starts
        End Function

        Private Function PathOf(node As Schema.ParquetSchemaNode) As List(Of Schema.ParquetSchemaNode)
            Dim chain As New List(Of Schema.ParquetSchemaNode)
            Dim current As Schema.ParquetSchemaNode = node

            While current IsNot Nothing AndAlso Not current.IsRoot
                Call chain.Insert(0, current)
                current = current.Parent
            End While

            Return chain
        End Function

        Private Function KeyFor(parent As JsonNode, slot As Object, name As String) As Object
            If parent Is Nothing Then
                Return Nothing
            End If
            If TypeOf parent Is JsonObject Then
                Return name
            End If

            Return slot
        End Function

        Private Function GetNodeAt(holder As JsonNode(), parent As JsonNode, key As Object) As JsonNode
            If parent Is Nothing Then
                Return holder(0)
            End If
            If TypeOf parent Is JsonObject Then
                Return DirectCast(parent, JsonObject)(CStr(key))
            End If

            Dim arrayNode As JsonArray = DirectCast(parent, JsonArray)
            Dim index As Integer = CInt(key)

            If index < 0 OrElse index >= arrayNode.Count Then
                Return Nothing
            End If

            Return arrayNode(index)
        End Function

        Private Sub AssignNode(holder As JsonNode(), parent As JsonNode, key As Object, value As JsonNode)
            If parent Is Nothing Then
                holder(0) = value
                Return
            End If

            If TypeOf parent Is JsonObject Then
                DirectCast(parent, JsonObject)(CStr(key)) = value
                Return
            End If

            Dim arrayNode As JsonArray = DirectCast(parent, JsonArray)
            Dim index As Integer = CInt(key)

            Call EnsureArraySize(arrayNode, index + 1)

            arrayNode(index) = value
        End Sub

        Private Sub EnsureArraySize(arrayNode As JsonArray, size As Integer)
            While arrayNode.Count < size
                Call arrayNode.Add(DirectCast(Nothing, JsonNode))
            End While
        End Sub

        ''' <summary>
        ''' 将列数值转换为 JSON 数值。
        ''' </summary>
        Friend Function ToJsonValue(node As Schema.ParquetSchemaNode, value As Object) As JsonNode
            If value Is Nothing Then
                Return Nothing
            End If

            If node.Logical IsNot Nothing Then
                Select Case node.Logical.Kind
                    Case Format.LogicalType.KindTimestamp
                        Return JsonValue.Create(FromMillis(Convert.ToInt64(value)).ToString("o"))
                    Case Format.LogicalType.KindDate
                        Return JsonValue.Create(DateTime.UnixEpoch.AddDays(Convert.ToInt32(value)).ToString("yyyy-MM-dd"))
                    Case Format.LogicalType.KindString
                        Return JsonValue.Create(Encoding.UTF8.GetString(CType(value, Byte())))
                End Select
            End If

            Select Case node.PhysicalType.Value
                Case Format.ParquetType.[BOOLEAN]
                    Return JsonValue.Create(Convert.ToBoolean(value))
                Case Format.ParquetType.INT32, Format.ParquetType.INT64
                    Return JsonValue.Create(Convert.ToInt64(value))
                Case Format.ParquetType.FLOAT, Format.ParquetType.[DOUBLE]
                    Return JsonValue.Create(Convert.ToDouble(value))
                Case Format.ParquetType.INT96
                    Return JsonValue.Create(Encodings.Int96.ToDateTime(CType(value, Byte())).ToString("o"))
                Case Else
                    Dim bytes As Byte() = CType(value, Byte())

                    If IsPrintableUtf8(bytes) Then
                        Return JsonValue.Create(Encoding.UTF8.GetString(bytes))
                    End If

                    Return JsonValue.Create(Convert.ToBase64String(bytes))
            End Select
        End Function

        Private Function IsPrintableUtf8(bytes As Byte()) As Boolean
            Try
                Dim text As String = Encoding.UTF8.GetString(bytes)

                For Each c As Char In text
                    If Char.IsControl(c) AndAlso c <> ChrW(9) AndAlso c <> ChrW(10) AndAlso c <> ChrW(13) Then
                        Return False
                    End If
                Next

                Return True
            Catch
                Return False
            End Try
        End Function

        Friend Function FromMillis(milliseconds As Long) As DateTime
            Return DateTime.UnixEpoch.AddMilliseconds(milliseconds)
        End Function

    End Module

End Namespace
