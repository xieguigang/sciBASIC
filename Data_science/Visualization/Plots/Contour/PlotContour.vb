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
                             Optional gridSize$ = "3,3") As GraphicsData

            Dim contour As New MarchingSquares()
            Dim matrix As New MapMatrix(sample)
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim level_cutoff As Double() = matrix.GetPercentages
                    Dim data As Double()() = matrix.GetMatrixInterpolation.MatrixTranspose.ToArray
                    Dim colors As Color() = Designer.GetColors(colorSet, level_cutoff.Length)
                    Dim i As i32 = Scan0
                    Dim dims = matrix.dimension
                    Dim rect = region.PlotRegion
                    Dim scaleX = d3js.scale.linear.domain(New Double() {0, dims.Width}).range(New Double() {rect.Left, rect.Right})
                    Dim scaleY = d3js.scale.linear.domain(New Double() {0, dims.Height}).range(New Double() {rect.Top, rect.Bottom})

                    For Each polygon As GeneralPath In contour.mkIsos(data, levels:=level_cutoff)
                        Dim color As Color = colors(++i)

                        Call polygon.Fill(g, New SolidBrush(color), scaleX, scaleY)
                        Call polygon.Draw(g, Pens.Black, scaleX, scaleY)
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