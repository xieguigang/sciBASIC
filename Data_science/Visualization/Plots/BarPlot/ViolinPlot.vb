#Region "Microsoft.VisualBasic::e3d2b21626f18e184d7a276124c9e61a, Data_science\Visualization\Plots\BarPlot\ViolinPlot.vb"

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

'   Total Lines: 124
'    Code Lines: 92 (74.19%)
' Comment Lines: 25 (20.16%)
'    - Xml Docs: 88.00%
' 
'   Blank Lines: 7 (5.65%)
'     File Size: 5.75 KB


' Module ViolinPlot
' 
'     Function: (+2 Overloads) Plot
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' ## 小提琴图
''' 
''' + 高度为数据的分布位置
''' + 宽度为对应的百分位上的数据点的数量
''' + 长度为最小值与最大值之间的差值
''' </summary>
Public Module ViolinPlot

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dataset">数据集中的样本数据可以不必等长</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <param name="colorset"></param>
    ''' <returns></returns>
    Public Function Plot(Of T As {INamedValue, DynamicPropertyBase(Of Double)})(dataset As IEnumerable(Of T),
                         Optional size$ = Canvas.Resolution2K.Size,
                         Optional margin$ = Canvas.Resolution2K.PaddingWithTopTitle,
                         Optional bg$ = "white",
                         Optional colorset$ = DesignerTerms.TSFShellColors,
                         Optional Ylabel$ = "y axis",
                         Optional yLabelFontCSS$ = Canvas.Resolution2K.PlotSmallTitle,
                         Optional ytickFontCSS$ = Canvas.Resolution2K.PlotLabelNormal,
                         Optional removesOutliers As Boolean = True,
                         Optional yTickFormat$ = "F2",
                         Optional stroke$ = Stroke.AxisStroke,
                         Optional title$ = "Volin Plot",
                         Optional titleFontCSS$ = Canvas.Resolution2K.PlotTitle,
                         Optional labelAngle As Double = -45,
                         Optional showStats As Boolean = True) As GraphicsData

        With dataset.ToArray
            Return .Select(Function(a) a.Properties.Keys).IteratesALL.Distinct _
                   .Select(Function(label)
                               Return New NamedCollection(Of Double)(label, .Select(Function(a) a(label)))
                           End Function) _
                   .DoCall(Function(data)
                               Return ViolinPlot.Plot(
                                   dataset:=data,
                                   size:=size,
                                   margin:=margin,
                                   bg:=bg,
                                   colorset:=colorset,
                                   Ylabel:=Ylabel,
                                   yLabelFontCSS:=yLabelFontCSS,
                                   ytickFontCSS:=ytickFontCSS,
                                   removesOutliers:=removesOutliers,
                                   yTickFormat:=yTickFormat,
                                   strokeCSS:=stroke,
                                   title:=title,
                                   titleFontCSS:=titleFontCSS,
                                   labelAngle:=labelAngle,
                                   showStats:=showStats
                               )
                           End Function)
        End With
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dataset">数据集中的样本数据可以不必等长</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <param name="colorset"></param>
    ''' <returns></returns>
    Public Function Plot(dataset As IEnumerable(Of NamedCollection(Of Double)),
                         Optional size$ = Canvas.Resolution2K.Size,
                         Optional margin$ = Canvas.Resolution2K.PaddingWithTopTitle,
                         Optional bg$ = "white",
                         Optional colorset$ = "#94cac1",
                         Optional Ylabel$ = "y axis",
                         Optional yLabelFontCSS$ = Canvas.Resolution2K.PlotSmallTitle,
                         Optional ytickFontCSS$ = Canvas.Resolution2K.PlotLabelNormal,
                         Optional splineDegree% = 2,
                         Optional removesOutliers As Boolean = True,
                         Optional yTickFormat$ = "F2",
                         Optional strokeCSS$ = "stroke: #6e797a; stroke-width: 15px; stroke-dash: solid;",
                         Optional title$ = "Volin Plot",
                         Optional titleFontCSS$ = Canvas.Resolution2K.PlotTitle,
                         Optional labelAngle As Double = -45,
                         Optional showStats As Boolean = True,
                         Optional ppi As Integer = 100,
                         Optional driver As Drivers = Drivers.GDI) As GraphicsData

        If removesOutliers Then
            dataset = Violin.removesOutliers(dataset).ToArray
        End If

        Dim theme As New Theme With {
            .padding = margin,
            .background = bg,
            .colorSet = colorset,
            .axisTickCSS = ytickFontCSS,
            .axisLabelCSS = yLabelFontCSS,
            .YaxisTickFormat = yTickFormat,
            .lineStroke = strokeCSS,
            .mainCSS = titleFontCSS,
            .xAxisRotate = labelAngle
        }
        Dim app As New Violin(dataset, theme) With {
            .main = title,
            .ylabel = Ylabel,
            .showStats = showStats,
            .splineDegree = splineDegree
        }

        Return app.Plot(size.SizeParser, ppi, driver)
    End Function
End Module
