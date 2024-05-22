#Region "Microsoft.VisualBasic::c4b793de52b4601311fde8ff80cf6d92, Data_science\Mathematica\Math\Math\Algebra\Polynomial\MultivariatePolynomial.vb"

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

    '   Total Lines: 43
    '    Code Lines: 24 (55.81%)
    ' Comment Lines: 12 (27.91%)
    '    - Xml Docs: 91.67%
    ' 
    '   Blank Lines: 7 (16.28%)
    '     File Size: 1.38 KB


    '     Class MultivariatePolynomial
    ' 
    '         Function: Evaluate, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    ''' <summary>
    ''' 多元多项式
    ''' 
    ''' ```
    ''' f(x1, x2, x3, ...) = a*x1 + b*x2 + c*x3 + ...
    ''' ```
    ''' </summary>
    Public Class MultivariatePolynomial : Inherits Formula

        ''' <summary>
        ''' sum(x * b) 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(ParamArray x() As Double) As Double
            Dim y As Double = 0

            For i As Integer = 0 To Factors.Length - 1
                y += x(i) * Factors(i)
            Next

            Return y
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString(format As String, Optional html As Boolean = False) As String
            Return Factors _
                .Select(Function(b, i) $"{b}*X{i + 1}") _
                .JoinBy(" + ")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString(variables() As String, format As String, Optional html As Boolean = False) As String
            Return Factors _
                .Select(Function(b, i) $"{b}*{variables(i)}") _
                .JoinBy(" + ")
        End Function
    End Class
End Namespace
