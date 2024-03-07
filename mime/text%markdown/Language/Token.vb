Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language

    Public Enum TokenTypes
        WhiteSpace
        Header
    End Enum

    Public Class Token : Inherits CodeToken(Of TokenTypes)

        Public styles As Styles

    End Class
End Namespace