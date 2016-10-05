Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Public Module Evaluator

    ReadOnly __symbolsNoWildcards As Char() =
        ASCII.Symbols _
        .Where(Function(x) x <> "*"c AndAlso x <> "%"c) _
        .Join(" "c, ASCII.TAB) _
        .ToArray

    ReadOnly __allASCIISymbols As Char() =
        __symbolsNoWildcards _
        .Join("*"c, "%"c) _
        .ToArray

    ''' <summary>
    ''' 大小写敏感，在使用之前要先用tolower或者toupper
    ''' </summary>
    ''' <param name="term$"></param>
    ''' <param name="searchIn$"></param>
    ''' <returns></returns>
    Public Function ContainsAny(term$, searchIn$) As Boolean
        If term.First = "#"c Then
            Dim regexp As New Regex(Mid(term$, 2), RegexICSng)
            Return regexp.Match(searchIn$).Success
        End If

        Dim t1$() = term.Split(__symbolsNoWildcards)  ' term
        Dim t2$() = searchIn.Split(__allASCIISymbols) ' 目标

        For Each t$ In t1$
            If t2.Located(t$) <> -1 Then
                Return True
            ElseIf t2.WildcardsLocated(t$) <> -1 Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Function MustContains(term$, searchIn$) As Boolean
        Return InStr(searchIn, term$, CompareMethod.Text) > 0
    End Function

    <Extension>
    Public Function IsMustExpression(s$) As Boolean
        Return s.First = """"c AndAlso s.Last = """"c
    End Function

    <Extension>
    Public Function IsAnyExpression(s$) As Boolean
        Return s.First = "'"c AndAlso s.Last = "'"c
    End Function
End Module
