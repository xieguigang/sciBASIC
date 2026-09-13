#Region "Microsoft.VisualBasic::NumericTable, Microsoft.VisualBasic.Core\src\Data\NumericTable.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace Data

    ''' <summary>
    ''' 经过预处理之后的纯数值二维表对象。
    ''' 
    ''' 该对象被设计为整个 sciBASIC# 框架之中的统一数据载体：表中存放的
    ''' 逻辑值、字符串枚举标签、缺失值等内容都应该在上游的预处理阶段被
    ''' 转换成数值，因此算法层只需要面对纯数值矩阵即可直接进行计算。
    ''' 
    ''' 表结构：
    ''' 
    ''' 1. <see cref="rowNames"/>：样本 ID / 行名
    ''' 2. <see cref="features"/> + <see cref="featureNames"/>：样本特征矩阵（行主序）
    ''' 3. <see cref="labels"/> + <see cref="labelNames"/>：标签矩阵（行主序）
    '''
    ''' 聚类、回归等分析所产生的结果统一通过 <see cref="SetLabel"/> 写入
    ''' <see cref="labels"/> 标签矩阵，从而保证所有算法共享同一种数据输入与输出。
    ''' </summary>
    Public Class NumericTable
        Implements INumericMatrix
        Implements ILabeledMatrix
        Implements IEnumerable(Of Double())

        ''' <summary>
        ''' the name of current table object
        ''' </summary>
        ''' <returns></returns>
        Public Property name As String
        ''' <summary>
        ''' the comment text about this table
        ''' </summary>
        ''' <returns></returns>
        Public Property description As String

        ''' <summary>
        ''' 行 ID / 样本名。如果这个属性为 Nothing 或者长度和样本数量不一致，
        ''' 则会使用 ``1..n`` 的序号作为缺省的行名。
        ''' </summary>
        ''' <returns></returns>
        Public Property rowNames As String()

        ''' <summary>
        ''' 样本特征矩阵的列名
        ''' </summary>
        ''' <returns></returns>
        Public Property featureNames As String()

        ''' <summary>
        ''' 样本特征矩阵，行主序的二维数组，即 ``features(sample)(feature)``
        ''' </summary>
        ''' <returns></returns>
        Public Property features As Double()()

        ''' <summary>
        ''' 标签矩阵的列名。聚类结果的类标签列也会出现在这里
        ''' </summary>
        ''' <returns></returns>
        Public Property labelNames As String()

        ''' <summary>
        ''' 标签矩阵，行主序的二维数组，即 ``labels(sample)(label)``
        ''' </summary>
        ''' <returns></returns>
        Public Property labels As Double()()

        ''' <summary>
        ''' 样本的数量（矩阵的行数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property nsamples As Integer
            Get
                Return If(features Is Nothing, 0, features.Length)
            End Get
        End Property

        ''' <summary>
        ''' 特征的数量（特征矩阵的列数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property nfeatures As Integer
            Get
                Return FeatureWidth(features)
            End Get
        End Property

        ''' <summary>
        ''' 标签列的数量（标签矩阵的列数）
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property nlabels As Integer
            Get
                Return FeatureWidth(labels)
            End Get
        End Property

        ''' <summary>
        ''' 是否当前表中没有任何特征数据？
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isEmpty As Boolean
            Get
                Return nsamples = 0
            End Get
        End Property

        ''' <summary>
        ''' 获取指定行的特征向量数据
        ''' </summary>
        ''' <param name="i"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(i As Integer) As Double()
            Get
                If features Is Nothing Then
                    Return Nothing
                Else
                    Return features(i)
                End If
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' 从特征矩阵构造一个二维表对象
        ''' </summary>
        ''' <param name="features">行主序的特征矩阵</param>
        ''' <param name="rowNames">行名/样本 ID，可选</param>
        ''' <param name="featureNames">特征列名，可选</param>
        Sub New(features As Double()(), Optional rowNames As String() = Nothing, Optional featureNames As String() = Nothing)
            Me.features = features
            Me.rowNames = rowNames
            Me.featureNames = featureNames
        End Sub

        ''' <summary>
        ''' 从列向量集合构造二维表（转置为行主序）
        ''' </summary>
        ''' <param name="featureNames">特征列名</param>
        ''' <param name="columns">每一列的特征数据</param>
        ''' <param name="rowNames">行名/样本 ID，可选</param>
        Sub New(featureNames As String(), columns As IEnumerable(Of Double()), Optional rowNames As String() = Nothing)
            Me.featureNames = featureNames
            Me.rowNames = rowNames
            Me.features = ColumnsToMatrix(columns)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function FeatureWidth(mat As Double()()) As Integer
            If mat Is Nothing OrElse mat.Length = 0 Then
                Return 0
            ElseIf mat(0) Is Nothing Then
                Return 0
            Else
                Return mat(0).Length
            End If
        End Function

        Private Shared Function ColumnsToMatrix(columns As IEnumerable(Of Double())) As Double()()
            If columns Is Nothing Then
                Return Nothing
            End If

            Dim cols As Double()() = columns.ToArray

            If cols.Length = 0 Then
                Return Nothing
            End If

            Dim nc As Integer = cols.Length
            Dim nr As Integer = If(cols(0) Is Nothing, 0, cols(0).Length)
            Dim mat As Double()() = CreateMatrix(nr, nc)

            For j As Integer = 0 To nc - 1
                Dim col As Double() = cols(j)

                If col IsNot Nothing Then
                    For i As Integer = 0 To nr - 1
                        mat(i)(j) = col(i)
                    Next
                End If
            Next

            Return mat
        End Function

        ''' <summary>
        ''' 创建一个 ``rows x cols`` 的零值二维数组
        ''' </summary>
        Private Shared Function CreateMatrix(rows As Integer, cols As Integer) As Double()()
            If rows <= 0 Then
                Return New Double()() {}
            End If

            Dim mat As Double()() = New Double(rows - 1)() {}

            For i As Integer = 0 To rows - 1
                mat(i) = New Double(cols - 1) {}
            Next

            Return mat
        End Function

        ''' <summary>
        ''' 获取缺省的行名数组（当 <see cref="rowNames"/> 缺失时生成 ``1..n`` 序号）
        ''' </summary>
        ''' <returns></returns>
        Public Function RowNamesOrDefault() As String()
            Dim n As Integer = nsamples

            If rowNames IsNot Nothing AndAlso rowNames.Length = n Then
                Return rowNames
            End If

            Return Enumerable.Range(1, n).Select(Function(i) CStr(i)).ToArray
        End Function

        ''' <summary>
        ''' 获取指定行的行名/样本 ID
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetRowName(i As Integer) As String
            Return RowNamesOrDefault()(i)
        End Function

        ''' <summary>
        ''' 获取指定行的特征向量数据（等价于默认索引器）
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Row(i As Integer) As Double()
            Return Item(i)
        End Function

        ''' <summary>
        ''' 获取指定名称的特征列向量
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function Feature(name As String) As Double()
            Dim idx As Integer = RequireFeatureIndex(name)
            Dim v As Double() = New Double(nsamples - 1) {}

            For i As Integer = 0 To nsamples - 1
                v(i) = features(i)(idx)
            Next

            Return v
        End Function

        ''' <summary>
        ''' 判断是否存在指定名称的特征列
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HasFeature(name As String) As Boolean
            Return FeatureIndex(name) > -1
        End Function

        ''' <summary>
        ''' 判断是否存在指定名称的标签列
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HasLabel(name As String) As Boolean
            Return LabelIndex(name) > -1
        End Function

        Private Function FeatureIndex(name As String) As Integer
            Return IndexOfName(featureNames, name)
        End Function

        Private Function LabelIndex(name As String) As Integer
            Return IndexOfName(labelNames, name)
        End Function

        Private Shared Function IndexOfName(names As String(), name As String) As Integer
            If names Is Nothing OrElse String.IsNullOrEmpty(name) Then
                Return -1
            End If

            For i As Integer = 0 To names.Length - 1
                If String.Equals(names(i), name, StringComparison.OrdinalIgnoreCase) Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Private Function RequireFeatureIndex(name As String) As Integer
            Dim idx As Integer = FeatureIndex(name)

            If idx < 0 Then
                Throw New KeyNotFoundException($"missing feature column '{name}' in the table: {featureNames.JoinBy(", ")}")
            End If

            Return idx
        End Function

        Private Function RequireLabelIndex(name As String) As Integer
            Dim idx As Integer = LabelIndex(name)

            If idx < 0 Then
                Throw New KeyNotFoundException($"missing label column '{name}' in the table: {If(labelNames, New String() {}).JoinBy(", ")}")
            End If

            Return idx
        End Function

        ''' <summary>
        ''' 获取指定名称的标签列向量
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function GetLabel(name As String) As Double()
            Dim idx As Integer = RequireLabelIndex(name)
            Dim v As Double() = New Double(nsamples - 1) {}

            For i As Integer = 0 To nsamples - 1
                v(i) = labels(i)(idx)
            Next

            Return v
        End Function

        ''' <summary>
        ''' 尝试获取指定名称的标签列向量
        ''' </summary>
        Public Function TryGetLabel(name As String, ByRef values As Double()) As Boolean
            Dim idx As Integer = LabelIndex(name)

            If idx < 0 Then
                values = Nothing
                Return False
            End If

            values = New Double(nsamples - 1) {}

            For i As Integer = 0 To nsamples - 1
                values(i) = labels(i)(idx)
            Next

            Return True
        End Function

        ''' <summary>
        ''' 写入或者新增一列标签数据。如果指定的标签列已经存在，则覆盖其数值；
        ''' 否则在标签矩阵的末尾追加一个新的标签列。
        ''' </summary>
        ''' <param name="name">标签列的名称</param>
        ''' <param name="values">标签数据，长度必须和样本数量一致</param>
        ''' <returns>返回当前表对象以支持链式调用</returns>
        Public Function SetLabel(name As String, values As Double()) As NumericTable
            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentNullException(NameOf(name))
            End If
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If
            If values.Length <> nsamples Then
                Throw New ArgumentException($"the label column '{name}' has {values.Length} values, but the table has {nsamples} samples!", NameOf(values))
            End If

            Dim idx As Integer = LabelIndex(name)

            If idx < 0 Then
                AppendLabelColumn(name, values)
            Else
                For i As Integer = 0 To nsamples - 1
                    labels(i)(idx) = values(i)
                Next
            End If

            Return Me
        End Function

        ''' <summary>
        ''' 写入或者新增一列整数标签数据（例如聚类的类编号结果）
        ''' </summary>
        Public Function SetLabel(name As String, values As Integer()) As NumericTable
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Return SetLabel(name, values.Select(Function(x) CDbl(x)).ToArray)
        End Function

        Private Sub AppendLabelColumn(name As String, values As Double())
            Dim oldK As Integer = nlabels
            Dim newK As Integer = oldK + 1
            Dim newLabels As Double()() = CreateMatrix(nsamples, newK)
            Dim newNames As String() = New String(newK - 1) {}

            For j As Integer = 0 To oldK - 1
                newNames(j) = labelNames(j)

                For i As Integer = 0 To nsamples - 1
                    newLabels(i)(j) = labels(i)(j)
                Next
            Next

            newNames(oldK) = name

            For i As Integer = 0 To nsamples - 1
                newLabels(i)(oldK) = values(i)
            Next

            labels = newLabels
            labelNames = newNames
        End Sub

        ''' <summary>
        ''' 删除指定名称的标签列
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>是否成功的删除了一个标签列</returns>
        Public Function RemoveLabel(name As String) As Boolean
            Dim idx As Integer = LabelIndex(name)

            If idx < 0 Then
                Return False
            End If

            Dim newK As Integer = nlabels - 1

            If newK <= 0 Then
                labels = Nothing
                labelNames = Nothing
                Return True
            End If

            Dim newLabels As Double()() = CreateMatrix(nsamples, newK)
            Dim newNames As String() = New String(newK - 1) {}
            Dim d As Integer = 0

            For j As Integer = 0 To labelNames.Length - 1
                If j = idx Then
                    Continue For
                End If

                newNames(d) = labelNames(j)

                For i As Integer = 0 To nsamples - 1
                    newLabels(i)(d) = labels(i)(j)
                Next

                d += 1
            Next

            labels = newLabels
            labelNames = newNames

            Return True
        End Function

        ''' <summary>
        ''' 按行下标进行行切片，返回一个共享列名、行名子集的新表对象
        ''' </summary>
        ''' <param name="rows"></param>
        ''' <returns></returns>
        Public Function Slice(rows As IEnumerable(Of Integer)) As NumericTable
            Dim idx As Integer() = rows.SafeQuery.ToArray
            Dim names0 As String() = RowNamesOrDefault()

            If idx.Length = 0 Then
                Return New NumericTable(New Double()() {}, New String() {}, featureNames) With {
                    .labels = If(labels Is Nothing, Nothing, New Double()() {}),
                    .labelNames = labelNames,
                    .name = name,
                    .description = description
                }
            End If

            Dim outNames As String() = New String(idx.Length - 1) {}
            Dim outFeatures As Double()() = New Double(idx.Length - 1)() {}
            Dim outLabels As Double()() = If(labels Is Nothing, Nothing, New Double(idx.Length - 1)() {})

            For n As Integer = 0 To idx.Length - 1
                Dim r As Integer = idx(n)

                If r < 0 OrElse r >= nsamples Then
                    Throw New ArgumentOutOfRangeException(NameOf(rows), $"row index {r} is out of the table range [0, {nsamples - 1}]")
                End If

                outNames(n) = names0(r)
                outFeatures(n) = features(r)

                If outLabels IsNot Nothing Then
                    outLabels(n) = labels(r)
                End If
            Next

            Return New NumericTable(outFeatures, outNames, featureNames) With {
                .labels = outLabels,
                .labelNames = labelNames,
                .name = name,
                .description = description
            }
        End Function

        ''' <summary>
        ''' 特征列投影，返回仅包含指定特征列的新表对象
        ''' </summary>
        ''' <param name="features">需要保留的特征列名</param>
        ''' <returns></returns>
        Public Function [Select](features As IEnumerable(Of String)) As NumericTable
            Dim cols As String() = features.SafeQuery.ToArray
            Dim idx As Integer() = cols.Select(AddressOf RequireFeatureIndex).ToArray
            Dim outFeatures As Double()() = CreateMatrix(nsamples, cols.Length)

            For i As Integer = 0 To nsamples - 1
                For j As Integer = 0 To cols.Length - 1
                    outFeatures(i)(j) = Me.features(i)(idx(j))
                Next
            Next

            Return New NumericTable(outFeatures, rowNames, cols) With {
                .labels = labels,
                .labelNames = labelNames,
                .name = name,
                .description = description
            }
        End Function

        ''' <summary>
        ''' 深拷贝当前表对象
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone() As NumericTable
            Return New NumericTable(CopyMatrix(features), CopyArray(rowNames), CopyArray(featureNames)) With {
                .labels = If(labels Is Nothing, Nothing, CopyMatrix(labels)),
                .labelNames = CopyArray(labelNames),
                .name = name,
                .description = description
            }
        End Function

        Private Shared Function CopyArray(Of T)(v As T()) As T()
            If v Is Nothing Then
                Return Nothing
            Else
                Return DirectCast(v.Clone(), T())
            End If
        End Function

        Private Shared Function CopyMatrix(mat As Double()()) As Double()()
            If mat Is Nothing Then
                Return Nothing
            End If

            Dim copy As Double()() = New Double(mat.Length - 1)() {}

            For i As Integer = 0 To mat.Length - 1
                copy(i) = If(mat(i) Is Nothing, Nothing, DirectCast(mat(i).Clone(), Double()))
            Next

            Return copy
        End Function

        ''' <summary>
        ''' 检查当前表内部的维度是否一致，用于在计算之前做数据校验
        ''' </summary>
        Public Sub Validate()
            Dim n As Integer = nsamples
            Dim w As Integer = nfeatures

            If features Is Nothing Then
                Return
            End If

            For i As Integer = 0 To features.Length - 1
                If features(i) Is Nothing OrElse features(i).Length <> w Then
                    Throw New InvalidConstraintException($"the feature row[{i}] length is not equals to the feature dimension {w}!")
                End If
            Next

            If featureNames IsNot Nothing AndAlso featureNames.Length <> w Then
                Throw New InvalidConstraintException($"the feature name size {featureNames.Length} is not equals to the feature dimension {w}!")
            End If

            If labels Is Nothing Then
                Return
            End If

            If labels.Length <> n Then
                Throw New InvalidConstraintException($"the label row size {labels.Length} is not equals to the sample size {n}!")
            End If

            Dim k As Integer = nlabels

            For i As Integer = 0 To labels.Length - 1
                If labels(i) Is Nothing OrElse labels(i).Length <> k Then
                    Throw New InvalidConstraintException($"the label row[{i}] length is not equals to the label dimension {k}!")
                End If
            Next

            If labelNames IsNot Nothing AndAlso labelNames.Length <> k Then
                Throw New InvalidConstraintException($"the label name size {labelNames.Length} is not equals to the label dimension {k}!")
            End If
        End Sub

        ''' <summary>
        ''' 从行主序的特征矩阵构造二维表对象
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromRows(rowNames As String(), features As Double()(), Optional featureNames As String() = Nothing) As NumericTable
            Return New NumericTable(features, rowNames, featureNames)
        End Function

        ''' <summary>
        ''' 从列向量集合构造二维表对象（内部转置为行主序）
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromColumns(featureNames As String(),
                                           columns As IEnumerable(Of Double()),
                                           Optional rowNames As String() = Nothing) As NumericTable

            Return New NumericTable(featureNames, columns, rowNames)
        End Function

        ''' <summary>
        ''' 从二进制文件之中加载一个二维表对象实例。
        ''' 
        ''' 二进制格式的详细说明请参考 <see cref="NumericTableBinary"/>；
        ''' 保存对象请使用 <see cref="NumericTableBinary.WriteBinary(NumericTable, String, Boolean)"/>。
        ''' </summary>
        ''' <param name="file">二进制文件的路径</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadBinary(file As String) As NumericTable
            Return NumericTableBinary.LoadBinary(file)
        End Function

        ''' <summary>
        ''' 从二进制数据流之中加载一个二维表对象实例（不会关闭调用方传入的流）。
        ''' </summary>
        ''' <param name="stream">二进制数据流</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadBinary(stream As Stream) As NumericTable
            Return NumericTableBinary.LoadBinary(stream)
        End Function

        ''' <summary>
        ''' 拷贝特征矩阵的内部一维数组引用（默认浅拷贝），实现
        ''' <see cref="INumericMatrix.ArrayPack(Boolean)"/> 接口
        ''' </summary>
        Private Function ArrayPack(Optional deepcopy As Boolean = False) As Double()() Implements INumericMatrix.ArrayPack
            If deepcopy Then
                Return CopyMatrix(features)
            Else
                Return features
            End If
        End Function

        ''' <summary>
        ''' 返回每一行数据的标签（行名），实现 <see cref="ILabeledMatrix.GetLabels"/> 接口
        ''' </summary>
        Private Function GetLabels() As IEnumerable(Of String) Implements ILabeledMatrix.GetLabels
            Return RowNamesOrDefault()
        End Function

        ''' <summary>
        ''' 按行枚举特征向量
        ''' </summary>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Double()) Implements IEnumerable(Of Double()).GetEnumerator
            If features IsNot Nothing Then
                For Each row As Double() In features
                    Yield row
                Next
            End If
        End Function

        Private Function EnumerateAll() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New StringBuilder

            Call sb.Append($"[{nsamples} x {nfeatures}]")

            If labelNames IsNot Nothing AndAlso labelNames.Length > 0 Then
                Call sb.Append($" labels: {labelNames.JoinBy(", ")}")
            End If
            If Not String.IsNullOrEmpty(name) Then
                Call sb.Append($" '{name}'")
            End If

            Return sb.ToString
        End Function
    End Class
End Namespace
