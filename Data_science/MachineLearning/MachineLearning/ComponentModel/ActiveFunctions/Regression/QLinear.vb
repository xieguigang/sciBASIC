Imports stdNum = System.Math

Namespace ComponentModel.Activations

    Public Class QLinear : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .name = "QLinear",
                    .Arguments = {}
                }
            End Get
        End Property

        Public Overrides Function [Function](x As Double) As Double
            If x < 1 Then
                Return 0
            Else
                Return stdNum.Log(x ^ 2)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            If x = 0.0 Then
                Return 10000
            Else
                Return 1 / (2 * x)
            End If
        End Function
    End Class
End Namespace