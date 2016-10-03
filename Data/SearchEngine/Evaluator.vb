Imports Microsoft.VisualBasic.Text

Public Module Evaluator

    ''' <summary>
    ''' 大小写敏感，在使用之前要先用tolower或者toupper
    ''' </summary>
    ''' <param name="term$"></param>
    ''' <param name="searchIn$"></param>
    ''' <returns></returns>
    Public Function ContainsAny(term$, searchIn$) As Boolean
        Dim t1$() = term.Split(ASCII.Symbols)  ' term
        Dim t2$() = term.Split(ASCII.Symbols)  ' 目标

        For Each t$ In t1$
            If t2.Located(t$) <> -1 Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Function MustContains(term$, searchIn$) As Boolean
        Return InStr(searchIn, term$, CompareMethod.Text) > 0
    End Function


End Module
