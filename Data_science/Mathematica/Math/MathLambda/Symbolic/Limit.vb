#Region "Microsoft.VisualBasic::8da4fd3a7ba2abf842cbdd8b39f13156, Data_science\Mathematica\Math\MathLambda\Symbolic\Limit.vb"

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

    '   Total Lines: 139
    '    Code Lines: 89 (64.03%)
    ' Comment Lines: 33 (23.74%)
    '    - Xml Docs: 33.33%
    ' 
    '   Blank Lines: 17 (12.23%)
    '     File Size: 6.11 KB


    '     Module Limit
    ' 
    '         Function: computeLimit, isInfinityTarget, (+2 Overloads) Limit, near, nearZero
    '                   (+2 Overloads) probe
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'     Module Limit
' 
'         Heuristic limit computation. Supports direct substitution, the two
'         classical indeterminate forms 0/0 and Infinity/Infinity via
'         L'Hopital's rule, and a numeric two-sided probe for finite and
'         infinite limit points. Evaluation is used only as a numeric probe
'         to decide the limit; the result is returned as a symbolic expression.
'
'     Author: xie.guigang@live.com
'     Copyright (c) 2018 GPL3 Licensed
'
' /********************************************************************************/

Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Script = Microsoft.VisualBasic.Math.Scripting.ScriptEngine

Namespace Symbolic

    Module Limit

        ''' <summary>
        ''' Compute lim_{var -> target} expr.
        ''' </summary>
        Public Function Limit(expr As String, var$, target As String) As Expression
            Return Limit(Script.ParseExpression(expr), var, Script.ParseExpression(target))
        End Function

        ''' <summary>
        ''' Compute lim_{var -> target} expr. The target is given as an expression
        ''' (typically a <see cref="Literal"/> such as 0, or a symbolic point).
        ''' </summary>
        Public Function Limit(expr As Expression, var$, target As Expression) As Expression
            Return computeLimit(expr, var, target, 0)
        End Function

        Private Function computeLimit(expr As Expression, var$, target As Expression, depth As Integer) As Expression
            If depth > 24 Then
                Return simplifyExpr(expr.Substitute(var, target))
            End If

            ' 1) L'Hopital's rule for the 0/0 and Infinity/Infinity forms. This must be
            '    tested BEFORE the direct-substitution step, because the engine may
            '    evaluate an indeterminate quotient to a finite value (e.g. 0/0 -> 0).
            If TypeOf target Is Literal AndAlso TypeOf expr Is BinaryExpression AndAlso DirectCast(expr, BinaryExpression).operator = "/"c Then
                Dim b = DirectCast(expr, BinaryExpression)
                Dim t = NumericValue(target).Value
                Dim av = probe(b.left, var, t)
                Dim bv = probe(b.right, var, t)

                If (nearZero(av) AndAlso nearZero(bv)) OrElse (Double.IsInfinity(av) AndAlso Double.IsInfinity(bv)) Then
                    Dim da = Differentiate(b.left, var)
                    Dim db = Differentiate(b.right, var)
                    If da IsNot Nothing AndAlso db IsNot Nothing Then
                        Return computeLimit(Div(da, db), var, target, depth + 1)
                    End If
                End If
            End If

            ' 2) Direct substitution into a constant expression.
            Dim s = simplifyExpr(expr.Substitute(var, target))
            If IsConstant(s) Then
                Dim v As Double
                Try
                    v = s.Evaluate(Script.Expression)
                Catch
                    v = Double.NaN
                End Try
                If Not (Double.IsNaN(v) OrElse Double.IsInfinity(v)) Then
                    Return MakeLiteral(v)
                End If
                ' It was an indeterminate constant form (0/0, Infinity/Infinity); fall
                ' through to the numeric probes below.
            End If

            ' 3) Infinite limit point: probe at a very large finite magnitude.
            If isInfinityTarget(target) Then
                Dim big = 1.0E9
                Dim v = probe(expr, var, big)
                If Not Double.IsNaN(v) Then
                    Return MakeLiteral(v)
                End If
            End If

            ' 4) Two-sided numeric probe around a finite limit point.
            If TypeOf target Is Literal Then
                Dim t = DirectCast(target, Literal).number
                Dim vL = probe(expr, var, t - 1.0E-6)
                Dim vR = probe(expr, var, t + 1.0E-6)
                If Not (Double.IsNaN(vL) OrElse Double.IsNaN(vR)) AndAlso near(vL, vR) Then
                    Return MakeLiteral((vL + vR) / 2.0)
                End If
            End If

            Return simplifyExpr(expr.Substitute(var, target))
        End Function

        ''' <summary>
        ''' Numerically evaluate the expression with <paramref name="var"/> bound to
        ''' <paramref name="value"/>. Returns NaN on failure (e.g. division by zero).
        ''' </summary>
        Private Function probe(expr As Expression, var$, value As Double) As Double
            Try
                Dim subbed = expr.Substitute(var, value)
                Return subbed.Evaluate(Script.Expression)
            Catch
                Return Double.NaN
            End Try
        End Function

        Private Function probe(expr As Expression, var$, target As Expression) As Double
            Dim t = NumericValue(target)
            If Not t.HasValue Then Return Double.NaN
            Return probe(expr, var, t.Value)
        End Function

        Private Function nearZero(v As Double) As Boolean
            Return Not Double.IsNaN(v) AndAlso System.Math.Abs(v) < 1.0E-7
        End Function

        Private Function near(a As Double, b As Double) As Boolean
            If Double.IsNaN(a) OrElse Double.IsNaN(b) Then Return False
            Return System.Math.Abs(a - b) < 1.0E-5
        End Function

        Private Function isInfinityTarget(target As Expression) As Boolean
            Dim t = NumericValue(target)
            If t.HasValue Then
                Return System.Math.Abs(t.Value) > 1.0E8
            End If
            If TypeOf target Is SymbolExpression Then
                Dim nm = DirectCast(target, SymbolExpression).symbolName.ToLower
                Return nm = "inf" OrElse nm = "infinity"
            End If
            Return False
        End Function
    End Module
End Namespace

