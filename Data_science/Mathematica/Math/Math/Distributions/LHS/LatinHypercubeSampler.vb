Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports std = System.Math

Namespace Distributions.LHS

    ''' <summary>
    ''' A scipy style Latin hypercube sampler: it owns the dimension count, the
    ''' random seed and the quality options, and every call of <see cref="Random(Integer)"/>
    ''' returns a complete new design.
    ''' </summary>
    ''' <remarks>
    ''' <code>
    ''' Dim sampler As New LatinHypercubeSampler(nDims:=12, seed:=42) With {
    '''     .Optimization = LhsOptimization.RandomCd
    ''' }
    ''' Dim unit As Double()() = sampler.Random(10000)     ' 10000 x 12, values inside of [0,1)
    ''' Dim params = LhsScale.ScaleLog10(unit, logLb, logUb)  ' the real parameter values
    ''' </code>
    ''' <para>
    ''' The design of one call is stratified, but two successive calls return two
    ''' independent designs; never merge the results of two calls into one single
    ''' design because the stratification of the merged design is broken.
    ''' </para>
    ''' <para>
    ''' The sampler owns a shared random number generator and is not thread safe,
    ''' create one sampler per thread (a fixed seed per sampler keeps the results
    ''' reproducible).
    ''' </para>
    ''' </remarks>
    Public Class LatinHypercubeSampler

        Private ReadOnly m_nDims As Integer
        Private ReadOnly m_rng As System.Random
        Private m_strength As Integer = 1
        Private m_optimization As LhsOptimization = LhsOptimization.None
        Private m_cdIterations As Integer = 100
        Private m_objectiveDimensions As Integer = 2
        Private m_center As Boolean = False
        Private m_lastDiscrepancy As Double = Double.NaN

        ''' <summary>
        ''' Create a sampler for the given number of dimensions.
        ''' </summary>
        ''' <param name="nDims">the number of dimensions, d</param>
        ''' <param name="seed">the random seed, a random seed is used when this parameter is not set</param>
        Public Sub New(nDims As Integer, Optional seed As Integer? = Nothing)
            LatinHypercube.CheckSize(1, nDims, "LatinHypercubeSampler")

            m_nDims = nDims
            m_rng = LatinHypercube.NewRandom(seed)
        End Sub

        ''' <summary>
        ''' The number of dimensions (the columns of the generated design).
        ''' </summary>
        ''' <returns>the dimension count of this sampler</returns>
        Public ReadOnly Property Dimensions As Integer
            Get
                Return m_nDims
            End Get
        End Property

        ''' <summary>
        ''' The strength of the design: 1 for the classic Latin hypercube which
        ''' guarantees the one dimensional stratification, 2 for the orthogonal
        ''' array based design which makes the two dimensional projections
        ''' (nearly) uniform as well.
        ''' </summary>
        ''' <returns>1 or 2</returns>
        ''' <remarks>
        ''' A strength 2 design requires that the number of samples is the square of
        ''' a prime number (`N = p^2`), so the sample count must be selected
        ''' accordingly (49, 121, 361, 961, 1024 = 2^10 is not valid, 5041 = 71^2 is).
        ''' </remarks>
        Public Property Strength As Integer
            Get
                Return m_strength
            End Get
            Set(value As Integer)
                If value <> 1 AndAlso value <> 2 Then
                    Throw New ArgumentOutOfRangeException(NameOf(Strength), value, "the strength should be 1 or 2.")
                End If

                m_strength = value
            End Set
        End Property

        ''' <summary>
        ''' The post optimization strategy of the generated design.
        ''' </summary>
        ''' <returns>None (default) or RandomCd</returns>
        Public Property Optimization As LhsOptimization
            Get
                Return m_optimization
            End Get
            Set(value As LhsOptimization)
                m_optimization = value
            End Set
        End Property

        ''' <summary>
        ''' The number of candidate coordinate swaps of the random-cd optimization.
        ''' </summary>
        ''' <returns>100 by default</returns>
        Public Property CdIterations As Integer
            Get
                Return m_cdIterations
            End Get
            Set(value As Integer)
                If value < 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(CdIterations), value, "the number of iterations can not be a negative number.")
                End If

                m_cdIterations = value
            End Set
        End Property

        ''' <summary>
        ''' The number of leading dimensions of the random-cd objective projection,
        ''' `2` by default because the one dimensional projection is already perfect
        ''' for any Latin hypercube design.
        ''' </summary>
        ''' <returns>the size of the objective projection</returns>
        Public Property ObjectiveDimensions As Integer
            Get
                Return m_objectiveDimensions
            End Get
            Set(value As Integer)
                If value < 1 Then
                    Throw New ArgumentOutOfRangeException(NameOf(ObjectiveDimensions), value, "the objective projection should own at least one dimension.")
                End If

                m_objectiveDimensions = value
            End Set
        End Property

        ''' <summary>
        ''' Place every point at the middle of its stratum instead of a random
        ''' position inside of the stratum (the median/centered Latin hypercube).
        ''' </summary>
        ''' <returns>false by default</returns>
        ''' <remarks>
        ''' The centering option only applies to a strength 1 design, a strength 2
        ''' design keeps its random jitter because the within level permutation of the
        ''' orthogonal array already decides the stratum of each point.
        ''' </remarks>
        Public Property Center As Boolean
            Get
                Return m_center
            End Get
            Set(value As Boolean)
                m_center = value
            End Set
        End Property

        ''' <summary>
        ''' The centered discrepancy of the last generated design over the leading
        ''' <see cref="ObjectiveDimensions"/> projection.
        ''' </summary>
        ''' <returns>
        ''' the discrepancy, or <see cref="Double.NaN"/> when the last design was not
        ''' measured (the measure is only evaluated when the random-cd optimization
        ''' is enabled, the full dimensional measure is too expensive for a large
        ''' sample count).
        ''' </returns>
        Public ReadOnly Property LastDiscrepancy As Double
            Get
                Return m_lastDiscrepancy
            End Get
        End Property

        ''' <summary>
        ''' Generate a complete new Latin hypercube design.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, N</param>
        ''' <returns>a `N x Dimensions` design in `[0,1)`</returns>
        ''' <exception cref="ArgumentException">the strength 2 design is requested with a sample count that is not the square of a prime number</exception>
        Public Function Random(nSamples As Integer) As Double()()
            LatinHypercube.CheckSize(nSamples, m_nDims, "LatinHypercubeSampler")

            Dim design As Double()()

            Select Case m_strength
                Case 2
                    design = LatinHypercube.StrengthTwo(nSamples, m_nDims, m_rng)
                Case Else
                    If m_center Then
                        design = LatinHypercube.Median(nSamples, m_nDims, m_rng)
                    Else
                        design = LatinHypercube.RandomUnit(nSamples, m_nDims, m_rng)
                    End If
            End Select

            If m_optimization = LhsOptimization.RandomCd Then
                design = LhsOptimizer.Optimize(design, m_rng, m_cdIterations, m_objectiveDimensions)
                m_lastDiscrepancy = MeasureDiscrepancy(design, std.Min(m_objectiveDimensions, m_nDims))
            Else
                m_lastDiscrepancy = Double.NaN
            End If

            Return design
        End Function

        ''' <summary>
        ''' Generate a complete new Latin hypercube design and wrap it into a
        ''' <see cref="NumericMatrix"/> where the rows are the samples.
        ''' </summary>
        ''' <param name="nSamples">the number of samples, N</param>
        ''' <returns>a `N x Dimensions` matrix in `[0,1)`</returns>
        Public Function RandomMatrix(nSamples As Integer) As NumericMatrix
            Return New NumericMatrix(Random(nSamples))
        End Function

        ''' <summary>
        ''' Measure the centered discrepancy of a design over the leading dimensions.
        ''' </summary>
        ''' <param name="design">the design to be measured</param>
        ''' <param name="nProjection">the number of leading dimensions to be projected, all dimensions when not set</param>
        ''' <returns>the centered discrepancy, smaller is better</returns>
        Public Shared Function MeasureDiscrepancy(design As Double()(), Optional nProjection As Integer? = Nothing) As Double
            Dim dims As Integer() = Nothing

            If nProjection IsNot Nothing Then
                Dim n As Integer = std.Min(nProjection.Value, design(0).Length)

                If n < 1 Then
                    Throw New ArgumentOutOfRangeException(NameOf(nProjection), nProjection, "the projection should own at least one dimension.")
                End If

                dims = New Integer(n - 1) {}

                For k As Integer = 0 To n - 1
                    dims(k) = k
                Next
            End If

            Return LhsDiscrepancy.Centered(design, dims)
        End Function

    End Class

End Namespace
