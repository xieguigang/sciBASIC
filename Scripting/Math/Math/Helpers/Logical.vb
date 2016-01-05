Imports System.Text
Imports System.Text.RegularExpressions

Namespace Helpers

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

        Friend Shared Function MathematicalExpression(expression As String) As String
            Dim matches = Regex.Matches(expression, MATH_EXPRESSION)
            Dim sBuilder As StringBuilder = New StringBuilder(expression)

            For Each Match As RegularExpressions.Match In matches
                Dim s As String = Match.Value

                For Each o In "=<>~"
                    If InStr(s, o) Then
                        Dim Tokens = s.Split(o)
                        Tokens = {Mathematical.Expression.Evaluate(Tokens(0)), Mathematical.Expression.Evaluate(Tokens(1))}
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
            Return Not Mathematical.Types.SimpleExpression.Evaluate(expression) = 0
        End Function

        ' a = b
        ' a =2 and b<3
        ' (a and b) or a >b

        Public Shared Function SimilarTo(a As Double, b As Double) As Boolean
            Dim E As Double = Math.Max(a, b) * (10 ^ -3)
            Return Math.Abs(a - b) < E
        End Function
    End Class
End Namespace
