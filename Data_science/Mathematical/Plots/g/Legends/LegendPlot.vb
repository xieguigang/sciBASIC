#Region "Microsoft.VisualBasic::c6cf6636d91a071f79ae71fbc957af83, ..\sciBASIC#\Data_science\Mathematical\Plots\g\Legends\LegendPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Graphic.Legend

    Public Module LegendPlotExtensions

        ''' <summary>
        ''' 函数返回最大的那个rectange的大小
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="pos"></param>
        ''' <param name="l"></param>
        ''' <param name="graphicsSize">图例之中的图形的大小都是根据这个参数值来自动调整的</param>
        ''' <param name="border">绘制每一个图例的边</param>
        ''' <returns></returns>
        <Extension>
        Public Function DrawLegend(ByRef g As Graphics, pos As Point, graphicsSize As SizeF, l As Legend, Optional border As Stroke = Nothing, Optional radius% = 5) As SizeF
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
                    Dim b As New SolidBrush(l.color.ToColor)

                    Call Diamond.Draw(g, topLeft, New Size(d, d), b, border)

                Case LegendStyles.Hexagon

                    Dim d As Integer = Math.Min(graphicsSize.Height, graphicsSize.Width)
                    Dim topLeft As New Point(pos.X + (graphicsSize.Width - d) / 2,
                                             pos.Y + (graphicsSize.Height - d) / 2)
                    Dim b As New SolidBrush(l.color.ToColor)

                    Call Hexagon.Draw(g, topLeft, New Size(d * 1.15, d), b, border)

                Case LegendStyles.Rectangle

                    Dim dw As Integer = graphicsSize.Width * 0.1
                    Dim dh As Integer = graphicsSize.Height * 0.2

                    Call Box.DrawRectangle(
                        g, New Point(pos.X + dw, pos.Y + dh),
                        New Size(graphicsSize.Width - dw * 2,
                                 graphicsSize.Height - dh * 2),
                        New SolidBrush(l.color.ToColor), border)

                Case LegendStyles.RoundRectangle

                    Dim dw As Integer = graphicsSize.Width * 0.1
                    Dim dh As Integer = graphicsSize.Height * 0.2

                    Call RoundRect.Draw(
                        g, New Point(pos.X + dw, pos.Y + dh),
                        New Size(graphicsSize.Width - dw * 2,
                                 graphicsSize.Height - dh * 2),
                        radius,
                        New SolidBrush(l.color.ToColor), border)

                Case LegendStyles.Square
                    Dim r As Single = Math.Min(graphicsSize.Height, graphicsSize.Width)
                    Dim location As New Point(
                        pos.X + graphicsSize.Width - r,
                        pos.Y + graphicsSize.Height - r)

                    Call Box.DrawRectangle(
                        g, location,
                        New Size(r, r),
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
        ''' <paramref name="graphicSize"/>的默认值是(120,45)
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="topLeft"></param>
        ''' <param name="ls"></param>
        ''' <param name="graphicSize">
        ''' 单个legend图形的绘图区域的大小，图例之中的shap的大小都是根据这个参数来进行限制自动调整的
        ''' </param>
        ''' <param name="d%">Interval distance between the legend graphics.</param>
        ''' <param name="regionBorder">整个图例的绘图区域的边的绘制设置</param>
        ''' <param name="radius">这个是用于圆角矩形的图例图形的绘制参数</param>
        <Extension>
        Public Sub DrawLegends(ByRef g As Graphics,
                               topLeft As Point,
                               ls As IEnumerable(Of Legend),
                               Optional graphicSize As SizeF = Nothing,
                               Optional d% = 10,
                               Optional border As Stroke = Nothing,
                               Optional regionBorder As Stroke = Nothing,
                               Optional roundRectRegion As Boolean = True,
                               Optional radius% = 5)

            Dim ZERO As Point = topLeft
            Dim n As Integer
            Dim size As SizeF
            Dim legends As Legend() = ls.ToArray

            If graphicSize.IsEmpty Then
                graphicSize = New SizeF(120.0!, 45.0!)
            End If

            For Each l As Legend In legends
                n += 1
                size = g.DrawLegend(topLeft, graphicSize, l, border, radius)
                topLeft = New Point(
                    topLeft.X,
                    size.Height + d + topLeft.Y)
            Next

            If Not regionBorder Is Nothing Then
                Dim maxTitleSize As SizeF = legends.MaxLegendSize(g)

                With graphicSize
                    size = New SizeF(.Width + d + maxTitleSize.Width, Math.Max(.Height, maxTitleSize.Height) * (n + 1))
                    ZERO = New Point(ZERO.X - d / 2, ZERO.Y - d * 1.2)

                    If roundRectRegion Then
                        Call RoundRect.Draw(g, ZERO, size, 15,, regionBorder)
                    Else
                        g.DrawRectangle(
                            regionBorder.GDIObject,
                            New Rectangle(ZERO, size.ToSize))
                    End If
                End With
            End If
        End Sub

        ''' <summary>
        ''' 这个函数返回legend之中的图例的标题字符串的最大绘制大小 
        ''' </summary>
        ''' <param name="legends"></param>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MaxLegendSize(legends As IEnumerable(Of Legend), g As Graphics) As SizeF
            Dim maxW! = Single.MinValue, maxH! = Single.MinValue

            For Each l As Legend In legends
                Dim font As Font = CSSFont.TryParse(l.fontstyle)
                Dim size As SizeF = g.MeasureString(l.title, font)

                If maxW < size.Width Then
                    maxW = size.Width
                End If
                If maxH < size.Height Then
                    maxH = size.Height
                End If
            Next

            Return New SizeF(maxW, maxH)
        End Function
    End Module
End Namespace