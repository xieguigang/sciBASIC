Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime

Public Module RegressionPlot

    <Extension>
    Public Function Plot(fit As FittedResult,
                         Optional size$ = "2000,1800",
                         Optional bg$ = "white",
                         Optional margin$ = g.DefaultPadding) As GraphicsData
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

            End Sub

        Return g.GraphicsPlots(
            size.SizeParser,
            margin,
            bg,
            plotInternal)
    End Function
End Module
