Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Public Module IComparer

    <Extension>
    Public Function Evaluate(query As String, x As Object) As Boolean
        Dim type As Type = x.GetType

        If type.Equals(GetType(String)) Then
            Return Build(query$) _
                .Evaluate(New IObject(GetType(Text)),
                          New Text With {
                            .Text = DirectCast(x, String)
                          })
        Else
            Return Build(query$).Evaluate(New IObject(type), x)
        End If
    End Function

    ''' <summary>
    ''' Does the text data can be matched by the query expression?
    ''' </summary>
    ''' <param name="text$"></param>
    ''' <param name="query$"></param>
    ''' <returns></returns>
    <Extension> Public Function Match(text$, query$) As Boolean
        Return Build(query).Evaluate(
            New IObject(GetType(Text)),
            New Text With {
                .Text = text$
            })
    End Function

    ''' <summary>
    ''' All of the world tokens in the input <paramref name="query"/> should match in one of the fileds in target object.
    ''' </summary>
    ''' <param name="query$"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FindAll(query$, x As Object) As Boolean
        Dim tokens$() = query.Split(" "c, ASCII.TAB)
        Dim exp$ = query.JoinBy(" AND ")

        Return exp.Evaluate(x)
    End Function

    ''' <summary>
    ''' All of the world tokens in the input <paramref name="query"/> should match in any fields in target object.
    ''' </summary>
    ''' <param name="query$"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MatchAll(query$, x As Object) As Boolean
        Dim tokens$() = query.Split(" "c, ASCII.TAB)

        For Each t$ In tokens
            If Not t$.Evaluate(x) Then
                Return False
            End If
        Next

        Return True
    End Function
End Module
