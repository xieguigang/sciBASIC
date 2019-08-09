Imports System.Drawing
Imports stdNum = System.Math

Namespace Layouts.EdgeBundling

    <HideModuleName> Module Extensions

        Public ReadOnly PHI As Double = (1 + stdNum.Sqrt(5)) / 2

        Public Function lerp(a As PointF, b As PointF, delta As Double) As PointF
            Return New PointF With {
                .X = a.X * (1 - delta) + b.X * delta,
                .Y = a.Y * (1 - delta) + b.Y * delta
            }
        End Function
    End Module
End Namespace