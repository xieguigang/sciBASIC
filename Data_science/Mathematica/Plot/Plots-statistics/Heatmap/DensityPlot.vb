Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.Graph
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Heatmap

    ''' <summary>
    ''' 类似于<see cref="Contour"/>，但是这个是基于点的密度来绘图的
    ''' </summary>
    Public Module DensityPlot

        ''' <summary>
        ''' Similar to the <see cref="Contour"/> plot.
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="size$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="schema$"></param>
        ''' <returns></returns>
        Public Function Plot(points As IEnumerable(Of PointF),
                             Optional size$ = "1600,1200",
                             Optional padding$ = g.DefaultPadding,
                             Optional bg$ = "white",
                             Optional schema$ = "Jet",
                             Optional steps$ = Nothing) As GraphicsData

            Dim data = points.VectorShadows
            Dim xrange As DoubleRange = data.X ' As IEnumerable(Of Single)
            Dim yrange As DoubleRange = data.Y ' As IEnumerable(Of Single)
            Dim canvas As Grid = (xrange, yrange).Grid(steps.FloatSizeParser)

            ' create point density function by using grid model

        End Function
    End Module
End Namespace