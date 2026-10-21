Imports std = System.Math

Namespace Distributions.LHS

    ''' <summary>
    ''' Generators of the Latin hypercube designs over the unit hypercube `[0,1)^d`.
    ''' </summary>
    ''' <remarks>
    ''' The sample matrix layout is `samples(i)(j)` where `i` is the sample row
    ''' (0 .. N - 1) and `j` is the dimension (0 .. d - 1), so the design is
    ''' returned as a `N x d` row orientated matrix.
    ''' </remarks>
    Public Module LatinHypercube

        ''' <summary>
        ''' Generate a random (jittered) Latin hypercube design of
        ''' <paramref name="nSamples"/> points in <paramref name="nDims"/> dimensions.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, N</param>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="rng">the random number generator that drives the permutations and the jitters</param>
        ''' <returns>
        ''' a `N x d` matrix where every column is a perfect stratification of
        ''' `[0,1)`: each of the N strata `[i/N, (i+1)/N)` holds exactly one point.
        ''' </returns>
        ''' <remarks>
        ''' For every dimension a random permutation of `0 .. N-1` is drawn and the
        ''' sample value becomes `(perm(i) + u) / N` with `u` uniform in `[0,1)`,
        ''' which is the algorithm described in the module readme.
        ''' </remarks>
        Public Function RandomUnit(nSamples As Integer, nDims As Integer, rng As Random) As Double()()
            Call CheckSize(nSamples, nDims, "RandomUnit")

            Dim sample As Double()() = NewSamples(nSamples, nDims)
            Dim ranks As Integer() = New Integer(nSamples - 1) {}

            For axis As Integer = 0 To nDims - 1
                Call PermuteRanks(ranks, rng)

                For i As Integer = 0 To nSamples - 1
                    ' the jitter keeps the point strictly inside of its own stratum
                    sample(i)(axis) = (ranks(i) + rng.NextDouble()) / nSamples
                Next
            Next

            Return sample
        End Function

        ''' <summary>
        ''' Generate a random Latin hypercube design with a reproducible seed.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, N</param>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="seed">the random seed, a random seed is used when this parameter is not set</param>
        ''' <returns>a `N x d` Latin hypercube design in `[0,1)`</returns>
        ''' <remarks>
        ''' The same <paramref name="seed"/> always produces the very same design,
        ''' which is required for reproducible experiments.
        ''' </remarks>
        Public Function RandomUnit(nSamples As Integer, nDims As Integer, Optional seed As Integer? = Nothing) As Double()()
            Return RandomUnit(nSamples, nDims, NewRandom(seed))
        End Function

        ''' <summary>
        ''' Generate a median (centered) Latin hypercube design: the point of each
        ''' stratum is placed in the middle of the stratum instead of a random position.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, N</param>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="rng">the random number generator that drives the permutations</param>
        ''' <returns>a `N x d` design where each column values are `{0.5, 1.5, .., N-0.5} / N` in a random order</returns>
        ''' <remarks>
        ''' A median design has no random jitter, hence it is perfectly balanced in
        ''' every one dimensional projection and it is the cheapest way to get a
        ''' deterministic, reproducible design.
        ''' </remarks>
        Public Function Median(nSamples As Integer, nDims As Integer, rng As Random) As Double()()
            Call CheckSize(nSamples, nDims, "Median")

            Dim sample As Double()() = NewSamples(nSamples, nDims)
            Dim ranks As Integer() = New Integer(nSamples - 1) {}

            For axis As Integer = 0 To nDims - 1
                Call PermuteRanks(ranks, rng)

                For i As Integer = 0 To nSamples - 1
                    sample(i)(axis) = (ranks(i) + 0.5) / nSamples
                Next
            Next

            Return sample
        End Function

        ''' <summary>
        ''' Generate a median (centered) Latin hypercube design with a reproducible seed.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, N</param>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="seed">the random seed, a random seed is used when this parameter is not set</param>
        ''' <returns>a `N x d` median Latin hypercube design in `[0,1)`</returns>
        Public Function Median(nSamples As Integer, nDims As Integer, Optional seed As Integer? = Nothing) As Double()()
            Return Median(nSamples, nDims, NewRandom(seed))
        End Function

        ''' <summary>
        ''' Generate a strength 2 Latin hypercube design from an orthogonal array,
        ''' so that the two dimensional projections are (nearly) uniform as well.
        ''' </summary>
        ''' <param name="nSamples">
        ''' the number of samples, it must be the square of a prime number, `N = p^2`.
        ''' </param>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="rng">the random number generator that drives the within-level permutations</param>
        ''' <returns>a `N x d` strength 2 Latin hypercube design in `[0,1)`</returns>
        ''' <exception cref="ArgumentException">the number of samples is not the square of a prime number</exception>
        ''' <remarks>
        ''' The orthogonal array `OA(p^2, p+1, p, 2)` is built from the row index
        ''' `(i,j)` in `{0..p-1} x {0..p-1}`: the first column is `j`, the remaining
        ''' `p` columns are `(i + k*j) mod p` for `k = 0 .. p-1`. Every level of a
        ''' column shows up exactly `p` times, so the `p` members of one level are
        ''' mapped onto the `p` distinct strata `[level*p, level*p + p)` through a
        ''' random permutation, which keeps the one dimensional stratification while
        ''' any pair of distinct columns covers each of the `p x p` cells of the
        ''' two dimensional projection exactly once.
        ''' <para>
        ''' Only the first `p+1` dimensions own an own orthogonal array column; the
        ''' remaining dimensions reuse those columns cyclically. Their one dimensional
        ''' stratification stays perfect, but the strength 2 property only holds for
        ''' the dimension pairs that use distinct columns.
        ''' </para>
        ''' </remarks>
        Public Function StrengthTwo(nSamples As Integer, nDims As Integer, rng As Random) As Double()()
            Call CheckSize(nSamples, nDims, "StrengthTwo")

            Dim p As Integer = PrimeSquareRoot(nSamples)
            Dim sample As Double()() = NewSamples(nSamples, nDims)
            Dim symbols As Integer() = New Integer(nSamples - 1) {}
            Dim ranks As Integer() = New Integer(nSamples - 1) {}
            Dim counts As Integer() = New Integer(p - 1) {}
            Dim order As Integer() = New Integer(nSamples - 1) {}
            Dim offset As Integer() = New Integer(p) {}
            Dim numberOfColumns As Integer = p + 1

            For axis As Integer = 0 To nDims - 1
                Dim col As Integer = axis Mod numberOfColumns

                For row As Integer = 0 To nSamples - 1
                    Dim i As Integer = row \ p
                    Dim j As Integer = row Mod p

                    If col = 0 Then
                        symbols(row) = j
                    Else
                        symbols(row) = (i + (col - 1) * j) Mod p
                    End If
                Next

                Call GroupByLevel(symbols, p, counts, order, offset)

                ' the p members of one level are spread over p distinct strata randomly
                For level As Integer = 0 To p - 1
                    Call ShuffleSegment(order, offset(level), p, rng)
                Next

                For level As Integer = 0 To p - 1
                    For t As Integer = offset(level) To offset(level) + p - 1
                        ranks(order(t)) = level * p + (t - offset(level))
                    Next
                Next

                For i As Integer = 0 To nSamples - 1
                    sample(i)(axis) = (ranks(i) + rng.NextDouble()) / nSamples
                Next
            Next

            Return sample
        End Function

        ''' <summary>
        ''' Generate a strength 2 Latin hypercube design with a reproducible seed.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, it must be the square of a prime number</param>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="seed">the random seed, a random seed is used when this parameter is not set</param>
        ''' <returns>a `N x d` strength 2 Latin hypercube design in `[0,1)`</returns>
        Public Function StrengthTwo(nSamples As Integer, nDims As Integer, Optional seed As Integer? = Nothing) As Double()()
            Return StrengthTwo(nSamples, nDims, NewRandom(seed))
        End Function

        ''' <summary>
        ''' Check that the given design is a valid Latin hypercube: every column must
        ''' own exactly one point inside each of the `N` equal width strata of `[0,1)`.
        ''' </summary>
        ''' <param name="sample">the `N x d` design to be validated</param>
        ''' <returns>true if every one dimensional projection of the design is stratified</returns>
        Public Function IsLatinHypercube(sample As Double()()) As Boolean
            If sample Is Nothing OrElse sample.Length = 0 Then
                Return False
            End If

            Dim n As Integer = sample.Length
            Dim nDims As Integer = If(sample(0) Is Nothing, 0, sample(0).Length)

            If nDims = 0 Then
                Return False
            End If

            Dim seen As Boolean() = New Boolean(n - 1) {}

            For axis As Integer = 0 To nDims - 1
                For i As Integer = 0 To n - 1
                    If sample(i) Is Nothing OrElse sample(i).Length <> nDims Then
                        Return False
                    End If

                    Dim x As Double = sample(i)(axis)

                    If x < 0 OrElse x >= 1 Then
                        Return False
                    End If

                    Dim cell As Integer = CInt(std.Floor(x * n))

                    ' numeric guard: a value extremely close to 1 may round up to N
                    If cell >= n Then
                        cell = n - 1
                    End If
                    If seen(cell) Then
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
        ''' Allocate a `N x d` sample matrix with all of the row arrays initialised.
        ''' </summary>
        Friend Function NewSamples(nSamples As Integer, nDims As Integer) As Double()()
            Dim sample As Double()() = New Double(nSamples - 1)() {}

            For i As Integer = 0 To nSamples - 1
                sample(i) = New Double(nDims - 1) {}
            Next

            Return sample
        End Function

        ''' <summary>
        ''' Deep copy of a sample matrix, the optimizer never mutates its input.
        ''' </summary>
        Friend Function CloneSamples(sample As Double()()) As Double()()
            Dim n As Integer = sample.Length
            Dim copy As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                copy(i) = DirectCast(sample(i).Clone(), Double())
            Next

            Return copy
        End Function

        ''' <summary>
        ''' Re-validate the design shape before any computation.
        ''' </summary>
        Friend Sub CheckSize(nSamples As Integer, nDims As Integer, caller As String)
            If nSamples < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(nSamples), nSamples, $"{caller}: the number of samples must be a positive integer.")
            End If
            If nDims < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(nDims), nDims, $"{caller}: the number of dimensions must be a positive integer.")
            End If
        End Sub

        ''' <summary>
        ''' Create the random number generator for the reproducible entry points.
        ''' </summary>
        Friend Function NewRandom(seed As Integer?) As Random
            If seed Is Nothing Then
                Return New Random()
            Else
                Return New Random(seed.Value)
            End If
        End Function

        ''' <summary>
        ''' Fisher-Yates shuffle of the `0 .. N-1` ranks.
        ''' </summary>
        Friend Sub PermuteRanks(ByRef ranks As Integer(), rng As Random)
            For i As Integer = 0 To ranks.Length - 1
                ranks(i) = i
            Next

            For i As Integer = ranks.Length - 1 To 1 Step -1
                Dim j As Integer = rng.Next(i + 1)
                Dim tmp As Integer = ranks(i)

                ranks(i) = ranks(j)
                ranks(j) = tmp
            Next
        End Sub

        ''' <summary>
        ''' Fisher-Yates shuffle of the segment `order(start .. start+length-1)`.
        ''' </summary>
        Friend Sub ShuffleSegment(ByRef order As Integer(), start As Integer, length As Integer, rng As Random)
            For i As Integer = start + length - 1 To start + 1 Step -1
                Dim j As Integer = start + rng.Next(i - start + 1)
                Dim tmp As Integer = order(i)

                order(i) = order(j)
                order(j) = tmp
            Next
        End Sub

        ''' <summary>
        ''' Counting sort of the row indices by their symbol level, the segments of
        ''' one level are stored consecutively inside of <paramref name="order"/>.
        ''' </summary>
        ''' <param name="symbols">the level of each row, values are in `0 .. nLevels-1`</param>
        ''' <param name="nLevels">the number of levels</param>
        ''' <param name="counts">a `nLevels` length scratch buffer</param>
        ''' <param name="order">the row indices sorted by level, a `N` length buffer</param>
        ''' <param name="offset">a `nLevels+1` length buffer, the segment start of each level</param>
        Friend Sub GroupByLevel(symbols As Integer(), nLevels As Integer, counts As Integer(), order As Integer(), offset As Integer())
            For level As Integer = 0 To nLevels - 1
                counts(level) = 0
            Next

            For row As Integer = 0 To symbols.Length - 1
                counts(symbols(row)) += 1
            Next

            Dim cursor As Integer = 0

            For level As Integer = 0 To nLevels - 1
                offset(level) = cursor
                cursor += counts(level)
                counts(level) = offset(level)
            Next

            offset(nLevels) = cursor

            For row As Integer = 0 To symbols.Length - 1
                Dim level As Integer = symbols(row)
                order(counts(level)) = row
                counts(level) += 1
            Next
        End Sub

        ''' <summary>
        ''' Return `p` when the number of samples is `p^2` with `p` a prime number.
        ''' </summary>
        ''' <param name="nSamples">the number of samples</param>
        ''' <returns>the prime number `p`</returns>
        ''' <exception cref="ArgumentException">the number of samples is not the square of a prime number</exception>
        Public Function PrimeSquareRoot(nSamples As Integer) As Integer
            ' Long arithmetic: the square of 46341 already overflows the Integer type
            Dim p As Long = CLng(std.Floor(std.Sqrt(CDbl(nSamples))))

            While p > 0 AndAlso p * p > nSamples
                p -= 1
            End While
            While (p + 1) * (p + 1) <= nSamples
                p += 1
            End While

            If p * p <> nSamples Then
                Throw New ArgumentException($"the number of samples {nSamples} is not the square of a prime number, the strength 2 Latin hypercube requires N = p^2.", NameOf(nSamples))
            End If
            If Not IsPrime(CInt(p)) Then
                Throw New ArgumentException($"the number of samples {nSamples} = {p}^2 is not the square of a prime number, the strength 2 Latin hypercube requires N = p^2.", NameOf(nSamples))
            End If

            Return CInt(p)
        End Function

        ''' <summary>
        ''' A simple trial division primality test.
        ''' </summary>
        ''' <param name="value">the integer to be tested</param>
        ''' <returns>true if the value is a prime number</returns>
        Public Function IsPrime(value As Integer) As Boolean
            If value < 2 Then
                Return False
            End If
            If value Mod 2 = 0 Then
                Return value = 2
            End If

            Dim limit As Integer = CInt(std.Floor(std.Sqrt(value)))

            For d As Integer = 3 To limit Step 2
                If value Mod d = 0 Then
                    Return False
                End If
            Next

            Return True
        End Function

    End Module

End Namespace
