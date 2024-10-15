#Region "Microsoft.VisualBasic::c459e20ab629a226e871e0471d57a32c, Data_science\Visualization\Plots-statistics\HistStackedBarplot.vb"

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

    '   Total Lines: 158
    '    Code Lines: 123 (77.85%)
    ' Comment Lines: 16 (10.13%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 19 (12.03%)
    '     File Size: 6.90 KB


    ' Module HistStackedBarplot
    ' 
    '     Function: Plot, SampleDataSet
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module HistStackedBarplot

    ' plot layout
    ' tree  stacked-barplot  legend

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Private Function SampleDataSet(sample As BarDataSample, keys$()) As DataSet
        Return New DataSet With {
            .ID = sample.tag,
            .Properties = keys _
                .SeqIterator _
                .ToDictionary(Function(key) key.value,
                              Function(i) sample.data(i))
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="treeWidth#">假若这个宽度值在[0-1]之之间，则认为是百分比，反之当这个宽度值超过了1，则认为是实际的像素值</param>
    ''' <param name="sampleGroup"><see cref="DendrogramPanelV2.classinfo"/></param>
    ''' <returns></returns>
    Public Function Plot(data As BarDataGroup,
                         Optional size$ = "2700,2100",
                         Optional margin$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional treeWidth# = 0.1,
                         Optional sampleGroup As Dictionary(Of String, String) = Nothing,
                         Optional legendTitleFontCSS$ = CSSFont.Win7LargerBold,
                         Optional dtreeBar! = 25,
                         Optional dbarbox! = 25,
                         Optional dboxLabel! = 10,
                         Optional dbarInterval! = 30,
                         Optional dboxInterval! = 3) As GraphicsData

        Dim array As DataSet() = data _
            .Samples _
            .Select(Function(sample) sample.SampleDataSet(data.Serials.Keys)) _
            .ToArray
        Dim histCanvas = Function(cluster As Cluster)
                             Return New DendrogramPanelV2(cluster, New Theme)
                         End Function

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim css As CSSEnvirnment = g.LoadEnvironment
                Dim plotRegion As Rectangle = region.PlotRegion(css)
                Dim treeRegion As New Rectangle With {
                    .Location = plotRegion.Location,
                    .Width = MeasureWidthOrHeight(treeWidth, plotRegion.Width),
                    .Height = plotRegion.Height
                }

                ' 首先绘制出层次聚类树
                ' rowKeys得到的是sample的从上到下的绘图顺序
                Dim cluster As Cluster = Time(AddressOf array.RunCluster)
                Dim rowKeys$() = histCanvas(cluster) _
                    .Paint(g, treeRegion) _
                    .OrderBy(Function(x) x.Value.Y) _
                    .Keys

                Dim left! = treeRegion.Right + dtreeBar
                Dim top! = treeRegion.Top
                Dim legendTitleFont As Font = css.GetFont(CSSFont.TryParse(legendTitleFontCSS))
                Dim maxLabelSize As SizeF = data _
                    .Serials _
                    .Keys _
                    .MaxLengthString _
                    .MeasureSize(g, legendTitleFont)
                Dim boxSize As New SizeF(maxLabelSize.Height, maxLabelSize.Height)

                plotRegion = New Rectangle With {
                    .X = left,
                    .Y = top,
                    .Width = plotRegion.Width _
                             - dtreeBar - treeRegion.Width _
                             - dbarbox - boxSize.Width _
                             - dboxLabel - maxLabelSize.Width,  ' barplot绘制的宽度为总宽度减去层次聚类树宽度减去legend标签宽度减去legend颜色标签宽度
                    .Height = plotRegion.Height
                }

                Dim barplotHeight! = StackedBarPlot.BarWidth(
                    plotRegion.Height,
                    data.Samples.Length,
                    dbarInterval
                )

                ' 在这里的barplot是横向的位于层次聚类树的右侧
                For Each group As BarDataSample In data.Samples
                    ' 绘制一个样品分类中的数据
                    Dim sum# = group.StackedSum
                    Dim x! = left
                    Dim y! = top

                    For Each serial As SeqValue(Of NamedValue(Of Color)) In data.Serials.SeqIterator
                        Dim percent# = group.data(serial) / sum
                        Dim width! = plotRegion.Width * percent
                        Dim rect As New Rectangle With {
                            .X = x,
                            .Y = y,
                            .Width = width,
                            .Height = barplotHeight
                        }
                        Dim color As New SolidBrush((+serial).Value)

                        g.FillRectangle(color, rect)
                        x += width
                    Next

                    top += barplotHeight + dbarInterval
                Next

                ' 现在开始绘制legend
                top = plotRegion.Top
                left += plotRegion.Width + dbarbox

                For Each label As NamedValue(Of Color) In data.Serials
                    Dim color As New SolidBrush(label.Value)
                    Dim pos As New PointF With {
                        .X = left + boxSize.Width + dboxLabel,
                        .Y = top
                    }

                    Call g.FillRectangle(color, left, top, boxSize)
                    Call g.DrawString(label.Name, legendTitleFont, Brushes.Black, pos)

                    top += boxSize.Width + dboxInterval
                Next
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser, margin,
            bg,
            plotInternal)
    End Function
End Module
