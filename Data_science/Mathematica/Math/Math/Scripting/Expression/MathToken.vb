Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Scripting.MathExpression

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
        Comma
        Terminator
    End Enum
End Namespace