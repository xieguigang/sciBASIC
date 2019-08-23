Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Data.csv.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Heatmap

    Public Module BubbleHeatmap

        <Extension>
        Public Function Plot(data As IEnumerable(Of DataSet),
                             Optional size$ = "300,2700",
                             Optional bg$ = "white",
                             Optional margin$ = g.DefaultLargerPadding,
                             Optional colors$ = "") As GraphicsData

        End Function
    End Module
End Namespace