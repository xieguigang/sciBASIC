#Region "Microsoft.VisualBasic::6d74f93d76061ef74bce15538772d2b8, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Shapes\Diamond.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Diamond
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Shapes

    Public Class Diamond

        Public Shared Sub Draw(ByRef g As IGraphics,
                               topLeft As Point,
                               size As Size,
                               Optional br As Brush = Nothing,
                               Optional border As Stroke = Nothing)

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

            Call g.FillPath(br Or BlackBrush, diamond)

            If Not border Is Nothing Then
                Call g.DrawPath(border.GDIObject, diamond)
            End If
        End Sub
    End Class
End Namespace
