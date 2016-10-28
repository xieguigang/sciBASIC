Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Text

Module Module1

    Sub Main()

        Dim s As New TestSin
        Dim result = s.Solve(1000, 0, 10)

        Call result.DataFrame.Save("x:\1.csv", Encodings.ASCII)

        Dim s2 As New TestRefSin With {
            .RefValues = New ValueVector With {
                .Y = result.y
            }
        }
        Dim result2 = s2.Solve(1000, 0, 10)

        Call result2.DataFrame.Save("x:\2.csv", Encodings.ASCII)

        Pause()
    End Sub

    Public Class TestRefSin : Inherits RefODEs

        Dim T As var

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector, Y As ValueVector)
            dy(T) = Math.Sin(dx) + Y("a")
        End Sub

        Protected Overrides Function y0() As var()
            Return {
                T = 33
            }
        End Function
    End Class

    Public Class TestSin : Inherits ODEs

        Dim T As var
        Dim a As var

        Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
            dy(T) = Math.Sin(dx) + a
            dy(a) = Math.Cos(dx)
        End Sub

        Protected Overrides Function y0() As var()
            Return {
                T = 33,
                a = -1
            }
        End Function
    End Class
End Module
