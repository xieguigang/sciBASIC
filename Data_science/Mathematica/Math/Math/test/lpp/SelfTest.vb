#Region "Microsoft.VisualBasic::63aad3b8d64fdd19fc5b12194d961c73, Data_science\Mathematica\Math\Math\test\lpp\SelfTest.vb"

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

    '   Total Lines: 245
    '    Code Lines: 213 (86.94%)
    ' Comment Lines: 16 (6.53%)
    '    - Xml Docs: 6.25%
    ' 
    '   Blank Lines: 16 (6.53%)
    '     File Size: 12.42 KB


    ' Module SelfTest
    ' 
    '     Function: MakeProb, RunAll
    ' 
    '     Sub: Check, T1, T2, T3, T4
    '          T5, T6, T7, T8, T9
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' SelfTest.vb — 内置自检（对应 Python 镜像 validate_lpp.py 的 8 组测试）
' ----------------------------------------------------------------------------
' T1 已知最优+影子价  T2 ≥/≤ 混合  T3 退化多解  T4 Klee-Minty
' T5 不可行证书  T6 无界证书  T7 随机阵 KKT  T8 冗余等式行
' 附加：LPPSolution 接口行为（SolverError/GetSolution/打印分档）
' ============================================================================

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

Public Module SelfTest

    Private failures As Int32 = 0

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
        Console.WriteLine("=== LppSolver SelfTest（IPM + Crossover）===")
        T1()
        T2()
        T3()
        T4()
        T5()
        T6()
        T7()
        T8()
        T9()
        Console.WriteLine($"=== {If(failures = 0, "ALL TESTS PASSED", failures & " TEST(S) FAILED")} ===")
        Return failures
    End Function

    ''' <summary>构造问题的便捷方法</summary>
    Private Function MakeProb(sense As String, vars As (String, Double)(),
                              cons As (Dictionary(Of String, Double), String, Double)()) As LppProblem
        Dim p As New LppProblem With {.ObjectiveSense = sense}
        For Each v In vars
            p.Variables.Add(New LppVariable(v.Item1, v.Item2))
        Next
        For Each cc In cons
            p.Constraints.Add(New LppConstraint(cc.Item1, cc.Item2, cc.Item3))
        Next
        Return p
    End Function

    Private Sub T1()
        Console.WriteLine("-- T1 已知最优 + 影子价 (max 3x1+5x2) --")
        Dim p = MakeProb("max", {("x1", 3.0), ("x2", 5.0)},
                         {(New Dictionary(Of String, Double) From {{"x1", 1.0}}, "<=", 4.0),
                              (New Dictionary(Of String, Double) From {{"x2", 2.0}}, "<=", 12.0),
                              (New Dictionary(Of String, Double) From {{"x1", 3.0}, {"x2", 2.0}}, "<=", 18.0)})
        Dim s As LPPSolution = LppSolver.Solve(p)
        Check(Not s.SolverError, "求解成功")
        Check(Math.Abs(s.ObjectiveFunctionValue - 36.0) < 0.000001, "目标 = 36", $"obj={s.ObjectiveFunctionValue:G10}")
        Check(Math.Abs(s.GetSolution("x1") - 2.0) < 0.00001 AndAlso Math.Abs(s.GetSolution("x2") - 6.0) < 0.00001,
              "x* = (2,6)", $"x=({s.GetSolution("x1"):G6},{s.GetSolution("x2"):G6})")
        Dim ys = s.shadowPrice
        Check(Math.Abs(ys(0)) < 0.000001 AndAlso Math.Abs(ys(1) - 1.5) < 0.000001 AndAlso Math.Abs(ys(2) - 1.0) < 0.000001,
              "影子价 (0,1.5,1)", $"y=({ys(0):G6},{ys(1):G6},{ys(2):G6})")
        ' slack：约束1 非绑 slack=2；约束2/3 绑定 0
        Dim sl = s.slack
        Check(Math.Abs(sl(0) - 2.0) < 0.000001 AndAlso sl(1) = 0 AndAlso sl(2) = 0,
              "slack (2,0,0) 绑定判定", $"sl=({sl(0):G6},{sl(1)},{sl(2)})")
    End Sub

    Private Sub T2()
        Console.WriteLine("-- T2 ≥/≤ 混合 (min 2x+3y) --")
        Dim p = MakeProb("min", {("x", 2.0), ("y", 3.0)},
                         {(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 1.0}}, ">=", 10.0),
                              (New Dictionary(Of String, Double) From {{"x", 1.0}}, ">=", 2.0),
                              (New Dictionary(Of String, Double) From {{"y", 1.0}}, "<=", 8.0)})
        Dim s = LppSolver.Solve(p)
        Check(Not s.SolverError, "求解成功")
        Check(Math.Abs(s.ObjectiveFunctionValue - 20.0) < 0.000001, "目标 = 20", $"obj={s.ObjectiveFunctionValue:G10}")
        Check(Math.Abs(s.GetSolution("x") - 10.0) < 0.00001 AndAlso Math.Abs(s.GetSolution("y")) < 0.00001,
              "x* = (10,0)")
        Dim ys = s.shadowPrice
        Check(Math.Abs(ys(0) - 2.0) < 0.000001 AndAlso Math.Abs(ys(1)) < 0.000001 AndAlso Math.Abs(ys(2)) < 0.000001,
              "影子价 (2,0,0)")
        ' ≥ 非绑定 → 负 slack（surplus）
        Dim sl = s.slack
        Check(Math.Abs(sl(1) - (-8.0)) < 0.000001, "x>=2 surplus = −8", $"sl(1)={sl(1):G6}")
    End Sub

    Private Sub T3()
        Console.WriteLine("-- T3 退化多解 (min x+y, x+y=5) --")
        Dim p = MakeProb("min", {("x", 1.0), ("y", 1.0)},
                         {(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", 1.0}}, "=", 5.0),
                              (New Dictionary(Of String, Double) From {{"x", 1.0}}, "<=", 3.0),
                              (New Dictionary(Of String, Double) From {{"y", 1.0}}, ">=", 1.0)})
        Dim s = LppSolver.Solve(p)
        Check(Not s.SolverError, "求解成功")
        Check(Math.Abs(s.ObjectiveFunctionValue - 5.0) < 0.000001, "目标 = 5", $"obj={s.ObjectiveFunctionValue:G10}")
        Dim xv = s.GetSolution("x") : Dim yv = s.GetSolution("y")
        Check(Math.Abs(xv + yv - 5.0) < 0.000001 AndAlso xv <= 3.0 + 0.000001 AndAlso yv >= 1.0 - 0.000001,
              "可行性", $"x=({xv:G6},{yv:G6})")
    End Sub

    Private Sub T4()
        Console.WriteLine("-- T4 Klee-Minty 3 维 --")
        Dim p = MakeProb("max", {("x1", 100.0), ("x2", 10.0), ("x3", 1.0)},
                         {(New Dictionary(Of String, Double) From {{"x1", 1.0}}, "<=", 1.0),
                              (New Dictionary(Of String, Double) From {{"x1", 20.0}, {"x2", 1.0}}, "<=", 100.0),
                              (New Dictionary(Of String, Double) From {{"x1", 200.0}, {"x2", 20.0}, {"x3", 1.0}}, "<=", 10000.0)})
        Dim s = LppSolver.Solve(p)
        Check(Not s.SolverError, "求解成功")
        Check(Math.Abs(s.ObjectiveFunctionValue - 10000.0) < 0.000001, "目标 = 10000",
              $"obj={s.ObjectiveFunctionValue:G10}")
    End Sub

    Private Sub T5()
        Console.WriteLine("-- T5 不可行证书 --")
        Dim p = MakeProb("min", {("x", 1.0)},
                         {(New Dictionary(Of String, Double) From {{"x", 1.0}}, ">=", 5.0),
                              (New Dictionary(Of String, Double) From {{"x", 1.0}}, "<=", 2.0)})
        Dim s = LppSolver.Solve(p)
        Check(s.SolverError, "SolverError = True")
        Check(s.failureMessage.Contains("不可行"), "失败消息含不可行", s.failureMessage)
    End Sub

    Private Sub T6()
        Console.WriteLine("-- T6 无界证书 --")
        ' max x s.t. x − y <= 1（y ≥ 0 可无限增大 → x 无界）
        Dim p = MakeProb("max", {("x", 1.0), ("y", 0.0)},
                         {(New Dictionary(Of String, Double) From {{"x", 1.0}, {"y", -1.0}}, "<=", 1.0)})
        Dim s = LppSolver.Solve(p)
        Check(s.SolverError, "SolverError = True")
        Check(s.failureMessage.Contains("无界"), "失败消息含无界", s.failureMessage)
    End Sub

    Private Sub T7()
        Console.WriteLine("-- T7 随机 25×65 KKT --")
        Dim rng As New Random(42)
        Dim m0 As Int32 = 15, n As Int32 = 25
        Dim vars As New List(Of LppVariable)()
        Dim c(n - 1) As Double
        For j = 0 To n - 1
            c(j) = Math.Round(3.0 * (rng.NextDouble() * 2.0 - 1.0), 3)
            vars.Add(New LppVariable($"v{j}", c(j)))
        Next
        Dim cons As New List(Of LppConstraint)()
        Dim xstar(n - 1) As Double
        For j = 0 To n - 1
            If rng.NextDouble() < 0.6 Then xstar(j) = 0.5 + 4.5 * rng.NextDouble()
        Next
        For i = 0 To m0 - 1
            Dim row As New Dictionary(Of String, Double)()
            For j = 0 To n - 1
                If rng.NextDouble() < 0.35 Then
                    row($"v{j}") = Math.Round(2.0 * (rng.NextDouble() * 2.0 - 1.0), 3)
                End If
            Next
            Dim rhs As Double = 0
            For j = 0 To n - 1
                If row.ContainsKey($"v{j}") Then rhs += row($"v{j}") * xstar(j)
            Next
            cons.Add(New LppConstraint(row, "<=", Math.Round(rhs, 6)))
        Next
        ' c<0 的变量加上界行保证有界
        For j = 0 To n - 1
            If c(j) < 0 Then
                cons.Add(New LppConstraint(New Dictionary(Of String, Double) From {{$"v{j}", 1.0}}, "<=",
                                           xstar(j) + 0.5 + 2.5 * rng.NextDouble()))
            End If
        Next
        Dim p As New LppProblem With {.ObjectiveSense = "min"}
        p.Variables.AddRange(vars)
        p.Constraints.AddRange(cons)
        Dim s = LppSolver.Solve(p)
        Check(Not s.SolverError, "求解成功")
        ' KKT：互补松弛 + 支撑集 reduced cost ≈ 0 + 可行性
        Dim feasOk = True
        For Each con In p.Constraints
            Dim lhs As Double = 0
            For Each kvp In con.Coefficients
                lhs += kvp.Value * s.GetSolution(kvp.Key)
            Next
            Select Case con.Op
                Case "<=" : If lhs > con.Rhs + 0.000001 Then feasOk = False
                Case ">=" : If lhs < con.Rhs - 0.000001 Then feasOk = False
            End Select
        Next
        Check(feasOk, "原始可行")
        ' 互补松弛（min：x_j > 0 → r_j ≈ 0；r_j > 0 → x_j ≈ 0）
        Dim scOk = True
        For j = 0 To n - 1
            Dim xj = s.solution(j)
            Dim dj = s.reducedCost(j)
            ' min 问题：x_j > 0 → r_j ≈ 0；r_j > 0 → x_j ≈ 0
            If xj > 0.000001 AndAlso Math.Abs(dj) > 0.00001 Then scOk = False
            If dj > 0.000001 AndAlso xj > 0.000001 Then scOk = False
        Next
        Check(scOk, "互补松弛（支撑集 reduced cost 零性）")
        ' 目标值对账：obj = cᵀx
        Dim obj2 As Double = 0
        For j = 0 To n - 1
            obj2 += c(j) * s.solution(j)
        Next
        Check(Math.Abs(obj2 - s.ObjectiveFunctionValue) < 0.000001, "目标值对账",
              $"{obj2:G10} vs {s.ObjectiveFunctionValue:G10}")
    End Sub

    Private Sub T8()
        Console.WriteLine("-- T8 冗余等式行 --")
        Dim p = MakeProb("min", {("x1", 1.0), ("x2", 1.0), ("x3", 1.0)},
                         {(New Dictionary(Of String, Double) From {{"x1", 1.0}, {"x2", 1.0}, {"x3", 1.0}}, "=", 4.0),
                              (New Dictionary(Of String, Double) From {{"x1", 1.0}, {"x2", 2.0}}, "=", 5.0),
                              (New Dictionary(Of String, Double) From {{"x1", 2.0}, {"x2", 3.0}, {"x3", 1.0}}, "=", 9.0)})
        Dim s = LppSolver.Solve(p)
        Check(Not s.SolverError, "求解成功", s.failureMessage)
        Check(Math.Abs(s.ObjectiveFunctionValue - 4.0) < 0.000001, "目标 = 4",
              $"obj={s.ObjectiveFunctionValue:G10}")
    End Sub

    Private Sub T9()
        Console.WriteLine("-- T9 LPPSolution 接口行为 --")
        Dim sol = New Double() {2.0, 6.0}
        Dim s = New LPPSolution(sol, 36.0, New String() {"x1", "x2"},
                                New String() {"<=", "<=", "<="},
                                New Double() {2.0, 0.0, 0.0},
                                New Double() {0.0, 1.5, 1.0},
                                New Double() {0.0, 0.0},
                                10, 5, "log", "G5")
        Check(Not s.SolverError, "成功解 SolverError = False")
        Check(s.GetSolution("x2") = 6.0, "GetSolution(name)")
        Dim named = s.GetSolution().ToList()
        Check(named.Count = 2 AndAlso named(1).Name = "x2" AndAlso named(1).Value = 6.0,
              "GetSolution() 枚举 NamedValue")
        Dim txt = s.ToString()
        Check(txt.Contains("binding") AndAlso txt.Contains("shadow price 1.5"),
              "打印分档：binding + shadow price")
        Dim f = New LPPSolution("测试失败", "log", 1)
        Check(f.SolverError, "失败解 SolverError = True")
        Check(f.ToString() = "测试失败", "失败时 ToString 返回消息")
    End Sub

End Module

