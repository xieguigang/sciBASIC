Imports Microsoft.VisualBasic.Scripting.TokenIcer

Public Class MathToken : Inherits CodeToken(Of MathTokens)

    Sub New(name As MathTokens, text As String)
        Call MyBase.New(name, text)
    End Sub
End Class

Public Enum MathTokens
    Invalid
    Literal
    [Operator]
    Open
    Close
    Symbol
End Enum