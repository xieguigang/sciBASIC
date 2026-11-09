Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Registration

    ''' <summary>
    ''' 对应点集的刚体变换估计（Kabsch/Horn）与其鲁棒变体。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 这是所有配准算法的“最后一次求解”：RANSAC 选出内点、ICP 建立对应、
    ''' 4PCS 得到四点对应之后，都要用同一套方法求最优 <c>(R, t)</c>。
    ''' 底层实现见 <see cref="Linalg.Kabsch(Vector3d(), Vector3d(), Double())"/>
    ''' （4x4 四元数矩阵的最大特征向量，闭式、无 SVD、天然得到真旋转）。
    ''' </para>
    ''' <para>
    ''' <b>鲁棒版本</b>：普通最小二乘会被少数错误对应拉偏。
    ''' <see cref="EstimateRobust(Vector3d(), Vector3d(), Integer, Double)"/> 采用
    ''' “迭代重加权 + 截尾”的组合：
    ''' 先按当前变换算出每个对应的残差，只保留残差最小的
    ''' <c>trimRatio</c> 比例（截尾），再对保留部分做加权最小二乘
    ''' （权重 <c>1/(1+ (r/r0)²)</c>）。迭代几轮即可逼近截尾最小二乘的解，
    ''' 效果接近 RANSAC 的“先内点后求解”，但代价低一到两个数量级。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class TransformEstimation

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 最小二乘刚体变换估计（等权）。
        ''' </summary>
        ''' <param name="source">源点集</param>
        ''' <param name="target">目标点集（一一对应）</param>
        Public Shared Function Estimate(source As Vector3d(), target As Vector3d()) As RigidTransform
            Return Linalg.Kabsch(source, target)
        End Function

        ''' <summary>
        ''' 加权最小二乘刚体变换估计。
        ''' </summary>
        Public Shared Function Estimate(source As Vector3d(), target As Vector3d(), weights As Double()) As RigidTransform
            Return Linalg.Kabsch(source, target, weights)
        End Function

        ''' <summary>
        ''' 鲁棒（截尾 + 重加权）刚体变换估计。
        ''' </summary>
        ''' <param name="source">源点集</param>
        ''' <param name="target">目标点集</param>
        ''' <param name="iterations">重加权轮数（默认 3，通常已经足够收敛）</param>
        ''' <param name="trimRatio">保留残差最小的一部分对应的比例（默认 0.8）</param>
        ''' <param name="scale">重加权的尺度 <c>r0</c>；<c>≤ 0</c> 表示用残差中位数自动估计</param>
        ''' <returns>变换与（最后一次迭代的）内点统计</returns>
        Public Shared Function EstimateRobust(source As Vector3d(), target As Vector3d(),
                                              Optional iterations As Integer = 3,
                                              Optional trimRatio As Double = 0.8,
                                              Optional scale As Double = 0.0) As RegistrationResult
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            If source.Length <> target.Length Then
                Throw New ArgumentException($"the size of the two point sets not agree: {source.Length} vs {target.Length}!", NameOf(target))
            End If
            If source.Length < 3 Then
                Throw new ArgumentException("at least 3 point pairs are required!", NameOf(source))
            End If

            Dim n As Integer = source.Length
            Dim keep As Double = std.Max(0.1, std.Min(1.0, trimRatio))
            Dim keepCount As Integer = std.Max(3, CInt(std.Floor(n * keep)))

            Dim current As RigidTransform = Linalg.Kabsch(source, target)
            Dim residuals As Double() = New Double(n - 1) {}
            Dim order As Integer() = New Integer(n - 1) {}

            For i As Integer = 0 To n - 1
                order(i) = i
            Next

            Dim inlierCount As Integer = n
            Dim rmse As Double = 0.0

            For iteration As Integer = 1 To std.Max(1, iterations)
                For i As Integer = 0 To n - 1
                    residuals(i) = (current.TransformPoint(source(i)) - target(i)).Length
                Next

                Array.Sort(DirectCast(residuals.Clone(), Double()), order)

                ' 用保留部分的中位数估计尺度（对残差分布的形状不敏感）
                Dim median As Double = residuals(order(std.Min(n - 1, keepCount \ 2)))
                Dim r0 As Double = If(scale > 0.0, scale, std.Max(median, 1.0E-9))

                Dim weights As Double() = New Double(keepCount - 1) {}
                Dim src(keepCount - 1) As Vector3d
                Dim dst(keepCount - 1) As Vector3d

                For k As Integer = 0 To keepCount - 1
                    Dim index As Integer = order(k)
                    Dim ratio As Double = residuals(index) / r0

                    src(k) = source(index)
                    dst(k) = target(index)
                    weights(k) = 1.0 / (1.0 + ratio * ratio)
                Next

                current = Linalg.Kabsch(src, dst, weights)

                inlierCount = keepCount
                rmse = 0.0

                For k As Integer = 0 To keepCount - 1
                    Dim d As Double = (current.TransformPoint(src(k)) - dst(k)).Length

                    rmse += d * d
                Next

                rmse = std.Sqrt(rmse / keepCount)
            Next

            Return New RegistrationResult(current, CDbl(inlierCount) / n, rmse,
                                          std.Max(1, iterations), True, "robust-kabsch")
        End Function

        ''' <summary>
        ''' 用两组对应下标估计点云之间的变换。
        ''' </summary>
        ''' <param name="source">源点云</param>
        ''' <param name="target">目标点云</param>
        ''' <param name="sourceIndices">源点下标</param>
        ''' <param name="targetIndices">目标点下标（与源一一对应）</param>
        Public Shared Function FromCorrespondences(source As PointCloudBuffer, target As PointCloudBuffer,
                                                   sourceIndices As Integer(), targetIndices As Integer()) As RigidTransform
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
            If sourceIndices Is Nothing Then Throw New ArgumentNullException(NameOf(sourceIndices))
            If targetIndices Is Nothing Then Throw New ArgumentNullException(NameOf(targetIndices))
            If sourceIndices.Length <> targetIndices.Length Then
                Throw New ArgumentException("the two correspondence lists should have the same length!", NameOf(targetIndices))
            End If

            Dim src(sourceIndices.Length - 1) As Vector3d
            Dim dst(targetIndices.Length - 1) As Vector3d

            For i As Integer = 0 To sourceIndices.Length - 1
                src(i) = source.Item(sourceIndices(i))
                dst(i) = target.Item(targetIndices(i))
            Next

            Return Linalg.Kabsch(src, dst)
        End Function

        ''' <summary>
        ''' 按点到点距离统计给定变换的残差（不建立对应关系，只统计“最近邻距离”）。
        ''' </summary>
        ''' <param name="source">源点云</param>
        ''' <param name="target">目标点云</param>
        ''' <param name="transform">源 → 目标的变换</param>
        ''' <param name="maxDistance">统计半径（超过视为外点，不计入 rmse）</param>
        ''' <param name="fitness">内点率</param>
        ''' <param name="rmse">内点均方根残差</param>
        Public Shared Sub Evaluate(source As PointCloudBuffer, target As PointCloudBuffer, transform As RigidTransform,
                                   maxDistance As Double, ByRef fitness As Double, ByRef rmse As Double)
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            If target Is Nothing Then Throw new ArgumentNullException(NameOf(target))
            If transform Is Nothing Then Throw new ArgumentNullException(NameOf(transform))

            Dim tree As New KdTree3D(target)
            Dim n As Integer = source.Count
            Dim inliers As Integer = 0
            Dim sumSquared As Double = 0.0
            Dim buffer As New Neighborhood(1)

            For i As Integer = 0 To n - 1
                Dim p As Vector3d = transform.TransformPoint(source.Item(i))
                Dim nearest As (index As Integer, distanceSquared As Double) =
                    tree.Nearest(p.X, p.Y, p.Z)

                If nearest.index >= 0 AndAlso nearest.distanceSquared <= maxDistance * maxDistance Then
                    inliers += 1
                    sumSquared += nearest.distanceSquared
                End If
            Next

            fitness = If(n = 0, 0.0, CDbl(inliers) / n)
            rmse = If(inliers = 0, Double.MaxValue, std.Sqrt(sumSquared / inliers))
        End Sub

    End Class
End Namespace
