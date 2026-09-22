' ============================================================================
' MilpHeuristics.vb — 整数原启发式（舍入 + 潜水 diving）
' ----------------------------------------------------------------------------
' 目的：在搜索树中尽早获得整数可行的 incumbent，从而大幅剪枝。
'
'   · Rounding：把 LP 最优解中的整数变量直接舍入到最近整数（并夹回界内），
'     再验证可行性；命中率低但代价 O(mn)，适合作为"零成本尝试"。
'   · Diving：逐步把分数整数变量固定到一个整数界并热启动重解 LP，直到：
'       - 所有整数变量取整（成功，返回可行解）；
'       - 子问题不可行（换另一个方向再试，仍失败则放弃）；
'       - 达到深度上限（放弃）。
'     每次重解都复用上一个基（对偶单纯形热启动），代价可控。
'
' 所有返回值都定义在"工作变量空间"（长度 = MilpLpForm.Cols），由调用方通过
' form.ToOriginalSolution 映射回原始变量；这样调用方可以统一计算目标值。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>整数原启发式。</summary>
    Public Module MilpHeuristics

        ''' <summary>
        ''' 挑选最分数（most fractional）的整数变量对应的工作列；没有则返回 −1。
        ''' </summary>
        Public Function PickFractionalWorkColumn(form As MilpLpForm, x As Double(), tol As Double) As Integer
            Dim best As Integer = -1
            Dim bestFrac As Double = tol

            For Each j As Integer In form.IntegerVariables
                Dim cols As Integer() = form.VariableColumns(j)

                If cols.Length <> 1 Then Continue For

                Dim k As Integer = cols(0)
                Dim v As Double = form.ColumnShift(k) + form.ColumnSign(k) * x(k)
                Dim frac As Double = v - std.Floor(v)

                If frac <= tol OrElse frac >= 1.0 - tol Then Continue For

                Dim score As Double = std.Min(frac, 1.0 - frac)

                If score > bestFrac Then
                    bestFrac = score
                    best = k
                End If
            Next

            Return best
        End Function

        ''' <summary>
        ''' 按变量索引顺序挑选第一个分数整数变量对应的工作列；没有则返回 −1。
        ''' </summary>
        Public Function FirstFractionalWorkColumn(form As MilpLpForm, x As Double(), tol As Double) As Integer
            For Each j As Integer In form.IntegerVariables
                Dim cols As Integer() = form.VariableColumns(j)

                If cols.Length <> 1 Then Continue For

                Dim k As Integer = cols(0)
                Dim v As Double = form.ColumnShift(k) + form.ColumnSign(k) * x(k)
                Dim frac As Double = v - std.Floor(v)

                If frac > tol AndAlso frac < 1.0 - tol Then Return k
            Next

            Return -1
        End Function

        ''' <summary>
        ''' 检查工作解是否满足全部等式约束与变量界（整数性由调用方保证）。
        ''' </summary>
        Public Function Feasible(form As MilpLpForm, x As Double(), tol As Double) As Boolean
            For i As Integer = 0 To form.Rows - 1
                Dim s As Double = 0.0

                For k As Integer = 0 To form.Cols - 1
                    If form.A(i, k) <> 0.0 Then s += form.A(i, k) * x(k)
                Next

                If std.Abs(s - form.b(i)) > tol * (1.0 + std.Abs(form.b(i))) Then Return False
            Next

            For k As Integer = 0 To form.Cols - 1
                If x(k) < form.l(k) - tol * (1.0 + std.Abs(form.l(k))) Then Return False
                If x(k) > form.u(k) + tol * (1.0 + std.Abs(form.u(k))) Then Return False
            Next

            Return True
        End Function

        ''' <summary>
        ''' 舍入启发式：把整数变量舍入到最近整数后验证可行性。
        ''' </summary>
        ''' <returns>工作空间可行解；不成功返回 Nothing</returns>
        Public Function Rounding(form As MilpLpForm, xLP As Double(), options As MilpOptions) As Double()
            If xLP Is Nothing Then Return Nothing

            Dim x As Double() = CType(xLP.Clone(), Double())

            For Each j As Integer In form.IntegerVariables
                Dim cols As Integer() = form.VariableColumns(j)

                If cols.Length <> 1 Then Continue For

                Dim k As Integer = cols(0)
                Dim v As Double = form.ColumnShift(k) + form.ColumnSign(k) * x(k)
                Dim rounded As Double = std.Floor(v + 0.5)

                ' 回到工作空间并夹回界内
                Dim wv As Double = form.ColumnSign(k) * (rounded - form.ColumnShift(k))

                If wv < form.l(k) Then wv = form.l(k)
                If wv > form.u(k) Then wv = form.u(k)

                x(k) = wv
            Next

            If Feasible(form, x, options.FeasibilityTolerance) Then Return x

            Return Nothing
        End Function

        ''' <summary>
        ''' 潜水（diving）启发式：逐个固定分数整数变量并热启动重解 LP。
        ''' </summary>
        ''' <param name="form">节点 LP 工作形式</param>
        ''' <param name="options">求解选项</param>
        ''' <param name="startBasis">起始热启动基</param>
        ''' <param name="startAtUpper">起始非基本状态</param>
        ''' <param name="lpSolves">累计 LP 求解次数（引用传出）</param>
        ''' <returns>工作空间整数可行解；不成功返回 Nothing</returns>
        Public Function Diving(form As MilpLpForm, options As MilpOptions,
                               startBasis As Integer(), startAtUpper As Boolean(),
                               ByRef lpSolves As Integer) As Double()

            Dim l As Double() = CType(form.l.Clone(), Double())
            Dim u As Double() = CType(form.u.Clone(), Double())
            Dim simplex As New BoundedSimplex(form.A, form.b, form.c, l, u,
                                             options.FeasibilityTolerance,
                                             options.FeasibilityTolerance)

            Dim res As BsResult = simplex.Solve(startBasis, startAtUpper, options.LpIterationLimit)

            lpSolves += 1

            If res.Status = BsStatus.Unbounded Then Return Nothing
            If Not res.IsOptimal Then Return Nothing

            Dim depth As Integer = 0

            Do
                Dim k As Integer = PickFractionalWorkColumn(form, res.X, options.IntegerTolerance)

                If k < 0 Then
                    ' 尝试在所有整数列上做一次净化（消除 1e-9 级别的数值残留）
                    Dim x = CType(res.X.Clone(), Double())

                    For Each j As Integer In form.IntegerVariables
                        Dim cols As Integer() = form.VariableColumns(j)

                        If cols.Length <> 1 Then Continue For

                        Dim kk As Integer = cols(0)

                        If u(kk) - l(kk) <= 1.0E-09 Then Continue For

                        Dim vv As Double = form.ColumnShift(kk) + form.ColumnSign(kk) * x(kk)

                        If std.Abs(vv - std.Floor(vv + 0.5)) <= options.IntegerTolerance * 10.0 Then
                            Dim target As Double = std.Floor(vv + 0.5)
                            Dim wv As Double = form.ColumnSign(kk) * (target - form.ColumnShift(kk))

                            If wv >= l(kk) - 1.0E-06 AndAlso wv <= u(kk) + 1.0E-06 Then
                                x(kk) = std.Min(u(kk), std.Max(l(kk), wv))
                            End If
                        End If
                    Next

                    If Feasible(form, x, options.FeasibilityTolerance * 10.0) Then Return x

                    Return Nothing
                End If

                If depth >= options.DivingDepthLimit Then Return Nothing

                ' 当前值与其上下取整
                Dim xcur As Double = res.X(k)
                Dim objfrac As Double = xcur - std.Floor(xcur)
                Dim down As Double = std.Floor(xcur)
                Dim up As Double = std.Ceiling(xcur)

                ' 优先取离当前值更近的一侧（经典 diving 方向启发）
                Dim order As Double() = If(objfrac <= 0.5, New Double() {down, up}, New Double() {up, down})

                Dim savedL As Double = l(k)
                Dim savedU As Double = u(k)
                Dim ok As Boolean = False

                For Each target As Double In order
                    ' 固定为整数：工作列界直接取整（整数变量的 shift 为整数、sign 为 +1）
                    Dim targetWork As Double = form.ColumnSign(k) * (target - form.ColumnShift(k))

                    If targetWork < savedL - 1.0E-09 OrElse targetWork > savedU + 1.0E-09 Then Continue For

                    l(k) = targetWork
                    u(k) = targetWork

                    Dim r As BsResult = simplex.Solve(res.Basis, res.AtUpper, options.LpIterationLimit)

                    lpSolves += 1

                    If r.IsOptimal Then
                        res = r
                        ok = True
                        Exit For
                    End If

                    If r.Status = BsStatus.Unbounded Then
                        ' 该方向无界，换方向
                        Continue For
                    End If
                Next

                If Not ok Then
                    l(k) = savedL
                    u(k) = savedU
                    Return Nothing
                End If

                depth += 1
            Loop
        End Function

    End Module

End Namespace
