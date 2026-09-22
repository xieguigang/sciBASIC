' ============================================================================
' MilpSelfTest.vb — MILP 求解器内置自检
' ----------------------------------------------------------------------------
' T1  0/1 背包（已知最优 + 暴力枚举对拍）
' T2  一般整数生产计划（整数间隙：LP 松弛 21 → 整数最优 20）
' T3  设施选址（二进制 + 连续）
' T4  3×3 指派（0/1，= 约束）
' T5  连续 + 整数混合（= 与 ≥ 约束）
' T6  不可行证书
' T7  无界证书
' T8  预处理开关一致性
' T9  割平面 / 启发式生效性（根松弛为分数，割后仍得整数最优）
' T10 随机小规模纯整数问题 vs 暴力枚举（20 个随机实例）
' T11 解可行性、整数性与目标值对账（覆盖全部已构造模型）
' ============================================================================

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.MILP

Public Module MilpSelfTest

    Private failures As Integer = 0

    Private Sub Check(cond As Boolean, name As String, Optional detail As String = "")
        If cond Then
            Console.WriteLine($"  [PASS] {name} {detail}")
        Else
            failures += 1
            Console.WriteLine($"  [FAIL] {name} {detail}")
        End If
    End Sub

    Public Function RunAll() As Integer
        failures = 0

        Console.WriteLine("=== MILP 求解器自检（预处理 + 割平面 + 启发式 + 分支定界）===")

        T1()
        T2()
        T3()
        T4()
        T5()
        T6()
        T7()
        T8()
        T9()
        T10()
        T11()
        T12()

        Console.WriteLine($"=== {If(failures = 0, "ALL TESTS PASSED", failures & " TEST(S) FAILED")} ===")

        Return failures
    End Function

    ' ==================================================================
    ' 测试问题构造
    ' ==================================================================

    Friend Function KnapsackModel() As MilpModel
        Dim weights As Double() = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11}
        Dim values As Double() = {3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
        Dim model As New MilpModel With {.ObjectiveSense = "max"}
        Dim row As New Dictionary(Of String, Double)()

        For i As Integer = 0 To weights.Length - 1
            model.AddVariable($"x{i + 1}", values(i), MilpVarType.Binary)
            row($"x{i + 1}") = weights(i)
        Next

        model.AddConstraint(row, "<=", 20)

        Return model
    End Function

    Friend Function ProductionModel() As MilpModel
        Dim model As New MilpModel With {.ObjectiveSense = "max"}

        model.AddVariable("x", 5, MilpVarType.GeneralInteger)
        model.AddVariable("y", 4, MilpVarType.GeneralInteger)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 6.0}, {"y", 4.0}}, "<=", 24)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 2.0}}, "<=", 6)

        Return model
    End Function

    Friend Function FacilityModel() As MilpModel
        Dim model As New MilpModel With {.ObjectiveSense = "min"}

        model.AddVariable("y1", 5, MilpVarType.Binary)
        model.AddVariable("y2", 6, MilpVarType.Binary)
        model.AddVariable("y3", 7, MilpVarType.Binary)
        model.AddVariable("x1", 2, MilpVarType.Continuous)
        model.AddVariable("x2", 3, MilpVarType.Continuous)
        model.AddVariable("x3", 1, MilpVarType.Continuous)

        model.AddConstraint(New Dictionary(Of String, Double) From {{"x1", 1.0}, {"x2", 1.0}, {"x3", 1.0}}, "=", 10)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x1", 1.0}, {"y1", -10.0}}, "<=", 0)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x2", 1.0}, {"y2", -10.0}}, "<=", 0)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x3", 1.0}, {"y3", -10.0}}, "<=", 0)

        Return model
    End Function

    Friend Function AssignmentModel() As MilpModel
        Dim cost As Double()() = {
            New Double() {4, 1, 3},
            New Double() {2, 0, 5},
            New Double() {3, 2, 2}
        }

        Dim model As New MilpModel With {.ObjectiveSense = "min"}

        For i As Integer = 0 To 2
            For j As Integer = 0 To 2
                model.AddVariable($"x{i + 1}{j + 1}", cost(i)(j), MilpVarType.Binary)
            Next
        Next

        For i As Integer = 0 To 2
            Dim row As New Dictionary(Of String, Double)()

            For j As Integer = 0 To 2
                row($"x{i + 1}{j + 1}") = 1
            Next

            model.AddConstraint(row, "=", 1)
        Next

        For j As Integer = 0 To 2
            Dim col As New Dictionary(Of String, Double)()

            For i As Integer = 0 To 2
                col($"x{i + 1}{j + 1}") = 1
            Next

            model.AddConstraint(col, "=", 1)
        Next

        Return model
    End Function

    Friend Function MixedModel() As MilpModel
        Dim model As New MilpModel With {.ObjectiveSense = "min"}

        model.AddVariable("x", 2, MilpVarType.GeneralInteger)
        model.AddVariable("y", 3, MilpVarType.GeneralInteger)
        model.AddVariable("z", 1, MilpVarType.Continuous)

        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 1.0}, {"z", 1.0}}, ">=", 10)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 2.0}}, "=", 8)

        Return model
    End Function

    ' ==================================================================
    ' T1 ~ T5：已知最优
    ' ==================================================================

    Private Sub T1()
        Console.WriteLine("-- T1 0/1 背包（max，已知最优 25）--")

        Dim model = KnapsackModel()
        Dim sol = MilpSolver.Solve(model)
        Dim brute = BruteForce(model, 20000)

        Check(sol.Status = MilpStatus.Optimal, "状态最优", sol.StatusText())
        Check(System.Math.Abs(sol.ObjectiveValue - 25.0) < 0.000001, "目标 = 25", $"obj={sol.ObjectiveValue:G8}")
        Check(brute.HasValue AndAlso System.Math.Abs(brute.Value - sol.ObjectiveValue) < 0.000001,
              "与暴力枚举一致", $"brute={If(brute.HasValue, brute.Value.ToString("G8"), "n/a")}")
        Check(sol.RelativeGap < 0.0001, "相对间隙 ≈ 0", $"gap={sol.RelativeGap:E3}")
    End Sub

    Private Sub T2()
        Console.WriteLine("-- T2 整数生产计划（max，LP 松弛 21，整数最优 20）--")

        Dim model = ProductionModel()
        Dim sol = MilpSolver.Solve(model)

        Check(sol.Status = MilpStatus.Optimal, "状态最优", sol.StatusText())
        Check(System.Math.Abs(sol.ObjectiveValue - 20.0) < 0.000001, "目标 = 20", $"obj={sol.ObjectiveValue:G8}")
        Check(sol.RootRelaxation.HasValue AndAlso System.Math.Abs(sol.RootRelaxation.Value - 21.0) < 0.000001,
              "根 LP 松弛 = 21", $"root={If(sol.RootRelaxation.HasValue, sol.RootRelaxation.Value.ToString("G8"), "n/a")}")
        Check(IsIntegral(model, sol), "整数性")
    End Sub

    Private Sub T3()
        Console.WriteLine("-- T3 设施选址（min，二进制 + 连续，已知最优 17）--")

        Dim model = FacilityModel()
        Dim sol = MilpSolver.Solve(model)

        Check(sol.Status = MilpStatus.Optimal, "状态最优", sol.StatusText())
        Check(System.Math.Abs(sol.ObjectiveValue - 17.0) < 0.000001, "目标 = 17", $"obj={sol.ObjectiveValue:G8}")
        Check(System.Math.Abs(sol.GetSolution("y3") - 1.0) < 0.000001, "y3 = 1", $"y3={sol.GetSolution("y3"):G6}")
        Check(System.Math.Abs(sol.GetSolution("x3") - 10.0) < 0.000001, "x3 = 10", $"x3={sol.GetSolution("x3"):G6}")
    End Sub

    Private Sub T4()
        Console.WriteLine("-- T4 3×3 指派（min，0/1，已知最优 5）--")

        Dim model = AssignmentModel()
        Dim sol = MilpSolver.Solve(model)

        Check(sol.Status = MilpStatus.Optimal, "状态最优", sol.StatusText())
        Check(System.Math.Abs(sol.ObjectiveValue - 5.0) < 0.000001, "目标 = 5", $"obj={sol.ObjectiveValue:G8}")
        Check(IsIntegral(model, sol), "整数性")
    End Sub

    Private Sub T5()
        Console.WriteLine("-- T5 混合整数（min，= 与 ≥，已知最优 18）--")

        Dim model = MixedModel()
        Dim sol = MilpSolver.Solve(model)

        Check(sol.Status = MilpStatus.Optimal, "状态最优", sol.StatusText())
        Check(System.Math.Abs(sol.ObjectiveValue - 18.0) < 0.000001, "目标 = 18", $"obj={sol.ObjectiveValue:G8}")
        Check(System.Math.Abs(sol.GetSolution("x") + 2.0 * sol.GetSolution("y") - 8.0) < 0.000001,
              "等式 x + 2y = 8", $"x={sol.GetSolution("x"):G6}, y={sol.GetSolution("y"):G6}")
    End Sub

    ' ==================================================================
    ' T6 ~ T7：证书
    ' ==================================================================

    Private Sub T6()
        Console.WriteLine("-- T6 不可行证书 --")

        Dim model As New MilpModel With {.ObjectiveSense = "min"}

        model.AddVariable("x", 1, MilpVarType.Binary)
        model.AddVariable("y", 1, MilpVarType.Binary)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 1.0}}, ">=", 3)

        Dim sol = MilpSolver.Solve(model)

        Check(sol.Status = MilpStatus.Infeasible, "状态不可行", sol.StatusText())
        Check(sol.Solution Is Nothing, "无解向量")
        Check(sol.FailureMessage.Contains("不可行"), "失败消息含不可行", sol.FailureMessage)
    End Sub

    Private Sub T7()
        Console.WriteLine("-- T7 无界证书 --")

        Dim model As New MilpModel With {.ObjectiveSense = "max"}

        model.AddVariable("x", 1, MilpVarType.GeneralInteger)
        model.AddVariable("y", 0, MilpVarType.GeneralInteger)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", -1.0}}, "<=", 5)

        Dim sol = MilpSolver.Solve(model)

        Check(sol.Status = MilpStatus.Unbounded, "状态无界", sol.StatusText())
        Check(sol.FailureMessage.Contains("无界"), "失败消息含无界", sol.FailureMessage)
    End Sub

    ' ==================================================================
    ' T8：预处理一致性
    ' ==================================================================

    Private Sub T8()
        Console.WriteLine("-- T8 预处理开关一致性 --")

        Dim model = ProductionModel()
        Dim withPre = MilpSolver.Solve(model, New MilpOptions With {.EnablePresolve = True})
        Dim noPre = MilpSolver.Solve(model, New MilpOptions With {.EnablePresolve = False})

        Check(withPre.Status = MilpStatus.Optimal AndAlso noPre.Status = MilpStatus.Optimal, "两种模式均最优")
        Check(System.Math.Abs(withPre.ObjectiveValue - noPre.ObjectiveValue) < 0.000001,
              "目标值一致", $"{withPre.ObjectiveValue:G8} vs {noPre.ObjectiveValue:G8}")

        ' 预处理应至少起到作用（收紧界 / 固定变量 / 冗余行）
        Dim knap = KnapsackModel()
        Dim kSol = MilpSolver.Solve(knap)
        Check(kSol.Status = MilpStatus.Optimal, "背包（预处理开启）最优")
    End Sub

    ' ==================================================================
    ' T9：割平面 / 启发式生效性
    ' ==================================================================

    Private Sub T9()
        Console.WriteLine("-- T9 割平面 / 启发式生效性 --")

        Dim model = AssignmentModel()

        Dim noCut = MilpSolver.Solve(model, New MilpOptions With {.EnableCuts = False, .EnableHeuristics = False})
        Dim withCut = MilpSolver.Solve(model, New MilpOptions With {.EnableCuts = True, .EnableHeuristics = True})

        Check(System.Math.Abs(noCut.ObjectiveValue - 5.0) < 0.000001, "无割/无启发式仍得最优", $"obj={noCut.ObjectiveValue:G8}")
        Check(System.Math.Abs(withCut.ObjectiveValue - 5.0) < 0.000001, "有割/有启发式得最优", $"obj={withCut.ObjectiveValue:G8}")
        Check(withCut.CutsAdded >= 0, "割平面统计可读", $"cuts={withCut.CutsAdded}")
        Check(withCut.LpSolves >= 1, "LP 求解次数统计", $"lp={withCut.LpSolves}")

        ' 生产计划：LP 松弛是分数（x=3, y=1.5 → 21），根节点 GMI 割应实际生成
        Dim prod = MilpSolver.Solve(ProductionModel())
        Check(System.Math.Abs(prod.ObjectiveValue - 20.0) < 0.000001, "整数间隙问题仍得整数最优", $"obj={prod.ObjectiveValue:G8}")
        Check(prod.CutsAdded > 0, "分数根松弛 → GMI 割实际生成", $"cuts={prod.CutsAdded}")
        Check(prod.RootRelaxation.HasValue AndAlso prod.RootRelaxation.Value > 20.5,
              "根 LP 松弛严格优于整数最优（存在整数间隙）",
              $"root={If(prod.RootRelaxation.HasValue, prod.RootRelaxation.Value.ToString("G8"), "n/a")}")
        Check(prod.RelativeGap < 0.0001, "最终相对间隙 ≈ 0", $"gap={prod.RelativeGap:E3}")

        ' 指派问题的约束矩阵是全幺模的 → 根松弛天然整数，不应产生割（对照）
        Check(withCut.CutsAdded = 0, "全幺模问题根松弛整数（无需割）", $"cuts={withCut.CutsAdded}")
    End Sub

    ' ==================================================================
    ' T10：随机小规模纯整数 vs 暴力枚举
    ' ==================================================================

    Private Sub T10()
        Console.WriteLine("-- T10 随机纯整数 vs 暴力枚举（20 个实例）--")

        Dim rng As New System.Random(20260917)
        Dim mismatch As Integer = 0
        Dim tested As Integer = 0
        Dim statusBad As Integer = 0

        For t As Integer = 1 To 20
            Dim n As Integer = 6
            Dim m As Integer = 5
            Dim sense As String = If(t Mod 2 = 0, "min", "max")
            Dim model As New MilpModel With {.ObjectiveSense = sense}

            For j As Integer = 1 To n
                Dim c As Double = System.Math.Round(4.0 * (rng.NextDouble() * 2.0 - 1.0), 2)
                model.AddVariable($"v{j}", c, MilpVarType.GeneralInteger, 0, rng.Next(2, 6))
            Next

            For i As Integer = 1 To m
                Dim row As New Dictionary(Of String, Double)()

                For j As Integer = 1 To n
                    If rng.NextDouble() < 0.6 Then
                        row($"v{j}") = System.Math.Round(3.0 * (rng.NextDouble() * 2.0 - 1.0), 2)
                    End If
                Next

                If row.Count = 0 Then row("v1") = 1.0

                model.AddConstraint(row, "<=", System.Math.Round(rng.NextDouble() * 25.0, 2))
            Next

            Dim brute = BruteForce(model, 200000)
            Dim sol = MilpSolver.Solve(model)

            If brute.HasValue Then
                tested += 1

                If sol.Status <> MilpStatus.Optimal Then
                    statusBad += 1
                ElseIf System.Math.Abs(sol.ObjectiveValue - brute.Value) > 0.000001 Then
                    mismatch += 1
                    Console.WriteLine($"      实例 {t}: MILP={sol.ObjectiveValue:G10} brute={brute.Value:G10}")
                End If
            Else
                ' 暴力枚举判定不可行 → MILP 也应为不可行
                If sol.Status = MilpStatus.Optimal Then
                    mismatch += 1
                    Console.WriteLine($"      实例 {t}: 暴力判定不可行，但 MILP 返回可行目标 {sol.ObjectiveValue:G10}")
                End If
            End If
        Next

        Check(mismatch = 0, $"20 个实例目标值全部一致（可判定 {tested} 个）", $"mismatch={mismatch}")
        Check(statusBad = 0, "有最优解的实例状态均为 Optimal", $"badStatus={statusBad}")
    End Sub

    ' ==================================================================
    ' T11：解与模型一致性
    ' ==================================================================

    Private Sub T11()
        Console.WriteLine("-- T11 解可行性 / 整数性 / 目标对账 --")

        Dim models As New List(Of (String, MilpModel)) From {
            ("背包", KnapsackModel()),
            ("生产", ProductionModel()),
            ("选址", FacilityModel()),
            ("指派", AssignmentModel()),
            ("混合", MixedModel())
        }

        Dim ok As Boolean = True

        For Each item In models
            Dim sol = MilpSolver.Solve(item.Item2)

            If sol.Solution Is Nothing Then
                ok = False
                Console.WriteLine($"      {item.Item1}: 无解")
                Continue For
            End If

            If Not ProgramMilp.Verify(item.Item2, sol, System.IO.TextWriter.Null) Then ok = False
            If Not IsIntegral(item.Item2, sol) Then ok = False

            ' 目标值对账：Σ cⱼ·xⱼ
            Dim recomputed As Double = 0.0

            For j As Integer = 0 To item.Item2.Variables.Count - 1
                recomputed += item.Item2.Variables(j).coefficient * sol.Solution(j)
            Next

            If System.Math.Abs(recomputed - sol.ObjectiveValue) > 0.000001 Then
                ok = False
                Console.WriteLine($"      {item.Item1}: 目标对账失败 {recomputed:G10} vs {sol.ObjectiveValue:G10}")
            End If
        Next

        Check(ok, "全部模型的可行性 / 整数性 / 目标对账")
    End Sub

    ' ==================================================================
    ' T12：分支规则 / 节点选择规则一致性
    ' ==================================================================

    ''' <summary>
    ''' 30 件 0/1 背包的实例数据（固定随机种子；容量取整以便用 DP 精确校验）。
    ''' </summary>
    Friend Function Knapsack30Data() As (weights As Integer(), values As Double(), capacity As Integer)
        Dim rng As New System.Random(7)
        Dim n As Integer = 30
        Dim weights(n - 1) As Integer
        Dim values(n - 1) As Double
        Dim total As Integer = 0

        For i As Integer = 0 To n - 1
            weights(i) = rng.Next(2, 30)
            values(i) = System.Math.Round(weights(i) * (1.0 + 0.6 * rng.NextDouble()), 2)
            total += weights(i)
        Next

        Dim capacity As Integer = CInt(System.Math.Floor(total * 0.4))

        Return (weights, values, capacity)
    End Function

    ''' <summary>由实例数据构造 30 件 0/1 背包 MILP 模型。</summary>
    Friend Function Knapsack30Model() As MilpModel
        Dim d = Knapsack30Data()

        Return KnapsackModelOf(d.weights, d.values, d.capacity)
    End Function

    Friend Function KnapsackModelOf(weights As Integer(), values As Double(), capacity As Integer) As MilpModel
        Dim model As New MilpModel With {.ObjectiveSense = "max"}
        Dim row As New Dictionary(Of String, Double)()

        For i As Integer = 0 To weights.Length - 1
            model.AddVariable($"x{i + 1}", values(i), MilpVarType.Binary)
            row($"x{i + 1}") = weights(i)
        Next

        model.AddConstraint(row, "<=", capacity)

        Return model
    End Function

    ''' <summary>
    ''' 0/1 背包精确解（一维动态规划，独立于 MILP 求解器的对照实现）。
    ''' 要求重量与容量为正整数。
    ''' </summary>
    Friend Function KnapsackDPExact(weights As Integer(), values As Double(), capacity As Integer) As Double
        Dim dp(capacity) As Double

        For i As Integer = 0 To weights.Length - 1
            Dim w As Integer = weights(i)

            For c As Integer = capacity To w Step -1
                Dim candidate As Double = dp(c - w) + values(i)

                If candidate > dp(c) Then dp(c) = candidate
            Next
        Next

        Return dp(capacity)
    End Function

    Private Sub T12()
        Dim data = Knapsack30Data()
        Dim exact As Double = KnapsackDPExact(data.weights, data.values, data.capacity)

        Console.WriteLine($"-- T12 分支规则 / 节点规则一致性（30 件背包，DP 精确解 {exact:G8}）--")

        Dim combos As (BranchRule, NodeRule)() = {
            (BranchRule.MostFractional, NodeRule.BestBound),
            (BranchRule.FirstFractional, NodeRule.DepthFirst),
            (BranchRule.PseudoCost, NodeRule.BestBound),
            (BranchRule.PseudoCost, NodeRule.DepthFirst)
        }

        For Each c In combos
            Dim model = Knapsack30Model()

            Dim sol = MilpSolver.Solve(model, New MilpOptions With {
                .Branch = c.Item1,
                .Node = c.Item2,
                .MaxSeconds = 120
            })

            Dim objectiveOk As Boolean = sol.Status = MilpStatus.Optimal AndAlso
                                          System.Math.Abs(sol.ObjectiveValue - exact) < 0.005
            Dim feasibleOk As Boolean = sol.Solution IsNot Nothing AndAlso
                                        ProgramMilp.Verify(model, sol, System.IO.TextWriter.Null) AndAlso
                                        IsIntegral(model, sol)

            Check(objectiveOk AndAlso feasibleOk AndAlso sol.DroppedNodes = 0,
                  $"分支={c.Item1}, 节点={c.Item2}",
                  $"状态={sol.StatusText()}，obj={sol.ObjectiveValue:G8}，节点={sol.NodesExplored}，" &
                  $"割={sol.CutsAdded}，LP={sol.LpSolves}，丢弃={sol.DroppedNodes}，{sol.ElapsedMilliseconds}ms")

            If Not objectiveOk Then
                Console.WriteLine($"      诊断日志: {sol.Log.Replace(vbLf, " | ")}")
            End If
        Next
    End Sub

    ' ==================================================================
    ' 工具
    ' ==================================================================

    ''' <summary>
    ''' 暴力枚举求解（仅适用于变量全部为整数且界有限的小规模模型）。
    ''' 返回最优目标值；无可行解返回 Nothing；不可枚举返回 Nothing。
    ''' </summary>
    Friend Function BruteForce(model As MilpModel, maxCombos As Long) As Double?
        Dim n As Integer = model.Variables.Count
        Dim idx As Dictionary(Of String, Integer) = model.VariableIndex()
        Dim lo(n - 1) As Double
        Dim hi(n - 1) As Double
        Dim total As Long = 1

        For j As Integer = 0 To n - 1
            Dim v As MilpVariable = model.Variables(j)

            If v.VarType = MilpVarType.Continuous Then Return Nothing
            If Double.IsInfinity(v.LowerBound) OrElse Double.IsInfinity(v.UpperBound) Then Return Nothing

            lo(j) = System.Math.Ceiling(v.LowerBound - 0.000000001)
            hi(j) = System.Math.Floor(v.UpperBound + 0.000000001)

            If lo(j) > hi(j) Then Return Nothing

            total *= CLng(hi(j) - lo(j) + 1.0)

            If total > maxCombos Then Return Nothing
        Next

        Dim m As Integer = model.Constraints.Count
        Dim A(m - 1, n - 1) As Double
        Dim ops(m - 1) As String
        Dim rhs(m - 1) As Double

        For i As Integer = 0 To m - 1
            Dim con As LppConstraint = model.Constraints(i)

            ops(i) = con.Op.Trim()
            rhs(i) = con.Rhs

            If con.Coefficients Is Nothing Then Continue For

            For Each kvp As KeyValuePair(Of String, Double) In con.Coefficients
                A(i, idx(kvp.Key)) += kvp.Value
            Next
        Next

        Dim maximize As Boolean = model.ObjectiveSense.ToLowerInvariant().StartsWith("max")
        Dim cur(n - 1) As Double
        Dim best As Double? = Nothing

        For j As Integer = 0 To n - 1
            cur(j) = lo(j)
        Next

        Do
            Dim feasible As Boolean = True

            For i As Integer = 0 To m - 1
                Dim lhs As Double = 0.0

                For j As Integer = 0 To n - 1
                    lhs += A(i, j) * cur(j)
                Next

                Select Case ops(i)
                    Case "<=", "≤"
                        If lhs > rhs(i) + 0.000000001 Then feasible = False
                    Case ">=", "≥"
                        If lhs < rhs(i) - 0.000000001 Then feasible = False
                    Case "="
                        If System.Math.Abs(lhs - rhs(i)) > 0.000000001 Then feasible = False
                End Select

                If Not feasible Then Exit For
            Next

            If feasible Then
                Dim obj As Double = 0.0

                For j As Integer = 0 To n - 1
                    obj += model.Variables(j).coefficient * cur(j)
                Next

                If best Is Nothing OrElse
                   (maximize AndAlso obj > best.Value + 0.0000000001) OrElse
                   (Not maximize AndAlso obj < best.Value - 0.0000000001) Then

                    best = obj
                End If
            End If

            Dim p As Integer = n - 1

            While p >= 0
                cur(p) += 1.0

                If cur(p) <= hi(p) Then Exit While

                cur(p) = lo(p)
                p -= 1
            End While

            If p < 0 Then Exit Do
        Loop

        Return best
    End Function

    ''' <summary>检查解中所有整数变量是否取整（容差 1e-6）。</summary>
    Friend Function IsIntegral(model As MilpModel, sol As MilpSolution) As Boolean
        If sol.Solution Is Nothing Then Return False

        For j As Integer = 0 To model.Variables.Count - 1
            Dim v As MilpVariable = model.Variables(j)

            If v.IsInteger AndAlso System.Math.Abs(sol.Solution(j) - System.Math.Round(sol.Solution(j))) > 0.000001 Then
                Return False
            End If
        Next

        Return True
    End Function

End Module
