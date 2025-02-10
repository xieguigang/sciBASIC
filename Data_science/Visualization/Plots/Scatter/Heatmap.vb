#Region "Microsoft.VisualBasic::5a97f8cb970f47a2678919f0ba2707cc, Data_science\Visualization\Plots\Scatter\Heatmap.vb"

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

    '   Total Lines: 119
    '    Code Lines: 92 (77.31%)
    ' Comment Lines: 18 (15.13%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 9 (7.56%)
    '     File Size: 5.25 KB


    ' Module Scatter
    ' 
    '     Function: PlotHeatmap
    ' 
    '     Sub: __plotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS

Partial Module Scatter

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="fieldX$"></param>
    ''' <param name="fieldY$"></param>
    ''' <param name="valueField$"></param>
    ''' <param name="colorSchema$"></param>
    ''' <param name="levels%"></param>
    ''' <param name="size"></param>
    ''' <param name="padding$"></param>
    ''' <param name="xlabel$">
    ''' 如果这个参数为默认的空值，则函数会使用<paramref name="fieldX"/>的值作为标签值，
    ''' 但是如果为空字符串，则这个坐标轴的标签将不会被显示出来
    ''' </param>
    ''' <param name="ylabel$"></param>
    ''' <param name="legendTitle$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PlotHeatmap(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(data As IEnumerable(Of DataSet),
                                Optional fieldX$ = "X",
                                Optional fieldY$ = "Y",
                                Optional valueField$ = "value",
                                Optional colorSchema$ = "jet",
                                Optional levels% = 25,
                                Optional size As Size = Nothing,
                                Optional padding$ = g.DefaultPadding,
                                Optional bg$ = "white",
                                Optional xlabel$ = Nothing,
                                Optional ylabel$ = Nothing,
                                Optional legendTitle$ = Nothing,
                                Optional ptSize% = 8) As GraphicsData

        Dim colors As Color() = Designer.GetColors(colorSchema, n:=levels)

        If xlabel Is Nothing Then
            xlabel = fieldX
        End If
        If ylabel Is Nothing Then
            ylabel = fieldY
        End If
        If legendTitle.StringEmpty Then
            legendTitle = valueField
        End If
        If size.IsEmpty Then
            size = New Size(1600, 1200)
        End If

        Return g.GraphicsPlots(
            size, padding,
            bg,
            Sub(ByRef g, rect)
                Call g.__plotInternal(rect, data.ToArray, colors,
                                      fieldX, fieldY, valueField,
                                      labelX:=xlabel, labelY:=ylabel,
                                      ptSize:=ptSize,
                                      legendTitle:=legendTitle)
            End Sub)
    End Function

    <Extension>
    Private Sub __plotInternal(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(g As IGraphics, rect As GraphicsRegion, data As DataSet(), colors As Color(),
                               fieldX$, fieldY$, fieldValue$,
                               labelX$, labelY$, legendTitle$,
                               ptSize%)

        Dim css As New CSSEnvirnment(rect.Size)
        Dim padding As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, rect.Padding)
        Dim points As (pt As PointF, value#)() = data.Select(
            Function(o) (New PointF(o(fieldX), o(fieldY)), o(fieldValue))).ToArray
        Dim levels%() = points.Select(Function(pt) pt.value) _
            .GenerateMapping(Level:=colors.Length)
        Dim valueGroups = points _
            .SeqIterator _
            .Select(Function(p) (p.value.pt, p.value.value, seq:=levels(p))) _
            .GroupBy(Function(o) o.seq)
        Dim colorHelper = colors.MapHelper
        Dim serials As SerialData() = valueGroups _
            .Select(Function(o) New SerialData() With {
                .color = colorHelper(o.Key),
                .pts = o.Select(Function(x) New PointData(x.Item1)).ToArray,
                .title = o.Key,
                .pointSize = ptSize
            }) _
            .ToArray
        Dim leftWidth% = rect.Size.Width * 0.9
        Dim scatterPlotSize$ = $"{leftWidth},{rect.Size.Height}"
        Dim left As GraphicsData = Scatter.Plot(
            serials, scatterPlotSize,
            Xlabel:=labelX, Ylabel:=labelY, drawLine:=False, showLegend:=False)
        Dim legend As GraphicsData = Legends.ColorMapLegend(
            designer:=colors,
            title:=legendTitle,
            min:=points.Min(Function(pt) pt.value),
            max:=points.Max(Function(pt) pt.value),
            lsize:=New Size(rect.Size.Width - leftWidth + padding.Right, rect.Size.Height * 0.7))

        leftWidth -= (padding.Right)

        With g
            .DrawImageUnscaled(left, New Point)
            .DrawImage(legend, leftWidth, CInt((rect.Size.Height - legend.Height) / 2))
        End With
    End Sub
End Module
