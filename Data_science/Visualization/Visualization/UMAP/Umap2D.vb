Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Public Class Umap2D ： Inherits UmapRender

    Public Sub New(umap As Umap, labels$(), clusters As Dictionary(Of String, String), colorSet$, theme As Theme)
        MyBase.New(umap, labels, clusters, colorSet, theme)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim embeddings As PointF() = umap.GetPoint2D
    End Sub
End Class
