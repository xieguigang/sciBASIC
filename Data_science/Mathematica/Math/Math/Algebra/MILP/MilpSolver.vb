' ============================================================================
' MilpSolver.vb — MILP 求解顶层入口
' ----------------------------------------------------------------------------
' 流水线编排：
'   MilpModel（校验）→ MilpPresolve（界收紧 / 固定变量消元 / 工作形式构造）
'   → BranchAndBound（根 LP 松弛 → 根节点 GMI 割 → 启发式 → 分支定界）
'   → MilpSolution（原始空间解 / 目标值 / 最优界 / 相对间隙 / 统计 / 日志）
'
' 复用关系（不重写既有 LP 代码）：
'   · 线性代数      : IPMCrossover.LinAlg / LuFactorization
'   · 变量与结果模型: LinearProgramming.LppVariable / LPPSolution / OptimizationType
'   · 约束模型      : IPMCrossover.LppConstraint
'   新增的 BoundedSimplex 只负责"有界变量"这一 LP 工作形式，
'   与既有 SimplexSolver / InteriorPointSolver 形成互补（后者面向 x ≥ 0 标准形）。
'
' Copyright (c) 2018 GPL3 Licensed — sciBASIC.NET Foundation
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming.MILP

    ''' <summary>
    ''' 混合整数线性规划（MILP）求解器入口。
    ''' </summary>
    ''' <example>
    ''' <code>
    ''' Dim model As New MilpModel With {.ObjectiveSense = "max"}
    ''' model.AddVariable("x", 60, MilpVarType.Binary)
    ''' model.AddVariable("y", 100, MilpVarType.Binary)
    ''' model.AddConstraint(New Dictionary(Of String, Double) From {{"x", 10}, {"y", 20}}, "&lt;=", 30)
    ''' Dim sol = MilpSolver.Solve(model)
    ''' Console.WriteLine(sol.ToString())
    ''' </code>
    ''' </example>
    Public Module MilpSolver

        ''' <summary>
        ''' 求解 MILP 模型。
        ''' </summary>
        ''' <param name="model">MILP 模型（变量含连续/整数/二进制与上下界）</param>
        ''' <param name="options">求解选项；Nothing 使用默认值</param>
        Public Function Solve(model As MilpModel, Optional options As MilpOptions = Nothing) As MilpSolution
            Dim opt As MilpOptions = If(options, New MilpOptions()).Clone()
            Dim log As New List(Of String)()
            Dim watch As Stopwatch = Stopwatch.StartNew()

            ' 规范化变量名（空名自动命名为 x1, x2, ...）
            Dim varNames As String()

            Try
                model.VariableIndex()
                varNames = model.VariableNames()
            Catch ex As Exception
                Return MakeError(New String() {}, $"模型变量名不合法: {ex.Message}", log, watch)
            End Try

            Dim err As String = model.Validate()

            If Not err.StringEmpty Then
                Return MakeError(varNames, err, log, watch)
            End If

            log.Add($"MILP 模型: {model}")

            Dim form As MilpLpForm

            Try
                form = MilpPresolve.Run(model, opt, log)
            Catch ex As Exception
                Return MakeError(varNames, $"预处理失败: {ex.Message}", log, watch)
            End Try

            If form.Stats IsNot Nothing AndAlso form.Stats.IsInfeasible Then
                Dim bad As New MilpSolution(MilpStatus.Infeasible, Nothing, form.VariableNames,
                                            Double.NaN, Double.NaN, Double.PositiveInfinity)

                bad.FailureMessage = form.Stats.InfeasibleReason
                bad.Log = String.Join(ControlChars.Lf, log)
                bad.ElapsedMilliseconds = watch.ElapsedMilliseconds

                Return bad
            End If

            Dim bnb As New BranchAndBound(form, opt, log, watch)
            Dim solution As MilpSolution = bnb.Solve()

            solution.Log = String.Join(ControlChars.Lf, log)
            solution.ElapsedMilliseconds = watch.ElapsedMilliseconds

            Return solution
        End Function

        ''' <summary>便捷重载：仅指定时间上限（秒）。</summary>
        Public Function Solve(model As MilpModel, maxSeconds As Double) As MilpSolution
            Return Solve(model, New MilpOptions With {.MaxSeconds = maxSeconds})
        End Function

        Private Function MakeError(varNames As String(), message As String,
                                   log As List(Of String), watch As Stopwatch) As MilpSolution

            Dim bad As New MilpSolution(MilpStatus.Error, Nothing, varNames,
                                        Double.NaN, Double.NaN, Double.PositiveInfinity)

            bad.FailureMessage = message
            bad.Log = String.Join(ControlChars.Lf, log)
            bad.ElapsedMilliseconds = watch.ElapsedMilliseconds

            Return bad
        End Function

    End Module

End Namespace
