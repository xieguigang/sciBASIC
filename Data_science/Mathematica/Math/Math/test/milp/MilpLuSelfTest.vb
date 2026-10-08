#Region "Microsoft.VisualBasic::MilpLuSelfTest.vb, Data_science\Mathematica\Math\Math\test\milp\MilpLuSelfTest.vb"

' Copyright (c) 2018 GPL3 Licensed
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

#End Region

' ============================================================================
' MilpLuSelfTest.vb — LU 增量更新（产品形式 η 修正）的自检
' ----------------------------------------------------------------------------
' T14  LuUpdater 单元对拍：随机可逆方阵 + 随机换基序列下，
'         · Solve / SolveT 与「对当前基矩阵全量重构后 LinAlg.LuSolve / LuSolveT」
'           的结果逐分量偏差 ≤ 1e-9；
'         · 更新解的线性方程残差 ‖B·x − rhs‖∞ ≤ 1e-9；
'         · 闸门拒绝（η 条数达 MaxUpdates）时 NeedsRefactor 置真，重构后恢复。
' T15  端到端 A/B：同一组 MILP 模型分别在 EnableLuUpdate = True / False 下求解，
'         · 状态一致；
'         · 目标值一致（|Δ| ≤ max(1e-9, 1e-9·|obj|)）；
'         · 30 件 0/1 背包两种开关下都必须命中 DP 精确最优 304.3；
'         · 至少一个模型在开启路径上确实发生了 η 更新（证明加速路径被走到）。
'
' 用法：dotnet run -- lu-selftest
' ============================================================================

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.MILP

Public Module MilpLuSelfTest

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

        Console.WriteLine("==================================================================")
        Console.WriteLine(" MILP LU 增量更新自检（T14 单元对拍 / T15 端到端 A/B）")
        Console.WriteLine("==================================================================")
        Console.WriteLine()

        T14()
        Console.WriteLine()
        T15()

        Console.WriteLine()

        If failures = 0 Then
            Console.WriteLine("ALL LU TESTS PASSED")
        Else
            Console.WriteLine($"{failures} LU TEST(S) FAILED")
        End If

        Return failures
    End Function

    ' ====================================================================
    ' T14：LuUpdater 单元对拍
    ' ====================================================================

    Private Sub T14()
        Console.WriteLine("-- T14 LuUpdater 残差对拍（随机矩阵 + 随机换基序列） --")

        Dim worstDelta As Double = 0.0
        Dim worstResidual As Double = 0.0
        Dim worstDeltaT As Double = 0.0
        Dim totalUpdates As Integer = 0
        Dim totalRejects As Integer = 0

        ' 六组规模递增的试验
        For trial As Integer = 0 To 5
            Dim m As Integer = 40 + 25 * trial
            Dim rng As New Random(20240 + trial)
            Dim opts As New LuUpdateOptions With {.Enabled = True, .MaxUpdates = 12}
            Dim lu As New LuUpdater(opts)

            Dim B0 As Double(,) = RandomInvertible(m, rng)
            Dim fac0 As LuFactorization = LinAlg.LuFactor(B0)

            Check(fac0 IsNot Nothing, $"T14 基矩阵可分解 (m={m}, trial={trial})")

            If fac0 Is Nothing Then Continue For

            Call lu.SetBase(fac0)

            Dim Bk As Double(,) = CType(B0.Clone(), Double(,))

            For su As Integer = 0 To 29
                Dim p As Integer = rng.Next(m)
                Dim aq As Double() = RandomColumn(m, p, rng)

                For i As Integer = 0 To m - 1
                    Bk(i, p) = aq(i)
                Next

                Dim accepted As Boolean = lu.ApplyUpdate(p, aq)

                If accepted Then
                    totalUpdates += 1
                Else
                    ' 闸门拒绝：必须标记需要重构，且重构后必须恢复可用
                    totalRejects += 1
                    Check(lu.NeedsRefactor,
                          $"T14 闸门拒绝后 NeedsRefactor (m={m}, step={su})")

                    Dim facR As LuFactorization = LinAlg.LuFactor(Bk)
                    Check(facR IsNot Nothing AndAlso lu.SetBase(facR),
                          $"T14 闸门拒绝后重构成功 (m={m}, step={su})")
                    Check(Not lu.NeedsRefactor AndAlso lu.PendingUpdates = 0,
                          $"T14 重构后状态复位 (m={m}, step={su})")
                End If

                ' ---- 对当前 Bk 全量重构作为参照 ----
                Dim facRef As LuFactorization = LinAlg.LuFactor(Bk)

                If facRef Is Nothing Then Continue For

                Dim rhs As Double() = RandomVec(m, rng)

                Dim xRef As Double() = LinAlg.LuSolve(facRef, rhs)
                Dim xLu As Double() = lu.Solve(rhs)

                If xRef Is Nothing OrElse xLu Is Nothing Then Continue For

                worstDelta = Math.Max(worstDelta, MaxAbsDiff(xLu, xRef))
                worstResidual = Math.Max(worstResidual, ResidualInf(Bk, xLu, rhs))

                Dim rhsT As Double() = RandomVec(m, rng)
                Dim yRef As Double() = LinAlg.LuSolveT(facRef, rhsT)
                Dim yLu As Double() = lu.SolveT(rhsT)

                If yRef IsNot Nothing AndAlso yLu IsNot Nothing Then
                    worstDeltaT = Math.Max(worstDeltaT, MaxAbsDiff(yLu, yRef))
                End If
            Next
        Next

        Console.WriteLine($"      η 更新 {totalUpdates} 次，闸门拒绝 {totalRejects} 次")
        Console.WriteLine($"      最大 |x_更新 − x_重构|∞ = {worstDelta:E3}")
        Console.WriteLine($"      最大 |y_更新 − y_重构|∞ = {worstDeltaT:E3}")
        Console.WriteLine($"      最大残差 ‖B·x − rhs‖∞   = {worstResidual:E3}")

        Check(totalUpdates > 0, "T14 增量更新路径确实被执行")
        Check(worstDelta <= 0.000000001,
              "T14 正解与全量重构逐分量一致 (≤1e-9)", $"worst={worstDelta:E3}")
        Check(worstDeltaT <= 0.000000001,
              "T14 转置解与全量重构逐分量一致 (≤1e-9)", $"worst={worstDeltaT:E3}")
        Check(worstResidual <= 0.000000001,
              "T14 线性方程残差 (≤1e-9)", $"worst={worstResidual:E3}")
    End Sub

    ' ====================================================================
    ' T15：端到端 A/B（EnableLuUpdate True vs False）
    ' ====================================================================

    Private Sub T15()
        Console.WriteLine("-- T15 端到端 A/B：EnableLuUpdate 开关前后结果一致 --")

        Dim cases As New List(Of KeyValuePair(Of String, Func(Of MilpModel))) From {
            New KeyValuePair(Of String, Func(Of MilpModel))("Knapsack", AddressOf MilpSelfTest.KnapsackModel),
            New KeyValuePair(Of String, Func(Of MilpModel))("Production", AddressOf MilpSelfTest.ProductionModel),
            New KeyValuePair(Of String, Func(Of MilpModel))("Facility", AddressOf MilpSelfTest.FacilityModel),
            New KeyValuePair(Of String, Func(Of MilpModel))("Assignment", AddressOf MilpSelfTest.AssignmentModel),
            New KeyValuePair(Of String, Func(Of MilpModel))("Mixed", AddressOf MilpSelfTest.MixedModel)
        }

        Dim anyUpdates As Boolean = False
        Dim anyRefactors As Boolean = False

        For Each cs As KeyValuePair(Of String, Func(Of MilpModel)) In cases
            Dim name As String = cs.Key
            Dim build As Func(Of MilpModel) = cs.Value
            Dim modelOn As MilpModel = build()
            Dim modelOff As MilpModel = build()
            Dim optOn As MilpOptions = LuOptions(True)
            Dim optOff As MilpOptions = LuOptions(False)

            Dim solOn As MilpSolution = MilpSolver.Solve(modelOn, optOn)
            Dim solOff As MilpSolution = MilpSolver.Solve(modelOff, optOff)

            Dim statusOk As Boolean = solOn.Status = solOff.Status
            Dim objOk As Boolean =
                Not Double.IsNaN(solOn.ObjectiveValue) AndAlso
                Not Double.IsNaN(solOff.ObjectiveValue) AndAlso
                Math.Abs(solOn.ObjectiveValue - solOff.ObjectiveValue) <=
                    0.000000001 * Math.Max(1.0, Math.Abs(solOff.ObjectiveValue))

            Dim solDiff As Double = 0.0

            If solOn.Solution IsNot Nothing AndAlso solOff.Solution IsNot Nothing Then
                solDiff = MaxAbsDiff(solOn.Solution, solOff.Solution)
            End If

            anyUpdates = anyUpdates OrElse solOn.LuUpdates > 0
            anyRefactors = anyRefactors OrElse solOn.LuRefactors > 0

            Console.WriteLine($"      {name,-12} ON obj={solOn.ObjectiveValue:G10} " &
                              $"OFF obj={solOff.ObjectiveValue:G10} " &
                              $"(LU 重构 {solOn.LuRefactors} / 更新 {solOn.LuUpdates} / 闸门 {solOn.LuGateRejects})")

            Check(statusOk, $"T15 {name}: 状态一致", $"{solOn.Status} vs {solOff.Status}")
            Check(objOk, $"T15 {name}: 目标值一致 (≤1e-9 相对)")
            Check(solDiff <= 0.000001,
                  $"T15 {name}: 解向量偏差在容差内 (≤1e-6)", $"worst={solDiff:E3}")
        Next

        ' ---- 最强锚点：30 件 0/1 背包必须命中 DP 精确最优 304.3 ----
        Dim bigOn As MilpSolution = MilpSolver.Solve(MilpSelfTest.Knapsack30Model(), LuOptions(True))
        Dim bigOff As MilpSolution = MilpSolver.Solve(MilpSelfTest.Knapsack30Model(), LuOptions(False))

        Console.WriteLine($"      Knapsack30   ON obj={bigOn.ObjectiveValue:G10} " &
                          $"OFF obj={bigOff.ObjectiveValue:G10} " &
                          $"(LU 重构 {bigOn.LuRefactors} / 更新 {bigOn.LuUpdates} / 闸门 {bigOn.LuGateRejects})")

        Check(Math.Abs(bigOn.ObjectiveValue - 304.3) <= 0.0000001,
              "T15 Knapsack30: 开启 LU 更新命中 DP 精确最优 304.3",
              $"got={bigOn.ObjectiveValue:G10}")
        Check(Math.Abs(bigOff.ObjectiveValue - 304.3) <= 0.0000001,
              "T15 Knapsack30: 关闭 LU 更新命中 DP 精确最优 304.3",
              $"got={bigOff.ObjectiveValue:G10}")
        Check(Math.Abs(bigOn.ObjectiveValue - bigOff.ObjectiveValue) <= 0.000000001,
              "T15 Knapsack30: 两种开关目标值一致")

        anyUpdates = anyUpdates OrElse bigOn.LuUpdates > 0

        Check(anyRefactors, "T15 开启路径至少发生一次基分解")
        Check(anyUpdates, "T15 开启路径确实发生 η 增量更新（加速路径被走到）")
    End Sub

    Private Function LuOptions(enableLu As Boolean) As MilpOptions
        Return New MilpOptions With {
            .MaxSeconds = 30,
            .EnableLuUpdate = enableLu
        }
    End Function

    ' ====================================================================
    ' 测试数据与数值工具
    ' ====================================================================

    ''' <summary>对角占优的随机可逆方阵（保证条件数良好，避免测试本身数值脆弱）。</summary>
    Private Function RandomInvertible(m As Integer, rng As Random) As Double(,)
        Dim a(m - 1, m - 1) As Double

        For i As Integer = 0 To m - 1
            For j As Integer = 0 To m - 1
                If i = j Then
                    a(i, j) = m + rng.NextDouble()
                Else
                    a(i, j) = 0.5 * (rng.NextDouble() - 0.5)
                End If
            Next
        Next

        Return a
    End Function

    ''' <summary>随机列：第 p 个分量为大主元，其余为小值（模拟对角占优结构的换列）。</summary>
    Private Function RandomColumn(m As Integer, p As Integer, rng As Random) As Double()
        Dim col(m - 1) As Double

        For i As Integer = 0 To m - 1
            col(i) = 0.5 * (rng.NextDouble() - 0.5)
        Next

        col(p) = m + rng.NextDouble()

        Return col
    End Function

    Private Function RandomVec(m As Integer, rng As Random) As Double()
        Dim v(m - 1) As Double

        For i As Integer = 0 To m - 1
            v(i) = 2.0 * (rng.NextDouble() - 0.5)
        Next

        Return v
    End Function

    Private Function MaxAbsDiff(a As Double(), b As Double()) As Double
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then
            Return Double.PositiveInfinity
        End If

        Dim worst As Double = 0.0

        For i As Integer = 0 To a.Length - 1
            Dim d As Double = Math.Abs(a(i) - b(i))

            If d > worst Then worst = d
        Next

        Return worst
    End Function

    ''' <summary>残差 ‖B·x − rhs‖∞（B 为方阵，行主序）。</summary>
    Private Function ResidualInf(B As Double(,), x As Double(), rhs As Double()) As Double
        If B Is Nothing OrElse x Is Nothing OrElse rhs Is Nothing Then
            Return Double.PositiveInfinity
        End If

        Dim m As Integer = B.GetLength(0)
        Dim n As Integer = B.GetLength(1)
        Dim worst As Double = 0.0

        For i As Integer = 0 To m - 1
            Dim s As Double = 0.0

            For j As Integer = 0 To n - 1
                s += B(i, j) * x(j)
            Next

            Dim d As Double = Math.Abs(s - rhs(i))

            If d > worst Then worst = d
        Next

        Return worst
    End Function

End Module
