Imports System.Threading
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports std = System.Math

Namespace Features

    ''' <summary>
    ''' Harris-3D 关键点检测：以法向场的一阶结构张量作为响应函数。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>原理</b>：Harris 检测器在二维图像上寻找“灰度在两个方向上都剧烈变化”的角点。
    ''' 三维的对应形式是把“灰度梯度”换成“法向变化”：
    ''' 邻域内归一化法向 <c>n_i</c> 相对均值 <c>n̄</c> 的协方差
    ''' <c>C = (1/n)·Σ(n_i − n̄)(n_i − n̄)ᵀ</c> 描述了这个局部面片“往哪些方向弯”。
    ''' 角点（球角、顶点）在两个方向上都弯 → <c>C</c> 的各向同性大；
    ''' 平面 → <c>C ≈ 0</c>；柱面 → 只有一个方向大。
    ''' </para>
    ''' <para>
    ''' 响应函数沿用 Harris 形式：<c>R = det(C) − k·tr(C)²</c>（<c>k</c> 默认 0.04）。
    ''' <c>det(C)</c> 只在“两个方向都有明显变化”时非零，而 <c>tr(C)²</c> 会被“只有一个方向变化”
    ''' 的柱面抬高，因此相减能压制边缘响应、突出角点。
    ''' </para>
    ''' <para>
    ''' <b>与 ISS 的差别</b>：ISS 直接对邻域<b>坐标</b>协方差做特征值比值筛选，
    ''' 结果是“几何上各向异性最明显”的点；Harris-3D 对<b>法向场</b>做响应，
    ''' 更偏向“曲率变化剧烈”的点（尖角、小凸起）。
    ''' 工业识别里两者可以互补使用。
    ''' </para>
    ''' <para>
    ''' <b>前置条件</b>：需要法向。若点云没有法向，会自动先做一次法向估计。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class Harris3DKeypoint

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 检测 Harris-3D 关键点。
        ''' </summary>
        ''' <param name="cloud">输入点云</param>
        ''' <param name="radius">邻域半径；<c>≤ 0</c> 表示按平均点距自适应（4 倍）</param>
        ''' <param name="k">Harris 常数（默认 0.04，典型范围 0.04~0.06）</param>
        ''' <param name="nonMaxRadius">非极大抑制半径；<c>≤ 0</c> 表示自适应（3 倍点距）</param>
        ''' <param name="thresholdRatio">响应阈值相对于最大响应的比例（默认 0.01）</param>
        ''' <param name="options">并行选项</param>
        Public Shared Function Detect(cloud As PointCloudBuffer, Optional radius As Double = 0.0,
                                      Optional k As Double = 0.04,
                                      Optional nonMaxRadius As Double = 0.0,
                                      Optional thresholdRatio As Double = 0.01,
                                      Optional options As CloudyOptions = Nothing) As KeypointSet
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim n As Integer = cloud.Count

            If n = 0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            Dim opt As CloudyOptions = If(options, CloudyOptions.Default)
            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                ' 用与检测半径相匹配的邻域估计法向，避免“法向尺度过大抹平角点”
                source = NormalEstimation.Estimate(cloud, 20, radius, opt)
            End If

            Dim spacing As Double = source.EstimateAverageSpacing()
            Dim r As Double = If(radius > 0.0, radius, std.Max(spacing * 4.0, 1.0E-9))
            Dim nms As Double = If(nonMaxRadius > 0.0, nonMaxRadius, std.Max(spacing * 3.0, 1.0E-9))

            Dim tree As New KdTree3D(source)
            Dim responses As Double() = New Double(n - 1) {}
            Dim pool As New ThreadLocal(Of NeighborhoodQuery)(Function() New NeighborhoodQuery(tree, 256))
            Dim scratch As New ThreadLocal(Of List(Of Integer))(Function() New List(Of Integer)(256))

            Try
                Call opt.ForRange(n,
                    Sub(start, ends)
                        Dim query As NeighborhoodQuery = pool.Value
                        Dim buffer As Neighborhood = query.Rent()
                        Dim neighbors As List(Of Integer) = scratch.Value

                        For i As Integer = start To ends - 1
                            Call tree.RadiusSearch(source.X(i), source.Y(i), source.Z(i), r, buffer, i, 1024)

                            Dim count As Integer = buffer.Count

                            If count < 5 Then Continue For

                            ' 法向均值
                            Dim mx As Double = 0, my As Double = 0, mz As Double = 0

                            For j As Integer = 0 To count - 1
                                Dim q As Integer = buffer.Indices(j)

                                mx += source.Nx(q)
                                my += source.Ny(q)
                                mz += source.Nz(q)
                            Next

                            Dim inv As Double = 1.0 / count

                            mx *= inv
                            my *= inv
                            mz *= inv

                            ' 法向协方差
                            Dim cxx As Double = 0, cxy As Double = 0, cxz As Double = 0
                            Dim cyy As Double = 0, cyz As Double = 0, czz As Double = 0

                            For j As Integer = 0 To count - 1
                                Dim q As Integer = buffer.Indices(j)
                                Dim dx As Double = source.Nx(q) - mx
                                Dim dy As Double = source.Ny(q) - my
                                Dim dz As Double = source.Nz(q) - mz

                                cxx += dx * dx
                                cxy += dx * dy
                                cxz += dx * dz
                                cyy += dy * dy
                                cyz += dy * dz
                                czz += dz * dz
                            Next

                            cxx *= inv
                            cxy *= inv
                            cxz *= inv
                            cyy *= inv
                            cyz *= inv
                            czz *= inv

                            Dim det As Double = cxx * (cyy * czz - cyz * cyz) -
                                cxy * (cxy * czz - cyz * cxz) +
                                cxz * (cxy * cyz - cyy * cxz)
                            Dim trace As Double = cxx + cyy + czz

                            responses(i) = det - k * trace * trace
                        Next
                    End Sub, 256)
            Finally
                Call pool.Dispose()
                Call scratch.Dispose()
            End Try

            Dim maxResponse As Double = 0.0

            For i As Integer = 0 To n - 1
                If responses(i) > maxResponse Then maxResponse = responses(i)
            Next

            If maxResponse <= 0.0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            Dim threshold As Double = maxResponse * std.Max(0.0, thresholdRatio)
            Dim indices As New List(Of Integer)()
            Dim scores As New List(Of Double)()

            For i As Integer = 0 To n - 1
                If responses(i) > threshold Then
                    Call indices.Add(i)
                    Call scores.Add(responses(i))
                End If
            Next

            If indices.Count = 0 Then
                Return New KeypointSet(Array.Empty(Of Integer)(), Array.Empty(Of Double)())
            End If

            Dim kept As Integer() = KeypointSuppression.Suppress(source, indices.ToArray(), scores.ToArray(), nms)
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

    End Class
End Namespace
