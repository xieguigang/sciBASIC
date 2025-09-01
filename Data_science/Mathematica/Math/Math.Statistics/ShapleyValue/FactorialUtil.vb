Namespace ShapleyValue

    Public Class FactorialUtil

        Public Shared Function factorial(n As Long) As Long
            If n > 20 Then
                Throw New ArithmeticException("Capacity exceded for factorial " & n.ToString())
            End If
            Return If(n > 1, n * factorial(n - 1), 1)
        End Function
    End Class
End Namespace
