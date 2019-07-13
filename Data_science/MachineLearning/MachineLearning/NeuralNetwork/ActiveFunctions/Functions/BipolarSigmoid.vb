#Region "Microsoft.VisualBasic::115015938390807a34ca8490dffd48d3, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\Functions\BipolarSigmoid.vb"

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

    '     Class BipolarSigmoid
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
    ''' Bipolar sigmoid activation function.
    ''' </summary>
    '''
    ''' <remarks><para>The class represents bipolar sigmoid activation function with
    ''' the next expression:
    ''' <code lang="none">
    '''                2
    ''' f(x) = ------------------ - 1
    '''        1 + exp(-alpha * x)
    '''
    '''           2 * alpha * exp(-alpha * x )
    ''' f'(x) = -------------------------------- = alpha * (1 - f(x)^2) / 2
    '''           (1 + exp(-alpha * x))^2
    ''' </code>
    ''' </para>
    ''' 
    ''' <para>Output range of the function: <b>[-1, 1]</b>.</para>
    ''' 
    ''' <para>Functions graph:</para>
    ''' <img src="img/neuro/sigmoid_bipolar.bmp" width="242" height="172" />
    ''' </remarks>
    ''' 
    <Serializable>
    Public Class BipolarSigmoid : Inherits IActivationFunction

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
        Public Property Alpha As Double = 2.0R

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {
                        New NamedValue With {.name = "alpha", .text = Alpha}
                    },
                    .name = NameOf(BipolarSigmoid)
                }
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SigmoidFunction"/> class.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="BipolarSigmoid"/> class.
        ''' </summary>
        ''' 
        ''' <param name="alpha">Sigmoid's alpha value.</param>
        ''' 
        Public Sub New(alpha As Double)
            Me.Alpha = alpha
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
            Return ((2 / (1 + Math.Exp(-_Alpha * x))) - 1)
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
            Return (_Alpha * (1 - x * x) / 2)
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(BipolarSigmoid)}(alpha:={Alpha})"
        End Function
    End Class
End Namespace
