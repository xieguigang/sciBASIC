' ============================================================================
' InteriorPoint.vb — 原始-对偶内点法（Mehrotra 预测-校正）[readme §二/§三]
' ----------------------------------------------------------------------------
' KKT 松弛：x_i·s_i = μ（μ→0）[readme §2.1]；牛顿方程经正规方程消元：
'   A D² Aᵀ Δy = rhs，D² = diag(x_i/s_i) [readme §2.2/§三]
'   rhs = −r_p − A·S⁻¹γ − A·D²·r_d，S⁻¹γ = −x + σμ/s + corr/s
'   （注意：+Ax 项已经含在 −A·(−x) 之中，切勿重复累加——镜像验证抓到的关键 bug）
'   Δx = D²AᵀΔy + S⁻¹γ + D²·r_d；Δs = −r_d − AᵀΔy
' 可行情形（r_p=r_d=0）时 rhs 退化为 readme 方框公式 b − σμ·A·S⁻¹e ✓
' 预测步（σ=0, corr=0）→ μ_aff → σ = (μ_aff/μ)³ → 校正步（含 ΔX_aff·ΔS_aff·e 补偿）
' 步长：fraction-to-boundary γ=0.99，原始/对偶分离取步 [readme §三]
' 数值手段：静态正则化（reg·I）+ 一次迭代精化共享同一 Cholesky 因子
'   [readme §2.2]；步长停滞 → 正则化 ×1000 重试 ≤4 档（自适应）；
'   连续 8 轮综合指标无改善 → 提前退出，交 simplex 兜底。
' 起始点：Mehrotra (1992) 启发式——最小范数 x̂/ŝ + 偏移平衡。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Class IpResult

        Public Status As String          ' optimal/primal_infeasible/dual_infeasible/stalled/numeric_fail/diverged/max_iter
        Public X As Double()
        Public Y As Double()
        Public S As Double()
        Public Iters As Int32

    End Class

    Public Class InteriorPointSolver

        Private ReadOnly A As Double(,)
        Private ReadOnly b As Double()
        Private ReadOnly c As Double()
        Private ReadOnly m As Int32
        Private ReadOnly n As Int32
        Private ReadOnly tol As Double
        Private ReadOnly maxIter As Int32
        Private ReadOnly log As List(Of String)

        ' 自适应正则化阶梯 [readme §2.2 的工程扩展]
        Private Shared ReadOnly RegLadder() As Double = {0.00000000001, 0.00000001, 0.00001, 0.01}

        Public Sub New(A As Double(,), b As Double(), c As Double(),
                       Optional tol As Double = 0.00000001, Optional maxIter As Int32 = 200,
                       Optional log As List(Of String) = Nothing)
            Me.A = A
            Me.b = b
            Me.c = c
            Me.m = A.GetLength(0)
            Me.n = A.GetLength(1)
            Me.tol = tol
            Me.maxIter = maxIter
            Me.log = log
        End Sub

        ''' <summary>Mehrotra 起始点：x̂/ŝ 最小范数解 + 偏移平衡</summary>
        Private Sub MehrotraStart(ByRef x As Double(), ByRef y As Double(), ByRef s As Double())
            Dim M0(m - 1, m - 1) As Double
            For i = 0 To m - 1
                For j = 0 To m - 1
                    Dim sum As Double = 0
                    For k = 0 To n - 1
                        sum += A(i, k) * A(j, k)
                    Next
                    M0(i, j) = sum
                Next
                M0(i, i) += 0.000000000001
            Next
            Dim L = LinAlg.Cholesky(M0)
            If L Is Nothing Then
                x = New Double(n - 1) {} : y = New Double(m - 1) {} : s = New Double(n - 1) {}
                For j = 0 To n - 1
                    x(j) = 1.0 : s(j) = 1.0
                Next
                Return
            End If
            Dim y1 = LinAlg.CholSolve(L, b)
            Dim xhat(n - 1) As Double
            For j = 0 To n - 1
                Dim sum As Double = 0
                For i = 0 To m - 1
                    sum += A(i, j) * y1(i)
                Next
                xhat(j) = sum
            Next
            Dim Ac(m - 1) As Double
            For i = 0 To m - 1
                Dim sum As Double = 0
                For k = 0 To n - 1
                    sum += A(i, k) * c(k)
                Next
                Ac(i) = sum
            Next
            Dim y2 = LinAlg.CholSolve(L, Ac)
            Dim shat(n - 1) As Double
            For j = 0 To n - 1
                Dim sum As Double = c(j)
                For i = 0 To m - 1
                    sum -= A(i, j) * y2(i)
                Next
                shat(j) = sum
            Next
            ' 偏移
            Dim minX = Double.MaxValue : Dim minS = Double.MaxValue
            For j = 0 To n - 1
                minX = Math.Min(minX, xhat(j))
                minS = Math.Min(minS, shat(j))
            Next
            Dim dx As Double = Math.Max(0.0, -1.5 * minX)
            Dim ds As Double = Math.Max(0.0, -1.5 * minS)
            x = New Double(n - 1) {}
            s = New Double(n - 1) {}
            For j = 0 To n - 1
                x(j) = xhat(j) + dx
                s(j) = shat(j) + ds
            Next
            ' 平衡偏移：δ̂ = 0.5·(xᵀs)/Σ(·)
            Dim dot As Double = LinAlg.Dot(x, s)
            Dim sumS As Double = s.Sum()
            Dim sumX As Double = x.Sum()
            Dim dx2 As Double = 0.5 * dot / Math.Max(0.000000000001, sumS)
            Dim ds2 As Double = 0.5 * dot / Math.Max(0.000000000001, sumX)
            For j = 0 To n - 1
                x(j) = Math.Max(0.0001, x(j) + dx2)
                s(j) = Math.Max(0.0001, s(j) + ds2)
            Next
            y = CType(y2.Clone(), Double())
        End Sub

        Private Function MaxStep(v As Double(), dv As Double()) As Double
            Dim alpha As Double = 1.0
            For i = 0 To n - 1
                If dv(i) < -0.00000000000001 Then
                    alpha = Math.Min(alpha, -v(i) / dv(i))
                End If
            Next
            Return alpha
        End Function

        ''' <summary>
        ''' 牛顿方向求解（共享 Cholesky 因子 + 一次迭代精化）。
        ''' S⁻¹γ = −x + σμ/s + corr/s；rhs = −r_p − A·S⁻¹γ − A·D²·r_d
        ''' （+Ax 已含在 −A·(−x) 内，勿重复累加——镜像对拍抓到的关键点）
        ''' </summary>
        Private Function NewtonDir(M As Double(,), L As Double(,), reg As Double,
                                   x As Double(), s As Double(), rp As Double(), rd As Double(),
                                   d2 As Double(), ADrD As Double(),
                                   sigmaMu As Double(), corr As Double()) As Double()()
            Dim Sinv(n - 1) As Double
            For j = 0 To n - 1
                Sinv(j) = -x(j) + sigmaMu(j) / s(j) + corr(j) / s(j)
            Next
            Dim mi As Integer = Me.m
            Dim rhs(mi - 1) As Double
            For i = 0 To mi - 1
                Dim sum As Double = -rp(i)
                For j = 0 To n - 1
                    sum -= A(i, j) * Sinv(j)
                Next
                rhs(i) = sum - ADrD(i)
            Next
            Dim dy = LinAlg.CholSolve(L, rhs)
            ' 迭代精化：res = rhs − (M+reg·I)·dy；dy += L⁻¹·res
            Dim res(mi - 1) As Double
            For i = 0 To mi - 1
                Dim sum As Double = rhs(i)
                For k = 0 To mi - 1
                    sum -= M(i, k) * dy(k)
                Next
                res(i) = sum - reg * dy(i)
            Next
            Dim dyr = LinAlg.CholSolve(L, res)
            For i = 0 To mi - 1
                dy(i) += dyr(i)
            Next
            Dim dx(n - 1) As Double
            Dim ds(n - 1) As Double
            For j = 0 To n - 1
                Dim aty As Double = 0
                For i = 0 To mi - 1
                    aty += A(i, j) * dy(i)
                Next
                dx(j) = d2(j) * aty - x(j) + sigmaMu(j) / s(j) + corr(j) / s(j) + d2(j) * rd(j)
                ds(j) = -rd(j) - aty
            Next
            Return New Double()() {dx, dy, ds}
        End Function

        ''' <summary>主入口</summary>
        Public Function Solve() As IpResult
            Dim x As Double() = Nothing
            Dim y As Double() = Nothing
            Dim s As Double() = Nothing
            MehrotraStart(x, y, s)
            Dim normB = 1.0 + LinAlg.Norm2(b)
            Dim normC = 1.0 + LinAlg.Norm2(c)
            Dim status As String = "max_iter"
            Dim iters As Int32 = 0
            Dim stallCount As Int32 = 0
            Dim prevMetric As Double = Double.PositiveInfinity

            For it = 0 To maxIter - 1
                iters = it + 1
                ' 残差
                Dim mi As Integer = Me.m
                Dim rp(mi - 1) As Double
                Dim rd(n - 1) As Double
                For i = 0 To mi - 1
                    Dim sum As Double = 0
                    For j = 0 To n - 1
                        sum += A(i, j) * x(j)
                    Next
                    rp(i) = sum - b(i)
                Next
                For j = 0 To n - 1
                    Dim sum As Double = s(j) - c(j)
                    For i = 0 To mi - 1
                        sum += A(i, j) * y(i)
                    Next
                    rd(j) = sum
                Next
                Dim mu As Double = LinAlg.Dot(x, s) / n
                Dim nrp = LinAlg.Norm2(rp) / normB
                Dim nrd = LinAlg.Norm2(rd) / normC
                Dim obj = LinAlg.Dot(c, x)
                Dim ngap = mu / (1.0 + Math.Abs(obj))
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
                ' D² 与 AD²r_d（本迭代常量）
                Dim d2(n - 1) As Double
                For j = 0 To n - 1
                    d2(j) = Math.Min(10000000000.0, Math.Max(0.000000000001, x(j) / s(j)))
                Next
                Dim ADrD(mi - 1) As Double
                For i = 0 To mi - 1
                    Dim sum As Double = 0
                    For j = 0 To n - 1
                        sum += A(i, j) * d2(j) * rd(j)
                    Next
                    ADrD(i) = sum
                Next

                ' 正规方程因子（每个正则化档位一次；预测/校正共享）
                Dim M(mi - 1, mi - 1) As Double
                For i = 0 To mi - 1
                    For k = 0 To mi - 1
                        Dim sum As Double = 0
                        For j = 0 To n - 1
                            sum += A(i, j) * d2(j) * A(k, j)
                        Next
                        M(i, k) = sum
                    Next
                Next
                Dim mScale = 1.0
                For i = 0 To mi - 1
                    For k = 0 To mi - 1
                        mScale = Math.Max(mScale, Math.Abs(M(i, k)))
                    Next
                Next

                Dim dxA As Double() = Nothing, dyA As Double() = Nothing, dsA As Double() = Nothing
                Dim dx As Double() = Nothing, dy As Double() = Nothing, ds As Double() = Nothing
                Dim aP As Double = 0, aD As Double = 0
                Dim ok As Boolean = False

                For Each regMult In RegLadder
                    Dim reg = regMult * mScale
                    Dim L = LinAlg.Cholesky(M, reg)
                    If L Is Nothing Then Continue For
                    Dim zero = New Double(n - 1) {}
                    Dim tripleA = NewtonDir(M, L, reg, x, s, rp, rd, d2, ADrD, zero, zero)
                    dxA = tripleA(0) : dyA = tripleA(1) : dsA = tripleA(2)
                    ' 预测步 μ_aff 与 σ
                    Dim aAff = Math.Min(1.0, Math.Min(MaxStep(x, dxA), MaxStep(s, dsA)))
                    Dim muAff As Double = 0
                    For j = 0 To n - 1
                        muAff += (x(j) + aAff * dxA(j)) * (s(j) + aAff * dsA(j))
                    Next
                    muAff /= n
                    Dim sigma = Math.Min(1.0, Math.Max(0.0, (muAff / mu) ^ 3))
                    ' 校正步
                    Dim corrV(n - 1) As Double
                    Dim sigMuV(n - 1) As Double
                    For j = 0 To n - 1
                        corrV(j) = dxA(j) * dsA(j)
                        sigMuV(j) = sigma * mu
                    Next
                    Dim triple = NewtonDir(M, L, reg, x, s, rp, rd, d2, ADrD, sigMuV, corrV)
                    dx = triple(0) : dy = triple(1) : ds = triple(2)
                    aP = Math.Min(1.0, 0.99 * MaxStep(x, dx))
                    aD = Math.Min(1.0, 0.99 * MaxStep(s, ds))
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
                For j = 0 To n - 1
                    x(j) += aP * dx(j)
                    s(j) += aD * ds(j)
                Next
                For i = 0 To mi - 1
                    y(i) += aD * dy(i)
                Next
                Dim big As Double = 0
                For j = 0 To n - 1
                    big = Math.Max(big, Math.Abs(x(j)))
                    big = Math.Max(big, Math.Abs(s(j)))
                Next
                If big > 100000000000000.0 Then
                    status = "diverged"
                    Exit For
                End If
            Next

            ' 非最优时按残差分类（供上层决定兜底策略）
            If status <> "optimal" Then
                Dim rp(m - 1) As Double
                Dim rd(n - 1) As Double
                For i = 0 To m - 1
                    Dim sum As Double = 0
                    For j = 0 To n - 1
                        sum += A(i, j) * x(j)
                    Next
                    rp(i) = sum - b(i)
                Next
                For j = 0 To n - 1
                    Dim sum As Double = s(j) - c(j)
                    For i = 0 To m - 1
                        sum += A(i, j) * y(i)
                    Next
                    rd(j) = sum
                Next
                Dim nrp = LinAlg.Norm2(rp) / normB
                Dim nrd = LinAlg.Norm2(rd) / normC
                If nrp > 0.0001 AndAlso nrd < 0.000001 Then
                    status = "primal_infeasible"
                ElseIf nrd > 0.0001 AndAlso nrp < 0.000001 Then
                    status = "dual_infeasible"
                End If
            End If
            Return New IpResult With {.Status = status, .X = x, .Y = y, .S = s, .Iters = iters}
        End Function

    End Class

End Namespace
