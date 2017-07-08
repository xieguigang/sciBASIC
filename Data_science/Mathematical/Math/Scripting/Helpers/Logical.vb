#Region "Microsoft.VisualBasic::9e190338316a14acee2458448ea236ae, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\Helpers\Logical.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports sys = System.Math

Namespace Scripting.Helpers

    Public Class Logical
        '=<>~
        Public Shared ReadOnly NumericRelationships As Dictionary(Of Char, System.Func(Of Double, Double, Boolean)) =
            New Dictionary(Of Char, System.Func(Of Double, Double, Boolean)) From {
                {"="c, Function(a As Double, b As Double) a = b},
                {"<"c, Function(a As Double, b As Double) a < b},
                {">"c, Function(a As Double, b As Double) a > b},
                {"~"c, AddressOf SimilarTo}}

        Const NOT_EXPRESSION As String = "not \([a-z ]+\)"

        ''' <summary>
        ''' Convert a logical expression into a math expression
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' and -> *
        ''' or -> +
        ''' True -> 1
        ''' False -> 0
        ''' </remarks>
        Friend Shared Function Convert(expression As String) As String
            Dim sBuilder As StringBuilder = New StringBuilder([NOT](BracketExpression(expression)))

            Call sBuilder.Replace(" and ", " * ")
            Call sBuilder.Replace(" or ", " + ")
            Call sBuilder.Replace(" true ", "1")
            Call sBuilder.Replace(" false ", "0")
            Call sBuilder.Replace("true ", "1")
            Call sBuilder.Replace("false ", "0")
            Call sBuilder.Replace(" true", "1")
            Call sBuilder.Replace(" false", "0")
            Call sBuilder.Replace("(", "")
            Call sBuilder.Replace(")", "")

            Call sBuilder.Replace("true", "1")
            Call sBuilder.Replace("false", "0")

            Return sBuilder.ToString
        End Function

        Friend Shared Function [NOT](expression As String) As String
            Dim sBuilder As StringBuilder = New StringBuilder(expression)

            Call sBuilder.Replace("not true", "false")
            Call sBuilder.Replace("not false", "true")
            Dim matches = Regex.Matches(sBuilder.ToString, NOT_EXPRESSION)
            For Each Match As RegularExpressions.Match In matches
                Dim s As String = Not Evaluate(Mid(Match.Value, 5))
                sBuilder.Replace(Match.Value, s.ToLower)
            Next

            Return sBuilder.ToString
        End Function

        Const BRACKET_EXPRESSION As String = "\([a-z ]+\)"

        Friend Shared Function BracketExpression(expression As String) As String
            Dim matches = Regex.Matches(expression, BRACKET_EXPRESSION)
            Dim sBuilder As StringBuilder = New StringBuilder(expression)

            For Each Match As RegularExpressions.Match In matches
                Dim s As String = Evaluate(Mid(Match.Value, 2, Len(Match.Value) - 2))
                sBuilder.Replace(Match.Value, s.ToLower)
            Next

            Return sBuilder.ToString
        End Function

        Const MATH_EXPRESSION As String = "\S+[=<>~]\S+"

        Friend Shared Function MathematicalExpression(exp As String) As String
            Dim matches = Regex.Matches(exp, MATH_EXPRESSION)
            Dim sBuilder As New StringBuilder(exp)

            For Each Match As RegularExpressions.Match In matches
                Dim s As String = Match.Value

                For Each o In "=<>~"
                    If InStr(s, o) Then
                        Dim Tokens = s.Split(o)
                        Tokens = {Expression.Evaluate(Tokens(0)), Expression.Evaluate(Tokens(1))}
                        Dim value As String = Logical.NumericRelationships(o)(Tokens(0), Tokens(1))
                        sBuilder.Replace(s, value.ToLower)
                        Exit For
                    End If
                Next
            Next

            Return sBuilder.ToString
        End Function

        Public Shared Function Evaluate(expression As String) As Boolean
            expression = MathematicalExpression(expression)
            expression = Convert(expression)
            Return Not Types.SimpleExpression.Evaluate(expression) = 0
        End Function

        ' a = b
        ' a =2 and b<3
        ' (a and b) or a >b

        Public Shared Function SimilarTo(a As Double, b As Double) As Boolean
            Dim E As Double = sys.Max(a, b) * (10 ^ -3)
            Return sys.Abs(a - b) < E
        End Function
    End Class
End Namespace
