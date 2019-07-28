Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    Public Class Softplus : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Softplus)
                }
            End Get
        End Property

        Public Overrides Function [Function](x As Double) As Double
            Return Math.Log(1 + Math.E ^ x)
        End Function

        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            Return 1 / (1 + Math.E ^ (-x))
        End Function
    End Class
End Namespace