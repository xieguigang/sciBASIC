#Region "Microsoft.VisualBasic::NumericTableExtensions, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\NumericTableExtensions.vb"

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

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations

''' <summary>
''' 层次聚类算法（Hierarchical Clustering）的统一二维表入口。
''' 
''' 数据流：``特征表 →（distanceMatrix）→ 距离矩阵表 →（hca / hcut）→ 聚类树 / 带 cluster 标签的表``
''' </summary>
<HideModuleName>
Public Module HierarchicalClusteringTableExtensions

    ''' <summary>
    ''' 将**特征矩阵**形式的二维表转换为**对称距离矩阵**形式的二维表。
    ''' 
    ''' 返回的新表中：``features`` 为 ``n x n`` 的对称距离矩阵（对角线为 0），
    ''' ``rowNames`` 与 ``featureNames`` 均为样本名，并继承源表的标签列与名称/描述信息。
    ''' 
    ''' ```vb
    ''' Dim dist = x.distanceMatrix()
    ''' Dim tree = dist.hca()
    ''' ```
    ''' </summary>
    ''' <param name="source">特征矩阵形式的二维表（每一行为一个样本，每一列为一个特征）</param>
    ''' <param name="metric">
    ''' 距离度量函数，缺省为欧氏距离 <see cref="DistanceMethods.EuclideanDistance(Double(), Double())"/>。
    ''' </param>
    ''' <returns>距离矩阵形式的二维表</returns>
    <Extension>
    Public Function distanceMatrix(source As NumericTable,
                                   Optional metric As Func(Of Double(), Double(), Double) = Nothing) As NumericTable

        If source Is Nothing Then
            Throw New ArgumentNullException(NameOf(source))
        End If
        If source.nsamples = 0 OrElse source.nfeatures = 0 Then
            Throw New InvalidConstraintException("the source table has no sample/feature data for distance matrix calculation!")
        End If

        Dim rows As Double()() = source.NumericRows()
        Dim n As Integer = rows.Length
        Dim names As String() = assertUniqueNames(source)
        Dim f As Func(Of Double(), Double(), Double) = metric

        If f Is Nothing Then
            f = Function(a As Double(), b As Double()) DistanceMethods.EuclideanDistance(a, b)
        End If

        Dim matrix As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            matrix(i) = New Double(n - 1) {}
        Next

        ' 距离矩阵是对称的：只需要计算上三角，然后镜像到下三角即可
        For i As Integer = 0 To n - 1
            For j As Integer = i + 1 To n - 1
                Dim d As Double = f(rows(i), rows(j))

                matrix(i)(j) = d
                matrix(j)(i) = d
            Next
        Next

        Return New NumericTable(matrix, names, DirectCast(names.Clone(), String())) With {
            .labels = source.labels,
            .labelNames = source.labelNames,
            .name = source.name,
            .description = source.description
        }
    End Function

    ''' <summary>
    ''' 对**距离矩阵**形式的二维表执行凝聚层次聚类，返回聚类树（dendrogram）。
    ''' 
    ''' <para>
    ''' <b>注意：传入的 NumericTable 必须是距离矩阵！</b>
    ''' 如果 <paramref name="source"/> 是原始的特征矩阵，请先调用
    ''' <see cref="distanceMatrix"/> 将特征矩阵转换为距离矩阵形式的二维表，
    ''' 然后再进行层次聚类：
    ''' </para>
    ''' 
    ''' ```vb
    ''' Dim dist = x.distanceMatrix()
    ''' Dim tree = dist.hca()
    ''' ```
    ''' </summary>
    ''' <param name="source">
    ''' 距离矩阵形式的二维表（方阵：``nfeatures = nsamples``，行与列均为样本）。
    ''' <b>注意：这里不是特征矩阵！</b>
    ''' </param>
    ''' <param name="linkage">连接策略，缺省为平均连接 <see cref="AverageLinkageStrategy"/></param>
    ''' <param name="silent">是否静默运行（不输出进度信息），缺省为 True</param>
    ''' <returns>层次聚类树（根节点）</returns>
    <Extension>
    Public Function hca(source As NumericTable,
                        Optional linkage As LinkageStrategy = Nothing,
                        Optional silent As Boolean = True) As Cluster

        Call assertDistanceMatrix(source, NameOf(hca))

        Dim algorithm As New DefaultClusteringAlgorithm With {.Silent = silent}

        Return algorithm.performClustering(
            distances:=source.NumericRows(),
            clusterNames:=source.RowNamesOrDefault(),
            linkageStrategy:=If(linkage, New AverageLinkageStrategy())
        )
    End Function

    ''' <summary>
    ''' 对**距离矩阵**形式的二维表执行层次聚类，并按照目标簇数量 <paramref name="k"/> 切分聚类树，
    ''' 将得到的类编号写入 ``cluster`` 标签列之后返回原表。
    ''' 
    ''' <para>
    ''' <b>注意：传入的 NumericTable 必须是距离矩阵！</b>
    ''' 如果 <paramref name="source"/> 是原始的特征矩阵，请先调用
    ''' <see cref="distanceMatrix"/> 进行转换：
    ''' </para>
    ''' 
    ''' ```vb
    ''' Dim flat = x.distanceMatrix().hcut(k:=3)
    ''' ```
    ''' </summary>
    ''' <param name="source">距离矩阵形式的二维表（方阵），<b>注意：不是特征矩阵！</b></param>
    ''' <param name="k">目标簇数量</param>
    ''' <param name="linkage">连接策略，缺省为平均连接 <see cref="AverageLinkageStrategy"/></param>
    ''' <param name="silent">是否静默运行，缺省为 True</param>
    ''' <returns>写入 ``cluster`` 标签之后的原表对象</returns>
    <Extension>
    Public Function hcut(source As NumericTable,
                         k As Integer,
                         Optional linkage As LinkageStrategy = Nothing,
                         Optional silent As Boolean = True) As NumericTable

        Call assertDistanceMatrix(source, NameOf(hcut))

        If k < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(k), "the cluster number k must be a positive value")
        End If
        If k > source.nsamples Then
            Throw New ArgumentOutOfRangeException(NameOf(k), $"the cluster number k({k}) can not be greater than the sample size {source.nsamples}!")
        End If

        Dim root As Cluster = hca(source, linkage, silent)
        Dim flat As List(Of Cluster) = cutTree(root, k)

        Return writeClusterLabels(source, flat)
    End Function

    ''' <summary>
    ''' 对**距离矩阵**形式的二维表执行层次聚类，并按照距离阈值 <paramref name="threshold"/>
    ''' 切分聚类树（复用既有的扁平切分实现），将得到的类编号写入 ``cluster`` 标签列之后返回原表。
    ''' 
    ''' <para>
    ''' <b>注意：传入的 NumericTable 必须是距离矩阵！</b>
    ''' 如果 <paramref name="source"/> 是原始的特征矩阵，请先调用
    ''' <see cref="distanceMatrix"/> 进行转换：
    ''' </para>
    ''' 
    ''' ```vb
    ''' Dim flat = x.distanceMatrix().hcut(threshold:=5.0)
    ''' ```
    ''' </summary>
    ''' <param name="source">距离矩阵形式的二维表（方阵），<b>注意：不是特征矩阵！</b></param>
    ''' <param name="threshold">距离阈值，小于该阈值的簇会被合并</param>
    ''' <param name="linkage">连接策略，缺省为平均连接 <see cref="AverageLinkageStrategy"/></param>
    ''' <param name="silent">是否静默运行，缺省为 True</param>
    ''' <returns>写入 ``cluster`` 标签之后的原表对象</returns>
    <Extension>
    Public Function hcut(source As NumericTable,
                         threshold As Double,
                         Optional linkage As LinkageStrategy = Nothing,
                         Optional silent As Boolean = True) As NumericTable

        Call assertDistanceMatrix(source, NameOf(hcut))

        If threshold <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(threshold), "the distance threshold must be a positive value")
        End If

        Dim algorithm As New DefaultClusteringAlgorithm With {.Silent = silent}
        Dim flat As IList(Of Cluster) = algorithm.performFlatClustering(
            distances:=source.NumericRows(),
            clusterNames:=source.RowNamesOrDefault(),
            linkageStrategy:=If(linkage, New AverageLinkageStrategy()),
            threshold:=threshold
        )

        Return writeClusterLabels(source, flat)
    End Function

#Region "Helpers"

    ''' <summary>
    ''' 校验目标表是否为距离矩阵（方阵），并给出醒目的转换提示
    ''' </summary>
    Private Sub assertDistanceMatrix(source As NumericTable, caller As String)
        If source Is Nothing Then
            Throw New ArgumentNullException(NameOf(source))
        End If
        If source.nsamples = 0 Then
            Throw New InvalidConstraintException($"the source table is empty for the '{caller}' hierarchical clustering!")
        End If
        If source.nfeatures <> source.nsamples Then
            Throw New InvalidConstraintException(
                $"the source table must be a distance matrix (a square matrix), but its shape is " &
                $"{source.nsamples} x {source.nfeatures}! " &
                "Please convert the feature table into a distance matrix via the 'distanceMatrix()' function first."
            )
        End If

        Call assertUniqueNames(source)
    End Sub

    ''' <summary>
    ''' 获取唯一的样本名数组（距离矩阵的行列名以及层次聚类的输入都要求样本名唯一）
    ''' </summary>
    Private Function assertUniqueNames(source As NumericTable) As String()
        Dim names As String() = source.RowNamesOrDefault()
        Dim duplicated As String() = names _
            .GroupBy(Function(s) s) _
            .Where(Function(g) g.Count() > 1) _
            .Select(Function(g) g.Key) _
            .ToArray

        If duplicated.Length > 0 Then
            Throw New InvalidConstraintException(
                $"the sample names of the table are not unique: {String.Join(", ", duplicated)}"
            )
        End If

        Return names
    End Function

    ''' <summary>
    ''' 按目标簇数量对聚类树进行切分：反复挑选“叶数最多且仍然可以分裂”的节点，
    ''' 用其子节点替换之，直到簇的数量达到 <paramref name="k"/> 或者已经没有可分裂的节点。
    ''' </summary>
    Private Function cutTree(root As Cluster, k As Integer) As List(Of Cluster)
        Dim clusters As New List(Of Cluster) From {root}

        While clusters.Count < k
            Dim target As Cluster = Nothing
            Dim maxLeafs As Integer = -1

            For Each c As Cluster In clusters
                If Not c.isLeaf AndAlso c.Leafs > maxLeafs Then
                    maxLeafs = c.Leafs
                    target = c
                End If
            Next

            If target Is Nothing Then
                ' 已经没有可以继续分裂的节点了
                Exit While
            End If

            Call clusters.Remove(target)

            For Each child As Cluster In target.Children
                Call clusters.Add(child)
            Next
        End While

        Return clusters
    End Function

    ''' <summary>
    ''' 将扁平化的簇集合写入 ``cluster`` 标签列并返回原表
    ''' </summary>
    Private Function writeClusterLabels(source As NumericTable, clusters As IEnumerable(Of Cluster)) As NumericTable
        Dim names As String() = source.RowNamesOrDefault()
        Dim index As Dictionary(Of String, Integer) = names _
            .SeqIterator _
            .ToDictionary(Function(x) x.value, Function(x) x.i)
        Dim labels As Integer() = New Integer(names.Length - 1) {}
        Dim classId As Integer = 0

        For Each c As Cluster In clusters
            classId += 1

            For Each leaf As String In collectLeafs(c)
                labels(index(leaf)) = classId
            Next
        Next

        Return source.SetLabel("cluster", labels)
    End Function

    ''' <summary>
    ''' 递归的收集一个簇（子树）之中所有叶节点的名称
    ''' </summary>
    Private Iterator Function collectLeafs(c As Cluster) As IEnumerable(Of String)
        If c.isLeaf Then
            Yield c.Name
        Else
            For Each child As Cluster In c.Children
                For Each name As String In collectLeafs(child)
                    Yield name
                Next
            Next
        End If
    End Function

#End Region

End Module
