Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Contour

    Public Module PlotContour

        <Extension>
        Public Function Plot(sample As IEnumerable(Of MeasureData),
                             Optional size$ = "2700,2000",
                             Optional padding$ = g.DefaultLargerPadding,
                             Optional bg$ = "white",
                             Optional gridSize$ = "5,5") As GraphicsData

            Dim matrix As New MapMatrix(sample, size.SizeParser, gridSize.SizeParser)
            Dim contour As New MarchingSquares(matrix)
            Dim q As QuantileEstimationGK = matrix.GetLevelQuantile
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim level_cutoff As Double = q.Query(0.5)

                    For Each polygon As PointF() In contour.CreateMapData(threshold:=level_cutoff)
                        Call g.DrawPolygon(Pens.Black, polygon)
                    Next
                End Sub

            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=padding,
                bg:=bg,
                plotAPI:=plotInternal
            )
        End Function
    End Module
End Namespace