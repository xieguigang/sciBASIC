Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language

    Public Class Token : Inherits CodeToken(Of Tokens)

        Sub New(type As Tokens, text As String)
            Call MyBase.New(type, text)
        End Sub

        Sub New(type As Tokens, chars As IEnumerable(Of Char))
            Call MyBase.New(type, chars.CharString)
        End Sub
    End Class
End Namespace