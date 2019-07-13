#Region "Microsoft.VisualBasic::0d53ee104fcf72cdae9e2da10382eca1, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\Functions\ReLU.vb"

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

    '     Class ReLU
    ' 
    '         Properties: Store
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [Function], CalculateDerivative, Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace NeuralNetwork.Activations

    Public Class ReLU : Inherits IActivationFunction

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {
                        New NamedValue With {
                            .name = "threshold",
                            .text = threshold
                        }
                    },
                    .name = NameOf(ReLU)
                }
            End Get
        End Property

        ReadOnly threshold# = 0

        Sub New()
        End Sub

        Sub New(threshold As Double)
            Me.threshold = threshold
        End Sub

        Public Overrides Function [Function](x As Double) As Double
            If x < threshold Then
                Return threshold
            ElseIf Truncate > 0 Then
                Return ValueTruncate(x, Truncate)
            Else
                Return x
            End If
        End Function

        Public Overrides Function CalculateDerivative(x As Double) As Double
            If x < threshold Then
                Return threshold
            Else
                Return 1
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            If x < threshold Then
                Return threshold
            Else
                Return 1
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(ReLU)}()"
        End Function
    End Class
End Namespace
