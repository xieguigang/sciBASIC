#Region "Microsoft.VisualBasic::c768083fcac42c909dbb967e85d1f1d2, ..\sciBASIC#\Data_science\Mathematical\Plots\Axis.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Text
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Text.HtmlParser

Namespace Graphic.Axis

    Public Module Axis

        <Extension>
        Public Sub DrawAxis(ByRef g As Graphics, region As GraphicsRegion,
                            scaler As Mapper,
                            showGrid As Boolean,
                            Optional offset As Point = Nothing,
                            Optional xlabel$ = "",
                            Optional ylabel$ = "",
                            Optional xlayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom,
                            Optional ylayout As YAxisLayoutStyles = YAxisLayoutStyles.Left,
                            Optional labelFont$ = CSSFont.PlotSubTitle,
                            Optional axisStroke$ = Stroke.AxisStroke,
                            Optional gridFill$ = "rgb(245,245,245)")
            With region
                Call g.DrawAxis(
                    .Size, .Padding,
                    scaler,
                    showGrid,
                    offset,
                    xlabel, ylabel,
                    xlayout:=xlayout, ylayout:=ylayout,
                    labelFontStyle:=labelFont,
                    axisStroke:=axisStroke, gridFill:=gridFill)
            End With
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="padding">需要根据这个值来计算出坐标轴的layout.</param>
        ''' <param name="g"></param>
        ''' <param name="size"></param>
        ''' <param name="scaler">Drawing Point data auto scaler</param>
        ''' <param name="showGrid">Show axis grid on the plot region?</param>
        ''' <param name="xlayout">修改y属性</param>
        ''' <param name="ylayout">修改x属性</param>
        <Extension>
        Public Sub DrawAxis(ByRef g As Graphics,
                            size As Size,
                            padding As Padding,
                            scaler As Mapper,
                            showGrid As Boolean,
                            Optional offset As Point = Nothing,
                            Optional xlabel$ = "",
                            Optional ylabel$ = "",
                            Optional labelFontStyle$ = CSSFont.PlotSubTitle,
                            Optional xlayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom,
                            Optional ylayout As YAxisLayoutStyles = YAxisLayoutStyles.Left,
                            Optional gridFill$ = "rgb(245,245,245)",
                            Optional gridColor$ = "white",
                            Optional axisStroke$ = Stroke.AxisStroke)

            ' 填充网格要先于坐标轴的绘制操作进行，否则会将坐标轴给覆盖掉
            Dim rect As Rectangle = padding.GetCanvasRegion(size)
            Dim tickFont As New Font(FontFace.MicrosoftYaHei, 14)
            Dim sx = scaler.XScaler(size, padding)
            Dim sy = scaler.YScaler(size, padding)
            Dim gridPenX As New Pen(gridColor.TranslateColor, 2) With {
                .DashStyle = Drawing2D.DashStyle.Dash
            }
            Dim gridPenY As New Pen(gridColor.TranslateColor, 2) With {
                .DashStyle = Drawing2D.DashStyle.Dot
            }

            Call g.FillRectangle(gridFill.GetBrush, rect)

            If scaler.dx <> 0R Then
                For Each tick In scaler.xAxis
                    Dim x = sx(tick) + offset.X
                    Dim top As New Point(x, rect.Top)
                    Dim bottom As New Point(x, rect.Bottom)

                    ' 绘制x网格线
                    Call g.DrawLine(gridPenX, top, bottom)
                Next
            End If

            If scaler.dy <> 0R Then
                For Each tick In scaler.yAxis
                    Dim y = sy(tick) + offset.Y
                    Dim left As New Point(rect.Left, y)
                    Dim right As New Point(rect.Right, y)

                    ' 绘制y网格线
                    Call g.DrawLine(gridPenY, left, right)
                Next
            End If

            Dim pen As Pen = Stroke.TryParse(axisStroke).GDIObject

            If xlayout <> XAxisLayoutStyles.None Then
                Call g.DrawX(size, padding, pen, xlabel, scaler, xlayout, offset, labelFontStyle, tickFont)
            End If
            If ylayout <> YAxisLayoutStyles.None Then
                Call g.DrawY(size, padding, pen, ylabel, scaler, ylayout, offset, labelFontStyle, tickFont)
            End If
        End Sub

        Public Property delta As Integer = 10

        <Extension> Private Sub DrawY(ByRef g As Graphics, size As Size, padding As Padding,
                                      pen As Pen, label$,
                                      scaler As Mapper,
                                      layout As YAxisLayoutStyles, offset As Point,
                                      labelFont$, tickFont As Font)

            Dim X%  ' y轴的layout的变化只需要变换x的值即可

            Select Case layout
                Case YAxisLayoutStyles.Centra
                    X = padding.Left + (size.Width - padding.Horizontal) / 2 + offset.X
                Case YAxisLayoutStyles.Right
                    X = size.Width - padding.Right + offset.X
                Case YAxisLayoutStyles.ZERO
                    X = scaler.XScaler(size, padding)(0) + offset.X
                Case Else
                    X = padding.Left + offset.X
            End Select

            Dim ZERO As New Point(X, size.Height - padding.Bottom + offset.Y) ' 坐标轴原点，需要在这里修改layout
            Dim top As New Point(X, padding.Top + offset.Y)                   ' Y轴
            Dim sy As Func(Of Single, Single) = scaler.YScaler(size, padding)

            Call g.DrawLine(pen, ZERO, top)     ' y轴

            For Each tick# In scaler.yAxis

                If scaler.dy <> 0R Then

                    Dim y! = sy(tick) + offset.Y
                    Dim axisY As New PointF(ZERO.X, y)

                    Call g.DrawLine(pen, axisY, New PointF(ZERO.X - delta, y))

                    Dim labelText = (tick).FormatNumeric(2)
                    Dim sz As SizeF = g.MeasureString(labelText, tickFont)

                    g.DrawString(labelText, tickFont, Brushes.Black, New Point(ZERO.X - delta - sz.Width, y - sz.Height / 2))
                End If
            Next

            If Not label.StripHTMLTags(stripBlank:=True).StringEmpty Then
                Dim labelImage As Image = label.__plotLabel(labelFont)

                ' y轴标签文本是旋转90度绘制于左边
                labelImage = labelImage.RotateImage(-90)

                Dim location As New Point(
                    (padding.Left - labelImage.Width) / 2,
                    (size.Height - labelImage.Height) / 2)

                Call g.DrawImageUnscaled(labelImage, location)
            End If
        End Sub

        ''' <summary>
        ''' 绘制坐标轴标签html文本
        ''' </summary>
        ''' <param name="label$"></param>
        ''' <param name="css$"></param>
        ''' <returns></returns>
        <Extension> Private Function __plotLabel(label$, css$) As Image
            Return DrawLabel(label, css)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="label$">HTML</param>
        ''' <param name="cssFont$">For html ``&lt;p>...&lt;/p>`` css style</param>
        ''' <param name="maxSize$"></param>
        ''' <returns></returns>
        Public Function DrawLabel(label$, cssFont$, Optional maxSize$ = "1600,600") As Image
            Dim g As GDIPlusDeviceHandle = New Size(1600, 600).CreateGDIDevice(Color.Transparent)
            Dim out As Image

            TextRender.RenderHTML(g.Graphics, label, cssFont,, maxWidth:=g.Width)
            out = g.ImageResource
            out = out.CorpBlank(blankColor:=Color.Transparent)

            Return out
        End Function

        <Extension> Private Sub DrawX(ByRef g As Graphics, size As Size, padding As Padding,
                                      pen As Pen, label$,
                                      scaler As Mapper,
                                      layout As XAxisLayoutStyles, offset As Point,
                                      labelFont$, tickFont As Font)
            Dim Y%

            Select Case layout
                Case XAxisLayoutStyles.Centra
                    Y = padding.Top + (size.Height - padding.Vertical) / 2 + offset.Y
                Case XAxisLayoutStyles.Top
                    Y = padding.Top + offset.Y
                Case Else
                    Y = size.Height - padding.Bottom + offset.Y
            End Select

            Dim ZERO As New Point(padding.Left + offset.X, Y)                       ' 坐标轴原点
            Dim right As New Point(size.Width - padding.Right + offset.X, Y)        ' X轴
            Dim sx = scaler.XScaler(size, padding)

            Call g.DrawLine(pen, ZERO, right)   ' X轴

            For Each tick# In scaler.xAxis

                If scaler.dx <> 0R Then
                    Dim x As Single = sx(tick) + offset.X
                    Dim axisX As New PointF(x, ZERO.Y)

                    Dim labelText = (tick).FormatNumeric(2)
                    Dim sz As SizeF = g.MeasureString(labelText, tickFont)

                    Call g.DrawLine(pen, axisX, New PointF(x, ZERO.Y + padding.Top * 0.2))
                    Call g.DrawString(labelText, tickFont, Brushes.Black, New Point(x - sz.Width / 2, ZERO.Y + padding.Top * 0.3))
                End If
            Next

            If Not label.StripHTMLTags(stripBlank:=True).StringEmpty Then
                Dim labelImage As Image = label.__plotLabel(labelFont)
                Call g.DrawImageUnscaled(
                    labelImage,
                    New Point((size.Width - labelImage.Width) / 2,
                              size.Height - padding.Bottom + (padding.Bottom - labelImage.Height) * 0.5))
            End If
        End Sub
    End Module
End Namespace