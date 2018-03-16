Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D

    Public Module HullPolygonDraw

        <Extension>
        Public Sub DrawHullPolygon(g As IGraphics, polygon As IEnumerable(Of PointF), color As Color, Optional strokeWidth! = 3, Optional alpha% = 180)
            Dim path As New GraphicsPath

            Call path.AddPolygon(polygon.ToArray)
            Call path.CloseFigure()

            Dim alphaBrush As New SolidBrush(color.Alpha(alpha))
            Dim stroke As New Pen(color, strokeWidth) With {
                .DashStyle = DashStyle.Dash
            }

            Call g.FillPath(alphaBrush, path)
            Call g.DrawPath(stroke, path)
        End Sub
    End Module
End Namespace