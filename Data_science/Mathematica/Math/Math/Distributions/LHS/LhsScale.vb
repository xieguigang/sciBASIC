Imports std = System.Math

Namespace Distributions.LHS

    ''' <summary>
    ''' Map a unit hypercube design `[0,1)^d` onto the real parameter ranges.
    ''' </summary>
    ''' <remarks>
    ''' A Latin hypercube design only fills the unit hypercube, the sampling ranges
    ''' of the parameters are applied afterwards, which is the same two step
    ''' workflow as `LatinHypercube.random` followed by `qmc.scale` in scipy.
    ''' <para>
    ''' For parameters that span several orders of magnitude (for example a rate
    ''' constant inside of `[0.001, 100]`) the design should be scaled inside of
    ''' the logarithm space, otherwise nearly all of the samples get clustered at
    ''' the upper end of the range.
    ''' </para>
    ''' </remarks>
    Public Module LhsScale

        ''' <summary>
        ''' Linearly scale each dimension of the unit design onto `[lb(j), ub(j)]`.
        ''' </summary>
        ''' <param name="unit">the `N x d` Latin hypercube design in `[0,1)`</param>
        ''' <param name="lb">the lower bound of each dimension, a `d` length array</param>
        ''' <param name="ub">the upper bound of each dimension, a `d` length array</param>
        ''' <returns>the scaled `N x d` sample matrix</returns>
        Public Function Scale(unit As Double()(), lb As Double(), ub As Double()) As Double()()
            Dim nDims As Integer = CheckBounds(unit, lb, ub, "Scale")
            Dim n As Integer = unit.Length
            Dim scaled As Double()() = LatinHypercube.NewSamples(n, nDims)

            For i As Integer = 0 To n - 1
                For axis As Integer = 0 To nDims - 1
                    scaled(i)(axis) = lb(axis) + unit(i)(axis) * (ub(axis) - lb(axis))
                Next
            Next

            Return scaled
        End Function

        ''' <summary>
        ''' Linearly scale the unit design onto the same `[lb, ub]` range for all of the dimensions.
        ''' </summary>
        ''' <param name="unit">the `N x d` Latin hypercube design in `[0,1)`</param>
        ''' <param name="lb">the lower bound of all dimensions</param>
        ''' <param name="ub">the upper bound of all dimensions</param>
        ''' <returns>the scaled `N x d` sample matrix</returns>
        Public Function Scale(unit As Double()(), lb As Double, ub As Double) As Double()()
            Dim nDims As Integer = UnitDimensions(unit, "Scale")

            Return Scale(unit, Fill(lb, nDims), Fill(ub, nDims))
        End Function

        ''' <summary>
        ''' Scale the design inside of the logarithm space of the given base, so that
        ''' every order of magnitude owns the same sampling density.
        ''' </summary>
        ''' <param name="unit">the `N x d` Latin hypercube design in `[0,1)`</param>
        ''' <param name="logLb">the logarithm of the lower bound of each dimension</param>
        ''' <param name="logUb">the logarithm of the upper bound of each dimension</param>
        ''' <param name="logBase">the base of the logarithm, 10 by default</param>
        ''' <returns>the scaled `N x d` sample matrix, `base^(logLb + unit*(logUb-logLb))`</returns>
        Public Function ScaleLog(unit As Double()(), logLb As Double(), logUb As Double(), Optional logBase As Double = 10) As Double()()
            If logBase <= 0 OrElse logBase = 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(logBase), logBase, "the logarithm base should be a positive number and not equal to 1.")
            End If

            Dim inLog As Double()() = Scale(unit, logLb, logUb)
            Dim n As Integer = inLog.Length
            Dim nDims As Integer = inLog(0).Length

            For i As Integer = 0 To n - 1
                For axis As Integer = 0 To nDims - 1
                    inLog(i)(axis) = std.Pow(logBase, inLog(i)(axis))
                Next
            Next

            Return inLog
        End Function

        ''' <summary>
        ''' Scale the design inside of the log10 space, the typical usage for the
        ''' parameters that span several orders of magnitude.
        ''' </summary>
        ''' <param name="unit">the `N x d` Latin hypercube design in `[0,1)`</param>
        ''' <param name="logLb">the log10 of the lower bound of each dimension</param>
        ''' <param name="logUb">the log10 of the upper bound of each dimension</param>
        ''' <returns>the scaled `N x d` sample matrix, `10^(logLb + unit*(logUb-logLb))`</returns>
        Public Function ScaleLog10(unit As Double()(), logLb As Double(), logUb As Double()) As Double()()
            Return ScaleLog(unit, logLb, logUb, 10.0)
        End Function

        ''' <summary>
        ''' Repeat a bound pattern cyclically until `nDims` bounds are produced, the
        ''' equivalent of `np.tile(pattern, nDims)`.
        ''' </summary>
        ''' <param name="pattern">the bound pattern, for example `{-1, -3}`</param>
        ''' <param name="nDims">the number of dimensions that should be covered</param>
        ''' <returns>a `nDims` length array, `result(i) = pattern(i mod pattern.Length)`</returns>
        Public Function TileBounds(pattern As Double(), nDims As Integer) As Double()
            If pattern Is Nothing OrElse pattern.Length = 0 Then
                Throw New ArgumentException("the bound pattern can not be empty.", NameOf(pattern))
            End If
            If nDims < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(nDims), nDims, "the number of dimensions must be a positive integer.")
            End If

            Dim bounds As Double() = New Double(nDims - 1) {}

            For i As Integer = 0 To nDims - 1
                bounds(i) = pattern(i Mod pattern.Length)
            Next

            Return bounds
        End Function

        ''' <summary>
        ''' Create a bound array with the same value for all of the dimensions.
        ''' </summary>
        ''' <param name="value">the bound value</param>
        ''' <param name="nDims">the number of dimensions</param>
        ''' <returns>a `nDims` length array filled with the given value</returns>
        Public Function Fill(value As Double, nDims As Integer) As Double()
            Dim bounds As Double() = New Double(nDims - 1) {}

            For i As Integer = 0 To nDims - 1
                bounds(i) = value
            Next

            Return bounds
        End Function

        ''' <summary>
        ''' Validate the design and the bounds, returns the number of dimensions.
        ''' </summary>
        Friend Function CheckBounds(unit As Double()(), lb As Double(), ub As Double(), caller As String) As Integer
            Dim nDims As Integer = UnitDimensions(unit, caller)

            If lb Is Nothing OrElse lb.Length <> nDims Then
                Throw New ArgumentException($"{caller}: the lower bound array should own {nDims} elements.", NameOf(lb))
            End If
            If ub Is Nothing OrElse ub.Length <> nDims Then
                Throw New ArgumentException($"{caller}: the upper bound array should own {nDims} elements.", NameOf(ub))
            End If

            For axis As Integer = 0 To nDims - 1
                If Not lb(axis) < ub(axis) Then
                    Throw New ArgumentException($"{caller}: the lower bound {lb(axis)} of the dimension {axis} should be less than the upper bound {ub(axis)}.", NameOf(lb))
                End If
            Next

            For i As Integer = 0 To unit.Length - 1
                If unit(i) Is Nothing OrElse unit(i).Length <> nDims Then
                    Throw New ArgumentException($"{caller}: the unit design is not rectangular, the row {i} does not own {nDims} columns.", NameOf(unit))
                End If
            Next

            Return nDims
        End Function

        ''' <summary>
        ''' Return the number of dimensions of the design and check that it is not empty.
        ''' </summary>
        ''' <param name="unit">the unit design</param>
        ''' <param name="caller">the name of the calling method, it is used inside of the error message</param>
        ''' <returns>the number of dimensions of the design</returns>
        Friend Function UnitDimensions(unit As Double()(), caller As String) As Integer
            If unit Is Nothing OrElse unit.Length = 0 Then
                Throw New ArgumentException($"{caller}: the unit design can not be empty.", NameOf(unit))
            End If

            Dim nDims As Integer = If(unit(0) Is Nothing, 0, unit(0).Length)

            If nDims = 0 Then
                Throw New ArgumentException($"{caller}: the unit design can not be empty.", NameOf(unit))
            End If

            Return nDims
        End Function

    End Module

End Namespace
