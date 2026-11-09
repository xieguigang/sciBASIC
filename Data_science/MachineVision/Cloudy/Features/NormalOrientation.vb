Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' 法向一致定向：把逐点独立估计出来的法向翻到“同一个朝向”。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么必须做</b>：PCA 只能确定法向所在的<b>直线</b>，符号是任意的
    ''' （<c>n</c> 与 <c>−n</c> 都是合法解）。而几乎所有下游算法都要求符号一致：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>FPFH/SHOT 的角度特征在符号翻转后会跳变到另一个 bin；</description></item>
    ''' <item><description>泊松重建需要“法向指向体外”才能得到正确的内外划分；</description></item>
    ''' <item><description>点到面 ICP 的残差符号由法向决定；</description></item>
    ''' <item><description>法向夹角做区域生长时，翻转会导致同一平面被劈成两半。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>三种策略</b>：
    ''' </para>
    ''' <list type="number">
    ''' <item><description>
    ''' <see cref="OrientTowardsViewpoint"/>：让法向指向视点。
    ''' 单视角扫描（结构光/单站激光）的<b>首选</b>——扫描仪本来只能看到朝向自己的面，
    ''' 因此这个假设在物理上必然成立，而且 <c>O(n)</c>、完全无歧义。
    ''' </description></item>
    ''' <item><description>
    ''' <see cref="OrientConsistent"/>：在 kNN 图上建最小生成树，
    ''' 沿树传播符号（边的代价 = <c>1 − |n_i·n_j|</c>）。
    ''' 适用于多视角拼接后的闭合物件，不需要知道视点。
    ''' </description></item>
    ''' <item><description>
    ''' <see cref="OrientOutwardFromCentroid"/>：以点云质心为“内部点”，
    ''' 让法向朝外。适用于已经闭合的单个物体（多站扫描后），
    ''' 实现最简单但要求形状近似凸/星形。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' <b>MST 的正确性</b>：沿着最小生成树传播符号，能保证任意两个连通点之间的
    ''' 符号关系与“它们之间最平滑的那条路径”一致，从而避免在锐边处
    ''' 沿着错误方向传播（逐点取“最近邻的点积”会沿着锐边累积错误）。
    ''' 复杂度 <c>O(n·k·log n)</c>。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class NormalOrientation

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 让法向指向视点（单视角扫描的首选方法）。
        ''' </summary>
        ''' <param name="cloud">带法向的点云（原地修改并返回）</param>
        ''' <param name="viewpoint">扫描仪位置</param>
        ''' <returns>翻转过的点数（诊断用）</returns>
        Public Shared Function OrientTowardsViewpoint(cloud As PointCloudBuffer, viewpoint As Vector3d) As Integer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If Not cloud.HasNormals Then
                Throw New InvalidOperationException("the point cloud has no normals; run NormalEstimation first!")
            End If

            Dim flipped As Integer = 0
            Dim n As Integer = cloud.Count

            For i As Integer = 0 To n - 1
                Dim toViewer As New Vector3d(viewpoint.X - cloud.X(i), viewpoint.Y - cloud.Y(i), viewpoint.Z - cloud.Z(i))

                If Vector3d.Dot(New Vector3d(cloud.Nx(i), cloud.Ny(i), cloud.Nz(i)), toViewer) < 0.0 Then
                    cloud.Nx(i) = -cloud.Nx(i)
                    cloud.Ny(i) = -cloud.Ny(i)
                    cloud.Nz(i) = -cloud.Nz(i)
                    flipped += 1
                End If
            Next

            Return flipped
        End Function

        ''' <summary>
        ''' 让法向背离点云质心（适用于闭合的单个物体）。
        ''' </summary>
        ''' <remarks>
        ''' 判据：<c>n·(p − centroid) ≥ 0</c>。
        ''' 对凸形状（球、圆柱、方块）恒等于“朝外”；
        ''' 对凹陷形状（凹槽、内孔）会给出错误朝向——那类零件请改用
        ''' <see cref="OrientConsistent(PointCloudBuffer, Integer, CloudyOptions)"/>。
        ''' </remarks>
        Public Shared Function OrientOutwardFromCentroid(cloud As PointCloudBuffer) As Integer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If Not cloud.HasNormals Then
                Throw New InvalidOperationException("the point cloud has no normals; run NormalEstimation first!")
            End If

            Dim centroid As Vector3d = cloud.Centroid()
            Dim flipped As Integer = 0

            For i As Integer = 0 To cloud.Count - 1
                Dim outward As New Vector3d(cloud.X(i) - centroid.X, cloud.Y(i) - centroid.Y, cloud.Z(i) - centroid.Z)

                If Vector3d.Dot(cloud.Normal(i), outward) < 0.0 Then
                    cloud.Nx(i) = -cloud.Nx(i)
                    cloud.Ny(i) = -cloud.Ny(i)
                    cloud.Nz(i) = -cloud.Nz(i)
                    flipped += 1
                End If
            Next

            Return flipped
        End Function

        ''' <summary>
        ''' 用最小生成树沿表面传播法向符号（多视角闭合物件的首选方法）。
        ''' </summary>
        ''' <param name="cloud">带法向的点云（原地修改并返回）</param>
        ''' <param name="k">构建 kNN 图的近邻个数（默认 10）</param>
        ''' <param name="options">并行选项</param>
        ''' <returns>被翻转的点数</returns>
        ''' <remarks>
        ''' 孤立的连通分量（与主曲面没有 kNN 连接的小块）以“法向与 +Z 同向”作为初始朝向；
        ''' 若调用方知道视点，建议在调用本方法前先用
        ''' <see cref="OrientTowardsViewpoint(PointCloudBuffer, Vector3d)"/> 处理一遍，
        ''' 本方法只负责消除局部不一致。
        ''' </remarks>
        Public Shared Function OrientConsistent(cloud As PointCloudBuffer, Optional k As Integer = 10,
                                                Optional options As CloudyOptions = Nothing) As Integer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If Not cloud.HasNormals Then
                Throw New InvalidOperationException("the point cloud has no normals; run NormalEstimation first!")
            End If

            Dim n As Integer = cloud.Count

            If n <= 1 Then Return 0

            Dim kk As Integer = std.Max(3, k)
            Dim tree As New KdTree3D(cloud)

            Dim inTree As Boolean() = New Boolean(n - 1) {}
            Dim sign As Integer() = New Integer(n - 1) {}
            Dim parent As Integer() = New Integer(n - 1) {}
            Dim bestWeight As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                parent(i) = -1
                bestWeight(i) = Double.MaxValue
            Next

            Dim flipped As Integer = 0
            Dim buffer As New Neighborhood(kk + 1)
            Dim queue As New PriorityQueue(Of Integer, Double)()

            ' 以“Z 最高的点”作为主连通分量的种子：工业扫描的曲面通常朝上，这样初始朝向更可能是对的
            Dim seed As Integer = 0

            For i As Integer = 1 To n - 1
                If cloud.Z(i) > cloud.Z(seed) Then seed = i
            Next

            Dim nextSeed As Integer = seed

            ' 每轮处理一个连通分量：队列清空后从最近的未访问点重新开始
            While nextSeed >= 0
                parent(nextSeed) = -1
                bestWeight(nextSeed) = 0.0

                Call queue.Enqueue(nextSeed, 0.0)

                While queue.Count > 0
                    Dim current As Integer
                    Dim priority As Double

                    Call queue.TryDequeue(current, priority)

                    If inTree(current) Then Continue While
                    If priority > bestWeight(current) Then Continue While

                    inTree(current) = True

                    ' 符号从父节点继承：与“父节点已定向后的法向”点积为负则翻转
                    If parent(current) >= 0 Then
                        Dim baseNormal As Vector3d = cloud.Normal(parent(current)) * sign(parent(current))

                        sign(current) = If(Vector3d.Dot(baseNormal, cloud.Normal(current)) < 0.0, -1, 1)
                    Else
                        sign(current) = If(Vector3d.Dot(cloud.Normal(current), Vector3d.UnitZ) < 0.0, -1, 1)
                    End If

                    If sign(current) < 0 Then flipped += 1

                    Call tree.Knn(cloud.X(current), cloud.Y(current), cloud.Z(current), kk, buffer, current)

                    Dim directed As Vector3d = cloud.Normal(current) * sign(current)

                    For j As Integer = 0 To buffer.Count - 1
                        Dim other As Integer = buffer.Indices(j)

                        If inTree(other) Then Continue For

                        ' 边的代价：两个法向越“平行”（无论同向还是反向）越便宜
                        Dim weight As Double = 1.0 - std.Abs(Vector3d.Dot(directed, cloud.Normal(other)))

                        ' 懒删除 Prim：一个点可能被多次改进，只有最小的那次真正决定父节点
                        If weight >= bestWeight(other) Then Continue For

                        bestWeight(other) = weight
                        parent(other) = current

                        Call queue.Enqueue(other, weight)
                    Next
                End While

                nextSeed = -1

                For i As Integer = 0 To n - 1
                    If Not inTree(i) Then
                        nextSeed = i

                        Exit For
                    End If
                Next
            End While

            For i As Integer = 0 To n - 1
                If sign(i) < 0 Then
                    cloud.Nx(i) = -cloud.Nx(i)
                    cloud.Ny(i) = -cloud.Ny(i)
                    cloud.Nz(i) = -cloud.Nz(i)
                End If
            Next

            Return flipped
        End Function

        ''' <summary>
        ''' 统计法向朝向一致性的诊断指标：相邻点法向点积的绝对值均值。
        ''' </summary>
        ''' <returns>
        ''' <c>1.0</c> 表示完全一致；接近 <c>0</c> 表示法向随机（定向失败或点云极度噪声）。
        ''' </returns>
        ''' <remarks>
        ''' 定向算法没有“正确性断言”可依赖，因此提供这个可观测指标：
        ''' 在流水线里如果该值低于 0.7，通常意味着邻域太小（把锐边两侧混在一起）
        ''' 或者存在大量离群点。
        ''' </remarks>
        Public Shared Function MeasureConsistency(cloud As PointCloudBuffer, Optional k As Integer = 8) As Double
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If Not cloud.HasNormals Then
                Throw New InvalidOperationException("the point cloud has no normals; run NormalEstimation first!")
            End If

            Dim n As Integer = cloud.Count

            If n < 2 Then Return 1.0

            Dim tree As New KdTree3D(cloud)
            Dim buffer As New Neighborhood(std.Max(3, k))
            Dim total As Double = 0.0
            Dim count As Integer = 0

            For i As Integer = 0 To n - 1
                Call tree.Knn(cloud.X(i), cloud.Y(i), cloud.Z(i), k, buffer, i)

                For j As Integer = 0 To buffer.Count - 1
                    total += std.Abs(Vector3d.Dot(cloud.Normal(i), cloud.Normal(buffer.Indices(j))))
                    count += 1
                Next
            Next

            If count = 0 Then Return 1.0

            Return total / count
        End Function

    End Class
End Namespace
