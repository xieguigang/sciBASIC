
Namespace NeuralNetwork

    Public NotInheritable Class Sigmoid
        Private Sub New()
        End Sub
        Public Shared Function Output(x As Double) As Double
            Return If(x < -45.0, 0.0, If(x > 45.0, 1.0, 1.0 / (1.0 + Math.Exp(-x))))
        End Function

        Public Shared Function Derivative(x As Double) As Double
            Return x * (1 - x)
        End Function
    End Class
End Namespace
