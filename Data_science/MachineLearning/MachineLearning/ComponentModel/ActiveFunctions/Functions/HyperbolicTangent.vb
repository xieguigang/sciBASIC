#Region "Microsoft.VisualBasic::6ee0c97abe91bbf1cbe7c33135daca84, Data_science\MachineLearning\MachineLearning\ComponentModel\ActiveFunctions\Functions\HyperbolicTangent.vb"

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

    '   Total Lines: 70
    '    Code Lines: 27 (38.57%)
    ' Comment Lines: 36 (51.43%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 7 (10.00%)
    '     File Size: 2.52 KB


    '     Class HyperbolicTangent
    ' 
    '         Properties: Store
    ' 
    '         Function: [Function], Derivative, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

Namespace ComponentModel.Activations

    ''' <summary>
    ''' The hyperbolic tangent activation function, its output range is <b>[-1, 1]</b>.
    ''' </summary>
    ''' <remarks>
    ''' ```
    '''         e ^ x - e ^ -x
    ''' f(x) = -----------------
    '''         e ^ x + e ^ -x
    ''' 
    ''' ```
    ''' </remarks>
    <Serializable>
    Public Class HyperbolicTangent : Inherits IActivationFunction

        ''' <summary>
        ''' Gets the XML serializable data model of this hyperbolic tangent function.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="ActiveFunction"/> data model which its function name is 
        ''' ``HyperbolicTangent`` and no argument is required.
        ''' </returns>
        Public Overrides ReadOnly Property Store As ActiveFunction
            Get
                Return New ActiveFunction() With {
                    .Arguments = {},
                    .name = NameOf(HyperbolicTangent)
                }
            End Get
        End Property

        ''' <summary>
        ''' 这个函数接受的参数应该是一个弧度值
        ''' </summary>
        ''' <param name="x">Function input value, in radius.</param>
        ''' <returns>
        ''' The function output value <i>f(x) = (e^x - e^-x) / (e^x + e^-x)</i>, 
        ''' which is limited in the interval ``[-1, 1]``.
        ''' </returns>
        Public Overrides Function [Function](x As Double) As Double
            Dim a = std.E ^ x
            Dim b = std.E ^ (-x)

            Return (a - b) / (a + b)
        End Function

        ''' <summary>
        ''' 这个函数所接受的参数也是一个弧度值
        ''' </summary>
        ''' <param name="x">The function input value, in radius.</param>
        ''' <returns>The derivative value <i>f'(x) = 1 / cosh(x) ^ 2</i>.</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Protected Overrides Function Derivative(x As Double) As Double
            Return 1 / (std.Cosh(x) ^ 2)
        End Function

        ''' <summary>
        ''' Display this activation function as a text expression.
        ''' </summary>
        ''' <returns>The text expression of this hyperbolic tangent function.</returns>
        Public Overrides Function ToString() As String
            Return Store.ToString
        End Function
    End Class
End Namespace
