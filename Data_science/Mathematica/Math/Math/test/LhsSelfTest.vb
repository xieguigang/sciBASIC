' ============================================================================
' LhsSelfTest.vb — Latin hypercube sampling 自检
' ----------------------------------------------------------------------------
' 用法：dotnet run -- lhs
' ============================================================================

Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.VisualBasic.Math.Distributions.LHS
Imports std = System.Math

''' <summary>
''' 拉丁超立方采样模块的自检：分层性、可复现性、strength 2 正交性、
''' centered discrepancy 正确性、random-cd 优化、缩放到真实参数区间。
''' </summary>
Public Module LhsSelfTest

    Private pass As Integer = 0
    Private fail As Integer = 0

    Public Function RunAll() As Integer
        pass = 0
        fail = 0

        Console.WriteLine("==================================================")
        Console.WriteLine(" Latin hypercube sampling self test")
        Console.WriteLine("==================================================")
        Console.WriteLine()

        Call TestRandomUnit()
        Call TestMedian()
        Call TestReproducible()
        Call TestStrengthTwo()
        Call TestDiscrepancy()
        Call TestOptimizer()
        Call TestScale()
        Call TestSampler()
        Call TestPaperScenario()

        Console.WriteLine()
        Console.WriteLine("========================================")
        Console.WriteLine($"LHS self test: {pass} passed, {fail} failed, {pass + fail} total")
        Console.WriteLine("========================================")

        Return If(fail = 0, 0, 1)
    End Function

#Region "Assert helpers"

    Private Sub Check(name As String, condition As Boolean, Optional detail As String = Nothing)
        If condition Then
            pass += 1
            Console.WriteLine($"  [PASS] {name}")
        Else
            fail += 1
            Console.WriteLine($"  [FAIL] {name}{If(detail Is Nothing, "", " -> " & detail)}")
        End If
    End Sub

    Private Sub CheckClose(name As String, expected As Double, actual As Double, Optional tolerance As Double = 0.000000001)
        Call Check(name, std.Abs(expected - actual) <= tolerance, $"expected {expected}, actual {actual}")
    End Sub

    Private Sub CheckThrows(Of TException As Exception)(name As String, action As Action)
        Try
            Call action()

            fail += 1
            Console.WriteLine($"  [FAIL] {name} -> no exception was thrown")
        Catch ex As Exception
            If TypeOf ex Is TException Then
                pass += 1
                Console.WriteLine($"  [PASS] {name}")
            Else
                fail += 1
                Console.WriteLine($"  [FAIL] {name} -> unexpected {ex.GetType.Name}: {ex.Message}")
            End If
        End Try
    End Sub

#End Region

#Region "Design validators used by the assertions"

    ''' <summary>
    ''' all of the sample values must be inside of the unit interval
    ''' </summary>
    Private Function IsInsideUnit(design As Double()()) As Boolean
        For i As Integer = 0 To design.Length - 1
            For axis As Integer = 0 To design(i).Length - 1
                Dim x As Double = design(i)(axis)

                If x < 0 OrElse x >= 1 Then
                    Return False
                End If
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' every column must own exactly one point inside of each of the N strata
    ''' </summary>
    Private Function IsPerfectStrata(design As Double()(), n As Integer) As Boolean
        Dim nDims As Integer = design(0).Length
        Dim seen As Boolean() = New Boolean(n - 1) {}

        For axis As Integer = 0 To nDims - 1
            For i As Integer = 0 To n - 1
                Dim cell As Integer = CInt(std.Floor(design(i)(axis) * n))

                If cell < 0 OrElse cell >= n OrElse seen(cell) Then
                    Return False
                End If

                seen(cell) = True
            Next

            For i As Integer = 0 To n - 1
                seen(i) = False
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' every pair of columns must cover each of the p x p cells exactly once
    ''' </summary>
    Private Function ArePairsUniform(design As Double()(), p As Integer) As Boolean
        Dim n As Integer = design.Length
        Dim nDims As Integer = design(0).Length

        For a As Integer = 0 To nDims - 1
            For b As Integer = a + 1 To nDims - 1
                Dim cells As New HashSet(Of Long)()

                For i As Integer = 0 To n - 1
                    Dim ia As Integer = CInt(std.Floor(design(i)(a) * p))
                    Dim ib As Integer = CInt(std.Floor(design(i)(b) * p))

                    Call cells.Add(CLng(ia) * 100000 + ib)
                Next

                If cells.Count <> n Then
                    Console.WriteLine($"      pair ({a},{b}) covers {cells.Count} of the {n} cells")
                    Return False
                End If
            Next
        Next

        Return True
    End Function

    Private Function SameDesign(a As Double()(), b As Double()()) As Boolean
        If a.Length <> b.Length Then
            Return False
        End If

        For i As Integer = 0 To a.Length - 1
            If a(i).Length <> b(i).Length Then
                Return False
            End If

            For axis As Integer = 0 To a(i).Length - 1
                If a(i)(axis) <> b(i)(axis) Then
                    Return False
                End If
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' the sorted values of each column must be {0.5, 1.5, .., N-0.5} / N
    ''' </summary>
    Private Function IsMedianDesign(design As Double()(), n As Integer) As Boolean
        Dim nDims As Integer = design(0).Length

        For axis As Integer = 0 To nDims - 1
            Dim values As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                values(i) = design(i)(axis)
            Next

            Call Array.Sort(values)

            For i As Integer = 0 To n - 1
                If std.Abs(values(i) - (i + 0.5) / n) > 0.000000001 Then
                    Return False
                End If
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' straight forward O(n^2 d) implementation of the same formula, used as a
    ''' independent reference implementation for the discrepancy assertions.
    ''' </summary>
    Private Function NaiveCentered(design As Double()(), columns As Integer()) As Double
        Dim n As Integer = design.Length
        Dim nDims As Integer = columns.Length
        Dim termA As Double = 0

        For i As Integer = 0 To n - 1
            Dim product As Double = 1

            For Each axis As Integer In columns
                Dim delta As Double = std.Abs(design(i)(axis) - 0.5)

                product *= 1.0 + 0.5 * delta - 0.5 * delta * delta
            Next

            termA += product
        Next

        Dim termB As Double = 0

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To n - 1
                Dim product As Double = 1

                For Each axis As Integer In columns
                    Dim xi As Double = design(i)(axis)
                    Dim xj As Double = design(j)(axis)

                    product *= 1.0 + 0.5 * std.Abs(xi - 0.5) + 0.5 * std.Abs(xj - 0.5) - 0.5 * std.Abs(xi - xj)
                Next

                termB += product
            Next
        Next

        Return std.Sqrt(std.Pow(13.0 / 12.0, nDims) - 2.0 * termA / n + termB / (CDbl(n) * n))
    End Function

    Private Function RandomMonteCarlo(n As Integer, nDims As Integer, seed As Integer) As Double()()
        Dim rng As New System.Random(seed)
        Dim design As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            design(i) = New Double(nDims - 1) {}

            For axis As Integer = 0 To nDims - 1
                design(i)(axis) = rng.NextDouble()
            Next
        Next

        Return design
    End Function

#End Region

    Private Sub TestRandomUnit()
        Console.WriteLine("--- random LHS (分层 + 跨维随机配对) ---")

        Dim design = LatinHypercube.RandomUnit(10, 3, seed:=42)

        Call Check("shape is 10 x 3", design.Length = 10 AndAlso design(0).Length = 3)
        Call Check("values inside of [0,1)", IsInsideUnit(design))
        Call Check("every stratum owns exactly one point", IsPerfectStrata(design, 10))
        Call Check("IsLatinHypercube validator", LatinHypercube.IsLatinHypercube(design))

        Dim large = LatinHypercube.RandomUnit(1000, 5, seed:=1)
        Call Check("1000 x 5 design is stratified", LatinHypercube.IsLatinHypercube(large))

        Console.WriteLine()
    End Sub

    Private Sub TestMedian()
        Console.WriteLine("--- median (centered) LHS ---")

        Dim design = LatinHypercube.Median(7, 2, seed:=2026)

        Call Check("median design is stratified", LatinHypercube.IsLatinHypercube(design))
        Call Check("median design uses the stratum centers", IsMedianDesign(design, 7))

        Dim smallest As Double = design(0)(0)

        For i As Integer = 1 To design.Length - 1
            If design(i)(0) < smallest Then
                smallest = design(i)(0)
            End If
        Next

        Call CheckClose("the smallest value is the first stratum center", 0.5 / 7, smallest)

        Console.WriteLine()
    End Sub

    Private Sub TestReproducible()
        Console.WriteLine("--- reproducible seed ---")

        Dim a = LatinHypercube.RandomUnit(100, 4, seed:=7)
        Dim b = LatinHypercube.RandomUnit(100, 4, seed:=7)
        Dim c = LatinHypercube.RandomUnit(100, 4, seed:=8)

        Call Check("same seed gives the identical design", SameDesign(a, b))
        Call Check("different seed gives a different design", Not SameDesign(a, c))

        Dim m1 = LatinHypercube.Median(50, 3, seed:=11)
        Dim m2 = LatinHypercube.Median(50, 3, seed:=11)

        Call Check("median design is reproducible", SameDesign(m1, m2))

        Console.WriteLine()
    End Sub

    Private Sub TestStrengthTwo()
        Console.WriteLine("--- strength 2 (orthogonal array) LHS ---")

        Dim p As Integer = 7
        Dim n As Integer = p * p

        Call Check("prime square root of 49 is 7", LatinHypercube.PrimeSquareRoot(n) = p)
        Call Check("7 is prime", LatinHypercube.IsPrime(7))
        Call Check("6 is not prime", Not LatinHypercube.IsPrime(6))
        Call Check("1 is not prime", Not LatinHypercube.IsPrime(1))

        Dim design = LatinHypercube.StrengthTwo(n, p + 1, seed:=2026)

        Call Check("strength 2 design is stratified", LatinHypercube.IsLatinHypercube(design))
        Call Check("all 2D projections are uniform", ArePairsUniform(design, p))

        Dim wide = LatinHypercube.StrengthTwo(n, 12, seed:=1)
        Call Check("d > p+1 keeps the 1D stratification", LatinHypercube.IsLatinHypercube(wide))

        Call CheckThrows(Of ArgumentException)("N = 50 is rejected", Sub() LatinHypercube.StrengthTwo(50, 4, seed:=1))
        Call CheckThrows(Of ArgumentException)("N = 36 = 6^2 is rejected", Sub() LatinHypercube.StrengthTwo(36, 4, seed:=1))

        Console.WriteLine()
    End Sub

    Private Sub TestDiscrepancy()
        Console.WriteLine("--- centered discrepancy ---")

        Dim onePoint As Double()() = {New Double() {0.5}}

        Call CheckClose("CD of one centered point is sqrt(1/12)", std.Sqrt(1.0 / 12.0), LhsDiscrepancy.Centered(onePoint), 0.000000000001)

        Dim design = LatinHypercube.RandomUnit(20, 3, seed:=5)
        Dim all As Integer() = {0, 1, 2}
        Dim projection As Integer() = {0, 1}
        Dim cd = LhsDiscrepancy.Centered(design)

        Call CheckClose("full dimensional CD matches the reference", NaiveCentered(design, all), cd, 0.000000000001)
        Call CheckClose("projected CD matches the reference", NaiveCentered(design, projection), LhsDiscrepancy.Centered(design, projection), 0.000000000001)
        Call Check("CD of a 3D design differs from its 2D projection", std.Abs(cd - LhsDiscrepancy.Centered(design, projection)) > 0)

        ' a Latin hypercube design should be more uniform than a plain Monte Carlo design
        Dim n As Integer = 1000
        Dim lhs As Double()() = LatinHypercube.Median(n, 2, seed:=42)
        Dim mc As Double()() = RandomMonteCarlo(n, 2, 42)

        Call Check("LHS is more uniform than Monte Carlo",
                   LatinHypercubeSampler.MeasureDiscrepancy(lhs, 2) < LatinHypercubeSampler.MeasureDiscrepancy(mc, 2),
                   $"LHS CD={LatinHypercubeSampler.MeasureDiscrepancy(lhs, 2)}, MC CD={LatinHypercubeSampler.MeasureDiscrepancy(mc, 2)}")

        Call CheckThrows(Of ArgumentOutOfRangeException)("empty projection is rejected", Sub() LatinHypercubeSampler.MeasureDiscrepancy(design, 0))

        Console.WriteLine()
    End Sub

    Private Sub TestOptimizer()
        Console.WriteLine("--- random-cd optimization ---")

        Dim raw = LatinHypercube.RandomUnit(400, 4, seed:=42)
        Dim projection As Integer() = {0, 1}
        Dim before As Double = LhsDiscrepancy.Centered(raw, projection)
        Dim optimized = LhsOptimizer.Optimize(raw, seed:=42, iterations:=200, objectiveDimensions:=2)
        Dim after As Double = LhsDiscrepancy.Centered(optimized, projection)

        Call Check("optimization never increases the CD", after <= before, $"before {before}, after {after}")
        Call Check("optimization improves the CD", after < before, $"before {before}, after {after}")
        Call Check("optimized design keeps the stratification", LatinHypercube.IsLatinHypercube(optimized))
        Call Check("the input design is not modified", SameDesign(raw, LatinHypercube.RandomUnit(400, 4, seed:=42)))

        Dim noIterations = LhsOptimizer.Optimize(raw, seed:=42, iterations:=0)
        Call Check("zero iterations returns an untouched copy", SameDesign(noIterations, raw))

        Console.WriteLine($"      CD {before:G6} -> {after:G6}")
        Console.WriteLine()
    End Sub

    Private Sub TestScale()
        Console.WriteLine("--- scaling onto the parameter ranges ---")

        Dim unit As Double()() = {
            New Double() {0, 0},
            New Double() {0.5, 0.5}
        }
        Dim scaled = LhsScale.Scale(unit, New Double() {-1, -3}, New Double() {1, 2})

        Call CheckClose("lb is hit", -1, scaled(0)(0))
        Call CheckClose("lb[j] is hit", -3, scaled(0)(1))
        Call CheckClose("midpoint is the range center", 0, scaled(1)(0))
        Call CheckClose("uneven range midpoint", -0.5, scaled(1)(1))

        Dim uniform = LhsScale.Scale(unit, 0, 10)
        Call CheckClose("scalar bounds are broadcast", 5, uniform(1)(0))
        Call CheckClose("scalar bounds are broadcast (2)", 5, uniform(1)(1))

        Dim tiled = LhsScale.TileBounds(New Double() {-1, -3}, 4)
        Call Check("tile pattern repeats cyclically",
                   tiled.Length = 4 AndAlso tiled(0) = -1 AndAlso tiled(1) = -3 AndAlso tiled(2) = -1 AndAlso tiled(3) = -3)

        Dim logSample As Double()() = {
            New Double() {0},
            New Double() {0.5},
            New Double() {0.999999}
        }
        Dim logs = LhsScale.ScaleLog10(logSample, LhsScale.Fill(-1, 1), LhsScale.Fill(1, 1))

        Call CheckClose("log10 lower bound", 0.1, logs(0)(0), 0.0000000001)
        Call CheckClose("log10 midpoint", 1, logs(1)(0), 0.0000000001)
        Call Check("log10 upper bound", logs(2)(0) > 9.999 AndAlso logs(2)(0) < 10)

        Call CheckThrows(Of ArgumentException)("empty bounds are rejected", Sub() LhsScale.Scale(unit, New Double() {}, New Double() {1, 2}))
        Call CheckThrows(Of ArgumentException)("inverted bounds are rejected", Sub() LhsScale.Scale(unit, New Double() {1, 1}, New Double() {1, 2}))
        Call CheckThrows(Of ArgumentException)("tile pattern can not be empty", Sub() LhsScale.TileBounds(New Double() {}, 3))

        Console.WriteLine()
    End Sub

    Private Sub TestSampler()
        Console.WriteLine("--- sampler facade ---")

        Dim sampler As New LatinHypercubeSampler(12, 42)
        Dim design = sampler.Random(50)

        Call Check("sampler shape is 50 x 12", design.Length = 50 AndAlso design(0).Length = 12)
        Call Check("sampler design is stratified", LatinHypercube.IsLatinHypercube(design))
        Call Check("no discrepancy when the optimization is disabled", Double.IsNaN(sampler.LastDiscrepancy))

        Dim nextDesign = sampler.Random(50)
        Call Check("successive calls give independent designs", Not SameDesign(design, nextDesign))
        Call Check("the second design is stratified as well", LatinHypercube.IsLatinHypercube(nextDesign))

        Dim snapshot As New LatinHypercubeSampler(5, 99)
        Dim snapshot2 As New LatinHypercubeSampler(5, 99)
        Call Check("samplers with the same seed agree", SameDesign(snapshot.Random(64), snapshot2.Random(64)))

        Dim centered As New LatinHypercubeSampler(3, 1) With {.Center = True}
        Dim medianDesign = centered.Random(10)
        Call Check("Center = True generates the median design", IsMedianDesign(medianDesign, 10))

        Dim orthogonal As New LatinHypercubeSampler(5, 7) With {.Strength = 2}
        Dim orthDesign = orthogonal.Random(49)
        Call Check("Strength = 2 generates an orthogonal design", LatinHypercube.IsLatinHypercube(orthDesign) AndAlso ArePairsUniform(orthDesign, 7))
        Call CheckThrows(Of ArgumentException)("Strength = 2 rejects a non prime square sample count", Sub() orthogonal.Random(50))

        Dim optimized As New LatinHypercubeSampler(4, 42) With {
            .Optimization = LhsOptimization.RandomCd,
            .CdIterations = 100
        }
        Dim optimizedDesign = optimized.Random(200)

        Call Check("optimized design is stratified", LatinHypercube.IsLatinHypercube(optimizedDesign))
        Call Check("LastDiscrepancy is measured", Not Double.IsNaN(optimized.LastDiscrepancy))

        Call CheckThrows(Of ArgumentOutOfRangeException)("invalid strength is rejected",
            Sub()
                Dim bad As New LatinHypercubeSampler(3, 1)
                bad.Strength = 3
            End Sub)

        Dim matrix = sampler.RandomMatrix(50)

        Call Check("RandomMatrix owns the expected shape", matrix.RowDimension = 50 AndAlso matrix.ColumnDimension = 12)
        Call matrix.Dispose()

        Console.WriteLine()
    End Sub

    Private Sub TestPaperScenario()
        Console.WriteLine("--- paper scenario: 10000 x 12 inside of the log10 space ---")

        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim sampler As New LatinHypercubeSampler(nDims:=12, seed:=42)
        Dim unit = sampler.Random(10000)
        watch.Stop()

        Call Check("10000 x 12 shape", unit.Length = 10000 AndAlso unit(0).Length = 12)
        Call Check("10000 x 12 design is stratified", LatinHypercube.IsLatinHypercube(unit))
        Console.WriteLine($"      generated in {watch.ElapsedMilliseconds} ms")

        ' log10(k) inside of [-1, 1] and log10(K) inside of [-3, 2], alternating over the dimensions
        Dim logLb = LhsScale.TileBounds(New Double() {-1, -3}, 12)
        Dim logUb = LhsScale.TileBounds(New Double() {1, 2}, 12)
        Dim params = LhsScale.ScaleLog10(unit, logLb, logUb)
        Dim minValue As Double = Double.MaxValue
        Dim maxValue As Double = 0

        For i As Integer = 0 To params.Length - 1
            For axis As Integer = 0 To params(i).Length - 1
                If params(i)(axis) < minValue Then
                    minValue = params(i)(axis)
                End If
                If params(i)(axis) > maxValue Then
                    maxValue = params(i)(axis)
                End If
            Next
        Next

        Call Check("parameters stay inside of [0.001, 100]", minValue >= 0.001 AndAlso maxValue < 100, $"min {minValue}, max {maxValue}")
        Console.WriteLine($"      parameter range [{minValue:G4}, {maxValue:G4}]")

        ' each decade owns the same sampling density inside of the log space
        Dim decadeCounts As Integer() = New Integer(4) {}
        For i As Integer = 0 To params.Length - 1
            Dim exponent As Integer = CInt(std.Floor(std.Log10(params(i)(1))))

            If exponent >= -3 AndAlso exponent <= 1 Then
                decadeCounts(exponent + 3) += 1
            End If
        Next

        Dim expected As Double = params.Length / 5.0
        Dim balanced As Boolean = True

        For k As Integer = 0 To 4
            If decadeCounts(k) <= 0 OrElse std.Abs(decadeCounts(k) - expected) / expected > 0.25 Then
                balanced = False
            End If
        Next

        Call Check("every order of magnitude is sampled evenly", balanced, $"counts {String.Join(",", decadeCounts)}")

        Console.WriteLine()
    End Sub

End Module
