#Region "Microsoft.VisualBasic::MilpLuBenchmark.vb, Data_science\Mathematica\Math\Math\test\milp\MilpLuBenchmark.vb"

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
' MilpLuBenchmark.vb — LU 增量更新（产品形式 η 修正）的 A/B 基准
' ----------------------------------------------------------------------------
' 测量方式与 SimdBenchmark 一致：同一个模型在同一程序集、同一优化级别下分别以
'   EnableLuUpdate = False  每次换基完整重构 LU（优化前路径，对照组）
'   EnableLuUpdate = True   换基 O(m²) 增量修正 + 周期性重构（优化后路径）
' 求解，多轮取最优耗时，输出 m/n、迭代数、重构/更新/闸门次数与加速比。
'
' 附带一个纯数值层基准：同一基矩阵在"每次换基全量重构 + LuSolve"与
' "LuUpdater.ApplyUpdate + Solve"下的耗时对比，隔离出 LU 层本身的收益。
'
' 用法：dotnet run -- milp-bench
' ============================================================================

Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.IPMCrossover
Imports Microsoft.VisualBasic.Math.LinearAlgebra.LinearProgramming.MILP

Public Module MilpLuBenchmark

    ''' <summary>累加器：阻止 JIT 把整段计算当作无用代码消除。</summary>
    Private sink As Double = 0

    Public Function RunAll() As Integer
        Console.WriteLine("==================================================================")
        Console.WriteLine(" MILP LU 增量更新基准（EnableLuUpdate = False vs True）")
        Console.WriteLine("==================================================================")
        Console.WriteLine()

        BenchLuLayer()
        Console.WriteLine()

        BenchMilpEndToEnd()
        Console.WriteLine()

        Console.WriteLine($" （校验和 sink = {sink:G6}，仅用于阻止 JIT 死代码消除）")

        Return 0
    End Function

    ' ====================================================================
    ' 一、纯数值层：LU 更新 vs 每次换基全量重构
    ' ====================================================================

    Private Sub BenchLuLayer()
        Console.WriteLine("--- 数值层：换基序列下的 LU 维护（重构 vs 增量更新） ---")

        Dim orders As Integer() = {80, 160, 320, 640}

        For Each m As Integer In orders
            Dim rng As New Random(97531 + m)
            Dim B0 As Double(,) = RandomInvertible(m, rng)

            ' 预生成整个换基序列（两种方案消费完全相同的输入）
            Dim cols As New List(Of KeyValuePair(Of Integer, Double()))()

            For s As Integer = 0 To 59
                cols.Add(New KeyValuePair(Of Integer, Double())(rng.Next(m), RandomColumn(m, -1, rng)))
            Next

            Dim baselineMs As Double = BestOf(Function() RefactorPath(B0, cols))
            Dim updatedMs As Double = BestOf(Function() UpdatePath(B0, cols, 20))
            Dim speedup As Double = If(updatedMs > 0, baselineMs / updatedMs, 0)

            Console.WriteLine($" m = {m,4}  重构 {baselineMs,8:F3} ms | 增量 {updatedMs,8:F3} ms | 加速 {speedup,6:F2}x   (60 次换基)")
        Next
    End Sub

    ''' <summary>对照组：每次换基都做一次完整 LU 分解。</summary>
    Private Function RefactorPath(B0 As Double(,), cols As List(Of KeyValuePair(Of Integer, Double()))) As Double
        Dim m As Integer = B0.GetLength(0)
        Dim B As Double(,) = CType(B0.Clone(), Double(,))
        Dim acc As Double = 0.0

        For Each kv In cols
            For i As Integer = 0 To m - 1
                B(i, kv.Key) = kv.Value(i)
            Next

            Dim fac = LinAlg.LuFactor(B)

            If fac IsNot Nothing Then
                Dim x = LinAlg.LuSolve(fac, UnitVec(m))
                acc += x(0)
            End If
        Next

        Return acc
    End Function

    ''' <summary>优化组：一次完整分解 + 增量修正，每 maxUpdates 次重构一次。</summary>
    Private Function UpdatePath(B0 As Double(,), cols As List(Of KeyValuePair(Of Integer, Double())),
                                maxUpdates As Integer) As Double
        Dim m As Integer = B0.GetLength(0)
        Dim B As Double(,) = CType(B0.Clone(), Double(,))
        Dim lu As New LuUpdater(New LuUpdateOptions With {.Enabled = True, .MaxUpdates = maxUpdates})

        Call lu.SetBase(LinAlg.LuFactor(B0))

        Dim acc As Double = 0.0

        For Each kv In cols
            For i As Integer = 0 To m - 1
                B(i, kv.Key) = kv.Value(i)
            Next

            If lu.NeedsRefactor Then
                Call lu.SetBase(LinAlg.LuFactor(B))
            End If

            If lu.ApplyUpdate(kv.Key, kv.Value) Then
                sink += 0.0
            End If

            Dim x = lu.Solve(UnitVec(m))

            If x IsNot Nothing Then acc += x(0)
        Next

        Return acc
    End Function

    ' ====================================================================
    ' 二、端到端：MILP 求解整体耗时与统计
    ' ====================================================================

    Private Sub BenchMilpEndToEnd()
        Console.WriteLine("--- 端到端：MILP 求解（30 件 0/1 背包 + 设施选址） ---")

        Dim cases As New List(Of KeyValuePair(Of String, Func(Of MilpModel))) From {
            New KeyValuePair(Of String, Func(Of MilpModel))("Knapsack30", AddressOf MilpSelfTest.Knapsack30Model),
            New KeyValuePair(Of String, Func(Of MilpModel))("Facility", AddressOf MilpSelfTest.FacilityModel),
            New KeyValuePair(Of String, Func(Of MilpModel))("Assignment", AddressOf MilpSelfTest.AssignmentModel)
        }

        For Each cs As KeyValuePair(Of String, Func(Of MilpModel)) In cases
            Dim build As Func(Of MilpModel) = cs.Value
            Dim modelOff As MilpModel = build()
            Dim modelOn As MilpModel = build()

            Dim solOff As MilpSolution = MilpSolver.Solve(modelOff, BenchOptions(False))
            Dim solOn As MilpSolution = MilpSolver.Solve(modelOn, BenchOptions(True))

            Dim speedup As Double =
                If(solOn.ElapsedMilliseconds > 0, solOff.ElapsedMilliseconds / solOn.ElapsedMilliseconds, 0)

            Console.WriteLine()
            Console.WriteLine($" {cs.Key}")
            Console.WriteLine($"   m×n 规模     : 见求解日志（稠密工作形式）")
            Console.WriteLine($"   关闭 LU 更新 : {solOff.ElapsedMilliseconds,8} ms | 状态 {solOff.Status} | obj {solOff.ObjectiveValue:G10}")
            Console.WriteLine($"   开启 LU 更新 : {solOn.ElapsedMilliseconds,8} ms | 状态 {solOn.Status} | obj {solOn.ObjectiveValue:G10}")
            Console.WriteLine($"   加速比       : {speedup,6:F2}x")
            Console.WriteLine($"   LU 统计      : 重构 {solOn.LuRefactors}，增量更新 {solOn.LuUpdates}，" &
                              $"闸门拒绝 {solOn.LuGateRejects}（对照：重构 {solOff.LuRefactors}，更新 {solOff.LuUpdates}）")
            Console.WriteLine($"   迭代/LP      : 节点 {solOn.NodesExplored}，LP {solOn.LpSolves}（对照 LP {solOff.LpSolves}）")

            sink += solOn.ObjectiveValue + solOff.ObjectiveValue
        Next
    End Sub

    Private Function BenchOptions(enableLu As Boolean) As MilpOptions
        Return New MilpOptions With {
            .MaxSeconds = 60,
            .EnableLuUpdate = enableLu
        }
    End Function

    ' ====================================================================
    ' 计时与数据工具
    ' ====================================================================

    Private Function BestOf(action As Func(Of Double)) As Double
        Const rounds As Integer = 3

        Dim best As Double = Double.MaxValue

        For i As Integer = 0 To rounds - 1
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim r As Double = action()
            sw.Stop()

            sink += r

            If sw.Elapsed.TotalMilliseconds < best Then
                best = sw.Elapsed.TotalMilliseconds
            End If
        Next

        Return best
    End Function

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

    ''' <summary>随机列；p &lt; 0 表示不指定主元位置（仍以第一个分量为主元）。</summary>
    Private Function RandomColumn(m As Integer, p As Integer, rng As Random) As Double()
        Dim col(m - 1) As Double
        Dim pivotRow As Integer = If(p >= 0, p, 0)

        For i As Integer = 0 To m - 1
            col(i) = 0.5 * (rng.NextDouble() - 0.5)
        Next

        col(pivotRow) = m + rng.NextDouble()

        Return col
    End Function

    Private Function UnitVec(m As Integer) As Double()
        Dim v(m - 1) As Double
        v(0) = 1.0
        Return v
    End Function

End Module
