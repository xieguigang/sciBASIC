#Region "Microsoft.VisualBasic::0a7014fe8803c7726f839d92e5991076, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\SigmoidFunction.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 75
    '    Code Lines: 29 (38.67%)
    ' Comment Lines: 39 (52.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (9.33%)
    '     File Size: 3.20 KB


    '     Class SigmoidFunction
    ' 
    '         Properties: Store
    ' 
    '         Function: [Function], Derivative, Sigmoid, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace ComponentModel.Activations

    ''' <summary>
    ''' https://github.com/trentsartain/Neural-Network/blob/master/NeuralNetwork/NeuralNetwork/Network/Sigmoid.cs
    ''' </summary>
    Public NotInheritable Class SigmoidFunction : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this sigmoid function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``SigmoidFunction`` and no argument is required.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(SigmoidFunction)
                }
            End Get
        End Property

        ''' <summary>
        ''' Calculates the derivative of the sigmoid function, the input value is 
        ''' assumed to be the function output value: <i>f'(x) = x * (1 - x)</i>.
        ''' </summary>
        ''' <param name="x">
        ''' The function output value <i>f(x)</i> instead of the raw input value.
        ''' </param>
        ''' <returns>The derivative value <i>f'(x) = x * (1 - x)</i>.</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            Return x * (1 - x)
        End Function

        ''' <summary>
        ''' Calculates the logistic sigmoid function value: <i>f(x) = 1 / (1 + e ^ -x)</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' The function output value which is limited in the interval ``[0, 1]``; 
        ''' the input value is clipped on the interval ``[-45, 45]`` so that the 
        ''' exponential term will never overflow.
        ''' </returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function [Function](x As Double) As Double
            Return If(x < -45.0, 0.0, If(x > 45.0, 1.0, 1.0 / (1.0 + std.Exp(-x))))
        End Function

        ''' <summary>
        ''' Apply the logistic sigmoid function on each element of the given vector.
        ''' </summary>
        ''' <param name="x">A <see cref="Vector"/> of the function input values.</param>
        ''' <returns>
        ''' A new <see cref="Vector"/> in which each element is limited in the 
        ''' interval ``[0, 1]``.
        ''' </returns>
        Public Shared Function Sigmoid(x As Vector) As Vector
            Return 1 / (1 + (-x).Exp)
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>A text expression in format like ``SigmoidFunction()``.</returns>
        Public Overrides Function ToString() As String
            Return $"{NameOf(SigmoidFunction)}()"
        End Function
    End Class
End Namespace
