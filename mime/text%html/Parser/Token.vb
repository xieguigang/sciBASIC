Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Class Token : Inherits CodeToken(Of HtmlTokens)

    Sub New(type As HtmlTokens, text As String)
        Call MyBase.New(type, text)
    End Sub
End Class
