Option Strict On
Option Explicit On
Option Infer On

Imports System.Linq
Imports System.Text
Imports DataFrameModel = Microsoft.VisualBasic.Data.Framework.DataFrame
Imports FeatureVectorModel = Microsoft.VisualBasic.Data.Framework.FeatureVector

''' <summary>
''' <see cref="DataFrameModel"/> 与 Parquet 列数据之间的双向映射。
''' <para>
''' 写：DataFrame → <see cref="WritePlan"/>（schema 树 + 顶层列数据）。<br/>
''' 读：schema 树 + 叶子列数据 → DataFrame。
''' </para>
''' <para>
''' 约定：<c>rownames</c> 以 <c>__index__</c> 列存储；struct 成员展开为 <c>a.b</c> 形式的点号列名；
''' LIST / MAP 转换为 JSON 文本列，并在 <c>FeatureVector.attributes("parquet.type")</c> 中记录类型描述。
''' </para>
''' </summary>
Public Module DataFrameMapping

    Public Const NestedTypeAttribute As String = Nested.JsonShredder.NestedTypeAttribute

    ' =====================================================================
    '  写：DataFrame → WritePlan
    ' =====================================================================

    ''' <summary>
    ''' 依据 DataFrame 构建写入计划。
    ''' </summary>
    Public Function CreateWritePlan(df As DataFrameModel, options As ParquetOptions) As WritePlan
        If df Is Nothing Then
            Throw New ArgumentNullException(NameOf(df))
        End If

        Dim opts As ParquetOptions = If(options, ParquetOptions.Defaults).Clone()
        Dim rowCount As Integer = CountRows(df)
        Dim root As New Schema.ParquetSchemaNode With {
            .Name = "schema",
            .IsRoot = True,
            .Element = New Format.SchemaElement With {.Name = "schema"}
        }
        Dim plan As New WritePlan With {.Root = root, .RowCount = rowCount}

        ' ---- 行名列 ----
        If opts.WriteIndexColumn AndAlso df.rownames IsNot Nothing AndAlso df.rownames.Length > 0 Then
            rowCount = System.Math.Max(rowCount, df.rownames.Length)
            plan.RowCount = rowCount

            Dim values As String() = New String(rowCount - 1) {}

            For i As Integer = 0 To rowCount - 1
                values(i) = If(i < df.rownames.Length, df.rownames(i), i.ToString)
            Next

            Dim node As Schema.ParquetSchemaNode = CreateLeafNode(ParquetConstants.IndexColumnName, GetType(String), False)

            Call root.Children.Add(node)
            Call plan.Columns.Add(New ColumnPlan With {
                .Name = node.Name,
                .Kind = ColumnKind.Leaf,
                .Node = node,
                .Values = values
            })
        End If

        ' ---- 按顶层名称分组（点号列名归并为 struct） ----
        Dim order As New List(Of String)
        Dim members As New Dictionary(Of String, List(Of String))

        For Each name As String In df.features.Keys
            Dim top As String = TopName(name)

            If Not members.ContainsKey(top) Then
                Call order.Add(top)
                members(top) = New List(Of String)
            End If

            members(top).Add(name)
        Next

        For Each top As String In order
            Dim group As List(Of String) = members(top)
            Dim declaration As String = GetNestedDefinition(df, top, group, opts)

            If declaration IsNot Nothing Then
                Call AddNestedColumn(plan, root, df, top, group, declaration, rowCount)
            ElseIf group.Count = 1 AndAlso String.Equals(group(0), top, StringComparison.Ordinal) Then
                Call AddLeafColumn(plan, root, df(top), rowCount)
            Else
                Call AddStructColumns(plan, root, df, top, group, rowCount)
            End If
        Next

        Call Schema.ParquetSchema.ComputeLevels(root)

        Return plan
    End Function

    Private Sub AddLeafColumn(plan As WritePlan, root As Schema.ParquetSchemaNode, column As FeatureVectorModel, rowCount As Integer)
        Dim nullMask As Boolean() = Nothing
        Dim values As Array = AsTypedColumn(column, rowCount, nullMask)
        Dim node As Schema.ParquetSchemaNode = CreateLeafNode(column.name, column.type, nullMask IsNot Nothing)

        Call root.Children.Add(node)
        Call plan.Columns.Add(New ColumnPlan With {
            .Name = node.Name,
            .Kind = ColumnKind.Leaf,
            .Node = node,
            .Values = values,
            .NullMask = nullMask
        })
    End Sub

    Private Sub AddStructColumns(plan As WritePlan,
                                 root As Schema.ParquetSchemaNode,
                                 df As DataFrameModel,
                                 top As String,
                                 group As List(Of String),
                                 rowCount As Integer)
        Dim structNode As New Schema.ParquetSchemaNode With {
            .Name = top,
            .Repetition = Format.FieldRepetitionType.[OPTIONAL],
            .Parent = root
        }

        Call root.Children.Add(structNode)

        For Each member As String In group
            Dim parts As String() = member.Split("."c)

            If parts.Length < 2 Then
                Throw New ParquetException($"the column '{member}' could not be grouped into the struct column '{top}'!")
            End If

            Dim parent As Schema.ParquetSchemaNode = structNode

            For i As Integer = 1 To parts.Length - 2
                Dim subNode As Schema.ParquetSchemaNode = parent.[Select](parts(i))

                If subNode Is Nothing Then
                    subNode = New Schema.ParquetSchemaNode With {
                        .Name = parts(i),
                        .Repetition = Format.FieldRepetitionType.[OPTIONAL],
                        .Parent = parent
                    }

                    Call parent.Children.Add(subNode)
                End If

                parent = subNode
            Next

            Dim column As FeatureVectorModel = df(member)
            Dim nullMask As Boolean() = Nothing
            Dim values As Array = AsTypedColumn(column, rowCount, nullMask)
            Dim leaf As Schema.ParquetSchemaNode = CreateLeafNode(parts(parts.Length - 1), column.type, nullMask IsNot Nothing)

            leaf.Parent = parent
            Call parent.Children.Add(leaf)

            Call plan.Columns.Add(New ColumnPlan With {
                .Name = member,
                .Kind = ColumnKind.Leaf,
                .Node = leaf,
                .Values = values,
                .NullMask = nullMask
            })
        Next
    End Sub

    Private Sub AddNestedColumn(plan As WritePlan,
                                root As Schema.ParquetSchemaNode,
                                df As DataFrameModel,
                                top As String,
                                group As List(Of String),
                                declaration As String,
                                rowCount As Integer)
        Dim node As Schema.ParquetSchemaNode = Schema.TypeDescriptor.FromJson(top, declaration)

        node.Parent = root
        Call root.Children.Add(node)

        Dim source As FeatureVectorModel = df(group(0))
        Dim json As String() = New String(rowCount - 1) {}

        For i As Integer = 0 To rowCount - 1
            Dim value As Object = If(i < source.size, source.GetValue(i), Nothing)

            json(i) = If(value Is Nothing, Nothing, Convert.ToString(value))
        Next

        Call plan.Columns.Add(New ColumnPlan With {
            .Name = top,
            .Kind = ColumnKind.Nested,
            .Node = node,
            .JsonValues = json,
            .TypeDefinition = declaration
        })
    End Sub

    Private Function GetNestedDefinition(df As DataFrameModel,
                                         top As String,
                                         group As List(Of String),
                                         options As ParquetOptions) As String
        If options.NestedColumns IsNot Nothing AndAlso options.NestedColumns.ContainsKey(top) Then
            Return options.NestedColumns(top)
        End If

        If group.Count <> 1 Then
            Return Nothing
        End If

        Dim column As FeatureVectorModel = df(group(0))

        If column.attributes Is Nothing Then
            Return Nothing
        End If

        Dim declaration As String = Nothing

        If column.attributes.TryGetValue(NestedTypeAttribute, declaration) Then
            Return declaration
        End If

        Return Nothing
    End Function

    Private Function TopName(name As String) As String
        Dim index As Integer = name.IndexOf("."c)

        If index < 0 Then
            Return name
        End If

        Return name.Substring(0, index)
    End Function

    ''' <summary>
    ''' DataFrame 的行数（取行名数量与所有列长度的最大值）。
    ''' </summary>
    Public Function CountRows(df As DataFrameModel) As Integer
        Dim count As Integer = If(df.rownames Is Nothing, 0, df.rownames.Length)

        For Each column As FeatureVectorModel In df.features.Values
            If column Is Nothing OrElse column.vector Is Nothing Then
                Continue For
            End If

            count = System.Math.Max(count, column.vector.Length)
        Next

        Return count
    End Function

    ''' <summary>
    ''' 按列的实际类型取出强类型数组，并生成字符串列的空值掩码。
    ''' </summary>
    Private Function AsTypedColumn(column As FeatureVectorModel, rowCount As Integer, ByRef nullMask As Boolean()) As Array
        If column Is Nothing OrElse column.vector Is Nothing Then
            nullMask = Nothing
            Return New String(rowCount - 1) {}
        End If

        Select Case column.type
            Case GetType(Integer)
                Return Adapt(Of Integer)(column, rowCount)
            Case GetType(Short)
                Dim source As Short() = Adapt(Of Short)(column, rowCount)
                Dim values As Integer() = New Integer(rowCount - 1) {}

                For i As Integer = 0 To rowCount - 1
                    values(i) = source(i)
                Next

                Return values
            Case GetType(Byte)
                Dim source As Byte() = Adapt(Of Byte)(column, rowCount)
                Dim values As Integer() = New Integer(rowCount - 1) {}

                For i As Integer = 0 To rowCount - 1
                    values(i) = source(i)
                Next

                Return values
            Case GetType(Long)
                Return Adapt(Of Long)(column, rowCount)
            Case GetType(Single)
                Return Adapt(Of Single)(column, rowCount)
            Case GetType(Double)
                Return Adapt(Of Double)(column, rowCount)
            Case GetType(Boolean)
                Return Adapt(Of Boolean)(column, rowCount)
            Case GetType(String)
                Dim source As String() = Adapt(Of String)(column, rowCount)
                Dim mask As Boolean() = Nothing

                For i As Integer = 0 To rowCount - 1
                    If source(i) Is Nothing Then
                        If mask Is Nothing Then
                            mask = New Boolean(rowCount - 1) {}
                        End If

                        mask(i) = True
                    End If
                Next

                nullMask = mask

                Return source
            Case GetType(Date)
                Dim source As Date() = Adapt(Of Date)(column, rowCount)
                Dim values As Long() = New Long(rowCount - 1) {}

                For i As Integer = 0 To rowCount - 1
                    values(i) = CLng((source(i).ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds)
                Next

                Return values
            Case GetType(TimeSpan)
                Dim source As TimeSpan() = Adapt(Of TimeSpan)(column, rowCount)
                Dim values As Long() = New Long(rowCount - 1) {}

                For i As Integer = 0 To rowCount - 1
                    values(i) = source(i).Ticks
                Next

                Return values
            Case Else
                Throw New ParquetException($"the column '{column.name}' has the type {column.type.FullName} which is not supported by the parquet writer!")
        End Select
    End Function

    Private Function Adapt(Of T)(column As FeatureVectorModel, rowCount As Integer) As T()
        Dim source As T() = DirectCast(column.vector, T())

        If source.Length = rowCount Then
            Return source
        End If

        Dim result As T() = New T(rowCount - 1) {}

        If source.Length = 1 Then
            For i As Integer = 0 To rowCount - 1
                result(i) = source(0)
            Next
        ElseIf source.Length > 1 Then
            For i As Integer = 0 To System.Math.Min(rowCount, source.Length) - 1
                result(i) = source(i)
            Next
        End If

        Return result
    End Function

    ''' <summary>
    ''' 依据 CLR 类型创建叶子 schema 节点。
    ''' </summary>
    Private Function CreateLeafNode(name As String, type As Type, hasNulls As Boolean) As Schema.ParquetSchemaNode
        Dim node As New Schema.ParquetSchemaNode With {
            .Name = name,
            .Repetition = If(hasNulls, Format.FieldRepetitionType.[OPTIONAL], Format.FieldRepetitionType.REQUIRED)
        }

        Select Case type
            Case GetType(Integer), GetType(Short), GetType(Byte)
                node.PhysicalType = Format.ParquetType.INT32
            Case GetType(Long)
                node.PhysicalType = Format.ParquetType.INT64
            Case GetType(Single)
                node.PhysicalType = Format.ParquetType.FLOAT
            Case GetType(Double)
                node.PhysicalType = Format.ParquetType.[DOUBLE]
            Case GetType(Boolean)
                node.PhysicalType = Format.ParquetType.[BOOLEAN]
            Case GetType(String)
                node.PhysicalType = Format.ParquetType.BYTE_ARRAY
                node.Logical = Format.LogicalType.MakeString()
                node.Converted = Format.ConvertedType.UTF8
            Case GetType(Date)
                node.PhysicalType = Format.ParquetType.INT64
                node.Logical = Format.LogicalType.TimestampMillis()
                node.Converted = Format.ConvertedType.TIMESTAMP_MILLIS
            Case GetType(TimeSpan)
                node.PhysicalType = Format.ParquetType.INT64
            Case Else
                Throw New ParquetException($"the type {type.FullName} of column '{name}' is not supported by the parquet writer!")
        End Select

        Return node
    End Function

    ' =====================================================================
    '  读：列数据 → DataFrame
    ' =====================================================================

    ''' <summary>
    ''' 将读取到的叶子列数据还原为 <see cref="DataFrameModel"/>。
    ''' </summary>
    Public Function ToDataFrame(tableSchema As Schema.ParquetSchema,
                                columnData As List(Of Columns.DataColumn),
                                numRows As Integer) As DataFrameModel
        Dim lookup As New Dictionary(Of Schema.ParquetSchemaNode, Columns.DataColumn)

        For Each column As Columns.DataColumn In columnData
            lookup(column.Node) = column
        Next

        Dim df As New DataFrameModel With {
            .features = New Dictionary(Of String, FeatureVectorModel)
        }
        Dim tops As New List(Of Schema.ParquetSchemaNode)

        For Each leaf As Schema.ParquetSchemaNode In tableSchema.Leaves
            Dim top As Schema.ParquetSchemaNode = leaf.TopLevel()

            If Not tops.Contains(top) Then
                Call tops.Add(top)
            End If
        Next

        For Each top As Schema.ParquetSchemaNode In tops
            Dim leaves As List(Of Schema.ParquetSchemaNode) = top.Leaves().ToList
            Dim isNested As Boolean = leaves.Any(Function(leaf) leaf.HasRepeatedAncestor())

            If isNested Then
                Dim json As String() = Nested.JsonAssembler.Assemble(top, lookup, numRows)
                Dim column As New FeatureVectorModel(top.Name, json)

                column.attributes = New Dictionary(Of String, String) From {
                    {NestedTypeAttribute, Schema.TypeDescriptor.ToJson(top)}
                }

                Call df.features.Add(top.Name, column)
            Else
                For Each leaf As Schema.ParquetSchemaNode In leaves
                    Dim name As String = String.Join(".", leaf.DataPath())
                    Dim data As Columns.DataColumn = Nothing

                    If Not lookup.TryGetValue(leaf, data) Then
                        ' 空文件（0 行）时没有列数据，这里按 schema 构造一个空列以保留表结构
                        data = New Columns.DataColumn With {
                            .Node = leaf,
                            .NumSlots = 0,
                            .DefinitionLevels = New Integer() {},
                            .RepetitionLevels = New Integer() {},
                            .Values = Columns.ArrayUtil.EmptyValues(leaf.PhysicalType.Value)
                        }
                    End If

                    Dim column As FeatureVectorModel = MakeFeatureVector(name, leaf, data)

                    If df.features.ContainsKey(column.name) Then
                        column = MakeFeatureVector(UniqueName(df, column.name), leaf, data)
                    End If

                    Call df.features.Add(column.name, column)
                Next
            End If
        Next

        Dim rownames As String() = Nothing

        If df.features.ContainsKey(ParquetConstants.IndexColumnName) Then
            Dim indexColumn As FeatureVectorModel = df.features(ParquetConstants.IndexColumnName)

            ' 行名列可能来自外部文件，类型不一定是字符串，这里统一转换为文本
            If indexColumn.size = numRows Then
                rownames = Columns.ArrayUtil.NewArray(Of String)(numRows)

                For i As Integer = 0 To numRows - 1
                    Dim value As Object = indexColumn.GetValue(i)

                    rownames(i) = If(value Is Nothing, Nothing, Convert.ToString(value, Globalization.CultureInfo.InvariantCulture))
                Next
            End If

            Call df.features.Remove(ParquetConstants.IndexColumnName)
        End If

        If rownames Is Nothing OrElse rownames.Length <> numRows Then
            Dim generated As String() = Columns.ArrayUtil.NewArray(Of String)(numRows)

            For i As Integer = 0 To numRows - 1
                generated(i) = i.ToString
            Next

            rownames = generated
        End If

        If numRows > 0 Then
            df.name = $"parquet[{df.features.Count} x {numRows}]"
        End If

        df.rownames = rownames

        Return df
    End Function

    Private Function UniqueName(df As DataFrameModel, name As String) As String
        Dim index As Integer = 1

        While df.features.ContainsKey($"{name}.{index}")
            index += 1
        End While

        Return $"{name}.{index}"
    End Function

    ''' <summary>
    ''' 将叶子列数据转换为 DataFrame 的特征列。
    ''' </summary>
    Private Function MakeFeatureVector(name As String, leaf As Schema.ParquetSchemaNode, column As Columns.DataColumn) As FeatureVectorModel
        Dim physical As Format.ParquetType = leaf.PhysicalType.Value
        Dim hasNulls As Boolean = column.DefinedCount < column.Count

        If physical = Format.ParquetType.INT96 Then
            Dim raw As Byte()() = CType(column.Values, Byte()())
            Dim times As Date() = Columns.ArrayUtil.NewArray(Of Date)(raw.Length)

            For i As Integer = 0 To raw.Length - 1
                times(i) = Encodings.Int96.ToDateTime(raw(i))
            Next

            Return New FeatureVectorModel(name, times)
        End If

        If leaf.Logical IsNot Nothing Then
            Select Case leaf.Logical.Kind
                Case Format.LogicalType.KindTimestamp
                    Dim ticks As Long() = CType(column.Values, Long())
                    Dim times As Date() = Columns.ArrayUtil.NewArray(Of Date)(ticks.Length)
                    Dim unit As String = If(leaf.Logical.TimeUnit, "MILLIS")

                    For i As Integer = 0 To ticks.Length - 1
                        Select Case UCase(unit)
                            Case "MICROS"
                                times(i) = DateTime.UnixEpoch.AddTicks(ticks(i) * 10L)
                            Case "NANOS"
                                times(i) = DateTime.UnixEpoch.AddTicks(ticks(i) \ 100L)
                            Case Else
                                times(i) = DateTime.UnixEpoch.AddMilliseconds(ticks(i))
                        End Select
                    Next

                    Return New FeatureVectorModel(name, times)
                Case Format.LogicalType.KindDate
                    Dim days As Integer() = CType(column.Values, Integer())
                    Dim times As Date() = Columns.ArrayUtil.NewArray(Of Date)(days.Length)

                    For i As Integer = 0 To days.Length - 1
                        times(i) = DateTime.UnixEpoch.AddDays(days(i))
                    Next

                    Return New FeatureVectorModel(name, times)
                Case Format.LogicalType.KindString
                    Return New FeatureVectorModel(name, AsStrings(column))
                Case Format.LogicalType.KindDecimal
                    Return New FeatureVectorModel(name, AsScaledDoubles(column, leaf.Logical.Scale))
            End Select
        End If

        Select Case physical
            Case Format.ParquetType.[BOOLEAN]
                Return New FeatureVectorModel(name, Fill(Of Boolean)(column))
            Case Format.ParquetType.INT32
                If hasNulls Then
                    Return New FeatureVectorModel(name, AsDoublesWithNaN(column))
                End If

                Return New FeatureVectorModel(name, Fill(Of Integer)(column))
            Case Format.ParquetType.INT64
                If hasNulls Then
                    Return New FeatureVectorModel(name, AsDoublesWithNaN(column))
                End If

                Return New FeatureVectorModel(name, Fill(Of Long)(column))
            Case Format.ParquetType.FLOAT
                Return New FeatureVectorModel(name, AsSingles(column, hasNulls))
            Case Format.ParquetType.[DOUBLE]
                Return New FeatureVectorModel(name, AsDoubles(column, hasNulls))
            Case Else
                Return New FeatureVectorModel(name, AsStrings(column))
        End Select
    End Function

    Private Function Fill(Of T)(column As Columns.DataColumn) As T()
        Dim count As Integer = column.Count
        Dim result As T() = New T(count - 1) {}

        If column.DefinedCount = count AndAlso column.Values IsNot Nothing AndAlso column.Values.Length = count Then
            Return DirectCast(column.Values, T())
        End If

        Dim valueIndex As Integer = 0

        For i As Integer = 0 To count - 1
            If column.DefinitionLevels(i) >= column.MaxDefinitionLevel Then
                result(i) = CType(column.Values.GetValue(valueIndex), T)
                valueIndex += 1
            End If
        Next

        Return result
    End Function

    Private Function AsStrings(column As Columns.DataColumn) As String()
        Dim count As Integer = column.Count
        Dim result As String() = New String(count - 1) {}
        Dim valueIndex As Integer = 0

        For i As Integer = 0 To count - 1
            If column.DefinitionLevels(i) >= column.MaxDefinitionLevel Then
                Dim value As Object = column.Values.GetValue(valueIndex)

                valueIndex += 1

                If TypeOf value Is Byte() Then
                    result(i) = Encoding.UTF8.GetString(CType(value, Byte()))
                Else
                    result(i) = Convert.ToString(value)
                End If
            End If
        Next

        Return result
    End Function

    Private Function AsDoubles(column As Columns.DataColumn, hasNulls As Boolean) As Double()
        If Not hasNulls Then
            Return CType(column.Values, Double())
        End If

        Return AsDoublesWithNaN(column)
    End Function

    Private Function AsSingles(column As Columns.DataColumn, hasNulls As Boolean) As Single()
        If Not hasNulls Then
            Return CType(column.Values, Single())
        End If

        Dim count As Integer = column.Count
        Dim result As Single() = New Single(count - 1) {}
        Dim valueIndex As Integer = 0

        For i As Integer = 0 To count - 1
            If column.DefinitionLevels(i) >= column.MaxDefinitionLevel Then
                result(i) = Convert.ToSingle(column.Values.GetValue(valueIndex))
                valueIndex += 1
            Else
                result(i) = Single.NaN
            End If
        Next

        Return result
    End Function

    Private Function AsDoublesWithNaN(column As Columns.DataColumn) As Double()
        Dim count As Integer = column.Count
        Dim result As Double() = New Double(count - 1) {}
        Dim valueIndex As Integer = 0

        For i As Integer = 0 To count - 1
            If column.DefinitionLevels(i) >= column.MaxDefinitionLevel Then
                result(i) = Convert.ToDouble(column.Values.GetValue(valueIndex))
                valueIndex += 1
            Else
                result(i) = Double.NaN
            End If
        Next

        Return result
    End Function

    Private Function AsScaledDoubles(column As Columns.DataColumn, scale As Integer) As Double()
        Dim data As Double() = AsDoublesWithNaN(column)
        Dim factor As Double = System.Math.Pow(10, scale)

        For i As Integer = 0 To data.Length - 1
            If Not Double.IsNaN(data(i)) Then
                data(i) = data(i) / factor
            End If
        Next

        Return data
    End Function

End Module
