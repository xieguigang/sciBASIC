Imports stdNum = System.Math
Imports cephes = Microsoft.VisualBasic.Math.Statistics.SpecialFunctions

Namespace Hypothesis

    Public Class StudenttDistribution

        Public ReadOnly Property DegreeOfFreedom As Integer
        Public ReadOnly Property pdf_const As Double

        Sub New(df As Integer)
            DegreeOfFreedom = df
            ' Math.exp(cephes.lgam((df + 1) / 2) - cephes.lgam(df / 2)) / Math.sqrt(this._df * Math.PI)
            pdf_const = stdNum.Exp(cephes.gammaln((df + 1) / 2) - cephes.gammaln(df / 2)) / stdNum.Sqrt(df * stdNum.PI)
        End Sub

        Public Function pdf(x As Double) As Double
            Return pdf_const / stdNum.Pow(1 + ((x * x) / DegreeOfFreedom), (DegreeOfFreedom + 1) / 2)
        End Function

        Public Function cdf(x As Double) As Double
            Return t.Tcdf(x, DegreeOfFreedom)
        End Function

        Public Function inv(p As Double) As Double
            If (p <= 0) Then Return Double.NegativeInfinity
            If (p >= 1) Then Return Double.PositiveInfinity
            If (p = 0.5) Then Return 0

            If (p > 0.25 AndAlso p < 0.75) Then
                Dim phat = 1 - 2 * p
                Dim z = cephes.RegularizedIncompleteBetaFunction(0.5, 0.5 * DegreeOfFreedom, stdNum.Abs(phat))
                ' Dim z = cephes.incbi(0.5, 0.5 * DegreeOfFreedom, stdNum.Abs(phat))
                Dim t = stdNum.Sqrt(DegreeOfFreedom * z / (1 - z))

                Return If(p < 0.5, -t, t)
            Else
                Dim phat = If(p >= 0.5, 1 - p, p)
                Dim z = cephes.RegularizedIncompleteBetaFunction(0.5 * DegreeOfFreedom, 0.5, 2 * phat)
                ' Dim z = cephes.incbi(0.5 * DegreeOfFreedom, 0.5, 2 * phat)
                Dim t = stdNum.Sqrt(DegreeOfFreedom / z - DegreeOfFreedom)

                Return If(p < 0.5, -t, t)
            End If
        End Function
    End Class
End Namespace