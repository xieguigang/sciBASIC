Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
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
                             Optional title$ = "PCA ScreePlot",
                             Optional titleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                             Optional tickFontStyle$ = CSSFont.Win7Large,
                             Optional labelFontStyle$ = CSSFont.Win7VeryLarge) As GraphicsData

            Dim cv As Vector = pca.CumulativeVariance
            Dim X As Vector = (cv.Dim + 1) _
                .Sequence _
                .AsVector _
                .CreateAxisTicks
            Dim Y As Vector = cv.CreateAxisTicks
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim rect As Rectangle = region.PlotRegion
                    Dim Xscaler = d3js.scale.linear.domain(X).range(integers:={rect.Left, rect.Right})
                    Dim Yscaler = d3js.scale.linear.domain(Y).range(integers:={rect.Top, rect.Bottom})
                    Dim scaler As New DataScaler With {
                        .AxisTicks = (X, Y),
                        .X = Xscaler,
                        .Y = Yscaler,
                        .Region = rect
                    }

                    Call g.DrawAxis(
                        region, scaler, True,
                        xlabel:="Components", ylabel:="Variances",
                        htmlLabel:=False,
                        tickFontStyle:=tickFontStyle,
                        labelFont:=labelFontStyle
                    )
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, margin, bg,
                plotInternal
            )
        End Function
    End Module
End Namespace