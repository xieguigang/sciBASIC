Namespace Framework.Optimization.LBFGSB

    Public MustInherit Class IGradFunction

        Public Overridable Function evaluate(x As Double(), grad As Double()) As Double
            Return Double.NaN
        End Function

        Public Overridable Function evaluate(x As Double()) As Double
            Return Double.NaN
        End Function

        Public Sub gradient(x As Double(), grad As Double())
            gradient(x, grad, 0.0001)
        End Sub

        ''' <summary>
        ''' finite difference, symmetrical gradient, stores result in grad[]
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="grad"></param>
        ''' <param name="eps"></param>
        Public Sub gradient(ByRef x As Double(), ByRef grad As Double(), eps As Double)
            Dim n = grad.Length

            For i = 0 To n - 1
                Dim tmp = x(i)
                Dim x1 = tmp - eps
                Dim x2 = tmp + eps
                x(i) = x1
                Dim y1 = evaluate(x)
                x(i) = x2
                Dim y2 = evaluate(x)
                x(i) = tmp ' restore
                grad(i) = (y2 - y1) / (2.0 * eps)
            Next
        End Sub

        Public Overridable Function in_place_gradient() As Boolean
            Return False
        End Function

        Public Function eval(x As Double(), grad As Double()) As Double
            If in_place_gradient() Then
                Return evaluate(x, grad)
            Else
                gradient(x, grad)
                Return evaluate(x)
            End If
        End Function

    End Class

End Namespace
