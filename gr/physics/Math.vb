Imports System.Math
Imports System.Runtime.CompilerServices

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

    <Extension>
    Public Function Sum(F As IEnumerable(Of Force)) As Force
        Dim result As New Force

        For Each n As Force In F
            result = result + n
        Next

        Return result
    End Function

    Public Function CoulombsLaw(q1#, q2#, r#, Optional k# = 9000000000.0) As Double
        Return k * q1 * q2 / r ^ 2
    End Function
End Module
