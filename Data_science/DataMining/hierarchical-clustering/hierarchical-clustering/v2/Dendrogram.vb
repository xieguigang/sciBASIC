Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module Dendrogram

    <Extension>
    Public Function Plot(hist As Cluster, Optional size$ = "2000,2000", Optional padding$ = g.DefaultPadding, Optional bg$ = "white") As GraphicsData
        Dim theme As New Theme With {
            .background = bg
        }

        Return New DendrogramPanelv2(hist, theme).Plot(size, padding)
    End Function
End Module
