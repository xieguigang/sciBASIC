Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Model

    ''' <summary>
    ''' Parse a line of sentence to a set of the word tokens
    ''' </summary>
    Public Class SentenceCharWalker

        ReadOnly buf As New CharBuffer
        ReadOnly str As CharPtr
        ReadOnly url_protocols As New Regex("[a-zA-Z0-9]+[:]//")

        Sub New(line As String)
            str = line
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of String)
            Dim token As New Value(Of String)

            Do While Not str
                If Not (token = WalkChar(++str)) Is Nothing Then
                    Yield Trim(CStr(token))
                End If
            Loop

            If buf > 0 Then
                Yield Trim(New String(buf.PopAllChars))
            End If
        End Function

        Private Shared Function Trim(si As String) As String
            Return si.Trim("."c, ","c, "-"c, """"c, "'"c, " "c, vbTab)
        End Function

        Private Function WalkChar(c As Char) As String
            If c = " "c OrElse c = ASCII.TAB OrElse c = ASCII.CR OrElse c = ASCII.LF Then
                If buf > 0 Then
                    Return New String(buf.PopAllChars)
                End If
            ElseIf Char.IsSeparator(c) AndAlso c <> "-"c Then
                If buf.StartsWith(url_protocols) Then
                    buf.Add(c)
                Else
                    If buf > 0 Then
                        Return New String(buf.PopAllChars)
                    End If
                End If
            Else
                buf.Add(c)
            End If

            Return Nothing
        End Function

    End Class
End Namespace