Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data

''' <summary>
''' 统一二维表（<see cref="NumericTable"/>）的构造与访问模块。
''' <para>
''' 表格的布局约定是「行名 + 特征列 + ``label:`` 前缀的标签列」，所有读写与分析函数
''' 都基于这一个载体，因此读文件得到的表可以直接进入分析与绘图流程。
''' </para>
''' <para>
''' 参数与返回值只使用脚本桥接可以转换的类型（数组 / 数值 / 字符串 / 具体类），
''' 因此 VBS 与 NJS 都可以直接调用。
''' </para>
''' </summary>
Public Module table

    ' ======================================================== 构造

    ''' <summary>由行名 + 行主序的特征矩阵构造表格。</summary>
    Public Function fromRows(rowNames As String(), features As Double()(), Optional featureNames As String() = Nothing) As NumericTable
        Return NumericTable.FromRows(rowNames, features, featureNames)
    End Function

    ''' <summary>
    ''' 由特征矩阵构造表格；行名与列名缺省时自动生成 ``sample_i`` / ``x_i``。
    ''' </summary>
    Public Function fromMatrix(m As Double()(), Optional rowNames As String() = Nothing, Optional featureNames As String() = Nothing) As NumericTable
        If m Is Nothing OrElse m.Length = 0 Then
            Throw New ArgumentException("the feature matrix is empty", NameOf(m))
        End If

        Dim names As String() = If(rowNames, uniqueNames("sample", m.Length))
        Dim columns As String() = If(featureNames, uniqueNames("x", m(0).Length))

        Return NumericTable.FromRows(names, m, columns)
    End Function

    ''' <summary>由列向量集合构造表格（列主序输入，内部转换为行主序）。</summary>
    Public Function fromColumns(featureNames As IEnumerable(Of String), columns As Double()(), Optional rowNames As IEnumerable(Of String) = Nothing) As NumericTable
        If columns Is Nothing OrElse columns.Length = 0 Then
            Throw New ArgumentException("the column collection is empty", NameOf(columns))
        End If

        Dim n As Integer = columns(0).Length
        Dim features(n - 1)() As Double

        For i As Integer = 0 To n - 1
            Dim row(columns.Length - 1) As Double

            For j As Integer = 0 To columns.Length - 1
                row(j) = columns(j)(i)
            Next

            features(i) = row
        Next

        Return NumericTable.FromRows(If(rowNames, uniqueNames("sample", n)), features, featureNames)
    End Function

    ' ======================================================== 取值

    ''' <summary>取出完整的特征矩阵（行主序，不复制）。</summary>
    ''' 
    <Extension>
    Public Function matrix(source As NumericTable) As Double()()
        Return source.features
    End Function

    ''' <summary>取出一列特征数据，列不存在时抛出异常。</summary>
    ''' 
    <Extension>
    Public Function feature(source As NumericTable, name As String) As Double()
        Return source.Feature(name)
    End Function

    ''' <summary>判断表格中是否存在指定的特征列。</summary>
    ''' 
    <Extension>
    Public Function hasFeature(source As NumericTable, name As String) As Boolean
        Return source.HasFeature(name)
    End Function

    ''' <summary>判断表格中是否存在指定的标签列。</summary>
    ''' 
    <Extension>
    Public Function hasLabel(source As NumericTable, name As String) As Boolean
        Return source.HasLabel(name)
    End Function

    ''' <summary>按名称取出标签列。</summary>
    ''' 
    <Extension>
    Public Function label(source As NumericTable, name As String) As Double()
        Return source.GetLabel(name)
    End Function

    ''' <summary>按序号取出标签列（序号从 0 开始）。</summary>
    Public Function labelAt(source As NumericTable, index As Integer) As Double()
        Return source.GetLabel(index)
    End Function

    ''' <summary>按序号取出标签列并转换为整数（用于分类/聚类结果）。</summary>
    ''' 
    <Extension>
    Public Function classLabelAt(source As NumericTable, index As Integer) As Integer()
        Return source.GetClassLabel(index)
    End Function

    ''' <summary>取出完整的标签矩阵。</summary>
    ''' 
    <Extension>
    Public Function labels(source As NumericTable) As Double()()
        Return source.labels
    End Function

    ''' <summary>标签列的名称列表。</summary>
    ''' 
    <Extension>
    Public Function labelNames(source As NumericTable) As String()
        Return source.labelNames
    End Function

    ''' <summary>特征列的名称列表。</summary>
    ''' 
    <Extension>
    Public Function featureNames(source As NumericTable) As String()
        Return source.featureNames
    End Function

    ''' <summary>行名列表。</summary>
    ''' 
    <Extension>
    Public Function rowNames(source As NumericTable) As String()
        Return source.rowNames
    End Function

    ''' <summary>样本数量。</summary>
    ''' 
    <Extension>
    Public Function nrows(source As NumericTable) As Integer
        Return source.nsamples
    End Function

    ''' <summary>特征列数量。</summary>
    ''' 
    <Extension>
    Public Function ncols(source As NumericTable) As Integer
        Return source.nfeatures
    End Function

    ' ======================================================== 修改

    ''' <summary>写入（或覆盖）一个数值标签列，返回原表。</summary>
    ''' 
    <Extension>
    Public Function setLabel(source As NumericTable, name As String, values As Double()) As NumericTable
        Return source.SetLabel(name, values)
    End Function

    ''' <summary>写入（或覆盖）一个整数标签列（分类编号 / 聚类编号），返回原表。</summary>
    ''' 
    <Extension>
    Public Function setClassLabel(source As NumericTable, name As String, values As Integer()) As NumericTable
        Return source.SetLabel(name, values)
    End Function

    ''' <summary>读取聚类结果标签列并转换为整数数组。</summary>
    ''' 
    <Extension>
    Public Function classLabels(source As NumericTable, Optional name As String = "cluster") As Integer()
        Return source.ClusterLabels(name)
    End Function

    ''' <summary>重写行名，返回原表。</summary>
    ''' 
    <Extension>
    Public Function setRowNames(source As NumericTable, names As String()) As NumericTable
        If names Is Nothing OrElse names.Length <> source.nsamples Then
            Throw New ArgumentException($"the row name count should equals the sample count ({source.nsamples})", NameOf(names))
        End If

        source.rowNames = names

        Return source
    End Function

    ''' <summary>删除一个标签列。</summary>
    Public Function removeLabel(source As NumericTable, name As String) As Boolean
        Return source.RemoveLabel(name)
    End Function

    ''' <summary>按列名选择特征列，返回新表（标签列全部保留）。</summary>
    Public Function columns(source As NumericTable, names As String()) As NumericTable
        Return source.Select(names)
    End Function

    ''' <summary>按行号选择样本，返回新表。</summary>
    Public Function rows(source As NumericTable, indices As Integer()) As NumericTable
        Return source.Slice(indices)
    End Function

    ''' <summary>取连续的若干行，返回新表。</summary>
    Public Function slice(source As NumericTable, start As Integer, count As Integer) As NumericTable
        Dim index(count - 1) As Integer

        For i As Integer = 0 To count - 1
            index(i) = start + i
        Next

        Return source.Slice(index)
    End Function

    ''' <summary>深拷贝一张表。</summary>
    Public Function clone(source As NumericTable) As NumericTable
        Return source.Clone()
    End Function

    ''' <summary>
    ''' 把行名改写为唯一的名字（``prefix + 序号``），返回原表。
    ''' <para>
    ''' 层次聚类、距离矩阵等算法要求行名唯一，而真实数据的行名常常重复
    ''' （例如 iris 数据集里的物种名），加载后先调用本函数即可。
    ''' </para>
    ''' </summary>
    Public Function uniqueRowNames(source As NumericTable, Optional prefix As String = "sample_", Optional start As Integer = 1) As NumericTable
        Dim names As String() = New String(source.nsamples - 1) {}

        For i As Integer = 0 To names.Length - 1
            names(i) = prefix & (start + i)
        Next

        source.rowNames = names

        Return source
    End Function

    ''' <summary>行名是否唯一。</summary>
    Public Function hasUniqueRowNames(source As NumericTable) As Boolean
        Dim names As String() = source.RowNamesOrDefault

        Return names.Distinct.Count = names.Length
    End Function

    ' ======================================================== 辅助

    ''' <summary>
    ''' 生成 ``prefix + 序号`` 形式的名称列表，例如 <c>names("p", 3)</c> 得到 <c>p_1, p_2, p_3</c>。
    ''' 这是 VBS 脚本魔法函数 ``fieldname`` 的标准库等价物。
    ''' </summary>
    Public Function names(prefix As String, n As Integer, Optional start As Integer = 1, Optional sep As String = "_") As String()
        ' names 与 NumericTable 的实例成员没有冲突；用局部变量避免与参数 n 混淆
        Dim out(n - 1) As String

        For i As Integer = 0 To n - 1
            out(i) = prefix & sep & (start + i)
        Next

        Return out
    End Function

    ''' <summary>把一张表的标签列复制到另一张行数相同的表上。</summary>
    Public Function copyLabels(target As NumericTable, source As NumericTable) As NumericTable
        Dim values As Double()() = source.labels

        If values Is Nothing Then
            Return target
        End If

        For i As Integer = 0 To source.labelNames.Length - 1
            ' the lambda must not capture the loop variable, so copy it to a local first
            Dim column As Integer = i
            Dim columnValues As Double() = values.Select(Function(row) row(column)).ToArray

            Call target.SetLabel(source.labelNames(i), columnValues)
        Next

        Return target
    End Function

    Private Function uniqueNames(prefix As String, n As Integer) As String()
        Return names(prefix, n)
    End Function
End Module
