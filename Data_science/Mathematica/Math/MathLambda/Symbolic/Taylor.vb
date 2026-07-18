#Region "Microsoft.VisualBasic::b832384086b943340ccef2a909e0af2d, Data_science\Mathematica\Math\MathLambda\Symbolic\Taylor.vb"

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

    '   Total Lines: 109
    '    Code Lines: 64 (58.72%)
    ' Comment Lines: 30 (27.52%)
    '    - Xml Docs: 53.33%
    ' 
    '   Blank Lines: 15 (13.76%)
    '     File Size: 4.45 KB


    '     Structure TaylorResult
    ' 
    ' 
    ' 
    '     Module Taylor
    ' 
    '         Function: expand, factorial, (+2 Overloads) Taylor, TaylorWithRemainder
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'     Module Taylor
' 
'         Symbolic Taylor / Maclaurin series expansion. The expansion is built
'         from the successive symbolic derivatives of the expression evaluated
'         at the expansion point. The Lagrange remainder (truncation error) is
'         also provided as a symbolic expression.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Script = Microsoft.VisualBasic.Math.Scripting.ScriptEngine

Namespace Symbolic

    ''' <summary>
    ''' The polynomial part and the Lagrange remainder of a Taylor expansion.
    ''' </summary>
    Public Structure TaylorResult
        ''' <summary>The truncated Taylor polynomial (degree &lt;= order).</summary>
        Public polynomial As Expression
        ''' <summary>The Lagrange remainder R_n = f^(n+1)(xi)/(n+1)! * (x-a)^(n+1).</summary>
        Public remainder As Expression
    End Structure

    Module Taylor

        ''' <summary>
        ''' Taylor expansion of <paramref name="expr"/> about <paramref name="point"/> up to the
        ''' given <paramref name="order"/>. Returns the truncated polynomial.
        ''' </summary>
        Public Function Taylor(expr As String, var$, point$, order%) As Expression
            Return Taylor(Script.ParseExpression(expr), var, Script.ParseExpression(point), order)
        End Function

        ''' <summary>
        ''' Taylor expansion of <paramref name="expr"/> about <paramref name="point"/> up to the
        ''' given <paramref name="order"/>. Returns the truncated polynomial.
        ''' </summary>
        Public Function Taylor(expr As Expression, var$, point As Expression, order%) As Expression
            Return expand(expr, var, point, order).polynomial
        End Function

        ''' <summary>
        ''' Taylor expansion together with its Lagrange remainder.
        ''' </summary>
        Public Function TaylorWithRemainder(expr As Expression, var$, point As Expression, order%) As TaylorResult
            Return expand(expr, var, point, order)
        End Function

        Private Function expand(expr As Expression, var$, point As Expression, order%) As TaylorResult
            Dim acc As Expression = MakeLiteral(0)
            Dim x = New SymbolExpression(var)

            For k As Integer = 0 To order
                Dim d = Derivative.DerivativeN(expr, var, k)
                Dim dAt = simplifyExpr(d.Substitute(var, point))

                Dim coeff As Expression
                If IsConstant(dAt) Then
                    Dim v As Double
                    Try
                        v = dAt.Evaluate(Script.Expression)
                    Catch
                        v = Double.NaN
                    End Try
                    coeff = MakeLiteral(v / factorial(k))
                Else
                    coeff = Div(dAt, MakeLiteral(factorial(k)))
                End If

                Dim term As Expression
                If k = 0 Then
                    term = coeff
                ElseIf k = 1 Then
                    term = Mul(coeff, Subt(Clone(x), Clone(point)))
                Else
                    term = Mul(coeff, Pow(Subt(Clone(x), Clone(point)), MakeLiteral(k)))
                End If

                acc = Add(acc, term)
            Next

            ' Lagrange remainder: f^(n+1)(xi) / (n+1)! * (x - a)^(n+1)
            Dim dNext = Derivative.DerivativeN(expr, var, order + 1)
            Dim xi = New SymbolExpression("_xi")
            Dim dAtXi = dNext.Substitute(var, xi)
            Dim remCoeff = Div(simplifyExpr(dAtXi), MakeLiteral(factorial(order + 1)))
            Dim remTerm = Mul(remCoeff, Pow(Subt(Clone(x), Clone(point)), MakeLiteral(order + 1)))

            Dim result As TaylorResult
            result.polynomial = simplifyExpr(acc)
            result.remainder = remTerm
            Return result
        End Function

        Private Function factorial(n As Integer) As Double
            Dim r = 1.0
            For i As Integer = 1 To n
                r *= i
            Next
            Return r
        End Function
    End Module
End Namespace

