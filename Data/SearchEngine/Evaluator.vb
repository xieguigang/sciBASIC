Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text

Public Module Evaluator

    ReadOnly __symbolsNoWildcards As Char() =
        ASCII.Symbols _
        .Where(Function(x) x <> "*"c AndAlso x <> "%"c) _
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
        Dim t2$() = searchIn.Split(ASCII.Symbols)  ' 目标

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
End Module
