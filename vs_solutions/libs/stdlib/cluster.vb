Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
' this module is itself named 'cluster', which would shadow the hierachical clustering tree
' type in a case insensitive language, so the tree type is aliased here
Imports HCluster = Microsoft.VisualBasic.DataMining.HierarchicalClustering.Cluster

''' <summary>
''' 聚类模块：k-means、层次聚类（距离矩阵 → 树 → 剪枝）。
''' <para>
''' 注意层次聚类的输入约定：<see cref="hca"/> / <see cref="hcut"/> / <see cref="hcutByDistance"/>
''' 都要求传入**距离矩阵**（由 <see cref="distanceMatrix"/> 从特征表生成），
''' 这与 <see cref="kmeans"/> 直接接收特征表不同。
''' </para>
''' </summary>
Public Module cluster

    ''' <summary>
    ''' k-means 聚类。聚类编号写入标签列 ``cluster``（从 1 开始），返回原表。
    ''' </summary>
    ''' <param name="threads">并行线程数；小于等于 0 时使用默认值 16</param>
    Public Function kmeans(source As NumericTable, k As Integer, Optional threads As Integer = 0) As NumericTable
        Return source.kmeans(k, If(threads <= 0, 16, threads), debug:=False)
    End Function

    ''' <summary>由特征表生成 n×n 的对称欧氏距离矩阵（返回新表）。</summary>
    Public Function distanceMatrix(source As NumericTable) As NumericTable
        Return source.distanceMatrix()
    End Function

    ''' <summary>对距离矩阵做层次聚类，返回树根节点。</summary>
    Public Function hca(dist As NumericTable) As HCluster
        Return dist.hca(Nothing, silent:=True)
    End Function

    ''' <summary>按聚类数量剪枝，聚类编号写入标签列 ``cluster``（从 1 开始），返回原表。</summary>
    Public Function hcut(dist As NumericTable, k As Integer) As NumericTable
        Return dist.hcut(k, Nothing, silent:=True)
    End Function

    ''' <summary>按连接距离阈值剪枝（距离小于阈值的簇会被合并），返回原表。</summary>
    Public Function hcutByDistance(dist As NumericTable, threshold As Double) As NumericTable
        Return dist.hcut(threshold, Nothing, silent:=True)
    End Function

    ' ======================================================== 树结构信息

    ''' <summary>叶节点数量。</summary>
    Public Function leafCount(tree As HCluster) As Integer
        Return tree.Leafs
    End Function

    ''' <summary>按树的顺序展开的叶节点名称。</summary>
    Public Function leafs(tree As HCluster) As String()
        Return tree.OrderLeafs()
    End Function

    ''' <summary>子节点数量。</summary>
    Public Function children(tree As HCluster) As Integer
        Return If(tree.Children Is Nothing, 0, tree.Children.Count)
    End Function

    ''' <summary>树的总连接距离。</summary>
    Public Function totalDistance(tree As HCluster) As Double
        Return tree.TotalDistance
    End Function

    ''' <summary>统计聚类编号（从 1 开始）各有多少个样本。</summary>
    Public Function clusterSizes(labels As Integer()) As Integer()
        Dim k As Integer = If(labels Is Nothing OrElse labels.Length = 0, 0, labels.Max)

        If k = 0 Then
            Return New Integer() {}
        End If

        Dim sizes(k - 1) As Integer

        For Each i As Integer In labels
            If i > 0 Then
                sizes(i - 1) += 1
            End If
        Next

        Return sizes
    End Function

    ''' <summary>
    ''' 交叉表：把聚类结果与真实分类做交叉统计，返回新表（行 = 分类，列 = 聚类）。
    ''' </summary>
    Public Function crossTable(truth As Integer(), labels As Integer()) As NumericTable
        If truth Is Nothing OrElse labels Is Nothing OrElse truth.Length <> labels.Length Then
            Throw New ArgumentException("the truth and cluster label vectors should have the same length")
        End If

        Dim n As Integer = labels.Max
        Dim classes As Integer() = truth.Distinct.OrderBy(Function(c) c).ToArray
        Dim rows As New List(Of Double())
        Dim names As New List(Of String)

        For Each cls As Integer In classes
            Dim counts(n - 1) As Double

            For i As Integer = 0 To truth.Length - 1
                If truth(i) = cls AndAlso labels(i) > 0 Then
                    counts(labels(i) - 1) += 1
                End If
            Next

            rows.Add(counts)
            names.Add($"class_{cls}")
        Next

        Return NumericTable.FromRows(names.ToArray, rows.ToArray, clusterColumnNames(n))
    End Function

    Private Function clusterColumnNames(n As Integer) As String()
        Dim out(n - 1) As String

        For i As Integer = 0 To n - 1
            out(i) = "cluster_" & (i + 1)
        Next

        Return out
    End Function
End Module
