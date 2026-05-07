Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Public Class TokenIcer

    Protected buffer As CharBuffer
    Protected escape As Char = ASCII.NUL
    Protected lastToken As Token
    Protected line As Integer = 0

    ''' <summary>
    ''' single line comment
    ''' </summary>
    Protected comment_escape As Boolean
    Protected comments As New Dictionary(Of String, String)
    Protected comment_key As String = ""

    Dim yaml As CharPtr

    Sub New(yaml As String)
        Me.yaml = New CharPtr(yaml)
        Me.buffer = ""
    End Sub

    Public Iterator Function GetTokens() As IEnumerable(Of Token)
        Do While Not yaml.EndRead
            For Each t As Token In WalkChar(++yaml)
                If Not t Is Nothing Then
                    lastToken = t.SetLine(line)
                    Yield t
                End If
            Next
        Loop

        If buffer > 0 Then
            If comment_escape Then
                comments(comment_key) = New String(buffer.PopAllChars)
            Else
                Throw New Exception("unknow parser error at the end of the json document stream!")
            End If
        End If
    End Function

    Private Iterator Function WalkChar(c As Char) As IEnumerable(Of Token)
        If c = ASCII.LF Then
            line += 1
        End If

        If comment_escape Then
            If c = ASCII.LF Then
                comment_escape = False
                comments(comment_key) = New String(buffer.PopAllChars)
                comment_key = ""
            Else
                Call buffer.Add(c)
            End If
        ElseIf escape <> ASCII.NUL Then
            ' is string escape
            If c = escape Then
                If buffer.StartEscaping Then
                    ' continute
                    buffer += c
                Else
                    ' end of the string escape
                    escape = ASCII.NUL
                    Yield New Token(Token.JSONElements.String, buffer.PopAllChars)
                End If
            Else
                buffer += c
            End If
        ElseIf c = "'"c Then
            escape = c
            Return
        ElseIf c = ":"c Then
            ' end previous token
            ' key: value
            If buffer > 0 Then
                Yield New Token(Token.JSONElements.Key, buffer.PopAllChars)
            End If

            Yield New Token(Token.JSONElements.Colon, ":")
        ElseIf c = "-"c Then
            If buffer > 0 Then
                buffer += c
            Else
                Yield New Token(Token.JSONElements.Serial, "-")
            End If
        ElseIf c = " "c OrElse c = ASCII.TAB OrElse c = ASCII.LF Then
            Yield MeasureToken()

            If c = " "c OrElse c = ASCII.TAB Then
                Yield New Token(Token.JSONElements.WhiteSpace, " ")
            Else
                Yield New Token(Token.JSONElements.NewLine, ASCII.LF)
            End If
        Else
            buffer += c
        End If
    End Function

    ''' <summary>
    ''' the entire <see cref="buffer"/> will be clear in this function
    ''' </summary>
    ''' <returns></returns>
    Protected Function MeasureToken() As Token
        If buffer = 0 Then
            Return Nothing
        End If

        Dim str As New String(buffer.PopAllChars)

        Static [boolean] As Index(Of String) = {"true", "false"}

        If str.IsInteger Then
            Return New Token(Token.JSONElements.Integer, str)
        ElseIf str.IsNumeric Then
            Return New Token(Token.JSONElements.Double, str)
        ElseIf str.ToLower Like [boolean] Then
            Return New Token(Token.JSONElements.Boolean, str)
        Else
            Return New Token(Token.JSONElements.String, str)
        End If
    End Function
End Class
