Imports System.Drawing
Imports System.Drawing.Drawing2D

Namespace Drawing2D.VectorElements

    Public Class Diamond

        Public Shared Sub Draw(ByRef g As Graphics,
                               topLeft As Point,
                               size As Size,
                               Optional br As Brush = Nothing,
                               Optional border As Border = Nothing)

            Dim a As New Point(topLeft.X + size.Width / 2, topLeft.Y)
            Dim b As New Point(topLeft.X + size.Width, topLeft.Y + size.Height / 2)
            Dim c As New Point(a.X, topLeft.Y + size.Height)
            Dim d As New Point(topLeft.X, b.Y)
            Dim diamond As New GraphicsPath

            diamond.AddLine(a, b)
            diamond.AddLine(b, c)
            diamond.AddLine(c, d)
            diamond.AddLine(d, a)
            diamond.CloseFigure()

            Call g.FillPath(If(br Is Nothing, Brushes.Black, br), diamond)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GetPen, diamond)
            End If
        End Sub
    End Class
End Namespace