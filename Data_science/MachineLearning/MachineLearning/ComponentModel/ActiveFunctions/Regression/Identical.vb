#Region "Microsoft.VisualBasic::f33f3b6125749e94c6b3aa35cadf7c3d, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Regression\Identical.vb"

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

    '   Total Lines: 54
    '    Code Lines: 21 (38.89%)
    ' Comment Lines: 28 (51.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (9.26%)
    '     File Size: 2.04 KB


    '     Class Identical
    ' 
    '         Properties: Store
    ' 
    '         Function: [Function], Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Activations

    ''' <summary>
    ''' The identity activation function: <i>f(x) = x</i>.
    ''' </summary>
    ''' <remarks>
    ''' This activation function is usually used by the regression model, in 
    ''' which the output of the neuron node should not be transformed.
    ''' </remarks>
    Public Class Identical : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this identity function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``Identical`` and no argument is required.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .name = NameOf(Identical),
                    .Arguments = {}
                }
            End Get
        End Property

        ''' <summary>
        ''' Returns the input value itself: <i>f(x) = x</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The <paramref name="x"/> value itself.</returns>
        Public Overrides Function [Function](x As Double) As Double
            Return x
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>The text expression of this identity function.</returns>
        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        ''' <summary>
        ''' The derivative of the identity function is the constant ``1``.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The constant value ``1``.</returns>
        Protected Overrides Function Derivative(x As Double) As Double
            Return 1
        End Function
    End Class
End Namespace
