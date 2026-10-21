Imports std = System.Math

Namespace Distributions.LHS

    ''' <summary>
    ''' The design quality optimization strategy of the <see cref="LatinHypercubeSampler"/>.
    ''' </summary>
    Public Enum LhsOptimization
        ''' <summary>
        ''' No post optimization, the plain Latin hypercube design is returned. Only
        ''' the one dimensional projections are guaranteed to be uniform.
        ''' </summary>
        None = 0
        ''' <summary>
        ''' Improve the space filling of the low dimensional projections by random
        ''' coordinate swaps that minimize the centered discrepancy, this is the
        ''' equivalent of the `optimization` option of the scipy #qmc sampler.
        ''' </summary>
        RandomCd = 1
    End Enum

    ''' <summary>
    ''' Post optimization of a Latin hypercube design by the random coordinate
    ''' descent that minimizes the centered discrepancy of the low dimensional
    ''' projections (the `random-cd` strategy).
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Swapping the coordinates of two rows inside of one column keeps the one
    ''' dimensional stratification intact, so the LHS property of the design is
    ''' never destroyed by the optimizer.
    ''' </para>
    ''' <para>
    ''' The optimizer is a heuristic: it does not guarantee a global minimum, but
    ''' the discrepancy of the objective projection never increases.
    ''' </para>
    ''' </remarks>
    Public Module LhsOptimizer

        ''' <summary>
        ''' Minimize the centered discrepancy of the low dimensional projections by
        ''' random coordinate swaps, the input design is never modified.
        ''' </summary>
        ''' <param name="sample">the `N x d` Latin hypercube design in `[0,1)`</param>
        ''' <param name="rng">the random number generator that drives the swaps</param>
        ''' <param name="iterations">the number of candidate swaps, a larger value gives a better result</param>
        ''' <param name="objectiveDimensions">
        ''' the number of leading dimensions of the objective projection, `2` by default
        ''' because the one dimensional projection is already perfect for a LHS design.
        ''' </param>
        ''' <returns>the optimized copy of the design</returns>
        Public Function Optimize(sample As Double()(), rng As Random, Optional iterations As Integer = 100, Optional objectiveDimensions As Integer? = Nothing) As Double()()
            Dim allDims As Integer() = LhsDiscrepancy.ResolveDimensions(sample, Nothing)
            Dim n As Integer = sample.Length
            Dim optimized As Double()() = LatinHypercube.CloneSamples(sample)

            Dim nObjectives As Integer = If(objectiveDimensions Is Nothing, std.Min(2, allDims.Length), std.Min(objectiveDimensions.Value, allDims.Length))

            If iterations < 1 OrElse nObjectives < 1 OrElse n < 3 Then
                Return optimized
            End If

            Dim dims As Integer() = New Integer(nObjectives - 1) {}

            For k As Integer = 0 To nObjectives - 1
                dims(k) = allDims(k)
            Next

            Dim factors As Double() = LhsDiscrepancy.RowFactors(optimized, dims)
            Dim kernels As Double() = LhsDiscrepancy.RowKernels(optimized, dims)
            Dim termA As Double = SumOf(factors)
            Dim termB As Double = SumOf(kernels)
            Dim best As Double = Objective(termA, termB, n)

            For iter As Integer = 0 To iterations - 1
                Dim dim As Integer = dims(rng.Next(dims.Length))
                Dim i1 As Integer = rng.Next(n)
                Dim i2 As Integer = (i1 + 1 + rng.Next(n - 1)) Mod n
                Dim v1 As Double = optimized(i1)(dim)
                Dim v2 As Double = optimized(i2)(dim)

                If v1 = v2 Then
                    Continue For
                End If

                optimized(i1)(dim) = v2
                optimized(i2)(dim) = v1

                Dim newA1 As Double = LhsDiscrepancy.FactorAt(optimized, dims, i1)
                Dim newA2 As Double = LhsDiscrepancy.FactorAt(optimized, dims, i2)
                Dim newB1 As Double = RowKernel(optimized, dims, i1)
                Dim newB2 As Double = RowKernel(optimized, dims, i2)
                Dim candidateA As Double = termA - factors(i1) - factors(i2) + newA1 + newA2
                Dim candidateB As Double = termB - kernels(i1) - kernels(i2) + newB1 + newB2
                Dim candidate As Double = Objective(candidateA, candidateB, n)

                If candidate < best Then
                    best = candidate
                    termA = candidateA
                    termB = candidateB
                    factors(i1) = newA1
                    factors(i2) = newA2
                    kernels(i1) = newB1
                    kernels(i2) = newB2
                Else
                    ' revert the rejected swap
                    optimized(i1)(dim) = v1
                    optimized(i2)(dim) = v2
                End If
            Next

            Return optimized
        End Function

        ''' <summary>
        ''' Minimize the centered discrepancy of the low dimensional projections with a reproducible seed.
        ''' </summary>
        ''' <param name="sample">the `N x d` Latin hypercube design in `[0,1)`</param>
        ''' <param name="seed">the random seed, a random seed is used when this parameter is not set</param>
        ''' <param name="iterations">the number of candidate swaps</param>
        ''' <param name="objectiveDimensions">the number of leading dimensions of the objective projection</param>
        ''' <returns>the optimized copy of the design</returns>
        Public Function Optimize(sample As Double()(), Optional seed As Integer? = Nothing, Optional iterations As Integer = 100, Optional objectiveDimensions As Integer? = Nothing) As Double()()
            Return Optimize(sample, LatinHypercube.NewRandom(seed), iterations, objectiveDimensions)
        End Function

        ''' <summary>
        ''' The kernel sum of one sample row against all of the sample rows.
        ''' </summary>
        ''' <param name="sample">the design matrix</param>
        ''' <param name="columns">the projected columns, must be resolved already</param>
        ''' <param name="row">the sample row index</param>
        ''' <returns>sum_j prod_k g(x_ik, x_jk)</returns>
        Friend Function RowKernel(sample As Double()(), columns As Integer(), row As Integer) As Double
            Dim sum As Double = 0

            For j As Integer = 0 To sample.Length - 1
                sum += LhsDiscrepancy.KernelAt(sample, columns, row, j)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' The part of the discrepacy that the optimizer minimizes, the constant
        ''' `(13/12)^d` term is dropped because it does not affect the ordering.
        ''' </summary>
        Friend Function Objective(termA As Double, termB As Double, n As Integer) As Double
            Return -2.0 * termA / n + termB / (CDbl(n) * n)
        End Function

        Friend Function SumOf(values As Double()) As Double
            Dim sum As Double = 0

            For i As Integer = 0 To values.Length - 1
                sum += values(i)
            Next

            Return sum
        End Function

    End Module

End Namespace
