#Region "Microsoft.VisualBasic::9d1652f7d501a9cca659ffa0a610e1f7, Data_science\Visualization\Plots-statistics\RegressionPlot.vb"

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

    '   Total Lines: 407
    '    Code Lines: 304
    ' Comment Lines: 47
    '   Blank Lines: 56
    '     File Size: 17.80 KB


    ' Module RegressionPlot
    ' 
    '     Function: Plot
    ' 
    '     Sub: printEquation, printLegend, printXPredicted
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Public Module RegressionPlot

    ''' <summary>
    ''' Plot of the linear regression result
    ''' </summary>
    ''' <param name="fit"></param>
    ''' <param name="size$"></param>
    ''' <param name="bg$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="xLabel$"></param>
    ''' <param name="yLabel$"></param>
    ''' <param name="pointSize!"></param>
    ''' <param name="title$"></param>
    ''' <param name="titleFontCss$"></param>
    ''' <param name="pointBrushStyle$"></param>
    ''' <param name="errorFitPointStyle$"></param>
    ''' <param name="predictPointStyle$"></param>
    ''' <param name="regressionLineStyle$"></param>
    ''' <param name="predictPointStroke$"></param>
    ''' <param name="labelAnchorLineStroke$"></param>
    ''' <param name="predictedX"></param>
    ''' <param name="showLegend"></param>
    ''' <param name="legendLabelFontCSS$"></param>
    ''' <param name="pointLabelFontCSS$"></param>
    ''' <param name="xAxisTickFormat$"></param>
    ''' <param name="yAxisTickFormat$"></param>
    ''' <param name="showErrorBand"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(fit As IFitted,
                         Optional size$ = "2100,1600",
                         Optional bg$ = "white",
                         Optional margin$ = g.DefaultPadding,
                         Optional xLabel$ = "X",
                         Optional yLabel$ = "Y",
                         Optional pointSize! = 5,
                         Optional title$ = Nothing,
                         Optional titleFontCss$ = CSSFont.PlotTitle,
                         Optional pointBrushStyle$ = "red",
                         Optional errorFitPointStyle$ = "blue",
                         Optional predictPointStyle$ = "green",
                         Optional regressionLineStyle$ = "stroke: black; stroke-width: 2px; stroke-dash: solid;",
                         Optional predictPointStroke$ = "stroke: black; stroke-width: 2px; stroke-dash: dash;",
                         Optional labelAnchorLineStroke$ = Stroke.StrongHighlightStroke,
                         Optional predictedX As IEnumerable(Of NamedValue(Of Double)) = Nothing,
                         Optional showLegend As Boolean = True,
                         Optional linearDetailsFontCSS$ = CSSFont.Win10NormalLarge,
                         Optional legendLabelFontCSS$ = CSSFont.Win10NormalLarger,
                         Optional pointLabelFontCSS$ = CSSFont.Win7LittleLarge,
                         Optional xAxisTickFormat$ = "F2",
                         Optional yAxisTickFormat$ = "F2",
                         Optional factorFormat$ = "G4",
                         Optional showErrorBand As Boolean = True,
                         Optional labelerIterations% = 1000,
                         Optional gridFill$ = NameOf(Color.LightGray),
                         Optional showYFitPoints As Boolean = True,
                         Optional reverse As Boolean = False,
                         Optional ppi As Integer = 100) As GraphicsData

        Dim xTicks#() = fit.X.AsEnumerable _
            .Range(scale:=1.125) _
            .CreateAxisTicks(decimalDigits:=xAxisTickFormat.Match("\d+"))
        Dim yTicks#() = (fit.Y.AsList + fit.Yfit.AsEnumerable) _
            .Range(scale:=1.125) _
            .CreateAxisTicks(decimalDigits:=yAxisTickFormat.Match("\d+"))

        If reverse Then
            Dim temp As Double() = xTicks

            xTicks = yTicks
            yTicks = temp
        End If

        If TypeOf fit Is MLRFit Then
            Throw New InvalidProgramException($"MLR model is not compatible with this plot function!")
        End If

        Dim pointBrush As Brush = pointBrushStyle.GetBrush
        Dim regressionPen As Pen = Stroke.TryParse(regressionLineStyle).GDIObject
        Dim predictedPointBorder As Pen = Stroke.TryParse(predictPointStroke).GDIObject
        Dim predictedBrush As Brush = predictPointStyle.GetBrush
        Dim errorFitPointBrush As Brush = errorFitPointStyle.GetBrush
        Dim pointLabelFont As Font = CSSFont.TryParse(pointLabelFontCSS).GDIObject(ppi)
        Dim labelAnchorPen As Pen = Stroke.TryParse(labelAnchorLineStroke).GDIObject
        Dim polynomial = DirectCast(fit.Polynomial, Polynomial)
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim rect = region.PlotRegion

                If xTicks.IsNullOrEmpty OrElse yTicks.IsNullOrEmpty OrElse fit.ErrorTest.Length = 0 Then
                    Call g.DrawString("Invalid curve!", CSSFont.TryParse(title).GDIObject(g.Dpi), Brushes.Black, New PointF)
                    Return
                End If

                Dim X = d3js.scale.linear.domain(values:=xTicks).range(integers:={rect.Left, rect.Right})
                Dim Y = d3js.scale.linear.domain(values:=yTicks).range(integers:={rect.Top, rect.Bottom})
                Dim scaler As New DataScaler With {
                    .X = X,
                    .Y = Y,
                    .region = rect,
                    .AxisTicks = (xTicks, yTicks)
                }

                Call g.DrawAxis(
                    region, scaler, True,
                    xlabel:=xLabel, ylabel:=yLabel,
                    htmlLabel:=False,
                    XtickFormat:=xAxisTickFormat,
                    YtickFormat:=yAxisTickFormat,
                    gridFill:=gridFill
                )

                ' scatter plot
                ' 绘制红色的实际数值点
                For Each point As TestPoint In fit.ErrorTest
                    Dim pt As PointF = scaler.Translate(point)

                    g.DrawCircle(
                        centra:=pt,
                        r:=pointSize,
                        color:=pointBrush
                    )
                Next

                If Not DirectCast(fit.Polynomial, Polynomial).IsLinear Then
                    For Each t In xTicks.SlideWindows(2)
                        Dim A As New PointF With {.X = t(0), .Y = polynomial(.X)}
                        Dim B As New PointF With {.X = t(1), .Y = polynomial(.X)}

                        A = scaler.Translate(A)
                        B = scaler.Translate(B)

                        Call g.DrawLine(regressionPen, A, B)
                    Next
                Else
                    'For Each t As SlideWindow(Of TestPoint) In fit.ErrorTest _
                    '    .Select(Function(d) DirectCast(d, TestPoint)) _
                    '    .SlideWindows(2)

                    '    ' regression line 
                    '    Dim A As New PointF With {.X = t.First.X, .Y = t.First.Yfit}
                    '    Dim B As New PointF With {.X = t.Last.X, .Y = t.Last.Yfit}

                    '    A = scaler.Translate(A)
                    '    B = scaler.Translate(B)

                    '    Call g.DrawLine(regressionPen, A, B)
                    'Next

                    ' regression line 
                    Dim xMin As Double = fit.X.Min
                    Dim xMax As Double = fit.X.Max

                    'If reverse Then
                    '    xMin = fit.Y.Min
                    '    xMax = fit.Y.Max
                    'Else
                    '    xMin = fit.X.Min
                    '    xMax = fit.X.Max
                    'End If

                    Dim yMin As Double = polynomial(xMin)
                    Dim yMax As Double = polynomial(xMax)
                    Dim A As New PointF With {.X = xMin, .Y = yMin}
                    Dim B As New PointF With {.X = xMax, .Y = yMax}

                    A = scaler.Translate(A)
                    B = scaler.Translate(B)

                    Call g.DrawLine(regressionPen, A, B)
                End If

                If showYFitPoints Then
                    ' 绘制蓝色的fit计算点
                    For Each point As TestPoint In fit.ErrorTest
                        Dim pt As New PointF With {
                            .X = point.X,
                            .Y = point.Yfit
                        }

                        pt = scaler.Translate(pt)

                        g.DrawCircle(
                            centra:=pt,
                            r:=pointSize,
                            color:=errorFitPointBrush
                        )
                        g.DrawCircle(
                            centra:=pt,
                            r:=pointSize,
                            color:=predictedPointBorder,
                            fill:=False
                        )
                    Next
                End If

                If showErrorBand Then
                    Dim dx = xTicks(1) - xTicks(0)
                    Dim plusError As New List(Of PointF)
                    Dim negError As New List(Of PointF)
                    Dim line As Line

                    For Each point As TestPoint In fit.ErrorTest
                        Dim A As New PointF(point.X, point.Yfit)
                        Dim B As New PointF With {
                            .X = point.X + dx,
                            .Y = polynomial(.X)
                        }

                        line = New Line(A, B).ParallelShift(stdNum.Abs(point.Err))
                        plusError.Add(scaler.Translate(line.A))

                        line = New Line(A, B).ParallelShift(-stdNum.Abs(point.Err))
                        negError.Add(scaler.Translate(line.A))
                    Next

                    negError.Reverse()

                    Dim path As New GraphicsPath

                    For Each t In plusError.SlideWindows(2)
                        path.AddLine(t(0), t(1))
                    Next
                    For Each t In negError.SlideWindows(2)
                        path.AddLine(t(0), t(1))
                    Next

                    path.CloseAllFigures()
                    g.FillPath(New SolidBrush(Color.FromArgb(230, Color.Cyan)), path)
                End If

                If Not predictedX Is Nothing Then
                    Call g.printXPredicted(
                        fit, scaler,
                        predictedX, predictedBrush, predictedPointBorder,
                        pointSize,
                        pointLabelFont,
                        rect,
                        labelAnchorPen,
                        labelerIterations:=labelerIterations
                    )
                End If

                If showLegend Then
                    Call g.printLegend(fit, rect, linearDetailsFontCSS, legendLabelFontCSS, factorFormat, Not predictedX Is Nothing)
                End If

                Call g.printEquation(fit, rect, linearDetailsFontCSS, legendLabelFontCSS, factorFormat, Not predictedX Is Nothing)

                If Not title.StringEmpty Then
                    Dim titleFont As Font = CSSFont.TryParse(titleFontCss).GDIObject(g.Dpi)
                    Dim titleSize = g.MeasureString(title, titleFont)
                    Dim top = (rect.Top - titleSize.Height) / 2
                    Dim left = rect.Left + (rect.Width - titleSize.Width) / 2

                    Call g.DrawString(title, titleFont, Brushes.Black, New PointF(left, top))
                End If
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotInternal,
            dpi:=$"{ppi},{ppi}"
        )
    End Function

    <Extension>
    Private Sub printXPredicted(g As IGraphics, fit As IFitted, scaler As DataScaler,
                                predictedX As IEnumerable(Of NamedValue(Of Double)),
                                predictedBrush As Brush,
                                predictedPointBorder As Pen,
                                pointSize!,
                                pointLabelFont As Font,
                                rect As RectangleF,
                                labelAnchorPen As Pen,
                                labelerIterations%)

        Dim labels As New List(Of Label)
        Dim anchors As New List(Of PointF)
        Dim labelSize As SizeF

        For Each ptX As NamedValue(Of Double) In predictedX
            Dim pt As New PointF With {
                .X = ptX.Value,
                .Y = fit.GetY(.X)
            }

            If Not (pt.X.IsNaNImaginary OrElse pt.Y.IsNaNImaginary) Then
                pt = scaler.Translate(pt)
                labelSize = g.MeasureString(ptX.Name, pointLabelFont)

                g.DrawCircle(
                    centra:=pt,
                    r:=pointSize,
                    color:=predictedBrush
                )
                g.DrawCircle(
                    centra:=pt,
                    r:=pointSize,
                    color:=predictedPointBorder,
                    fill:=False
                )

                anchors += pt
                labels += New Label With {
                    .height = labelSize.Height,
                    .width = labelSize.Width,
                    .text = ptX.Name,
                    .X = pt.X,
                    .Y = pt.Y
                }
            End If
        Next

        Call d3js.labeler(maxMove:=50, maxAngle:=0.6, w_len:=0.5, w_inter:=5, w_lab2:=30, w_lab_anc:=30, w_orient:=5) _
            .Labels(labels) _
            .Anchors(labels.GetLabelAnchors(pointSize)) _
            .Width(rect.Width) _
            .Height(rect.Height) _
            .Start(showProgress:=False, nsweeps:=labelerIterations)

        For Each label As SeqValue(Of Label) In labels.SeqIterator
            With label.value
                Call g.DrawLine(labelAnchorPen, .GetTextAnchor(anchors(label)).PointF, anchors(label))
                Call g.DrawString(.text, pointLabelFont, Brushes.Black, .ByRef)
            End With
        Next
    End Sub

    <Extension>
    Private Sub printEquation(g As IGraphics, fit As IFitted, rect As RectangleF, linearDetailsFontCSS$, legendLabelFontCSS$, factorFormat$, hasPredictedSamples As Boolean)
        Dim legendLabelFont As Font = CSSFont.TryParse(linearDetailsFontCSS).GDIObject(g.Dpi)
        Dim eq$ = "f<sub>(x)</sub> = " & fit.Polynomial.ToString(factorFormat, html:=True)
        Dim R2$ = "R<sup>2</sup> = " & fit.CorrelationCoefficient.ToString("F5")
        Dim pt As New PointF With {
            .X = rect.Left + g.MeasureString("00", legendLabelFont).Width,
            .Y = rect.Top + 20
        }

        Call g.DrawHtmlString(eq, legendLabelFont, Color.Black, pt)

        pt = New PointF With {
            .X = pt.X,
            .Y = pt.Y + legendLabelFont.Height + 5
        }

        Call g.DrawHtmlString(R2, legendLabelFont, Color.Black, pt)
    End Sub

    <Extension>
    Private Sub printLegend(g As IGraphics, fit As IFitted, rect As RectangleF, linearDetailsFontCSS$, legendLabelFontCSS$, factorFormat$, hasPredictedSamples As Boolean)
        Dim legends As LegendObject() = {
            New LegendObject With {.color = "blue", .fontstyle = legendLabelFontCSS, .style = LegendStyles.Circle, .title = "Predicts"},
            New LegendObject With {.color = "red", .fontstyle = legendLabelFontCSS, .style = LegendStyles.Circle, .title = "Standard Reference"},
            New LegendObject With {.color = "black", .fontstyle = legendLabelFontCSS, .style = LegendStyles.SolidLine, .title = "Linear"}
        }
        Dim legendLabelFont As Font = CSSFont.TryParse(linearDetailsFontCSS).GDIObject(g.Dpi)

        If hasPredictedSamples Then
            legends.Add(New LegendObject With {.color = "green", .fontstyle = legendLabelFontCSS, .style = LegendStyles.Circle, .title = "Samples"})
        End If

        Dim border As Stroke = Stroke.ScatterLineStroke
        Dim maxLabelSize! = legends _
            .Select(Function(l) l.title) _
            .MaxLengthString _
            .DoCall(Function(str)
                        Return g.MeasureString(str, legendLabelFont)
                    End Function) _
            .Width
        Dim top = rect.Top + rect.Height / 1.5
        Dim left = rect.Right - 1.25 * maxLabelSize

        Call g.DrawLegends(
            topLeft:=New Point(left, top),
            legends:=legends,
            gSize:="64,18",
            fillBg:="white",
            regionBorder:=border
        )
    End Sub
End Module
