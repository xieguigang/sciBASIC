Namespace Mathematical

    Partial Module Trigonometric

        Const HalfPi = Math.PI / 2
        Const n1 = 1.0R
        Const n05 = 0.5R

        ''' <summary>
        ''' Taylor Atan
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="precise">通过这个参数来控制计算精度，这个参数值越大，计算精度越高</param>
        ''' <returns></returns>
        ''' <remarks>Atan测试没有问题</remarks>>
        Public Function Atn(x As Double, Optional precise As Integer = 500) As Double
            If x = 1.0 Then
                Return Math.PI / 4
            End If
            If Math.Sign(x) = Math.Sign(-1) Then
                Return -Atn(-x)
            End If
            If x > n1 Then
                Return HalfPi - Atn(1 / x)
            End If
            If x > n05 Then
                Return Atn(n1) + Atn((x - n1) / (1 + x))
            Else
                Dim xPow2 = x * x
                Dim n__1 = precise
                Dim y = 1 / (2 * n__1 + 1)
                Dim i As Integer = precise

                While i > 0
                    y = (1 / (2 * n__1 - 1)) - (xPow2 * y)
                    i -= 1
                    n__1 -= 1
                End While

                Return x * y
            End If
        End Function
    End Module
End Namespace