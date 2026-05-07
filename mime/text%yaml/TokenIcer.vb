Imports Microsoft.VisualBasic.MIME.application.json
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


    End Function

End Class
