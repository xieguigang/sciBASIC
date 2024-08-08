#Region "Microsoft.VisualBasic::f4cc23631c443dca3c8f2e348395286c, Data_science\Visualization\Plots-statistics\RegressionPlot.vb"

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

    '   Total Lines: 411
    '    Code Lines: 308 (74.94%)
    ' Comment Lines: 47 (11.44%)
    '    - Xml Docs: 55.32%
    ' 
    '   Blank Lines: 56 (13.63%)
    '     File Size: 18.03 KB


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
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
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
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Public Class RegressionPlot : Inherits Plot

    ReadOnly fit As IFitted

    Public Property reverse As Boolean
    Public Property pointBrushStyle As String = "blue"
    Public Property predictPointStyle As String = "green"
    Public Property errorFitPointStyle As String = "red"
    Public Property showYFitPoints As Boolean = True
    Public Property showErrorBand As Boolean = False
    Public Property labelerIterations As Integer = -1
    Public Property predictedX As NamedValue(Of Double)()
    Public Property predictPointStroke As String
    Public Property absolute_positive As Boolean = False

    Sub New(fit As IFitted, theme As Theme)
        Call MyBase.New(theme)

        Me.fit = fit
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim raw_x = fit.X
        Dim raw_y = (fit.Y.AsList + fit.Yfit.AsEnumerable)

        Dim xTicks#() = raw_x.AsEnumerable _
          .Range(scale:=1.125) _
          .CreateAxisTicks(decimalDigits:=theme.XaxisTickFormat.Match("\d+"))
        Dim yTicks#() = raw_y _
            .Range(scale:=1.125) _
            .CreateAxisTicks(decimalDigits:=theme.YaxisTickFormat.Match("\d+"))

        If absolute_positive OrElse raw_x.All(Function(xi) xi >= 0) Then
            xTicks = xTicks.Where(Function(xi) xi >= 0).CreateAxisTicks(decimalDigits:=theme.XaxisTickFormat.Match("\d+"))
        End If
        If absolute_positive OrElse raw_y.All(Function(yi) yi >= 0) Then
            yTicks = yTicks.Where(Function(yi) yi >= 0).CreateAxisTicks(decimalDigits:=theme.YaxisTickFormat.Match("\d+"))
        End If

        If reverse Then
            Dim temp As Double() = xTicks

            xTicks = yTicks
            yTicks = temp
        End If

        If TypeOf fit Is MLRFit Then
            Throw New InvalidProgramException($"MLR model is not compatible with this plot function!")
        End If

        Dim pointBrush As Brush = pointBrushStyle.GetBrush
        Dim predictedBrush As Brush = predictPointStyle.GetBrush
        Dim errorFitPointBrush As Brush = errorFitPointStyle.GetBrush
        Dim polynomial = DirectCast(fit.Polynomial, Polynomial)

        Dim rect = canvas.PlotRegion
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim pointLabelFont As Font = css.GetFont(CSSFont.TryParse(theme.tagCSS))
        Dim regressionPen As Pen = css.GetPen(Stroke.TryParse(theme.lineStroke))
        Dim predictedPointBorder As Pen = css.GetPen(Stroke.TryParse(predictPointStroke))
        Dim labelAnchorPen As Pen = css.GetPen(Stroke.TryParse(theme.tagLinkStroke))

        If xTicks.IsNullOrEmpty OrElse yTicks.IsNullOrEmpty OrElse fit.ErrorTest.Length = 0 Then
            Call g.DrawString("Invalid curve!", css.GetFont(CSSFont.TryParse(main)), Brushes.Black, New PointF)
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
            canvas, scaler, True,
            xlabel:=xlabel, ylabel:=ylabel,
            htmlLabel:=False,
            XtickFormat:=theme.XaxisTickFormat,
            YtickFormat:=theme.YaxisTickFormat,
            gridFill:=theme.gridFill
        )

        ' scatter plot
        ' 绘制红色的实际数值点
        For Each point As TestPoint In fit.ErrorTest
            Dim pt As PointF = scaler.Translate(point)

            g.DrawCircle(
                centra:=pt,
                r:=theme.pointSize,
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
                    r:=theme.pointSize,
                    color:=errorFitPointBrush
                )
                g.DrawCircle(
                    centra:=pt,
                    r:=theme.pointSize,
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

                line = New Line(A, B).ParallelShift(std.Abs(point.Err))
                plusError.Add(scaler.Translate(line.A))

                line = New Line(A, B).ParallelShift(-std.Abs(point.Err))
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
            Call printXPredicted(
                g, scaler,
                predictedX, predictedBrush, predictedPointBorder,
                theme.pointSize,
                pointLabelFont,
                rect,
                labelAnchorPen,
                labelerIterations:=labelerIterations
            )
        End If

        If theme.drawLegend Then
            Call printLegend(g, rect, theme.legendTickCSS, theme.legendLabelCSS, theme.legendTickFormat)
        End If

        Call printEquation(g, rect, theme.legendTickCSS, theme.legendLabelCSS, theme.legendTickFormat)

        If Not main.StringEmpty Then
            Call DrawMainTitle(g, canvas.PlotRegion)
        End If
    End Sub

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
    Public Overloads Shared Function Plot(fit As IFitted,
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
                                          Optional absolute_positive As Boolean = False,
                                                 Optional PredictsLabel As String = "Predicts",
       Optional ReferenceLabel As String = "Standard Reference",
      Optional LinearLabel As String = "Linear",
        Optional SamplesLabel As String = "Samples",
                         Optional ppi As Integer = 100,
                                          Optional driver As Drivers = Drivers.Default) As GraphicsData

        Dim theme As New Theme With {
            .padding = margin,
            .background = bg,
            .pointSize = pointSize,
            .drawLegend = showLegend,
            .XaxisTickFormat = xAxisTickFormat,
            .YaxisTickFormat = yAxisTickFormat,
            .legendTickCSS = linearDetailsFontCSS,
            .legendLabelCSS = legendLabelFontCSS,
            .legendTickFormat = factorFormat,
            .tagCSS = pointLabelFontCSS,
            .lineStroke = regressionLineStyle,
            .tagLinkStroke = labelAnchorLineStroke
        }
        Dim app As New RegressionPlot(fit, theme) With {
            .reverse = reverse,
            .xlabel = xLabel,
            .ylabel = yLabel,
            .main = title,
            .errorFitPointStyle = errorFitPointStyle,
            .labelerIterations = labelerIterations,
            .pointBrushStyle = pointBrushStyle,
            .predictPointStyle = predictPointStyle,
            .showErrorBand = showErrorBand,
            .showYFitPoints = showYFitPoints,
            .predictedX = If(predictedX Is Nothing, Nothing, predictedX.ToArray),
            .predictPointStroke = predictPointStroke,
            .SamplesLabel = SamplesLabel,
            .LinearLabel = LinearLabel,
            .PredictsLabel = PredictsLabel,
            .ReferenceLabel = ReferenceLabel,
            .absolute_positive = absolute_positive
        }

        Return app.Plot(size, ppi, driver)
    End Function

    Private Sub printXPredicted(g As IGraphics, scaler As DataScaler,
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

    Private Sub printEquation(g As IGraphics, rect As RectangleF, linearDetailsFontCSS$, legendLabelFontCSS$, factorFormat$)
        Dim hasPredictedSamples = Not predictedX Is Nothing
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim legendLabelFont As Font = css.GetFont(CSSFont.TryParse(linearDetailsFontCSS))
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

    Public Property PredictsLabel As String = "Predicts"
    Public Property ReferenceLabel As String = "Standard Reference"
    Public Property LinearLabel As String = "Linear"
    Public Property SamplesLabel As String = "Samples"

    Private Sub printLegend(g As IGraphics, rect As RectangleF, linearDetailsFontCSS$, legendLabelFontCSS$, factorFormat$)
        Dim hasPredictedSamples = Not predictedX Is Nothing
        Dim legends As LegendObject() = {
            New LegendObject With {.color = "blue", .fontstyle = legendLabelFontCSS, .style = LegendStyles.Circle, .title = PredictsLabel},
            New LegendObject With {.color = "red", .fontstyle = legendLabelFontCSS, .style = LegendStyles.Circle, .title = ReferenceLabel},
            New LegendObject With {.color = "black", .fontstyle = legendLabelFontCSS, .style = LegendStyles.SolidLine, .title = LinearLabel}
        }
        Dim css As CSSEnvirnment = g.LoadEnvironment
        Dim legendLabelFont As Font = css.GetFont(CSSFont.TryParse(linearDetailsFontCSS))

        If hasPredictedSamples Then
            legends.Add(New LegendObject With {.color = "green", .fontstyle = legendLabelFontCSS, .style = LegendStyles.Circle, .title = SamplesLabel})
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
        Dim left = rect.Right - 1.5 * maxLabelSize

        Call g.DrawLegends(
            topLeft:=New Point(left, top),
            legends:=legends,
            gSize:="64,18",
            fillBg:="white",
            regionBorder:=border
        )
    End Sub
End Class
