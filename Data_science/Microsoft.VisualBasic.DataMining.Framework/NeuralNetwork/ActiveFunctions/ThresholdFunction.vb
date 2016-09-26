#Region "Microsoft.VisualBasic::6554acae71280bcbf7b6d9fb0964db0b, ..\visualbasic_App\Data_science\Microsoft.VisualBasic.DataMining.Framework\NeuralNetwork\ActiveFunctions\ThresholdFunction.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

' AForge Neural Net Library
' AForge.NET framework
' http://www.aforgenet.com/framework/
'
' Copyright © AForge.NET, 2007-2012
' contacts@aforgenet.com
'

Namespace NeuralNetwork.IFuncs

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
    Public Class ThresholdFunction
        Implements IActivationFunction
        Implements ICloneable

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThresholdFunction"/> class.
        ''' </summary>
        Public Sub New()
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
            Return If((x >= 0), 1, 0)
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
        '''
        Public Function Derivative(x As Double) As Double Implements IActivationFunction.Derivative
            Return 0
        End Function

        ''' <summary>
        ''' Calculates function derivative (not supported).
        ''' </summary>
        ''' 
        ''' <param name="y">Input value.</param>
        ''' 
        ''' <returns>Always returns 0.</returns>
        ''' 
        ''' <remarks><para><note>The method is not supported, because it is not possible to
        ''' calculate derivative of the function.</note></para></remarks>
        ''' 
        Public Function Derivative2(y As Double) As Double Implements IActivationFunction.Derivative2
            Return 0
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
            Return New ThresholdFunction()
        End Function
    End Class
End Namespace
