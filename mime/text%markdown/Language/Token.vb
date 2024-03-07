Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language

    Public Enum TokenTypes
        WhiteSpace
        ''' <summary>
        ''' h1,h2,h3,h4
        ''' </summary>
        Header
        NewLine
    End Enum

    Public Class Token : Inherits CodeToken(Of TokenTypes)

        Public styles As Styles

        ''' <summary>
        ''' only works when token type is <see cref="TokenTypes.Header"/>
        ''' </summary>
        Public level As Integer

        Sub New(type As TokenTypes, text As String, style As Styles)
            Me.name = type
            Me.text = text
            Me.styles = style
        End Sub

    End Class
End Namespace