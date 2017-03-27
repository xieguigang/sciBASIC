Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Vector.Shapes

    ''' <summary>
    ''' 绘制圆角矩形
    ''' </summary>
    Public Class RoundRect

        Public Shared Sub Draw(ByRef g As Graphics,
                               topLeft As Point,
                               size As SizeF,
                               radius%,
                               Optional br As Brush = Nothing,
                               Optional border As Stroke = Nothing)

            Dim rect As New Rectangle(topLeft, size.ToSize)
            Dim path As GraphicsPath = GetRoundedRectPath(rect, radius)

            If Not br Is Nothing Then
                Call g.FillPath(br, path)
            End If
            If Not border Is Nothing Then
                Call g.DrawPath(border, path)
            End If
        End Sub

        Public Shared Function GetRoundedRectPath(rect As Rectangle, radius%) As GraphicsPath
            Dim roundRect As Rectangle
            Dim path As New GraphicsPath

            With rect

                .Offset(-1, -1)
                roundRect = New Rectangle(
                    rect.Location,
                    New Size(radius - 1, radius - 1))

                path.AddArc(roundRect, 180, 90)     ' 左上角
                roundRect.X = .Right - radius       ' 右上角
                path.AddArc(roundRect, 270, 90)
                roundRect.Y = .Bottom - radius      ' 右下角
                path.AddArc(roundRect, 0, 90)
                roundRect.X = .Left                 ' 左下角
                path.AddArc(roundRect, 90, 90)
                path.CloseFigure()

                Return path
            End With
        End Function
    End Class
End Namespace