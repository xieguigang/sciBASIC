Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Statistics.MomentFunctions
Imports Microsoft.VisualBasic.Mathematical.StatisticsMathExtensions
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module Ttest

    Public Enum Hypothesis
        ''' <summary>
        ''' ``mu > mu0``
        ''' </summary>
        Greater
        ''' <summary>
        ''' ``mu &lt; mu0``
        ''' </summary>
        Less
        ''' <summary>
        ''' ``mu &lt;> mu0``
        ''' </summary>
        TwoSided
    End Enum

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="mu#"></param>
    ''' <param name="alpha#"></param>
    ''' <param name="hyp"></param>
    ''' <param name="varEqual"></param>
    ''' <remarks>``ttest({0,1,1,1}, mu:= 1).valid() = True``</remarks>
    ''' <returns></returns>
    Public Function Ttest(x As IEnumerable(Of Double), mu#, Optional alpha# = 0.05, Optional hyp As Hypothesis = Hypothesis.TwoSided, Optional varEqual As Boolean = False) As TtestResult
        Dim statics As New BasicProductMoments(x)
        Return New TtestResult With {
            .alpha = alpha,
            .Freedom = statics.SampleSize - 1,
            .StdErr = Math.Sqrt(x.Variance / statics.SampleSize),
            .TestValue = (statics.Mean - mu) / .StdErr,
            .Pvalue = Pvalue(.TestValue, .Freedom, hyp),
            .Mean = statics.Mean
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="mu#"></param>
    ''' <param name="alpha#"></param>
    ''' <param name="hyp"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ``ttest({0,1,1,1}, {1,2,2,2}, mu:= -1).valid() = True``
    ''' </remarks>
    Public Function Ttest(a As IEnumerable(Of Double), b As IEnumerable(Of Double), Optional mu# = 0, Optional alpha# = 0.05, Optional hyp As Hypothesis = Hypothesis.TwoSided) As TtestResult
        Dim va#() = a.ToArray, vb = b.ToArray
        Dim left As New BasicProductMoments(a)
        Dim right As New BasicProductMoments(b)
        Dim df = left.SampleSize + right.SampleSize - 2
        Dim commonVariance = ((left.SampleSize - 1) * va.Variance + (right.SampleSize - 1) * vb.Variance) / df
        Return New TtestResult With {
            .alpha = alpha,
            .Freedom = df,
            .Mean = left.Mean - right.Mean,
            .StdErr = Math.Sqrt(commonVariance * (1 / left.SampleSize + 1 / right.SampleSize)),
            .TestValue = (.Mean - mu) / .StdErr,
            .Pvalue = Pvalue(.TestValue, df, hyp)
        }
    End Function

    Public Structure TtestResult

        Public Property Freedom As Integer
        Public Property Confidence As Double
        Public Property Pvalue As Double
        Public Property TestValue As Double

        Public Property alpha As Double
        Public Property Mean As Double
        Public Property StdErr As Double

        Public Function Valid() As Boolean
            Return Pvalue >= alpha
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="t#">The t test value</param>
    ''' <param name="v">v is the degrees of freedom</param>
    ''' <returns></returns>
    Public Function Pvalue(t#, v#, Optional hyp As Hypothesis = Hypothesis.TwoSided) As Double
        Select Case hyp
            Case Hypothesis.Greater
                Return 1 - Tcdf(t, v)
            Case Hypothesis.Less
                Return Tcdf(t, v)
            Case Else
                Return 2 * (1 - Tcdf(Math.Abs(t), v))
        End Select
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="t"></param>
    ''' <param name="v">v is the degrees of freedom</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ###### 2017-1-11 test success!
    ''' 
    ''' ```
    ''' tcdf(1,1) = 0.75
    ''' tcdf(0,5) = 0.5
    ''' ```
    ''' </remarks>
    Public Function Tcdf(t#, v#) As Double
        Dim x# = v / (v + t ^ 2)
        Dim inc = SpecialFunctions.RegularizedIncompleteBetaFunction(v / 2, 0.5, x)
        Dim cdf# = 1 - 0.5 * inc
        Return cdf
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="t"></param>
    ''' <param name="v#"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' ```
    ''' Tcdf({-4, -2, 0, 2, 4}, 5) = {~0.00516, ~0.051, ~0.5, ~0.949, ~0.995}
    ''' ```
    ''' </remarks>
    Public Function Tcdf(t As Vector, v#) As Vector
        Return New Vector(t.Select(Function(x) Tcdf(x, v)))
    End Function
End Module
