Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' ISS(Intrinsic Shape Signatures)关键点检测。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>原理</b>：对每个点求邻域协方差特征值 <c>λ1 ≥ λ2 ≥ λ3</c>，
    ''' 只有当两个比值都足够小的时候才认为是“内在形状显著点”：
    ''' </para>
    ''' <code>
    ''' λ2 / λ1 &lt; γ21   且   λ3 / λ2 &lt; γ32
    ''' </code>
    ''' <para>
    ''' 第一个条件排除“线状/平坦”邻域（在一条线上分布的点 <c>λ2≈λ1</c>）；
    ''' 第二个条件排除“球状/噪声”邻域（各向同性时 <c>λ3≈λ2</c>）。
    ''' 剩下的就是“在一个方向显著更薄、另一个方向更薄”的角点/棱线端点，
    ''' 也就是几何上真正稳定的特征。
    ''' </para>
    ''' <para>
    ''' <b>尺度</b>：<c>salientRadius</c> 决定特征的物理尺度（默认 6 倍平均点距）。
    ''' 太小对噪声敏感，太大会把整个小零件当成一个点。
    ''' </para>
    ''' <para>
    ''' <b>非极大抑制</b>：候选按 <c>λ3</c> 排序，在 <c>nonMaxRadius</c> 内只保留最强的。
    ''' ISS 的原始论文就包含这一步，否则一个角点会输出几十个近邻关键点。
    ''' </para>
    ''' <para>
    ''' <b>复杂度</b>：<c>O(n·k·log n)</c>（半径邻域 + 协方差的闭式特征分解）。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class IssKeypoint

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 检测 ISS 关键点。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="salientRadius">特征尺度半径；<c>≤ 0</c> 表示按平均点距自适应（6 倍）</param>
        ''' <param name="nonMaxRadius">非极大抑制半径；<c>≤ 0</c> 表示自适应（4 倍点距）</param>
        ''' <param name="gamma21">λ2/λ1 的上限（默认 0.975，越小越严格）</param>
        ''' <param name="gamma32">λ3/λ2 的上限（默认 0.975）</param>
        ''' <param name="minNeighbors">邻域点数下限（不足时跳过，避免噪声导致误检）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Detect(cloud As PointCloudBuffer, Optional salientRadius As Double = 0.0,
                                      Optional nonMaxRadius As Double = 0.0,
                                      Optional gamma21 As Double = 0.975,
                                      Optional gamma32 As Double = 0.975,
                                      Optional minNeighbors As Integer = 6,
                                      Optional options As CloudyOptions = Nothing) As KeypointSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If n = 0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim spacing As Double = cloud.EstimateAverageSpacing()

            Dim salient As Double = If(salientRadius > 0.0, salientRadius, std.Max(spacing * 6.0, 1.0E-9))
            Dim nms As Double = If(nonMaxRadius > 0.0, nonMaxRadius, std.Max(spacing * 4.0, 1.0E-9))

            Dim tree As New KdTree3D(cloud)
            Dim weights As Double() = New Double(n - 1) {}
            Dim candidate As Boolean() = New Boolean(n - 1) {}

            Dim scratch As New ThreadLocal(Of Scratch)(Function() New Scratch())
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree, 128))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim local As Scratch = scratch.Value
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()

                        For i As Integer = start To ends - 1
                            Call tree.RadiusSearch(cloud.X(i), cloud.Y(i), cloud.Z(i), salient, buffer, i, 512)

                            Dim count As Integer = buffer.Count

                            If count < minNeighbors Then Continue For

                            Call local.EnsureCapacity(count)
                            Call SimdIndexed.Gather3(cloud.X, cloud.Y, cloud.Z, buffer.Indices, 0, count,
                                                     local.X, local.Y, local.Z)

                            Dim moments As Vector3Moments = SimdVector3.Covariance3(local.X, local.Y, local.Z, 0, count)
                            Dim eigen As Linalg.Eigen3 = Linalg.EigenSymmetric3(
                                moments.Cxx, moments.Cxy, moments.Cxz, moments.Cyy, moments.Cyz, moments.Czz)

                            If eigen.L1 <= 0.0 OrElse eigen.L2 <= 0.0 Then Continue For

                            If eigen.L2 / eigen.L1 < gamma21 AndAlso eigen.L3 / eigen.L2 < gamma32 Then
                                weights(i) = eigen.L3
                                candidate(i) = True
                            End If
                        Next
                    End Sub, 256)
            Finally
                Call scratch.Dispose()
                Call pool.Dispose()
            End Try

            Dim indices As New List(Of Integer)()
            Dim scores As New List(Of Double)()

            For i As Integer = 0 To n - 1
                If candidate(i) Then
                    Call indices.Add(i)
                    Call scores.Add(weights(i))
                End If
            Next

            If indices.Count = 0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            Dim kept As Integer() = KeypointSuppression.Suppress(cloud, indices.ToArray(), scores.ToArray(), nms)
            Dim keptIndices(kept.Length - 1) As Integer
            Dim keptScores(kept.Length - 1) As Double

            For i As Integer = 0 To kept.Length - 1
                keptIndices(i) = indices(kept(i))
                keptScores(i) = scores(kept(i))
            Next

            Dim result As New KeypointSet(keptIndices, keptScores)

            Call result.SortByScore()

            Return result
        End Function

        Private Class Scratch
            Public X As Double() = New Double(127) {}
            Public Y As Double() = New Double(127) {}
            Public Z As Double() = New Double(127) {}

            Public Sub EnsureCapacity(count As Integer)
                If X.Length >= count Then Return

                Dim size As Integer = std.Max(count, X.Length * 2)

                Array.Resize(X, size)
                Array.Resize(Y, size)
                Array.Resize(Z, size)
            End Sub
        End Class

    End Class
End Namespace
