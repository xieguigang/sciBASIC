' ============================================================================
' Simplex.vb — 修订单纯形（Phase 1 人造基 + Phase 2），双重角色：
'   1. Crossover 收尾的"单纯形式主元"清理 [readme §四 阶段3]
'   2. IPM 失败/不可行/无界时的独立兜底与证书
' ----------------------------------------------------------------------------
' 标准形 x ≥ 0 无上界 → 非基变量恒 0，x_B = B⁻¹b；每次主元后重构 LU
'   （中等规模足够；生产实现用 LU 更新 + Markowitz 稀疏主元——文档化简化）。
' Phase 1：人造单位列，min Σa；最优 > tol → 不可行证书；残留人造基变量
'   逐个驱出，行无法驱出 → 冗余行删除（影子价记 0）。
' Phase 2：Dantzig 定价 + 停滞 20 轮切 Bland（防循环）；比值检验无阻挡行
'   → 无界证书。
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.IPMCrossover

    Public Class SimplexResult

        Public Status As String        ' optimal/infeasible/unbounded/numeric_fail/max_iter
        Public X As Double()           ' 标准形全变量
        Public Y As Double()           ' 对偶（标准形方向）
        Public Basis As List(Of Int32)
        Public Iters As Int32
        Public DropRows As List(Of Int32)   ' 冗余行（已删除）

    End Class

    Public Class SimplexSolver

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

        ''' <summary>单位向量</summary>
        Private Shared Function Unit(len As Int32, i As Int32) As Double()
            Dim e(len - 1) As Double
            e(i) = 1.0
            Return e
        End Function

        Public Function Solve() As SimplexResult
            ' Phase 1：[A | I]，人造基
            Dim A1(m - 1, n + m - 1) As Double
            For i = 0 To m - 1
                For j = 0 To n - 1
                    A1(i, j) = A(i, j)
                Next
                A1(i, n + i) = 1.0
            Next
            Dim c1(n + m - 1) As Double
            For j = n To n + m - 1
                c1(j) = 1.0
            Next
            Dim basis As New List(Of Int32)()
            For i = 0 To m - 1
                basis.Add(n + i)
            Next
            Dim barred As New HashSet(Of Int32)()
            For j = n To n + m - 1
                barred.Add(j)               ' 人造列离基后禁止再入
            Next
            Dim st = LoopPhase1(A1, c1, basis, barred, 40 * std.Max(1, m))
            If st.Item1 <> "optimal" Then
                Return New SimplexResult With {.Status = st.Item1, .Iters = st.Item4,
                                               .DropRows = New List(Of Int32)()}
            End If
            Dim iters As Int32 = st.Item4
            basis = st.Item2
            ' 驱逐残留人造基变量 / 删除冗余行
            Dim rowsAlive As New List(Of Int32)()
            For i = 0 To m - 1
                rowsAlive.Add(i)
            Next
            Dim dropRows As New List(Of Int32)()
            Dim bi As Int32 = 0
            While bi < basis.Count
                If basis(bi) >= n Then
                    ' B = rowsAlive 行 × basis 列；ρ = B⁻ᵀe_i 定位可换入列
                    Dim Balive(rowsAlive.Count - 1, basis.Count - 1) As Double
                    For i = 0 To rowsAlive.Count - 1
                        For jj = 0 To basis.Count - 1
                            Balive(i, jj) = A1(rowsAlive(i), basis(jj))
                        Next
                    Next
                    Dim rho As Double() = Nothing
                    Dim facB = LinAlg.LuFactor(Balive)
                    If facB IsNot Nothing Then
                        rho = LinAlg.LuSolveT(facB, Unit(basis.Count, bi))
                    End If
                    Dim pivCol As Int32 = -1
                    If rho IsNot Nothing Then
                        For j = 0 To n - 1
                            If Not basis.Contains(j) Then
                                Dim dot As Double = 0
                                For i = 0 To rowsAlive.Count - 1
                                    dot += rho(i) * A1(rowsAlive(i), j)
                                Next
                                If std.Abs(dot) > 0.000000001 Then
                                    pivCol = j
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                    If pivCol >= 0 Then
                        basis(bi) = pivCol
                    Else
                        dropRows.Add(rowsAlive(bi))
                        rowsAlive.RemoveAt(bi)
                        basis.RemoveAt(bi)
                        bi -= 1
                    End If
                End If
                bi += 1
            End While
            ' Phase 2：真实成本，删除冗余行
            Dim keep As New List(Of Int32)()
            For i = 0 To m - 1
                If Not dropRows.Contains(i) Then keep.Add(i)
            Next
            Dim st2 As Tuple(Of String, List(Of Int32), Double(), Int32)
            If keep.Count = 0 Then
                ' 无约束平凡情形
                Dim x0(n - 1) As Double
                Return New SimplexResult With {.Status = "optimal", .X = x0,
                                               .Y = New Double(-1) {}, .Basis = New List(Of Int32)(),
                                               .Iters = iters, .DropRows = dropRows}
            End If
            st2 = LoopPhase2(keep, basis, 40 * std.Max(1, keep.Count))
            iters += st2.Item4
            If st2.Item1 <> "optimal" Then
                Return New SimplexResult With {.Status = st2.Item1, .Iters = iters, .DropRows = dropRows}
            End If
            basis = st2.Item2
            Dim x = New Double(n - 1) {}
            For jj = 0 To basis.Count - 1
                x(basis(jj)) = st2.Item3(jj)
            Next
            ' 对偶：y = B⁻ᵀc_B（keep 行 × basis 列）
            Dim Bf(keep.Count - 1, basis.Count - 1) As Double
            For i = 0 To keep.Count - 1
                For jj = 0 To basis.Count - 1
                    Bf(i, jj) = A(keep(i), basis(jj))
                Next
            Next
            Dim facF = LinAlg.LuFactor(Bf)
            Dim yFull(m - 1) As Double
            If facF IsNot Nothing Then
                Dim cB(basis.Count - 1) As Double
                For jj = 0 To basis.Count - 1
                    cB(jj) = c(basis(jj))
                Next
                Dim yk = LinAlg.LuSolveT(facF, cB)
                For i = 0 To keep.Count - 1
                    yFull(keep(i)) = yk(i)
                Next
            End If
            Return New SimplexResult With {.Status = "optimal", .X = x, .Y = yFull,
                                           .Basis = basis, .Iters = iters, .DropRows = dropRows}
        End Function

        ''' <summary>Phase 1 主循环：从人造基出发，xB = B⁻¹b</summary>
        Private Function LoopPhase1(A1 As Double(,), c1 As Double(),
                                    basis As List(Of Int32), barred As HashSet(Of Int32),
                                    maxIter As Int32) As Tuple(Of String, List(Of Int32), Double(), Int32)
            Dim iters As Int32 = 0
            Dim stall As Int32 = 0
            Dim bland As Boolean = False
            Dim prevObj As Double? = Nothing
            Dim bNorm = 1.0 + LinAlg.Norm2(b)
            While iters < maxIter
                iters += 1
                Dim Bm = LinAlg.TakeCols(A1, basis)
                Dim facB = LinAlg.LuFactor(Bm)
                If facB Is Nothing Then Return Tuple.Create("numeric_fail", basis, CType(Nothing, Double()), iters)
                Dim xB = LinAlg.LuSolve(facB, b)
                If xB.Min() < -0.0000001 * bNorm Then
                    Return Tuple.Create("infeasible", basis, xB, iters)
                End If
                Dim y = LinAlg.LuSolveT(facB, ColPick(c1, basis))
                Dim d = ReducedCosts(A1, c1, y)
                Dim enter = Price(d, basis, barred, bland, c1, 0.000000001)
                If enter < 0 Then
                    ' Phase 1 目标 = 残留人造变量之和
                    Dim obj1 As Double = 0
                    For i = 0 To basis.Count - 1
                        If basis(i) >= n Then obj1 += std.Max(0.0, xB(i))
                    Next
                    Dim status = If(obj1 <= 0.0000001 * bNorm, "optimal", "infeasible")
                    Return Tuple.Create(status, basis, xB, iters)
                End If
                Dim alpha = LinAlg.LuSolve(facB, ColGet(A1, enter))
                Dim leave = RatioTest(xB, alpha)
                If leave < 0 Then Return Tuple.Create("unbounded", basis, xB, iters)
                basis(leave) = enter
                Dim obj = 0.0
                For i = 0 To basis.Count - 1
                    obj += c1(basis(i)) * xB(i)
                Next
                If prevObj.HasValue AndAlso std.Abs(prevObj.Value - obj) < 0.00000000000001 Then
                    stall += 1
                    If stall >= 20 Then bland = True
                Else
                    stall = 0
                End If
                prevObj = obj
            End While
            Return Tuple.Create("max_iter", basis, CType(Nothing, Double()), iters)
        End Function

        ''' <summary>Phase 2 主循环：原始可行基出发（crossover 收尾复用入口）</summary>
        Public Function LoopPhase2(keepRows As List(Of Int32), basis As List(Of Int32),
                                   maxIter As Int32) As Tuple(Of String, List(Of Int32), Double(), Int32)
            Dim mm = keepRows.Count
            Dim iters As Int32 = 0
            Dim stall As Int32 = 0
            Dim bland As Boolean = False
            Dim prevObj As Double? = Nothing
            Dim bAlive(mm - 1) As Double
            For i = 0 To mm - 1
                bAlive(i) = b(keepRows(i))
            Next
            Dim bNorm = 1.0 + LinAlg.Norm2(bAlive)
            While iters < maxIter
                iters += 1
                Dim Bm(mm - 1, basis.Count - 1) As Double
                For i = 0 To mm - 1
                    For jj = 0 To basis.Count - 1
                        Bm(i, jj) = A(keepRows(i), basis(jj))
                    Next
                Next
                Dim facB = LinAlg.LuFactor(Bm)
                If facB Is Nothing Then Return Tuple.Create("numeric_fail", basis, CType(Nothing, Double()), iters)
                Dim xB = LinAlg.LuSolve(facB, bAlive)
                If xB.Min() < -0.0000001 * bNorm Then
                    Return Tuple.Create("infeasible", basis, xB, iters)
                End If
                Dim y = LinAlg.LuSolveT(facB, ColPick(c, basis))
                ' reduced costs（keep 行）
                Dim d(n - 1) As Double
                For j = 0 To n - 1
                    Dim sum As Double = c(j)
                    For i = 0 To mm - 1
                        sum -= A(keepRows(i), j) * y(i)
                    Next
                    d(j) = sum
                Next
                Dim enter = Price(d, basis, New HashSet(Of Int32)(), bland, c, 0.000000001)
                If enter < 0 Then
                    Return Tuple.Create("optimal", basis, xB, iters)
                End If
                Dim alpha = LuSolveCol(Bm, facB, ColGetAlive(A, keepRows, enter))
                Dim leave = RatioTest(xB, alpha)
                If leave < 0 Then Return Tuple.Create("unbounded", basis, xB, iters)
                basis(leave) = enter
                Dim obj = 0.0
                For i = 0 To basis.Count - 1
                    obj += c(basis(i)) * xB(i)
                Next
                If prevObj.HasValue AndAlso std.Abs(prevObj.Value - obj) < 0.00000000000001 Then
                    stall += 1
                    If stall >= 20 Then bland = True
                Else
                    stall = 0
                End If
                prevObj = obj
            End While
            Return Tuple.Create("max_iter", basis, CType(Nothing, Double()), iters)
        End Function

        ''' <summary>B⁻¹·col（给定已分解的 Bm/facB）</summary>
        Private Shared Function LuSolveCol(Bm As Double(,), fac As LuFactorization, col As Double()) As Double()
            Return LinAlg.LuSolve(fac, col)
        End Function

        ''' <summary>Dantzig 定价；bland=True 时取最小索引（防循环）</summary>
        Private Function Price(d As Double(), basis As List(Of Int32), barred As HashSet(Of Int32),
                               bland As Boolean, cc As Double(), tol As Double) As Int32
            Dim inB As New HashSet(Of Int32)(basis)
            If bland Then
                For j = 0 To n - 1
                    If Not inB.Contains(j) AndAlso Not barred.Contains(j) AndAlso
                       d(j) < -tol * (1.0 + std.Abs(cc(j))) Then Return j
                Next
                Return -1
            End If
            Dim best = -tol
            Dim enter As Int32 = -1
            For j = 0 To n - 1
                If Not inB.Contains(j) AndAlso Not barred.Contains(j) AndAlso d(j) < best Then
                    best = d(j)
                    enter = j
                End If
            Next
            Return enter
        End Function

        ''' <summary>比值检验：min x_B,i/α_i（α_i > 1e-9），并列取最大主元；无阻挡 → −1（无界）</summary>
        Private Function RatioTest(xB As Double(), alpha As Double()) As Int32
            Dim leave As Int32 = -1
            Dim ratio As Double = Double.PositiveInfinity
            For i = 0 To xB.Length - 1
                If alpha(i) > 0.000000001 Then
                    Dim r = std.Max(0.0, xB(i)) / alpha(i)
                    If r < ratio - 0.000000000001 OrElse (std.Abs(r - ratio) <= 0.000000000001 AndAlso
                       leave >= 0 AndAlso std.Abs(alpha(i)) > std.Abs(alpha(leave))) Then
                        ratio = r
                        leave = i
                    End If
                End If
            Next
            Return leave
        End Function

        Private Function ColGet(Am As Double(,), col As Int32) As Double()
            Dim rows = Am.GetLength(0)
            Dim v(rows - 1) As Double
            For i = 0 To rows - 1
                v(i) = Am(i, col)
            Next
            Return v
        End Function

        Private Function ColGetAlive(Am As Double(,), rows As List(Of Int32), col As Int32) As Double()
            Dim v(rows.Count - 1) As Double
            For i = 0 To rows.Count - 1
                v(i) = Am(rows(i), col)
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

        Private Function ReducedCosts(Am As Double(,), cc As Double(), y As Double()) As Double()
            Dim d(n - 1) As Double
            For j = 0 To n - 1
                Dim sum = cc(j)
                For i = 0 To Am.GetLength(0) - 1
                    sum -= Am(i, j) * y(i)
                Next
                d(j) = sum
            Next
            Return d
        End Function

    End Class

End Namespace
