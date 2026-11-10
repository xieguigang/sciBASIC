Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Preprocessing

    ''' <summary>
    ''' 降采样策略集合：随机、均匀抽稀、曲率自适应与最远点采样。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 体素滤波（<see cref="VoxelGridFilter"/>）是“空间均匀”的降采样，
    ''' 但在两类场景下并不够用：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>
    ''' <b>特征区域需要更高密度</b>：薄壁、倒角、焊缝处曲率大，
    ''' 体素滤波会把它们抹平。<see cref="CurvatureAdaptive(Double(), PointCloudBuffer, Double(), Integer)"/>
    ''' 按曲率分配采样配额，在保持总点数不变的前提下把点数集中到高曲率区。
    ''' </description></item>
    ''' <item><description>
    ''' <b>需要空间覆盖最优的子集</b>：关键点验证、采样一致性算法、可视化抽样
    ''' 都希望“用最少的点覆盖尽可能大的空间”，这正是
    ''' <see cref="FarthestPoint(Integer, PointCloudBuffer, Integer)"/> 的用途。
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' <b>确定性</b>：除 <see cref="RandomCount"/> 之外，所有函数都是确定性的
    ''' （相同输入必然得到相同子集），这是工业检测流程可复现的前提。
    ''' 随机版本也接受显式种子。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class Sampling

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 随机抽取给定比例的点。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="ratio">保留比例 <c>(0, 1]</c></param>
        ''' <param name="seed">随机种子（<c>0</c> 表示使用 <see cref="CloudyOptions.Default"/> 的种子）</param>
        Public Shared Function Random(cloud As PointCloudBuffer, ratio As Double, Optional seed As Integer = 0) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If ratio <= 0.0 OrElse ratio > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(ratio), "the ratio should be in (0, 1]!")
            End If

            Return RandomCount(cloud, CInt(std.Floor(cloud.Count * ratio)), seed)
        End Function

        ''' <summary>
        ''' 随机抽取指定数量的点（无放回）。
        ''' </summary>
        Public Shared Function RandomCount(cloud As PointCloudBuffer, count As Integer, Optional seed As Integer = 0) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If count >= n Then Return cloud.Clone()
            If count <= 0 Then Return PointCloudBuffer.NewCloud(0)

            Dim rnd As Random = If(seed = 0, CloudyOptions.Default.CreateRandom(), New Random(seed))
            Dim picked As New HashSet(Of Integer)(count)

            While picked.Count < count
                Call picked.Add(rnd.Next(n))
            End While

            Dim indices As Integer() = picked.ToArray()

            ' HashSet 的枚举顺序不保证稳定，排序后输出才是可复现的
            Array.Sort(indices)

            Return cloud.Subset(indices)
        End Function

        ''' <summary>
        ''' 等步长抽稀（确定性）。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="ratio">保留比例 <c>(0, 1]</c></param>
        ''' <remarks>
        ''' 直接按输入顺序每隔 <c>1/ratio</c> 个点取一个。
        ''' 优点是零开销、完全确定；缺点是如果输入顺序本身带空间规律
        ''' （例如按扫描线排列），会在点云上留下条纹状空洞。
        ''' 扫描线数据请优先使用体素滤波。
        ''' </remarks>
        Public Shared Function Uniform(cloud As PointCloudBuffer, ratio As Double) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If ratio <= 0.0 OrElse ratio > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(ratio), "the ratio should be in (0, 1]!")
            End If

            Return UniformCount(cloud, CInt(std.Floor(cloud.Count * ratio)))
        End Function

        ''' <summary>
        ''' 等步长抽取指定数量的点。
        ''' </summary>
        Public Shared Function UniformCount(cloud As PointCloudBuffer, count As Integer) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If count >= n Then Return cloud.Clone()
            If count <= 0 Then Return PointCloudBuffer.NewCloud(0)

            Dim stride As Double = CDbl(n) / count
            Dim indices(count - 1) As Integer

            For i As Integer = 0 To count - 1
                Dim index As Integer = CInt(std.Floor(i * stride))

                If index >= n Then index = n - 1

                indices(i) = index
            Next

            Return cloud.Subset(indices)
        End Function

        ''' <summary>
        ''' 曲率自适应降采样：在保持总点数的前提下，把点数向高曲率区域倾斜。
        ''' </summary>
        ''' <param name="curvature">逐点曲率/表面变化度（可由法向估计得到）</param>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="targetCount">目标点数</param>
        ''' <param name="gamma">倾斜强度（1 = 与曲率线性相关，2~4 = 更激进地保护特征区）</param>
        ''' <remarks>
        ''' <para>
        ''' <b>做法</b>：把曲率归一化后作为采样权重 <c>w(i) = (c(i)+ε)^γ</c>，
        ''' 按权重做<b>系统抽样</b>（沿累积权重等间隔取点）而不是独立随机抽样——
        ''' 系统抽样的方差更小、结果确定，而且不会出现“某个高曲率小块被完全跳过”的情况。
        ''' </para>
        ''' <para>
        ''' <b>ε 的作用</b>：平坦区域的曲率接近 0，若不设下限则权重为 0，
        ''' 平面会被完全丢弃；加一个与平均曲率同量级的 <c>ε</c> 保证平坦区仍有基本采样率。
        ''' </para>
        ''' <para>
        ''' 复杂度 <c>O(n·log n)</c>（一次排序）。
        ''' </para>
        ''' </remarks>
        Public Shared Function CurvatureAdaptive(curvature As Double(), cloud As PointCloudBuffer,
                                                 targetCount As Integer, Optional gamma As Double = 2.0) As PointCloudBuffer
            If curvature Is Nothing Then Throw New ArgumentNullException(NameOf(curvature))
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))
            If curvature.Length <> cloud.Count Then
                Throw New ArgumentException($"the curvature array length ({curvature.Length}) should equal the point count ({cloud.Count})!", NameOf(curvature))
            End If

            Dim n As Integer = cloud.Count

            If targetCount >= n Then Return cloud.Clone()
            If targetCount <= 0 Then Return PointCloudBuffer.NewCloud(0)

            Dim meanCurvature As Double = 0.0

            For i As Integer = 0 To n - 1
                meanCurvature += curvature(i)
            Next

            meanCurvature /= n

            Dim eps As Double = std.Max(meanCurvature, 1.0E-9)
            Dim power As Double = If(gamma <= 0.0, 1.0, gamma)
            Dim weights As Double() = New Double(n - 1) {}
            Dim total As Double = 0.0

            For i As Integer = 0 To n - 1
                weights(i) = std.Pow(curvature(i) + eps, power)
                total += weights(i)
            Next

            If total <= 0.0 Then Return UniformCount(cloud, targetCount)

            ' 系统抽样：沿累积权重等间隔取点
            Dim stride As Double = total / targetCount
            Dim indices As New List(Of Integer)(targetCount)
            Dim cursor As Double = 0.0
            Dim nextTarget As Double = stride * 0.5

            For i As Integer = 0 To n - 1
                cursor += weights(i)

                Do While cursor >= nextTarget AndAlso indices.Count < targetCount
                    Call indices.Add(i)
                    nextTarget += stride
                Loop
            Next

            ' 浮点误差可能少收一个点，用最后一个点补齐
            While indices.Count < targetCount
                Call indices.Add(std.Min(n - 1, indices(indices.Count - 1) + 1))
            End While

            Return cloud.Subset(indices.ToArray())
        End Function

        ''' <summary>
        ''' 最远点采样(FPS)：贪心地选择“距离已选集合最远”的点，使子集的空间覆盖最优。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="count">目标点数</param>
        ''' <param name="seedIndex">第一个点（默认 0；也可用来强制包含某个关键点）</param>
        ''' <param name="options">并行选项</param>
        ''' <remarks>
        ''' <para>
        ''' <b>复杂度</b>：<c>O(n·m)</c>（<c>m</c> 为输出点数）。
        ''' 每轮只更新“每个点到已选集合的最小距离”，是纯数组扫描、无近邻查询；
        ''' 对 1e5 点选 1e3 点约为 1e8 次简单算术，运行时只做一次。
        ''' 点数更大时请改用体素滤波。
        ''' </para>
        ''' <para>
        ''' <b>用途</b>：关键点检测后的均匀化、训练样本挑选、快速可视化。
        ''' 与曲率自适应相比，FPS 完全忽略几何特征，只保证空间均匀。
        ''' </para>
        ''' </remarks>
        Public Shared Function FarthestPoint(cloud As PointCloudBuffer, count As Integer,
                                             Optional seedIndex As Integer = 0,
                                             Optional options As CloudyOptions = Nothing) As PointCloudBuffer
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If count >= n Then Return cloud.Clone()
            If count <= 0 Then Return PointCloudBuffer.NewCloud(0)

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim selected(count - 1) As Integer
            Dim minDist As Double() = New Double(n - 1) {}
            Dim first As Integer = std.Max(0, std.Min(n - 1, seedIndex))

            selected(0) = first

            For i As Integer = 0 To n - 1
                minDist(i) = SquaredDistance(cloud, i, first)
            Next

            For k As Integer = 1 To count - 1
                Dim best As Integer = 0
                Dim bestDist As Double = -1.0

                For i As Integer = 0 To n - 1
                    If minDist(i) > bestDist Then
                        bestDist = minDist(i)
                        best = i
                    End If
                Next

                selected(k) = best

                ' 更新所有点到“已选集合”的最近距离
                Call opt.ForRange(n,
                    Sub(start, ends)
                        For i As Integer = start To ends - 1
                            Dim d As Double = SquaredDistance(cloud, i, best)

                            If d < minDist(i) Then minDist(i) = d
                        Next
                    End Sub, 8192)
            Next

            Return cloud.Subset(selected)
        End Function

        Private Shared Function SquaredDistance(cloud As PointCloudBuffer, a As Integer, b As Integer) As Double
            Dim dx As Double = cloud.X(a) - cloud.X(b)
            Dim dy As Double = cloud.Y(a) - cloud.Y(b)
            Dim dz As Double = cloud.Z(a) - cloud.Z(b)

            Return dx * dx + dy * dy + dz * dz
        End Function

    End Class
End Namespace
