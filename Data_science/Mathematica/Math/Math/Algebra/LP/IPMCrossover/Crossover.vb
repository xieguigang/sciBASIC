' ============================================================================
' Crossover.vb — 内点解 → 最优基可行解 [readme §四]
' ----------------------------------------------------------------------------
' 阶段0 变量划分（严格互补）：x_j ≈ 0 且 s_j ≥ x_j → 非基下界(0)；
'   x_j > κ 且 x_j ≥ s_j → 基候选。κ = 1e-8·max(1,‖x‖∞)。
' 阶段1 构造初始基：候选按 x 降序贪心 + 秩检测（EchelonRank）；不足 m 列时
'   用剩余列补（优先稀疏列）。全部不入基的候选 = 超基本（带 IPM 值）。
' 阶段2 主元循环消超基本（保原始可行）：对超基本 j（先取 |x_j| 最大者），
'   沿"压向下界 0"方向做比率测试 δ_max = min_{α_i<0} x_B,i/(−α_i)：
'   δ_max ≥ x_j → 直接到界（非基化，无主元）；否则阻挡基本变量出界(到 0)、
'   j 以值 x_j−δ_max 入基。
' 阶段3 对偶检查：非基 reduced cost < −tol → 原始单纯形收尾至最优
'   [readme §四 "单纯形式主元收尾"]。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Class CrossoverResult

        Public Status As String        ' optimal/infeasible/unbounded/numeric_fail/max_iter/rank_deficient
        Public X As Double()           ' 标准形全变量
        Public Y As Double()
        Public Basis As List(Of Int32)
        Public Pivots As Int32
        Public BoundFlips As Int32

    End Class

    Public Class CrossoverSolver

        Private ReadOnly A As Double(,)
        Private ReadOnly b As Double()
        Private ReadOnly c As Double()
        Private ReadOnly m As Int32
        Private ReadOnly n As Int32
        Private ReadOnly log As List(Of String)

        Public Sub New(A As Double(,), b As Double(), c As Double(), Optional log As List(Of String) = Nothing)
            Me.A = A
            Me.b = b
            Me.c = c
            Me.m = A.GetLength(0)
            Me.n = A.GetLength(1)
            Me.log = log
        End Sub

        Public Function Run(xIpm As Double(), sIpm As Double()) As CrossoverResult
            ' ---- 阶段 0：划分 ----
            Dim xMax = 1.0
            For j = 0 To n - 1
                xMax = std.Max(xMax, std.Abs(xIpm(j)))
            Next
            Dim kappa = 0.00000001 * xMax
            Dim basicCand As New List(Of Int32)()
            Dim nonbasic As New HashSet(Of Int32)()
            For j = 0 To n - 1
                If xIpm(j) > kappa AndAlso xIpm(j) >= sIpm(j) Then
                    basicCand.Add(j)
                Else
                    nonbasic.Add(j)
                End If
            Next
            If log IsNot Nothing Then
                log.Add($"  Crossover 划分: 基候选={basicCand.Count} 非基={nonbasic.Count}")
            End If
            ' ---- 阶段 1：贪心构造初始基 ----
            Dim basis As New List(Of Int32)()
            For Each j In basicCand.OrderByDescending(Function(t) xIpm(t))
                If basis.Count = m Then Exit For
                Dim cols As New List(Of Int32)(basis)
                cols.Add(j)
                If LinAlg.EchelonRank(LinAlg.TakeCols(A, cols)) > basis.Count Then
                    basis.Add(j)
                End If
            Next
            If basis.Count < m Then
                ' 补齐：剩余列优先稀疏
                Dim rest As New List(Of Int32)()
                For j = 0 To n - 1
                    If Not basis.Contains(j) Then rest.Add(j)
                Next
                rest.Sort(Function(p, q)
                              Dim nzP = ColNnz(p), nzQ = ColNnz(q)
                              If nzP <> nzQ Then Return nzP.CompareTo(nzQ)
                              Return -xIpm(p).CompareTo(-xIpm(q))
                          End Function)
                For Each j In rest
                    If basis.Count = m Then Exit For
                    Dim cols As New List(Of Int32)(basis)
                    cols.Add(j)
                    If LinAlg.EchelonRank(LinAlg.TakeCols(A, cols)) > basis.Count Then
                        basis.Add(j)
                    End If
                Next
            End If
            If basis.Count < m Then
                Return New CrossoverResult With {.Status = "rank_deficient"}
            End If
            Dim inB As New HashSet(Of Int32)(basis)
            ' 超基本 = 候选未入基（带 IPM 值）
            Dim superVal As New Dictionary(Of Int32, Double)()
            For Each j In basicCand
                If Not inB.Contains(j) Then superVal(j) = xIpm(j)
            Next
            Dim pivots As Int32 = 0
            Dim flips As Int32 = 0
            ' ---- 阶段 2：主元循环 ----
            While superVal.Count > 0
                ' 取值最大的超基本
                Dim j As Int32 = -1
                Dim bestV As Double = Double.NegativeInfinity
                For Each kvp In superVal
                    If kvp.Value > bestV Then
                        bestV = kvp.Value
                        j = kvp.Key
                    End If
                Next
                Dim xj = superVal(j)
                ' x_B = B⁻¹(b − Σ 超基本列×值)
                Dim rhs(m - 1) As Double
                For i = 0 To m - 1
                    rhs(i) = b(i)
                Next
                For Each kvp In superVal
                    For i = 0 To m - 1
                        rhs(i) -= A(i, kvp.Key) * kvp.Value
                    Next
                Next
                Dim Bm = LinAlg.TakeCols(A, basis)
                Dim facB = LinAlg.LuFactor(Bm)
                If facB Is Nothing Then Return New CrossoverResult With {.Status = "numeric_fail"}
                Dim xB = LinAlg.LuSolve(facB, rhs)
                Dim alpha = LinAlg.LuSolve(facB, ColOf(j))
                ' 方向：x_j 下降 δ → x_B + δ·α ≥ 0
                Dim dmax As Double = Double.PositiveInfinity
                Dim leave As Int32 = -1
                For i = 0 To m - 1
                    If alpha(i) < -0.000000000001 Then
                        Dim r = std.Max(0.0, xB(i)) / (-alpha(i))
                        If r < dmax Then
                            dmax = r
                            leave = i
                        End If
                    End If
                Next
                If Double.IsPositiveInfinity(dmax) Then dmax = Double.MaxValue
                If dmax >= xj - 0.000000000001 Then
                    ' 直接到界
                    superVal.Remove(j)
                    nonbasic.Add(j)
                    flips += 1
                Else
                    ' 主元：j 入基（值 xj − δmax），出基变量到下界 0（非基）
                    Dim delta = dmax
                    Dim enterVal = std.Max(0.0, xj - delta)
                    superVal.Remove(j)
                    Dim leaving = basis(leave)
                    basis(leave) = j
                    inB.Remove(leaving)
                    inB.Add(j)
                    nonbasic.Add(leaving)
                    pivots += 1
                    If log IsNot Nothing Then
                        log.Add($"    pivot: var{j}(val={enterVal:G6}) 入基, var{leaving} 出基到 0")
                    End If
                End If
            End While
            ' ---- 阶段 3：对偶检查 + 单纯形收尾 ----
            Dim Bfin = LinAlg.TakeCols(A, basis)
            Dim facF = LinAlg.LuFactor(Bfin)
            If facF Is Nothing Then Return New CrossoverResult With {.Status = "numeric_fail"}
            Dim xBf = LinAlg.LuSolve(facF, b)
            Dim y = LinAlg.LuSolveT(facF, ColPick(c, basis))
            Dim dualOk As Boolean = True
            For j = 0 To n - 1
                If Not basis.Contains(j) Then
                    Dim d = c(j)
                    For i = 0 To m - 1
                        d -= A(i, j) * y(i)
                    Next
                    If d < -0.0000001 * (1.0 + std.Abs(c(j))) Then
                        dualOk = False
                        Exit For
                    End If
                End If
            Next
            If log IsNot Nothing Then
                log.Add($"  Crossover: 主元 {pivots} 次翻界 {flips} 次，对偶可行={dualOk}")
            End If
            If Not dualOk Then
                Dim sx As New SimplexSolver(A, b, c, log)
                Dim keep As New List(Of Int32)()
                For i = 0 To m - 1
                    keep.Add(i)
                Next
                Dim res = sx.LoopPhase2(keep, basis, 40 * std.Max(1, m))
                If res.Item1 <> "optimal" Then
                    Return New CrossoverResult With {.Status = res.Item1}
                End If
                basis = res.Item2
                xBf = res.Item3
                Bfin = LinAlg.TakeCols(A, basis)
                facF = LinAlg.LuFactor(Bfin)
                If facF Is Nothing Then Return New CrossoverResult With {.Status = "numeric_fail"}
                y = LinAlg.LuSolveT(facF, ColPick(c, basis))
            End If
            Dim x = New Double(n - 1) {}
            For jj = 0 To basis.Count - 1
                x(basis(jj)) = xBf(jj)
            Next
            Return New CrossoverResult With {.Status = "optimal", .X = x, .Y = y,
                                             .Basis = basis, .Pivots = pivots, .BoundFlips = flips}
        End Function

        Private Function ColNnz(j As Int32) As Int32
            Dim cnt As Int32 = 0
            For i = 0 To m - 1
                If std.Abs(A(i, j)) > 0 Then cnt += 1
            Next
            Return cnt
        End Function

        Private Function ColOf(j As Int32) As Double()
            Dim v(m - 1) As Double
            For i = 0 To m - 1
                v(i) = A(i, j)
            Next
            Return v
        End Function

        Private Function ColPick(v As Double(), idx As List(Of Int32)) As Double()
            Dim out_(idx.Count - 1) As Double
            For i = 0 To idx.Count - 1
                out_(i) = v(idx(i))
            Next
            Return out_
        End Function

    End Class

End Namespace
