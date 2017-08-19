#Region "Microsoft.VisualBasic::53cae1cfbdad7b74daf62519184de1a4, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots-statistics\BoxPlot.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' ```
''' min, q1, q2, q3, max
'''       _________
'''  +----|   |   |----+
'''       ---------
''' ```
''' </summary>
Public Module BoxPlot

    <Extension> Public Function Plot(data As BoxData,
                                     Optional size$ = "3000,2700",
                                     Optional padding$ = g.DefaultPadding,
                                     Optional bg$ = "white",
                                     Optional schema$ = ColorBrewer.QualitativeSchemes.Set1_9,
                                     Optional groupLabelCSSFont$ = CSSFont.Win7LittleLarge,
                                     Optional YAxisLabelFontCSS$ = CSSFont.Win7LittleLarge,
                                     Optional tickFontCSS$ = CSSFont.Win7Normal,
                                     Optional regionStroke$ = Stroke.AxisStroke,
                                     Optional interval# = 100) As GraphicsData

        Dim yAxisLabelFont As Font = CSSFont.TryParse(YAxisLabelFontCSS)
        Dim groupLabelFont As Font = CSSFont.TryParse(groupLabelCSSFont)
        Dim tickLabelFont As Font = CSSFont.TryParse(tickFontCSS)
        Dim ranges As DoubleRange = data _
            .Groups _
            .Select(Function(x) x.Value) _
            .IteratesALL _
            .ToArray
        Dim colors As LoopArray(Of SolidBrush) = Designer _
            .GetColors(schema) _
            .Select(Function(color) New SolidBrush(color)) _
            .ToArray

        Dim plotInternal =
            Sub(ByRef g As IGraphics, rect As GraphicsRegion)

                Dim plotRegion = rect.PlotRegion

                With plotRegion
                    Dim leftPart = yAxisLabelFont.Height + tickLabelFont.Height + 50
                    Dim bottomPart = groupLabelFont.Height + 50
                    Dim topLeft = .Location.OffSet2D(leftPart, 0)
                    Dim rectSize As New Size(.Width - leftPart, .Height - bottomPart)

                    plotRegion = New Rectangle(topLeft, rectSize)
                End With

                Dim boxWidth = StackedBarPlot.BarWidth(plotRegion.Width, data.Groups.Length, interval)
                Dim bottom = plotRegion.Bottom
                Dim height = plotRegion.Height
                Dim y = Function(x#) bottom - height * (x - ranges.Min) / ranges.Length

                If Not regionStroke.StringEmpty Then
                    Call g.DrawRectangle(
                        Stroke.TryParse(regionStroke).GDIObject,
                        plotRegion)
                End If

                ' 绘制盒子
                For Each group As NamedValue(Of Vector) In data.Groups
                    Dim quartile = group.Value.Quartile
                    Dim outlier = group.Value.Outlier(quartile)
                    Dim brush As SolidBrush = colors.Next

                    If Not outlier.Outlier.IsNullOrEmpty Then
                        quartile = outlier.Normal.Quartile
                    End If

                    ' max
                    ' min
                    ' outliers

                    For Each x In outlier.Outlier
                        Call g.FillEllipse()
                    Next
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser, padding,
            bg,
            plotInternal)
    End Function
End Module

