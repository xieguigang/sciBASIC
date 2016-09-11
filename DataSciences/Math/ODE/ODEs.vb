Public Class ODEs

    Public Class ODE : Inherits Calculus.ODE

        Public SetY As Action(Of Double)
    End Class

    Public Property ODEs As ODE()

    Public Sub Solve(n As Integer, a As Integer, b As Integer)
        Dim h As Double = (b - a) / n

        For Each df In ODEs
            df.x = New Double(n - 1) {}
            df.y = New Double(n - 1) {}
            df.x(Scan0) = a
            df.y(Scan0) = df.y0

            Call df.SetY(df.y0)
        Next

        Dim k1, k2, k3, k4 As Double

        For i As Integer = 1 To n - 1
            For Each df In ODEs
                df.x(i) = a + h * i
                k1 = df(df.x(i - 1), df.y(i - 1))
                k2 = df(df.x(i - 1) + 0.5 * h, df.y(i - 1) + 0.5 * h * k1)
                k3 = df(df.x(i - 1) + 0.5 * h, df.y(i - 1) + 0.5 * h * k2)
                k4 = df(df.x(i - 1) + h, df.y(i - 1) + h * k3)
                df.y(i) = df.y(i - 1) + h / 6 * (k1 + 2 * k2 + 2 * k3 + k4)

                Call df.SetY(df.y(i))
            Next
        Next
    End Sub
End Class
