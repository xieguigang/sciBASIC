Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Segmentation

    ''' <summary>
    ''' 聚类标签集合：逐点标签（<c>-1</c> 表示噪声/未分配）与簇统计。
    ''' </summary>
    Public Class ClusterSet

        ''' <summary>逐点簇标签，<c>-1</c> 表示噪声。</summary>
        Public Property Labels As Integer()

        ''' <summary>连通判据使用的距离阈值。</summary>
        Public Property Tolerance As Double

        Public Sub New(labels As Integer(), tolerance As Double)
            Me.Labels = labels
            Me.Tolerance = tolerance
        End Sub

        ''' <summary>点数量。</summary>
        Public ReadOnly Property Count As Integer
            Get
                Return If(Labels Is Nothing, 0, Labels.Length)
            End Get
        End Property

        ''' <summary>簇数量（不含噪声）。</summary>
        Public ReadOnly Property ClusterCount As Integer
            Get
                If Labels Is Nothing OrElse Labels.Length = 0 Then Return 0

                Dim maxLabel As Integer = -1

                For i As Integer = 0 To Labels.Length - 1
                    If Labels(i) > maxLabel Then maxLabel = Labels(i)
                Next

                Return maxLabel + 1
            End Get
        End Property

        ''' <summary>噪声点数量。</summary>
        Public ReadOnly Property NoiseCount As Integer
            Get
                Dim noise As Integer = 0

                For i As Integer = 0 To Count - 1
                    If Labels(i) < 0 Then noise += 1
                Next

                Return noise
            End Get
        End Property

        ''' <summary>各簇的点数（按簇编号索引）。</summary>
        Public Function Sizes() As Integer()
            Dim counts As Integer() = New Integer(std.Max(0, ClusterCount) - 1) {}

            For i As Integer = 0 To Count - 1
                Dim label As Integer = Labels(i)

                If label >= 0 Then counts(label) += 1
            Next

            Return counts
        End Function

        ''' <summary>某个簇的点下标。</summary>
        Public Function Indices(cluster As Integer) As Integer()
            Dim list As New List(Of Integer)()

            For i As Integer = 0 To Count - 1
                If Labels(i) = cluster Then Call list.Add(i)
            Next

            Return list.ToArray()
        End Function

        ''' <summary>拆分为多个点云（每簇一个）。</summary>
        Public Function ToClouds(cloud As PointCloudBuffer) As PointCloudBuffer()
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim count As Integer = ClusterCount
            Dim clouds(count - 1) As PointCloudBuffer

            For c As Integer = 0 To count - 1
                clouds(c) = cloud.Subset(Indices(c))
            Next

            Return clouds
        End Function

        Public Overrides Function ToString() As String
            Return $"{ClusterCount} clusters, {NoiseCount} noise points (tolerance={Tolerance:G4})"
        End Function
    End Class

    ''' <summary>
    ''' 欧氏聚类：按空间距离把相邻点归并成簇。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>判据</b>：两点距离 ≤ <c>tolerance</c> 即连通；连通性有传递性
    ''' （A–B、B–C 连通则 A、B、C 同簇），因此本质是求“距离图的连通分量”。
    ''' </para>
    ''' <para>
    ''' <b>实现</b>：并查集 + 体素哈希。
    ''' 体素边长取 <c>tolerance</c> 时，任意两个距离 ≤ <c>tolerance</c> 的点必然落在
    ''' 同一体素或 26 邻域体素内（因为 <c>|Δx| ≤ tolerance</c> 时
    ''' <c>floor(x/c)</c> 最多相差 1），因此每个点只需检查 27 个体素，
    ''' 总复杂度 <c>O(n)</c>，而不需要每个点做一次半径查询。
    ''' </para>
    ''' <para>
    ''' <b>并查集为什么不用并行</b>：Union 操作天然需要串行（或需要复杂的无锁实现），
    ''' 而这里的查找已经是 O(1) 量级，串行总成本远低于并行协调开销。
    ''' </para>
    ''' <para>
    ''' <b>阈值怎么选</b>：经验值是平均点距的 2~4 倍。太小会把同一零件切碎，
    ''' 太大会把相邻零件粘连（紧密堆叠场景请改用
    ''' <see cref="CascadeSegmentation"/>）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class EuclideanClustering

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 欧氏聚类。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="tolerance">连通距离阈值；<c>≤ 0</c> 表示按平均点距自适应（3 倍）</param>
        ''' <param name="minClusterSize">最小簇点数（小于该值的簇标记为噪声）</param>
        ''' <param name="maxClusterSize">最大簇点数（<c>0</c> 表示不限制；超过的簇标记为噪声）</param>
        Public Shared Function Cluster(cloud As PointCloudBuffer, Optional tolerance As Double = 0.0,
                                       Optional minClusterSize As Integer = 10,
                                       Optional maxClusterSize As Integer = 0) As ClusterSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If n = 0 Then Return New ClusterSet(Array.Empty(Of Integer)(), tolerance)

            Dim t As Double = If(tolerance > 0.0, tolerance, std.Max(cloud.EstimateAverageSpacing() * 3.0, 1.0E-9))

            Dim hash As New VoxelHash(cloud, t)
            Dim parent As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                parent(i) = i
            Next

            Dim neighbors As New List(Of Integer)(32)

            For i As Integer = 0 To n - 1
                Call hash.NeighborsInRadius(cloud.X(i), cloud.Y(i), cloud.Z(i), t, neighbors, i)

                For k As Integer = 0 To neighbors.Count - 1
                    Dim j As Integer = neighbors(k)

                    If j > i Then Union(parent, i, j)
                Next
            Next

            ' 压缩标签：只有满足尺寸约束的连通分量才成为有效簇
            Dim rootToLabel As New Dictionary(Of Integer, Integer)()
            Dim sizes As New Dictionary(Of Integer, Integer)()

            For i As Integer = 0 To n - 1
                Dim root As Integer = Find(parent, i)
                Dim size As Integer

                sizes.TryGetValue(root, size)
                sizes(root) = size + 1
            Next

            Dim labels As Integer() = New Integer(n - 1) {}
            Dim nextLabel As Integer = 0

            For i As Integer = 0 To n - 1
                Dim root As Integer = Find(parent, i)
                Dim size As Integer = sizes(root)

                If size < minClusterSize OrElse (maxClusterSize > 0 AndAlso size > maxClusterSize) Then
                    labels(i) = -1

                    Continue For
                End If

                Dim label As Integer

                If Not rootToLabel.TryGetValue(root, label) Then
                    label = nextLabel
                    nextLabel += 1
                    rootToLabel(root) = label
                End If

                labels(i) = label
            Next

            Return New ClusterSet(labels, t)
        End Function

        Private Shared Function Find(parent As Integer(), i As Integer) As Integer
            Dim root As Integer = i

            While parent(root) <> root
                root = parent(root)
            End While

            ' 路径压缩
            While parent(i) <> root
                Dim nextNode As Integer = parent(i)

                parent(i) = root
                i = nextNode
            End While

            Return root
        End Function

        Private Shared Sub Union(parent As Integer(), a As Integer, b As Integer)
            Dim ra As Integer = Find(parent, a)
            Dim rb As Integer = Find(parent, b)

            If ra = rb Then Return

            ' 按编号合并（保持确定性与浅树高）
            If ra < rb Then
                parent(rb) = ra
            Else
                parent(ra) = rb
            End If
        End Sub

    End Class
End Namespace
