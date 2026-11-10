Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Preprocessing
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' SIFT-3D 关键点检测（多尺度曲率极值，简化实现）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>与二维 SIFT 的对应关系</b>：二维 SIFT 在高斯金字塔上找
    ''' “DoG 响应在空间与尺度上同时取极值”的点；三维没有可靠的图像金字塔卷积，
    ''' 通行的做法是<b>用体素金字塔代替高斯金字塔</b>，
    ''' 用“尺度归一化的曲率/表面变化度”代替 DoG 响应。
    ''' 本实现即按此思路：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>以 <c>v_s = v0·2^s</c> 做体素降采样得到尺度金字塔。</description></item>
    ''' <item><description>每一层估计法向与表面变化度 <c>c_s</c>，响应取 <c>r_s = c_s · v_s</c>（尺度归一化）。</description></item>
    ''' <item><description>在空间上取极大值（<c>2·v_s</c> 邻域），再要求它在相邻尺度上也更大。</description></item>
    ''' <item><description>用最近邻把关键点映射回原始点云下标，并在原尺度做一次非极大抑制。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>能力边界（务必注意）</b>：这是<b>简化实现</b>，它不是 PCL 中
    ''' <c>SIFTKeypoint</c> 的逐位复刻（后者还包含强度梯度方向直方图与亚体素插值）。
    ''' 它保留了 SIFT-3D 的两个核心性质——多尺度与“尺度极值处稳定”，
    ''' 因此适合作为配准关键点；如果需要与 PCL 的结果逐点对拍，请改用 ISS/Harris。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(Σ n_s·k)</c>，各层点数按 <c>1/4^s</c> 递减，总体约为最细层的 4/3 倍。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class Sift3DKeypoint

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 检测 SIFT-3D 关键点。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="minScale">最细层体素尺寸；<c>≤ 0</c> 表示按平均点距自适应（2 倍）</param>
        ''' <param name="scaleFactor">层间尺度倍数（默认 2）</param>
        ''' <param name="scaleLevels">层数（默认 3；层数越多越慢）</param>
        ''' <param name="curvatureThreshold">响应阈值（尺度归一化曲率，默认 0.01）</param>
        ''' <param name="nonMaxRadius">原尺度上的抑制半径；<c>≤ 0</c> 表示自适应（4 倍最小层体素）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Detect(cloud As PointCloudBuffer, Optional minScale As Double = 0.0,
                                      Optional scaleFactor As Double = 2.0,
                                      Optional scaleLevels As Integer = 3,
                                      Optional curvatureThreshold As Double = 0.01,
                                      Optional nonMaxRadius As Double = 0.0,
                                      Optional options As CloudyOptions = Nothing) As KeypointSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If n = 0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim spacing As Double = cloud.EstimateAverageSpacing()
            Dim v0 As Double = If(minScale > 0.0, minScale, std.Max(spacing * 2.0, 1.0E-9))
            Dim levels As Integer = std.Max(2, scaleLevels)
            Dim factor As Double = std.Max(1.2, scaleFactor)

            ' 1) 构建尺度金字塔（每层：点云 + 曲率 + KD 树）
            Dim pyramids(levels - 1) As PointCloudBuffer
            Dim curvatures As Double()() = New Double(levels - 1)() {}
            Dim responses As Double()() = New Double(levels - 1)() {}
            Dim trees(levels - 1) As KdTree3D
            Dim voxels(levels - 1) As Double

            For s As Integer = 0 To levels - 1
                Dim v As Double = v0 * std.Pow(factor, s)

                voxels(s) = v
                pyramids(s) = If(s = 0, cloud, VoxelGridFilter.Downsample(cloud, v, opt))

                Dim estimated As PointCloudBuffer = NormalEstimation.Estimate(pyramids(s), 15, v * 2.5, opt)

                curvatures(s) = estimated.Curvature

                Dim resp As Double() = New Double(estimated.Count - 1) {}

                For i As Integer = 0 To resp.Length - 1
                    ' 尺度归一化：粗尺度的曲率天然更平滑，乘以尺度才能与细尺度可比
                    resp(i) = curvatures(s)(i) * v
                Next

                responses(s) = resp
                trees(s) = New KdTree3D(estimated)
            Next

            ' 2) 各层内部空间非极大 + 阈值
            Dim candidateIndices As New List(Of Integer)()
            Dim candidateScores As New List(Of Double)()
            Dim candidateScales As New List(Of Double)()

            Dim neighbor As New Neighborhood(32)
            Dim buffer As New Neighborhood(32)
            ' 原始点云的 KD 树只建一次：候选点可能有上千个，逐次重建是不可接受的开销
            Dim originalTree As New KdTree3D(cloud)

            For s As Integer = 0 To levels - 1
                Dim layer As PointCloudBuffer = pyramids(s)
                Dim resp As Double() = responses(s)
                Dim r As Double = voxels(s) * 2.0

                For i As Integer = 0 To layer.Count - 1
                    If resp(i) < curvatureThreshold Then Continue For

                    Call trees(s).RadiusSearch(layer.X(i), layer.Y(i), layer.Z(i), r, buffer, i, 64)

                    Dim isMax As Boolean = True

                    For j As Integer = 0 To buffer.Count - 1
                        If resp(buffer.Indices(j)) > resp(i) Then
                            isMax = False

                            Exit For
                        End If
                    Next

                    If Not isMax Then Continue For

                    ' 3) 跨尺度极值检验：与相邻层同位置的响应比较
                    Dim lowerResponse As Double = Double.MinValue
                    Dim upperResponse As Double = Double.MinValue

                    If s > 0 Then lowerResponse = ResponseNear(trees(s - 1), pyramids(s - 1), responses(s - 1), layer, i)
                    If s < levels - 1 Then upperResponse = ResponseNear(trees(s + 1), pyramids(s + 1), responses(s + 1), layer, i)

                    If resp(i) < lowerResponse OrElse resp(i) < upperResponse Then Continue For

                    ' 4) 映射回原始点云下标
                    Dim nearest As (index As Integer, distanceSquared As Double) =
                        originalTree.Nearest(layer.X(i), layer.Y(i), layer.Z(i))

                    If nearest.index < 0 Then Continue For

                    Call candidateIndices.Add(nearest.index)
                    Call candidateScores.Add(resp(i))
                    Call candidateScales.Add(voxels(s))
                Next
            Next

            If candidateIndices.Count = 0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            ' 5) 原尺度非极大抑制 + 去重（同一原始点可能被多层选中）
            Dim uniqueIndex As New Dictionary(Of Integer, Integer)()
            Dim uniquePoints As New List(Of Integer)()
            Dim uniqueScores As New List(Of Double)()
            Dim uniqueScales As New List(Of Double)()

            For i As Integer = 0 To candidateIndices.Count - 1
                Dim p As Integer = candidateIndices(i)
                Dim slot As Integer = -1

                If uniqueIndex.TryGetValue(p, slot) Then
                    ' 同一点在多尺度被检测到：保留响应更强的那次
                    If candidateScores(i) > uniqueScores(slot) Then
                        uniqueScores(slot) = candidateScores(i)
                        uniqueScales(slot) = candidateScales(i)
                    End If
                Else
                    uniqueIndex(p) = uniquePoints.Count
                    Call uniquePoints.Add(p)
                    Call uniqueScores.Add(candidateScores(i))
                    Call uniqueScales.Add(candidateScales(i))
                End If
            Next

            Dim nms As Double = If(nonMaxRadius > 0.0, nonMaxRadius, v0 * 4.0)
            Dim kept As Integer() = KeypointSuppression.Suppress(cloud, uniquePoints.ToArray(), uniqueScores.ToArray(), nms)
            Dim keptIndices(kept.Length - 1) As Integer
            Dim keptScores(kept.Length - 1) As Double
            Dim keptScales(kept.Length - 1) As Double

            For i As Integer = 0 To kept.Length - 1
                keptIndices(i) = uniquePoints(kept(i))
                keptScores(i) = uniqueScores(kept(i))
                keptScales(i) = uniqueScales(kept(i))
            Next

            Dim result As New KeypointSet(keptIndices, keptScores, keptScales)

            Call result.SortByScore()

            Return result
        End Function

        ''' <summary>
        ''' 在另一层的同位置取响应（最近邻查找，半径不设限）。
        ''' </summary>
        Private Shared Function ResponseNear(tree As KdTree3D, layer As PointCloudBuffer, responses As Double(),
                                             source As PointCloudBuffer, index As Integer) As Double
            Dim nearest As (index As Integer, distanceSquared As Double) =
                tree.Nearest(source.X(index), source.Y(index), source.Z(index))

            If nearest.index < 0 Then Return Double.MinValue

            Return responses(nearest.index)
        End Function

    End Class
End Namespace
