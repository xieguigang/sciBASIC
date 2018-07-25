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
                             Optional labelFontStyle$ = CSSFont.Win7VeryLarge,
                             Optional axisStrokeCSS$ = Stroke.AxisStroke) As GraphicsData

            Dim cv As Vector = pca.CumulativeVariance
            Dim X$() = cv.Dim _
                .SeqIterator(offset:=1) _
                .Select(Function(i) $"Comp.{i}") _
                .ToArray
            Dim Y As Vector = cv.CreateAxisTicks
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim rect As Rectangle = region.PlotRegion
                    Dim Xscaler = d3js.scale.ordinal.domain(X).range(integers:={rect.Left, rect.Right})
                    Dim Yscaler = d3js.scale.linear.domain(Y).range(integers:={rect.Top, rect.Bottom})
                    Dim scaler As New TermScaler With {
                        .AxisTicks = (X, Y),
                        .X = Xscaler,
                        .Y = Yscaler,
                        .region = rect
                    }

                    Call g.DrawY(Stroke.TryParse(axisStrokeCSS), "Variances", region, scaler, -1, Y, YAxisLayoutStyles.Left, Nothing, labelFontStyle, CSSFont.TryParse(tickFontStyle), htmlLabel:=False, tickFormat:="F2")
                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, margin, bg,
                plotInternal
            )
        End Function
    End Module
End Namespace