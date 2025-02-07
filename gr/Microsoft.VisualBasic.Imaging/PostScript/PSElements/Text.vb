Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace PostScript.Elements

    Public Class Text : Inherits PSElement

        Public Property text As String
        Public Property font As CSSFont
        Public Property rotation As Single
        Public Property location As PointF

        Friend Overrides Sub WriteAscii(ps As Writer)
            ps.font(font)
            ps.color(font.color.TranslateColor)
            ps.text(text, location.X, location.Y)
        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Throw New NotImplementedException()
        End Sub
    End Class

End Namespace
