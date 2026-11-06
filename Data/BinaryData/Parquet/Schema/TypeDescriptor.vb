Option Strict On
Option Explicit On
Option Infer On

Imports System.Text
Imports System.Text.Json.Nodes

Namespace Schema

    ''' <summary>
    ''' 嵌套列的类型描述（JSON 文本）与 schema 节点之间的转换。
    ''' <para>
    ''' 描述格式示例：<code>
    ''' {"type":"list","element":{"type":"double"}}
    ''' {"type":"map","key":{"type":"string"},"value":{"type":"int64"}}
    ''' {"type":"struct","fields":{"a":{"type":"double"},"b":{"type":"list","element":{"type":"string"}}}}
    ''' {"type":"int64","logical":"timestamp"}
    ''' </code>
    ''' </para>
    ''' </summary>
    Public Module TypeDescriptor

        Public Const TypeBoolean As String = "boolean"
        Public Const TypeInt32 As String = "int32"
        Public Const TypeInt64 As String = "int64"
        Public Const TypeFloat As String = "float"
        Public Const TypeDouble As String = "double"
        Public Const TypeString As String = "string"
        Public Const TypeBytes As String = "bytes"
        Public Const TypeDate As String = "date"
        Public Const TypeTimestamp As String = "timestamp"
        Public Const TypeTime As String = "time"

        ''' <summary>
        ''' 解析类型描述并构建 schema 节点。
        ''' </summary>
        Public Function FromJson(name As String, json As String) As ParquetSchemaNode
            If String.IsNullOrWhiteSpace(json) Then
                Throw New ParquetException($"the nested type definition of column '{name}' is empty!")
            End If

            Dim document As JsonNode = JsonNode.Parse(json)

            Return Build(name, document)
        End Function

        Private Function Build(name As String, descriptor As JsonNode) As ParquetSchemaNode
            If descriptor Is Nothing Then
                Throw New ParquetException($"invalid nested type definition for column '{name}': the descriptor is empty!")
            End If

            Dim typeName As String = GetString(descriptor, "type")

            If typeName Is Nothing Then
                Throw New ParquetException($"invalid nested type definition for column '{name}': the 'type' field is missing!")
            End If

            Select Case LCase(typeName)
                Case "list"
                    Dim listNode As New ParquetSchemaNode With {
                        .Name = name,
                        .Repetition = Format.FieldRepetitionType.[OPTIONAL],
                        .Logical = Format.LogicalType.MakeList(),
                        .Converted = Format.ConvertedType.LIST
                    }
                    Dim middle As New ParquetSchemaNode With {
                        .Name = "list",
                        .Repetition = Format.FieldRepetitionType.REPEATED,
                        .Parent = listNode
                    }
                    Dim element As ParquetSchemaNode = Build("element", descriptor("element"))

                    Call middle.Children.Add(element)

                    If element IsNot Nothing Then
                        element.Parent = middle
                    End If

                    Call listNode.Children.Add(middle)

                    Return listNode
                Case "map"
                    Dim mapNode As New ParquetSchemaNode With {
                        .Name = name,
                        .Repetition = Format.FieldRepetitionType.[OPTIONAL],
                        .Logical = Format.LogicalType.MakeMap(),
                        .Converted = Format.ConvertedType.MAP
                    }
                    Dim keyValue As New ParquetSchemaNode With {
                        .Name = "key_value",
                        .Repetition = Format.FieldRepetitionType.REPEATED,
                        .Converted = Format.ConvertedType.MAP_KEY_VALUE,
                        .Parent = mapNode
                    }
                    Dim keyNode As ParquetSchemaNode = Build("key", descriptor("key"))
                    Dim valueNode As ParquetSchemaNode = Build("value", descriptor("value"))

                    keyNode.Repetition = Format.FieldRepetitionType.REQUIRED
                    keyNode.Parent = keyValue

                    Call keyValue.Children.Add(keyNode)

                    If valueNode IsNot Nothing Then
                        valueNode.Parent = keyValue
                        Call keyValue.Children.Add(valueNode)
                    End If

                    Call mapNode.Children.Add(keyValue)

                    Return mapNode
                Case "struct"
                    Dim structNode As New ParquetSchemaNode With {
                        .Name = name,
                        .Repetition = Format.FieldRepetitionType.[OPTIONAL]
                    }
                    Dim fields As JsonNode = descriptor("fields")

                    If fields IsNot Nothing Then
                        For Each field As KeyValuePair(Of String, JsonNode) In DirectCast(fields, JsonObject)
                            Dim child As ParquetSchemaNode = Build(field.Key, field.Value)

                            child.Parent = structNode
                            Call structNode.Children.Add(child)
                        Next
                    End If

                    Return structNode
                Case Else
                    Return BuildLeaf(name, typeName, descriptor)
            End Select
        End Function

        Private Function BuildLeaf(name As String, typeName As String, descriptor As JsonNode) As ParquetSchemaNode
            Dim node As New ParquetSchemaNode With {
                .Name = name,
                .Repetition = Format.FieldRepetitionType.[OPTIONAL]
            }
            Dim logical As String = GetString(descriptor, "logical")

            Select Case LCase(typeName)
                Case TypeBoolean
                    node.PhysicalType = Format.ParquetType.[BOOLEAN]
                Case TypeInt32
                    node.PhysicalType = Format.ParquetType.INT32
                Case TypeInt64
                    node.PhysicalType = Format.ParquetType.INT64
                Case TypeFloat
                    node.PhysicalType = Format.ParquetType.FLOAT
                Case TypeDouble
                    node.PhysicalType = Format.ParquetType.[DOUBLE]
                Case TypeString
                    node.PhysicalType = Format.ParquetType.BYTE_ARRAY
                    node.Logical = Format.LogicalType.MakeString()
                    node.Converted = Format.ConvertedType.UTF8
                Case TypeBytes
                    node.PhysicalType = Format.ParquetType.BYTE_ARRAY
                Case TypeDate
                    node.PhysicalType = Format.ParquetType.INT32
                    node.Logical = Format.LogicalType.MakeDate()
                    node.Converted = Format.ConvertedType.[DATE]
                Case TypeTimestamp
                    node.PhysicalType = Format.ParquetType.INT64
                    node.Logical = Format.LogicalType.TimestampMillis()
                    node.Converted = Format.ConvertedType.TIMESTAMP_MILLIS
                Case TypeTime
                    node.PhysicalType = Format.ParquetType.INT64
                Case Else
                    Throw New ParquetException($"unsupported nested element type '{typeName}' of column '{name}'!")
            End Select

            If logical IsNot Nothing AndAlso LCase(logical).StartsWith("timestamp") Then
                node.Logical = Format.LogicalType.TimestampMillis()
                node.Converted = Format.ConvertedType.TIMESTAMP_MILLIS
            End If

            Return node
        End Function

        ''' <summary>
        ''' 将 schema 节点转换为类型描述 JSON。
        ''' </summary>
        Public Function ToJson(node As ParquetSchemaNode) As String
            Dim builder As New StringBuilder()

            Call WriteNode(builder, node)

            Return builder.ToString()
        End Function

        Private Sub WriteNode(builder As StringBuilder, node As ParquetSchemaNode)
            If node.IsListGroup Then
                Call builder.Append("{""type"":""list"",""element"":")
                Call WriteNode(builder, node.Children(0).Children(0))
                Call builder.Append("}")
            ElseIf node.IsMapGroup Then
                Dim keyValue As ParquetSchemaNode = node.Children(0)

                Call builder.Append("{""type"":""map"",""key"":")
                Call WriteNode(builder, keyValue.Children(0))
                Call builder.Append(",""value"":")

                If keyValue.Children.Count > 1 Then
                    Call WriteNode(builder, keyValue.Children(1))
                Else
                    Call builder.Append("""unknown""")
                End If

                Call builder.Append("}")
            ElseIf node.IsGroup Then
                Call builder.Append("{""type"":""struct"",""fields"":{")

                For i As Integer = 0 To node.Children.Count - 1
                    If i > 0 Then
                        Call builder.Append(",")
                    End If

                    Call builder.Append(""""c).Append(Escape(node.Children(i).Name)).Append(""":")
                    Call WriteNode(builder, node.Children(i))
                Next

                Call builder.Append("}}")
            Else
                Call builder.Append("{""type"":""").Append(PhysicalName(node)).Append("""")

                If node.Logical IsNot Nothing Then
                    Select Case node.Logical.Kind
                        Case Format.LogicalType.KindTimestamp
                            Call builder.Append(",""logical"":""timestamp""")
                        Case Format.LogicalType.KindDate
                            Call builder.Append(",""logical"":""date""")
                        Case Format.LogicalType.KindString
                            ' string 类型已经由 type 字段表达
                    End Select
                End If

                Call builder.Append("}")
            End If
        End Sub

        Private Function PhysicalName(node As ParquetSchemaNode) As String
            If node.Logical IsNot Nothing Then
                Select Case node.Logical.Kind
                    Case Format.LogicalType.KindString
                        Return TypeString
                    Case Format.LogicalType.KindTimestamp
                        Return TypeTimestamp
                    Case Format.LogicalType.KindDate
                        Return TypeDate
                End Select
            End If

            If Not node.PhysicalType.HasValue Then
                Return TypeBytes
            End If

            Select Case node.PhysicalType.Value
                Case Format.ParquetType.[BOOLEAN] : Return TypeBoolean
                Case Format.ParquetType.INT32 : Return TypeInt32
                Case Format.ParquetType.INT64 : Return TypeInt64
                Case Format.ParquetType.FLOAT : Return TypeFloat
                Case Format.ParquetType.[DOUBLE] : Return TypeDouble
                Case Format.ParquetType.BYTE_ARRAY : Return TypeString
                Case Else : Return TypeBytes
            End Select
        End Function

        Private Function GetString(node As JsonNode, name As String) As String
            Dim value As JsonNode = node(name)

            If value Is Nothing Then
                Return Nothing
            End If

            Return value.GetValue(Of String)()
        End Function

        Private Function Escape(text As String) As String
            Return If(text, "").Replace("\", "\\").Replace("""", "\""")
        End Function

    End Module

End Namespace
