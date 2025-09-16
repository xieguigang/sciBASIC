#Region "Microsoft.VisualBasic::ce64ba3f7adb48422b0da783e59c4e85, Data_science\Visualization\Plots\g\Legends\LegendPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 399
    '    Code Lines: 283 (70.93%)
    ' Comment Lines: 55 (13.78%)
    '    - Xml Docs: 89.09%
    ' 
    '   Blank Lines: 61 (15.29%)
    '     File Size: 16.48 KB


    '     Module LegendPlotExtensions
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DrawLegend, LegendStyls, MaxLegendSize, ParseLegendStyle
    ' 
    '         Sub: DrawLegends, DrawLegendShape, DrawShape
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
#End If

Namespace Graphic.Legend

    <HideModuleName>
    Public Module LegendPlotExtensions

        ReadOnly legendExpressions As Dictionary(Of String, LegendStyles)

        Sub New()
            ' parse the shape enums and the corresponding element description text
            ' used the shape symbol name and the description key as the text parser 
            ' key inputs
            legendExpressions = Enums(Of LegendStyles) _
                .Select(Iterator Function(f) As IEnumerable(Of (key As String, flag As LegendStyles))
                            Yield (f.ToString.ToLower, f)
                            Yield (f.Description.ToLower, f)
                        End Function) _
                .IteratesALL _
                .GroupBy(Function(f) f.key) _
                .ToDictionary(Function(f) f.Key,
                              Function(f)
                                  Return f.First.flag
                              End Function)
        End Sub

        ''' <summary>
        ''' 从字符串表达式之中解析出<see cref="LegendStyles"/>
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="defaultStyle"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ParseLegendStyle(str$, Optional defaultStyle As LegendStyles = LegendStyles.Circle) As LegendStyles
            With LCase(str)
                If legendExpressions.ContainsKey(.ToString) Then
                    Return legendExpressions(.ToString)
                Else
                    Return defaultStyle
                End If
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="expr$">``a,b,c,d...``</param>
        ''' <returns></returns>
        <Extension>
        Public Function LegendStyls(expr$) As LegendStyles()
            Return expr _
                .Split(","c) _
                .Select(AddressOf Trim) _
                .Select(AddressOf ParseLegendStyle) _
                .ToArray
        End Function

        Public Sub DrawShape(g As IGraphics, pos As PointF, gSize As SizeF, shape As String, color As Brush, border As Stroke, radius%, ByRef labelPos As PointF, lineWidth!)
            Call g.DrawLegendShape(pos, gSize, ParseLegendStyle(shape), color, border, radius, labelPos, lineWidth)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g">Canvas device</param>
        ''' <param name="pos">The top left location of the shape rectangle</param>
        ''' <param name="gSize">The shape size</param>
        ''' <param name="style">The shape style</param>
        ''' <param name="color">The shape color</param>
        ''' <param name="border">The shape polygon border, nothing means no border line will be draw.</param>
        ''' <param name="radius%"></param>
        <Extension>
        Public Sub DrawLegendShape(g As IGraphics,
                                   pos As PointF,
                                   gSize As SizeF,
                                   style As LegendStyles,
                                   color As Brush,
                                   Optional border As Stroke = Nothing,
                                   Optional radius% = 5,
                                   Optional ByRef labelPos As PointF = Nothing,
                                   Optional lineWidth! = 10)
            Select Case style

                Case LegendStyles.Circle
                    Dim r As Single = std.Min(gSize.Height, gSize.Width) / 2
                    Dim c As New Point With {
                        .X = pos.X + r,
                        .Y = pos.Y + r
                    }

                    labelPos = New PointF With {
                        .X = std.Max(c.X + r, labelPos.X),
                        .Y = labelPos.Y
                    }

                    Call Circle.Draw(g, c, r, color, border)

                Case LegendStyles.DashLine

                    Dim d As Integer = gSize.Width * 0.1
                    Dim a As New Point(pos.X + d, pos.Y + gSize.Height / 2)
                    Dim b As New Point(pos.X + gSize.Width - d, a.Y)
                    Dim pen As New Pen(color, lineWidth) With {
                        .DashStyle = DashStyle.Dot
                    }

                    Call g.DrawLine(pen, a, b)

                Case LegendStyles.Diamond

                    Dim d As Integer = std.Min(gSize.Height, gSize.Width)
                    Dim topLeft As New Point With {
                        .X = pos.X + (gSize.Width - d) / 2,
                        .Y = pos.Y + (gSize.Height - d) / 2
                    }

                    Call Diamond.Draw(g, topLeft, New Size(d, d), color, border)

                Case LegendStyles.Hexagon

                    Dim d As Integer = std.Min(gSize.Height, gSize.Width)
                    Dim topLeft As New Point With {
                        .X = pos.X + (gSize.Width - d) / 2,
                        .Y = pos.Y + (gSize.Height - d) / 2
                    }

                    Call Hexagon.Draw(g, topLeft, New Size(d * 1.15, d), color, border)

                Case LegendStyles.Rectangle

                    Dim dw As Integer = gSize.Width * 0.45
                    Dim dh As Integer = gSize.Height * 0.1
                    Dim size As New Size With {
                        .Width = dw,
                        .Height = gSize.Height - dh * 2
                    }

                    Call Box.DrawRectangle(
                        g, New Point(pos.X + dw, pos.Y + dh),
                        size,
                        color, border
                    )

                Case LegendStyles.RoundRectangle

                    Dim dw As Integer = gSize.Width * 0.1
                    Dim dh As Integer = gSize.Height * 0.2
                    Dim size As New Size With {
                        .Width = gSize.Width - dw * 2,
                        .Height = gSize.Height - dh * 2
                    }

                    Call RoundRect.Draw(
                        g, New Point(pos.X + dw, pos.Y + dh),
                        size,
                        radius,
                        color, border)

                Case LegendStyles.Square
                    Dim r As Single = std.Min(gSize.Height, gSize.Width)
                    Dim location As New Point With {
                        .X = pos.X + gSize.Width - r,
                        .Y = pos.Y + gSize.Height - r
                    }

                    Call Box.DrawRectangle(
                        g, location,
                        New Size(r, r),
                        color, border)

                Case LegendStyles.SolidLine

                    Dim d As Integer = gSize.Width * 0.1
                    Dim a As New Point(pos.X + d, pos.Y + gSize.Height / 2)
                    Dim b As New Point(pos.X + gSize.Width - d, a.Y)
                    Dim pen As New Pen(color, lineWidth) With {
                        .DashStyle = DashStyle.Solid
                    }

                    Call g.DrawLine(pen, a, b)

                Case LegendStyles.Triangle

                    Dim d As Integer = std.Min(gSize.Height, gSize.Width)
                    Dim topLeft As New Point With {
                        .X = pos.X + (gSize.Width - d) / 2,
                        .Y = pos.Y + (gSize.Height - d) / 2
                    }

                    labelPos = New PointF With {
                        .X = topLeft.X + d + 5,
                        .Y = labelPos.Y
                    }

                    Call Triangle.Draw(g, topLeft, New Size(d, d), color, border)

                Case LegendStyles.Pentacle

                    Call Pentacle.Draw(g, pos, gSize, color, border)

                Case Else
                    Throw New NotSupportedException(
                        style.ToString & " currently is not supported yet!")

            End Select
        End Sub

        ''' <summary>
        ''' 函数返回最大的那个rectange的大小
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="pos"></param>
        ''' <param name="l"></param>
        ''' <param name="canvas">图例之中的图形的大小都是根据这个参数值来自动调整的</param>
        ''' <param name="border">绘制每一个图例的边</param>
        ''' <returns></returns>
        <Extension>
        Public Function DrawLegend(ByRef g As IGraphics,
                                   pos As PointF,
                                   canvas As SizeF,
                                   l As LegendObject,
                                   Optional border As Stroke = Nothing,
                                   Optional radius% = 5,
                                   Optional titleBrush As Brush = Nothing,
                                   Optional lineWidth! = -1) As SizeF

            Dim font As Font = l.GetFont(g.LoadEnvironment)
            Dim fSize As SizeF = g.MeasureString(l.title, font)
            Dim labelPosition As New PointF With {
                .X = pos.X + canvas.Width + 5,
                .Y = pos.Y + (canvas.Height - fSize.Height) / 2
            }
            Dim color As Brush = l.color.GetBrush

            Static blackTitle As [Default](Of Brush) = Brushes.Black

            If lineWidth <= 0 Then
                lineWidth = font.Size / 2
            End If

            SyncLock blackTitle.DefaultValue
                Call g.DrawLegendShape(
                    pos, canvas, l.style, color,
                    border:=border,
                    radius:=radius,
                    labelPos:=labelPosition,
                    lineWidth:=lineWidth
                )
                Call g.DrawString(
                    l.title,
                    font,
                    brush:=titleBrush Or blackTitle,
                    point:=labelPosition
                )
            End SyncLock

            If fSize.Height > canvas.Height Then
                Return fSize
            Else
                Return canvas
            End If
        End Function

        ''' <summary>
        ''' <paramref name="gsize"/>的默认值是(120,45)
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="topLeft"></param>
        ''' <param name="legends"></param>
        ''' <param name="fillBg">
        ''' The background color of the legend area, by default is transparent
        ''' </param>
        ''' <param name="gSize">
        ''' 单个legend图形的绘图区域的大小，图例之中的shap的大小都是根据这个参数来进行限制自动调整的
        ''' </param>
        ''' <param name="d%">Interval distance between the legend graphics.</param>
        ''' <param name="regionBorder">整个图例的绘图区域的边框的绘制设置，如果这个参数值是空值，则不会做边框的绘制</param>
        ''' <param name="radius">这个是用于圆角矩形的图例图形的绘制参数</param>
        <Extension>
        Public Sub DrawLegends(ByRef g As IGraphics,
                               topLeft As PointF,
                               legends As IEnumerable(Of LegendObject),
                               Optional gSize$ = "120,45",
                               Optional fillBg$ = Nothing,
                               Optional d% = 10,
                               Optional shapeBorder As Stroke = Nothing,
                               Optional regionBorder As Stroke = Nothing,
                               Optional roundRectRegion As Boolean = True,
                               Optional radius% = 5,
                               Optional titleBrush As Brush = Nothing)

            Dim ZERO As PointF = topLeft
            Dim size As SizeF
            Dim legendList As LegendObject() = legends.ToArray
            Dim graphicSize As SizeF = gSize.FloatSizeParser
            Dim css As CSSEnvirnment = g.LoadEnvironment

            If Not regionBorder Is Nothing Then
                Dim maxTitleSize As SizeF = legendList.MaxLegendSize(g)
                Dim rect As RectangleF

                With graphicSize

                    Dim width! = .Width + .Height * 1.25 + maxTitleSize.Width
                    Dim height! = (std.Max(.Height, maxTitleSize.Height) + d + 1.25) * legendList.Length
                    Dim background As Brush = Nothing

                    If Not fillBg.StringEmpty Then
                        background = fillBg.GetBrush
                    End If

                    size = New SizeF(width, height)
                    ZERO = New Point(ZERO.X - d, ZERO.Y - d * 1.2)

                    If roundRectRegion Then
                        Call RoundRect.Draw(g, ZERO, size, 15, background, regionBorder)
                    Else
                        rect = New RectangleF(ZERO, size)

                        If Not background Is Nothing Then
                            Call g.FillRectangle(background, rect)
                        End If

                        Call g.DrawRectangle(pen:=css.GetPen(regionBorder), rect:=rect)
                    End If
                End With
            End If

            For Each l As LegendObject In legendList
                size = g.DrawLegend(topLeft, graphicSize, l, shapeBorder, radius, titleBrush)
                topLeft = New Point With {
                    .X = topLeft.X,
                    .Y = size.Height + d + topLeft.Y
                }
            Next
        End Sub

        ''' <summary>
        ''' 这个函数返回legend之中的图例的标题字符串的最大绘制大小 
        ''' </summary>
        ''' <param name="legends"></param>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MaxLegendSize(legends As IEnumerable(Of LegendObject), g As IGraphics) As SizeF
            Dim maxW! = Single.MinValue, maxH! = Single.MinValue
            Dim css As CSSEnvirnment = g.LoadEnvironment

            For Each l As LegendObject In legends
                Dim font As Font = css.GetFont(CSSFont.TryParse(l.fontstyle))
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
