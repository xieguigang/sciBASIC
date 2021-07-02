Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.MarchingSquares
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Contour

    Public Module PlotContour

        <Extension>
        Public Function Plot(sample As IEnumerable(Of MeasureData),
                             Optional size$ = "3600,2400",
                             Optional padding$ = "padding:100px 400px 100px 100px;",
                             Optional bg$ = "white",
                             Optional colorSet$ = "Jet",
                             Optional legendTitle$ = "Contour Levels",
                             Optional legendTitleCSS$ = CSSFont.Win7LargeBold,
                             Optional tickCSS$ = CSSFont.Win10NormalLarger,
                             Optional tickAxisStroke$ = Stroke.ScatterLineStroke,
                             Optional ppi% = 300) As GraphicsData

            Dim contours As GeneralPath() = ContourLayer.GetContours(sample).ToArray
            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .colorSet = colorSet,
                .legendTitleCSS = legendTitleCSS,
                .legendTickCSS = tickCSS,
                .legendTickAxisStroke = tickAxisStroke
            }
            Dim plotApp As New ContourPlot(contours, theme) With {
                .legendTitle = legendTitle
            }

            Return plotApp.Plot(size, ppi)
        End Function

        <Extension>
        Public Function Plot(sample As IEnumerable(Of ContourLayer),
                             Optional size$ = "2700,2000",
                             Optional padding$ = "padding:100px 400px 100px 100px;",
                             Optional bg$ = "white",
                             Optional colorSet$ = "Jet",
                             Optional legendTitle$ = "Contour Levels",
                             Optional legendTitleCSS$ = CSSFont.Win7LargeBold,
                             Optional tickCSS$ = CSSFont.Win10NormalLarger,
                             Optional tickAxisStroke$ = Stroke.ScatterLineStroke,
                             Optional ppi% = 300) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .colorSet = colorSet,
                .legendTitleCSS = legendTitleCSS,
                .legendTickCSS = tickCSS,
                .legendTickAxisStroke = tickAxisStroke
            }
            Dim plotApp As New ContourPlot(sample, theme) With {
                .legendTitle = legendTitle
            }

            Return plotApp.Plot(size, ppi)
        End Function
    End Module
End Namespace