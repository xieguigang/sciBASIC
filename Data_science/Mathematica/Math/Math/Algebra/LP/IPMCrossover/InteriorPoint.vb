' ============================================================================
' InteriorPoint.vb — 有界变量原始-对偶内点法（Mehrotra 预测-校正）[readme §二/§三]
' ----------------------------------------------------------------------------
' 问题形式：min cᵀx, Ax = b, **0 ≤ x ≤ u**（u 可为 +∞；有限下界已由标准形平移消掉）
'
' KKT：  Ax = b；  Aᵀy + s − z = c；  x∘s = μ；  w∘z = μ（w = u − x）；  s,z ≥ 0
'
' 牛顿系统：
'   A·Δx = −r_p                                   (1)
'   Aᵀ·Δy + Δs − Δz = −r_d                        (2)
'   S·Δx + X·Δs = γ_L = −XSe + σμe + corr_L       (3)
'   W·Δz − Z·Δx = γ_U = −WZe + σμe + corr_U       (4)   （Δw = −Δx）
'
' (2)→ Δz = Δs + r_d + AᵀΔy；与 (3)(4) 联立消元：
'   Δx = Θ·(AᵀΔy + r_d + X⁻¹γ_L − W⁻¹γ_U)，  Θ = (S/X + Z/W)⁻¹
'   Δs = X⁻¹γ_L − (s/x)·Δx
'   Δz = W⁻¹γ_U + (z/w)·Δx
'   正规方程：A·Θ·Aᵀ·Δy = −r_p − A·Θ·(r_d + X⁻¹γ_L − W⁻¹γ_U)   ★
'
' u = +∞ 时 z = 0、Z/W = 0、W⁻¹γ_U = 0，Θ 退化为 X/S，
' ★ 退化为 readme 方框公式 b − σμ·A·S⁻¹e，与原无上界实现**完全等价**（T1-T9 不变）。
'
' 步长：双侧 fraction-to-boundary（x 同时看 x→0 与 x→u），原始/对偶分离取步。
' 数值：静态正则化阶梯 + 一次迭代精化；步长停滞 → 提升正则化档位重试 ≤4 档。
' 矩阵：全部经 ILpMatrix 访问（稠密或稀疏），本文件不得出现 A(i,j) 直接下标。
' ============================================================================

Imports std = System.Math
Imports System.Collections.Generic
Imports System.Linq

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Class IpResult

        Public Status As String          ' optimal/primal_infeasible/dual_infeasible/stalled/numeric_fail/diverged/max_iter
        Public X As Double()
        Public Y As Double()
        Public S As Double()
        ''' <summary>上界对偶变量（无上界变量恒为 0）</summary>
        Public Z As Double()
        Public Iters As Int32

    End Class

    Public Class InteriorPointSolver

        Private ReadOnly Mat As ILpMatrix
        Private ReadOnly b As Double()
        Private ReadOnly c As Double()
        ''' <summary>上界（+∞ = 无上界）</summary>
        Private ReadOnly u As Double()
        Private ReadOnly ubFin As Boolean()
        Private ReadOnly m As Int32
        Private ReadOnly n As Int32
        Private ReadOnly tol As Double
        Private ReadOnly maxIter As Int32
        Private ReadOnly log As List(Of String)
        Private ReadOnly anyBound As Boolean
        Private ReadOnly muDen As Double

        ' 自适应正则化阶梯 [readme §2.2 的工程扩展]
        Private Shared ReadOnly RegLadder() As Double = {0.00000000001, 0.00000001, 0.00001, 0.01}

        ''' <summary>向后兼容构造：稠密矩阵 + 无上界（x ≥ 0）</summary>
        Public Sub New(A As Double(,), b As Double(), c As Double(),
                       Optional tol As Double = 0.00000001, Optional maxIter As Int32 = 200,
                       Optional log As List(Of String) = Nothing)
            Me.New(New DenseLpMatrix(A), b, c, Nothing, tol, maxIter, log)
        End Sub

        ''' <summary>
        ''' 通用构造：矩阵抽象 + 可选上界
        ''' </summary>
        ''' <param name="u">上界数组（长度 = 列数）；Nothing 或元素为 +∞ 表示该变量无上界</param>
        Public Sub New(mat As ILpMatrix, b As Double(), c As Double(), u As Double(),
                       Optional tol As Double = 0.00000001, Optional maxIter As Int32 = 200,
                       Optional log As List(Of String) = Nothing)
            Me.Mat = mat
            Me.b = b
            Me.c = c
            Me.m = mat.Rows
            Me.n = mat.Columns
            Me.tol = tol
            Me.maxIter = maxIter
            Me.log = log

            Me.u = New Double(Me.n - 1) {}
            Me.ubFin = New Boolean(Me.n - 1) {}

            Dim nFin As Int32 = 0

            For j As Int32 = 0 To Me.n - 1
                Dim uj As Double = If(u Is Nothing OrElse j >= u.Length, Double.PositiveInfinity, u(j))

                If Double.IsNaN(uj) Then uj = Double.PositiveInfinity

                Me.u(j) = uj

                If Double.IsInfinity(uj) Then
                    Me.ubFin(j) = False
                Else
                    Me.ubFin(j) = True
                    nFin += 1
                End If
            Next

            Me.anyBound = (nFin > 0)
            Me.muDen = Me.n + nFin
        End Sub

        ''' <summary>w = u − x（带数值下限，避免除零）</summary>
        Private Sub UpdateUpperSlack(x As Double(), w As Double())
            If Not anyBound Then Return

            For j As Int32 = 0 To n - 1
                If ubFin(j) Then
                    w(j) = std.Max(u(j) - x(j), 0.0000000001 * std.Max(1.0, u(j)))
                End If
            Next
        End Sub

        ''' <summary>平均互补间隙 μ = (xᵀs + wᵀz) / (n + n_finite)</summary>
        Private Function Complementarity(x As Double(), s As Double(), w As Double(), z As Double()) As Double
            Dim acc As Double = LinAlg.Dot(x, s)

            If anyBound Then
                For j As Int32 = 0 To n - 1
                    If ubFin(j) Then acc += w(j) * z(j)
                Next
            End If

            Return acc / muDen
        End Function

        ''' <summary>沿方向走 α 之后的平均互补间隙（Mehrotra σ 用）</summary>
        Private Function ComplementarityAt(x As Double(), dx As Double(),
                                           s As Double(), ds As Double(),
                                           w As Double(), z As Double(), dz As Double(),
                                           alpha As Double) As Double
            Dim acc As Double = 0.0

            For j As Int32 = 0 To n - 1
                acc += (x(j) + alpha * dx(j)) * (s(j) + alpha * ds(j))

                If ubFin(j) Then
                    acc += (w(j) - alpha * dx(j)) * (z(j) + alpha * dz(j))
                End If
            Next

            Return acc / muDen
        End Function

        ''' <summary>原始步长上界：0 ≤ x + αΔx ≤ u（双侧）</summary>
        Private Function MaxStepX(x As Double(), dx As Double()) As Double
            Dim alpha As Double = 1.0

            For j As Int32 = 0 To n - 1
                If dx(j) < -0.00000000000001 Then
                    alpha = std.Min(alpha, -x(j) / dx(j))
                ElseIf dx(j) > 0.00000000000001 AndAlso ubFin(j) Then
                    Dim room As Double = u(j) - x(j)

                    If room <= 0.0 Then
                        Return 0.0
                    End If

                    alpha = std.Min(alpha, room / dx(j))
                End If
            Next

            Return alpha
        End Function

        ''' <summary>对偶步长上界：s + αΔs ≥ 0，z + αΔz ≥ 0</summary>
        Private Function MaxStepDual(s As Double(), ds As Double(), z As Double(), dz As Double()) As Double
            Dim alpha As Double = 1.0

            For j As Int32 = 0 To n - 1
                If ds(j) < -0.00000000000001 Then
                    alpha = std.Min(alpha, -s(j) / ds(j))
                End If

                If ubFin(j) AndAlso dz(j) < -0.00000000000001 Then
                    alpha = std.Min(alpha, -z(j) / dz(j))
                End If
            Next

            Return alpha
        End Function

        ''' <summary>Mehrotra 起始点：最小范数 x̂/ŝ + 偏移平衡（并夹到上界内部）</summary>
        Private Sub MehrotraStart(ByRef x As Double(), ByRef y As Double(),
                                  ByRef s As Double(), ByRef z As Double())
            x = New Double(n - 1) {}
            s = New Double(n - 1) {}
            z = New Double(n - 1) {}

            Dim xhat(n - 1) As Double
            Dim dhat(n - 1) As Double
            Dim y0(m - 1) As Double

            For j As Int32 = 0 To n - 1
                xhat(j) = 1.0
                dhat(j) = c(j)
            Next

            Dim ones(n - 1) As Double

            For j As Int32 = 0 To n - 1
                ones(j) = 1.0
            Next

            Dim fac As INormalFactor = Mat.FactorNormal(ones, 0.000000000001)

            If fac IsNot Nothing Then
                Dim y1 As Double() = fac.Solve(b)

                If y1 IsNot Nothing Then
                    xhat = Mat.Mtv(y1)
                End If

                Dim Ac As Double() = Mat.Mv(c)
                Dim y2 As Double() = fac.Solve(Ac)

                If y2 IsNot Nothing Then
                    Dim aty As Double() = Mat.Mtv(y2)

                    For j As Int32 = 0 To n - 1
                        dhat(j) = c(j) - aty(j)
                    Next

                    Array.Copy(y2, y0, std.Min(y2.Length, m))
                End If
            End If

            ' 偏移到严格内部
            Dim minX As Double = Double.MaxValue
            Dim minS As Double = Double.MaxValue
            Dim minZ As Double = Double.MaxValue

            For j As Int32 = 0 To n - 1
                If xhat(j) < minX Then minX = xhat(j)
                If dhat(j) < minS Then minS = dhat(j)
                If ubFin(j) AndAlso -dhat(j) < minZ Then minZ = -dhat(j)
            Next

            Dim dx0 As Double = std.Max(0.0, -1.5 * minX)
            Dim ds0 As Double = std.Max(0.0, -1.5 * minS)
            Dim dz0 As Double = std.Max(0.0, -1.5 * minZ)

            For j As Int32 = 0 To n - 1
                x(j) = xhat(j) + dx0
                s(j) = dhat(j) + ds0
                z(j) = If(ubFin(j), -dhat(j) + dz0, 0.0)
            Next

            ' 夹进 (0, u) 内部
            For j As Int32 = 0 To n - 1
                If ubFin(j) Then
                    Dim room As Double = u(j)
                    Dim lo As Double = std.Min(0.0001, 0.001 * room)
                    Dim hi As Double = 0.5 * room

                    x(j) = std.Min(std.Max(x(j), lo), hi)
                Else
                    x(j) = std.Max(x(j), 0.0001)
                End If

                s(j) = std.Max(s(j), 0.0001)
                z(j) = If(ubFin(j), std.Max(z(j), 0.0001), 0.0)
            Next

            ' 平衡偏移：δ̂ = 0.5·(xᵀs + wᵀz)/Σ(·)
            Dim w(n - 1) As Double

            UpdateUpperSlack(x, w)

            Dim dot As Double = LinAlg.Dot(x, s)
            Dim sumD As Double = 0.0
            Dim sumX As Double = 0.0

            For j As Int32 = 0 To n - 1
                sumD += s(j)
                sumX += x(j)

                If ubFin(j) Then
                    dot += w(j) * z(j)
                    sumD += z(j)
                    sumX += w(j)
                End If
            Next

            Dim dx2 As Double = 0.5 * dot / std.Max(0.000000000001, sumD)
            Dim dd2 As Double = 0.5 * dot / std.Max(0.000000000001, sumX)

            For j As Int32 = 0 To n - 1
                If ubFin(j) Then
                    x(j) = std.Min(x(j) + dx2, 0.5 * u(j))
                    x(j) = std.Max(x(j), std.Min(0.0001, 0.001 * u(j)))
                    z(j) = std.Max(0.0001, z(j) + dd2)
                Else
                    x(j) = std.Max(0.0001, x(j) + dx2)
                End If

                s(j) = std.Max(0.0001, s(j) + dd2)
            Next

            y = y0
        End Sub

        ''' <summary>主入口</summary>
        Public Function Solve() As IpResult
            Dim x As Double() = Nothing
            Dim y As Double() = Nothing
            Dim s As Double() = Nothing
            Dim z As Double() = Nothing

            MehrotraStart(x, y, s, z)

            Dim w(n - 1) As Double

            UpdateUpperSlack(x, w)

            Dim normB = 1.0 + LinAlg.Norm2(b)
            Dim normC = 1.0 + LinAlg.Norm2(c)
            Dim status As String = "max_iter"
            Dim iters As Int32 = 0
            Dim stallCount As Int32 = 0
            Dim prevMetric As Double = Double.PositiveInfinity

            Dim rp(m - 1) As Double
            Dim rd(n - 1) As Double
            Dim rhs(m - 1) As Double
            Dim res(m - 1) As Double
            Dim theta(n - 1) As Double
            Dim xl(n - 1) As Double
            Dim wu(n - 1) As Double
            Dim g(n - 1) As Double
            Dim tg(n - 1) As Double
            Dim sigMuV(n - 1) As Double
            Dim dxA(n - 1) As Double, dsA(n - 1) As Double, dzA(n - 1) As Double
            Dim dx(n - 1) As Double, ds(n - 1) As Double, dz(n - 1) As Double
            Dim dy As Double() = Nothing

            For it = 0 To maxIter - 1
                iters = it + 1

                ' ---- 残差 ----
                Dim ax As Double() = Mat.Mv(x)

                For i As Int32 = 0 To m - 1
                    rp(i) = ax(i) - b(i)
                Next

                Dim aty0 As Double() = Mat.Mtv(y)

                For j As Int32 = 0 To n - 1
                    rd(j) = s(j) - z(j) - c(j) + aty0(j)
                Next

                Dim mu As Double = Complementarity(x, s, w, z)
                Dim nrp = LinAlg.Norm2(rp) / normB
                Dim nrd = LinAlg.Norm2(rd) / normC
                Dim obj = LinAlg.Dot(c, x)
                Dim ngap = mu / (1.0 + std.Abs(obj))

                If log IsNot Nothing Then
                    log.Add($"  IPM {it,3}: rp={nrp:E2} rd={nrd:E2} mu={mu:E2} obj={obj:G10}")
                End If

                If nrp <= tol AndAlso nrd <= tol AndAlso ngap <= tol Then
                    status = "optimal"
                    Exit For
                End If

                ' 停滞检测
                Dim metric = nrp + nrd + ngap

                If metric > prevMetric * (1.0 - 0.000000000001) Then
                    stallCount += 1
                    If stallCount >= 8 Then
                        status = "stalled"
                        Exit For
                    End If
                Else
                    stallCount = 0
                End If

                prevMetric = metric

                ' ---- Θ = (S/X + Z/W)⁻¹ ----
                For j As Int32 = 0 To n - 1
                    Dim sr As Double = s(j) / x(j)

                    If ubFin(j) Then
                        theta(j) = 1.0 / (sr + z(j) / w(j))
                    Else
                        theta(j) = 1.0 / sr
                    End If

                    theta(j) = std.Min(10000000000.0, std.Max(0.000000000001, theta(j)))
                Next

                ' ---- 正规方程规模（正则化量级基准）----
                Dim mScale As Double = 1.0
                Dim ndiag As Double() = Mat.NormalDiag(theta)

                For i As Int32 = 0 To m - 1
                    If ndiag(i) > mScale Then mScale = ndiag(i)
                Next

                Dim aP As Double = 0, aD As Double = 0
                Dim ok As Boolean = False

                For Each regMult In RegLadder
                    Dim reg As Double = regMult * mScale
                    Dim fac As INormalFactor = Mat.FactorNormal(theta, reg)

                    If fac Is Nothing Then Continue For

                    ' ========== 预测步（σ = 0，无校正项）==========
                    For j As Int32 = 0 To n - 1
                        xl(j) = -s(j)
                        wu(j) = If(ubFin(j), -z(j), 0.0)
                        g(j) = xl(j) - wu(j) + rd(j)
                        tg(j) = theta(j) * g(j)
                    Next

                    Dim atg As Double() = Mat.Mv(tg)

                    For i As Int32 = 0 To m - 1
                        rhs(i) = -rp(i) - atg(i)
                    Next

                    Dim dyA As Double() = fac.Solve(rhs)

                    If dyA Is Nothing Then Continue For

                    If fac.IsExact Then
                        ' 迭代精化：res = rhs − (M+reg·I)·dy
                        Dim mvy As Double() = fac.Mv(dyA)

                        For i As Int32 = 0 To m - 1
                            res(i) = rhs(i) - mvy(i)
                        Next

                        Dim dyr As Double() = fac.Solve(res)

                        If dyr IsNot Nothing Then
                            For i As Int32 = 0 To m - 1
                                dyA(i) += dyr(i)
                            Next
                        End If
                    End If

                    Dim atyA As Double() = Mat.Mtv(dyA)

                    For j As Int32 = 0 To n - 1
                        dxA(j) = theta(j) * (atyA(j) + g(j))
                        dsA(j) = xl(j) - (s(j) / x(j)) * dxA(j)
                        dzA(j) = If(ubFin(j), wu(j) + (z(j) / w(j)) * dxA(j), 0.0)
                    Next

                    Dim aAff As Double = std.Min(1.0, std.Min(MaxStepX(x, dxA), MaxStepDual(s, dsA, z, dzA)))
                    Dim muAff As Double = ComplementarityAt(x, dxA, s, dsA, w, z, dzA, aAff)
                    Dim sigma As Double = std.Min(1.0, std.Max(0.0, (muAff / mu) ^ 3))

                    ' ========== 校正步 ==========
                    For j As Int32 = 0 To n - 1
                        sigMuV(j) = sigma * mu
                        xl(j) = -s(j) + sigMuV(j) / x(j) + (dxA(j) * dsA(j)) / x(j)

                        If ubFin(j) Then
                            wu(j) = -z(j) + sigMuV(j) / w(j) + (dxA(j) * dzA(j)) / w(j)
                        Else
                            wu(j) = 0.0
                        End If

                        g(j) = xl(j) - wu(j) + rd(j)
                        tg(j) = theta(j) * g(j)
                    Next

                    atg = Mat.Mv(tg)

                    For i As Int32 = 0 To m - 1
                        rhs(i) = -rp(i) - atg(i)
                    Next

                    dy = fac.Solve(rhs)

                    If dy Is Nothing Then Continue For

                    If fac.IsExact Then
                        Dim mvy As Double() = fac.Mv(dy)

                        For i As Int32 = 0 To m - 1
                            res(i) = rhs(i) - mvy(i)
                        Next

                        Dim dyr As Double() = fac.Solve(res)

                        If dyr IsNot Nothing Then
                            For i As Int32 = 0 To m - 1
                                dy(i) += dyr(i)
                            Next
                        End If
                    End If

                    Dim atyC As Double() = Mat.Mtv(dy)

                    For j As Int32 = 0 To n - 1
                        dx(j) = theta(j) * (atyC(j) + g(j))
                        ds(j) = xl(j) - (s(j) / x(j)) * dx(j)
                        dz(j) = If(ubFin(j), wu(j) + (z(j) / w(j)) * dx(j), 0.0)
                    Next

                    aP = std.Min(1.0, 0.99 * MaxStepX(x, dx))
                    aD = std.Min(1.0, 0.99 * MaxStepDual(s, ds, z, dz))

                    ' 迭代法（PCG）未收敛：提升正则化档位重试，最后一档才接受
                    If Not fac.Converged AndAlso regMult <> RegLadder(RegLadder.Length - 1) Then
                        Continue For
                    End If

                    If aP > 0.000000001 OrElse aD > 0.000000001 Then
                        ok = True
                        Exit For        ' 当前档位可用
                    End If
                    ' 停滞 → 提升正则化档位重试
                Next

                If Not ok Then
                    status = "numeric_fail"
                    Exit For
                End If

                For j As Int32 = 0 To n - 1
                    x(j) += aP * dx(j)
                    s(j) += aD * ds(j)

                    If ubFin(j) Then z(j) += aD * dz(j)
                Next

                For i As Int32 = 0 To m - 1
                    y(i) += aD * dy(i)
                Next

                UpdateUpperSlack(x, w)

                Dim big As Double = 0

                For j As Int32 = 0 To n - 1
                    big = std.Max(big, std.Abs(x(j)))
                    big = std.Max(big, std.Abs(s(j)))
                Next

                If big > 100000000000000.0 Then
                    status = "diverged"
                    Exit For
                End If
            Next

            ' 非最优时按残差分类（供上层决定兜底策略）
            If status <> "optimal" Then
                Dim ax As Double() = Mat.Mv(x)

                For i As Int32 = 0 To m - 1
                    rp(i) = ax(i) - b(i)
                Next

                Dim atyE As Double() = Mat.Mtv(y)

                For j As Int32 = 0 To n - 1
                    rd(j) = s(j) - z(j) - c(j) + atyE(j)
                Next

                Dim nrp = LinAlg.Norm2(rp) / normB
                Dim nrd = LinAlg.Norm2(rd) / normC

                If nrp > 0.0001 AndAlso nrd < 0.000001 Then
                    status = "primal_infeasible"
                ElseIf nrd > 0.0001 AndAlso nrp < 0.000001 Then
                    status = "dual_infeasible"
                End If
            End If

            Return New IpResult With {.Status = status, .X = x, .Y = y, .S = s, .Z = z, .Iters = iters}
        End Function

    End Class

End Namespace
