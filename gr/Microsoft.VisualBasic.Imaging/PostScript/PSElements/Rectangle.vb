Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes

Namespace PostScript.Elements

    Public Class Rectangle : Inherits PSElement(Of Box)

        Friend Overrides Sub WriteAscii(ps As Writer)

        End Sub

        Friend Overrides Sub Paint(g As IGraphics)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace