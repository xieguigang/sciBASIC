Imports Microsoft.VisualBasic.Data.GraphQuery.Language
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language

Public Class QueryParser

    Public Shared Function GetQuery(text As String) As Query
        Dim tokenList As Token() = New TokenIcer(text) _
            .GetTokens _
            .Where(Function(tk) Not tk.name = Tokens.comment) _
            .ToArray
        Dim i As Pointer(Of Token) = tokenList
        Dim t As Value(Of Token) = ++i
        Dim query As Query
        Dim queryStack As New Stack(Of Query)

        If CType(t, Token).name = Tokens.symbol Then
            query = New Query With {.name = CType(t, Token).text}
        Else
            query = New Query With {.name = "n/a"}
        End If

        Call queryStack.Push(query)

        Do While i
            Select Case (t = ++i).name
                Case Tokens.open
                    Select Case CType(t, Token).text
                        Case "{"
                        Case "("

                    End Select
                Case Tokens.close
            End Select
        Loop

        Return query
    End Function
End Class
