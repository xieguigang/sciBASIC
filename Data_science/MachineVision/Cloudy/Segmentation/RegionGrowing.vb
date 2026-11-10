Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Segmentation

    ''' <summary>
    ''' 区域生长分割：按法向连续性与曲率上限把光滑曲面扩成一个区域。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>判据</b>：从曲率最小的点（最“平”的点）作为种子开始，
    ''' 向邻域扩张；只要邻居满足
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>当前点与邻居的法向夹角 ≤ <paramref name="smoothnessDegrees"/>（默认 3°）；</description></item>
    ''' <item><description>邻居的曲率 ≤ <paramref name="curvatureThreshold"/>（排除边缘/角点）；</description></item>
    ''' </list>
    ''' <para>
    ''' 就把它并入同一区域。
    ''' </para>
    ''' <para>
    ''' <b>为什么种子按曲率升序</b>：从平坦区域开始生长，可以让一个平面稳定地
    ''' 吸收它周围的所有点；如果从边缘开始，生长会沿着边缘“爬”到另一个面，
    ''' 造成过分割/欠分割。这是 PCL <c>RegionGrowing</c> 的标准做法。
    ''' </para>
    ''' <para>
    ''' <b>与欧氏聚类的分工</b>：欧氏聚类按空间距离切分，会把“贴在一起但属于
    ''' 不同面”的点留在同一簇；区域生长按曲率/法向切分，能沿着棱线把面分开，
    ''' 但对“不连通但共面”的两个区域会给出同一个标签（因为它不关心连通性之外的全局关系）。
    ''' 工业上常用“欧氏聚类先分出零件 → 区域生长再分出各个面”的级联策略。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class RegionGrowing

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 区域生长分割。
        ''' </summary>
        ''' <param name="cloud">输入点云（缺法向/曲率时自动估计）</param>
        ''' <param name="k">邻域点数（默认 30）</param>
        ''' <param name="smoothnessDegrees">法向夹角阈值（度）</param>
        ''' <param name="curvatureThreshold">曲率上限（表面变化度，默认 0.05）</param>
        ''' <param name="minClusterSize">最小区域点数</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Segment(cloud As PointCloudBuffer, Optional k As Integer = 30,
                                       Optional smoothnessDegrees As Double = 3.0,
                                       Optional curvatureThreshold As Double = 0.05,
                                       Optional minClusterSize As Integer = 30,
                                       Optional options As CloudyOptions = Nothing) As ClusterSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count
            Dim global_ As CloudyOptions = If(options, CloudyOptions.Default)

            If n = 0 Then Return New ClusterSet(Array.Empty(Of Integer)(), 0.0)

            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals OrElse source.Curvature Is Nothing Then
                source = NormalEstimation.Estimate(cloud, std.Max(15, k \ 2), 0.0, global_)
            End If

            Dim tree As New KdTree3D(source)
            Dim order As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            Dim curvature As Double() = source.Curvature

            Array.Sort(DirectCast(curvature.Clone(), Double()), order)

            Dim labels As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                labels(i) = -1
            Next

            Dim cosThreshold As Double = std.Cos(smoothnessDegrees * std.PI / 180.0)
            Dim buffer As New Neighborhood(std.Max(8, k))
            Dim queue As New Queue(Of Integer)()
            Dim cluster As Integer = 0

            For Each seed As Integer In order
                If labels(seed) >= 0 Then Continue For
                If curvature(seed) > curvatureThreshold Then Continue For

                labels(seed) = cluster
                Call queue.Enqueue(seed)

                While queue.Count > 0
                    Dim current As Integer = queue.Dequeue()
                    Dim currentNormal As Vector3d = source.Normal(current)

                    Call tree.Knn(source.X(current), source.Y(current), source.Z(current), k, buffer, current)

                    For j As Integer = 0 To buffer.Count - 1
                        Dim neighbor As Integer = buffer.Indices(j)

                        If labels(neighbor) >= 0 Then Continue For
                        If curvature(neighbor) > curvatureThreshold Then Continue For

                        Dim dot As Double = Vector3d.Dot(currentNormal, source.Normal(neighbor))

                        If std.Abs(dot) < cosThreshold Then Continue For

                        labels(neighbor) = cluster
                        Call queue.Enqueue(neighbor)
                    Next
                End While

                cluster += 1
            Next

            ' 小区域归为噪声
            Dim sizes As Integer() = New Integer(std.Max(0, cluster) - 1) {}

            For i As Integer = 0 To n - 1
                If labels(i) >= 0 Then sizes(labels(i)) += 1
            Next

            Dim remap(cluster - 1) As Integer

            For c As Integer = 0 To cluster - 1
                remap(c) = -1
            Next

            Dim nextLabel As Integer = 0

            For c As Integer = 0 To cluster - 1
                If sizes(c) >= minClusterSize Then
                    remap(c) = nextLabel
                    nextLabel += 1
                End If
            Next

            For i As Integer = 0 To n - 1
                Dim label As Integer = labels(i)

                If label >= 0 Then labels(i) = remap(label)
            Next

            Return New ClusterSet(labels, smoothnessDegrees)
        End Function

    End Class
End Namespace
