Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Contour

    Public Module PlotContour

        <Extension>
        Public Function Plot(sample As IEnumerable(Of MeasureData),
                             Optional size$ = "2700,2000",
                             Optional padding$ = g.DefaultLargerPadding,
                             Optional bg$ = "white",
                             Optional colorSet$ = "Jet",
                             Optional gridSize$ = "5,5") As GraphicsData

            Dim matrix As New MapMatrix(sample, size.SizeParser, gridSize.SizeParser)
            Dim contour As New MarchingSquares()
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim level_cutoff As Double() = matrix.GetPercentages
                    Dim data As Double()() = matrix.GetMatrixInterpolation.ToArray
                    Dim colors As Color() = Designer.GetColors(colorSet, level_cutoff.Length)
                    Dim i As i32 = Scan0

                    For Each polygon As GeneralPath In contour.mkIsos(data, levels:=level_cutoff)
                        Dim color As Color = colors(++i)

                        Call polygon.Fill(g, New SolidBrush(color))
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