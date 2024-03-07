Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class Tokenlizer

        Dim buf As CharBuffer
        Dim s As CharPtr
        Dim styles As Styles

        Sub New(text As String)
            s = text
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim tokens As Value(Of Token()) = {}

            Do While (tokens = WalkChar(++s).ToArray).Length > 0
                For Each t As Token In tokens.AsEnumerable
                    If Not t Is Nothing Then
                        Yield t
                    End If
                Next
            Loop
        End Function

        Private Iterator Function WalkChar(c As Char) As IEnumerable(Of Token)
            If c = "#"c Then
                If buf > 0 AndAlso buf.Last <> "#"c Then
                    Yield measure()
                End If

                buf += c

            End If
        End Function

        Private Function measure() As Token

        End Function
    End Class
End Namespace