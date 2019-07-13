#Region "Microsoft.VisualBasic::d3f9c219c2f6cc5d6f74421f62d0b979, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\Functions\SigmoidFunction.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class SigmoidFunction
    ' 
    '         Properties: Store
    ' 
    '         Function: [Function], Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' https://github.com/trentsartain/Neural-Network/blob/master/NeuralNetwork/NeuralNetwork/Network/Sigmoid.cs
    ''' </summary>
    Public NotInheritable Class SigmoidFunction : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(SigmoidFunction)
                }
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            Return x * (1 - x)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function [Function](x As Double) As Double
            Return If(x < -45.0, 0.0, If(x > 45.0, 1.0, 1.0 / (1.0 + Math.Exp(-x))))
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(SigmoidFunction)}()"
        End Function
    End Class
End Namespace
