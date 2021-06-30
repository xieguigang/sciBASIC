Imports System.Drawing
Imports Microsoft.VisualBasic.Language

Namespace Drawing2D.Math2D.MarchingSquares

    Public Class GeneralPath

        Dim polygons As New List(Of PointF())
        Dim temp As New List(Of PointF)
        Dim level As Double

        Sub New(level As Double)
            Me.level = level
        End Sub

        Friend Sub moveTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Friend Sub lineTo(x As Double, y As Double)
            temp.Add(New PointF(x, y))
        End Sub

        Public Sub Fill(canvas As IGraphics, color As Brush)
            For Each polygon In polygons
                Call canvas.FillPolygon(color, polygon)
            Next
        End Sub

        Public Sub Draw(canvas As IGraphics, border As Pen)
            For Each polygon In polygons
                Call canvas.DrawPolygon(border, polygon)
            Next
        End Sub

        Friend Sub closePath()
            polygons.Add(temp.PopAll)
        End Sub
    End Class
End Namespace