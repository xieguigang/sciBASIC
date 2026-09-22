#Region "Microsoft.VisualBasic::b7e43da5a21ff4962785cf3ded148e5e, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\Sinc.vb"

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

    '   Total Lines: 36
    '    Code Lines: 30 (83.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (16.67%)
    '     File Size: 1019 B


    '     Class Sinc
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
    ''' The sinc (cardinal sine) activation function: <i>f(x) = sin(x) / x</i>.
    ''' </summary>
    ''' <remarks>
    ''' The function value at the origin point <i>x = 0</i> is defined as ``1``, 
    ''' which is the limit value of <i>sin(x) / x</i> when <i>x</i> tends to zero.
    ''' </remarks>
    Public Class Sinc : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this sinc function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``Sinc`` and no argument is required.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .Arguments = {},
                    .name = NameOf(Sinc)
                }
            End Get
        End Property

        ''' <summary>
        ''' Calculates the sinc function value: <i>sin(x) / x</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' The function output value, ``1`` will be returned when 
        ''' <paramref name="x"/> is zero.
        ''' </returns>
        Public Overrides Function [Function](x As Double) As Double
            If x = 0R Then
                Return 1
            Else
                Return std.Sin(x) / x
            End If
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>The text expression of this sinc function.</returns>
        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        ''' <summary>
        ''' Calculates the derivative of the sinc function: 
        ''' <i>f'(x) = cos(x) / x - sin(x) / x ^ 2</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' The derivative value, ``0`` will be returned when 
        ''' <paramref name="x"/> is zero.
        ''' </returns>
        Protected Overrides Function Derivative(x As Double) As Double
            If x = 0R Then
                Return 0
            Else
                Return std.Cos(x) / x - std.Sin(x) / (x ^ 2)
            End If
        End Function
    End Class
End Namespace
