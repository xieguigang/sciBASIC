' AForge Neural Net Library
' AForge.NET framework
' http://www.aforgenet.com/framework/
'
' Copyright © AForge.NET, 2007-2012
' contacts@aforgenet.com
'

Namespace NeuralNetwork.IFuncs

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
    Public Class SigmoidFunction
        Implements IActivationFunction
        Implements ICloneable

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

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SigmoidFunction"/> class.
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SigmoidFunction"/> class.
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
        '''
        Public Function [Function](x As Double) As Double Implements IActivationFunction.[Function]
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
        '''
        Public Function Derivative(x As Double) As Double Implements IActivationFunction.Derivative
            Dim y As Double = [Function](x)

            Return (_Alpha * y * (1 - y))
        End Function

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="y">Function output value - the value, which was obtained
        ''' with the help of "Function" method.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks><para>The method calculates the same derivative value as the
        ''' <see cref="Derivative"/> method, but it takes not the input <b>x</b> value
        ''' itself, but the function value, which was calculated previously with
        ''' the help of "Function" method.</para>
        ''' 
        ''' <para><note>Some applications require as function value, as derivative value,
        ''' so they can save the amount of calculations using this method to calculate derivative.</note></para>
        ''' </remarks>
        ''' 
        Public Function Derivative2(y As Double) As Double Implements IActivationFunction.Derivative2
            Return (_Alpha * y * (1 - y))
        End Function

        ''' <summary>
        ''' Creates a new object that is a copy of the current instance.
        ''' </summary>
        ''' 
        ''' <returns>
        ''' A new object that is a copy of this instance.
        ''' </returns>
        ''' 
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New SigmoidFunction(_Alpha)
        End Function
    End Class
End Namespace
