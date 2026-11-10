Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' PFH / SPFH / FPFH(Fast Point Feature Histograms，33 维)描述子。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>特征定义</b>：对点对 <c>(p, q)</c> 构造 Darboux 坐标系
    ''' <c>u = n_p</c>、<c>v = normalize((q−p) × u)</c>、<c>w = u × v</c>，
    ''' 然后取三个角度量：
    ''' </para>
    ''' <code>
    ''' α = v·n_q
    ''' φ = u·(q−p)/‖q−p‖
    ''' θ = atan2(w·n_q, u·n_q)
    ''' </code>
    ''' <para>
    ''' 每个量分 11 个 bin，拼成 33 维直方图。这三个量对刚体变换不变，
    ''' 对采样密度和噪声相对鲁棒（角度只依赖法向与相对位置），
    ''' 因此在工业配准里长期是“默认描述子”。
    ''' </para>
    ''' <para>
    ''' <b>PFH → SPFH → FPFH</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>PFH 统计邻域内<b>所有点对</b>，复杂度 <c>O(k²)</c>，k=30 时是 900 次特征计算/点，太慢。</description></item>
    ''' <item><description>SPFH（Simplified PFH）只统计 <c>p</c> 与邻点的点对，<c>O(k)</c>，但丢失了邻域内部结构。</description></item>
    ''' <item><description>FPFH 用“邻域的 SPFH 加权和”补偿：<c>FPFH(p) = SPFH(p) + (1/k)·Σ (1/d_q)·SPFH(q)</c>，
    ''' 复杂度仍是 <c>O(k)</c>，信息量接近 PFH。这就是它成为工业主流的原因。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>内存</b>：FPFH 需要邻域的 SPFH，因此实现里缓存已计算的 SPFH 行
    ''' （每行 33 × 8 字节）。全点云计算时内存约为 <c>264·n</c> 字节（100 万点约 270MB）；
    ''' 只对关键点计算时会按实际触达的点缓存。
    ''' </para>
    ''' <para>
    ''' <b>归一化</b>：本实现把直方图归一化为概率分布（总和 1）。
    ''' PCL 归一化到总和 100；两者在欧氏距离/比值检验下等价（差一个固定缩放）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class FpfhDescriptor

        ''' <summary>FPFH 维度（11 × 3）。</summary>
        Public Const Dimension As Integer = 33

        ''' <summary>每个角度量的 bin 数。</summary>
        Public Const BinsPerFeature As Integer = 11

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 计算 FPFH 描述子。
        ''' </summary>
        ''' <param name="cloud">输入点云（会自动补齐法向）</param>
        ''' <param name="keypoints">需要计算描述子的点下标；<c>Nothing</c> 表示全点云</param>
        ''' <param name="spfhRadius">SPFH 邻域半径；<c>≤ 0</c> 表示自适应（5 倍点距）</param>
        ''' <param name="fpfhRadius">FPFH 加权邻域半径；<c>≤ 0</c> 表示自适应（10 倍点距）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Compute(cloud As PointCloudBuffer, Optional keypoints As Integer() = Nothing,
                                       Optional spfhRadius As Double = 0.0,
                                       Optional fpfhRadius As Double = 0.0,
                                       Optional options As CloudyOptions = Nothing) As FeatureCloud
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 15, 0.0, opt)
            End If

            Dim targets As Integer() = If(keypoints, AllIndices(source.Count))
            Dim nTargets As Integer = targets.Length

            If nTargets = 0 Then
                Return New FeatureCloud(New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)()),
                                        Array.Empty(Of Double())())
            End If

            Dim spacing As Double = source.EstimateAverageSpacing()
            Dim rSpfh As Double = If(spfhRadius > 0.0, spfhRadius, std.Max(spacing * 5.0, 1.0E-9))
            Dim rFpfh As Double = If(fpfhRadius > 0.0, fpfhRadius, std.Max(spacing * 10.0, 1.0E-9))

            Dim tree As New KdTree3D(source)
            Dim cache As New SpfhCache(source, tree, rSpfh)
            Dim rows As Double()() = New Double(nTargets - 1)() {}

            Try
                Call opt.ForRange(nTargets,
                    Sub(start, ends)
                        Dim buffer As Neighborhood = cache.RentNeighborhood()
                        Dim hist As Double() = New Double(Dimension - 1) {}
                        Dim weight As Double() = New Double(Dimension - 1) {}

                        For k As Integer = start To ends - 1
                            Dim p As Integer = targets(k)

                            Call cache.GetSpfh(p, hist)

                            ' FPFH 的加权项：邻域 SPFH 的 (1/d) 加权平均
                            For j As Integer = 0 To Dimension - 1
                                weight(j) = hist(j)
                            Next

                            Call tree.RadiusSearch(source.X(p), source.Y(p), source.Z(p), rFpfh, buffer, p, 256)

                            Dim used As Integer = 0

                            For j As Integer = 0 To buffer.Count - 1
                                Dim q As Integer = buffer.Indices(j)
                                Dim d As Double = std.Sqrt(buffer.Distances(j))

                                If d <= 1.0E-12 Then Continue For

                                Dim neighbor As Double() = cache.GetSpfhShared(q)
                                Dim invD As Double = 1.0 / d

                                For f As Integer = 0 To Dimension - 1
                                    weight(f) += neighbor(f) * invD
                                Next

                                used += 1
                            Next

                            If used > 0 Then
                                Dim invUsed As Double = 1.0 / used

                                For f As Integer = 0 To Dimension - 1
                                    weight(f) *= invUsed
                                Next
                            End If

                            Call SimdHistogram.Normalize(weight)

                            rows(k) = weight
                        Next
                    End Sub, 32)
            Finally
                Call cache.Dispose()
            End Try

            Dim scores As Double() = New Double(nTargets - 1) {}

            Return New FeatureCloud(New KeypointSet(targets, scores), rows)
        End Function

        ''' <summary>
        ''' 计算 PFH（邻域内所有点对的 33 维直方图）。
        ''' </summary>
        ''' <remarks>
        ''' <b>复杂度 <c>O(k²)</c></b>：k=20 时每个点约 190 次特征计算，比 FPFH 慢一个数量级。
        ''' 保留它主要用于“与 FPFH 对比验证”以及极小邻域下的精细特征。
        ''' </remarks>
        Public Shared Function ComputePfh(cloud As PointCloudBuffer, Optional keypoints As Integer() = Nothing,
                                          Optional radius As Double = 0.0,
                                          Optional options As CloudyOptions = Nothing) As FeatureCloud
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 15, 0.0, opt)
            End If

            Dim targets As Integer() = If(keypoints, AllIndices(source.Count))
            Dim r As Double = If(radius > 0.0, radius, std.Max(source.EstimateAverageSpacing() * 5.0, 1.0E-9))
            Dim tree As New KdTree3D(source)
            Dim rows As Double()() = New Double(targets.Length - 1)() {}

            Call opt.ForRange(targets.Length,
                Sub(start, ends)
                    Dim buffer As Neighborhood = New Neighborhood(64)

                    For k As Integer = start To ends - 1
                        Dim p As Integer = targets(k)
                        Dim hist As Double() = New Double(Dimension - 1) {}

                        Call tree.RadiusSearch(source.X(p), source.Y(p), source.Z(p), r, buffer, p, 128)

                        For a As Integer = 0 To buffer.Count - 1
                            For b As Integer = a + 1 To buffer.Count - 1
                                Call AddPairFeature(source, p, buffer.Indices(a), hist)
                                Call AddPairFeature(source, buffer.Indices(a), buffer.Indices(b), hist)
                            Next
                        Next

                        Call SimdHistogram.Normalize(hist)

                        rows(k) = hist
                    Next
                End Sub, 8)

            Dim scores As Double() = New Double(targets.Length - 1) {}

            Return New FeatureCloud(New KeypointSet(targets, scores), rows)
        End Function

        Private Shared Function AllIndices(n As Integer) As Integer()
            Dim indices(n - 1) As Integer

            For i As Integer = 0 To n - 1
                indices(i) = i
            Next

            Return indices
        End Function

        ''' <summary>
        ''' SPFH 缓存：按需计算并保存每个点的 SPFH 行。
        ''' </summary>
        Private NotInheritable Class SpfhCache

            Private ReadOnly m_cloud As PointCloudBuffer
            Private ReadOnly m_tree As KdTree3D
            Private ReadOnly m_radius As Double
            Private ReadOnly m_rows As New Dictionary(Of Integer, Double())
            Private ReadOnly m_sync As New Object()
            ''' <summary>调用方使用的邻域缓冲（外层循环复用）。</summary>
            Private ReadOnly m_outerPool As ThreadLocal(Of Neighborhood)
            ''' <summary>内部 SPFH 计算使用的邻域缓冲（与外层缓冲分离，避免互相覆盖）。</summary>
            Private ReadOnly m_innerPool As ThreadLocal(Of Neighborhood)

            Public Sub New(cloud As PointCloudBuffer, tree As KdTree3D, radius As Double)
                m_cloud = cloud
                m_tree = tree
                m_radius = radius
                m_outerPool = New ThreadLocal(Of Neighborhood)(Function() New Neighborhood(64))
                m_innerPool = New ThreadLocal(Of Neighborhood)(Function() New Neighborhood(64))
            End Sub

            Public Function RentNeighborhood() As Neighborhood
                Return m_outerPool.Value
            End Function

            Public Sub Dispose()
                Call m_outerPool.Dispose()
                Call m_innerPool.Dispose()
            End Sub

            ''' <summary>
            ''' 取第 <paramref name="index"/> 个点的 SPFH（写入调用方缓冲，避免返回内部数组）。
            ''' </summary>
            Public Sub GetSpfh(index As Integer, dst As Double())
                Dim row As Double() = GetOrCreate(index)

                Array.Copy(row, dst, FpfhDescriptor.Dimension)
            End Sub

            ''' <summary>
            ''' 取 SPFH 行（共享引用，<b>只读</b>）。
            ''' </summary>
            Public Function GetSpfhShared(index As Integer) As Double()
                Return GetOrCreate(index)
            End Function

            Private Function GetOrCreate(index As Integer) As Double()
                SyncLock m_sync
                    Dim cached As Double() = Nothing

                    If m_rows.TryGetValue(index, cached) Then Return cached
                End SyncLock

                Dim hist As Double() = New Double(FpfhDescriptor.Dimension - 1) {}
                Dim buffer As Neighborhood = m_innerPool.Value

                Call m_tree.RadiusSearch(m_cloud.X(index), m_cloud.Y(index), m_cloud.Z(index), m_radius, buffer, index, 256)

                For j As Integer = 0 To buffer.Count - 1
                    Call AddPairFeature(m_cloud, index, buffer.Indices(j), hist)
                Next

                Call SimdHistogram.Normalize(hist)

                SyncLock m_sync
                    Dim existing As Double() = Nothing

                    If m_rows.TryGetValue(index, existing) Then Return existing

                    m_rows(index) = hist
                End SyncLock

                Return hist
            End Function
        End Class

        ''' <summary>
        ''' 把点对 <c>(p, q)</c> 的三个角度量投票进直方图。
        ''' </summary>
        Private Shared Sub AddPairFeature(cloud As PointCloudBuffer, p As Integer, q As Integer, hist As Double())
            Dim np As Vector3d = cloud.Normal(p)
            Dim nq As Vector3d = cloud.Normal(q)

            If np.IsZero(1.0E-12) OrElse nq.IsZero(1.0E-12) Then Return

            Dim link As New Vector3d(cloud.X(q) - cloud.X(p), cloud.Y(q) - cloud.Y(p), cloud.Z(q) - cloud.Z(p))
            Dim distance As Double = link.Length

            If distance <= 1.0E-12 OrElse distance > 1.0E12 Then Return

            Dim u As Vector3d = np.Normalize()
            Dim side As Vector3d = Vector3d.Cross(link, u)

            If side.IsZero(1.0E-12) Then Return

            Dim v As Vector3d = side.Normalize()
            Dim w As Vector3d = Vector3d.Cross(u, v)

            Dim alpha As Double = Vector3d.Dot(v, nq)
            Dim phi As Double = Vector3d.Dot(u, link) / distance
            Dim theta As Double = std.Atan2(Vector3d.Dot(w, nq), Vector3d.Dot(u, nq))

            Dim binA As Integer = ToBin(alpha, -1.0, 1.0)
            Dim binP As Integer = ToBin(phi, -1.0, 1.0)
            Dim binT As Integer = ToBin(theta, -std.PI, std.PI)

            hist(binA) += 1
            hist(BinsPerFeature + binP) += 1
            hist(BinsPerFeature * 2 + binT) += 1
        End Sub

        ''' <summary>
        ''' 把区间量映射到 <c>[0, BinsPerFeature)</c>。
        ''' </summary>
        Private Shared Function ToBin(value As Double, min As Double, max As Double) As Integer
            Dim bin As Integer = CInt(std.Floor((value - min) / (max - min) * BinsPerFeature))

            If bin < 0 Then Return 0
            If bin >= BinsPerFeature Then Return BinsPerFeature - 1

            Return bin
        End Function

    End Class
End Namespace
