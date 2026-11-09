Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' SHOT(Signature of Histograms of Orientations)描述子：352 维局部特征。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>与 FPFH 的差别</b>：FPFH 只用“点对之间的三个角度量”，对法向的绝对朝向不敏感；
    ''' SHOT 显式构造<b>局部参考坐标系(LRF)</b>，把邻域点投到球坐标网格里，
    ''' 再分别统计各扇区的法向分布。因此 SHOT 更强（描述能力更高），
    ''' 代价是它对 LRF 的构造稳定性敏感。
    ''' </para>
    ''' <para>
    ''' <b>LRF 与符号消歧</b>：用邻域协方差的前两个主方向与法向构成 <c>(x, y, z)</c>，
    ''' 但特征向量的符号仍然任意。SHOT 的原始做法是：对每个轴，
    ''' 统计邻域内“点相对中心在该轴方向上的投影符号”，
    ''' 若负向点更多则翻转该轴。本实现遵循该做法（这也是 SHOT 对噪声有一定鲁棒性的来源）。
    ''' </para>
    ''' <para>
    ''' <b>维度</b>：32 个方位扇区 × 11 个径向区间 = 352 维。
    ''' 仰角方向按原始论文用于<b>加权</b>（<c>cos²</c> 权重）而不是独立分箱，
    ''' 因此维度是 352 而不是 32×8×11。这一点与 PCL 的 <c>SHOT352</c> 一致。
    ''' </para>
    ''' <para>
    ''' <b>适用场景</b>：纹理单一的金属件、需要高辨识度的位姿估计
    ''' （位姿假设生成 + 验证）。实时性要求高的场景应改用 FPFH（33 维，快 5~10 倍）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class ShotDescriptor

        ''' <summary>SHOT 维度（32 方位 × 11 径向）。</summary>
        Public Const Dimension As Integer = 352

        Private Const AzimuthBins As Integer = 32
        Private Const RadialBins As Integer = 11

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 计算 SHOT 描述子。
        ''' </summary>
        ''' <param name="cloud">输入点云（会自动补齐法向）</param>
        ''' <param name="keypoints">需要计算的点下标；<c>Nothing</c> 表示全点云</param>
        ''' <param name="radius">支撑球半径；<c>≤ 0</c> 表示自适应（8 倍点距）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Compute(cloud As PointCloudBuffer, Optional keypoints As Integer() = Nothing,
                                       Optional radius As Double = 0.0,
                                       Optional options As CloudyOptions = Nothing) As FeatureCloud
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 20, 0.0, opt)
            End If

            Dim targets As Integer()

            If keypoints Is Nothing Then
                targets = New Integer(source.Count - 1) {}

                For i As Integer = 0 To targets.Length - 1
                    targets(i) = i
                Next
            Else
                targets = keypoints
            End If

            If targets.Length = 0 Then
                Return New FeatureCloud(New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)()),
                                        Array.Empty(Of Double())())
            End If

            Dim r As Double = If(radius > 0.0, radius, std.Max(source.EstimateAverageSpacing() * 8.0, 1.0E-9))
            Dim tree As New KdTree3D(source)
            Dim rows As Double()() = New Double(targets.Length - 1)() {}
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree, 256))

            Try
                Call opt.ForRange(targets.Length,
                    Sub(start, ends)
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()
                        Dim hist As Double() = New Double(Dimension - 1) {}

                        For k As Integer = start To ends - 1
                            Dim p As Integer = targets(k)

                            Array.Clear(hist, 0, hist.Length)

                            Call ComputeInto(source, tree, buffer, p, r, hist)

                            Call SimdHistogram.Normalize(hist)

                            rows(k) = hist
                        Next
                    End Sub, 32)
            Finally
                Call pool.Dispose()
            End Try

            Dim scores As Double() = New Double(targets.Length - 1) {}

            Return New FeatureCloud(New KeypointSet(targets, scores), rows)
        End Function

        ''' <summary>
        ''' 单点 SHOT 计算（写入调用方直方图）。
        ''' </summary>
        Private Shared Sub ComputeInto(cloud As PointCloudBuffer, tree As KdTree3D, buffer As Neighborhood,
                                       index As Integer, radius As Double, hist As Double())
            Call tree.RadiusSearch(cloud.X(index), cloud.Y(index), cloud.Z(index), radius, buffer, index, 512)

            Dim count As Integer = buffer.Count

            If count < 5 Then Return

            ' ---- 1) LRF：邻域协方差的特征向量 + 符号消歧 ----
            Dim cx As Double = cloud.X(index)
            Dim cy As Double = cloud.Y(index)
            Dim cz As Double = cloud.Z(index)

            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

            For j As Integer = 0 To count - 1
                Dim q As Integer = buffer.Indices(j)
                Dim dx As Double = cloud.X(q) - cx
                Dim dy As Double = cloud.Y(q) - cy
                Dim dz As Double = cloud.Z(q) - cz

                cxx += dx * dx
                cxy += dx * dy
                cxz += dx * dz
                cyy += dy * dy
                cyz += dy * dz
                czz += dz * dz
            Next

            Dim inv As Double = 1.0 / count
            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                cxx * inv, cxy * inv, cxz * inv, cyy * inv, cyz * inv, czz * inv)

            Dim axisX As Vector3d = eigen.V1
            Dim axisY As Vector3d = eigen.V2
            Dim axisZ As Vector3d = eigen.V3

            ' 符号消歧：统计投影符号，多数为负则翻转
            Dim sx As Double = 0, sy As Double = 0, sz As Double = 0

            For j As Integer = 0 To count - 1
                Dim q As Integer = buffer.Indices(j)
                Dim d As New Vector3d(cloud.X(q) - cx, cloud.Y(q) - cy, cloud.Z(q) - cz)

                If Vector3d.Dot(d, axisX) < 0 Then sx += 1 Else sx -= 1
                If Vector3d.Dot(d, axisY) < 0 Then sy += 1 Else sy -= 1
                If Vector3d.Dot(d, axisZ) < 0 Then sz += 1 Else sz -= 1
            Next

            If sx > 0 Then axisX = -axisX
            If sy > 0 Then axisY = -axisY
            If sz > 0 Then axisZ = -axisZ

            ' ---- 2) 球坐标分箱投票 ----
            For j As Integer = 0 To count - 1
                Dim q As Integer = buffer.Indices(j)
                Dim dx As Double = cloud.X(q) - cx
                Dim dy As Double = cloud.Y(q) - cy
                Dim dz As Double = cloud.Z(q) - cz

                Dim lx As Double = dx * axisX.X + dy * axisX.Y + dz * axisX.Z
                Dim ly As Double = dx * axisY.X + dy * axisY.Y + dz * axisY.Z
                Dim lz As Double = dx * axisZ.X + dy * axisZ.Y + dz * axisZ.Z

                ' 折到上半球：SHOT 只对朝向参考帧正侧的点建模
                If lz < 0.0 Then
                    lx = -lx
                    ly = -ly
                    lz = -lz
                End If

                Dim distance As Double = std.Sqrt(lx * lx + ly * ly + lz * lz)

                If distance <= 1.0E-12 OrElse distance > radius Then Continue For

                Dim azimuth As Double = std.Atan2(ly, lx)

                If azimuth < 0.0 Then azimuth += 2.0 * std.PI

                Dim azimuthBin As Integer = CInt(std.Floor(azimuth / (2.0 * std.PI) * AzimuthBins))
                Dim radialBin As Integer = CInt(std.Floor(distance / radius * RadialBins))

                If azimuthBin >= AzimuthBins Then azimuthBin = AzimuthBins - 1
                If radialBin >= RadialBins Then radialBin = RadialBins - 1
                If azimuthBin < 0 Then azimuthBin = 0
                If radialBin < 0 Then radialBin = 0

                ' 仰角加权：越接近赤道（与法向垂直）权重越大，抑制极点处的方位歧义
                Dim ratio As Double = lz / distance

                If ratio > 1.0 Then ratio = 1.0
                If ratio < -1.0 Then ratio = -1.0

                Dim cosElevation As Double = std.Sqrt(std.Max(0.0, 1.0 - ratio * ratio))
                Dim weight As Double = cosElevation * cosElevation

                If weight <= 0.0 Then weight = 1.0E-6

                hist(azimuthBin * RadialBins + radialBin) += weight
            Next
        End Sub

    End Class
End Namespace
