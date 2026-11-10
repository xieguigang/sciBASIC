Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Segmentation

    ''' <summary>
    ''' DBSCAN：基于密度的聚类，可发现任意形状的簇并显式标注噪声。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>与欧氏聚类的本质差别</b>：欧氏聚类只有一个参数（距离阈值），
    ''' 它默认“所有簇的密度相近”。当点云同时包含“近处密集的螺栓”和
    ''' “远处稀疏的薄板”时，单一阈值必然在某一侧出错。
    ''' DBSCAN 用 <c>eps</c>（邻域半径）+ <c>minPts</c>（最小邻居数）定义“密集”，
    ''' 只把密度足够高的区域连成簇，稀疏区域自动成为噪声。
    ''' </para>
    ''' <para>
    ''' <b>三个概念</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>核心点</b>：<c>eps</c> 内邻居数 ≥ <c>minPts</c>。</description></item>
    ''' <item><description><b>边界点</b>：自身不是核心点，但落在某个核心点的 <c>eps</c> 内。</description></item>
    ''' <item><description><b>噪声</b>：既不是核心点也不是边界点。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n·k·log n)</c>（KD 树半径查询）；
    ''' 相比欧氏聚类的 <c>O(n)</c> 慢一些，但能在密度分层的场景给出更合理的结果。
    ''' </para>
    ''' <para>
    ''' <b>参数经验</b>：<c>eps</c> ≈ 平均点距的 2~3 倍；
    ''' <c>minPts</c> ≈ 邻域维数 + 1（三维点云常用 6~20）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class DbscanClustering

        Private Sub New()
        End Sub

        ''' <summary>
        ''' DBSCAN 聚类。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="eps">邻域半径；<c>≤ 0</c> 表示按平均点距自适应（2.5 倍）</param>
        ''' <param name="minPts">核心点所需的最小邻居数（默认 10）</param>
        ''' <param name="maxClusterSize">最大簇点数（<c>0</c> 表示不限制）</param>
        Public Shared Function Cluster(cloud As PointCloudBuffer, Optional eps As Double = 0.0,
                                       Optional minPts As Integer = 10,
                                       Optional maxClusterSize As Integer = 0) As ClusterSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If n = 0 Then Return New ClusterSet(Array.Empty(Of Integer)(), eps)

            Dim radius As Double = If(eps > 0.0, eps, std.Max(cloud.EstimateAverageSpacing() * 2.5, 1.0E-9))
            Dim minPoints As Integer = std.Max(2, minPts)

            Dim tree As New KdTree3D(cloud)
            Dim labels As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                labels(i) = -2 ' -2 = 未分类，-1 = 噪声
            Next

            Dim buffer As New Neighborhood(64)
            Dim seeds As New Queue(Of Integer)()
            Dim clusterId As Integer = 0

            For i As Integer = 0 To n - 1
                If labels(i) <> -2 Then Continue For

                Call tree.RadiusSearch(cloud.X(i), cloud.Y(i), cloud.Z(i), radius, buffer, i)

                If buffer.Count + 1 < minPoints Then
                    labels(i) = -1 ' 噪声（先标记，后续可能因成为边界点而被改）

                    Continue For
                End If

                Call ExpandCluster(tree, cloud, i, clusterId, labels, radius, minPoints, seeds)
                clusterId += 1
            Next

            ' 尺寸过滤
            If maxClusterSize > 0 Then
                Dim sizes As Integer() = New Integer(std.Max(0, clusterId) - 1) {}

                For i As Integer = 0 To n - 1
                    If labels(i) >= 0 Then sizes(labels(i)) += 1
                Next

                For i As Integer = 0 To n - 1
                    Dim label As Integer = labels(i)

                    If label >= 0 AndAlso sizes(label) > maxClusterSize Then labels(i) = -1
                Next
            End If

            Return New ClusterSet(labels, radius)
        End Function

        Private Shared Sub ExpandCluster(tree As KdTree3D, cloud As PointCloudBuffer,
                                         seed As Integer, cluster As Integer, labels As Integer(),
                                         radius As Double, minPts As Integer, seeds As Queue(Of Integer))
            labels(seed) = cluster
            Call seeds.Enqueue(seed)

            Dim local As New Neighborhood(64)

            While seeds.Count > 0
                Dim current As Integer = seeds.Dequeue()

                Call tree.RadiusSearch(cloud.X(current), cloud.Y(current), cloud.Z(current), radius, local, current)

                If local.Count + 1 < minPts Then Continue While ' current 是边界点

                For k As Integer = 0 To local.Count - 1
                    Dim neighbor As Integer = local.Indices(k)

                    If labels(neighbor) = -1 Then
                        ' 之前的噪声点被“认领”为边界点
                        labels(neighbor) = cluster
                    ElseIf labels(neighbor) = -2 Then
                        labels(neighbor) = cluster
                        Call seeds.Enqueue(neighbor)
                    End If
                Next
            End While
        End Sub

    End Class
End Namespace
