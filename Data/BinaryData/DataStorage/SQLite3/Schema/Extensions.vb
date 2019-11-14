Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

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

        <Extension>
        Friend Function SplitByTopLevelDelimiter(tokens As IEnumerable(Of Token), delimiter As TokenTypes,
                                                 Optional includeKeyword As Boolean = False,
                                                 Optional tokenText$ = Nothing) As List(Of Token())
            Dim blocks As New List(Of Token())
            Dim buf As New List(Of Token)
            Dim stack As New Stack(Of Token)
            Dim isDelimiter As Func(Of Token, Boolean)

            If tokenText Is Nothing Then
                isDelimiter = Function(t) t.name = delimiter
            Else
                isDelimiter = Function(t)
                                  Return t.name = delimiter AndAlso t.text = tokenText
                              End Function
            End If

            ' 使用最顶层的comma进行分割
            For Each t As Token In tokens
                Dim add As Boolean = True

                If t.name = TokenTypes.open Then
                    stack.Push(t)
                ElseIf t.name = TokenTypes.close Then
                    stack.Pop()
                End If

                If isDelimiter(t) OrElse (includeKeyword AndAlso t.name = TokenTypes.keyword) Then
                    If stack.Count = 0 Then
                        ' 这个是最顶层的分割
                        If buf > 0 Then
                            blocks += buf.PopAll
                        End If

                        blocks += {t}
                        add = False
                    End If
                End If

                If add Then
                    buf += t
                End If
            Next

            If buf > 0 Then
                Return blocks + buf.ToArray
            Else
                Return blocks
            End If
        End Function
    End Module
End Namespace