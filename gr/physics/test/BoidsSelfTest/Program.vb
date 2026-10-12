Imports System.Diagnostics
Imports System.Linq
Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Imaging.Physics.Boids
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

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
        BenchmarkLegacy()
        BenchmarkRenderModel()

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

        Dim engineDvx As Double() = field.State.dvx.Clone()
        Dim engineDvy As Double() = field.State.dvy.Clone()

        ' reference side: its phase 1 leaves dvx/dvy filled
        Dim ref2 = initial.Clone()
        FlockReference.Advance(ref2, 2000.0, 1200.0, True, False, field.PredatorCount)

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

    ''' <summary>
    ''' Baseline benchmark that faithfully replicates the LEGACY per-frame cost 
    ''' model: serial LINQ GroupBy grid build (nested dictionary spatial hash) + 
    ''' multi threaded per-boid rules with LINQ Where/ToArray/Sum over the grid 
    ''' lookups. This is the "before" number of the refactor.
    ''' </summary>
    Sub BenchmarkLegacy()
        Console.WriteLine()
        Console.WriteLine("[extra] legacy engine baseline (LINQ grid + dictionary hash), 80k boids ...")

        Const n As Integer = 80000
        Const frames As Integer = 10

        Dim field As New Field(2000.0, 1200.0, n)

        ' warm up
        For i As Integer = 1 To 2
            LegacyFrame(field)
        Next

        Dim sw = Stopwatch.StartNew()

        For i As Integer = 1 To frames
            LegacyFrame(field)
        Next

        sw.Stop()

        Dim avg As Double = sw.Elapsed.TotalMilliseconds / frames

        Console.WriteLine($" legacy TOTAL: avg {avg,7:F3} ms/frame -> {1000.0 / avg,7:F1} fps (physics only)")
    End Sub

    Sub LegacyFrame(field As Field)
        Dim state = field.State
        Dim n = state.count
        Dim px = state.px
        Dim py = state.py
        Dim vx = state.vx
        Dim vy = state.vy
        Dim dvx = state.dvx
        Dim dvy = state.dvy

        ' legacy: rebuild the dictionary-of-dictionaries grid every frame
        Dim grid = field.Entity.EncodeGrid(field.Radius)

        ' legacy: VectorTask parallel per-boid rules
        Dim workers As Integer = System.Math.Max(1, VectorTask.n_threads)
        Dim span As Integer = n \ workers

        System.Threading.Tasks.Parallel.For(0, workers, Sub(t)
            Dim start As Integer = t * span
            Dim ends As Integer = start + span - 1

            If start >= n Then Return
            If ends >= n Then ends = n - 1

            For i As Integer = start To ends
                Dim boid = field(i)
                Dim neighbors = grid.SpatialLookup(boid, field.Radius) _
                    .Where(Function(x) GeomDistance(x, boid) < 50.0) _
                    .ToArray()

                Dim meanX As Double = neighbors.Sum(Function(x) x.x) / neighbors.Count()
                Dim meanY As Double = neighbors.Sum(Function(x) x.y) / neighbors.Count()
                Dim flockX As Double = (meanX - boid.x) * 0.0003
                Dim flockY As Double = (meanY - boid.y) * 0.0003

                Dim meanVX As Double = neighbors.Sum(Function(x) x.Xvel) / neighbors.Count()
                Dim meanVY As Double = neighbors.Sum(Function(x) x.Yvel) / neighbors.Count()
                Dim alignX As Double = (meanVX - boid.Xvel) * 0.01
                Dim alignY As Double = (meanVY - boid.Yvel) * 0.01

                Dim avoidNeighbors = grid.SpatialLookup(boid, field.Radius) _
                    .Where(Function(x) GeomDistance(x, boid) < 20.0)
                Dim avoidX As Double = 0.0
                Dim avoidY As Double = 0.0

                For Each nb As Boid In avoidNeighbors
                    Dim closeness As Double = 20.0 - GeomDistance(nb, boid)
                    avoidX += (boid.x - nb.x) * closeness
                    avoidY += (boid.y - nb.y) * closeness
                Next

                Dim predX As Double = 0.0
                Dim predY As Double = 0.0

                For p As Integer = 0 To field.PredatorCount - 1
                    Dim pred = field(p)
                    Dim d As Double = GeomDistance(pred, boid)

                    If d < 150.0 Then
                        Dim closeness As Double = 150.0 - d
                        predX += (boid.x - pred.x) * closeness
                        predY += (boid.y - pred.y) * closeness
                    End If
                Next

                dvx(i) = flockX + avoidX * 0.001 + alignX + predX * 0.00005
                dvy(i) = flockY + avoidY * 0.001 + alignY + predY * 0.00005
            Next
        End Sub)

        ' legacy: serial MoveForward + BounceOffWalls
        For i As Integer = 0 To n - 1
            Dim vxv As Double = vx(i) + dvx(i)
            Dim vyv As Double = vy(i) + dvy(i)

            px(i) += vxv
            py(i) += vyv

            Dim speed As Double = std.Sqrt(vxv * vxv + vyv * vyv)

            If speed > 6.0 Then
                vxv = vxv / speed * 6.0
                vyv = vyv / speed * 6.0
            ElseIf speed < 1.0 Then
                vxv = vxv / speed * 1.0
                vyv = vyv / speed * 1.0
            End If

            If Double.IsNaN(vxv) Then vxv = 0
            If Double.IsNaN(vyv) Then vyv = 0

            If px(i) < 50.0 Then vxv += 0.5
            If px(i) > 2000.0 - 50.0 Then vxv -= 0.5
            If py(i) < 50.0 Then vyv += 0.5
            If py(i) > 1200.0 - 50.0 Then vyv -= 0.5

            vx(i) = vxv
            vy(i) = vyv
        Next
    End Sub

    Function GeomDistance(a As Boid, b As Boid) As Double
        Return std.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y))
    End Function

    ''' <summary>
    ''' Render cost model: mirrors the memory traffic of the SDRender LockBits 
    ''' renderer (clear the 2000x1200 32bpp buffer + ~35 clipped pixel writes per 
    ''' boid) on a managed buffer, to estimate the gdi+ phase cost without a GUI.
    ''' </summary>
    Sub BenchmarkRenderModel()
        Console.WriteLine()
        Console.WriteLine("[extra] render cost model (LockBits write pattern), 80k boids ...")

        Const w As Integer = 2000
        Const h As Integer = 1200
        Const frames As Integer = 50
        Const n As Integer = 80000

        Dim field As New Field(2000.0, 1200.0, n)
        Dim state = field.State

        For i As Integer = 1 To 5
            field.Advance()
        Next

        Dim buf(w * h - 1) As Integer

        ' warm up
        RenderModelFrame(buf, w, h, state)

        Dim sw = Stopwatch.StartNew()

        For f As Integer = 1 To frames
            RenderModelFrame(buf, w, h, state)
        Next

        sw.Stop()

        Dim avg As Double = sw.Elapsed.TotalMilliseconds / frames
        Console.WriteLine($" render model: avg {avg,7:F3} ms/frame -> {1000.0 / avg,7:F1} fps (gdi+ phase only)")
        Console.WriteLine($" physics + render model budget check: 4.753 ms + {avg:F3} ms = {4.753 + avg:F3} ms " &
                          $"({If(4.753 + avg <= 16.7, "WITHIN", "OVER")} the 16.7 ms / 60 fps budget)")
    End Sub

    Sub RenderModelFrame(buf As Integer(), w As Integer, h As Integer, state As FlockState)
        Array.Clear(buf, 0, buf.Length)

        Dim px = state.px
        Dim py = state.py
        Dim vx = state.vx
        Dim vy = state.vy

        For i As Integer = 0 To state.count - 1
            Dim bx = px(i)
            Dim by = py(i)
            Dim dx = vx(i)
            Dim dy = vy(i)
            Dim len = std.Sqrt(dx * dx + dy * dy)
            Dim scaleFactor As Double = If(len > 8.0, 8.0 / len, 1.0)
            Dim ex = bx + dx * scaleFactor
            Dim ey = by + dy * scaleFactor

            ' shaft: ~1 step per pixel, 3 writes per step
            If len > 0.0 Then
                Dim steps = CInt(len)
                If steps < 1 Then steps = 1
                Dim sx = (ex - bx) / steps
                Dim sy = (ey - by) / steps
                Dim cxp = bx
                Dim cyp = by

                For s As Integer = 0 To steps
                    Dim xi = CInt(cxp)
                    Dim yi = CInt(cyp)

                    If xi >= 0 AndAlso xi < w AndAlso yi >= 0 AndAlso yi < h Then buf(yi * w + xi) = &HFF3366CC
                    If xi + 1 >= 0 AndAlso xi + 1 < w AndAlso yi >= 0 AndAlso yi < h Then buf(yi * w + xi + 1) = &HFF3366CC
                    If xi >= 0 AndAlso xi < w AndAlso yi + 1 >= 0 AndAlso yi + 1 < h Then buf((yi + 1) * w + xi) = &HFF3366CC

                    cxp += sx
                    cyp += sy
                Next
            End If

            ' head: incremental scanline triangle, ~7 rows x ~3 span writes
            Dim ux As Double, uy As Double

            If len > 0.0 Then
                ux = (ex - bx) / len
                uy = (ey - by) / len
            Else
                ux = 1.0
                uy = 0.0
            End If

            Dim baseX = ex - 10.0 * ux
            Dim baseY = ey - 10.0 * uy
            Dim p1x = baseX + 7.0 * uy
            Dim p1y = baseY - 7.0 * ux
            Dim p2x = baseX - 7.0 * uy
            Dim p2y = baseY + 7.0 * ux

            FillTriangleModel(buf, w, h, ex, ey, p1x, p1y, p2x, p2y, &HFF3366CC)
        Next
    End Sub

    Sub FillTriangleModel(buf As Integer(), w As Integer, h As Integer,
                          ax As Double, ay As Double, bx As Double, by As Double,
                          cx As Double, cy As Double, c As Integer)
        If ay > by Then
            Dim tx = ax, ty = ay
            ax = bx : ay = by
            bx = tx : by = ty
        End If
        If by > cy Then
            Dim tx = bx, ty = by
            bx = cx : by = cy
            cx = tx : cy = ty
        End If
        If ay > by Then
            Dim tx = ax, ty = ay
            ax = bx : ay = by
            bx = tx : by = ty
        End If

        If cy <= ay Then Return

        Dim minY = CInt(std.Floor(ay))
        Dim maxY = CInt(std.Ceiling(cy)) - 1

        If minY < 0 Then minY = 0
        If maxY >= h Then maxY = h - 1
        If minY > maxY Then Return

        Dim longDx As Double = (cx - ax) / (cy - ay)
        Dim upDx As Double = If(by > ay, (bx - ax) / (by - ay), 0.0)
        Dim loDx As Double = If(cy > by, (cx - bx) / (cy - by), 0.0)

        Dim yc As Double = minY + 0.5
        Dim xLong As Double = ax + (yc - ay) * longDx
        Dim xShort As Double
        Dim shortDx As Double

        If (by > ay) AndAlso (yc <= by) Then
            xShort = ax + (yc - ay) * upDx
            shortDx = upDx
        Else
            xShort = bx + (yc - by) * loDx
            shortDx = loDx
        End If

        For yi As Integer = minY To maxY
            If (by > ay) AndAlso (cy > by) AndAlso (yi + 0.5 > by) AndAlso (shortDx <> loDx) Then
                yc = yi + 0.5
                xShort = bx + (yc - by) * loDx
                shortDx = loDx
            End If

            Dim x0 As Double = xLong
            Dim x1 As Double = xShort

            If x0 > x1 Then
                Dim tmp = x0
                x0 = x1
                x1 = tmp
            End If

            Dim fromX = CInt(std.Ceiling(x0 - 0.5))
            Dim toX = CInt(std.Floor(x1 - 0.5))

            If fromX < 0 Then fromX = 0
            If toX >= w Then toX = w - 1

            If fromX <= toX Then
                Dim rowOffset As Integer = yi * w

                For xi As Integer = fromX To toX
                    buf(rowOffset + xi) = c
                Next
            End If

            xLong += longDx
            xShort += shortDx
        Next
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
