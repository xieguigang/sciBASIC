Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace ManagedSqlite.Core

    Public Enum TokenTypes
        name
        type
        keyword
        open
        close
        comma
        length
    End Enum

    Public Class Token : Inherits CodeToken(Of TokenTypes)
    End Class

    Public Class SQLParser

        Dim sql As CharPtr
        ''' <summary>
        ''' 主要是针对name进行escape
        ''' </summary>
        Dim escape As Boolean
        Dim escapeChar As Char
        Dim buffer As List(Of Char)

        Sub New(sql As String)
            Me.sql = sql
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim token As New Value(Of Token)

            Do While Not sql
                If Not token = walkChar(++sql) Is Nothing Then
                    Yield token

                    If buffer > 0 Then
                        Select Case buffer.PopAll.CharString
                            Case "("
                                Yield New Token With {.name = TokenTypes.open, .text = "("}
                            Case ")"
                                Yield New Token With {.name = TokenTypes.close, .text = ")"}
                            Case ","
                                Yield New Token With {.name = TokenTypes.comma, .text = ","}
                            Case Else
                                Throw New NotImplementedException
                        End Select
                    End If
                End If
            Loop
        End Function

        Private Function walkChar(c As Char) As Token
            If c = """"c Then
                If escape AndAlso escapeChar = """"c Then
                    escape = False
                    Return New Token With {
                        .name = TokenTypes.name,
                        .text = buffer.PopAll.CharString
                    }
                Else
                    escapeChar = """"c
                    escape = True
                End If

                Return Nothing
            ElseIf c = "["c Then
                If escape Then
                    buffer += c
                Else
                    escapeChar = c
                    escape = True
                    buffer += "["c
                End If

                Return Nothing
            ElseIf escape Then
                If c = "]"c Then
                    If escape AndAlso escapeChar = "["c Then
                        buffer += "]"c
                        escape = False
                        Return populateBufferToken(Nothing)
                    Else
                        Throw New SyntaxErrorException
                    End If
                Else
                    buffer += c
                End If

                Return Nothing
            End If

            If c = "("c OrElse c = ")"c OrElse c = ","c Then
                If buffer = 0 Then
                    Return New Token With {
                        .text = c.ToString,
                        .name = If(c = "("c, TokenTypes.open, If(c = ")"c, TokenTypes.close, TokenTypes.comma))
                    }
                Else
                    Return populateBufferToken(bufferNext:=c)
                End If
            ElseIf c Like whitespace Then
                Return populateBufferToken(Nothing)
            Else
                buffer += c
            End If

            Return Nothing
        End Function

        ReadOnly whitespace As Index(Of Char) = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF}

        ReadOnly keywords As Index(Of String) = {
            "create", "table", "not", "null", "nocase", "unique", "collate"
        }

        ReadOnly datatype As Index(Of String) = {
            "int", "varchar", "bit", "datetime"
        }

        Private Function populateBufferToken(bufferNext As Char?) As Token
            Dim text = buffer.PopAll.CharString

            If text.Length = 0 Then
                Return Nothing
            End If

            If Not bufferNext Is Nothing Then
                buffer += CChar(bufferNext)
            End If

            If text.ToLower Like keywords Then
                Return New Token With {
                    .text = text,
                    .name = TokenTypes.keyword
                }
            ElseIf text.First = "["c AndAlso text.Last = "]"c Then
                text = text.GetStackValue("[", "]")

                If text.ToLower Like datatype Then
                    Return New Token With {
                        .text = text,
                        .name = TokenTypes.type
                    }
                Else
                    Return New Token With {
                        .text = text,
                        .name = TokenTypes.name
                    }
                End If
            ElseIf text.IsPattern("\d+") Then
                Return New Token With {
                    .name = TokenTypes.length,
                    .text = text
                }
            End If

            Throw New NotImplementedException(text)
        End Function
    End Class
End Namespace