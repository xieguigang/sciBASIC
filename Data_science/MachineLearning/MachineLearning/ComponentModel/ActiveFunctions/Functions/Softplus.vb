#Region "Microsoft.VisualBasic::2f8e02c23b7352a29dd4dcaec24d7397, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\Softplus.vb"

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

    '   Total Lines: 57
    '    Code Lines: 22 (38.60%)
    ' Comment Lines: 29 (50.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (10.53%)
    '     File Size: 2.20 KB


    '     Class Softplus
    ' 
    '         Properties: Store
    ' 
    '         Function: [Function], Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace ComponentModel.Activations

    ''' <summary>
    ''' The softplus activation function: <i>f(x) = ln(1 + e ^ x)</i>.
    ''' </summary>
    ''' <remarks>
    ''' Softplus is a smooth approximation of the <see cref="ReLU"/> function, and
    ''' its derivative is the standard logistic sigmoid function.
    ''' </remarks>
    Public Class Softplus : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this softplus function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``Softplus`` and no argument is required.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Softplus)
                }
            End Get
        End Property

        ''' <summary>
        ''' Calculates the softplus function value: <i>ln(1 + e ^ x)</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The function output value.</returns>
        Public Overrides Function [Function](x As Double) As Double
            Return std.Log(1 + std.E ^ x)
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>The text expression of this softplus function.</returns>
        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        ''' <summary>
        ''' Calculates the derivative of the softplus function, which equals to 
        ''' the logistic sigmoid function: <i>f'(x) = 1 / (1 + e ^ -x)</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The derivative value.</returns>
        Protected Overrides Function Derivative(x As Double) As Double
            Return 1 / (1 + std.E ^ (-x))
        End Function
    End Class
End Namespace
