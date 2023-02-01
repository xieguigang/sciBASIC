Namespace ComponentModel.Activations

    Public Class Identical : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .name = NameOf(Identical),
                    .Arguments = {}
                }
            End Get
        End Property

        Public Overrides Function [Function](x As Double) As Double
            Return x
        End Function

        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        Protected Overrides Function Derivative(x As Double) As Double
            Return 1
        End Function
    End Class
End Namespace