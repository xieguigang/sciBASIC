
Namespace NeuralNetwork.IFuncs

    Public NotInheritable Class Sigmoid
        Implements IActivationFunction
        Implements ICloneable

        Public Function Derivative(x As Double) As Double Implements IActivationFunction.Derivative
            Return x * (1 - x)
        End Function

        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me
        End Function

        Public Function [Function](x As Double) As Double Implements IActivationFunction.Function
            Return If(x < -45.0, 0.0, If(x > 45.0, 1.0, 1.0 / (1.0 + Math.Exp(-x))))
        End Function

        Public Function Derivative2(y As Double) As Double Implements IActivationFunction.Derivative2
            Return Derivative(y)
        End Function
    End Class
End Namespace
