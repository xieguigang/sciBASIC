#Region "Microsoft.VisualBasic::c0bd1750dd517fd062538b3ddca70820, Data_science\Mathematica\Math\Math\test\milp\ProgramMilp.vb"

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

    '   Total Lines: 333
    '    Code Lines: 210 (63.06%)
    ' Comment Lines: 48 (14.41%)
    '    - Xml Docs: 6.25%
    ' 
    '   Blank Lines: 75 (22.52%)
    '     File Size: 14.59 KB


    ' Module ProgramMilp
    ' 
    '     Function: Main, Verify
    ' 
    '     Sub: DemoAssignment, DemoFacilityLocation, DemoInfeasible, DemoIntegerProduction, DemoKnapsackBinary
    '          DemoLargeKnapsack, DemoMixedEquality, DemoUnbounded, PrintModel, RunAndReport
    '          RunDemos
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' ProgramMilp.vb — MILP 求解器演示入口
' ----------------------------------------------------------------------------
' 用法：
'   dotnet run                 运行全部演示（默认 demo）
'   dotnet run -- demo         同上
'   dotnet run -- selftest     内置自检（与暴力枚举 / DP 精确解 / 已知最优对拍）
'   dotnet run -- lpp          既有 IPM+Crossover 线性规划演示
'   dotnet run -- lpp-selftest 既有线性规划自检
'
' 演示覆盖：0/1 背包、一般整数生产计划、设施选址（二进制 + 连续）、
' 指派问题、连续+整数混合（= 与 ≥ 约束）、不可行、无界，
' 以及一个稍大规模的背包（展示割平面 / 启发式 / 分支定界的综合效果）。
' ============================================================================

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.MILP

Public Module ProgramMilp

    Public Function Main(args As String()) As Integer
        If args.Length > 0 Then
            Select Case args(0).ToLowerInvariant()
                Case "selftest"
                    Return MilpSelfTest.RunAll()
                Case "simd"
                    ' Vector / NumericMatrix SIMD 重构的正确性验证
                    Return VectorMatrixSimdTest.RunAll()
                Case "simd-bench"
                    ' 标量 vs SIMD 的耗时与加速比基准
                    Return SimdBenchmark.RunAll()
                Case "simd-all"
                    Dim simdCode As Integer = VectorMatrixSimdTest.RunAll()

                    Console.WriteLine()
                    Console.WriteLine()

                    Dim benchCode As Integer = SimdBenchmark.RunAll()

                    Return If(simdCode <> 0 OrElse benchCode <> 0, 1, 0)
                Case "svd"
                    ' 既有 TruncatedSVD 对拍测试（覆盖 SVD/NumericMatrix 的数值路径）
                    TruncatedSVDTest.Main()

                    Return 0
                Case "lpp"
                    ' 保持对既有 LP 求解器演示入口的访问（自 test.vbproj 的启动对象切换之后）
                    Return ProgramLpp.Main(New String() {})
                Case "lpp-selftest"
                    Return ProgramLpp.Main(New String() {"selftest"})
            End Select
        End If

        RunDemos()

        Return 0
    End Function

    Private Sub RunDemos()
        Console.WriteLine("==================================================================")
        Console.WriteLine(" MILP 求解器演示（预处理 + 根割平面 + 启发式 + 分支定界）")
        Console.WriteLine("==================================================================")
        Console.WriteLine()

        DemoKnapsackBinary()
        DemoIntegerProduction()
        DemoFacilityLocation()
        DemoAssignment()
        DemoMixedEquality()
        DemoInfeasible()
        DemoUnbounded()
        DemoLargeKnapsack()

        Console.WriteLine("演示结束。运行 `dotnet run -- selftest` 可执行内置自检。")
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 1：0/1 背包（max）
    ' ------------------------------------------------------------------
    Private Sub DemoKnapsackBinary()
        Dim weights As Double() = {2, 3, 4, 5, 6, 7, 8, 9, 10, 11}
        Dim values As Double() = {3, 4, 5, 6, 7, 8, 9, 10, 11, 12}
        Dim capacity As Double = 20

        Dim model As New MilpModel With {.ObjectiveSense = "max"}

        For i As Integer = 0 To weights.Length - 1
            model.AddVariable($"x{i + 1}", values(i), MilpVarType.Binary)
        Next

        Dim row As New Dictionary(Of String, Double)()

        For i As Integer = 0 To weights.Length - 1
            row($"x{i + 1}") = weights(i)
        Next

        model.AddConstraint(row, "<=", capacity)

        RunAndReport("演示 1：0/1 背包 max Σvᵢxᵢ, s.t. Σwᵢxᵢ ≤ 20（已知最优 25 = 重量 20 装满 5 件）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 2：一般整数生产计划（max，含整数间隙）
    ' ------------------------------------------------------------------
    Private Sub DemoIntegerProduction()
        Dim model As New MilpModel With {.ObjectiveSense = "max"}

        model.AddVariable("x", 5, MilpVarType.GeneralInteger)
        model.AddVariable("y", 4, MilpVarType.GeneralInteger)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 6.0}, {"y", 4.0}}, "<=", 24)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 2.0}}, "<=", 6)

        RunAndReport("演示 2：整数生产计划 max 5x+4y, s.t. 6x+4y ≤ 24, x+2y ≤ 6（LP 松弛 21，整数最优 20）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 3：设施选址（min，二进制 + 连续）
    ' ------------------------------------------------------------------
    Private Sub DemoFacilityLocation()
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

        RunAndReport("演示 3：设施选址 min 5y₁+6y₂+7y₃+2x₁+3x₂+x₃, Σx=10, xⱼ ≤ 10yⱼ（已知最优 17）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 4：指派问题（min，0/1）
    ' ------------------------------------------------------------------
    Private Sub DemoAssignment()
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

        RunAndReport("演示 4：3×3 指派问题 min Σcᵢⱼxᵢⱼ（已知最优 5）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 5：连续 + 整数混合（= 与 ≥ 约束，多解）
    ' ------------------------------------------------------------------
    Private Sub DemoMixedEquality()
        Dim model As New MilpModel With {.ObjectiveSense = "min"}

        model.AddVariable("x", 2, MilpVarType.GeneralInteger)
        model.AddVariable("y", 3, MilpVarType.GeneralInteger)
        model.AddVariable("z", 1, MilpVarType.Continuous)

        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 1.0}, {"z", 1.0}}, ">=", 10)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 2.0}}, "=", 8)

        RunAndReport("演示 5：混合整数 min 2x+3y+z, x+y+z ≥ 10, x+2y = 8（已知最优 18）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 6：不可行
    ' ------------------------------------------------------------------
    Private Sub DemoInfeasible()
        Dim model As New MilpModel With {.ObjectiveSense = "min"}

        model.AddVariable("x", 1, MilpVarType.Binary)
        model.AddVariable("y", 1, MilpVarType.Binary)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 1.0}}, ">=", 3)

        RunAndReport("演示 6：不可行 x+y ≥ 3（x,y ∈ {0,1}）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 7：无界
    ' ------------------------------------------------------------------
    Private Sub DemoUnbounded()
        Dim model As New MilpModel With {.ObjectiveSense = "max"}

        model.AddVariable("x", 1, MilpVarType.GeneralInteger)
        model.AddVariable("y", 0, MilpVarType.GeneralInteger)
        model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", -1.0}}, "<=", 5)

        RunAndReport("演示 7：无界 max x, s.t. x − y ≤ 5（y 可无限增大）", model)
    End Sub

    ' ------------------------------------------------------------------
    ' 演示 8：稍大规模 0/1 背包（30 件，展示割平面 / 启发式效果）
    ' ------------------------------------------------------------------
    Private Sub DemoLargeKnapsack()
        Dim data = MilpSelfTest.Knapsack30Data()
        Dim model = MilpSelfTest.KnapsackModelOf(data.weights, data.values, data.capacity)
        Dim exact As Double = MilpSelfTest.KnapsackDPExact(data.weights, data.values, data.capacity)

        RunAndReport($"演示 8：30 件 0/1 背包（容量 {data.capacity}，动态规划精确解 {exact:G8} → 与分支定界结果对拍）", model)
    End Sub

    ' ==================================================================
    ' 通用报告
    ' ==================================================================

    Public Sub RunAndReport(title As String, model As MilpModel)
        Console.WriteLine("------------------------------------------------------------------")
        Console.WriteLine(title)
        Console.WriteLine("------------------------------------------------------------------")

        PrintModel(model)
        Console.WriteLine()

        Dim options As New MilpOptions With {.MaxSeconds = 30}
        Dim solution As MilpSolution = MilpSolver.Solve(model, options)

        Console.WriteLine(solution.ToString())

        If solution.Solution IsNot Nothing Then
            Dim okAll As Boolean = Verify(model, solution, Console.Out)
            Console.WriteLine($"约束与整数性校验: {If(okAll, "通过", "失败")}")
        End If

        Console.WriteLine()
    End Sub

    Private Sub PrintModel(model As MilpModel)
        Console.WriteLine($"目标方向: {model.ObjectiveSense}")

        Console.Write("变量: ")

        For Each v As MilpVariable In model.Variables
            Dim t As String = If(v.VarType = MilpVarType.Continuous, "连续", v.VarType.ToString())
            Dim ub As String = If(Double.IsPositiveInfinity(v.UpperBound), "+∞", v.UpperBound.ToString("G6"))

            Console.Write($"{v.symbol}[{t}, {v.LowerBound:G6}..{ub}, c={v.coefficient:G6}]  ")
        Next

        Console.WriteLine()

        For i As Integer = 0 To model.Constraints.Count - 1
            Dim con As LppConstraint = model.Constraints(i)
            Dim parts As New List(Of String)()

            For Each kvp As KeyValuePair(Of String, Double) In con.Coefficients
                parts.Add($"{kvp.Value:G6}·{kvp.Key}")
            Next

            Console.WriteLine($"  约束 {i + 1}: {String.Join(" + ", parts)} {con.Op} {con.Rhs:G6}")
        Next
    End Sub

    ''' <summary>
    ''' 校验解是否满足变量界、整数性与全部约束；同时把违反项打印到 <paramref name="out"/>。
    ''' </summary>
    Public Function Verify(model As MilpModel, solution As MilpSolution, out As System.IO.TextWriter) As Boolean
        If solution.Solution Is Nothing Then Return False

        Dim tol As Double = 0.000001
        Dim ok As Boolean = True

        For i As Integer = 0 To model.Variables.Count - 1
            Dim v As MilpVariable = model.Variables(i)
            Dim x As Double = solution.Solution(i)

            If x < v.LowerBound - tol OrElse x > v.UpperBound + tol Then
                out.WriteLine($"  [违反] {v.symbol} = {x} 超出界 [{v.LowerBound}, {v.UpperBound}]")
                ok = False
            End If

            If v.IsInteger AndAlso System.Math.Abs(x - System.Math.Round(x)) > tol Then
                out.WriteLine($"  [违反] {v.symbol} = {x} 不是整数")
                ok = False
            End If
        Next

        For k As Integer = 0 To model.Constraints.Count - 1
            Dim con As LppConstraint = model.Constraints(k)
            Dim lhs As Double = 0.0

            For Each kvp As KeyValuePair(Of String, Double) In con.Coefficients
                lhs += kvp.Value * solution.GetSolution(kvp.Key)
            Next

            Dim bad As Boolean = False

            Select Case con.Op.Trim()
                Case "<=", "≤" : bad = lhs > con.Rhs + tol * (1.0 + System.Math.Abs(con.Rhs))
                Case ">=", "≥" : bad = lhs < con.Rhs - tol * (1.0 + System.Math.Abs(con.Rhs))
                Case "=" : bad = System.Math.Abs(lhs - con.Rhs) > tol * (1.0 + System.Math.Abs(con.Rhs))
            End Select

            If bad Then
                out.WriteLine($"  [违反] 约束 {k + 1}: {lhs:G8} {con.Op} {con.Rhs:G8}")
                ok = False
            End If
        Next

        Return ok
    End Function

End Module
