#Region "Microsoft.VisualBasic::a2e5ad90b454fc2164e36be382059fdd, Microsoft.VisualBasic.Core\src\Data\NumericTableExtensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 565
    '    Code Lines: 318 (56.28%)
    ' Comment Lines: 161 (28.50%)
    '    - Xml Docs: 92.55%
    ' 
    '   Blank Lines: 86 (15.22%)
    '     File Size: 23.34 KB


    '     Module NumericTableExtensions
    ' 
    '         Function: ClusterLabels, ColumnNames, ColumnVectors, dataframe, ElementType
    '                   EmptyTable, EncodeColumn, EncodeFactor, EnumerateColumn, FillNaN
    '                   GroupByLabel, IsScalarType, LabelMatrix, LevelToken, name
    '                   NumericRows, RowIds, SingleColumn, TakeColumns, TakeRows
    '                   ToEmbeddingTable, ToNumeric, Transpose, WithCluster
    ' 
    '         Sub: CheckDuplicated
    '         Structure EncodedColumn
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections
Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.ValueTypes

Namespace Data

    ''' <summary>
    ''' <see cref="NumericTable"/> 的通用扩展方法集合，主要提供给聚类、
    ''' 回归等分析算法进行数据访问与结果回写。
    ''' </summary>
    <HideModuleName>
    Public Module NumericTableExtensions

        ''' <summary>
        ''' 标签列的缺省列名前缀。
        ''' 
        ''' 该取值与 <c>Microsoft.VisualBasic.Data.Framework</c> 程序集之中的
        ''' <c>NumericTableIO.DefaultLabelPrefix</c> 保持一致；由于本模块位于
        ''' <c>Microsoft.VisualBasic.Runtime</c> 程序集，而两个程序集之间没有引用关系，
        ''' 因此在这里本地定义同样取值的常量。
        ''' </summary>
        Public Const DefaultLabelPrefix As String = "label:"

        ''' <summary>
        ''' 因子展开的时候，用于表示缺失值的水平名称
        ''' </summary>
        Private Const MissingLevel As String = "<NA>"

        Public Function name(x As String) As ArgumentReference
            Return New ArgumentReference With {.name = x}
        End Function

        ''' <summary>
        ''' Create a numeric table by columns
        ''' </summary>
        ''' <param name="cols">value cast rules:
        ''' 
        ''' 1. any numeric type: double
        ''' 2. boolean: 1 = true, 0 = false
        ''' 3. enum type: enum base type to double
        ''' 4. date type: unix time stamp (seconds)
        ''' 5. timespan type: total seconds
        ''' 6. string, char type: make binary columns, the factor levels are ordered by alphabet order
        ''' 
        ''' collection type of the column field maybe array, list, linq enumerable
        '''
        ''' the column field its name starts with the <see cref="DefaultLabelPrefix"/> prefix
        ''' will be treated as a label column, and the prefix will be removed from the label name
        ''' </param>
        ''' <returns></returns>
        ''' <example>
        ''' Dim x = dataframe(
        '''    name("field1") = {1,3,5,7,9},
        '''    name("field2") = {2,4,6,8,0},
        '''    name("flags") = {True, False, True, True, False},
        '''    name("factors") = {"a","b","c","c","b"},
        '''    name("label:class") = {1,2,2,2,3}
        ''' )
        ''' 
        ''' ' feature matrix:
        ''' ' field1: integer cast to double
        ''' ' field2: integer cast to double
        ''' ' flags: boolean map to double
        ''' ' factors: factor binarized, make columns
        ''' '    factors:a  {1,0,0,0,0}
        ''' '    factors:b  {0,1,0,0,1}
        ''' '    factors:c  {0,0,1,1,0}
        ''' '
        ''' ' label matrix:
        ''' ' class: integer cast to double
        ''' </example>
        Public Function dataframe(ParamArray cols As ArgumentReference()) As NumericTable
            If cols Is Nothing OrElse cols.Length = 0 Then
                Return EmptyTable()
            End If

            Dim featureCols As New List(Of EncodedColumn)
            Dim labelCols As New List(Of EncodedColumn)
            Dim nsamples As Integer = -1

            For i As Integer = 0 To cols.Length - 1
                Dim col As ArgumentReference = cols(i)

                If col Is Nothing Then
                    Throw New InvalidConstraintException($"the column at index {i} is Nothing!")
                End If
                If String.IsNullOrWhiteSpace(col.name) Then
                    Throw New InvalidConstraintException($"the column name at index {i} is empty!")
                End If

                Dim values As Object() = EnumerateColumn(col.Value)

                If nsamples < 0 Then
                    nsamples = values.Length
                ElseIf values.Length <> nsamples Then
                    Throw New InvalidConstraintException(
                        $"the column '{col.name}' has {values.Length} values, but the expected sample size is {nsamples}!"
                    )
                End If

                If col.name.StartsWith(DefaultLabelPrefix, StringComparison.Ordinal) Then
                    Dim labelName As String = col.name.Substring(DefaultLabelPrefix.Length)

                    If String.IsNullOrWhiteSpace(labelName) Then
                        Throw New InvalidConstraintException(
                            $"the label column '{col.name}' has no label name after the '{DefaultLabelPrefix}' prefix!"
                        )
                    End If

                    Call labelCols.Add(EncodeColumn(labelName, values))
                Else
                    Call featureCols.Add(EncodeColumn(col.name, values))
                End If
            Next

            If nsamples <= 0 Then
                Return EmptyTable()
            End If

            Dim featureNames As String() = ColumnNames(featureCols)
            Dim labelNames As String() = ColumnNames(labelCols)
            Dim featureVectors As Double()() = ColumnVectors(featureCols)
            Dim labelVectors As Double()() = ColumnVectors(labelCols)

            Call CheckDuplicated(featureNames, "feature")
            Call CheckDuplicated(labelNames, "label")

            Dim table As New NumericTable(Transpose(featureVectors, nsamples), Nothing, featureNames) With {
                .labels = If(labelVectors.Length = 0, Nothing, Transpose(labelVectors, nsamples)),
                .labelNames = If(labelVectors.Length = 0, Nothing, labelNames)
            }

            Call table.Validate()

            Return table
        End Function

        ''' <summary>
        ''' 获取行主序的特征矩阵。默认直接返回表内部的矩阵引用以避免不必要的拷贝，
        ''' 需要隔离数据的时候将 <paramref name="deepcopy"/> 设置为 True。
        ''' </summary>
        <Extension>
        Public Function NumericRows(table As NumericTable, Optional deepcopy As Boolean = False) As Double()()
            If table Is Nothing Then
                Return Nothing
            End If

            Return DirectCast(table, INumericMatrix).ArrayPack(deepcopy)
        End Function

        ''' <summary>
        ''' 获取标签矩阵。可能为 Nothing，表示当前表还没有任何标签列。
        ''' </summary>
        <Extension>
        Public Function LabelMatrix(table As NumericTable) As Double()()
            If table Is Nothing Then
                Return Nothing
            Else
                Return table.labels
            End If
        End Function

        ''' <summary>
        ''' 获取行名/样本 ID 数组（缺省时自动生成 ``1..n`` 序号）
        ''' </summary>
        <Extension>
        Public Function RowIds(table As NumericTable) As String()
            If table Is Nothing Then
                Return New String() {}
            Else
                Return table.RowNamesOrDefault()
            End If
        End Function

        ''' <summary>
        ''' 将聚类结果（类编号）写入标签矩阵并返回当前表对象，以支持链式调用
        ''' </summary>
        <Extension>
        Public Function WithCluster(table As NumericTable, cluster As IEnumerable(Of Integer), Optional name As String = "cluster") As NumericTable
            Return table.SetLabel(name, cluster.SafeQuery.ToArray)
        End Function

        ''' <summary>
        ''' 读取聚类结果标签列（缺省列名 ``cluster``）
        ''' </summary>
        <Extension>
        Public Function ClusterLabels(table As NumericTable, Optional name As String = "cluster") As Integer()
            Return table.GetLabel(name).Select(Function(d) CInt(d)).ToArray
        End Function

        ''' <summary>
        ''' 行投影（等价于 <see cref="NumericTable.Slice"/>）
        ''' </summary>
        <Extension>
        Public Function TakeRows(table As NumericTable, rows As IEnumerable(Of Integer)) As NumericTable
            Return table.Slice(rows)
        End Function

        ''' <summary>
        ''' 特征列投影（等价于 <see cref="NumericTable.Select"/>）
        ''' </summary>
        <Extension>
        Public Function TakeColumns(table As NumericTable, columns As IEnumerable(Of String)) As NumericTable
            Return table.Select(columns)
        End Function

        ''' <summary>
        ''' 以指定标签列的值作为分组键，对表格中的行下标进行分组
        ''' </summary>
        <Extension>
        Public Function GroupByLabel(table As NumericTable, name As String) As Dictionary(Of Double, Integer())
            Dim keys As Double() = table.GetLabel(name)

            Return keys _
                .SeqIterator _
                .GroupBy(Function(x) x.value) _
                .ToDictionary(Function(g) g.Key,
                              Function(g)
                                  Return g.Select(Function(x) x.i).ToArray
                              End Function)
        End Function

        ''' <summary>
        ''' 将降维/嵌入算法的结果矩阵封装为一个新的二维表。
        ''' 
        ''' 新的表以嵌入坐标作为特征矩阵（列名 ``{prefix}_1 .. {prefix}_n``），
        ''' 并继承源表的行名、标签矩阵以及名称/描述信息，因此生成的结果表可以
        ''' 直接送入聚类等后续分析入口进行链式调用。
        ''' </summary>
        ''' <param name="source">产生该嵌入结果的原始二维表</param>
        ''' <param name="embedding">行主序的嵌入坐标矩阵，行数必须与源表的样本数一致</param>
        ''' <param name="prefix">嵌入维度列的名称前缀，缺省为 ``dim``</param>
        ''' <returns>以嵌入坐标为特征的新二维表</returns>
        <Extension>
        Public Function ToEmbeddingTable(source As NumericTable,
                                         embedding As Double()(),
                                         Optional prefix As String = "dim") As NumericTable

            If source Is Nothing Then
                Throw New ArgumentNullException(NameOf(source))
            End If
            If embedding Is Nothing Then
                Throw New ArgumentNullException(NameOf(embedding))
            End If
            If embedding.Length <> source.nsamples Then
                Throw New InvalidConstraintException(
                    $"the embedding rows {embedding.Length} is not equals to the source table samples {source.nsamples}!"
                )
            End If

            Dim dims As Integer = If(embedding.Length = 0 OrElse embedding(0) Is Nothing, 0, embedding(0).Length)
            Dim featureNames As String() = Enumerable _
                .Range(1, dims) _
                .Select(Function(i) $"{prefix}_{i}") _
                .ToArray

            Return New NumericTable(embedding, source.RowNamesOrDefault(), featureNames) With {
                .labels = source.labels,
                .labelNames = source.labelNames,
                .name = source.name,
                .description = source.description
            }
        End Function

#Region "column encoding helpers"

        ''' <summary>
        ''' 单列编码结果。
        ''' 
        ''' 数值/布尔/枚举/日期/时间间隔类型的列只会展开为一列；而字符串、字符以及其它
        ''' 未知类型的列会按照水平值展开成多列（one-hot 二进制编码），因此这里使用数组
        ''' 来承载展开之后的列名与列向量。
        ''' </summary>
        Private Structure EncodedColumn
            ''' <summary>
            ''' 展开之后的列名集合
            ''' </summary>
            Public ReadOnly names As String()
            ''' <summary>
            ''' 展开之后的列向量集合，和 <see cref="names"/> 一一对应
            ''' </summary>
            Public ReadOnly columns As Double()()

            Sub New(names As String(), columns As Double()())
                Me.names = names
                Me.columns = columns
            End Sub
        End Structure

        ''' <summary>
        ''' 把一个列值统一枚举为 <see cref="Object"/> 数组，以支持数组、<c>List(Of T)</c>
        ''' 以及任意的 LINQ 可枚举序列。标量值（含字符串、字符）会被包装成单元素列。
        ''' </summary>
        Private Function EnumerateColumn(value As Object) As Object()
            If value Is Nothing Then
                Return New Object() {}
            End If
            If TypeOf value Is String OrElse TypeOf value Is Char Then
                ' 字符串/字符本身是标量的因子值，而不是字符序列
                Return New Object() {value}
            End If

            Dim sequence As IEnumerable = TryCast(value, IEnumerable)

            If sequence Is Nothing Then
                Return New Object() {value}
            End If

            Dim items As New List(Of Object)

            For Each item As Object In sequence
                Call items.Add(item)
            Next

            Return items.ToArray
        End Function

        ''' <summary>
        ''' 编码一列数据，返回结果可能包含一个或者多个数值列。
        ''' </summary>
        ''' <param name="columnName">去掉 <see cref="DefaultLabelPrefix"/> 前缀之后的列名</param>
        ''' <param name="values">列数据</param>
        Private Function EncodeColumn(columnName As String, values As Object()) As EncodedColumn
            Dim columnType As Type = ElementType(values)

            If columnType Is Nothing Then
                ' 整列都是 Nothing：作为缺失值处理
                Return SingleColumn(columnName, FillNaN(values.Length))
            ElseIf IsScalarType(columnType) Then
                Dim vector As Double() = New Double(values.Length - 1) {}

                For i As Integer = 0 To values.Length - 1
                    vector(i) = ToNumeric(values(i), columnName, i)
                Next

                Return SingleColumn(columnName, vector)
            Else
                ' 字符串、字符以及其它未知类型都作为分类因子进行二进制展开
                Return EncodeFactor(columnName, values)
            End If
        End Function

        ''' <summary>
        ''' 取列之中第一个非 Nothing 元素的运行时类型作为该列的类型
        ''' </summary>
        Private Function ElementType(values As Object()) As Type
            If values Is Nothing Then
                Return Nothing
            End If

            For i As Integer = 0 To values.Length - 1
                If values(i) IsNot Nothing Then
                    Return values(i).GetType
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 该类型是否可以直接转换成数值（数值/布尔/枚举/日期/时间间隔）？
        ''' </summary>
        Private Function IsScalarType(columnType As Type) As Boolean
            If columnType Is Nothing Then
                Return False
            End If
            If columnType.IsEnum Then
                Return True
            End If

            Select Case Type.GetTypeCode(columnType)
                Case TypeCode.Byte, TypeCode.SByte,
                     TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32,
                     TypeCode.Int64, TypeCode.UInt64,
                     TypeCode.Single, TypeCode.Double,
                     TypeCode.Decimal, TypeCode.Boolean,
                     TypeCode.DateTime
                    Return True
                Case Else
                    Return columnType.Equals(GetType(TimeSpan))
            End Select
        End Function

        ''' <summary>
        ''' 按照数据类型把单元格的值转换成数值：
        ''' 
        ''' 1. 数值类型：直接转 <see cref="Double"/>
        ''' 2. 布尔：True = 1，False = 0
        ''' 3. 枚举：按照底层基类型转换成数值
        ''' 4. 日期：转换成 Unix 时间戳（秒）
        ''' 5. 时间间隔：转换成总秒数
        ''' 6. 其它的情况（包括 Nothing）：缺失值返回 <see cref="Double.NaN"/>
        ''' </summary>
        Private Function ToNumeric(value As Object, columnName As String, i As Integer) As Double
            If value Is Nothing Then
                Return Double.NaN
            End If

            Dim valueType As Type = value.GetType

            If valueType.IsEnum Then
                Dim baseType As Type = [Enum].GetUnderlyingType(valueType)

                Return Convert.ToDouble(Convert.ChangeType(value, baseType, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            ElseIf valueType Is GetType(Boolean) Then
                Return If(DirectCast(value, Boolean), 1.0, 0.0)
            ElseIf valueType Is GetType(Date) Then
                Return DirectCast(value, Date).UnixTimeStamp
            ElseIf valueType Is GetType(TimeSpan) Then
                Return DirectCast(value, TimeSpan).TotalSeconds
            End If

            Try
                Return Convert.ToDouble(value, CultureInfo.InvariantCulture)
            Catch ex As Exception
                Throw New InvalidConstraintException(
                    $"the cell value '{value}' at row {i + 1} of the column '{columnName}' cannot be converted to a numeric value!",
                    ex
                )
            End Try
        End Function

        ''' <summary>
        ''' 构造一个单列的编码结果
        ''' </summary>
        Private Function SingleColumn(name As String, vector As Double()) As EncodedColumn
            Return New EncodedColumn(New String() {name}, New Double()() {vector})
        End Function

        ''' <summary>
        ''' 生成一个全部为 <see cref="Double.NaN"/> 的列向量
        ''' </summary>
        Private Function FillNaN(len As Integer) As Double()
            Dim vector As Double() = New Double(len - 1) {}

            For i As Integer = 0 To len - 1
                vector(i) = Double.NaN
            Next

            Return vector
        End Function

        ''' <summary>
        ''' 把字符串、字符等分类因子展开成 one-hot 二进制列。
        ''' 
        ''' 水平值按照 <see cref="StringComparer.Ordinal"/> 字母序排序，生成的列名形如
        ''' ``{列名}:{水平值}``；Nothing 以及空字符串统一归入缺失水平 ``&lt;NA&gt;``。
        ''' </summary>
        Private Function EncodeFactor(columnName As String, values As Object()) As EncodedColumn
            Dim tokens As String() = New String(values.Length - 1) {}
            Dim levels As New SortedSet(Of String)(StringComparer.Ordinal)

            For i As Integer = 0 To values.Length - 1
                tokens(i) = LevelToken(values(i))
                Call levels.Add(tokens(i))
            Next

            Dim levelList As String() = levels.ToArray
            Dim names As String() = New String(levelList.Length - 1) {}
            Dim columns As Double()() = New Double(levelList.Length - 1)() {}

            For j As Integer = 0 To levelList.Length - 1
                Dim level As String = levelList(j)
                Dim vector As Double() = New Double(values.Length - 1) {}

                names(j) = $"{columnName}:{level}"

                For i As Integer = 0 To values.Length - 1
                    vector(i) = If(String.Equals(tokens(i), level, StringComparison.Ordinal), 1.0, 0.0)
                Next

                columns(j) = vector
            Next

            Return New EncodedColumn(names, columns)
        End Function

        ''' <summary>
        ''' 取因子水平的文本表示，Nothing 与空字符串都会被映射为缺失水平
        ''' </summary>
        Private Function LevelToken(value As Object) As String
            If value Is Nothing Then
                Return MissingLevel
            ElseIf TypeOf value Is String Then
                Dim text As String = DirectCast(value, String)

                Return If(text.Length = 0, MissingLevel, text)
            Else
                Return value.ToString()
            End If
        End Function

        ''' <summary>
        ''' 把列向量集合转置成行主序的二维矩阵
        ''' </summary>
        Private Function Transpose(columns As Double()(), nsamples As Integer) As Double()()
            Dim matrix As Double()() = New Double(nsamples - 1)() {}

            For i As Integer = 0 To nsamples - 1
                Dim row As Double() = New Double(columns.Length - 1) {}

                For j As Integer = 0 To columns.Length - 1
                    row(j) = columns(j)(i)
                Next

                matrix(i) = row
            Next

            Return matrix
        End Function

        ''' <summary>
        ''' 取出全部编码结果的列名
        ''' </summary>
        Private Function ColumnNames(columns As List(Of EncodedColumn)) As String()
            Dim names As New List(Of String)

            For Each col As EncodedColumn In columns
                Call names.AddRange(col.names)
            Next

            Return names.ToArray
        End Function

        ''' <summary>
        ''' 取出全部编码结果的列向量
        ''' </summary>
        Private Function ColumnVectors(columns As List(Of EncodedColumn)) As Double()()
            Dim vectors As New List(Of Double())

            For Each col As EncodedColumn In columns
                Call vectors.AddRange(col.columns)
            Next

            Return vectors.ToArray
        End Function

        ''' <summary>
        ''' 检测展开之后是否存在重复的列名，重复的列名会导致下游按名称取列出现歧义
        ''' </summary>
        Private Sub CheckDuplicated(names As String(), kind As String)
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)

            For Each name As String In names
                If Not seen.Add(name) Then
                    Throw New InvalidConstraintException($"duplicated {kind} column name '{name}' was detected!")
                End If
            Next
        End Sub

        ''' <summary>
        ''' 创建一个不包含任何数据的空表
        ''' </summary>
        Private Function EmptyTable() As NumericTable
            Return New NumericTable(New Double()() {}, New String() {}, New String() {})
        End Function

#End Region
    End Module
End Namespace
