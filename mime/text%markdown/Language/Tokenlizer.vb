Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class Tokenlizer

        Dim buf As CharBuffer
        Dim s As CharPtr
        Dim escape As New escapes

        Private Class escapes

            Public code_span As Boolean
            Public code_block As Boolean
            Public quot_block As Boolean

        End Class

        Sub New(text As String)
            s = text
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim tokens As Value(Of Token()) = {}

            Do While (tokens = WalkChar(++s).ToArray).Length > 0
                For Each t As Token In tokens.AsEnumerable
                    Yield t
                Next
            Loop
        End Function

        Private Iterator Function WalkChar(c As Char) As IEnumerable(Of Token)
            If c = "#"c Then

            End If
        End Function
    End Class
End Namespace