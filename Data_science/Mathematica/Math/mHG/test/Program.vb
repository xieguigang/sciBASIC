Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports mHGImpl = Microsoft.VisualBasic.Math.Statistics.Hypothesis.mHG

''' <summary>
''' The demo/test entry of the ``mHG.vbproj`` library.
'''
''' The golden expected values used below are taken from the official unit test suite of the
''' CRAN R package ``mHG`` (``tests/testthat/test-mHG.R``, author Kobi Perl), which is the
''' reference implementation that this library has been ported from. In addition to those
''' golden cases, the independent reference implementation (see <see cref="ReferenceImpl"/>)
''' is used to cross validate both the statistic and the exact permutation p-value.
''' </summary>
Module Program

    Private ReadOnly failures As New List(Of String)
    Private testCount As Integer = 0

    Sub Main(args As String())
        Console.WriteLine("=================================================================")
        Console.WriteLine(" mHG (minimum Hypergeometric test) - demo && test suite")
        Console.WriteLine(" library   : Microsoft.VisualBasic.Math.Statistics.Hypothesis.mHG")
        Console.WriteLine(" reference : R package ``mHG`` (Kobi Perl / Eden 2007, CRAN)")
        Console.WriteLine("=================================================================")
        Console.WriteLine()

        Call runRatioTests()
        Call runHGRowTests()
        Call runSeparationLineTests()
        Call runPiRTests()
        Call runStatisticGoldenTests()
        Call runPValueGoldenTests()
        Call runRandomStatisticTests()
        Call runBruteForcePValueTests()
        Call runEnrichmentDemo()

        Console.WriteLine()
        Console.WriteLine("-----------------------------------------------------------------")

        If failures.Count = 0 Then
            Console.WriteLine($" ALL {testCount} CHECKS PASSED")
        Else
            Console.WriteLine($" {failures.Count} / {testCount} CHECKS FAILED:")
            Console.WriteLine()

            For Each f As String In failures
                Console.WriteLine("   - " & f)
            Next

            Environment.ExitCode = 1
        End If

        Console.WriteLine("-----------------------------------------------------------------")
    End Sub

#Region "Test sections"

    ''' <summary>
    ''' 1. the two recurrence ratios must be consistent with an independently computed
    ''' hypergeometric PDF ratio.
    ''' </summary>
    Private Sub runRatioTests()
        section("1. d_ratio / v_ratio  vs  an independent hypergeometric PDF ratio")

        Const numBalls As Integer = 100
        Const numBlack As Integer = 40

        For draws As Integer = 1 To numBalls - 1
            Dim lo As Integer = System.Math.Max(1, numBlack + draws - numBalls + 1)
            Dim hi As Integer = System.Math.Min(draws, numBlack)

            For hits As Integer = lo To hi
                Dim dPrev As Double = ReferenceImpl.HypergeometricPmf(hits - 1, numBalls, numBlack, draws - 1)
                Dim pmf As Double = ReferenceImpl.HypergeometricPmf(hits, numBalls, numBlack, draws)

                If dPrev > 1.0E-200 Then
                    checkDouble($"d_ratio(n={draws}, b={hits})",
                                pmf / dPrev, mHGImpl.d_ratio(draws, hits, numBalls, numBlack), 1.0E-9)
                End If

                Dim vPrev As Double = ReferenceImpl.HypergeometricPmf(hits, numBalls, numBlack, draws - 1)

                If vPrev > 1.0E-200 Then
                    checkDouble($"v_ratio(n={draws}, b={hits})",
                                pmf / vPrev, mHGImpl.v_ratio(draws, hits, numBalls, numBlack), 1.0E-9)
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' 2. both the iterative and the recursive HG row calculators must produce the exact
    ''' hypergeometric PDF row (the ``HG row calculation is consistent with R`` test case
    ''' of the R package).
    ''' </summary>
    Private Sub runHGRowTests()
        section("2. HG_row_ncalc.iter / .recur  vs  the independent dhyper row")

        Const N As Integer = 100
        Const B As Integer = 40

        ' (n - m) is small here, so the library dispatcher would select ``iter``
        checkHGRow("m=90, n=99, b_n=20", 90, 99, 20, N, B)
        checkHGRow("m=5,  n=99, b_n=15", 5, 99, 15, N, B)
        checkHGRow("m=5,  n=6,  b_n=3", 5, 6, 3, N, B)

        ' (n - m) is large here, which is the branch taken by ``recur``
        checkHGRow("m=1,  n=80, b_n=30", 1, 80, 30, N, B)
        checkHGRow("m=20, n=100, b_n=5", 20, 100, 5, N, B)
    End Sub

    ''' <summary>
    ''' 3. the R separation line boundary cases (``R separation line, nmax = 0``).
    ''' </summary>
    Private Sub runSeparationLineTests()
        section("3. R_separation_linecalc boundary cases (N=20, B=5, n_max=0)")

        Dim line0 As Vector = mHGImpl.R_separation_linecalc(0.0, 20, 5, 0)
        checkInt("separation line length (p=0, n_max=0)", 6, line0.Length)

        For i As Integer = 0 To line0.Length - 1
            checkDouble($"separation line (p=0, n_max=0) [{i}]", 0, line0(i), 0)
        Next

        Dim line1 As Vector = mHGImpl.R_separation_linecalc(1.0, 20, 5, 0)
        checkInt("separation line length (p=1, n_max=0)", 6, line1.Length)

        For i As Integer = 0 To line1.Length - 1
            checkDouble($"separation line (p=1, n_max=0) [{i}]", 1, line1(i), 0)
        Next
    End Sub

    ''' <summary>
    ''' 4. the ``pi_r`` matrix layout (the ``pi_r calculation`` predetermined scenarios of
    ''' the R package). The expected matrices below are written with the 0-based layout used
    ''' by <see cref="Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix"/>, i.e.
    ''' the R cell ``pi_r[w + 2, b + 2]`` is found at ``pi_r(w + 1, b + 1)`` here.
    ''' </summary>
    Private Sub runPiRTests()
        section("4. pi_rcalc matrix layout (predetermined scenarios)")

        checkPiR("N=2, B=1, line=[0,0]", 2, 1,
                 {{0.0, 0.0, 0.0}, {0.0, 1.0, 0.5}, {0.0, 0.5, 1.0}},
                 {0, 0})

        checkPiR("N=3, B=2, line=[0,0,0]", 3, 2,
                 {{0.0, 0.0, 0.0, 0.0},
                  {0.0, 1.0, 2.0 / 3.0, 1.0 / 3.0},
                  {0.0, 1.0 / 3.0, 2.0 / 3.0, 1.0}},
                 {0, 0, 0})

        checkPiR("N=10, B=3, line=[7,7,7,7]", 10, 3,
                 zeros(9, 5),
                 {7, 7, 7, 7})

        checkPiR("N=3, B=1, line=[0,2]", 3, 1,
                 {{0.0, 0.0, 0.0},
                  {0.0, 1.0, 0.0},
                  {0.0, 2.0 / 3.0, 0.0},
                  {0.0, 1.0 / 3.0, 1.0 / 3.0}},
                 {0, 2})

        checkPiR("N=3, B=2, line=[0,1,1]", 3, 2,
                 {{0.0, 0.0, 0.0, 0.0},
                  {0.0, 1.0, 0.0, 0.0},
                  {0.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0}},
                 {0, 1, 1})

        checkPiR("N=4, B=2, line=[0,1,1]", 4, 2,
                 {{0.0, 0.0, 0.0, 0.0},
                  {0.0, 1.0, 0.0, 0.0},
                  {0.0, 0.5, 1.0 / 3.0, 1.0 / 6.0},
                  {0.0, 1.0 / 6.0, 1.0 / 3.0, 0.5}},
                 {0, 1, 1})

        checkPiR("N=4, B=2, line=[0,1,2]", 4, 2,
                 {{0.0, 0.0, 0.0, 0.0},
                  {0.0, 1.0, 0.0, 0.0},
                  {0.0, 0.5, 1.0 / 3.0, 0.0},
                  {0.0, 1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0}},
                 {0, 1, 2})

        checkPiR("N=4, B=2, line=[0,2,2]", 4, 2,
                 {{0.0, 0.0, 0.0, 0.0},
                  {0.0, 1.0, 0.0, 0.0},
                  {0.0, 0.5, 0.0, 0.0},
                  {0.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0}},
                 {0, 2, 2})

        ' ``pi_r calculation, no R separation line, N = B``
        checkPiR("N=B=5, line=[0,0,0,0,0,0]", 5, 5,
                 piRDiagonalOnes(2, 7),
                 {0, 0, 0, 0, 0, 0})
    End Sub

    ''' <summary>
    ''' 5. the mHG statistic / threshold / exact p-value golden cases
    ''' (``mHG.test work, predetermined scenarios``).
    ''' </summary>
    Private Sub runStatisticGoldenTests()
        section("5. mHGtest golden cases (n_max = N)")

        checkCase({0, 0}, 2, 1, 0, 1.0)
        checkCase({0, 1}, 2, 1, 0, 1.0)
        checkCase({1, 0}, 2, 0.5, 1, 0.5)
        checkCase({1, 1}, 2, 1, 0, 1.0)
        checkCase({0, 0, 0}, 3, 1, 0, 1.0)
        checkCase({0, 0, 1}, 3, 1, 0, 1.0)
        checkCase({0, 1, 0}, 3, 2.0 / 3.0, 2, 2.0 / 3.0)
        checkCase({0, 1, 1}, 3, 1, 0, 1.0)
        checkCase({1, 0, 0}, 3, 1.0 / 3.0, 1, 1.0 / 3.0)
        checkCase({1, 0, 1}, 3, 2.0 / 3.0, 1, 2.0 / 3.0)
        checkCase({1, 1, 0}, 3, 1.0 / 3.0, 2, 1.0 / 3.0)
        checkCase({1, 1, 1}, 3, 1, 0, 1.0)

        section("5b. mHGtest golden cases (n_max = 1)")

        checkCase({0, 0, 0}, 1, 1, 0, 1.0)
        checkCase({0, 0, 1}, 1, 1, 0, 1.0)
        checkCase({0, 1, 0}, 1, 1, 0, 1.0)
        checkCase({0, 1, 1}, 1, 1, 0, 1.0)
        checkCase({1, 0, 0}, 1, 1.0 / 3.0, 1, 1.0 / 3.0)
        checkCase({1, 0, 1}, 1, 2.0 / 3.0, 1, 2.0 / 3.0)
        checkCase({1, 1, 0}, 1, 2.0 / 3.0, 1, 2.0 / 3.0)
        checkCase({1, 1, 1}, 1, 1, 0, 1.0)

        section("5c. mHGtest golden cases (n_max = 2)")

        checkCase({0, 0, 0}, 2, 1, 0, 1.0)
        checkCase({0, 0, 1}, 2, 1, 0, 1.0)
        checkCase({0, 1, 0}, 2, 2.0 / 3.0, 2, 2.0 / 3.0)
        checkCase({0, 1, 1}, 2, 1, 0, 1.0)
        checkCase({1, 0, 0}, 2, 1.0 / 3.0, 1, 1.0 / 3.0)
        checkCase({1, 0, 1}, 2, 2.0 / 3.0, 1, 2.0 / 3.0)
        checkCase({1, 1, 0}, 2, 1.0 / 3.0, 2, 1.0 / 3.0)
        checkCase({1, 1, 1}, 2, 1, 0, 1.0)
    End Sub

    ''' <summary>
    ''' 6. the exact permutation p-value golden cases
    ''' (``mHG.pval.calc, predetermined scenarios``).
    ''' </summary>
    Private Sub runPValueGoldenTests()
        section("6. mHGpvalcalc golden cases (n_max = N)")

        checkPValue(4, 2, 4, 1.0 / 6.0, 1.0 / 6.0)
        checkPValue(4, 2, 4, 0.5, 2.0 / 3.0)
        checkPValue(4, 2, 4, 5.0 / 6.0, 5.0 / 6.0)
        checkPValue(4, 2, 4, 0.0, 0.0)
        checkPValue(4, 2, 4, 1.0, 1.0)

        checkPValue(4, 3, 4, 0.25, 0.25)
        checkPValue(4, 3, 4, 0.5, 0.5)
        checkPValue(4, 3, 4, 0.75, 0.75)

        checkPValue(5, 3, 5, 0.1, 0.1)
        checkPValue(5, 3, 5, 0.3, 0.3)
        checkPValue(5, 3, 5, 2.0 / 5.0, 0.5)
        checkPValue(5, 3, 5, 3.0 / 5.0, 0.7)
        checkPValue(5, 3, 5, 0.7, 0.8)
        checkPValue(5, 3, 5, 0.9, 0.9)

        checkPValue(6, 2, 6, 1.0 / 15.0, 1.0 / 15.0)
        checkPValue(6, 2, 6, 0.2, 0.2)
        checkPValue(6, 2, 6, 1.0 / 3.0, 2.0 / 5.0)
        checkPValue(6, 2, 6, 2.0 / 5.0, 8.0 / 15.0)
        checkPValue(6, 2, 6, 0.6, 2.0 / 3.0)
        checkPValue(6, 2, 6, 2.0 / 3.0, 0.8)
        checkPValue(6, 2, 6, 0.8, 13.0 / 15.0)
        checkPValue(6, 2, 6, 14.0 / 15.0, 14.0 / 15.0)

        checkPValue(6, 3, 6, 1.0 / 20.0, 1.0 / 20.0)
        checkPValue(6, 3, 6, 0.2, 0.3)
        checkPValue(6, 3, 6, 0.5, 0.75)
        checkPValue(6, 3, 6, 0.8, 0.9)
        checkPValue(6, 3, 6, 19.0 / 20.0, 19.0 / 20.0)

        section("6b. mHGpvalcalc golden cases (n_max < N)")

        checkPValue(6, 2, 1, 1.0 / 3.0, 1.0 / 3.0)

        checkPValue(6, 2, 2, 1.0 / 15.0, 1.0 / 15.0)
        checkPValue(6, 2, 2, 1.0 / 3.0, 1.0 / 3.0)
        checkPValue(6, 2, 2, 0.6, 0.6)

        checkPValue(6, 2, 3, 1.0 / 15.0, 1.0 / 15.0)
        checkPValue(6, 2, 3, 0.2, 0.2)
        checkPValue(6, 2, 3, 1.0 / 3.0, 2.0 / 5.0)
        checkPValue(6, 2, 3, 0.6, 0.6)
        checkPValue(6, 2, 3, 0.8, 0.8)

        checkPValue(6, 2, 5, 1.0 / 15.0, 1.0 / 15.0)
        checkPValue(6, 2, 5, 0.2, 0.2)
        checkPValue(6, 2, 5, 1.0 / 3.0, 2.0 / 5.0)
        checkPValue(6, 2, 5, 2.0 / 5.0, 8.0 / 15.0)
        checkPValue(6, 2, 5, 0.6, 2.0 / 3.0)
        checkPValue(6, 2, 5, 2.0 / 3.0, 0.8)
        checkPValue(6, 2, 5, 0.8, 13.0 / 15.0)
        checkPValue(6, 2, 5, 14.0 / 15.0, 14.0 / 15.0)
    End Sub

    ''' <summary>
    ''' 7. the whole statistic pipeline (which internally picks between the iterative and the
    ''' recursive HG row calculation) must agree with the plain definition of the statistic.
    ''' </summary>
    Private Sub runRandomStatisticTests()
        section("7. mHGstatisticcalc  vs  the direct (non dynamic programming) definition")

        Dim rnd As New ReferenceImpl.Lcg(20260917L)

        For trial As Integer = 1 To 20
            Dim lambdas As Integer() = ReferenceImpl.RandomLambdas(100, 40, rnd)
            Dim expected = ReferenceImpl.mHGStatisticSimple(lambdas)
            Dim actual = mHGImpl.mHGstatisticcalc(vec(lambdas))

            checkDouble($"random(100, 40) #{trial} :: statistic",
                        expected.mhg, actual.mHG, 0.000000001)
            checkInt($"random(100, 40) #{trial} :: n", expected.n, CInt(actual.n))
            checkInt($"random(100, 40) #{trial} :: b", expected.b, CInt(actual.b))
        Next

        ' Only 2 hits separated by a long distance: (n - m) becomes much larger than
        ' 20 * log2(b_n), so the O(B * log B) recursion branch of HG_row_ncalc is taken.
        Dim sparse As Integer() = New Integer(499) {}
        sparse(0) = 1
        sparse(499) = 1

        Dim expectedSparse = ReferenceImpl.mHGStatisticSimple(sparse)
        Dim actualSparse = mHGImpl.mHGstatisticcalc(vec(sparse))

        checkDouble("sparse(500, 2) :: statistic", expectedSparse.mhg, actualSparse.mHG, 0.000000000001)
        checkInt("sparse(500, 2) :: n", expectedSparse.n, CInt(actualSparse.n))
        checkInt("sparse(500, 2) :: b", expectedSparse.b, CInt(actualSparse.b))
    End Sub

    ''' <summary>
    ''' 8. the dynamic programming p-value must equal the exact permutation p-value obtained
    ''' by brute force enumeration of all the ``C(N, B)`` rearrangements.
    ''' </summary>
    Private Sub runBruteForcePValueTests()
        section("8. mHGtest p-value  vs  brute force exact permutation p-value")

        checkBruteForce("N=3,  B=1", {1, 0, 0})
        checkBruteForce("N=3,  B=2", {1, 0, 1})
        checkBruteForce("N=4,  B=2", {1, 1, 0, 0})
        checkBruteForce("N=5,  B=2", {1, 0, 0, 0, 1})
        checkBruteForce("N=8,  B=3", {1, 0, 1, 0, 0, 1, 0, 0})
        checkBruteForce("N=10, B=3", {1, 0, 1, 1, 0, 0, 0, 0, 0, 0})
        checkBruteForce("N=12, B=4", {1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0})
        checkBruteForce("N=12, B=6", {1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0})
    End Sub

    ''' <summary>
    ''' 9. a realistic looking usage of the library: a GO term whose annotated genes are
    ''' enriched among the top ranked (differentially expressed) genes.
    ''' </summary>
    Private Sub runEnrichmentDemo()
        section("9. demo - GO term enrichment on a ranked gene list")

        Const N As Integer = 1000
        Const B As Integer = 100

        Dim rnd As New ReferenceImpl.Lcg(42L)
        Dim lambdas As Integer() = ReferenceImpl.EnrichedLambdas(N, B, rnd)

        Dim watch As Stopwatch = Stopwatch.StartNew()
        Dim result = mHGImpl.mHGtest.mHGtest(vec(lambdas))
        watch.Stop()

        Console.WriteLine($"   N (all genes)          = {N}")
        Console.WriteLine($"   B (annotated genes)    = {lambdas.Sum}")
        Console.WriteLine($"   mHG statistic          = {result.statistic("mHG")}")
        Console.WriteLine($"   threshold n            = {result.n}")
        Console.WriteLine($"   b_n                    = {result.b}")
        Console.WriteLine($"   exact p-value          = {result.pvalue}")
        Console.WriteLine($"   elapsed                = {watch.ElapsedMilliseconds} ms")

        checkTrue("demo :: p-value within [0, 1]", result.pvalue >= 0 AndAlso result.pvalue <= 1)
        checkTrue("demo :: statistic <= 1", result.statistic("mHG") <= 1)
        checkTrue("demo :: threshold n in [0, N]", result.n >= 0 AndAlso result.n <= N)
    End Sub

#End Region

#Region "Assertions"

    Private Sub section(title As String)
        Console.WriteLine("[" & title & "]")
    End Sub

    Private Sub checkDouble(name As String, expected As Double, actual As Double,
                            Optional tolerance# = 0.000000001)
        testCount += 1

        Dim diff# = System.Math.Abs(expected - actual)

        If diff > tolerance Then
            failures.Add($"{name}: expected {expected}, actual {actual} (diff={diff})")
        End If
    End Sub

    Private Sub checkInt(name As String, expected As Integer, actual As Integer)
        testCount += 1

        If expected <> actual Then
            failures.Add($"{name}: expected {expected}, actual {actual}")
        End If
    End Sub

    Private Sub checkTrue(name As String, condition As Boolean)
        testCount += 1

        If Not condition Then
            failures.Add($"{name}: condition not satisfied")
        End If
    End Sub

#End Region

#Region "Test helper functions"

    Private Function vec(ParamArray x As Integer()) As Vector
        Return New Vector(x.Select(Function(i) CDbl(i)))
    End Function

    Private Function zeros(rows As Integer, cols As Integer) As Double(,)
        Return New Double(rows - 1, cols - 1) {}
    End Function

    ''' <summary>
    ''' The expected ``pi_r`` of the ``N = B`` / no separation line scenario: everything is
    ''' 1 except the padding row/column 0.
    ''' </summary>
    Private Function piRDiagonalOnes(rows As Integer, cols As Integer) As Double(,)
        Dim m As Double(,) = zeros(rows, cols)

        For r As Integer = 1 To rows - 1
            For c As Integer = 1 To cols - 1
                m(r, c) = 1
            Next
        Next

        Return m
    End Function

    Private Sub checkHGRow(label As String, m As Integer, cutoff As Integer, b_n As Integer,
                           numBalls As Integer, numBlack As Integer)
        ' R: HG_row_m[1:b_n] <- dhyper(0:(b_n - 1), B, N - B, m)
        ' the row must hold b_n + 1 cells, because the diagonal recurrence also writes HG_row_m[b_n + 1]
        Dim rowM As New Vector(b_n + 1)

        For j As Integer = 0 To b_n - 1
            rowM(j) = ReferenceImpl.HypergeometricPmf(j, numBalls, numBlack, m)
        Next

        ' R: HG_row_n[1:(b_n + 1)] <- dhyper(0:b_n, B, N - B, n)
        Dim expected As New Vector(b_n + 1)

        For j As Integer = 0 To b_n
            expected(j) = ReferenceImpl.HypergeometricPmf(j, numBalls, numBlack, cutoff)
        Next

        ' NOTE: both calculators update the given row in place, hence the copies.
        Dim rowIter As Vector = mHGImpl.HG_row_ncalc.iter(New Vector(rowM), m, cutoff, b_n, numBalls, numBlack)
        Dim rowRecur As Vector = mHGImpl.HG_row_ncalc.recur(New Vector(rowM), m, cutoff, b_n, numBalls, numBlack)

        For j As Integer = 0 To b_n
            checkDouble($"{label} :: iter(j={j})", expected(j), rowIter(j), 1.0E-9)
            checkDouble($"{label} :: recur(j={j})", expected(j), rowRecur(j), 1.0E-9)
        Next
    End Sub

    Private Sub checkPiR(label As String, N As Integer, B As Integer,
                         expected As Double(,), line As Integer())
        Dim actual = mHGImpl.pi_rcalc(N, B, vec(line))

        checkInt($"{label} :: rows", expected.GetLength(0), actual.RowDimension)
        checkInt($"{label} :: cols", expected.GetLength(1), actual.ColumnDimension)

        For r As Integer = 0 To expected.GetLength(0) - 1
            For c As Integer = 0 To expected.GetLength(1) - 1
                checkDouble($"{label} :: [{r}, {c}]", expected(r, c), actual(r, c), 1.0E-9)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Runs one mHG test golden case and validates the statistic, the optimal threshold,
    ''' the ``b`` value and the exact p-value.
    ''' </summary>
    Private Sub checkCase(lambdas As Integer(), n_max As Integer, expectedStatistic As Double,
                          expectedN As Integer, expectedPValue As Double)
        Dim result = mHGImpl.mHGtest.mHGtest(vec(lambdas), n_max)
        Dim text$ = String.Join(", ", lambdas.Select(Function(i) i.ToString()).ToArray())
        Dim label$ = $"mHGtest(lambdas=[{text}], n_max={n_max})"

        checkDouble(label & " :: statistic", expectedStatistic, result.statistic("mHG"), 1.0E-9)
        checkDouble(label & " :: p-value", expectedPValue, result.pvalue, 1.0E-9)
        checkInt(label & " :: n", expectedN, result.n)
        checkDouble(label & " :: parameters[N]", lambdas.Length, result.parameters("N"), 0)
        checkDouble(label & " :: parameters[B]", lambdas.Sum, result.parameters("B"), 0)
        checkDouble(label & " :: parameters[n_max]", n_max, result.parameters("n_max"), 0)

        Dim expectedB As Double = 0

        For i As Integer = 0 To expectedN - 1
            expectedB += lambdas(i)
        Next

        checkDouble(label & " :: b", expectedB, result.b, 1.0E-9)
    End Sub

    Private Sub checkPValue(N As Integer, B As Integer, n_max As Integer,
                            statistic As Double, expected As Double)
        Dim actual# = mHGImpl.mHGpvalcalc(statistic, N, CDbl(B), CDbl(n_max))

        checkDouble($"mHGpvalcalc(N={N}, B={B}, n_max={n_max}, statistic={statistic})",
                    expected, actual, 1.0E-9)
    End Sub

    Private Sub checkBruteForce(label As String, lambdas As Integer())
        Dim expected# = ReferenceImpl.ExactPermutationPValue(lambdas)
        Dim result = mHGImpl.mHGtest.mHGtest(vec(lambdas))

        checkDouble($"{label} :: p-value vs brute force", expected, result.pvalue, 1.0E-9)
    End Sub

#End Region

End Module
