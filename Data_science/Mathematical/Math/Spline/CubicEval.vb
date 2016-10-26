Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Interpolation

    Public Structure Cubic

        Public ReadOnly a, b, c, d As Double

        Public Sub New(p0 As Double, d2 As Double, e As Double, f As Double)
            d = p0
            c = d2
            b = e
            a = f
        End Sub

        Public Function Eval(u As Double) As Double
            'equals a*x^3 + b*x^2 + c*x + d
            Return (((a * u) + b) * u + c) * u + d
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace