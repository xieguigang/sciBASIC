#Region "Microsoft.VisualBasic::NumericTableExtensions, Microsoft.VisualBasic.Core\src\Data\NumericTableExtensions.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace Data

    ''' <summary>
    ''' <see cref="NumericTable"/> 的通用扩展方法集合，主要提供给聚类、
    ''' 回归等分析算法进行数据访问与结果回写。
    ''' </summary>
    <HideModuleName>
    Public Module NumericTableExtensions

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
    End Module
End Namespace
