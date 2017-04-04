Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace SVG.CSS

    Public Class ObjectStyle : Inherits ICSSValue

        Public Property stroke As Stroke
        Public Property fill As String

        Public Overrides ReadOnly Property CSSValue As String
            Get
                Return ToString()
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return stroke.ToString & " fill: " & fill
        End Function
    End Class
End Namespace