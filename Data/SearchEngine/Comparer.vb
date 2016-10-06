Imports System.Runtime.CompilerServices

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
End Module
