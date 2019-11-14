Imports System.Runtime.CompilerServices

Namespace ManagedSqlite.Core.SQLSchema

    Module Extensions

        <Extension>
        Public Function isKeyword(token As Token, ParamArray keywords As String()) As Boolean
            If token.name <> TokenTypes.keyword Then
                Return False
            ElseIf token.text.ToLower Like keywords.Select(AddressOf Strings.LCase).Indexing Then
                Return True
            Else
                Return False
            End If
        End Function
    End Module
End Namespace