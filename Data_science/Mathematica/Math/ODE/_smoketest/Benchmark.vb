Imports System.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Math.Calculus.Dynamics
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' The performance benchmark of the ODEs integration kernels.
''' 
''' 基准的比较对象：
''' 
''' 1. ``legacy``：使用 <see cref="Vector"/> 的算术运算符实现的 RK4，每一个积分步都会产生
'''    十余个临时向量，这是重构之前的实现方式；
''' 2. ``func``：新的零分配内核 + 用户重写的 ``func``(需要同步 var 状态变量)；
''' 3. ``fast``：新的零分配内核 + 数组式直通的导数求值器，没有任何 var 包装开销。
''' </summary>
Module Benchmark

    Public Sub Run()
        Console.WriteLine()
        Console.WriteLine("--- benchmark ---")
        Console.WriteLine($"{"case",-22}{"mode",-10}{"n",8}{"elapsed(ms)",14}{"allocated(MB)",16}{"speedup",10}")

        Dim logisticFunc As New Cases.LogisticModel()
        Dim logisticFast As New Cases.LogisticModel()

        Call logisticFast.EnableFastPath()
        Call BenchVar("logistic (1 var)", logisticFunc, logisticFast, 24000, 0.0, 5.0, True)
        Call BenchVar("chain (10 vars)", Cases.CreateChain(10, fast:=False), Cases.CreateChain(10, fast:=True), 12000, 0.0, 5.0, False)

        Dim lorenzFunc As New Cases.LorenzModel()
        Dim lorenzFast As New Cases.LorenzModel()

        Call lorenzFast.EnableFastPath()
        Call BenchVar("lorenz (3 vars)", lorenzFunc, lorenzFast, 24000, 0.0, 20.0, True)

        Console.WriteLine()
        Call BenchAdaptive("lorenz (3 vars)", lorenzFast, 1000, 0.0, 20.0)
    End Sub

    ''' <summary>
    ''' Benchmark the fixed step size solvers.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="modelFunc">The model which is integrated by the ``func`` override.</param>
    ''' <param name="modelFast">The model which is integrated by the array based derivative evaluator.</param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="checkLegacy">
    ''' The legacy implementation is compared with the reflection based model only, because
    ''' the variable names of the <see cref="GenericODEs"/> model cannot be resolved by the
    ''' field order.
    ''' </param>
    Private Sub BenchVar(name As String, modelFunc As ODEs, modelFast As ODEs,
                         n As Integer, a As Double, b As Double, checkLegacy As Boolean)

        Dim y0 As Double() = modelFunc.GetY0()

        ' 预热
        Call LegacyRk4(modelFunc, y0, 200, a, b)
        Call modelFunc.Solve(200, a, b)
        Call modelFast.Solve(200, a, b)

        Dim legacy = Measure(Sub() Call LegacyRk4(modelFunc, y0, n, a, b))
        Dim funcPath = Measure(Sub() Call modelFunc.Solve(n, a, b))
        Dim fastPath = Measure(Sub() Call modelFast.Solve(n, a, b))

        Call PrintRow(name, "legacy", n, legacy.ms, legacy.bytes, legacy.ms)
        Call PrintRow(name, "func", n, funcPath.ms, funcPath.bytes, legacy.ms)
        Call PrintRow(name, "fast", n, fastPath.ms, fastPath.bytes, legacy.ms)

        ' 分配量：新的内核只分配输出结果，所以应该远小于旧的实现
        Call Check($"bench {name}: the new kernel allocates less than 1/4 of the legacy implementation",
                   fastPath.bytes * 4 < legacy.bytes,
                   $"legacy={legacy.bytes / 1024.0 / 1024.0:F2}MB new={fastPath.bytes / 1024.0 / 1024.0:F2}MB")

        ' 数值对拍：两条新路径必须完全一致
        Dim reference = modelFunc.Solve(1000, a, b)
        Dim compare = modelFast.Solve(1000, a, b)
        Dim maxDiff As Double = 0.0

        For Each series As NamedCollection(Of Double) In reference.y.Values
            Dim diff As Double = series.value.MaxDiff(compare.y(series.name).value)

            If diff > maxDiff Then
                maxDiff = diff
            End If
        Next

        Call Check($"bench {name}: the fast path matches the func path", maxDiff < 1.0E-12, $"max diff={maxDiff:G4}")

        If checkLegacy Then
            Dim legacyResult As Double()() = LegacyRk4(modelFunc, y0, 1000, a, b)
            Dim varNames As String() = ODEs.GetVariables(modelFunc.GetType()).ToArray

            For i As Integer = 0 To varNames.Length - 1
                Call Check($"bench {name}: the zero allocation kernel matches the legacy rk4 [{varNames(i)}]",
                           legacyResult(i).MaxDiff(reference.y(varNames(i)).value) < 1.0E-12,
                           $"max diff={legacyResult(i).MaxDiff(reference.y(varNames(i)).value):G4}")
            Next
        End If
    End Sub

    ''' <summary>
    ''' Benchmark the adaptive step size solver.
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="model"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    Private Sub BenchAdaptive(name As String, model As ODEs, n As Integer, a As Double, b As Double)
        Dim solver As New RungeKutta45(model)
        Dim y0 As Double() = model.GetY0()

        ' 预热
        Call solver.Solve(y0, 100, a, b, 1.0E-10, 1.0E-8)

        Dim adaptive = Measure(Sub() Call solver.Solve(y0, n, a, b, 1.0E-10, 1.0E-8))

        Call PrintRow(name, "rk45", n, adaptive.ms, adaptive.bytes, adaptive.ms)
        Console.WriteLine($"    adaptive steps={solver.NumberOfSteps}, function evaluations={solver.NumberOfEvaluations}")

        Dim result = model.SolveAdaptive(10, a, b, 1.0E-10, 1.0E-8)

        Call Check("bench adaptive: the adaptive solver produces a finite result", Not result.HaveNaN)
    End Sub

    ''' <summary>
    ''' The reference Runge-Kutta 4th order implementation which is based on the
    ''' <see cref="Vector"/> arithmetic operators, this is the implementation style of the
    ''' original solver, and it is used as the performance baseline here.
    ''' </summary>
    ''' <param name="system"></param>
    ''' <param name="y0"></param>
    ''' <param name="n"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Function LegacyRk4(system As ODEs, y0 As Double(), n As Integer, a As Double, b As Double) As Double()()
        Dim dh As Double = (b - a) / n
        Dim y As New Vector(y0)
        Dim columns As Double()() = New Double(y0.Length - 1)() {}

        For i As Integer = 0 To columns.Length - 1
            columns(i) = New Double(n) {}
            columns(i)(0) = y0(i)
        Next

        Dim k1 As Vector
        Dim k2 As Vector
        Dim k3 As Vector
        Dim k4 As Vector

        For iStep As Integer = 1 To n
            Dim t As Double = a + dh * (iStep - 1)

            k1 = Derivative(system, t, y)
            k2 = Derivative(system, t + dh / 2, y + dh / 2 * k1)
            k3 = Derivative(system, t + dh / 2, y + dh / 2 * k2)
            k4 = Derivative(system, t + dh, y + dh * k3)

            y = y + (k1 + 2 * k2 + 2 * k3 + k4) * dh / 6

            For i As Integer = 0 To y0.Length - 1
                columns(i)(iStep) = y(i)
            Next
        Next

        Return columns
    End Function

    Private Function Derivative(system As ODEs, t As Double, y As Vector) As Vector
        Dim k As New Vector(y.Length)

        Call system.Derivatives(t, y, k)

        Return k
    End Function

    Private Function Measure(work As Action) As (ms As Double, bytes As Long)
        Dim allocated As Long = GC.GetAllocatedBytesForCurrentThread()
        Dim started As Long = Stopwatch.GetTimestamp()

        Call work()

        Dim finished As Long = Stopwatch.GetTimestamp()
        Dim allocatedAfter As Long = GC.GetAllocatedBytesForCurrentThread()

        Return ((finished - started) * 1000.0 / Stopwatch.Frequency, allocatedAfter - allocated)
    End Function

    Private Sub PrintRow(caseName As String, mode As String, n As Integer, ms As Double, bytes As Long, baseline As Double)
        Console.WriteLine($"{caseName,-22}{mode,-10}{n,8}{ms,14:F3}{bytes / 1024.0 / 1024.0,16:F3}{baseline / ms,9:F2}x")
    End Sub
End Module
