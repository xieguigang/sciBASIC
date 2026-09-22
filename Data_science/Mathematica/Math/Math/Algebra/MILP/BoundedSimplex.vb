' ============================================================================
' BoundedSimplex.vb — 有界变量修订单纯形（MILP 节点 LP 引擎）
' ----------------------------------------------------------------------------
' 工作形式：min cᵀx,  A x = b,  l ≤ x ≤ u   （界原生表达，不做下界平移）
'
' 三大职责：
'   1. Phase 1：以"符号人工列"构造可行基
'         artSign(i) = sign(b_i − A_i·v)，v 为非基本变量在其（有限）界上的取值；
'         人工列 = artSign(i)·e_i（无需翻转约束行，矩阵 A/b 保持原样）。
'         人工基目标 = Σ a_i；最优 > tol ⇒ 不可行证书。
'         随后把基本人工逐个换出（"冗余行"的人工留在基中并固定为 0，
'         其行内所有工作列系数为 0，因此永远不会阻塞主元）。
'   2. Phase 2 原始单纯形：Dantzig 定价 + 双侧比值检验（同时看 x→l 与 x→u），
'         进入变量先到对侧界 ⇒ 翻界（bound flip）；否则主元换基。
'         停滞 20 轮切 Bland 防循环（与既有 SimplexSolver 约定一致）。
'   3. 对偶单纯形（长步 + 翻界）：分支改界 / 加割后的热启动重优化。
'         选最大违反的基本变量出基，用 B⁻ᵀe_i 得到 tableau 行 α = wᵀA，
'         按 |d_j| / |α_j| 升序做比率检验；变量到对侧界的距离不足时先翻界。
'
' 基表示：basis(k) ≥ 0 → 工作列；basis(k) < 0 → 人工列 (−1−i)。人工列与工作
' 列索引空间分离，因此追加工作列（割平面松弛列）不会移动人工列下标，
' 热启动基可以跨"加行/加列"复用。
'
' 线性代数全部复用 LinAlg（LU 分解 / 前后代 / 转置求解），不引入新范式。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports std = System.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>有界单纯形状态。</summary>
    Public Enum BsStatus

        Optimal = 0
        Infeasible = 1
        Unbounded = 2
        MaxIter = 3
        NumericFail = 4

    End Enum

    ''' <summary>有界单纯形结果。</summary>
    Public Class BsResult

        Public Property Status As BsStatus
        Public Property StatusText As String
        ''' <summary>工作列取值（长度 = 工作列数）</summary>
        Public Property X As Double()
        ''' <summary>对偶值（长度 = 行数）</summary>
        Public Property Y As Double()
        ''' <summary>约简成本（长度 = 工作列数）</summary>
        Public Property ReducedCosts As Double()
        ''' <summary>基（≥0 工作列，&lt;0 人工列）</summary>
        Public Property Basis As Integer()
        ''' <summary>非基本工作列是否处于上界</summary>
        Public Property AtUpper As Boolean()
        Public Property Iters As Integer
        ''' <summary>内部 min 方向目标值</summary>
        Public Property Objective As Double

        Public ReadOnly Property IsOptimal As Boolean
            Get
                Return Status = BsStatus.Optimal
            End Get
        End Property

    End Class

    ''' <summary>
    ''' 有界变量修订单纯形。实例与一个问题矩阵 (A, b, c, l, u) 绑定；
    ''' 分支只修改 l/u，割平面通过重建实例（行/列追加）+ 热启动基复用。
    ''' </summary>
    Public Class BoundedSimplex

        Private ReadOnly m As Integer
        Private ReadOnly n As Integer
        Private ReadOnly A As Double(,)
        Private ReadOnly b As Double()
        Private ReadOnly c As Double()
        Private ReadOnly l As Double()
        Private ReadOnly u As Double()

        Private ReadOnly tolP As Double
        Private ReadOnly tolD As Double
        Private ReadOnly pivotEps As Double
        Private ReadOnly rangeEps As Double

        ''' <summary>人工列符号（sign_i·e_i），构造时确定</summary>
        Private ReadOnly artSign As Double()

        ' ---------- 可变状态 ----------
        Private basis As Integer()
        Private inBasis As Boolean()
        Private atUpper As Boolean()
        Private xB As Double()
        Private d As Double()
        Private y As Double()
        Private fac As LuFactorization
        Private phaseOne As Boolean
        Private iters As Integer
        Private diagnostic As String = ""

        Public Sub New(A As Double(,), b As Double(), c As Double(), l As Double(), u As Double(),
                       Optional tolP As Double = 0.0000001,
                       Optional tolD As Double = 0.0000001,
                       Optional pivotEps As Double = 0.000000001,
                       Optional rangeEps As Double = 0.000000001)

            Me.A = A
            Me.b = b
            Me.c = c
            Me.l = l
            Me.u = u
            Me.m = A.GetLength(0)
            Me.n = A.GetLength(1)
            Me.tolP = tolP
            Me.tolD = tolD
            Me.pivotEps = pivotEps
            Me.rangeEps = rangeEps

            Me.artSign = New Double(Me.m - 1) {}
            InitializeArtificialSigns()
            ResetState()
        End Sub

        Private Sub ResetState()
            basis = New Integer(m - 1) {}
            inBasis = New Boolean(n - 1) {}
            atUpper = New Boolean(n - 1) {}
            xB = New Double(m - 1) {}
            d = New Double(n - 1) {}
            y = New Double(m - 1) {}

            For j As Integer = 0 To n - 1
                ' 非基本初值：优先下界；下界 −∞ 时用上界
                atUpper(j) = Double.IsNegativeInfinity(l(j))
            Next
        End Sub

        ''' <summary>
        ''' 计算人工列符号：artSign(i) = sign(b_i − A_i·v)，v 取非基本变量的界值。
        ''' 保证初始人工变量 a_i = artSign(i)·(b_i − A_i·v) ≥ 0，从而人工基原始可行。
        ''' </summary>
        Private Sub InitializeArtificialSigns()
            For i As Integer = 0 To m - 1
                Dim s As Double = b(i)

                For j As Integer = 0 To n - 1
                    Dim aij As Double = A(i, j)

                    If aij = 0.0 Then Continue For

                    s -= aij * NonbasicBoundValue(j)
                Next

                artSign(i) = If(s < -tolP, -1.0, 1.0)
            Next
        End Sub

        ''' <summary>非基本变量的初始界值。</summary>
        Private Function NonbasicBoundValue(j As Integer) As Double
            If Double.IsNegativeInfinity(l(j)) Then Return u(j)
            Return l(j)
        End Function

        ''' <summary>非基本工作列的当前取值。</summary>
        Private Function NonbasicValue(j As Integer) As Double
            If atUpper(j) Then Return u(j)
            Return l(j)
        End Function

        ''' <summary>基本变量的界（人工列：Phase1 为 [0,+∞)，Phase2 固定为 [0,0]）。</summary>
        Private Sub BasicBounds(bj As Integer, ByRef lo As Double, ByRef hi As Double)
            If bj >= 0 Then
                lo = l(bj)
                hi = u(bj)
            Else
                lo = 0.0
                hi = If(phaseOne, Double.PositiveInfinity, 0.0)
            End If
        End Sub

        Private Function BasicLower(bj As Integer) As Double
            If bj >= 0 Then Return l(bj)
            Return 0.0
        End Function

        Private Function BasicUpper(bj As Integer) As Double
            If bj >= 0 Then Return u(bj)
            Return If(phaseOne, Double.PositiveInfinity, 0.0)
        End Function

        ''' <summary>人工列成本（Phase1 = 1，Phase2 = 0）。</summary>
        Private Function ArtCost(bj As Integer) As Double
            Return If(phaseOne, 1.0, 0.0)
        End Function

        ''' <summary>
        ''' 工作列在当前阶段的目标系数：Phase 1 的目标是 Σ 人工变量，故工作列成本为 0；
        ''' Phase 2 使用真实目标系数。
        ''' </summary>
        Private Function WorkCost(j As Integer) As Double
            Return If(phaseOne, 0.0, c(j))
        End Function

        Private Function BasisColumn(bj As Integer) As Double()
            Dim col(m - 1) As Double

            If bj >= 0 Then
                For i As Integer = 0 To m - 1
                    col(i) = A(i, bj)
                Next
            Else
                Dim art As Integer = -1 - bj
                col(art) = artSign(art)
            End If

            Return col
        End Function

        Private Function UnitVector(i As Integer) As Double()
            Dim e(m - 1) As Double
            e(i) = 1.0
            Return e
        End Function

        ''' <summary>当前解的完整工作列取值。</summary>
        Public Function CurrentX() As Double()
            Dim x(n - 1) As Double

            For j As Integer = 0 To n - 1
                x(j) = NonbasicValue(j)
            Next

            For k As Integer = 0 To m - 1
                If basis(k) >= 0 Then x(basis(k)) = xB(k)
            Next

            Return x
        End Function

        ''' <summary>
        ''' 当前基的 LU 分解（在 <see cref="Solve"/> 返回后有效）。
        ''' 割平面推导需要 B⁻ᵀe_i（tableau 行），直接复用该分解，避免重复计算。
        ''' </summary>
        Public ReadOnly Property Factorization As LuFactorization
            Get
                Return fac
            End Get
        End Property

        ''' <summary>内部 min 方向目标值。</summary>
        Private Function ObjectiveValue() As Double
            Dim v As Double = 0.0

            For k As Integer = 0 To m - 1
                Dim bj As Integer = basis(k)

                v += If(bj >= 0, WorkCost(bj), ArtCost(bj)) * xB(k)
            Next

            Return v
        End Function

        ' ====================================================================
        ' 主入口
        ' ====================================================================

        ''' <summary>
        ''' 求解。传入热启动基（可为 Nothing）时复用该基做对偶/原始重优化。
        ''' </summary>
        Public Function Solve(Optional warmBasis As Integer() = Nothing,
                              Optional warmAtUpper As Boolean() = Nothing,
                              Optional maxIter As Integer = 20000) As BsResult

            iters = 0
            phaseOne = False

            If warmBasis IsNot Nothing AndAlso warmBasis.Length = m Then
                If Not LoadWarmStart(warmBasis, warmAtUpper) Then
                    Return MakeResult(BsStatus.NumericFail, "热启动基非法（长度/索引/奇异性校验失败）。")
                End If

                Dim st = RunDual(maxIter)
                If st = BsStatus.Infeasible Then Return MakeResult(st, "LP 不可行（对偶单纯形比值检验无可行换代）。")
                If st = BsStatus.MaxIter Then Return MakeResult(st, "对偶单纯形迭代数超限。")
                If st = BsStatus.NumericFail Then Return MakeResult(st, "对偶单纯形矩阵分解奇异。")

                st = RunPrimal(maxIter)
                Return MakeResult(st, StatusMessage(st))
            End If

            ' ---------- 冷启动：Phase 1 ----------
            Dim p1 = Phase1(maxIter)

            If p1 <> BsStatus.Optimal Then
                Return MakeResult(p1, StatusMessage(p1))
            End If

            Dim st2 = RunPrimal(maxIter)

            If st2 = BsStatus.MaxIter Then
                diagnostic = $"Phase2 原始单纯形迭代超限（{iters} 次），基规模 {m}×{n}"
            End If

            Return MakeResult(st2, StatusMessage(st2))
        End Function

        Private Function StatusMessage(st As BsStatus) As String
            Dim detail As String = If(diagnostic.StringEmpty, "", $"（{diagnostic}）")

            Select Case st
                Case BsStatus.Optimal : Return "最优"
                Case BsStatus.Infeasible : Return "LP 不可行" & detail
                Case BsStatus.Unbounded : Return "LP 无界（存在无阻挡下降射线）" & detail
                Case BsStatus.MaxIter : Return "迭代数超限未收敛" & detail
                Case Else : Return "数值失败（基矩阵奇异）" & detail
            End Select
        End Function

        Private Function MakeResult(st As BsStatus, text As String) As BsResult
            Return New BsResult With {
                .Status = st,
                .StatusText = text,
                .X = CurrentX(),
                .Y = CType(y.Clone(), Double()),
                .ReducedCosts = CType(d.Clone(), Double()),
                .Basis = CType(basis.Clone(), Integer()),
                .AtUpper = CType(atUpper.Clone(), Boolean()),
                .Iters = iters,
                .Objective = ObjectiveValue()
            }
        End Function

        ''' <summary>校验并载入热启动基。</summary>
        Private Function LoadWarmStart(warmBasis As Integer(), warmAtUpper As Boolean()) As Boolean
            Dim hasArtificial As Boolean = False
            Dim seen As New HashSet(Of Integer)()

            basis = CType(warmBasis.Clone(), Integer())
            inBasis = New Boolean(n - 1) {}

            For k As Integer = 0 To m - 1
                Dim bj As Integer = basis(k)

                If bj >= 0 Then
                    If bj >= n Then Return False
                    If Not seen.Add(bj) Then Return False
                    inBasis(bj) = True
                Else
                    Dim art As Integer = -1 - bj

                    If art < 0 OrElse art >= m Then Return False
                    hasArtificial = True
                End If
            Next

            If warmAtUpper IsNot Nothing AndAlso warmAtUpper.Length = n Then
                For j As Integer = 0 To n - 1
                    atUpper(j) = warmAtUpper(j)
                Next
            Else
                For j As Integer = 0 To n - 1
                    If inBasis(j) Then Continue For
                    atUpper(j) = Double.IsNegativeInfinity(l(j))
                Next
            End If

            ' 基本变量的 atUpper 无意义，统一清为 False
            For j As Integer = 0 To n - 1
                If inBasis(j) Then atUpper(j) = False
            Next

            ' 基矩阵必须非奇异
            Dim Bm = BasisMatrix()

            If LinAlg.LuFactor(Bm) Is Nothing Then Return False

            Return True
        End Function

        Private Function BasisMatrix() As Double(,)
            Dim Bm(m - 1, m - 1) As Double

            For k As Integer = 0 To m - 1
                Dim bj As Integer = basis(k)

                If bj >= 0 Then
                    For i As Integer = 0 To m - 1
                        Bm(i, k) = A(i, bj)
                    Next
                Else
                    Dim art As Integer = -1 - bj
                    Bm(art, k) = artSign(art)
                End If
            Next

            Return Bm
        End Function

        ''' <summary>
        ''' 刷新分解、基本解、对偶值与约简成本。返回 False 表示数值失败。
        ''' </summary>
        Private Function Refresh() As Boolean
            fac = LinAlg.LuFactor(BasisMatrix())

            If fac Is Nothing Then Return False

            ' ---- xB = B⁻¹(b − Σ_{nonbasic} A_j v_j) ----
            Dim rhs(m - 1) As Double
            Array.Copy(b, rhs, m)

            For j As Integer = 0 To n - 1
                If inBasis(j) Then Continue For

                Dim vj As Double = NonbasicValue(j)

                If vj = 0.0 Then Continue For

                For i As Integer = 0 To m - 1
                    rhs(i) -= A(i, j) * vj
                Next
            Next

            xB = LinAlg.LuSolve(fac, rhs)

            If xB Is Nothing Then Return False

            ' ---- y = B⁻ᵀ c_B ----
            Dim cB(m - 1) As Double

            For k As Integer = 0 To m - 1
                Dim bj As Integer = basis(k)
                cB(k) = If(bj >= 0, WorkCost(bj), ArtCost(bj))
            Next

            y = LinAlg.LuSolveT(fac, cB)

            If y Is Nothing Then Return False

            ' ---- d_j = c_j − yᵀA_j ----
            For j As Integer = 0 To n - 1
                Dim s As Double = WorkCost(j)

                For i As Integer = 0 To m - 1
                    s -= A(i, j) * y(i)
                Next

                d(j) = s
            Next

            Return True
        End Function

        ''' <summary>B⁻¹·col。</summary>
        Private Function SolveWithBasis(col As Double()) As Double()
            Return LinAlg.LuSolve(fac, col)
        End Function

        ' ====================================================================
        ' Phase 1
        ' ====================================================================

        Private Function Phase1(maxIter As Integer) As BsStatus
            phaseOne = True

            ' 初始基 = 全部人工列；非基本工作列在界上
            For k As Integer = 0 To m - 1
                basis(k) = -1 - k
            Next

            For j As Integer = 0 To n - 1
                inBasis(j) = False
                atUpper(j) = Double.IsNegativeInfinity(l(j))
            Next

            Dim st = RunPrimal(maxIter)

            If st = BsStatus.MaxIter Then
                diagnostic = $"Phase1 原始单纯形迭代超限（{iters} 次）"
                Return BsStatus.MaxIter
            End If

            If st = BsStatus.NumericFail Then
                diagnostic = "Phase1 基矩阵分解奇异"
                Return BsStatus.NumericFail
            End If

            If st = BsStatus.Unbounded Then
                diagnostic = $"Phase1 目标意外无界（{iters} 次迭代）"
                Return BsStatus.MaxIter
            End If

            If Not Refresh() Then Return BsStatus.NumericFail

            ' ---- 不可行判定：人工变量之和 ----
            Dim artSum As Double = 0.0

            For k As Integer = 0 To m - 1
                If basis(k) < 0 Then artSum += std.Max(0.0, xB(k))
            Next

            Dim bNorm As Double = 1.0
            For i As Integer = 0 To m - 1
                bNorm += std.Abs(b(i))
            Next

            If artSum > tolP * bNorm Then
                Dim posCount As Integer = 0

                For k As Integer = 0 To m - 1
                    If basis(k) < 0 AndAlso xB(k) > tolP Then posCount += 1
                Next

                diagnostic = $"Phase1 人工残量 {artSum:G6}，正人工基变量 {posCount}/{m}，迭代 {iters}"
                Return BsStatus.Infeasible
            End If

            ' ---- 把人工变量逐个换出（冗余行的人工留在基中并固定为 0）----
            Dim moved As Boolean = True
            Dim guard As Integer = 0

            While moved AndAlso guard < 4 * m + 16
                moved = False
                guard += 1

                If Not Refresh() Then Return BsStatus.NumericFail

                For k As Integer = 0 To m - 1
                    If basis(k) >= 0 Then Continue For
                    If std.Abs(xB(k)) > tolP * bNorm Then Continue For

                    ' tableau 行：w = B⁻ᵀ e_k ⇒ α_j = wᵀA_j
                    Dim w = LinAlg.LuSolveT(fac, UnitVector(k))

                    If w Is Nothing Then Return BsStatus.NumericFail

                    Dim bestJ As Integer = -1
                    Dim bestAlpha As Double = pivotEps

                    For j As Integer = 0 To n - 1
                        If inBasis(j) Then Continue For
                        If u(j) - l(j) <= rangeEps Then Continue For

                        Dim alpha As Double = 0.0

                        For i As Integer = 0 To m - 1
                            alpha += w(i) * A(i, j)
                        Next

                        If std.Abs(alpha) > bestAlpha Then
                            bestAlpha = std.Abs(alpha)
                            bestJ = j
                        End If
                    Next

                    If bestJ >= 0 Then
                        basis(k) = bestJ
                        inBasis(bestJ) = True
                        atUpper(bestJ) = False
                        moved = True
                        Exit For
                    End If
                    ' 否则：该行为冗余行，人工变量留在基中（固定为 0，永不阻塞）
                Next
            End While

            ' ---- 切换到 Phase 2 ----
            phaseOne = False

            ' 人工变量固定为 0（非基本人工取值 0）
            Return BsStatus.Optimal
        End Function

        ' ====================================================================
        ' Phase 2 原始单纯形（有界变量，翻界 + 主元）
        ' ====================================================================

        Private Function RunPrimal(maxIter As Integer) As BsStatus
            Dim stall As Integer = 0
            Dim bland As Boolean = False
            Dim prevObj As Double? = Nothing

            For it As Integer = 1 To maxIter
                iters += 1

                If Not Refresh() Then Return BsStatus.NumericFail

                ' ---------- 定价 ----------
                Dim enter As Integer = -1
                Dim best As Double = 0.0

                For j As Integer = 0 To n - 1
                    If inBasis(j) Then Continue For

                    Dim range As Double = u(j) - l(j)

                    If range <= rangeEps Then Continue For

                    Dim sigmaJ As Double = If(atUpper(j), -1.0, 1.0)
                    Dim rc As Double = d(j) * sigmaJ
                    Dim thr As Double = -tolD * (1.0 + std.Abs(WorkCost(j)))

                    If rc < thr Then
                        If bland Then
                            enter = j
                            Exit For
                        ElseIf rc < best - 1.0E-15 Then
                            best = rc
                            enter = j
                        End If
                    End If
                Next

                If enter < 0 Then Return BsStatus.Optimal

                ' ---------- 方向 α = B⁻¹A_enter ----------
                Dim colEnter(m - 1) As Double

                For i As Integer = 0 To m - 1
                    colEnter(i) = A(i, enter)
                Next

                Dim alpha = SolveWithBasis(colEnter)

                If alpha Is Nothing Then Return BsStatus.NumericFail

                Dim sigma As Double = If(atUpper(enter), -1.0, 1.0)
                Dim thetaEnter As Double = u(enter) - l(enter)
                Dim thetaBlock As Double = Double.PositiveInfinity
                Dim leave As Integer = -1

                For i As Integer = 0 To m - 1
                    Dim beta As Double = -sigma * alpha(i)

                    If beta > pivotEps Then
                        Dim hiB As Double = BasicUpper(basis(i))

                        If Not Double.IsPositiveInfinity(hiB) Then
                            Dim t As Double = (hiB - xB(i)) / beta

                            If t < thetaBlock - 1.0E-12 Then
                                thetaBlock = t
                                leave = i
                            ElseIf std.Abs(t - thetaBlock) <= 1.0E-12 AndAlso leave >= 0 AndAlso
                                   std.Abs(alpha(i)) > std.Abs(alpha(leave)) Then

                                leave = i
                            End If
                        End If
                    ElseIf beta < -pivotEps Then
                        Dim loB As Double = BasicLower(basis(i))

                        If Not Double.IsNegativeInfinity(loB) Then
                            Dim t As Double = (xB(i) - loB) / (-beta)

                            If t < thetaBlock - 1.0E-12 Then
                                thetaBlock = t
                                leave = i
                            ElseIf std.Abs(t - thetaBlock) <= 1.0E-12 AndAlso leave >= 0 AndAlso
                                   std.Abs(alpha(i)) > std.Abs(alpha(leave)) Then

                                leave = i
                            End If
                        End If
                    End If
                Next

                If Double.IsPositiveInfinity(thetaBlock) AndAlso Double.IsPositiveInfinity(thetaEnter) Then
                    Return BsStatus.Unbounded
                End If

                Dim pivot As Boolean = thetaBlock < thetaEnter
                Dim theta As Double = If(pivot, thetaBlock, thetaEnter)

                If theta < 0.0 Then theta = 0.0

                ' ---------- 更新基本解 ----------
                For i As Integer = 0 To m - 1
                    xB(i) += (-sigma * alpha(i)) * theta
                Next

                If pivot Then
                    Dim leavingVar As Integer = basis(leave)
                    Dim betaLeave As Double = -sigma * alpha(leave)

                    If leavingVar >= 0 Then
                        atUpper(leavingVar) = betaLeave > 0.0
                        inBasis(leavingVar) = False
                    End If

                    Dim newVal As Double = NonbasicValue(enter) + sigma * theta

                    inBasis(enter) = True
                    atUpper(enter) = False
                    basis(leave) = enter
                    xB(leave) = newVal
                Else
                    atUpper(enter) = Not atUpper(enter)
                End If

                ' ---------- 停滞检测（切 Bland） ----------
                Dim obj As Double = ObjectiveValue()

                If prevObj.HasValue AndAlso std.Abs(prevObj.Value - obj) < 1.0E-14 Then
                    stall += 1

                    If stall >= 20 Then bland = True
                Else
                    stall = 0
                End If

                prevObj = obj
            Next

            Return BsStatus.MaxIter
        End Function

        ' ====================================================================
        ' 对偶单纯形（长步 + 翻界）：分支改界 / 加割后的热启动
        ' ====================================================================

        ''' <summary>
        ''' 返回 <see cref="BsStatus.Optimal"/> 表示"已原始可行"（无需对偶迭代，
        ''' 交由原始单纯形收尾）；返回 Infeasible / NumericFail / MaxIter 表示对应失败。
        ''' </summary>
        Private Function RunDual(maxIter As Integer) As BsStatus
            Dim bland As Boolean = False
            Dim stall As Integer = 0
            Dim prevViol As Double = Double.PositiveInfinity

            For it As Integer = 1 To maxIter
                iters += 1

                If Not Refresh() Then Return BsStatus.NumericFail

                ' ---------- 选最大违反的基本变量 ----------
                Dim leave As Integer = -1
                Dim maxViol As Double = tolP * 0.5

                For i As Integer = 0 To m - 1
                    Dim loB As Double
                    Dim hiB As Double

                    BasicBounds(basis(i), loB, hiB)

                    Dim v As Double = 0.0

                    If xB(i) < loB - tolP Then
                        v = loB - xB(i)
                    ElseIf xB(i) > hiB + tolP Then
                        v = xB(i) - hiB
                    End If

                    If v > maxViol Then
                        maxViol = v
                        leave = i
                    End If
                Next

                If leave < 0 Then Return BsStatus.Optimal     ' 已原始可行

                If basis(leave) < 0 Then
                    ' 人工变量被固定为 0，理论上不会违反；防御性返回
                    Return BsStatus.NumericFail
                End If

                Dim loL As Double
                Dim hiL As Double

                BasicBounds(basis(leave), loL, hiL)

                Dim tooLow As Boolean = xB(leave) < loL - tolP
                Dim need As Double = If(tooLow, 1.0, -1.0)
                Dim delta As Double = If(tooLow, loL - xB(leave), xB(leave) - hiL)

                ' ---------- tableau 行：w = B⁻ᵀ e_leave ⇒ α_j = wᵀA_j ----------
                Dim w = LinAlg.LuSolveT(fac, UnitVector(leave))

                If w Is Nothing Then Return BsStatus.NumericFail

                Dim cand As New List(Of DualCandidate)()

                For j As Integer = 0 To n - 1
                    If inBasis(j) Then Continue For

                    Dim dist As Double = u(j) - l(j)

                    If dist <= rangeEps Then Continue For

                    Dim alpha As Double = 0.0

                    For i As Integer = 0 To m - 1
                        alpha += w(i) * A(i, j)
                    Next

                    If std.Abs(alpha) <= pivotEps Then Continue For

                    Dim sigmaJ As Double = If(atUpper(j), -1.0, 1.0)
                    Dim g As Double = -sigmaJ * alpha

                    ' g 与 need 必须同号才能修复该违反
                    If g * need <= 0.0 Then Continue For

                    ' 对偶比值：|d_j| / |g|；Bland 模式下退化为按索引
                    Dim ratio As Double = If(bland, CDbl(j), std.Abs(d(j)) / std.Abs(g))

                    cand.Add(New DualCandidate With {
                        .j = j,
                        .ratio = ratio,
                        .g = g,
                        .dist = dist,
                        .sigma = sigmaJ
                    })
                Next

                If cand.Count = 0 Then
                    ' 无法修复 → 不可行证书
                    Return BsStatus.Infeasible
                End If

                cand.Sort(Function(p, q) p.ratio.CompareTo(q.ratio))

                ' ---------- 长步：先翻界，再主元 ----------
                Dim pivoted As Boolean = False

                For Each cd In cand
                    Dim contribution As Double = std.Abs(cd.g) * cd.dist

                    If contribution >= delta - 1.0E-12 Then
                        Dim theta As Double = delta / std.Abs(cd.g)

                        For i As Integer = 0 To m - 1
                            xB(i) += cd.g * theta
                        Next

                        atUpper(basis(leave)) = Not tooLow
                        inBasis(basis(leave)) = False

                        Dim newVal As Double = If(cd.sigma > 0.0, l(cd.j) + theta, u(cd.j) - theta)

                        inBasis(cd.j) = True
                        atUpper(cd.j) = False
                        basis(leave) = cd.j
                        xB(leave) = newVal
                        delta = 0.0
                        pivoted = True
                        Exit For
                    Else
                        ' 翻界：变量移到对侧界
                        For i As Integer = 0 To m - 1
                            xB(i) += cd.g * cd.dist
                        Next

                        atUpper(cd.j) = Not atUpper(cd.j)
                        delta -= contribution
                    End If
                Next

                If Not pivoted AndAlso delta > tolP Then
                    ' 候选全部翻界后仍无法修复 → 不可行
                    Return BsStatus.Infeasible
                End If

                ' ---------- 停滞检测 ----------
                If delta > prevViol * 0.999999 Then
                    stall += 1

                    If stall >= 30 Then bland = True
                Else
                    stall = 0
                End If

                prevViol = delta
            Next

            Return BsStatus.MaxIter
        End Function

        Private Class DualCandidate

            Public Property j As Integer
            Public Property ratio As Double
            Public Property g As Double
            Public Property dist As Double
            Public Property sigma As Double

        End Class

    End Class

End Namespace
