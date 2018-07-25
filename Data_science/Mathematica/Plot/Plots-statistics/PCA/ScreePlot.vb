Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports PCA_analysis = Microsoft.VisualBasic.Math.LinearAlgebra.PCA

Namespace PCA

    ''' <summary>
    ''' 碎石图
    ''' </summary>
    Public Module ScreePlot

        <Extension>
        Public Function Plot(pca As PCA_analysis,
                             Optional size$ = "3300,2700",
                             Optional margin$ = g.DefaultUltraLargePadding,
                             Optional bg$ = "white",
                             Optional lineStroke$ = Stroke.HighlightStroke,
                             Optional pointSize! = 10,
                             Optional title$ = "PCA ScreePlot") As GraphicsData

            Dim cv As Vector = pca.CumulativeVariance
            Dim X = (cv.Dim + 1).Sequence.AsVector.CreateAxisTicks
            Dim Y = cv.CreateAxisTicks
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)

                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
        End Function
    End Module
End Namespace