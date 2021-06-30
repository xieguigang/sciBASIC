Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Contour

    Public Module PlotContour

        <Extension>
        Public Function Plot(sample As IEnumerable(Of MeasureData),
                             Optional size$ = "2700,2000",
                             Optional padding$ = "padding:100px 400px 100px 100px;",
                             Optional bg$ = "white",
                             Optional colorSet$ = "Jet",
                             Optional legendTitle$ = "Contour Levels",
                             Optional legendTitleCSS$ = CSSFont.Win7LargeBold,
                             Optional tickCSS$ = CSSFont.Win10NormalLarger,
                             Optional tickAxisStroke$ = Stroke.ScatterLineStroke) As GraphicsData

            Dim contour As New MarchingSquares()
            Dim matrix As New MapMatrix(sample)
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim level_cutoff As Double() = matrix.GetPercentages
                    Dim data As Double()() = matrix.GetMatrixInterpolation.MatrixTranspose.ToArray
                    Dim colors As SolidBrush() = Designer.GetColors(colorSet, level_cutoff.Length).Select(Function(c) New SolidBrush(c)).ToArray
                    Dim i As i32 = Scan0
                    Dim dims = matrix.dimension
                    Dim rect = region.PlotRegion
                    Dim scaleX = d3js.scale.linear.domain(New Double() {0, dims.Width}).range(New Double() {rect.Left, rect.Right})
                    Dim scaleY = d3js.scale.linear(True).domain(New Double() {0, dims.Height}).range(New Double() {rect.Top, rect.Bottom})

                    For Each polygon As GeneralPath In contour.mkIsos(data, levels:=level_cutoff)
                        Dim color As SolidBrush = colors(++i)

                        Call polygon.Fill(g, color, scaleX, scaleY)
                        Call polygon.Draw(g, Pens.Black, scaleX, scaleY)
                    Next

                    Dim layout As New Rectangle(rect.Right + 10, rect.Top, region.Padding.Right / 3 * 2, rect.Height / 3 * 2)
                    Dim legendTitleFont As Font = CSSFont.TryParse(legendTitleCSS)
                    Dim tickFont As Font = CSSFont.TryParse(tickCSS)
                    Dim tickStroke As Pen = Stroke.TryParse(tickAxisStroke)

                    Call g.ColorMapLegend(layout, colors, level_cutoff, legendTitleFont, title:=legendTitle, tickFont, tickStroke)
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