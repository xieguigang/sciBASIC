#Region "Microsoft.VisualBasic::6ce0ec46daa30ea086b36d2e9e894003, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\Functions\Sigmoid.vb"

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

    '     Class Sigmoid
    ' 
    '         Properties: Alpha, Store
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: [Function], Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' AForge Neural Net Library
' AForge.NET framework
' http://www.aforgenet.com/framework/
'
' Copyright © AForge.NET, 2007-2012
' contacts@aforgenet.com
'

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' Sigmoid activation function.
    ''' </summary>
    '''
    ''' <remarks><para>The class represents sigmoid activation function with
    ''' the next expression:
    ''' <code lang="none">
    '''                1
    ''' f(x) = ------------------
    '''        1 + exp(-alpha * x)
    '''
    '''           alpha * exp(-alpha * x )
    ''' f'(x) = ---------------------------- = alpha * f(x) * (1 - f(x))
    '''           (1 + exp(-alpha * x))^2
    ''' </code>
    ''' </para>
    '''
    ''' <para>Output range of the function: <b>[0, 1]</b>.</para>
    ''' 
    ''' <para>Functions graph:</para>
    ''' <img src="img/neuro/sigmoid.bmp" width="242" height="172" />
    ''' </remarks>
    ''' 
    <Serializable>
    Public Class Sigmoid : Inherits IActivationFunction

        ''' <summary>
        ''' Sigmoid's alpha value.
        ''' </summary>
        ''' 
        ''' <remarks><para>The value determines steepness of the function. Increasing value of
        ''' this property changes sigmoid to look more like a threshold function. Decreasing
        ''' value of this property makes sigmoid to be very smooth (slowly growing from its
        ''' minimum value to its maximum value).</para>
        '''
        ''' <para>Default value is set to <b>2</b>.</para>
        ''' </remarks>
        ''' 
        Public Property Alpha() As Double = 2.0R

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {
                        New NamedValue With {.name = "alpha", .text = Alpha}
                    },
                    .name = NameOf(Sigmoid)
                }
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Sigmoid"/> class.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Sigmoid"/> class.
        ''' </summary>
        ''' 
        ''' <param name="alpha">Sigmoid's alpha value.</param>
        ''' 
        Public Sub New(alpha As Double)
            Me._Alpha = alpha
        End Sub


        ''' <summary>
        ''' Calculates function value.
        ''' </summary>
        '''
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function output value, <i>f(x)</i>.</returns>
        '''
        ''' <remarks>The method calculates function value at point <paramref name="x"/>.</remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function [Function](x As Double) As Double
            Return (1 / (1 + Math.Exp(-_Alpha * x)))
        End Function

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks>The method calculates function derivative at point <paramref name="x"/>.</remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            Return (_Alpha * x * (1 - x))
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(Sigmoid)}(alpha:={Alpha})"
        End Function
    End Class
End Namespace
