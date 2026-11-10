Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 移动最小二乘(MLS)平滑：把每个点投影到邻域拟合的局部平面上，并可顺带重算法向。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么需要它</b>：结构光/激光扫描的噪声主要是<b>沿法向</b>的抖动
    ''' （约 0.5~2 倍传感器精度）。直接对这种数据做曲率、法向、法向微分（主曲率）
    ''' 会把噪声放大成“虚假的高曲率边缘”，进而毁掉区域生长与特征匹配。
    ''' MLS 用邻域的加权平面把点拉回“光滑表面”，同时保留原始切向位置，
    ''' 因此<b>不会</b>像高斯/均值滤波那样把尖锐倒角磨圆。
    ''' </para>
    ''' <para>
    ''' <b>算法</b>（对每个点 <c>p</c>）：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>取半径 <c>r</c>（或 k 近邻）内的邻点，按 <c>w = EXP(−d²/2σ²)</c> 加权。</description></item>
    ''' <item><description>加权质心 <c>c</c> 与加权协方差 <c>C</c>（<see cref="Linalg.EigenSymmetric3"/>）。</description></item>
    ''' <item><description>最小特征向量即平面法向 <c>n</c>。</description></item>
    ''' <item><description>投影：<c>p' = p − ((p − c)·n)·n</c>。</description></item>
    ''' </list>
    ''' <para>
    ''' 这是“单次投影”版本（不迭代重加权），对噪声水平 ≤ 1/4 邻域半径的扫描数据足够；
    ''' 噪声更大时应该先换更好的硬件，而不是继续迭代 MLS（迭代会把薄壁两侧拉成一片）。
    ''' </para>
    ''' <para>
    ''' <b>上采样</b>：<see cref="Upsample(PointCloudBuffer, Double, Double, Integer, CloudyOptions)"/>
    ''' 在邻点连线的中点插入新点并同样投影到局部平面，用于把稀疏扫描“加密”到
    ''' 足以支撑泊松重建/网格化的密度。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class MlsSmoother

        Private Sub New()
        End Sub

        ''' <summary>
        ''' MLS 平滑（仅移动坐标）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="radius">邻域半径；<c>≤ 0</c> 表示按平均点距自适应（≈3 倍点距）</param>
        ''' <param name="recomputeNormals">是否用拟合平面法向替换现有点云的法向</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Smooth(cloud As PointCloudBuffer, Optional radius As Double = 0.0,
                                      Optional recomputeNormals As Boolean = True,
                                      Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then Return cloud.Clone()

            Dim r As Double = If(radius > 0.0, radius, cloud.EstimateAverageSpacing() * 3.0)

            If r <= 0.0 Then Return cloud.Clone()

            Dim tree As New KdTree3D(cloud)
            Dim out As PointCloudBuffer = PointCloudBuffer.NewCloud(n)

            Dim normalsX As Double() = Nothing
            Dim normalsY As Double() = Nothing
            Dim normalsZ As Double() = Nothing

            If recomputeNormals Then
                normalsX = GC.AllocateUninitializedArray(Of Double)(n)
                normalsY = GC.AllocateUninitializedArray(Of Double)(n)
                normalsZ = GC.AllocateUninitializedArray(Of Double)(n)
            End If

            Dim sigma As Double = r * 0.5
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()

                        For i As Integer = start To ends - 1
                            Call tree.RadiusSearch(cloud.X(i), cloud.Y(i), cloud.Z(i), r, buffer, i)

                            If buffer.Count < 3 Then
                                ' 邻域不足以定义平面：保持原点位（不引入虚假形状）
                                out.X(i) = cloud.X(i)
                                out.Y(i) = cloud.Y(i)
                                out.Z(i) = cloud.Z(i)

                                If recomputeNormals Then
                                    normalsX(i) = If(cloud.HasNormals, cloud.Nx(i), 0.0)
                                    normalsY(i) = If(cloud.HasNormals, cloud.Ny(i), 0.0)
                                    normalsZ(i) = If(cloud.HasNormals, cloud.Nz(i), 0.0)
                                End If

                                Continue For
                            End If

                            Dim wx As Double = 0, wy As Double = 0, wz As Double = 0, wsum As Double = 0
                            Dim inv2Sigma2 As Double = -1.0 / (2.0 * sigma * sigma)

                            For k As Integer = 0 To buffer.Count - 1
                                Dim p As Integer = buffer.Indices(k)
                                Dim w As Double = std.Exp(buffer.Distances(k) * inv2Sigma2)

                                wx += cloud.X(p) * w
                                wy += cloud.Y(p) * w
                                wz += cloud.Z(p) * w
                                wsum += w
                            Next

                            If wsum <= 1.0E-15 Then
                                out.X(i) = cloud.X(i)
                                out.Y(i) = cloud.Y(i)
                                out.Z(i) = cloud.Z(i)

                                Continue For
                            End If

                            Dim cx As Double = wx / wsum
                            Dim cy As Double = wy / wsum
                            Dim cz As Double = wz / wsum

                            ' 加权协方差（以加权质心为中心，两趟写法保证大坐标下的精度）
                            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
                            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

                            For k As Integer = 0 To buffer.Count - 1
                                Dim p As Integer = buffer.Indices(k)
                                Dim w As Double = std.Exp(buffer.Distances(k) * inv2Sigma2)
                                Dim dx As Double = cloud.X(p) - cx
                                Dim dy As Double = cloud.Y(p) - cy
                                Dim dz As Double = cloud.Z(p) - cz

                                cxx += w * dx * dx
                                cxy += w * dx * dy
                                cxz += w * dx * dz
                                cyy += w * dy * dy
                                cyz += w * dy * dz
                                czz += w * dz * dz
                            Next

                            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                                cxx / wsum, cxy / wsum, cxz / wsum, cyy / wsum, cyz / wsum, czz / wsum)
                            Dim normal As Vector3d = eigen.V3

                            ' 保持法向朝向：与原始法向同向时更可能正确（未定向数据则不动）
                            If cloud.HasNormals Then
                                Dim oldNormal As New Vector3d(cloud.Nx(i), cloud.Ny(i), cloud.Nz(i))

                                If Vector3d.Dot(oldNormal, normal) < 0.0 Then normal = -normal
                            End If

                            Dim px As Double = cloud.X(i) - cx
                            Dim py As Double = cloud.Y(i) - cy
                            Dim pz As Double = cloud.Z(i) - cz
                            Dim dist As Double = px * normal.X + py * normal.Y + pz * normal.Z

                            out.X(i) = cloud.X(i) - dist * normal.X
                            out.Y(i) = cloud.Y(i) - dist * normal.Y
                            out.Z(i) = cloud.Z(i) - dist * normal.Z

                            If recomputeNormals Then
                                normalsX(i) = normal.X
                                normalsY(i) = normal.Y
                                normalsZ(i) = normal.Z
                            End If
                        Next
                    End Sub, 256)
            Finally
                Call pool.Dispose()
            End Try

            If recomputeNormals Then Call out.SetNormals(normalsX, normalsY, normalsZ)

            Return out
        End Function

        ''' <summary>
        ''' MLS 上采样：在邻点连线中点插入新点，并投影到局部平面。
        ''' </summary>
        ''' <param name="cloud">输入点云（应已平滑）</param>
        ''' <param name="spacing">新点的最小间距（用于去重，通常取目标平均点距）</param>
        ''' <param name="radius">邻域半径（<c>≤ 0</c> 表示自适应）</param>
        ''' <param name="maxNewPerPoint">每个点最多贡献多少新点（控制总量与耗时）</param>
        ''' <param name="options">并行选项</param>
        ''' <remarks>
        ''' <para>
        ''' 密度提升是泊松重建/网格化的前置条件：三角化算法需要“点间距 ≤ 特征尺寸/2”。
        ''' 这里采用最简单可靠的中点细分——新点原本就落在原始表面上（弦中点），
        ''' 再沿法向投到 MLS 平面上，因此不会产生虚假的凸起。
        ''' </para>
        ''' <para>
        ''' <b>去重</b>：用输入点间距的体素哈希过滤重复点；
        ''' 否则同一条边会被相邻的两个点同时生成，点数会爆炸式增长。
        ''' </para>
        ''' </remarks>
        Public Shared Function Upsample(cloud As PointCloudBuffer, spacing As Double,
                                        Optional radius As Double = 0.0,
                                        Optional maxNewPerPoint As Integer = 3,
                                        Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If spacing <= 0.0 Then Throw New ArgumentOutOfRangeException(NameOf(spacing), "the spacing should be positive!")

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim n As Integer = cloud.Count

            If n = 0 Then Return cloud.Clone()

            Dim r As Double = If(radius > 0.0, radius, spacing * 4.0)
            Dim tree As New KdTree3D(cloud)
            Dim halfSpacing As Double = spacing * 0.5
            Dim seen As New VoxelHash(cloud, std.Max(halfSpacing, 1.0E-9))
            Dim used As New HashSet(Of Long)()

            For i As Integer = 0 To n - 1
                Call used.Add(seen.KeyAt(i))
            Next

            Dim newX As New List(Of Double)(n)
            Dim newY As New List(Of Double)(n)
            Dim newZ As New List(Of Double)(n)
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree))
            Dim sync As New Object()

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()
                        Dim localX As New List(Of Double)(64)
                        Dim localY As New List(Of Double)(64)
                        Dim localZ As New List(Of Double)(64)

                        For i As Integer = start To ends - 1
                            Call tree.RadiusSearch(cloud.X(i), cloud.Y(i), cloud.Z(i), r, buffer, i)

                            Dim added As Integer = 0

                            For k As Integer = 0 To buffer.Count - 1
                                If added >= maxNewPerPoint Then Exit For

                                Dim p As Integer = buffer.Indices(k)
                                Dim mx As Double = (cloud.X(i) + cloud.X(p)) * 0.5
                                Dim my As Double = (cloud.Y(i) + cloud.Y(p)) * 0.5
                                Dim mz As Double = (cloud.Z(i) + cloud.Z(p)) * 0.5
                                Dim dx As Double = cloud.X(p) - cloud.X(i)
                                Dim dy As Double = cloud.Y(p) - cloud.Y(i)
                                Dim dz As Double = cloud.Z(p) - cloud.Z(i)

                                ' 太近的邻点，中点会与原点重合，没有加密意义
                                If dx * dx + dy * dy + dz * dz < spacing * spacing * 0.25 Then Continue For

                                Dim key As Long = seen.KeyOf(mx, my, mz)

                                SyncLock sync
                                    If used.Contains(key) Then Continue For

                                    Call used.Add(key)
                                End SyncLock

                                Call localX.Add(mx)
                                Call localY.Add(my)
                                Call localZ.Add(mz)
                                added += 1
                            Next
                        Next

                        SyncLock sync
                            Call newX.AddRange(localX)
                            Call newY.AddRange(localY)
                            Call newZ.AddRange(localZ)
                        End SyncLock
                    End Sub, 512)
            Finally
                Call pool.Dispose()
            End Try

            If newX.Count = 0 Then Return cloud.Clone()

            Dim extra As PointCloudBuffer = PointCloudBuffer.Create(newX.ToArray(), newY.ToArray(), newZ.ToArray())

            Return PointCloudBuffer.Merge(cloud, extra)
        End Function

    End Class
End Namespace
