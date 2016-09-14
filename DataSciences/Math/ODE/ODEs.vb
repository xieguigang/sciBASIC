Public Class ODEs

    Public Class ODE : Inherits diffEq.ODE

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

    Sub ODERungeKuttaOneStep(dxn As Double,           ' x初值
         dyn As Double(), ' 初值y(n)
      dh As Double,            ' 步长
         func As [Function],            ' 微分方程组右端项
       dynext As Double()    ' 下一步的值y(n+1)
        )

        Dim n = dyn.Length   ' 微分方程组中方程的个数，同时是初值y(n)和下一步值y(n+1)的长度
        If (n <> dynext.Length) Then

            '   dynext.resize(n, 0.0);  //下一步的值y(n+1)与y(n)长度相等
        End If
        'Dim K1, K2, K3, K4 As Double()
        'func(dxn, dyn, K1)               ' 求解K1
        'func(dxn + dh / 2, dyn + dh / 2 * K1, K2)  ' 求解K2
        'func(dxn + dh / 2, dyn + dh / 2 * K2, K3)  ' 求解K3
        'func(dxn + dh, dyn + dh * K3, K4)      ' 求解K4
        'dynext = dyn + (K1 + K2 + K3 + K4) * dh / 6.0 ' 求解下一步的值y(n+1)
    End Sub
End Class

Public Delegate Function [Function](dx As Double, y As Double(), k As Double())