Imports System.Math

Public Module Math

    Public Function ParallelogramLaw(f1 As Force, f2 As Force) As Force
        Dim alpha = f1.Angle - f2.Angle
        Dim F = f1 ^ 2 + f2 ^ 2 + 2 * f1 * f2 * Cos(alpha)
        Dim sina = Sin(alpha) * f1 / F

        alpha = Sinh(sina)

        Return New Force With {
            .Strength = F,
            .Angle = alpha
        }
    End Function
End Module
