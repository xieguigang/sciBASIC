Imports System.Diagnostics
Imports System.Linq
Imports Microsoft.VisualBasic.Imaging.Physics.Boids
Imports Microsoft.VisualBasic.Math.SIMD

Module Program

    Dim failures As Integer = 0
    Dim checks As Integer = 0

    Sub Main()
        Console.WriteLine("===========================================================")
        Console.WriteLine(" Boids SIMD engine self test")
        Console.WriteLine($" runtime: {Environment.Version}, os: {Environment.OSVersion}")
        Console.WriteLine($" cpu cores: {Environment.ProcessorCount}")
        Console.WriteLine($" simd: enabled={SIMDEnvironment.IsEnabled}, hardware={SIMDEnvironment.IsHardwareAccelerated}, avx2={SIMDEnvironment.IsAvx2Supported}, fma={SIMDEnvironment.IsFmaSupported}")
        Console.WriteLine($" vector(double).count = {System.Numerics.Vector(Of Double).Count}")
        Console.WriteLine("===========================================================")

        TestCellRounding()
        DebugDivergence()
        TestEngineAgainstReference()
        TestViewApi()
        Benchmark()

        Console.WriteLine()
        Console.WriteLine("===========================================================")
        Console.WriteLine($" result: {checks - failures}/{checks} checks passed")

        If failures > 0 Then
            Console.WriteLine($" FAILED with {failures} error(s).")
            Environment.ExitCode = 1
        Else
            Console.WriteLine(" ALL PASSED.")
            Environment.ExitCode = 0
        End If

        Console.WriteLine("===========================================================")
    End Sub

    ''' <summary>
    ''' the fast banker's rounding used by the grid cell computation must agree 
    ''' with the VB CInt() conversion bit for bit
    ''' </summary>
    Sub TestCellRounding()
        Console.WriteLine()
        Console.WriteLine("[1/4] grid cell rounding (CellRound vs CInt) ...")

        Dim rand As New Random(42)
        Dim samples As New List(Of Double)

        ' exact ties around zero and positives/negatives
        For k As Integer = -12 To 12
            samples.Add(k + 0.5)
            samples.Add(k - 0.5)
            samples.Add(k)
        Next

        ' random values across several magnitudes
        For i As Integer = 0 To 200000
            Dim scale = 10.0 ^ rand.Next(-3, 6)
            samples.Add((rand.NextDouble() * 2 - 1) * scale)
        Next

        ' values that stress the double -> int conversion boundary
        For i As Integer = 0 To 20000
            samples.Add(rand.Next(-100, 100) / 15.0)
            samples.Add(rand.Next(-100, 100) / 7.0)
        Next

        Dim bad As Integer = 0

        For Each v As Double In samples
            Dim expect As Integer = CInt(v)
            Dim actual As Integer = FlockGrid.CellRound(v)

            If expect <> actual Then
                bad += 1

                If bad <= 10 Then
                    Console.WriteLine($"   MISMATCH: CellRound({v}) = {actual}, CInt({v}) = {expect}")
                End If
            End If
        Next

        Check($"CellRound matches CInt on {samples.Count} samples", pass:=bad = 0)
    End Sub

    ''' <summary>
    ''' Focused divergence diagnostics: run both engines for a single frame on a 
    ''' small flock and dump the per-boid rule-force deltas that disagree, so the 
    ''' source of the discrepancy can be identified.
    ''' </summary>
    Sub DebugDivergence()
        Console.WriteLine()
        Console.WriteLine("[debug] single frame divergence diagnostics (n=100) ...")

        Const n As Integer = 100
        Dim seed As New Random(20240101)
        Dim initial As New FlockState(n)

        For i As Integer = 0 To n - 1
            initial.px(i) = seed.NextDouble() * 2000.0
            initial.py(i) = seed.NextDouble() * 1200.0
            initial.vx(i) = (seed.NextDouble() - 0.5) * 4.0
            initial.vy(i) = (seed.NextDouble() - 0.5) * 4.0
        Next

        ' engine side: build grid + solve only, then snapshot dvx/dvy
        Dim field As New Field(2000.0, 1200.0, n)
        Array.Copy(initial.px, field.State.px, n)
        Array.Copy(initial.py, field.State.py, n)
        Array.Copy(initial.vx, field.State.vx, n)
        Array.Copy(initial.vy, field.State.vy, n)

        field.Grid.Build(field.State, field.Radius)
        Dim solver As New FlockSolver(field)
        solver.Run()

        Dim engineDvx = field.State.dvx.Clone()
        Dim engineDvy = field.State.dvy.Clone()

        ' reference side
        Dim referenceState = initial.Clone()
        FlockReference.Advance(referenceState, 2000.0, 1200.0, True, False, field.PredatorCount)

        ' the reference integrates in place; recompute its dv from the state delta
        ' is not possible, so instead re-run the reference phase 1 only by using a
        ' fresh clone and comparing dv directly through a second pass
        Dim ref2 = initial.Clone()
        Dim refSolver = New FlockSolver(field)   ' dummy, not used
        refSolver.Run()

        Dim diffs As New List(Of (i As Integer, dx As Double, dy As Double))

        For i As Integer = 0 To n - 1
            Dim dx = engineDvx(i) - ref2.dvx(i)
            Dim dy = engineDvy(i) - ref2.dvy(i)

            If System.Math.Abs(dx) > 1.0E-9 OrElse System.Math.Abs(dy) > 1.0E-9 Then
                diffs.Add((i, dx, dy))
            End If
        Next

        Console.WriteLine($" boids with dv mismatch: {diffs.Count} / {n}")

        For Each d In diffs.Take(8)
            Console.WriteLine($"   boid {d.i}: dvx diff = {d.dx:E4}, dvy diff = {d.dy:E4}, " &
                              $"engine dvx = {engineDvx(d.i):F6}, ref dvx = {ref2.dvx(d.i):F6}")
        Next
    End Sub

    ''' <summary>
    ''' runs the SIMD engine and the legacy-semantics reference engine on two 
    ''' identical copies of a deterministic initial state and compares the result
    ''' </summary>
    Sub TestEngineAgainstReference()
        Console.WriteLine()
        Console.WriteLine("[2/4] SIMD engine vs legacy reference ...")

        Dim configs = {
            (n:=500, frames:=30, bounce:=True, wrap:=False),
            (n:=500, frames:=30, bounce:=False, wrap:=True),
            (n:=2000, frames:=20, bounce:=True, wrap:=False),
            (n:=2000, frames:=20, bounce:=True, wrap:=True)
        }

        For Each cfg In configs
            Dim seed As New Random(20240101)

            ' deterministic initial state
            Dim initial As New FlockState(cfg.n)

            For i As Integer = 0 To cfg.n - 1
                initial.px(i) = seed.NextDouble() * 2000.0
                initial.py(i) = seed.NextDouble() * 1200.0
                initial.vx(i) = (seed.NextDouble() - 0.5) * 4.0
                initial.vy(i) = (seed.NextDouble() - 0.5) * 4.0
            Next

            Dim referenceState = initial.Clone()

            Dim field As New Field(2000.0, 1200.0, cfg.n)
            ' the solver reads the field's own grid instance, so drive that one
            Dim grid = field.Grid
            Dim solver As New FlockSolver(field)

            ' inject the deterministic initial state into the engine side
            Array.Copy(initial.px, field.State.px, cfg.n)
            Array.Copy(initial.py, field.State.py, cfg.n)
            Array.Copy(initial.vx, field.State.vx, cfg.n)
            Array.Copy(initial.vy, field.State.vy, cfg.n)

            For f As Integer = 1 To cfg.frames
                grid.Build(field.State, field.Radius)
                solver.Run()
                FlockIntegrate.Integrate(field.State, 2000.0, 1200.0, cfg.bounce, cfg.wrap, 1.0, 6.0)

                FlockReference.Advance(referenceState, 2000.0, 1200.0, cfg.bounce, cfg.wrap, field.PredatorCount)

                Dim diff = MaxDiff(field.State, referenceState)

                If diff > 1.0E-9 Then
                    Check($"n={cfg.n} frame={f} bounce={cfg.bounce} wrap={cfg.wrap}", False,
                          $"max|delta| = {diff:E3}")
                    Return
                End If
            Next

            Check($"n={cfg.n} frames={cfg.frames} bounce={cfg.bounce} wrap={cfg.wrap} (max|delta| < 1e-9)", True)
        Next
    End Sub

    ''' <summary>
    ''' the public <see cref="Boid"/> view API must round trip through the SoA state
    ''' </summary>
    Sub TestViewApi()
        Console.WriteLine()
        Console.WriteLine("[3/4] Boid view API ...")

        Dim field As New Field(800.0, 600.0, 64)
        Dim state = field.State

        Dim ok As Boolean = True

        If field.Entity.Count <> 64 Then ok = False
        If state.count <> 64 Then ok = False
        If field.Width <> 800.0 OrElse field.Height <> 600.0 Then ok = False

        Dim b = field(7)
        Dim x0 = state.px(7)
        Dim y0 = state.py(7)

        If b.x <> x0 OrElse b.y <> y0 Then ok = False

        b.Xvel = 3.0
        b.Yvel = 4.0

        If state.vx(7) <> 3.0 OrElse state.vy(7) <> 4.0 Then ok = False
        If b.GetSpeed() <> 5.0 Then ok = False

        b.MoveForward(1, 5)

        If state.px(7) <> x0 + 3.0 OrElse state.py(7) <> y0 + 4.0 Then ok = False
        If System.Math.Abs(b.GetSpeed() - 5.0) > 1.0E-12 Then ok = False

        Dim pos = b.GetPosition(2.0)
        If pos.Item1 <> state.px(7) + 6.0 Then ok = False

        Dim counted As Integer = 0

        For Each e In field.Entity
            counted += 1
        Next

        If counted <> 64 Then ok = False

        Check("Boid view API", ok)
    End Sub

    ''' <summary>
    ''' 80k boids performance benchmark with per phase breakdown
    ''' </summary>
    Sub Benchmark()
        Console.WriteLine()
        Console.WriteLine("[4/4] 80k boids benchmark (Release numbers only) ...")

        Const n As Integer = 80000
        Const frames As Integer = 100
        Const warmup As Integer = 10

        Dim field As New Field(2000.0, 1200.0, n)
        Dim state = field.State
        Dim grid = field.Grid
        Dim solver As New FlockSolver(field)

        ' warm up (JIT, thread pool, page faults)
        For i As Integer = 1 To warmup
            AdvanceOne(field, grid, solver)
        Next

        ' grid diagnostics: make sure the spatial grid is actually populated
        Dim occupied As Integer = 0

        For c As Integer = 0 To grid.nCells - 1
            If grid.cellStart(c + 1) > grid.cellStart(c) Then occupied += 1
        Next

        Console.WriteLine($" grid: {grid.nx} x {grid.ny} = {grid.nCells} cells, occupied = {occupied}, " &
                          $"avg fill = {n / grid.nCells:F2}, avg neighbor candidates ~= {9.0 * n / grid.nCells:F1}")

        Report($"SIMD   ({frames} frames)", field, grid, solver, state, frames)

        SIMDEnvironment.config = SIMDConfiguration.disable
        Report($"scalar ({frames} frames)", field, grid, solver, state, frames)
        SIMDEnvironment.config = SIMDConfiguration.auto

        ' allocation check (steady state)
        Dim before As Long = GC.GetAllocatedBytesForCurrentThread()

        For i As Integer = 1 To 20
            AdvanceOne(field, grid, solver)
        Next

        Dim perFrame As Double = (GC.GetAllocatedBytesForCurrentThread() - before) / 20.0
        Console.WriteLine()
        Console.WriteLine($" allocated bytes per frame: {perFrame:F1} bytes")
    End Sub

    Sub AdvanceOne(field As Field, grid As FlockGrid, solver As FlockSolver)
        grid.Build(field.State, field.Radius)
        solver.Run()
        FlockIntegrate.Integrate(field.State, field.Width, field.Height, True, False, 1.0, 6.0)
    End Sub

    Sub Report(title As String, field As Field, grid As FlockGrid, solver As FlockSolver,
               state As FlockState, frames As Integer)
        Dim tGrid As New List(Of Double)
        Dim tSolve As New List(Of Double)
        Dim tIntegrate As New List(Of Double)
        Dim sw As Stopwatch

        For i As Integer = 1 To frames
            sw = Stopwatch.StartNew()
            grid.Build(state, field.Radius)
            sw.Stop()
            tGrid.Add(sw.Elapsed.TotalMilliseconds)

            sw = Stopwatch.StartNew()
            solver.Run()
            sw.Stop()
            tSolve.Add(sw.Elapsed.TotalMilliseconds)

            sw = Stopwatch.StartNew()
            FlockIntegrate.Integrate(state, field.Width, field.Height, True, False, 1.0, 6.0)
            sw.Stop()
            tIntegrate.Add(sw.Elapsed.TotalMilliseconds)
        Next

        Dim gAvg = tGrid.Average(), sAvg = tSolve.Average(), iAvg = tIntegrate.Average()

        Console.WriteLine()
        Console.WriteLine($" --- {title} ---")
        Console.WriteLine($" grid build : avg {gAvg,7:F3} ms   min {tGrid.Min(),7:F3} ms   max {tGrid.Max(),7:F3} ms")
        Console.WriteLine($" solve      : avg {sAvg,7:F3} ms   min {tSolve.Min(),7:F3} ms   max {tSolve.Max(),7:F3} ms")
        Console.WriteLine($" integrate  : avg {iAvg,7:F3} ms   min {tIntegrate.Min(),7:F3} ms   max {tIntegrate.Max(),7:F3} ms")
        Console.WriteLine($" TOTAL      : avg {gAvg + sAvg + iAvg,7:F3} ms   min {tGrid.Min() + tSolve.Min() + tIntegrate.Min(),7:F3} ms   max {tGrid.Max() + tSolve.Max() + tIntegrate.Max(),7:F3} ms")
        Console.WriteLine($" implied fps: {1000.0 / (gAvg + sAvg + iAvg),7:F1}")
    End Sub

    Function MaxDiff(a As FlockState, b As FlockState) As Double
        Dim worst As Double = 0.0

        For i As Integer = 0 To a.count - 1
            worst = System.Math.Max(worst, System.Math.Abs(a.px(i) - b.px(i)))
            worst = System.Math.Max(worst, System.Math.Abs(a.py(i) - b.py(i)))
            worst = System.Math.Max(worst, System.Math.Abs(a.vx(i) - b.vx(i)))
            worst = System.Math.Max(worst, System.Math.Abs(a.vy(i) - b.vy(i)))
        Next

        Return worst
    End Function

    Sub Check(name As String, pass As Boolean, Optional detail As String = "")
        checks += 1

        If pass Then
            Console.WriteLine($"   [PASS] {name}")
        Else
            failures += 1
            Console.WriteLine($"   [FAIL] {name} {detail}")
        End If
    End Sub

End Module
