#Region "Microsoft.VisualBasic::b4de42d7f57ef74a4ca74c91c5749f6e, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Regression\QLinear.vb"

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
    '     File Size: 997 B


    '     Class QLinear
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
    ''' A quadratic-linear activation function: <i>f(x) = ln(x ^ 2)</i> when 
    ''' <i>x &gt;= 1</i>, otherwise zero.
    ''' </summary>
    ''' <remarks>
    ''' The output of this function is flat (zero) on the interval 
    ''' <i>(-inf, 1)</i> and grows logarithmically outside of that interval.
    ''' </remarks>
    Public Class QLinear : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this quadratic-linear function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``QLinear`` and no argument is required.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction With {
                    .name = "QLinear",
                    .Arguments = {}
                }
            End Get
        End Property

        ''' <summary>
        ''' Calculates the function value: <i>ln(x ^ 2)</i> when <i>x &gt;= 1</i>, 
        ''' otherwise ``0``.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>The function output value.</returns>
        Public Overrides Function [Function](x As Double) As Double
            If x < 1 Then
                Return 0
            Else
                Return std.Log(x ^ 2)
            End If
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>The text expression of this quadratic-linear function.</returns>
        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function

        ''' <summary>
        ''' Calculates the derivative: <i>f'(x) = 1 / (2 * x)</i>.
        ''' </summary>
        ''' <param name="x">The function input value.</param>
        ''' <returns>
        ''' The derivative value; a large constant value ``10000`` will be 
        ''' returned when <paramref name="x"/> is zero, so that the derivative 
        ''' value will never be infinite.
        ''' </returns>
        Protected Overrides Function Derivative(x As Double) As Double
            If x = 0.0 Then
                Return 10000
            Else
                Return 1 / (2 * x)
            End If
        End Function
    End Class
End Namespace
