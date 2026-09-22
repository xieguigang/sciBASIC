#Region "Microsoft.VisualBasic::7568a7c1b1b3880c01e1df0c87d7594c, Data_science\Mathematica\Math\Math\Algebra\MILP\BranchAndBound.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 658
    '    Code Lines: 433 (65.81%)
    ' Comment Lines: 78 (11.85%)
    '    - Xml Docs: 20.51%
    ' 
    '   Blank Lines: 147 (22.34%)
    '     File Size: 27.75 KB


    '     Class BranchAndBound
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ApplyRootCuts, BestOriginalValue, Finish, GlobalBoundInternal, PickBranchColumn
    '                   PickByPseudoCost, Pop, PushChild, RelativeGap, Solve
    '                   SolveUnconstrained, TakeId
    ' 
    '         Sub: Push, RecordIncumbent, TryHeuristics, UpdatePseudoCost
    '         Class Node
    ' 
    '             Properties: AtUpper, Basis, Bound, BranchUp, BranchVar
    '                         Depth, Id, L, ParentBound, U
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' BranchAndBound.vb — 分支定界搜索（配根节点割平面）
' ----------------------------------------------------------------------------
' 流程：
'   根 LP 松弛（BoundedSimplex）→ 根节点 GMI 割若干轮（cut-and-branch，
'   割平面为全局有效不等式，在根节点收紧松弛后一次性并行整棵树）
'   → 初始化 incumbent（舍入 + 潜水启发式）
'   → best-bound / depth-first 搜索：
'        · 节点 LP 用父节点基热启动（分支只改工作列 l/u，矩阵不变）
'        · 按界剪枝（node.Bound ≥ incumbent − 绝对间隙）
'        · 分数整数变量 → 两个子节点（x_j ≤ ⌊v⌋ / x_j ≥ ⌈v⌉）
'        · 得到整数可行解 → 更新 incumbent 与全局界
'        · 时间 / 节点 / 开集规模 / 相对间隙 四重终止条件
'
' 界与目标的方向约定：内部一律按 min 处理（MilpLpForm.Sigma 已把 max 归一），
' 报告时用 ObjOffset + Sigma·内部值 映回原始方向。
'
' 已知简化（如实声明）：
'   · 割平面只在根节点生成（标准 cut-and-branch 策略）；节点级割留作扩展；
'   · 开集用线性扫描做 best-bound / depth-first 选择（中等规模足够）。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>分支定界搜索。</summary>
    Public Class BranchAndBound

        Private Class Node

            ''' <summary>内部 min 方向的松弛界（子树的下界）</summary>
            Public Property Bound As Double
            Public Property L As Double()
            Public Property U As Double()
            Public Property Basis As Integer()
            Public Property AtUpper As Boolean()
            Public Property Depth As Integer
            Public Property Id As Integer

            ' ---- 伪成本统计用（生成该节点时的分支信息）----
            ''' <summary>生成该节点时分支的原始变量索引；根节点为 −1</summary>
            Public Property BranchVar As Integer = -1
            ''' <summary>是否为向上分支（x ≥ ⌈v⌉）</summary>
            Public Property BranchUp As Boolean = False
            ''' <summary>父节点的界（用于计算分支带来的界退化）</summary>
            Public Property ParentBound As Double = 0.0

        End Class

        Private ReadOnly form As MilpLpForm
        Private ReadOnly options As MilpOptions
        Private ReadOnly log As List(Of String)
        Private ReadOnly watch As Stopwatch
        Private ReadOnly tol As Double

        Private simplex As BoundedSimplex
        Private ReadOnly open As New List(Of Node)()

        Private lpSolves As Integer = 0
        Private cutsAdded As Integer = 0
        Private heuristicsHits As Integer = 0
        Private nodesExplored As Integer = 0
        Private nextId As Integer = 0
        ''' <summary>因 LP 数值失败（冷启动也无法求解）而被丢弃的子树数量</summary>
        Private droppedByNumeric As Integer = 0

        Private bestX As Double() = Nothing
        Private bestInternal As Double = Double.PositiveInfinity
        Private rootRelax As Double? = Nothing
        Private incumbentAtRoot As Boolean = False

        Private stopStatus As MilpStatus = MilpStatus.Optimal
        Private stopMessage As String = ""

        ''' <summary>伪成本状态：按原始变量累计"分支单位取整距离引起的界退化"</summary>
        Private ReadOnly pcDown As Double()
        Private ReadOnly pcUp As Double()
        Private ReadOnly pcDownCount As Integer()
        Private ReadOnly pcUpCount As Integer()

        Public Sub New(form As MilpLpForm, options As MilpOptions, log As List(Of String), watch As Stopwatch)
            Me.form = form
            Me.options = options
            Me.log = log
            Me.watch = watch
            Me.tol = options.FeasibilityTolerance

            Dim n As Integer = form.OriginalVariableCount

            pcDown = New Double(n - 1) {}
            pcUp = New Double(n - 1) {}
            pcDownCount = New Integer(n - 1) {}
            pcUpCount = New Integer(n - 1) {}

            For j As Integer = 0 To n - 1
                pcDown(j) = 1.0
                pcUp(j) = 1.0
            Next
        End Sub

        ' ====================================================================
        ' 主流程
        ' ====================================================================

        Public Function Solve() As MilpSolution
            If form.Stats.IsInfeasible Then
                Return Finish(MilpStatus.Infeasible, form.Stats.InfeasibleReason)
            End If

            If form.Rows = 0 Then
                ' 无约束：逐分量取使目标最优的界（MILP 整数变量同样适用）
                Dim x0 = SolveUnconstrained()
                bestX = form.ToOriginalSolution(x0)
                bestInternal = form.InternalObjective(x0)
                Return Finish(MilpStatus.Optimal, "无约束问题（各变量独立取最优界）。")
            End If

            simplex = New BoundedSimplex(form.A, form.b, form.c, form.l, form.u, tol, tol)

            Dim res As BsResult = simplex.Solve(Nothing, Nothing, options.LpIterationLimit)
            lpSolves += 1

            If res.Status = BsStatus.Infeasible Then Return Finish(MilpStatus.Infeasible, $"根 LP 松弛不可行：{res.StatusText}")
            If res.Status = BsStatus.Unbounded Then Return Finish(MilpStatus.Unbounded, "根 LP 松弛无界（存在无界下降射线）。")
            If Not res.IsOptimal Then Return Finish(MilpStatus.Error, $"根 LP 松弛未收敛：{res.StatusText}")

            rootRelax = form.ToOriginalObjective(res.X)

            If options.Verbose Then
                log.Add($"  根 LP 松弛: 目标 {rootRelax.Value:G8}（{res.Iters} 次迭代）")
            End If

            ' ---------- 根节点割平面 ----------
            If options.EnableCuts AndAlso options.RootCutRounds > 0 Then
                res = ApplyRootCuts(res)

                If res Is Nothing Then Return Finish(MilpStatus.Error, "根节点割平面：重优化数值失败。")
                If res.Status = BsStatus.Infeasible Then Return Finish(MilpStatus.Infeasible, "根 LP 松弛 + 割平面后不可行。")
                If res.Status = BsStatus.Unbounded Then Return Finish(MilpStatus.Unbounded, "加入割平面后的根 LP 无界。")
                If Not res.IsOptimal Then Return Finish(MilpStatus.Error, $"割平面重优化未收敛：{res.StatusText}")
            End If

            ' ---------- 初始 incumbent ----------
            If options.EnableHeuristics Then
                TryHeuristics(res)
            End If

            ' ---------- 根节点入队 ----------
            Push(New Node With {
                .Bound = form.InternalObjective(res.X),
                .L = CType(form.l.Clone(), Double()),
                .U = CType(form.u.Clone(), Double()),
                .Basis = res.Basis,
                .AtUpper = res.AtUpper,
                .Depth = 0,
                .Id = TakeId()
            })

            ' ================= 主循环 =================
            While open.Count > 0
                If nodesExplored >= options.MaxNodes Then
                    stopStatus = MilpStatus.NodeLimit
                    stopMessage = $"达到节点上限 {options.MaxNodes}。"
                    Exit While
                End If

                If watch.Elapsed.TotalSeconds >= options.MaxSeconds Then
                    stopStatus = MilpStatus.TimeLimit
                    stopMessage = $"达到时间上限 {options.MaxSeconds} 秒。"
                    Exit While
                End If

                If open.Count > options.MaxOpenNodes Then
                    stopStatus = MilpStatus.NodeLimit
                    stopMessage = $"开集节点数超过上限 {options.MaxOpenNodes}（内存保护）。"
                    Exit While
                End If

                If bestX IsNot Nothing AndAlso RelativeGap() <= options.RelativeGap Then
                    stopStatus = MilpStatus.Optimal
                    stopMessage = "已证明最优（相对间隙 ≤ 相对间隙容差）。"
                    Exit While
                End If

                Dim node As Node = Pop()

                ' 按界剪枝
                If bestX IsNot Nothing AndAlso node.Bound >= bestInternal - options.AbsoluteGap Then
                    If options.Verbose Then
                        log.Add($"    剪枝 节点#{node.Id} 深度{node.Depth} 界{node.Bound:G8} vs incumbent {bestInternal:G8}")
                    End If

                    Continue While
                End If

                nodesExplored += 1

                ' ---------- 热启动求解节点 LP ----------
                Array.Copy(node.L, form.l, form.Cols)
                Array.Copy(node.U, form.u, form.Cols)

                Dim r As BsResult = simplex.Solve(node.Basis, node.AtUpper, options.LpIterationLimit)
                lpSolves += 1

                If Not r.IsOptimal Then
                    ' 热启动（对偶单纯形）失败 → 冷启动兜底重解，避免把"数值失败"
                    ' 误判为不可行而静默丢弃子树（这会破坏最优性证明）。
                    r = simplex.Solve(Nothing, Nothing, options.LpIterationLimit)
                    lpSolves += 1
                End If

                If r.Status = BsStatus.Infeasible Then Continue While

                If r.Status = BsStatus.Unbounded Then
                    stopStatus = MilpStatus.Unbounded
                    stopMessage = "分支子问题 LP 无界，原 MILP 目标无界。"
                    Exit While
                End If

                If Not r.IsOptimal Then
                    ' 冷启动仍失败：该子树无法证明，记录下来（最终不宣称最优）
                    droppedByNumeric += 1
                    Continue While
                End If

                Dim nodeBound As Double = form.InternalObjective(r.X)

                UpdatePseudoCost(node, nodeBound)

                If bestX IsNot Nothing AndAlso nodeBound >= bestInternal - options.AbsoluteGap Then
                    Continue While
                End If

                ' ---------- 整数可行性 ----------
                Dim k As Integer = PickBranchColumn(r)

                If k < 0 Then
                    RecordIncumbent(r.X)
                    Continue While
                End If

                ' ---------- 节点启发式（抽样调用，控制开销）----------
                If options.EnableHeuristics AndAlso (nodesExplored Mod 4 = 0) Then
                    TryHeuristics(r)
                End If

                ' ---------- 分支 ----------
                Dim j As Integer = form.ColumnOriginal(k)
                Dim ob = form.GetOriginalBounds(j, node.L, node.U)
                Dim xj As Double = form.ColumnShift(k) + form.ColumnSign(k) * r.X(k)

                Dim down As Double = std.Floor(xj)
                Dim up As Double = std.Ceiling(xj)

                ' 数值保护：确保两侧都在当前值之外
                If down >= xj - 1.0E-12 Then down = xj - 1.0
                If up <= xj + 1.0E-12 Then up = xj + 1.0

                Dim pushed As Boolean = False
                Dim pushDown As Boolean = PushChild(node, k, j, ob.lo, down, False, nodeBound, r)
                Dim pushUp As Boolean = PushChild(node, k, j, up, ob.hi, True, nodeBound, r)

                pushed = pushDown OrElse pushUp

                If options.Verbose Then
                    log.Add($"    节点#{node.Id} 深度{node.Depth} 界{nodeBound:G8} → 分支 var{j} 值{xj:G10} " &
                            $"[≤{down:G6}:{If(pushDown, "推", "跳过")} ≥{up:G6}:{If(pushUp, "推", "跳过")}] 开集{open.Count}")
                End If

                If Not pushed Then
                    ' 分支未能收紧任何一侧（数值退化）→ 该节点视为叶子
                    If Not MilpHeuristics.Feasible(form, r.X, tol * 10.0) Then
                        ' 无法得到整数解，直接剪枝
                    Else
                        RecordIncumbent(r.X)
                    End If
                End If
            End While

            ' ---------- 搜索摘要（始终记录，便于诊断）----------
            log.Add($"  搜索结束: 节点 {nodesExplored}，开集 {open.Count}，LP 求解 {lpSolves}，" &
                    $"incumbent {(If(bestX Is Nothing, "无", BestOriginalValue().ToString("G8")))}，" &
                    $"全局界 {(If(Double.IsNaN(GlobalBoundInternal()), "n/a", (form.ObjOffset + form.Sigma * GlobalBoundInternal()).ToString("G8")))}，" &
                    $"丢弃 {droppedByNumeric}")

            ' ---------- 结果状态 ----------
            Dim status As MilpStatus
            Dim message As String = stopMessage

            If bestX Is Nothing Then
                If stopStatus = MilpStatus.TimeLimit OrElse stopStatus = MilpStatus.NodeLimit Then
                    status = stopStatus
                    If message.StringEmpty Then message = "在找到整数可行解之前达到终止条件。"
                ElseIf stopStatus = MilpStatus.Unbounded Then
                    status = MilpStatus.Unbounded
                Else
                    status = MilpStatus.Infeasible
                    If message.StringEmpty Then message = "搜索树遍历完毕，不存在整数可行解。"
                End If
            Else
                status = stopStatus

                If status = MilpStatus.Infeasible OrElse status = MilpStatus.Error Then status = MilpStatus.Optimal
            End If

            Return Finish(status, message)
        End Function

        ' ====================================================================
        ' 根节点割平面
        ' ====================================================================

        Private Function ApplyRootCuts(res As BsResult) As BsResult
            Dim current As BsResult = res

            For round As Integer = 1 To options.RootCutRounds
                Dim cuts As List(Of CutRow) = GomoryCut.Generate(
                    form, simplex, current, options, options.MaxCutsPerRound,
                    If(options.Verbose, log, Nothing))

                If cuts Is Nothing OrElse cuts.Count = 0 Then Exit For

                Dim oldRows As Integer = form.Rows
                Dim newSlacks As New List(Of Integer)()

                For Each cut As CutRow In cuts
                    newSlacks.Add(form.AddCutRowWork(cut.Coefficients, cut.Op, cut.Rhs))
                    cutsAdded += 1
                Next

                ' ---- 重建单纯形，并把新松弛列作为新行的基变量 ----
                simplex = New BoundedSimplex(form.A, form.b, form.c, form.l, form.u, tol, tol)

                Dim newBasis(form.Rows - 1) As Integer

                Array.Copy(current.Basis, newBasis, current.Basis.Length)

                For t As Integer = 0 To newSlacks.Count - 1
                    newBasis(oldRows + t) = newSlacks(t)
                Next

                Dim newAtUpper(form.Cols - 1) As Boolean

                Array.Copy(current.AtUpper, newAtUpper, current.AtUpper.Length)

                Dim r As BsResult = simplex.Solve(newBasis, newAtUpper, options.LpIterationLimit)
                lpSolves += 1

                If r.Status = BsStatus.Infeasible Then Return r
                If Not r.IsOptimal Then Return r

                current = r
                log.Add($"  割平面第 {round} 轮: 新增 {cuts.Count} 条（累计 {cutsAdded}），" &
                        $"根目标 {form.ToOriginalObjective(r.X):G8}")
            Next

            Return current
        End Function

        ' ====================================================================
        ' incumbent / 界 / 隙
        ' ====================================================================

        Private Sub RecordIncumbent(xWork As Double())
            Dim internalObj As Double = form.InternalObjective(xWork)

            If internalObj >= bestInternal - options.AbsoluteGap Then Return

            If Not MilpHeuristics.Feasible(form, xWork, std.Max(tol * 10.0, 1.0E-07)) Then Return

            bestInternal = internalObj
            bestX = form.ToOriginalSolution(xWork)

            If nodesExplored = 0 Then incumbentAtRoot = True

            If options.Verbose Then
                log.Add($"  新 incumbent: {BestOriginalValue():G8}（已探索节点 {nodesExplored}）")
            End If
        End Sub

        Private Sub TryHeuristics(r As BsResult)
            Dim x = MilpHeuristics.Rounding(form, r.X, options)

            If x IsNot Nothing Then
                heuristicsHits += 1
                RecordIncumbent(x)
                Return
            End If

            Dim d = MilpHeuristics.Diving(form, options, r.Basis, r.AtUpper, lpSolves)

            If d IsNot Nothing Then
                heuristicsHits += 1
                RecordIncumbent(d)
            End If
        End Sub

        Private Function BestOriginalValue() As Double
            Return form.ObjOffset + form.Sigma * bestInternal
        End Function

        Private Function GlobalBoundInternal() As Double
            Dim g As Double = Double.PositiveInfinity

            For Each n As Node In open
                If n.Bound < g Then g = n.Bound
            Next

            If Double.IsPositiveInfinity(g) Then
                If bestX Is Nothing Then Return Double.NaN
                Return bestInternal
            End If

            Return g
        End Function

        Private Function RelativeGap() As Double
            If bestX Is Nothing Then Return Double.PositiveInfinity

            Dim gb As Double = GlobalBoundInternal()

            If Double.IsNaN(gb) Then Return Double.PositiveInfinity

            Dim incumbentOrig As Double = BestOriginalValue()
            Dim boundOrig As Double = form.ObjOffset + form.Sigma * gb
            Dim denom As Double = std.Max(1.0, std.Abs(incumbentOrig))

            Return std.Abs(incumbentOrig - boundOrig) / denom
        End Function

        ' ====================================================================
        ' 开集与分支
        ' ====================================================================

        Private Function TakeId() As Integer
            Dim id As Integer = nextId
            nextId += 1
            Return id
        End Function

        Private Sub Push(node As Node)
            open.Add(node)
        End Sub

        Private Function Pop() As Node
            Dim pick As Integer = 0

            If options.Node = NodeRule.DepthFirst Then
                For i As Integer = 1 To open.Count - 1
                    If open(i).Depth > open(pick).Depth OrElse
                       (open(i).Depth = open(pick).Depth AndAlso open(i).Bound < open(pick).Bound) Then

                        pick = i
                    End If
                Next
            Else
                For i As Integer = 1 To open.Count - 1
                    If open(i).Bound < open(pick).Bound Then pick = i
                Next
            End If

            Dim n As Node = open(pick)
            open.RemoveAt(pick)
            Return n
        End Function

        ''' <summary>
        ''' 按 [newLower, newUpper]（原始变量界）生成子节点；若界未收紧则返回 False。
        ''' </summary>
        Private Function PushChild(parent As Node, workCol As Integer, varIndex As Integer,
                                   newLower As Double, newUpper As Double, branchUp As Boolean,
                                   parentBound As Double, parentRes As BsResult) As Boolean

            Dim wb = form.WorkBoundsFor(varIndex, newLower, newUpper)

            If Double.IsNaN(wb.lo) OrElse Double.IsNaN(wb.hi) Then Return False
            If wb.lo > wb.hi + 1.0E-09 Then Return False

            If wb.lo <= parent.L(workCol) + 1.0E-12 AndAlso wb.hi >= parent.U(workCol) - 1.0E-12 Then
                Return False     ' 界没有收紧
            End If

            Dim nl As Double() = CType(parent.L.Clone(), Double())
            Dim nu As Double() = CType(parent.U.Clone(), Double())

            nl(workCol) = wb.lo
            nu(workCol) = wb.hi

            Push(New Node With {
                .Bound = parentBound,
                .L = nl,
                .U = nu,
                .Basis = parentRes.Basis,
                .AtUpper = parentRes.AtUpper,
                .Depth = parent.Depth + 1,
                .Id = TakeId(),
                .BranchVar = varIndex,
                .BranchUp = branchUp,
                .ParentBound = parentBound
            })

            Return True
        End Function

        ' ====================================================================
        ' 分支变量选择（most-fractional / first-fractional / 伪成本）
        ' ====================================================================

        Private Function PickBranchColumn(r As BsResult) As Integer
            Select Case options.Branch
                Case BranchRule.FirstFractional
                    Return MilpHeuristics.FirstFractionalWorkColumn(form, r.X, options.IntegerTolerance)

                Case BranchRule.PseudoCost
                    Return PickByPseudoCost(r)

                Case Else
                    Return MilpHeuristics.PickFractionalWorkColumn(form, r.X, options.IntegerTolerance)
            End Select
        End Function

        ''' <summary>
        ''' 伪成本分支：score = min(Δdown·pcDown, Δup·pcUp)，取 score 最大者
        ''' （两方向都"贵"的变量优先分支；pc 初值为 1，随观测更新）。
        ''' </summary>
        Private Function PickByPseudoCost(r As BsResult) As Integer
            Dim bestK As Integer = -1
            Dim bestScore As Double = Double.NegativeInfinity

            For Each j As Integer In form.IntegerVariables
                Dim cols As Integer() = form.VariableColumns(j)

                If cols.Length <> 1 Then Continue For

                Dim k As Integer = cols(0)
                Dim v As Double = form.ColumnShift(k) + form.ColumnSign(k) * r.X(k)
                Dim frac As Double = v - std.Floor(v)

                If frac <= options.IntegerTolerance OrElse frac >= 1.0 - options.IntegerTolerance Then Continue For

                Dim downDist As Double = frac
                Dim upDist As Double = 1.0 - frac
                Dim pcD As Double = pcDown(j) / std.Max(1, pcDownCount(j))
                Dim pcU As Double = pcUp(j) / std.Max(1, pcUpCount(j))
                Dim score As Double = std.Min(downDist * pcD, upDist * pcU)

                If score > bestScore Then
                    bestScore = score
                    bestK = k
                End If
            Next

            If bestK < 0 Then
                Return MilpHeuristics.PickFractionalWorkColumn(form, r.X, options.IntegerTolerance)
            End If

            Return bestK
        End Function

        ''' <summary>用子节点实际界退化更新伪成本统计。</summary>
        Private Sub UpdatePseudoCost(node As Node, nodeBound As Double)
            If node.BranchVar < 0 Then Return

            Dim j As Integer = node.BranchVar

            If j < 0 OrElse j >= pcDown.Length Then Return

            Dim delta As Double = nodeBound - node.ParentBound

            If delta < 0.0 OrElse Double.IsNaN(delta) Then delta = 0.0

            If node.BranchUp Then
                pcUp(j) += delta
                pcUpCount(j) += 1
            Else
                pcDown(j) += delta
                pcDownCount(j) += 1
            End If
        End Sub

        ' ====================================================================
        ' 无约束特例与结果封装
        ' ====================================================================

        ''' <summary>无约束：每个变量独立地取使目标最优的界。</summary>
        Private Function SolveUnconstrained() As Double()
            Dim x(form.Cols - 1) As Double

            For k As Integer = 0 To form.Cols - 1
                Dim ck As Double = form.c(k)
                Dim lo As Double = form.l(k)
                Dim hi As Double = form.u(k)

                If ck >= 0.0 Then
                    x(k) = If(Double.IsNegativeInfinity(lo), 0.0, lo)
                Else
                    x(k) = If(Double.IsPositiveInfinity(hi), If(Double.IsNegativeInfinity(lo), 0.0, lo), hi)
                End If
            Next

            Return x
        End Function

        Private Function Finish(status As MilpStatus, message As String) As MilpSolution
            ' 有子树因数值失败被丢弃时，不能宣称"已证明最优"
            If droppedByNumeric > 0 AndAlso status = MilpStatus.Optimal Then
                status = MilpStatus.Error
                message = $"{droppedByNumeric} 个节点因 LP 数值失败被丢弃，最优性未被证明（返回当前最优可行解）。"
            End If

            Dim objVal As Double = Double.NaN
            Dim bound As Double = Double.NaN
            Dim gap As Double = Double.PositiveInfinity

            If bestX IsNot Nothing Then
                objVal = BestOriginalValue()
                Dim gb As Double = GlobalBoundInternal()

                If Not Double.IsNaN(gb) Then
                    bound = form.ObjOffset + form.Sigma * gb
                    gap = RelativeGap()
                Else
                    bound = objVal
                    gap = 0.0
                End If
            Else
                Dim gb As Double = GlobalBoundInternal()

                If Not Double.IsNaN(gb) Then
                    bound = form.ObjOffset + form.Sigma * gb
                End If
            End If

            Dim sol As New MilpSolution(status, bestX, form.VariableNames, objVal, bound, gap)

            sol.NodesExplored = nodesExplored
            sol.LpSolves = lpSolves
            sol.CutsAdded = cutsAdded
            sol.HeuristicSolutions = heuristicsHits
            sol.DroppedNodes = droppedByNumeric
            sol.RootRelaxation = rootRelax
            sol.IncumbentAtRoot = incumbentAtRoot
            sol.ElapsedMilliseconds = watch.ElapsedMilliseconds
            sol.Log = String.Join(ControlChars.Lf, log)
            sol.FailureMessage = If(message, "")

            Return sol
        End Function

    End Class

End Namespace

