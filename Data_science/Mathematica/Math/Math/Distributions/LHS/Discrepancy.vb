Imports std = System.Math

Namespace Distributions.LHS

    ''' <summary>
    ''' Quality measures for a Latin hypercube design.
    ''' </summary>
    ''' <remarks>
    ''' The centered L2 discrepancy (Hickernell 1998) is the default quality
    ''' measure used by the quasi Monte Carlo (#qmc) module of scipy: a smaller
    ''' value means a more uniform distribution of the sample points, which
    ''' reflects the space filling quality of the two dimensional and higher
    ''' dimensional projections as well.
    ''' </remarks>
    Public Module LhsDiscrepancy

        ''' <summary>
        ''' The centered L2 discrepancy of the given design projected onto all of the dimensions.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="dims">the dimensions (columns) that should be projected, all of the dimensions are used when not set</param>
        ''' <returns>the centered discrepancy, smaller is better</returns>
        ''' <remarks>
        ''' The computation costs `O(n^2 * d)`, so the full dimensional measure
        ''' should be applied to designs with a moderate number of samples only.
        ''' </remarks>
        Public Function Centered(sample As Double()(), Optional dims As Integer() = Nothing) As Double
            Return std.Sqrt(std.Max(0.0, CenteredSquared(sample, dims)))
        End Function

        ''' <summary>
        ''' The squared centered L2 discrepancy of the given design.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="dims">the dimensions (columns) that should be projected, all of the dimensions are used when not set</param>
        ''' <returns>CD^2 = (13/12)^d - (2/n)*sum_i prod_k a(x_ik) + (1/n^2)*sum_ij prod_k g(x_ik, x_jk)</returns>
        Public Function CenteredSquared(sample As Double()(), Optional dims As Integer() = Nothing) As Double
            Dim columns As Integer() = ResolveDimensions(sample, dims)
            Dim n As Integer = sample.Length
            Dim termA As Double = SumFactors(sample, columns)
            Dim termB As Double = SumKernels(sample, columns)

            Return std.Pow(13.0 / 12.0, columns.Length) - 2.0 * termA / n + termB / (CDbl(n) * n)
        End Function

        ''' <summary>
        ''' Sum of the one dimensional marginal factor of each row, the
        ''' `sum_i prod_k a(x_ik)` term of the discrepancy.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="dims">the dimensions (columns) that should be projected, all of the dimensions are used when not set</param>
        ''' <returns>the sum of the per row factors</returns>
        Public Function SumFactors(sample As Double()(), Optional dims As Integer() = Nothing) As Double
            Dim columns As Integer() = ResolveDimensions(sample, dims)
            Dim factors As Double() = RowFactors(sample, columns)
            Dim sum As Double = 0

            For i As Integer = 0 To factors.Length - 1
                sum += factors(i)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' Sum of the pairwise kernel of each row, the
        ''' `sum_ij prod_k g(x_ik, x_jk)` term of the discrepancy.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="dims">the dimensions (columns) that should be projected, all of the dimensions are used when not set</param>
        ''' <returns>the sum of the per row kernel sums</returns>
        Public Function SumKernels(sample As Double()(), Optional dims As Integer() = Nothing) As Double
            Dim columns As Integer() = ResolveDimensions(sample, dims)
            Dim kernels As Double() = RowKernels(sample, columns)
            Dim sum As Double = 0

            For i As Integer = 0 To kernels.Length - 1
                sum += kernels(i)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' The factor `a(x) = 1 + |x - 0.5|/2 - |x - 0.5|^2/2` of one coordinate.
        ''' </summary>
        ''' <param name="x">the coordinate value in `[0,1)`</param>
        ''' <returns>the marginal factor of the centered discrepancy</returns>
        Public Function Factor(x As Double) As Double
            Dim delta As Double = std.Abs(x - 0.5)

            Return 1.0 + 0.5 * delta - 0.5 * delta * delta
        End Function

        ''' <summary>
        ''' The pairwise kernel `g(x, y) = 1 + |x - 0.5|/2 + |y - 0.5|/2 - |x - y|/2`,
        ''' it is symmetric: `g(x, y) = g(y, x)`.
        ''' </summary>
        ''' <param name="x">the coordinate of the first point</param>
        ''' <param name="y">the coordinate of the second point</param>
        ''' <returns>the pairwise kernel of the centered discrepancy</returns>
        Public Function Kernel(x As Double, y As Double) As Double
            Return 1.0 + 0.5 * std.Abs(x - 0.5) + 0.5 * std.Abs(y - 0.5) - 0.5 * std.Abs(x - y)
        End Function

        ''' <summary>
        ''' The product of the marginal factors of one sample row.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="columns">the projected columns, must be resolved already</param>
        ''' <param name="row">the sample row index</param>
        ''' <returns>prod_k a(x_ik)</returns>
        Friend Function FactorAt(sample As Double()(), columns As Integer(), row As Integer) As Double
            Dim product As Double = 1

            For Each axis As Integer In columns
                product *= Factor(sample(row)(axis))
            Next

            Return product
        End Function

        ''' <summary>
        ''' The product of the pairwise kernels between two sample rows.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="columns">the projected columns, must be resolved already</param>
        ''' <param name="i">the first sample row index</param>
        ''' <param name="j">the second sample row index</param>
        ''' <returns>prod_k g(x_ik, x_jk)</returns>
        Friend Function KernelAt(sample As Double()(), columns As Integer(), i As Integer, j As Integer) As Double
            Dim product As Double = 1

            For Each axis As Integer In columns
                product *= Kernel(sample(i)(axis), sample(j)(axis))
            Next

            Return product
        End Function

        ''' <summary>
        ''' The per row marginal factor, `A_i = prod_k a(x_ik)`.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="columns">the projected columns, must be resolved already</param>
        ''' <returns>a `N` length array of the per row factors</returns>
        Friend Function RowFactors(sample As Double()(), columns As Integer()) As Double()
            Dim n As Integer = sample.Length
            Dim factors As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                factors(i) = FactorAt(sample, columns, i)
            Next

            Return factors
        End Function

        ''' <summary>
        ''' The per row kernel sum, `R_i = sum_j prod_k g(x_ik, x_jk)`.
        ''' </summary>
        ''' <param name="sample">the `N x d` design in `[0,1)`</param>
        ''' <param name="columns">the projected columns, must be resolved already</param>
        ''' <returns>a `N` length array of the per row kernel sums</returns>
        ''' <remarks>
        ''' The kernel is symmetric, so each pair is only evaluated once and the
        ''' result is accumulated into the two rows at the same time.
        ''' </remarks>
        Friend Function RowKernels(sample As Double()(), columns As Integer()) As Double()
            Dim n As Integer = sample.Length
            Dim kernels As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                kernels(i) += KernelAt(sample, columns, i, i)

                For j As Integer = i + 1 To n - 1
                    Dim g As Double = KernelAt(sample, columns, i, j)

                    kernels(i) += g
                    kernels(j) += g
                Next
            Next

            Return kernels
        End Function

        ''' <summary>
        ''' Validate the design and resolve the requested projection columns.
        ''' </summary>
        ''' <param name="sample">the design to be checked</param>
        ''' <param name="dims">the requested dimensions, Nothing for all of the dimensions</param>
        ''' <returns>the resolved column indices</returns>
        Friend Function ResolveDimensions(sample As Double()(), dims As Integer()) As Integer()
            If sample Is Nothing OrElse sample.Length = 0 Then
                Throw New ArgumentException("the design matrix can not be empty.", NameOf(sample))
            End If

            Dim nDims As Integer = If(sample(0) Is Nothing, 0, sample(0).Length)

            If nDims = 0 Then
                Throw New ArgumentException("the design matrix can not be empty.", NameOf(sample))
            End If

            For i As Integer = 0 To sample.Length - 1
                If sample(i) Is Nothing OrElse sample(i).Length <> nDims Then
                    Throw New ArgumentException($"the design matrix is not rectangular, the row {i} does not own {nDims} columns.", NameOf(sample))
                End If
            Next

            If dims Is Nothing OrElse dims.Length = 0 Then
                Dim all As Integer() = New Integer(nDims - 1) {}

                For k As Integer = 0 To nDims - 1
                    all(k) = k
                Next

                Return all
            End If

            For Each axis As Integer In dims
                If axis < 0 OrElse axis >= nDims Then
                    Throw New ArgumentOutOfRangeException(NameOf(dims), axis, $"the projected dimension should be inside of the range [0, {nDims - 1}].")
                End If
            Next

            Return dims
        End Function

    End Module

End Namespace
