' ============================================================================
' MilpPresolve.vb — 预处理 + 节点 LP 工作形式（bounded-variable work form）
' ----------------------------------------------------------------------------
' 一、预处理（MilpPresolve.Run）
'   1. 二进制变量界规范化到 [0,1]，并保证整数变量具有有限下界；
'   2. 固定变量消元：lb ≈ ub 的变量从约束矩阵中消除（列删除），其目标贡献
'      并入 ObjOffset，变量值记录在 FixedValue 中；
'   3. 约束活动度传播（activity-based bound tightening）若干轮：由
'      min/max activity 收紧每个变量界，整数变量按 ceil/floor 取整；同时
'      给出"不可行"的确定性判据（minAct > rhs / maxAct < rhs）；
'   4. 空行判定：0 ≤ rhs 恒真（冗余）或恒假（不可行）。
'
' 二、工作形式（MilpLpForm）
'   内部 LP：min c_workᵀ x_work,  A_work x_work = b_work,  l ≤ x_work ≤ u
'   · 每个工作列由 (原始变量, 符号 sign, 平移 shift) 描述：
'         x_orig = shift + sign · x_work
'     常规变量 sign=+1, shift=0；l=−∞ 的变量翻转（sign=−1, shift=u）；
'     双侧无限的自由变量拆为两个非负列（sign=+1 / −1）。
'   · ≤ 行追加 +1 松弛列，≥ 行追加 −1 剩余列（界 [0,+∞)），= 行不追加；
'   · 目标方向：Sigma = +1（min）/ −1（max），c_work = Sigma · c_orig · sign，
'     于是原始目标 = ObjOffset + Sigma · (c_workᵀ x_work)；
'   · 行均衡：每个约束行按 max|a_ij| 归一到 O(1) 量级（行缩放不改变可行域）。
'
' 【设计取舍】分支定界只修改工作列的 l/u（矩阵 A/b/c 保持不变），因此可以
' 复用同一个基进行热启动；割平面只追加行与新松弛列。这两点是"中等规模 +
' 节点热启动"定位的实现基础。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports std = System.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>
    ''' 预处理统计。
    ''' </summary>
    Public Class PresolveStats

        Public Property FixedVariables As Integer = 0
        Public Property TightenedBounds As Integer = 0
        Public Property RedundantRows As Integer = 0
        Public Property PropagationRounds As Integer = 0
        Public Property IsInfeasible As Boolean = False
        Public Property InfeasibleReason As String = ""

        Public Overrides Function ToString() As String
            Return $"固定变量 {FixedVariables}，收紧界 {TightenedBounds}，冗余行 {RedundantRows}，传播轮数 {PropagationRounds}"
        End Function

    End Class

    ''' <summary>
    ''' MILP 预处理结果 + 节点 LP 工作形式（含全部"工作列 → 原始变量"映射）。
    ''' </summary>
    Public Class MilpLpForm

        ' ---------- 工作 LP ----------
        Public Property A As Double(,)
        Public Property b As Double()
        ''' <summary>内部 min 方向目标（已乘 Sigma 与列符号）</summary>
        Public Property c As Double()
        Public Property l As Double()
        Public Property u As Double()
        Public Property Rows As Integer
        Public Property Cols As Integer

        ' ---------- 映射（对外只读） ----------
        Public ReadOnly Property OriginalVariableCount As Integer

        Friend _colOrig As Integer()
        Friend _colSign As Double()
        Friend _colShift As Double()
        Friend _colNames As String()
        Friend _colTypes As MilpVarType()
        Friend _slackCol As Integer()
        Friend _varCols As Integer()()
        Friend _varShift As Double()
        Friend _fixed As Boolean()
        Friend _fixedValue As Double()
        Friend _names As String()
        Friend _types As MilpVarType()
        Friend _integers As New List(Of Integer)()

        ''' <summary>工作列 → 原始变量索引；松弛/剩余列为 −1</summary>
        Public ReadOnly Property ColumnOriginal As Integer()
            Get
                Return _colOrig
            End Get
        End Property

        ''' <summary>工作列符号（±1）</summary>
        Public ReadOnly Property ColumnSign As Double()
            Get
                Return _colSign
            End Get
        End Property

        ''' <summary>工作列平移</summary>
        Public ReadOnly Property ColumnShift As Double()
            Get
                Return _colShift
            End Get
        End Property

        ''' <summary>工作列名称</summary>
        Public ReadOnly Property ColumnNames As String()
            Get
                Return _colNames
            End Get
        End Property

        ''' <summary>工作列类型（松弛列一律连续）</summary>
        Public ReadOnly Property ColumnTypes As MilpVarType()
            Get
                Return _colTypes
            End Get
        End Property

        ''' <summary>约束行对应的松弛列索引；无松弛行为 −1</summary>
        Public ReadOnly Property SlackColumn As Integer()
            Get
                Return _slackCol
            End Get
        End Property

        ''' <summary>原始变量 → 组成它的工作列（自由变量为两列，固定变量为空）</summary>
        Public ReadOnly Property VariableColumns As Integer()()
            Get
                Return _varCols
            End Get
        End Property

        ''' <summary>原始变量的总平移量（固定变量为固定值；翻转变量为 u；自由变量为 0）</summary>
        Public ReadOnly Property VariableShift As Double()
            Get
                Return _varShift
            End Get
        End Property

        ''' <summary>是否为固定变量</summary>
        Public ReadOnly Property IsFixed As Boolean()
            Get
                Return _fixed
            End Get
        End Property

        ''' <summary>固定变量的取值</summary>
        Public ReadOnly Property FixedValue As Double()
            Get
                Return _fixedValue
            End Get
        End Property

        ''' <summary>原始变量名</summary>
        Public ReadOnly Property VariableNames As String()
            Get
                Return _names
            End Get
        End Property

        ''' <summary>原始变量类型</summary>
        Public ReadOnly Property VariableTypes As MilpVarType()
            Get
                Return _types
            End Get
        End Property

        ''' <summary>整数（含二进制）原始变量索引</summary>
        Public ReadOnly Property IntegerVariables As List(Of Integer)
            Get
                Return _integers
            End Get
        End Property

        ''' <summary>目标方向：+1 = min，−1 = max（c_work 已乘该系数）</summary>
        Public Property Sigma As Double

        ''' <summary>平移/固定变量引入的原始方向目标常数</summary>
        Public Property ObjOffset As Double = 0.0

        ''' <summary>约束行方向（"&lt;=" / "&gt;=" / "="）；割平面追加行为 "&lt;=" / "&gt;="</summary>
        Public ReadOnly Property RowTypes As New List(Of String)()
        ''' <summary>约束行右端项（原始尺度）</summary>
        Public ReadOnly Property RowRhs As New List(Of Double)()

        Friend _stats As PresolveStats

        Public ReadOnly Property Stats As PresolveStats
            Get
                Return _stats
            End Get
        End Property

        Friend Sub New(originalCount As Integer)
            OriginalVariableCount = originalCount
            _stats = New PresolveStats()
            _integers = New List(Of Integer)()
        End Sub

        ' ====================================================================
        ' 原始空间 ↔ 工作空间
        ' ====================================================================

        ''' <summary>把工作解转换为原始变量的解向量。</summary>
        Public Function ToOriginalSolution(xWork As Double()) As Double()
            Dim x(OriginalVariableCount - 1) As Double

            For j As Integer = 0 To OriginalVariableCount - 1
                If _fixed(j) Then
                    x(j) = _fixedValue(j)
                    Continue For
                End If

                Dim s As Double = 0.0

                For Each k As Integer In _varCols(j)
                    s += _colShift(k) + _colSign(k) * xWork(k)
                Next

                x(j) = s
            Next

            Return x
        End Function

        ''' <summary>原始方向目标值 = ObjOffset + Sigma · c_workᵀx_work。</summary>
        Public Function ToOriginalObjective(xWork As Double()) As Double
            Return ObjOffset + Sigma * InternalObjective(xWork)
        End Function

        ''' <summary>工作列绑定的原始变量是否为整数（松弛/剩余列为 False）。</summary>
        Public Function IsIntegerColumn(k As Integer) As Boolean
            Dim j As Integer = _colOrig(k)

            If j < 0 Then Return False

            Return _types(j) = MilpVarType.GeneralInteger OrElse _types(j) = MilpVarType.Binary
        End Function

        ''' <summary>内部 min 方向目标值。</summary>
        Public Function InternalObjective(xWork As Double()) As Double
            Dim s As Double = 0.0

            For k As Integer = 0 To Cols - 1
                s += c(k) * xWork(k)
            Next

            Return s
        End Function

        ' ====================================================================
        ' 分支：修改原始变量界（只动工作列 l/u，矩阵不变 → 可热启动）
        ' ====================================================================

        ''' <summary>
        ''' 按原始变量界更新对应工作列的界（仅支持单列变量；整数变量在预处理
        ''' 后必然单列，因为整数变量被要求具有有限下界）。
        ''' </summary>
        Public Sub SetOriginalBounds(j As Integer, lower As Double, upper As Double)
            If _fixed(j) Then Return

            Dim w = WorkBoundsFor(j, lower, upper)

            l(_varCols(j)(0)) = w.lo
            u(_varCols(j)(0)) = w.hi
        End Sub

        ''' <summary>
        ''' 计算"原始变量界 [lower, upper]"对应的工作列界（不修改对象状态）。
        ''' </summary>
        Public Function WorkBoundsFor(j As Integer, lower As Double, upper As Double) As (lo As Double, hi As Double)
            Dim cols As Integer() = _varCols(j)

            If cols.Length <> 1 Then
                Throw New NotSupportedException(
                    $"变量 {_names(j)} 由 {cols.Length} 个工作列表示（自由变量拆分），不支持分支改界。")
            End If

            Dim k As Integer = cols(0)
            Dim sign As Double = _colSign(k)
            Dim shift As Double = _colShift(k)

            ' x_orig = shift + sign · x_k  ⇒  x_k = sign · (x_orig − shift)
            Dim v1 As Double = sign * (lower - shift)
            Dim v2 As Double = sign * (upper - shift)

            If sign > 0 Then
                Return (v1, v2)
            Else
                Return (v2, v1)
            End If
        End Function

        ''' <summary>
        ''' 由工作列界反推原始变量的当前界（不修改对象状态）。
        ''' </summary>
        Public Function GetOriginalBounds(j As Integer, lArr As Double(), uArr As Double()) As (lo As Double, hi As Double)
            If _fixed(j) Then Return (_fixedValue(j), _fixedValue(j))

            Dim cols As Integer() = _varCols(j)

            If cols.Length <> 1 Then
                ' 自由变量：x_orig = x⁺ − x⁻，界分别为 [0,∞)
                Return (Double.NegativeInfinity, Double.PositiveInfinity)
            End If

            Dim k As Integer = cols(0)
            Dim sign As Double = _colSign(k)
            Dim shift As Double = _colShift(k)

            Dim a As Double = shift + sign * lArr(k)
            Dim b As Double = shift + sign * uArr(k)

            If a > b Then
                Return (b, a)
            Else
                Return (a, b)
            End If
        End Function

        ' ====================================================================
        ' 割平面：追加一行（重建稠密矩阵，追加新松弛列）
        ' ====================================================================

        ''' <summary>
        ''' 追加一条割平面约束（系数直接定义在工作变量空间）。
        ''' </summary>
        ''' <param name="gamma">长度 = 工作列数 <see cref="Cols"/> 的行系数</param>
        ''' <param name="op">"&lt;=" 或 "&gt;="</param>
        ''' <param name="rhs">右端项（工作变量尺度）</param>
        ''' <returns>新松弛列的索引</returns>
        Public Function AddCutRowWork(gamma As Double(), op As String, rhs As Double) As Integer
            Dim newRows As Integer = Rows + 1
            Dim newCols As Integer = Cols + 1
            Dim A2(newRows - 1, newCols - 1) As Double

            For i As Integer = 0 To Rows - 1
                For k As Integer = 0 To Cols - 1
                    A2(i, k) = A(i, k)
                Next
            Next

            Dim b2(newRows - 1) As Double
            Array.Copy(b, b2, Rows)

            ' gamma 可能只覆盖"追加本割之前"的列空间（同一轮追加多条割时），
            ' 缺失的新松弛列系数视为 0。
            For k As Integer = 0 To Cols - 1
                If k < gamma.Length Then A2(Rows, k) = gamma(k)
            Next

            Dim slackIdx As Integer = Cols

            A2(Rows, slackIdx) = If(op = ">=", -1.0, 1.0)
            b2(Rows) = rhs

            ' ---- 行均衡 ----
            Dim scale As Double = 0.0

            For k As Integer = 0 To newCols - 1
                scale = std.Max(scale, std.Abs(A2(Rows, k)))
            Next

            If scale > 0.0 Then
                For k As Integer = 0 To newCols - 1
                    A2(Rows, k) /= scale
                Next

                b2(Rows) /= scale
            End If

            ' ---- 提交 ----
            A = A2
            b = b2
            Rows = newRows
            Cols = newCols

            ReDim Preserve c(newCols - 1)
            ReDim Preserve l(newCols - 1)
            ReDim Preserve u(newCols - 1)
            ReDim Preserve _colOrig(newCols - 1)
            ReDim Preserve _colSign(newCols - 1)
            ReDim Preserve _colShift(newCols - 1)
            ReDim Preserve _colNames(newCols - 1)
            ReDim Preserve _colTypes(newCols - 1)
            ReDim Preserve _slackCol(newRows - 1)

            c(slackIdx) = 0.0
            l(slackIdx) = 0.0
            u(slackIdx) = Double.PositiveInfinity
            _colOrig(slackIdx) = -1
            _colSign(slackIdx) = 1.0
            _colShift(slackIdx) = 0.0
            _colNames(slackIdx) = $"cut{Rows}slack"
            _colTypes(slackIdx) = MilpVarType.Continuous
            _slackCol(newRows - 1) = slackIdx

            RowTypes.Add(If(op = ">=", ">=", "<="))
            RowRhs.Add(rhs)

            Return slackIdx
        End Function

        Public Overrides Function ToString() As String
            Return $"MILP work form: {Rows} × {Cols}（原始变量 {OriginalVariableCount}，整数 {_integers.Count}）"
        End Function

    End Class

    ''' <summary>
    ''' 预处理与工作形式构造。
    ''' </summary>
    Public Module MilpPresolve

        Friend Const BOUND_TOL As Double = 0.000000001

        ''' <summary>
        ''' 执行预处理并构造节点 LP 工作形式。
        ''' </summary>
        ''' <remarks>
        ''' 当预处理器判定问题不可行时，返回对象的 <c>Stats.IsInfeasible</c> 为 True，
        ''' 且 <c>A</c> 等数组保持 Nothing；调用方应先检查该标志。
        ''' </remarks>
        Public Function Run(model As MilpModel, options As MilpOptions, log As List(Of String)) As MilpLpForm
            Dim n As Integer = model.Variables.Count
            Dim m As Integer = model.Constraints.Count
            Dim idx As Dictionary(Of String, Integer) = model.VariableIndex()

            Dim lb(n - 1) As Double
            Dim ub(n - 1) As Double
            Dim varTypes(n - 1) As String

            For j As Integer = 0 To n - 1
                Dim v As MilpVariable = model.Variables(j)

                lb(j) = v.LowerBound
                ub(j) = v.UpperBound
                varTypes(j) = If(v.IsInteger, "I", "C")

                If v.IsInteger AndAlso Double.IsNegativeInfinity(lb(j)) Then
                    Throw New ArgumentException(
                        $"整数变量 {v.symbol} 缺少有限下界，MILP 分支定界无法进行；请为其指定下界。")
                End If
            Next

            ' ---- 原始约束矩阵 ----
            Dim Aorg(m - 1, n - 1) As Double
            Dim ops(m - 1) As String
            Dim rhs(m - 1) As Double

            For i As Integer = 0 To m - 1
                Dim con As LppConstraint = model.Constraints(i)

                ops(i) = NormalizeOp(con.Op)
                rhs(i) = con.Rhs

                If con.Coefficients Is Nothing Then Continue For

                For Each kvp As KeyValuePair(Of String, Double) In con.Coefficients
                    Dim j As Integer = idx(kvp.Key)
                    Aorg(i, j) += kvp.Value
                Next
            Next

            Dim stats As New PresolveStats()
            Dim fixedValue(n - 1) As Double
            Dim isFixed(n - 1) As Boolean

            If options.EnablePresolve Then
                PresolveCore(model, Aorg, ops, rhs, lb, ub, varTypes, fixedValue, isFixed, stats)
            End If

            Dim form As New MilpLpForm(n)

            ' 基础元数据先就位，保证"不可行提前返回"等路径也能安全取到变量名/类型
            form._stats = stats
            form._names = model.Variables.Select(Function(v) v.symbol).ToArray()
            form._types = model.Variables.Select(Function(v) v.VarType).ToArray()
            form._fixed = isFixed
            form._fixedValue = fixedValue

            If stats.IsInfeasible Then Return form

            BuildShell(form, model, Aorg, ops, rhs, lb, ub, fixedValue, isFixed, varTypes, stats)

            If log IsNot Nothing Then
                log.Add($"  Presolve: {stats}")
            End If

            Return form
        End Function

        Private Function NormalizeOp(op As String) As String
            Dim t As String = If(op, "").Trim()

            Select Case t
                Case "≤", "<=" : Return "<="
                Case "≥", ">=" : Return ">="
                Case "=" : Return "="
                Case Else : Return t
            End Select
        End Function

        ''' <summary>
        ''' 预处理主体：固定变量消元 + 活动度界传播 + 冗余/不可行判定。
        ''' </summary>
        Private Sub PresolveCore(model As MilpModel, Aorg As Double(,), ops As String(), rhs As Double(),
                                 lb As Double(), ub As Double(), varTypes As String(),
                                 fixedValue As Double(), isFixed As Boolean(), stats As PresolveStats)

            ' ---------- 1. 固定变量消元 ----------
            For j As Integer = 0 To lb.Length - 1
                If lb(j) = ub(j) Then
                    FoldFixed(Aorg, rhs, j, lb(j), fixedValue, isFixed)
                    lb(j) = 0.0
                    ub(j) = 0.0
                    stats.FixedVariables += 1
                End If
            Next

            ' ---------- 2. 活动度界传播 ----------
            Dim rounds As Integer = 0

            While rounds < 8
                Dim changed As Boolean = False

                For i As Integer = 0 To rhs.Length - 1
                    Dim op As String = ops(i)
                    Dim minAct As Double = 0.0
                    Dim maxAct As Double = 0.0
                    Dim varCount As Integer = 0

                    For j As Integer = 0 To lb.Length - 1
                        Dim a As Double = Aorg(i, j)

                        If a = 0.0 Then Continue For

                        varCount += 1
                        AccumulateActivity(a, lb(j), ub(j), minAct, maxAct)
                    Next

                    If varCount = 0 Then
                        Dim bad As Boolean = (op = "<=" AndAlso 0.0 > rhs(i) + BOUND_TOL) OrElse
                                             (op = ">=" AndAlso 0.0 < rhs(i) - BOUND_TOL) OrElse
                                             (op = "=" AndAlso std.Abs(rhs(i)) > BOUND_TOL)

                        If bad Then
                            stats.IsInfeasible = True
                            stats.InfeasibleReason = $"约束 #{i + 1} 化简后为 0 {op} {rhs(i)}，不可行。"
                            Return
                        End If

                        stats.RedundantRows += 1
                        Continue For
                    End If

                    Select Case op
                        Case "<="
                            If minAct > rhs(i) + 1.0E-07 * (1.0 + std.Abs(rhs(i))) Then
                                MarkInfeasible(stats, $"约束 #{i + 1} 的最小活动度 {minAct} > rhs {rhs(i)}，不可行。")
                                Return
                            End If
                        Case ">="
                            If maxAct < rhs(i) - 1.0E-07 * (1.0 + std.Abs(rhs(i))) Then
                                MarkInfeasible(stats, $"约束 #{i + 1} 的最大活动度 {maxAct} < rhs {rhs(i)}，不可行。")
                                Return
                            End If
                        Case "="
                            If minAct > rhs(i) + 1.0E-07 * (1.0 + std.Abs(rhs(i))) OrElse
                               maxAct < rhs(i) - 1.0E-07 * (1.0 + std.Abs(rhs(i))) Then

                                MarkInfeasible(stats, $"等式约束 #{i + 1} 的活动度区间 [{minAct}, {maxAct}] 不含 {rhs(i)}，不可行。")
                                Return
                            End If
                    End Select

                    ' 逐个变量收紧界（"其它变量"的活动度单独求和，避免无穷界相减失真）
                    For j As Integer = 0 To lb.Length - 1
                        Dim a As Double = Aorg(i, j)

                        If a = 0.0 OrElse isFixed(j) Then Continue For

                        Dim minOther As Double = 0.0
                        Dim maxOther As Double = 0.0

                        For j2 As Integer = 0 To lb.Length - 1
                            If j2 = j Then Continue For

                            Dim a2 As Double = Aorg(i, j2)

                            If a2 = 0.0 Then Continue For

                            AccumulateActivity(a2, lb(j2), ub(j2), minOther, maxOther)
                        Next

                        Dim newLb As Double = lb(j)
                        Dim newUb As Double = ub(j)

                        If op = "<=" OrElse op = "=" Then
                            ' a·x_k ≤ rhs − minOther  ⇒  按 a 的符号定向收紧（务必除以 a）
                            Dim bound As Double = (rhs(i) - minOther) / a

                            If a > 0 Then
                                If bound < newUb Then newUb = bound
                            Else
                                If bound > newLb Then newLb = bound
                            End If
                        End If

                        If op = ">=" OrElse op = "=" Then
                            ' a·x_k ≥ rhs − maxOther  ⇒  按 a 的符号定向收紧（务必除以 a）
                            Dim bound As Double = (rhs(i) - maxOther) / a

                            If a > 0 Then
                                If bound > newLb Then newLb = bound
                            Else
                                If bound < newUb Then newUb = bound
                            End If
                        End If

                        If varTypes(j) = "I" Then
                            If Not Double.IsNegativeInfinity(newLb) Then newLb = std.Ceiling(newLb - BOUND_TOL)
                            If Not Double.IsPositiveInfinity(newUb) Then newUb = std.Floor(newUb + BOUND_TOL)
                        End If

                        If newLb > newUb + BOUND_TOL Then
                            MarkInfeasible(stats, $"变量 {model.Variables(j).symbol} 的界被收紧为 [{newLb}, {newUb}]，不可行。")
                            Return
                        End If

                        If newLb > lb(j) + BOUND_TOL OrElse newUb < ub(j) - BOUND_TOL Then
                            lb(j) = newLb
                            ub(j) = newUb
                            changed = True

                            If lb(j) = ub(j) Then
                                FoldFixed(Aorg, rhs, j, lb(j), fixedValue, isFixed)
                                lb(j) = 0.0
                                ub(j) = 0.0
                                stats.FixedVariables += 1
                            Else
                                stats.TightenedBounds += 1
                            End If
                        End If
                    Next
                Next

                rounds += 1

                If Not changed Then Exit While
            End While

            stats.PropagationRounds = rounds

            ' ---------- 3. 不变量：整数变量必须具有有限下界 ----------
            For j As Integer = 0 To lb.Length - 1
                If varTypes(j) = "I" AndAlso Double.IsNegativeInfinity(lb(j)) Then
                    MarkInfeasible(stats, $"整数变量 {model.Variables(j).symbol} 在预处理后仍无有限下界。")
                    Return
                End If
            Next
        End Sub

        Private Sub MarkInfeasible(stats As PresolveStats, reason As String)
            stats.IsInfeasible = True
            stats.InfeasibleReason = reason
        End Sub

        Private Sub FoldFixed(Aorg As Double(,), rhs As Double(), j As Integer, value As Double,
                              fixedValue As Double(), isFixed As Boolean())

            isFixed(j) = True
            fixedValue(j) = value

            For i As Integer = 0 To rhs.Length - 1
                If Aorg(i, j) <> 0.0 Then
                    rhs(i) -= Aorg(i, j) * value
                    Aorg(i, j) = 0.0
                End If
            Next
        End Sub

        Private Sub AccumulateActivity(a As Double, lb As Double, ub As Double,
                                       ByRef minAct As Double, ByRef maxAct As Double)

            Dim lo As Double = lb * a
            Dim hi As Double = ub * a

            If lo > hi Then
                Dim t As Double = lo : lo = hi : hi = t
            End If

            ' 无穷界需要显式传播，否则"减掉有限项"会得到虚假的有限活动度
            If Double.IsNegativeInfinity(lo) Then
                minAct = Double.NegativeInfinity
            ElseIf Not Double.IsNegativeInfinity(minAct) Then
                minAct += lo
            End If

            If Double.IsPositiveInfinity(hi) Then
                maxAct = Double.PositiveInfinity
            ElseIf Not Double.IsPositiveInfinity(maxAct) Then
                maxAct += hi
            End If
        End Sub

        ''' <summary>
        ''' 构造工作形式（含松弛列、自由变量规范化、目标方向归一与行均衡）。
        ''' </summary>
        Private Sub BuildShell(form As MilpLpForm, model As MilpModel,
                               Aorg As Double(,), ops As String(), rhs As Double(),
                               lb As Double(), ub As Double(),
                               fixedValue As Double(), isFixed As Boolean(), varTypes As String(),
                               stats As PresolveStats)

            Dim n As Integer = lb.Length
            Dim m As Integer = rhs.Length
            Dim sigma As Double = If(model.ObjectiveSense.ToLowerInvariant().StartsWith("max"), -1.0, 1.0)

            ' ============================================================
            ' 1. 分配工作列（两遍法：先定索引，再填属性）
            ' ============================================================
            Dim varCols(n - 1)() As Integer
            Dim colCount As Integer = 0

            For j As Integer = 0 To n - 1
                If isFixed(j) Then
                    varCols(j) = New Integer() {}
                    Continue For
                End If

                Dim lower As Double = lb(j)
                Dim upper As Double = ub(j)

                If Double.IsNegativeInfinity(lower) AndAlso Double.IsPositiveInfinity(upper) Then
                    varCols(j) = New Integer() {colCount, colCount + 1}
                    colCount += 2
                Else
                    varCols(j) = New Integer() {colCount}
                    colCount += 1
                End If
            Next

            Dim nSlack As Integer = ops.Count(Function(o) o <> "=")
            Dim cols As Integer = colCount + nSlack

            Dim colOrig(cols - 1) As Integer
            Dim colSign(cols - 1) As Double
            Dim colShift(cols - 1) As Double
            Dim colNames(cols - 1) As String
            Dim colTypes(cols - 1) As MilpVarType

            For j As Integer = 0 To n - 1
                If isFixed(j) Then Continue For

                Dim v As MilpVariable = model.Variables(j)
                Dim cc As Integer() = varCols(j)
                Dim lower As Double = lb(j)
                Dim upper As Double = ub(j)

                If cc.Length = 1 Then
                    Dim k As Integer = cc(0)

                    colOrig(k) = j
                    colNames(k) = v.symbol
                    colTypes(k) = v.VarType

                    If Double.IsNegativeInfinity(lower) Then
                        ' 翻转：y = upper − x ≥ 0
                        colSign(k) = -1.0
                        colShift(k) = upper
                    Else
                        colSign(k) = 1.0
                        colShift(k) = 0.0
                    End If
                Else
                    Dim kp As Integer = cc(0)
                    Dim km As Integer = cc(1)

                    colOrig(kp) = j : colSign(kp) = 1.0 : colShift(kp) = 0.0
                    colOrig(km) = j : colSign(km) = -1.0 : colShift(km) = 0.0
                    colNames(kp) = $"{v.symbol}+"
                    colNames(km) = $"{v.symbol}-"
                    colTypes(kp) = v.VarType
                    colTypes(km) = v.VarType
                End If
            Next

            ' 松弛列
            Dim slackCol(m - 1) As Integer
            Dim ptr As Integer = colCount

            For i As Integer = 0 To m - 1
                If ops(i) = "=" Then
                    slackCol(i) = -1
                    Continue For
                End If

                slackCol(i) = ptr
                colOrig(ptr) = -1
                colSign(ptr) = 1.0
                colShift(ptr) = 0.0
                colNames(ptr) = $"s{i + 1}"
                colTypes(ptr) = MilpVarType.Continuous
                ptr += 1
            Next

            ' ============================================================
            ' 2. 原始变量的总平移 + 目标常数
            ' ============================================================
            Dim varShift(n - 1) As Double
            Dim objOffset As Double = 0.0

            For j As Integer = 0 To n - 1
                Dim v As MilpVariable = model.Variables(j)

                If isFixed(j) Then
                    varShift(j) = fixedValue(j)
                Else
                    Dim s As Double = 0.0

                    For Each k As Integer In varCols(j)
                        s += colShift(k)
                    Next

                    varShift(j) = s
                End If

                objOffset += v.coefficient * varShift(j)
            Next

            ' ============================================================
            ' 3. 组装 A / b / c / l / u
            ' ============================================================
            ' 无约束（m = 0）时仍保留 1 行占位，避免 VB 负维度数组；Rows 仍记录为 0
            Dim A(std.Max(m, 1) - 1, cols - 1) As Double
            Dim bNew(std.Max(m, 1) - 1) As Double
            Dim cW(cols - 1) As Double
            Dim lW(cols - 1) As Double
            Dim uW(cols - 1) As Double

            ' 目标系数与界
            For k As Integer = 0 To cols - 1
                Dim j As Integer = colOrig(k)

                If j < 0 Then
                    cW(k) = 0.0
                    lW(k) = 0.0
                    uW(k) = Double.PositiveInfinity
                    Continue For
                End If

                Dim v As MilpVariable = model.Variables(j)

                cW(k) = sigma * v.coefficient * colSign(k)

                If colSign(k) < 0 Then
                    lW(k) = 0.0
                    uW(k) = Double.PositiveInfinity
                ElseIf Double.IsNegativeInfinity(lb(j)) Then
                    ' 自由变量的 + 分量
                    lW(k) = 0.0
                    uW(k) = Double.PositiveInfinity
                Else
                    lW(k) = lb(j)
                    uW(k) = ub(j)
                End If
            Next

            ' 约束行
            For i As Integer = 0 To m - 1
                Dim rhsShift As Double = 0.0

                For j As Integer = 0 To n - 1
                    Dim aij As Double = Aorg(i, j)

                    If aij = 0.0 Then Continue For

                    For Each k As Integer In varCols(j)
                        A(i, k) = aij * colSign(k)
                    Next

                    rhsShift += aij * varShift(j)
                Next

                If slackCol(i) >= 0 Then
                    A(i, slackCol(i)) = If(ops(i) = ">=", -1.0, 1.0)
                End If

                bNew(i) = rhs(i) - rhsShift
            Next

            ' ---- 行均衡（行缩放不改变可行域） ----
            For i As Integer = 0 To m - 1
                Dim scale As Double = 0.0

                For k As Integer = 0 To cols - 1
                    scale = std.Max(scale, std.Abs(A(i, k)))
                Next

                If scale > 0.0 AndAlso std.Abs(scale - 1.0) > 0.000000001 Then
                    For k As Integer = 0 To cols - 1
                        A(i, k) /= scale
                    Next

                    bNew(i) /= scale
                End If
            Next

            ' ============================================================
            ' 4. 提交
            ' ============================================================
            form.A = A
            form.b = bNew
            form.c = cW
            form.l = lW
            form.u = uW
            form.Rows = m
            form.Cols = cols
            form.Sigma = sigma
            form.ObjOffset = objOffset
            form._colOrig = colOrig
            form._colSign = colSign
            form._colShift = colShift
            form._colNames = colNames
            form._colTypes = colTypes
            form._slackCol = slackCol
            form._varCols = varCols
            form._varShift = varShift
            form._fixed = isFixed
            form._fixedValue = fixedValue

            form._names = model.Variables.Select(Function(v) v.symbol).ToArray()
            form._types = model.Variables.Select(Function(v) v.VarType).ToArray()

            For i As Integer = 0 To m - 1
                form.RowTypes.Add(ops(i))
                form.RowRhs.Add(rhs(i))
            Next

            Dim integers As New List(Of Integer)()

            For j As Integer = 0 To n - 1
                If model.Variables(j).IsInteger Then integers.Add(j)
            Next

            form._integers.Clear()
            form._integers.AddRange(integers)

            form._stats = stats
        End Sub

    End Module

End Namespace
