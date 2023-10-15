Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.Distributions

Namespace Hypothesis

    Public Class MoranTest

        Public Property Observed As Double
        Public Property Expected As Double
        Public Property SD As Double
        Public Property pvalue As Double
        Public Property z As Double
        Public Property prob2 As Double
        Public Property t As Double
        Public Property df As Double

        Public Shared Function moran_test(spatial As IEnumerable(Of Pixel),
                                          Optional alternative As Hypothesis = Hypothesis.TwoSided,
                                          Optional throwMaxIterError As Boolean = True,
                                          Optional parallel As Boolean = True) As MoranTest
            With spatial.ToArray
                Return moran_test(
                    .Select(Function(p) p.Scale).ToArray,
                    .Select(Function(p) CDbl(p.X)).ToArray,
                    .Select(Function(p) CDbl(p.Y)).ToArray,
                    alternative:=alternative,
                    throwMaxIterError:=throwMaxIterError,
                    parallel:=parallel
                )
            End With
        End Function

        ''' <summary>
        ''' Calculate Moran's I quickly for point data
        ''' </summary>
        ''' <param name="x">a numeric vector, the value distributed over space</param>
        ''' <param name="c1">a numeric vector, the first (x) value of a column of coordinates</param>
        ''' <param name="c2">a numeric vector, the second (y) value of a column of coordinates</param>
        ''' <param name="alternative">
        ''' alternative a character sring specifying the alternative hypothesis that
        ''' is tested against; must be one of "two.sided", "less", or "greater", 
        ''' or any unambiguous abbreviation of these.
        ''' </param>
        ''' <returns></returns>
        Public Shared Function moran_test(x As Double(), c1 As Double(), c2 As Double(),
                                          Optional alternative As Hypothesis = Hypothesis.TwoSided,
                                          Optional throwMaxIterError As Boolean = True,
                                          Optional parallel As Boolean = True) As MoranTest

            Dim res = Moran.calc_moran(x, c1, c2, parallel)
            Dim pv As Double = pnorm.eval(res.observed,
                                     mean:=res.expected,
                                     sd:=res.sd,
                                     resolution:=10000)
            Dim n As Integer = x.Length
            Dim z As Double, prob2 As Double, t As Double, df As Double
            Dim prob As Double

            Call Correlations.TestStats(res.observed, n, z, prob, prob2, t, df, throwMaxIterError)

            pv = 1 - (pv - 1)

            If alternative = Hypothesis.TwoSided Then
                If res.observed <= -1 / (x.Length - 1) Then
                    pv = 2 * pv
                Else
                    pv = 2 * (1 - pv)
                End If
            End If
            If alternative = Hypothesis.Greater Then
                pv = 1 - pv
            End If

            If pv < 0 Then
                pv = 1 / Single.MaxValue
            ElseIf pv.IsNaNImaginary Then
                pv = 1
            End If

            Return New MoranTest With {
                .Observed = res.observed,
                .Expected = res.expected,
                .pvalue = pv,
                .SD = res.sd,
                .df = df,
                .prob2 = prob2,
                .t = t,
                .z = z
            }
        End Function
    End Class
End Namespace