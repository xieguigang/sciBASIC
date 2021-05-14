Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser

Public Class TokenIcer

    Dim buf As New CharBuffer
    Dim text As CharPtr
    Dim escape As New Escaping

    Sub New(document As CharPtr)
        Me.text = document
    End Sub

    Public Iterator Function GetTokens() As IEnumerable(Of Token)
        Dim token As New Value(Of Token)

        Do While text
            If Not token = WalkChar(++text) Is Nothing Then
                Yield CType(token, Token)
            End If
        Loop

        If buf > 0 Then
            Yield MeasureToken(New String(buf.PopAllChars))
        End If
    End Function

    Private Function WalkChar(c As Char) As Token

    End Function

    Private Function MeasureToken(text As String) As Token

    End Function

End Class

Friend Class Escaping

    Public [string] As Boolean
    Public quote As Char

End Class