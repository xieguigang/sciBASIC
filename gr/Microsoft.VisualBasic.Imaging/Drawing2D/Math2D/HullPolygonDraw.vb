Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace Drawing2D.Math2D

    Public Module HullPolygonDraw

        <Extension>
        Public Sub DrawHullPolygon(g As IGraphics,
                                   polygon As IEnumerable(Of PointF),
                                   color As Color,
                                   Optional strokeWidth! = 8.5,
                                   Optional alpha% = 95,
                                   Optional shadow As Boolean = True)

            Dim shape As PointF() = polygon.ToArray
            Dim alphaBrush As New SolidBrush(color.Alpha(alpha))
            Dim path = shape.buildPath(Nothing)
            Dim shadowPath = shape.buildPath(New PointF(strokeWidth / 2, strokeWidth))
            Dim stroke As New Pen(color, strokeWidth) With {
                .DashStyle = DashStyle.Dash
            }
            Dim shadowStroke As New Pen(Color.LightGray, strokeWidth) With {
                .DashStyle = stroke.DashStyle
            }

            Call g.FillPath(alphaBrush, path)
            Call g.DrawPath(shadowStroke, shadowPath)
            Call g.DrawPath(stroke, path)
        End Sub

        <Extension>
        Private Function buildPath(polygon As IEnumerable(Of PointF), offset As PointF) As GraphicsPath
            Dim path As New GraphicsPath

            Call path.AddPolygon(polygon.Select(Function(p) p.OffSet2D(offset)).ToArray)
            Call path.CloseFigure()

            Return path
        End Function
    End Module
End Namespace