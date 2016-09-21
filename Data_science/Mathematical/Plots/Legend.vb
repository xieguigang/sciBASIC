Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Legend

    Public Property style As LegendStyles
    Public Property title As String
    Public Property color As String
    ''' <summary>
    ''' CSS expression, which can be parsing by <see cref="CSSFont"/> 
    ''' </summary>
    ''' <returns></returns>
    Public Property fontstyle As String

    Public Function GetFont() As Font
        Return CSSFont.TryParse(fontstyle).GDIObject
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

''' <summary>
''' Vector shapes that drawing of this legend.
''' </summary>
Public Enum LegendStyles
    ''' <summary>
    ''' 矩形
    ''' </summary>
    Rectangle
    ''' <summary>
    ''' 圆形
    ''' </summary>
    Circle
    ''' <summary>
    ''' 实线
    ''' </summary>
    SolidLine
    ''' <summary>
    ''' 虚线
    ''' </summary>
    DashLine
    ''' <summary>
    ''' 菱形
    ''' </summary>
    Diamond
    ''' <summary>
    ''' 三角形
    ''' </summary>
    Triangle
    ''' <summary>
    ''' 六边形
    ''' </summary>
    Hexagon
    ''' <summary>
    ''' 五角星
    ''' </summary>
    Pentacle
End Enum

Public Module LegendPlotExtensions

    ''' <summary>
    ''' 函数返回最大的那个rectange的大小
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="pos"></param>
    ''' <param name="l"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DrawLegend(ByRef g As Graphics, pos As Point, graphicsSize As SizeF, l As Legend, Optional border As Border = Nothing) As SizeF
        Dim font As Font = l.GetFont
        Dim fSize As SizeF = g.MeasureString(l.title, font)

        Select Case l.style
            Case LegendStyles.Circle
                Dim r As Single = Math.Min(graphicsSize.Height, graphicsSize.Width) / 2
                Dim c As New Point(pos.X + graphicsSize.Width / 2,
                                   pos.Y + graphicsSize.Height / 2)

                Call Circle.Draw(g, c, r, New SolidBrush(l.color.ToColor), border)

            Case LegendStyles.DashLine

                Dim d As Integer = graphicsSize.Width * 0.2
                Dim a As New Point(pos.X + d, pos.Y + graphicsSize.Height / 2)
                Dim b As New Point(pos.X + graphicsSize.Width - d, a.Y)
                Dim pen As New Pen(l.color.ToColor, 3) With {
                    .DashStyle = DashStyle.Dash
                }

                Call g.DrawLine(pen, a, b)

            Case LegendStyles.Diamond

                Dim d As Integer = Math.Min(graphicsSize.Height, graphicsSize.Width)
                Dim topLeft As New Point(pos.X + (graphicsSize.Width - d) / 2,
                                         pos.Y + (graphicsSize.Height - d) / 2)

                Call Diamond.Draw(g, topLeft, New Size(d, d), New SolidBrush(l.color.ToColor), border)

            Case LegendStyles.Hexagon

                Dim d As Integer = Math.Min(graphicsSize.Height, graphicsSize.Width)
                Dim topLeft As New Point(pos.X + (graphicsSize.Width - d) / 2,
                                         pos.Y + (graphicsSize.Height - d) / 2)

                Call Hexagon.Draw(g, topLeft, New Size(d * 1.15, d), New SolidBrush(l.color.ToColor), border)

            Case LegendStyles.Rectangle

                Dim dw As Integer = graphicsSize.Width * 0.1
                Dim dh As Integer = graphicsSize.Height * 0.2

                Call Box.DrawRectangle(
                    g, New Point(pos.X + dw, pos.Y + dh),
                    New Size(graphicsSize.Width - dw * 2,
                             graphicsSize.Height - dh * 2),
                    New SolidBrush(l.color.ToColor), border)

            Case LegendStyles.SolidLine

                Dim d As Integer = graphicsSize.Width * 0.2
                Dim a As New Point(pos.X + d, pos.Y + graphicsSize.Height / 2)
                Dim b As New Point(pos.X + graphicsSize.Width - d, a.Y)
                Dim pen As New Pen(l.color.ToColor, 3) With {
                    .DashStyle = DashStyle.Solid
                }

                Call g.DrawLine(pen, a, b)

            Case LegendStyles.Triangle

                Dim d As Integer = Math.Min(graphicsSize.Height, graphicsSize.Width)
                Dim topLeft As New Point(pos.X + (graphicsSize.Width - d) / 2,
                                         pos.Y + (graphicsSize.Height - d) / 2)

                Call Triangle.Draw(g, topLeft, New Size(d, d), New SolidBrush(l.color.ToColor), border)

            Case LegendStyles.Pentacle

                Call Pentacle.Draw(g, pos, graphicsSize, New SolidBrush(l.color.ToColor), border)

            Case Else
                Throw New NotSupportedException(
                    l.style.ToString & " currently is not supported yet!")
        End Select

        Call g.DrawString(l.title,
                          font,
                          Brushes.Black,
                          New Point(pos.X + graphicsSize.Width + 5,
                                    pos.Y + (graphicsSize.Height - fSize.Height) / 2))

        If fSize.Height > graphicsSize.Height Then
            Return fSize
        Else
            Return graphicsSize
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="topLeft"></param>
    ''' <param name="ls"></param>
    ''' <param name="graphicSize">单个legend图形的绘图区域的大小</param>
    ''' <param name="d"></param>
    <Extension>
    Public Sub DrawLegends(ByRef g As Graphics,
                           topLeft As Point,
                           ls As IEnumerable(Of Legend),
                           Optional graphicSize As SizeF = Nothing,
                           Optional d As Integer = 10,
                           Optional border As Border = Nothing)

        If graphicSize.IsEmpty Then
            graphicSize = New SizeF(120, 45)
        End If

        For Each l As Legend In ls
            topLeft = New Point(
                topLeft.X,
                g.DrawLegend(
                topLeft,
                graphicSize,
                l,
                border).Height + d + topLeft.Y)
        Next
    End Sub
End Module