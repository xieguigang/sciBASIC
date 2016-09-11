Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus

Module ODEsTest

    Const a = -8 / 3
    Const b = -10
    Const c = -28

    Sub test()
        Dim dx As New ODEs.ODE With {.SetY = Sub(s) X = s, .df = AddressOf ODEsTest.dX, .y0 = 1}
        Dim dy As New ODEs.ODE With {.SetY = Sub(s) Y = s, .df = AddressOf ODEsTest.dY, .y0 = 1}
        Dim dz As New ODEs.ODE With {.SetY = Sub(s) Z = s, .df = AddressOf ODEsTest.dZ, .y0 = 1}
        Dim ss As New ODEs With {.ODEs = {dx, dy, dz}}
        Call ss.Solve(10000, 0, 100)


        Call (From i In dx.y.SeqIterator Select xx = i.obj, yy = dy.y(i.i)).ToArray.SaveTo("x:/ffffff.csv")
        Pause()
    End Sub

    Dim X, Y, Z

    Function dX(x As Double, y As Double) As Double
        Return a * y + ODEsTest.Y * Z
    End Function

    Function dY(x As Double, y As Double) As Double
        Return b * (y - Z)
    End Function

    Function dZ(x As Double, y As Double) As Double
        Return -ODEsTest.X * ODEsTest.Y + c * ODEsTest.Y - y
    End Function

End Module
