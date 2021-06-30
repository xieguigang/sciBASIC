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
            Dim contour As New MarchingSquares()
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim level_cutoff As Double() = matrix.GetPercentages
                    Dim data As Double()() = matrix.GetMatrixInterpolation.ToArray

                    For Each polygon As GeneralPath In contour.mkIsos(data, levels:=level_cutoff)
                        Call polygon.Fill(g, Brushes.Red)
                        Call polygon.Draw(g, Pens.Black)
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