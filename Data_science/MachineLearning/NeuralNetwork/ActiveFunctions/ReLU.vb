Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    Public Class ReLU : Implements IActivationFunction

        Public ReadOnly Property Store As ActiveFunction Implements IActivationFunction.Store
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(ReLU)
                }
            End Get
        End Property

        Public Function [Function](x As Double) As Double Implements IActivationFunction.Function
            If x < 0 Then
                Return 0.0
            Else
                Return x
            End If
        End Function

        Public Function Derivative(x As Double) As Double Implements IActivationFunction.Derivative
            Return 1
        End Function

        Public Function Derivative2(y As Double) As Double Implements IActivationFunction.Derivative2
            Return 1
        End Function
    End Class
End Namespace