Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Namespace Graphic.Legend

    Public Class CircleSizeLegend

        Public Property radius As Integer()
        Public Property title As String
        Public Property titleFont As Font
        Public Property radiusFont As Font
        Public Property CircleStroke As Pen

        Public Sub Draw(g As IGraphics, layout As Rectangle)

        End Sub
    End Class
End Namespace