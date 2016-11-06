Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus

Public Class NetworkDemo : Inherits ODEs

    Dim a, b, c, d As var

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(a) = 0.01 * a + b + c
        dy(b) = c - d - 0.5 * b
        dy(c) = (c ^ -3) * d - 0.3 * c
        dy(d) = 2 * a - a ^ 0.6 - 0.2 * d
    End Sub

    Protected Overrides Function y0() As var()
        Return {
            a = 1,
            b = 1,
            c = 1,
            d = 1
        }
    End Function
End Class
