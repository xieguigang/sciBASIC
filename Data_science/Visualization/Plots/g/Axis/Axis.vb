#Region "Microsoft.VisualBasic::ae05e362a3dceb1e835fcca67aab5abc, Data_science\Visualization\Plots\g\Axis\Axis.vb"

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

    '     Module Axis
    ' 
    '         Properties: delta
    ' 
    '         Function: __plotLabel, (+2 Overloads) DrawLabel
    ' 
    '         Sub: checkScaler, (+2 Overloads) DrawAxis, DrawString, DrawX, DrawY
    '              DrawYGrid
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser
Imports stdNum = System.Math

Namespace Graphic.Axis

    Public Module Axis

        ''' <summary>
        ''' 绘制按照任意角度旋转的文本
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="text"></param>
        ''' <param name="font"></param>
        ''' <param name="brush"></param>
        ''' <param name="x!"></param>
        ''' <param name="y!"></param>
        ''' <param name="angle!"></param>
        <Extension>
        Public Sub DrawString(g As IGraphics, text$, font As Font, brush As Brush, x!, y!, angle!)
            With g
                Call g.TranslateTransform(.Size.Width / 2, .Size.Height / 2)
                Call g.RotateTransform(angle)

                ' 不清楚旋转之后会不会对字符串的大小产生影响，所以measureString放在旋转之后
                Dim textSize As SizeF = g.MeasureString(text, font)

                Call g.DrawString(text, font, brush, -(textSize.Width / 2), -(textSize.Height / 2))
                Call g.ResetTransform()
            End With
        End Sub

        ''' <summary>
        ''' 一般而言，``X``坐标轴是绘制在<paramref name="region"/>的底部的
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="region"></param>
        ''' <param name="scaler"></param>
        ''' <param name="showGrid"></param>
        ''' <param name="offset"></param>
        ''' <param name="xlabel$"></param>
        ''' <param name="ylabel$"></param>
        ''' <param name="xlayout"></param>
        ''' <param name="ylayout"></param>
        ''' <param name="labelFont$"></param>
        ''' <param name="axisStroke$"></param>
        ''' <param name="gridFill$"></param>
        ''' <param name="htmlLabel"></param>
        ''' <param name="XtickFormat$"></param>
        ''' <param name="YtickFormat$"></param>
        ''' <param name="tickFontStyle$"></param>
        <Extension>
        Public Sub DrawAxis(ByRef g As IGraphics, region As GraphicsRegion,
                            scaler As DataScaler,
                            showGrid As Boolean,
                            Optional offset As Point = Nothing,
                            Optional xlabel$ = "",
                            Optional ylabel$ = "",
                            Optional xlayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom,
                            Optional ylayout As YAxisLayoutStyles = YAxisLayoutStyles.Left,
                            Optional labelFont$ = CSSFont.Win7Large,
                            Optional axisStroke$ = Stroke.AxisStroke,
                            Optional gridFill$ = "rgb(245,245,245)",
                            Optional gridX$ = Stroke.AxisGridStroke,
                            Optional gridY$ = Stroke.AxisGridStroke,
                            Optional htmlLabel As Boolean = False,
                            Optional XtickFormat$ = "F2",
                            Optional YtickFormat$ = "F2",
                            Optional tickFontStyle$ = CSSFont.Win10NormalLarger)
            With region
                Call g.DrawAxis(
                    scaler,
                    .ByRef,
                    showGrid,
                    offset,
                    xlabel, ylabel,
                    xlayout:=xlayout, ylayout:=ylayout,
                    labelFontStyle:=labelFont,
                    axisStroke:=axisStroke, gridFill:=gridFill, htmlLabel:=htmlLabel,
                    XtickFormat:=XtickFormat,
                    YtickFormat:=YtickFormat,
                    tickFontStyle:=tickFontStyle,
                    gridX:=gridX,
                    gridY:=gridY
                )
            End With
        End Sub

        <Extension>
        Private Sub checkScaler(scaler As DataScaler)
            If scaler.X.domainSize = 0.0 Then
                Throw New InvalidProgramException("the x axis range length is ZERO!")
            ElseIf scaler.Y.valueDomain.Length = 0.0 Then
                Throw New InvalidProgramException("the y axis range length is ZERO!")
            Else
                ' pass
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="scaler">Drawing Point data auto scaler</param>
        ''' <param name="showGrid">Show axis grid on the plot region?</param>
        ''' <param name="xlayout">修改y属性</param>
        ''' <param name="ylayout">修改x属性</param>
        ''' <param name="gridX">空值表示不进行绘制</param>
        <Extension>
        Public Sub DrawAxis(ByRef g As IGraphics,
                            scaler As DataScaler,
                            region As GraphicsRegion,
                            showGrid As Boolean,
                            Optional offset As Point = Nothing,
                            Optional xlabel$ = "",
                            Optional ylabel$ = "",
                            Optional labelFontStyle$ = CSSFont.PlotSubTitle,
                            Optional xlayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom,
                            Optional ylayout As YAxisLayoutStyles = YAxisLayoutStyles.Left,
                            Optional gridFill$ = "rgb(245,245,245)",
                            Optional gridX$ = Stroke.AxisGridStroke,
                            Optional gridY$ = Stroke.AxisGridStroke,
                            Optional axisStroke$ = Stroke.AxisStroke,
                            Optional tickFontStyle$ = CSSFont.Win7Normal,
                            Optional htmlLabel As Boolean = True,
                            Optional XtickFormat$ = "F2",
                            Optional YtickFormat$ = "F2")

            ' 填充网格要先于坐标轴的绘制操作进行，否则会将坐标轴给覆盖掉
            Dim rect As Rectangle = scaler.region
            Dim tickFont As Font = CSSFont.TryParse(tickFontStyle)
            Dim gridPenX As Pen = Stroke.TryParse(gridX)
            Dim gridPenY As Pen = Stroke.TryParse(gridY)

            Call scaler.checkScaler
            Call g.FillRectangle(gridFill.GetBrush, rect)

            If showGrid AndAlso Not scaler.AxisTicks.X.IsNullOrEmpty Then
                Dim ticks As Double()

                If TypeOf scaler.X Is OrdinalScale Then
                    ticks = DirectCast(scaler.X, OrdinalScale) _
                        .getTerms _
                        .Objects _
                        .Select(Function(label) scaler.X(label)) _
                        .ToArray
                Else
                    ticks = scaler.AxisTicks.X _
                        .Select(Function(xi) scaler.X(xi)) _
                        .ToArray
                End If

                ' nothing for not drawing
                If Not gridPenX Is Nothing Then
                    For Each tick As Double In ticks
                        Dim x As Single = tick + offset.X
                        Dim top As New PointF(x, rect.Top)
                        Dim bottom As New PointF(x, rect.Bottom)

                        ' 绘制x网格线
                        Call g.DrawLine(gridPenX, top, bottom)
                    Next
                End If
            End If

            If showGrid AndAlso (Not scaler.AxisTicks.Y.IsNullOrEmpty) AndAlso Not gridPenY Is Nothing Then
                For Each tick In scaler.AxisTicks.Y
                    Dim y = scaler.TranslateY(tick) + offset.Y
                    Dim left As New Point(rect.Left, y)
                    Dim right As New Point(rect.Right, y)

                    ' 绘制y网格线
                    Call g.DrawLine(gridPenY, left, right)
                Next
            End If

            Dim pen As Pen = Stroke.TryParse(axisStroke).GDIObject

            If xlayout <> XAxisLayoutStyles.None Then
                Call g.DrawX(pen, xlabel, scaler, xlayout, scaler.Y(0), offset,
                             labelFontStyle,
                             tickFont,
                             htmlLabel:=htmlLabel,
                             tickFormat:=XtickFormat
                     )
            End If
            If ylayout <> YAxisLayoutStyles.None Then
                Call g.DrawY(pen, ylabel,
                             scaler, scaler.X.Zero, scaler.AxisTicks.Y,
                             ylayout, offset,
                             labelFontStyle,
                             tickFont,
                             htmlLabel:=htmlLabel,
                             tickFormat:=YtickFormat
                )
            End If
        End Sub

        <Extension>
        Public Sub DrawYGrid(scaler As DataScaler, g As IGraphics, region As GraphicsRegion,
                             pen As Pen,
                             label$,
                             Optional offset As Point = Nothing,
                             Optional labelFont$ = CSSFont.Win7Large,
                             Optional tickFont$ = CSSFont.Win10NormalLarger,
                             Optional gridStroke$ = Stroke.AxisGridStroke)
            With region
                Dim rect As Rectangle = .Padding.GetCanvasRegion(.Size)
                Dim gridPen As Pen = Stroke.TryParse(css:=gridStroke)

                For Each tick As Double In scaler.AxisTicks.Y
                    Dim y = scaler.TranslateY(tick) + offset.Y
                    Dim left As New Point(rect.Left, y)
                    Dim right As New Point(rect.Right, y)

                    ' 绘制y网格线
                    Call g.DrawLine(gridPen, left, right)
                Next

                Call g.DrawY(pen, label,
                             scaler, scaler.X(0), scaler.AxisTicks.Y,
                             YAxisLayoutStyles.Left,
                             offset,
                             labelFont, CSSFont.TryParse(tickFont),
                             False
                     )
            End With
        End Sub

        Public Property delta As Integer = 10

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="pen"></param>
        ''' <param name="label$"></param>
        ''' <param name="X0">
        ''' 当X等于零的时候的横坐标轴的值，这个参数只有在<paramref name="layout"/>的值为
        ''' <see cref="YAxisLayoutStyles.ZERO"/>的时候才会需要.
        ''' </param>
        ''' <param name="scaler"></param>
        ''' <param name="layout"></param>
        ''' <param name="offset"></param>
        ''' <param name="labelFont$"></param>
        ''' <param name="tickFont"></param>
        ''' <param name="showAxisLine"></param>
        ''' <param name="htmlLabel">
        ''' Parameter <paramref name="label"/> is using html text format, function will using html renderer to draw this label
        ''' </param>
        <Extension> Public Sub DrawY(ByRef g As IGraphics,
                                     pen As Pen, label$,
                                     scaler As YScaler,
                                     X0#, yTicks As Vector,
                                     layout As YAxisLayoutStyles, offset As Point,
                                     labelFont$,
                                     tickFont As Font,
                                     Optional showAxisLine As Boolean = True,
                                     Optional htmlLabel As Boolean = True,
                                     Optional tickFormat$ = "F2")

            Dim X%  ' y轴的layout的变化只需要变换x的值即可
            Dim size = scaler.region.Size

            Select Case layout
                Case YAxisLayoutStyles.Centra
                    X = scaler.region.Left + (size.Width) / 2 + offset.X
                Case YAxisLayoutStyles.Right
                    X = scaler.region.Left + size.Width + offset.X
                Case YAxisLayoutStyles.ZERO
                    X = X0 + offset.X
                Case Else
                    X = scaler.region.Left + offset.X
            End Select

            Dim top As New Point(X, scaler.TranslateY(yTicks.Max) + offset.Y)  ' Y轴
            Dim ZERO As New Point(X, scaler.TranslateY(yTicks.Min) + offset.Y) ' 坐标轴原点，需要在这里修改layout

            If showAxisLine Then
                Call g.DrawLine(pen, ZERO, top)     ' y轴
            End If

            Dim maxYTickSize!

            If Not yTicks.IsNullOrEmpty Then
                Dim p As PointF

                For Each tick As Double In yTicks
                    Dim y! = scaler.TranslateY(tick) + offset.Y
                    Dim axisY As New PointF(ZERO.X, y)

                    If showAxisLine Then
                        If layout = YAxisLayoutStyles.Right Then
                            Call g.DrawLine(pen, axisY, New PointF(ZERO.X + delta, y))
                        Else
                            Call g.DrawLine(pen, axisY, New PointF(ZERO.X - delta, y))
                        End If
                    End If

                    Dim labelText = (tick).ToString(tickFormat)
                    Dim sz As SizeF = g.MeasureString(labelText, tickFont)

                    If layout = YAxisLayoutStyles.Right Then
                        p = New PointF(ZERO.X + delta, y - sz.Height / 2)
                    Else
                        p = New PointF(ZERO.X - delta - sz.Width, y - sz.Height / 2)
                    End If

                    If sz.Width > maxYTickSize Then
                        maxYTickSize = sz.Width
                    End If

                    g.DrawString(labelText, tickFont, Brushes.Black, p)
                Next
            End If

            If Not label.StripHTMLTags(stripBlank:=True).StringEmpty Then
                If htmlLabel Then
                    Dim labelImage As Image = label.__plotLabel(labelFont, False)

                    ' y轴标签文本是旋转90度绘制于左边
                    labelImage = labelImage.RotateImage(-90)

                    Dim location As New Point With {
                        .X = scaler.region.Left - labelImage.Width + maxYTickSize,
                        .Y = (size.Height - labelImage.Height) / 2 + scaler.region.Top
                    }

                    Call g.DrawImageUnscaled(labelImage, location)
                Else
                    Dim font As Font = CSSFont.TryParse(labelFont)
                    Dim fSize As SizeF = g.MeasureString(label, font)
                    Dim location As New PointF With {
                        .X = scaler.region.Left - fSize.Height - maxYTickSize * 1.5,
                        .Y = fSize.Width + (size.Height - fSize.Width) / 2 + scaler.region.Top
                    }

                    If location.X < 5 Then
                        location = New PointF(5, location.Y)
                    End If

                    ' Call $"[Y:={label}] {location.ToString}".__INFO_ECHO
                    If TypeOf g Is Graphics2D Then
                        With New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                            Call .DrawString(label, font, Brushes.Black, location, -90)
                        End With
                    Else
                        Call g.DrawString(label, font, Brushes.Black, location)
                    End If
                End If
            End If
        End Sub

        ''' <summary>
        ''' 绘制坐标轴标签html文本
        ''' </summary>
        ''' <param name="label$"></param>
        ''' <param name="css$"></param>
        ''' <returns></returns>
        <Extension> Private Function __plotLabel(label$, css$, Optional throwEx As Boolean = True) As Image
            Try
                Return TextRender.DrawHtmlText(label, css)
            Catch ex As Exception
                If throwEx Then
                    Throw
                Else
                    Call App.LogException(ex)
                    Return New Bitmap(1, 1)
                End If
            End Try
        End Function

        ''' <summary>
        ''' 这个函数不是将文本作为html来进行渲染，而是直接使用gdi进行绘图，如果需要将文本
        ''' 作为html渲染出来，则需要使用<see cref="TextRender.DrawHtmlText"/>方法
        ''' </summary>
        ''' <param name="label$"></param>
        ''' <param name="css$"><see cref="CssFont"/></param>
        ''' <param name="fcolor">Brush color or texture.</param>
        ''' <returns></returns>
        <Extension>
        Public Function DrawLabel(label$, css$, Optional fcolor$ = "black", Optional size$ = "1440,900") As Image
            Dim font As Font = CSSFont.TryParse(css, [default]:=New Font(FontFace.MicrosoftYaHei, 12)).GDIObject
            Return label.DrawLabel(font, fcolor, size)
        End Function

        ''' <summary>
        ''' 这个函数不是将文本作为html来进行渲染，而是直接使用gdi进行绘图，如果需要将文本
        ''' 作为html渲染出来，则需要使用<see cref="TextRender.DrawHtmlText"/>方法
        ''' </summary>
        ''' <param name="label$"></param>
        ''' <param name="font"></param>
        ''' <param name="fcolor$"></param>
        ''' <param name="size$">
        ''' 假若程序是运行在低内存的机器之上，则大小值应该尽量设置小些，避免内存被浪费，
        ''' 测试发现在32bit系统上经常会出现OutOfmemory的错误，将这个大小值改小之后
        ''' 一切恢复正常
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function DrawLabel(label$, font As Font, Optional fcolor$ = "black", Optional size$ = "500,300") As Image
            Using g As Graphics2D = size.SizeParser.CreateGDIDevice(Color.Transparent)
                With g
                    Dim b As Brush = fcolor.GetBrush

                    Call .DrawString(label, font, b, New Point)

                    Dim img As Image =
                        .ImageResource _
                        .CorpBlank(blankColor:=Color.Transparent) _
                        .RotateImage(-90)
                    Return img
                End With
            End Using
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="pen"></param>
        ''' <param name="label$"></param>
        ''' <param name="scaler"></param>
        ''' <param name="layout"></param>
        ''' <param name="offset"></param>
        ''' <param name="labelFont$"></param>
        ''' <param name="tickFont"></param>
        ''' <param name="overridesTickLine%"></param>
        ''' <param name="noTicks"></param>
        ''' <param name="htmlLabel">
        ''' Parameter <paramref name="label"/> is using html text format, function will using html renderer to draw this label
        ''' </param>
        <Extension> Public Sub DrawX(ByRef g As IGraphics,
                                     pen As Pen, label$,
                                     scaler As DataScaler,
                                     layout As XAxisLayoutStyles, Y0#, offset As Point,
                                     labelFont$,
                                     tickFont As Font,
                                     Optional overridesTickLine% = -1,
                                     Optional noTicks As Boolean = False,
                                     Optional htmlLabel As Boolean = True,
                                     Optional tickFormat$ = "F2")

            Dim Y% = scaler.region.Top + offset.Y
            Dim size = scaler.region.Size

            Select Case layout
                Case XAxisLayoutStyles.Centra
                    Y += size.Height / 2 + offset.Y
                Case XAxisLayoutStyles.Top
                    Y += 0
                Case XAxisLayoutStyles.ZERO
                    Y = Y0 + offset.Y
                Case Else
                    Y += size.Height
            End Select

            Dim ZERO As New Point(scaler.region.Left + offset.X, Y)                 ' 坐标轴原点
            Dim right As New Point(ZERO.X + size.Width, Y)   ' X轴
            Dim d! = If(overridesTickLine <= 0, 10, overridesTickLine)

            Call g.DrawLine(pen, ZERO, right)   ' X轴

            If Not noTicks AndAlso Not scaler.AxisTicks.X.IsNullOrEmpty Then
                ' 绘制坐标轴标签
                Dim ticks As (x#, label$)()

                If TypeOf scaler.X Is LinearScale Then
                    ticks = scaler.AxisTicks.X _
                        .Select(Function(tick)
                                    Return (scaler.X(tick), (tick).ToString(tickFormat))
                                End Function) _
                        .ToArray
                Else
                    ticks = DirectCast(scaler.X, OrdinalScale) _
                        .getTerms _
                        .Select(Function(tick) (scaler.X(tick.value), tick.value)) _
                        .ToArray
                End If

                For Each tick As (X#, label$) In ticks
                    Dim x As Single = tick.X + offset.X
                    Dim axisX As New PointF(x, ZERO.Y)
                    Dim labelText = tick.label
                    Dim sz As SizeF = g.MeasureString(labelText, tickFont)

                    Call g.DrawLine(pen, axisX, New PointF(x, ZERO.Y + d!))
                    Call g.DrawString(labelText, tickFont, Brushes.Black, New Point(x - sz.Width / 2, ZERO.Y + d * 1.2))
                Next
            End If

            If Not label.StripHTMLTags(stripBlank:=True).StringEmpty Then
                If htmlLabel Then
                    Dim labelImage As Image = label.__plotLabel(labelFont, False)
                    Dim point As New Point With {
                        .X = (size.Width - labelImage.Width) / 2 + scaler.region.Left,
                        .Y = scaler.region.Top + size.Height + tickFont.Height + d * 4
                    }

                    Call g.DrawImageUnscaled(labelImage, point)
                Else
                    Dim font As Font = CSSFont.TryParse(labelFont).GDIObject
                    Dim fSize As SizeF = g.MeasureString(label, font)
                    Dim y1 As Double = scaler.region.Bottom + tickFont.Height + d * 5
                    Dim y2 As Double = scaler.region.Bottom + ((g.Size.Height - scaler.region.Bottom) - fSize.Height) / 2
                    Dim point As New PointF With {
                        .X = (size.Width - fSize.Width) / 2 + scaler.region.Left,
                        .Y = stdNum.Max(y1, y2)
                    }

                    ' Call $"[X:={label}] {point.ToString}".__INFO_ECHO
                    Call g.DrawString(label, font, Brushes.Black, point)
                End If
            End If
        End Sub
    End Module
End Namespace
