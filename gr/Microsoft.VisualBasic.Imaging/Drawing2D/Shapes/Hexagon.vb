Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Drawing2D.Vector.Shapes

    ''' <summary>
    ''' 六边形
    ''' </summary>
    Public Class Hexagon

        Public Shared Sub Draw(ByRef g As Graphics, topLeft As Point, size As Size, Optional br As Brush = Nothing, Optional border As Border = Nothing)
            Dim rect As New Rectangle(topLeft, size)
            Dim a As New Point(topLeft.X + size.Width / 4, topLeft.Y)
            Dim b As New Point(topLeft.X + size.Width * 3 / 4, topLeft.Y)
            Dim c As New Point(rect.Right, topLeft.Y + size.Height / 2)
            Dim d As New Point(b.X, rect.Bottom)
            Dim e As New Point(a.X, rect.Bottom)
            Dim f As New Point(topLeft.X, c.Y)
            Dim hex As New GraphicsPath

            Call hex.AddLine(a, b)
            Call hex.AddLine(b, c)
            Call hex.AddLine(c, d)
            Call hex.AddLine(d, e)
            Call hex.AddLine(e, f)
            Call hex.AddLine(f, a)
            Call hex.CloseAllFigures()

            Call g.FillPath(If(br Is Nothing, Brushes.Black, br), hex)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GetPen, hex)
            End If
        End Sub
    End Class
End Namespace