#Region "Microsoft.VisualBasic::a20051ab27c55b1616c1f2d77979d508, Data_science\MachineLearning\MachineLearning\NeuralNetwork\ActiveFunctions\Functions\Threshold.vb"

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

    '     Class Threshold
    ' 
    '         Properties: Store
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Function], CalculateDerivative, Derivative, ToString
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

Namespace NeuralNetwork.Activations

    ''' <summary>
    ''' Threshold activation function.
    ''' </summary>
    '''
    ''' <remarks><para>The class represents threshold activation function with
    ''' the next expression:
    ''' <code lang="none">
    ''' f(x) = 1, if x >= 0, otherwise 0
    ''' </code>
    ''' </para>
    ''' 
    ''' <para>Output range of the function: <b>[0, 1]</b>.</para>
    ''' 
    ''' <para>Functions graph:</para>
    ''' <img src="img/neuro/threshold.bmp" width="242" height="172" />
    ''' </remarks>
    '''
    <Serializable>
    Public Class Threshold : Inherits IActivationFunction

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Threshold"/> class.
        ''' </summary>
        Public Sub New()
        End Sub

        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Threshold)
                }
            End Get
        End Property

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
            Return If((x >= 0), 1, 0)
        End Function

        Public Overrides Function CalculateDerivative(x As Double) As Double
            Return 0
        End Function

        ''' <summary>
        ''' Calculates function derivative (not supported).
        ''' </summary>
        ''' 
        ''' <param name="x">Input value.</param>
        ''' 
        ''' <returns>Always returns 0.</returns>
        ''' 
        ''' <remarks><para><note>The method is not supported, because it is not possible to
        ''' calculate derivative of the function.</note></para></remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            Return 0
        End Function

        Public Overrides Function ToString() As String
            Return $"{NameOf(Threshold)}()"
        End Function
    End Class
End Namespace
