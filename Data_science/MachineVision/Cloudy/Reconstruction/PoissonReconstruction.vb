Imports Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports Microsoft.VisualBasic.MachineVision.Cloudy.Features
Imports std = System.Math

Namespace Reconstruction

    ''' <summary>
    ''' 泊松重建：求解指示函数的泊松方程并提取等值面。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>核心思想</b>：对封闭曲面，存在一个“指示函数” <c>χ</c>（内部 1、外部 0）。
    ''' 它的梯度在曲面处集中（等于法向），在别处为 0。因此
    ''' <c>∇χ = V</c>（<c>V</c> 为由法向构成的向量场）。
    ''' 两边取散度得到泊松方程 <c>Δχ = ∇·V</c>，
    ''' 解出 <c>χ</c> 后取等值面即得水密曲面。
    ''' </para>
    ''' <para>
    ''' <b>为什么它“水密且平滑”</b>：因为求解的是全局偏微分方程，而不是逐点连线。
    ''' 遮挡造成的空洞会被“平滑地补上”（而不是留下边界），噪声也被平均掉。
    ''' 代价是细小特征会被削弱，所以计量级检测通常用 BPA/贪心三角化，
    ''' 而“实体建模/3D 打印/CAD 逆向”用泊松。
    ''' </para>
    ''' <para>
    ''' <b>本实现的简化（必须知悉）</b>：
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>使用<b>均匀网格</b>而不是自适应八叉树：实现简单、可复现，
    ''' 但同样的内存预算下细节分辨率略低（八叉树能在细节处加密）。</description></item>
    ''' <item><description>泊松方程用<b>红黑 Gauss-Seidel 迭代</b>求解，而不是多重网格/直接解。
    ''' 收敛速度是 <c>O(网格尺度²)</c>，因此网格规模建议 ≤ 128³；
    ''' 更大规模请分段重建后合并。</description></item>
    ''' <item><description>法向扩散采用“最近体素累加 + 高斯加权”，没有做八叉树上的
    ''' 法向插值（原始论文的 screened Poisson 还包含数据保真项，这里未实现）。</description></item>
    ''' </list>
    ''' <para>
    ''' <b>前置条件</b>：法向必须<b>一致朝外</b>（用
    ''' <see cref="NormalOrientation.OrientConsistent(PointCloudBuffer, Integer, CloudyOptions)"/>
    ''' 或视点定向处理），否则内外会翻转，重建出的曲面会出现“负体积”。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class PoissonReconstruction

        Private Sub New()
        End Sub

        ''' <summary>重建参数。</summary>
        Public Class PoissonOptions

            ''' <summary>网格分辨率（最长边的体素数，默认 96）。</summary>
            Public Property Resolution As Integer = 96

            ''' <summary>包围盒外扩（相对对角线比例，默认 0.1）。</summary>
            Public Property Padding As Double = 0.1

            ''' <summary>Gauss-Seidel 最大迭代次数（默认 400）。</summary>
            Public Property MaxIterations As Integer = 400

            ''' <summary>收敛阈值（相邻两轮 <c>χ</c> 的最大变化）。</summary>
            Public Property Epsilon As Double = 1.0E-6

            ''' <summary>超松弛因子（1.0 = 纯 Gauss-Seidel，1.9 = 最快但可能震荡）。</summary>
            Public Property Omega As Double = 1.6

            ''' <summary>内存预算（MB）：超过时自动降低分辨率。</summary>
            Public Property MemoryBudgetMB As Integer = 1024
        End Class

        ''' <summary>
        ''' 重建结果。
        ''' </summary>
        Public Class PoissonResult

            ''' <summary>输出的三角网格。</summary>
            Public Property Mesh As MeshData

            ''' <summary>实际使用的分辨率。</summary>
            Public Property Resolution As Integer

            ''' <summary>迭代次数。</summary>
            Public Property Iterations As Integer

            ''' <summary>是否收敛。</summary>
            Public Property Converged As Boolean

            ''' <summary>网格包围盒（用于诊断“是否把点云包住”）。</summary>
            Public Property Bounds As Aabb3

            Public Overrides Function ToString() As String
                Return $"poisson: {Resolution}³ grid, {Iterations} iterations, converged={Converged}, {Mesh?.TriangleCount} triangles"
            End Function
        End Class

        ''' <summary>
        ''' 执行泊松重建。
        ''' </summary>
        ''' <param name="cloud">输入点云（必须有一致朝外的法向）</param>
        ''' <param name="options">参数</param>
        ''' <param name="optionsCloudy">并行选项</param>
        Public Shared Function Reconstruct(cloud As PointCloudBuffer, Optional options As PoissonOptions = Nothing,
                                           Optional optionsCloudy As CloudyOptions = Nothing) As PoissonResult
            If cloud Is Nothing Then Throw New ArgumentNullException(NameOf(cloud))

            Dim opt As PoissonOptions = If(options, New PoissonOptions())
            Dim global_ As CloudyOptions = If(optionsCloudy, CloudyOptions.Default)

            If cloud.Count < 10 Then
                Throw New ArgumentException("at least 10 points are required for Poisson reconstruction!", NameOf(cloud))
            End If

            Dim source As PointCloudBuffer = cloud

            If Not source.HasNormals Then
                source = NormalEstimation.Estimate(cloud, 20, 0.0, global_)
            End If

            ' 分辨率受内存预算约束：三个 Double 体积 = 3·8·n³
            Dim resolution As Integer = std.Max(16, std.Min(256, opt.Resolution))
            Dim budget As Double = std.Max(64, opt.MemoryBudgetMB) * 1024.0 * 1024.0

            While resolution > 16 AndAlso 3.0 * 8.0 * resolution ^ 3 > budget
                resolution \= 2
            End While

            Dim box As Aabb3 = source.Bounds()
            Dim diag As Double = std.Max(box.Diagonal, 1.0E-9)
            Dim padding As Double = diag * std.Max(0.0, opt.Padding)

            box = box.ExpandBy(padding)

            Dim size As Vector3d = box.Size
            Dim cell As Double = std.Max(size.X, std.Max(size.Y, size.Z)) / resolution

            Dim nx As Integer = std.Max(4, CInt(std.Ceiling(size.X / cell)) + 1)
            Dim ny As Integer = std.Max(4, CInt(std.Ceiling(size.Y / cell)) + 1)
            Dim nz As Integer = std.Max(4, CInt(std.Ceiling(size.Z / cell)) + 1)

            ' 1) 法向扩散到网格：V(i,j,k) 累加，W 为权重
            Dim vx As Double()()() = NewVolume(nx, ny, nz)
            Dim vy As Double()()() = NewVolume(nx, ny, nz)
            Dim vz As Double()()() = NewVolume(nx, ny, nz)
            Dim weight As Double()()() = NewVolume(nx, ny, nz)

            Dim splatRadius As Integer = 1

            For p As Integer = 0 To source.Count - 1
                Dim normal As Vector3d = source.Normal(p)

                If normal.IsZero(1.0E-9) Then Continue For

                Dim gi As Integer = CInt(std.Round((source.X(p) - box.Min.X) / cell))
                Dim gj As Integer = CInt(std.Round((source.Y(p) - box.Min.Y) / cell))
                Dim gk As Integer = CInt(std.Round((source.Z(p) - box.Min.Z) / cell))

                For di As Integer = -splatRadius To splatRadius
                    For dj As Integer = -splatRadius To splatRadius
                        For dk As Integer = -splatRadius To splatRadius
                            Dim i As Integer = gi + di
                            Dim j As Integer = gj + dj
                            Dim k As Integer = gk + dk

                            If i < 0 OrElse i >= nx OrElse j < 0 OrElse j >= ny OrElse k < 0 OrElse k >= nz Then Continue For

                            Dim distance As Double = std.Sqrt(di * di + dj * dj + dk * dk)

                            If distance > splatRadius + 0.5 Then Continue For

                            Dim w As Double = std.Exp(-distance * distance)

                            vx(i)(j)(k) += normal.X * w
                            vy(i)(j)(k) += normal.Y * w
                            vz(i)(j)(k) += normal.Z * w
                            weight(i)(j)(k) += w
                        Next
                    Next
                Next
            Next

            ' 归一化向量场（空体素为零，视为“无数据”）
            For i As Integer = 0 To nx - 1
                For j As Integer = 0 To ny - 1
                    For k As Integer = 0 To nz - 1
                        Dim w As Double = weight(i)(j)(k)

                        If w > 1.0E-12 Then
                            vx(i)(j)(k) /= w
                            vy(i)(j)(k) /= w
                            vz(i)(j)(k) /= w
                        End If
                    Next
                Next
            Next

            ' 2) 散度
            Dim divergence As Double()()() = NewVolume(nx, ny, nz)

            For i As Integer = 1 To nx - 2
                For j As Integer = 1 To ny - 2
                    For k As Integer = 1 To nz - 2
                        divergence(i)(j)(k) =
                            (vx(i + 1)(j)(k) - vx(i - 1)(j)(k)) * 0.5 +
                            (vy(i)(j + 1)(k) - vy(i)(j - 1)(k)) * 0.5 +
                            (vz(i)(j)(k + 1) - vz(i)(j)(k - 1)) * 0.5
                    Next
                Next
            Next

            ' 3) 红黑 Gauss-Seidel 求解 Δχ = div
            Dim chi As Double()()() = NewVolume(nx, ny, nz)
            Dim converged As Boolean = False
            Dim iterations As Integer = 0

            For iteration As Integer = 1 To std.Max(1, opt.MaxIterations)
                iterations = iteration

                Dim delta As Double = 0.0

                For color As Integer = 0 To 1
                    For i As Integer = 1 To nx - 2
                        For j As Integer = 1 To ny - 2
                            For k As Integer = 1 To nz - 2
                                If ((i + j + k) And 1) <> color Then Continue For

                                Dim neighbors As Double =
                                    chi(i - 1)(j)(k) + chi(i + 1)(j)(k) +
                                    chi(i)(j - 1)(k) + chi(i)(j + 1)(k) +
                                    chi(i)(j)(k - 1) + chi(i)(j)(k + 1)

                                Dim target As Double = (neighbors - divergence(i)(j)(k) * cell * cell) / 6.0
                                Dim updated As Double = chi(i)(j)(k) + opt.Omega * (target - chi(i)(j)(k))

                                delta = std.Max(delta, std.Abs(updated - chi(i)(j)(k)))
                                chi(i)(j)(k) = updated
                            Next
                        Next
                    Next
                Next

                If delta < opt.Epsilon Then
                    converged = True

                    Exit For
                End If
            Next

            ' 4) 等值面：取“表面附近的平均值”作为阈值
            Dim sum As Double = 0.0
            Dim count As Integer = 0

            For p As Integer = 0 To source.Count - 1
                Dim i As Integer = CInt(std.Round((source.X(p) - box.Min.X) / cell))
                Dim j As Integer = CInt(std.Round((source.Y(p) - box.Min.Y) / cell))
                Dim k As Integer = CInt(std.Round((source.Z(p) - box.Min.Z) / cell))

                If i < 0 OrElse i >= nx OrElse j < 0 OrElse j >= ny OrElse k < 0 OrElse k >= nz Then Continue For

                sum += chi(i)(j)(k)
                count += 1
            Next

            Dim isovalue As Double = If(count > 0, sum / count, 0.0)
            Dim mesh As MeshData = MarchingCubes.Extract(chi, box.Min, New Vector3d(cell, cell, cell), isovalue)

            Return New PoissonResult With {
                .Mesh = mesh,
                .Resolution = resolution,
                .Iterations = iterations,
                .Converged = converged,
                .Bounds = box
            }
        End Function

        Private Shared Function NewVolume(nx As Integer, ny As Integer, nz As Integer) As Double()()()
            Dim volume As Double()()() = New Double(nx - 1)()() {}

            For i As Integer = 0 To nx - 1
                volume(i) = New Double(ny - 1)() {}

                For j As Integer = 0 To ny - 1
                    volume(i)(j) = New Double(nz - 1) {}
                Next
            Next

            Return volume
        End Function

    End Class
End Namespace
