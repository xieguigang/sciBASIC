' ============================================================================
' Program.vb — LppSolver 演示入口
' ----------------------------------------------------------------------------
' 用法：
'   LppSolver demo        运行三个示例问题（打印 LPPSolution.ToString）
'   LppSolver selftest    内置自检（8 组，对应 Python 镜像测试）
' ============================================================================

Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover

Public Module Program

    Public Function Main(args As String()) As Integer
        If args.Length > 0 AndAlso args(0) = "selftest" Then
            Return SelfTest.RunAll()
        End If
        RunDemos()
        Return 0
    End Function

    Private Sub RunDemos()
        ' ---------- 示例 1：max 3x1 + 5x2（教科书例） ----------
        Dim p1 As New LppProblem With {.ObjectiveSense = "max"}
        p1.Variables.Add(New LppVariable("x1", 3))
        p1.Variables.Add(New LppVariable("x2", 5))
        p1.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x1", 1}}, "<=", 4))
        p1.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x2", 2}}, "<=", 12))
        p1.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x1", 3}, {"x2", 2}}, "<=", 18))
        Dim s1 = LppSolver.Solve(p1)
        Console.WriteLine("===== 示例 1: max 3x1+5x2 s.t. x1<=4, 2x2<=12, 3x1+2x2<=18 =====")
        Console.WriteLine(s1.ToString())
        Console.WriteLine($"SolverError={s1.SolverError}  SolveTime={s1.SolveTime}ms  FeasibleTime={s1.FeasibleSolutionTime}ms")

        ' ---------- 示例 2：min 2x + 3y（≥/≤ 混合） ----------
        Dim p2 As New LppProblem With {.ObjectiveSense = "min"}
        p2.Variables.Add(New LppVariable("x", 2))
        p2.Variables.Add(New LppVariable("y", 3))
        p2.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x", 1}, {"y", 1}}, ">=", 10))
        p2.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x", 1}}, ">=", 2))
        p2.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"y", 1}}, "<=", 8))
        Dim s2 = LppSolver.Solve(p2)
        Console.WriteLine("===== 示例 2: min 2x+3y s.t. x+y>=10, x>=2, y<=8 =====")
        Console.WriteLine(s2.ToString())

        ' ---------- 示例 3：不可行问题 ----------
        Dim p3 As New LppProblem With {.ObjectiveSense = "min"}
        p3.Variables.Add(New LppVariable("x", 1))
        p3.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x", 1}}, ">=", 5))
        p3.Constraints.Add(New LppConstraint(New Dictionary(Of String, Double) From {{"x", 1}}, "<=", 2))
        Dim s3 = LppSolver.Solve(p3)
        Console.WriteLine("===== 示例 3: 不可行（x>=5 且 x<=2） =====")
        Console.WriteLine(s3.ToString())
        Console.WriteLine($"SolverError={s3.SolverError}")
    End Sub

End Module
