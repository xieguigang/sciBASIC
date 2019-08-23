Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace Heatmap

    ''' <summary>
    ''' 普通的热图是整体进行比对，使用填充在方格内的颜色来区分数值
    ''' 而泡泡热图，则是进行单个行数据在样本间的比较，行之间可以使用不同的颜色区分，数值使用泡泡的半径大小来表示
    ''' </summary>
    Public Module BubbleHeatmap

        <Extension>
        Public Function Plot(data As IEnumerable(Of DataSet),
                             Optional size$ = "300,2700",
                             Optional bg$ = "white",
                             Optional margin$ = g.DefaultLargerPadding,
                             Optional colors$ = DesignerTerms.GoogleMaterialPalette,
                             Optional minRadius! = 1,
                             Optional scaleMethod As DrawElements = DrawElements.Rows) As GraphicsData

            Dim dataMatrix = data.ToArray
            Dim columnNames$() = dataMatrix.PropertyNames
            Dim nrows = dataMatrix.Length
            Dim ncols = dataMatrix(Scan0).Properties.Count
            Dim valueRanges As DoubleRange()

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim plotRegion As Rectangle = region.PlotRegion
                    ' 应该是正方形的
                    Dim maxRadius = stdNum.Min(plotRegion.Width / ncols, plotRegion.Height / nrows)

                End Sub

            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=margin,
                bg:=bg,
                plotAPI:=plotInternal
            )
        End Function
    End Module
End Namespace