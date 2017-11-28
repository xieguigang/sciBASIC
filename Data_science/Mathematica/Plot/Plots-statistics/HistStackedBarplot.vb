Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramVisualize
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module HistStackedBarplot

    ' plot layout
    ' tree  stacked-barplot  legend

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"></param>
    ''' <param name="size$"></param>
    ''' <param name="margin$"></param>
    ''' <param name="treeWidth#">假若这个宽度值在[0-1]之之间，则认为是百分比，反之当这个宽度值超过了1，则认为是实际的像素值</param>
    ''' <returns></returns>
    Public Function Plot(data As BarDataGroup,
                         Optional size$ = "3000,2700",
                         Optional margin$ = g.DefaultPadding,
                         Optional bg$ = "white",
                         Optional treeWidth# = 0.2,
                         Optional sampleGroup As Dictionary(Of String, String) = Nothing) As GraphicsData

        Dim array As DataSet() = data.Samples _
            .Select(Function(sample)
                        Return New DataSet With {
                            .ID = sample.Tag,
                            .Properties = data.Serials _
                                .Keys _
                                .SeqIterator _
                                .ToDictionary(Function(key) key.value,
                                              Function(i) sample.data(i))
                        }
                    End Function) _
            .ToArray
        Dim histCanvas = Function(cluster As Cluster)
                             Return New DendrogramPanel With {
                                 .LineColor = Color.Black,
                                 .ScaleValueDecimals = 0,
                                 .ScaleValueInterval = 1,
                                 .Model = cluster,
                                 .ShowScale = False,
                                 .ShowDistanceValues = False,
                                 .ShowLeafLabel = False,
                                 .LinkDotRadius = 0,
                                 .ClassTable = sampleGroup
                             }
                         End Function

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim plotRegion As Rectangle = region.PlotRegion
                Dim treeRegion As New Rectangle With {
                    .Location = plotRegion.Location,
                    .Width = MeasureWidthOrHeight(treeWidth, plotRegion.Width),
                    .Height = plotRegion.Height
                }

                ' 首先绘制出层次聚类树
                ' rowKeys得到的是sample的从上到下的绘图顺序
                Dim cluster As Cluster = Time(AddressOf array.RunCluster)
                Dim rowKeys$() = histCanvas(cluster) _
                    .Paint(DirectCast(g, Graphics2D), treeRegion) _
                    .OrderBy(Function(x) x.Value.Y) _
                    .Keys

                Dim left! = treeRegion.Right
                Dim top! = treeRegion.Top

                plotRegion = New Rectangle With {
                    .X = left,
                    .Y = top,
                    .Width = plotRegion.Width - treeRegion.Width,
                    .Height = plotRegion.Height
                }
            End Sub

        Return g.GraphicsPlots(
            size.SizeParser, margin,
            bg,
            plotInternal)
    End Function
End Module
