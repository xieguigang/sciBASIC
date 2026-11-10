Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Segmentation

    ''' <summary>
    ''' 四级级联分割：面向紧密堆叠零件的欠分割治理。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么单一算法不够</b>：船舶钢板、冲压件这类“紧密贴合、曲面连续”的场景，
    ''' 欧氏聚类会因为零件之间只隔 0.2mm 而把整堆粘连成一个簇；
    ''' 区域生长又会在平面之间来回渗透。工业上可行的做法是“多判据级联”：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>
    ''' <b>第一级：欧氏聚类</b>——先把空间上明显分离的零件分开（大尺度）。
    ''' </description></item>
    ''' <item><description>
    ''' <b>第二级：局部凸连接面片</b>——在连通图上切断“凹”的边。
    ''' 相邻两点的法向夹角与连线的几何关系能区分“同一个面继续延伸”与“转到另一个面”：
    ''' 当 <c>n_i·(p_j − p_i)</c> 明显为正（邻居在切平面上方），说明存在折角/凹缝，
    ''' 应当切开。
    ''' </description></item>
    ''' <item><description>
    ''' <b>第三级：法向/曲率区域生长</b>——把每个凸面片内部按曲率连续性细分，
    ''' 得到“面片级”的语义单元。
    ''' </description></item>
    ''' <item><description>
    ''' <b>第四级：平面精修</b>——用 RANSAC 提取平面，把落在同一平面上的区域合并。
    ''' 这一步能修复“同一平面被凹缝切成两块”的过分割。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' <b>实测参数走向</b>：第二级的 <see cref="CascadeOptions.ConvexityThreshold"/>
    ''' 是最关键的旋钮——取值太大不起作用，太小会把正常曲面切碎；
    ''' 建议从 0.02（约为 1.1° 的切向偏置）开始，按过分割/欠分割现象微调。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class CascadeSegmentation

        Private Sub New()
        End Sub

        ''' <summary>级联参数。</summary>
        Public Class CascadeOptions

            ''' <summary>第一级欧氏聚类的连通距离；<c>≤ 0</c> 自适应（2 倍点距）。</summary>
            Public Property EuclideanTolerance As Double = 0.0

            ''' <summary>最小区域点数（默认 30）。</summary>
            Public Property MinClusterSize As Integer = 30

            '''
            ''' 第二级凸性阈值：邻居相对切平面的归一化高度超过该值即切开（默认 0.02）。
            '''
            Public Property ConvexityThreshold As Double = 0.02

            ''' <summary>第三级区域生长的法向夹角阈值（度，默认 5）。</summary>
            Public Property SmoothnessDegrees As Double = 5.0

            ''' <summary>第三级区域生长的曲率上限（默认 0.06）。</summary>
            Public Property CurvatureThreshold As Double = 0.06

            ''' <summary>第四级平面提取的内点距离；<c>≤ 0</c> 自适应（1.5 倍点距）。</summary>
            Public Property PlaneInlierDistance As Double = 0.0

            ''' <summary>第四级合并时，平面需覆盖的最小区域比例（默认 0.6）。</summary>
            Public Property PlaneMergeRatio As Double = 0.6

            ''' <summary>第四级最多提取多少个平面（默认 12）。</summary>
            Public Property MaxPlanes As Integer = 12
        End Class

        ''' <summary>级联分割结果。</summary>
        Public Class CascadeResult

            ''' <summary>最终标签。</summary>
            Public Property Clusters As ClusterSet

            ''' <summary>第四级提取出的平面。</summary>
            Public Property Planes As List(Of PrimitiveDetection)

            ''' <summary>各级的分割结果（诊断用，按级别顺序）。</summary>
            Public Property Levels As ClusterSet()

            Public Overrides Function ToString() As String
                Return $"cascade: {Clusters.ClusterCount} regions, {If(Planes, New List(Of PrimitiveDetection)()).Count} planes"
            End Function
        End Class

        ''' <summary>
        ''' 执行四级级联分割。
        ''' </summary>
        Public Shared Function Segment(cloud As PointCloudBuffer, Optional options As CascadeOptions = Nothing,
                                       Optional optionsCloudy As CloudyOptions = Nothing) As CascadeResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CascadeOptions = If(options, New CascadeOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then
                Return New CascadeResult With {
                    .Clusters = New ClusterSet(Array.Empty(Of Integer)(), 0.0),
                    .Planes = New List(Of PrimitiveDetection)(),
                    .Levels = Array.Empty(Of ClusterSet)()
                }
            End If

            Dim spacing As Double = cloud.EstimateAverageSpacing()
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals OrElse source.Curvature Is Nothing Then
                source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)
            End If

            Dim levels As New List(Of ClusterSet)()

            ' 第一级：欧氏聚类
            Dim tolerance As Double = If(opt.EuclideanTolerance > 0.0, opt.EuclideanTolerance, std.Max(spacing * 2.0, 1.0E-9))
            Dim level1 As ClusterSet = FastEuclideanClustering.Cluster(source, tolerance, opt.MinClusterSize)

            Call levels.Add(level1)

            ' 第二级：切断凹边
            Dim convexLabels As Integer() = SplitByConvexity(source, level1, opt, global_)
            Dim level2 As ClusterSet = Compact(convexLabels, opt.MinClusterSize, tolerance)

            Call levels.Add(level2)

            ' 第三级：在凸面片内部做区域生长
            Dim regionLabels As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                regionLabels(i) = -1
            Next

            Dim nextLabel As Integer = 0

            For Each clusterIds As Integer() In EnumerateClusters(level2)
                Dim subset As PointCloudBuffer = source.Subset(clusterIds)
                Dim local As ClusterSet = RegionGrowing.Segment(subset, 30, opt.SmoothnessDegrees,
                                                                opt.CurvatureThreshold, opt.MinClusterSize, global_)

                For k As Integer = 0 To clusterIds.Length - 1
                    Dim label As Integer = local.Labels(k)

                    regionLabels(clusterIds(k)) = If(label < 0, -1, nextLabel + label)
                Next

                nextLabel += std.Max(1, local.ClusterCount)
            Next

            Dim level3 As ClusterSet = Compact(regionLabels, opt.MinClusterSize, opt.SmoothnessDegrees)

            Call levels.Add(level3)

            ' 第四级：平面精修（同一平面上的区域合并）
            Dim ransac As New ModelFitting.RansacOptions With {
                .MaxIterations = 600,
                .InlierDistance = If(opt.PlaneInlierDistance > 0.0, opt.PlaneInlierDistance, std.Max(spacing * 1.5, 1.0E-9)),
                .MinInliers = std.Max(20, opt.MinClusterSize),
                .MaxModels = opt.MaxPlanes
            }

            Dim planes As List(Of PrimitiveDetection) = ModelFitting.ExtractAll(source, {PrimitiveType.Plane}, ransac, global_)

            Dim finalLabels As Integer() = DirectCast(level3.Labels.Clone(), Integer())
            Dim nextPlaneLabel As Integer = level3.ClusterCount

            For Each plane As PrimitiveDetection In planes
                ' 只有当平面覆盖某个区域的足够大比例时，才把该区域整体归并到平面
                Dim regionCounts As New Dictionary(Of Integer, Integer)()

                For Each index As Integer In plane.InlierIndices
                    Dim label As Integer = finalLabels(index)

                    If label < 0 Then Continue For

                    Dim count As Integer

                    regionCounts.TryGetValue(label, count)
                    regionCounts(label) = count + 1
                Next

                Dim sizes As Integer() = level3.Sizes()
                Dim merged As Boolean = False

                For Each entry As KeyValuePair(Of Integer, Integer) In regionCounts
                    Dim label As Integer = entry.Key

                    If label >= sizes.Length Then Continue For
                    If sizes(label) <= 0 Then Continue For
                    If CDbl(entry.Value) / sizes(label) < opt.PlaneMergeRatio Then Continue For

                    For i As Integer = 0 To n - 1
                        If finalLabels(i) = label Then finalLabels(i) = nextPlaneLabel
                    Next

                    merged = True
                Next

                If merged Then nextPlaneLabel += 1
            Next

            Dim level4 As ClusterSet = Compact(finalLabels, opt.MinClusterSize, tolerance)

            Call levels.Add(level4)

            Return New CascadeResult With {
                .Clusters = level4,
                .Planes = planes,
                .Levels = levels.ToArray()
            }
        End Function

        ''' <summary>
        ''' 第二级：在 kNN 图上切断凹边后求连通分量。
        ''' </summary>
        Private Shared Function SplitByConvexity(cloud As PointCloudBuffer, level1 As ClusterSet,
                                                 opt As CascadeOptions, options As CloudyOptions) As Integer()
            Dim n As Integer = cloud.Count
            Dim parent As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                parent(i) = i
            Next

            Dim tree As New KdTree3D(cloud)
            Dim k As Integer = 12
            Dim buffer As New Neighborhood(k + 1)

            ' 并查集必须串行：Union 不是线程安全的，并行化会丢失连通信息。
            ' 这里的开销主要在 kNN 查询（O(n·k·log n)），已经足够快。
            For i As Integer = 0 To n - 1
                Call tree.Knn(cloud.X(i), cloud.Y(i), cloud.Z(i), k, buffer, i)

                Dim normal As Vector3d = cloud.Normal(i)

                For j As Integer = 0 To buffer.Count - 1
                    Dim other As Integer = buffer.Indices(j)

                    ' 只在同一欧氏簇内部考虑（第一级已经分开的不再合并）
                    If level1.Labels(i) <> level1.Labels(other) Then Continue For

                    Dim d As Double = std.Sqrt(buffer.Distances(j))

                    If d <= 1.0E-12 Then Continue For

                    Dim direction As Vector3d = (cloud.Item(other) - cloud.Item(i)) / d
                    Dim height As Double = Vector3d.Dot(normal, direction)

                    ' 邻居在切平面上方过多 ⇒ 存在折角/凹缝，切断
                    If height > opt.ConvexityThreshold Then Continue For

                    Union(parent, i, other)
                Next
            Next

            Dim labels As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                labels(i) = Find(parent, i)
            Next

            Return labels
        End Function

        ''' <summary>
        ''' 把任意整型标签压缩成 <c>0..k-1</c>（小于 <paramref name="minSize"/> 的成为噪声）。
        ''' </summary>
        Private Shared Function Compact(labels As Integer(), minSize As Integer, tolerance As Double) As ClusterSet
            Dim map As New Dictionary(Of Integer, Integer)()
            Dim sizes As New Dictionary(Of Integer, Integer)()

            For i As Integer = 0 To labels.Length - 1
                Dim label As Integer = labels(i)

                If label < 0 Then Continue For

                Dim size As Integer

                sizes.TryGetValue(label, size)
                sizes(label) = size + 1
            Next

            Dim nextLabel As Integer = 0

            For Each entry As KeyValuePair(Of Integer, Integer) In sizes
                If entry.Value >= minSize Then
                    map(entry.Key) = nextLabel
                    nextLabel += 1
                End If
            Next

            Dim result As Integer() = New Integer(labels.Length - 1) {}

            For i As Integer = 0 To labels.Length - 1
                Dim label As Integer = labels(i)

                If label < 0 Then
                    result(i) = -1

                    Continue For
                End If

                Dim mapped As Integer

                result(i) = If(map.TryGetValue(label, mapped), mapped, -1)
            Next

            Return New ClusterSet(result, tolerance)
        End Function

        Private Shared Function EnumerateClusters(labels As ClusterSet) As IEnumerable(Of Integer())
            Dim count As Integer = labels.ClusterCount
            Dim result As New List(Of Integer())(count)

            For c As Integer = 0 To count - 1
                Call result.Add(labels.Indices(c))
            Next

            Return result
        End Function

        Private Shared Function Find(parent As Integer(), i As Integer) As Integer
            Dim root As Integer = i

            While parent(root) <> root
                root = parent(root)
            End While

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

            If ra < rb Then
                parent(rb) = ra
            Else
                parent(ra) = rb
            End If
        End Sub

    End Class
End Namespace
