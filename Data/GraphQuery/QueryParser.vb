Imports Microsoft.VisualBasic.Data.GraphQuery.Language
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language

Public Class QueryParser

    Public Shared Function GetQuery(text As String) As Query
        Dim tokenList As Token() = New TokenIcer(text) _
            .GetTokens _
            .Where(Function(tk) Not tk.name = Tokens.comment) _
            .ToArray
        Dim i As Pointer(Of QueryToken) = GetQueryTokens(tokenList).ToArray
        Dim t As Value(Of QueryToken) = ++i
        Dim query As Query
        Dim queryStack As New Stack(Of Query)
        Dim pipeNext As Boolean = False

        If CType(t, QueryToken).name = Tokens.symbol Then
            query = New Query With {.name = CType(t, QueryToken).text}
        Else
            query = New Query With {.name = "n/a"}
        End If

        Call queryStack.Push(query)

        Do While i
            Select Case (t = ++i).name
                Case Tokens.open
                    Select Case CType(t, QueryToken).text
                        Case "{"
                            queryStack.Push(query)
                        Case "["
                            query.isArray = True
                        Case Else
                            Throw New SyntaxErrorException
                    End Select
                Case Tokens.close
                    Select Case CType(t, QueryToken).text
                        Case "}"
                            queryStack.Pop()
                        Case Else
                            ' do nothing 
                    End Select
                Case Tokens.symbol
                    query = New Query With {.name = CType(t, QueryToken).text}
                    queryStack.Peek.Add(query)

                    pipeNext = False
                Case Tokens.NA
                    If pipeNext Then
                        query.parser.pipeNext = CType(t, QueryToken).func
                    ElseIf query.isArray Then
                        If query.members.IsNullOrEmpty Then
                            query.members = {
                                New Query With {
                                    .parser = CType(t, QueryToken).func,
                                    .name = "@array"
                                }
                            }
                        Else
                            Throw New SyntaxErrorException
                        End If
                    Else
                        query.parser = CType(t, QueryToken).func
                    End If

                    pipeNext = False
                Case Tokens.pipeline
                    pipeNext = True
                Case Else
                    Throw New NotImplementedException
            End Select
        Loop

        query = queryStack.First

        Return query
    End Function

    Private Shared Iterator Function GetQueryTokens(tokenList As Token()) As IEnumerable(Of QueryToken)
        Dim buf As New List(Of Token)

        For Each t As Token In tokenList
            If t.name = Tokens.symbol Then
                If buf = 1 Then
                    Yield New QueryToken With {.token = buf(Scan0)}
                    buf.Clear()
                End If

                buf += t
            ElseIf t.name = Tokens.open AndAlso t.text = "(" Then
                buf += t
            ElseIf t.name = Tokens.text OrElse t.name = Tokens.comma Then
                buf += t
            ElseIf t.name = Tokens.close AndAlso t.text = ")" Then
                buf += t
                Yield New QueryToken With {.func = GetParser(buf.PopAll)}
            Else
                If buf > 0 Then
                    Yield New QueryToken With {.token = buf.PopAll()(Scan0)}
                End If

                Yield New QueryToken With {.token = t}
            End If
        Next

        If buf > 0 Then
            Yield New QueryToken With {.func = GetParser(buf.PopAll)}
        End If
    End Function

    Private Shared Function GetParser(buf As Token()) As Parser
        Dim name As Token = buf(Scan0)
        Dim args = buf.Skip(2).Take(buf.Length - 3).Split(Function(t) t.name = Tokens.comma).ToArray

        Return New Parser With {
            .func = name.text,
            .parameters = args.Select(Function(a) a(Scan0).text).ToArray
        }
    End Function
End Class

Public Class QueryToken

    Public Property token As Token
    Public Property func As Parser

    Public ReadOnly Property name As Tokens
        Get
            If token Is Nothing Then
                Return Tokens.NA
            Else
                Return token.name
            End If
        End Get
    End Property

    Public ReadOnly Property text As String
        Get
            If token Is Nothing Then
                Return ""
            Else
                Return token.text
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        If token Is Nothing Then
            Return func.ToString
        Else
            Return token.text
        End If
    End Function

End Class