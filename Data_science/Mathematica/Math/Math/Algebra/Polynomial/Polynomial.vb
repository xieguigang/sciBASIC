#Region "Microsoft.VisualBasic::da7d37f5c2821e7d67cec90f93723e9e, Data_science\Mathematica\Math\Math\Algebra\Polynomial\Polynomial.vb"

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

    '   Total Lines: 125
    '    Code Lines: 82
    ' Comment Lines: 24
    '   Blank Lines: 19
    '     File Size: 4.11 KB


    '     Class Polynomial
    ' 
    '         Properties: IsLinear
    ' 
    '         Function: Evaluate, Parse, (+3 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl

Namespace LinearAlgebra

    ''' <summary>
    ''' 一元多项式的数据模型
    ''' 
    ''' ```
    ''' f(x) = a + bx + cx^2 + dx^3 + ...
    ''' ```
    ''' </summary>
    Public Class Polynomial : Inherits Formula

        ''' <summary>
        ''' f(x)
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property F(x#) As Double
            Get
                Dim ans As Double = 0

                For i As Integer = 0 To Factors.Length - 1
                    ans += Factors(i) * (x ^ i)
                Next

                Return ans
            End Get
        End Property

        ''' <summary>
        ''' Is linear or poly?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsLinear As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Factors.Length <= 2
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(ParamArray x() As Double) As Double
            Return F(x:=x(Scan0))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Overrides Function ToString(format As String, Optional html As Boolean = False) As String
            Return ToString({"X"}, format, html)
        End Function

        Private Overloads Shared Function ToString(a As Double, format$, html As Boolean) As String
            Dim text = a.ToString(format).ToLower

            If Not html Then
                Return text
            Else
                Dim t As String() = text.Split("e"c)

                If t.Length = 1 Then
                    Return text
                Else
                    Return $"{t(Scan0)}e<sup>{t(1)}</sup>"
                End If
            End If
        End Function

        Public Overrides Function ToString(variables() As String, format As String, Optional html As Boolean = False) As String
            Dim X As String = variables(Scan0)
            Dim items = Factors _
                .Select(Function(a, i)
                            If i = 0 Then
                                Return ToString(a, format, html)
                            ElseIf i = 1 Then
                                Return $"{ToString(a, format, html)}*{X}"
                            Else
                                Return $"{ToString(a, format, html)}*{X}^{i}"
                            End If
                        End Function) _
                .ToArray
            Dim Y$ = items.JoinBy(" + ")

            Return Y
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="str">
        ''' a+bx
        ''' y=a+bx
        ''' </param>
        ''' <returns></returns>
        Public Shared Function Parse(str As String) As Polynomial
            If str.Contains("="c) Then
                str = str.GetTagValue("=", trim:=True).Value
            End If

            Dim tokens As Expression = New ExpressionTokenIcer(str) _
                .GetTokens _
                .ToArray _
                .DoCall(AddressOf BuildExpression)
            Dim values As New List(Of Double)

            Do While TypeOf tokens Is BinaryExpression
                Dim bin = DirectCast(tokens, BinaryExpression)

                If TypeOf bin.left Is Literal Then
                    values.Add(bin.left.Evaluate(Nothing))
                Else
                    values.Add(1)
                End If

                tokens = bin.right
            Loop

            Return New Polynomial With {
                .Factors = values.ToArray
            }
        End Function
    End Class
End Namespace
