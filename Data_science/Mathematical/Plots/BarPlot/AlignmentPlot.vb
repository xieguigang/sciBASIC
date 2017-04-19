Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting

Namespace BarPlot

    Public Module AlignmentPlot

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="query"></param>
        ''' <param name="subject"></param>
        ''' <param name="cla$">Color expression for <paramref name="query"/></param>
        ''' <param name="clb$">Color expression for <paramref name="subject"/></param>
        ''' <returns></returns>
        <Extension>
        Public Function PlotAlignment(query As Dictionary(Of Double, Double),
                                      subject As Dictionary(Of Double, Double),
                                      Optional size$ = "960,600",
                                      Optional padding$ = g.DefaultPadding,
                                      Optional cla$ = "steelblue",
                                      Optional clb$ = "brown",
                                      Optional xlab$ = "X",
                                      Optional ylab$ = "Y",
                                      Optional title$ = "Alignments Plot") As GraphicsData
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)

                End Sub

            Return g.GraphicsPlots(
                size.SizeParser, padding,
                "white",
                plotInternal)
        End Function
    End Module
End Namespace