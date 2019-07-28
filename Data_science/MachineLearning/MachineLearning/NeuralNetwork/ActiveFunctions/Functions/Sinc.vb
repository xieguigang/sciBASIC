Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    Public Class Sinc : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Sinc)
                }
            End Get
        End Property

        Public Overrides Function [Function](x As Double) As Double
            If x = 0R Then
                Return 1
            Else
                Return Math.Sin(x) / x
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            If x = 0R Then
                Return 0
            Else
                Return Math.Cos(x) / x - Math.Sin(x) / (x ^ 2)
            End If
        End Function
    End Class
End Namespace