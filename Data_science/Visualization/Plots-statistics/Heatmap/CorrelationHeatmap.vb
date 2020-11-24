
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Heatmap

    Public Class CorrelationHeatmap : Inherits Plot

        Dim cor As CorrelationData
        Dim hist As Cluster

        Public Sub New(cor As CorrelationData, theme As Theme)
            MyBase.New(theme)

            Me.cor = cor
            Me.hist = New DefaultClusteringAlgorithm().performClustering(
                distances:=cor.GetMatrix,
                clusterNames:=cor.data.keys,
                linkageStrategy:=New AverageLinkageStrategy
            )
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            ' left
            Dim hor As New DendrogramPanelV2(hist, theme)
            ' top
            Dim ver As New Horizon(hist, theme)
            Dim region = canvas.PlotRegion
            Dim labelOrders As String() = hist.OrderLeafs

            Call hor.Plot(g, New Rectangle(New Point(region.Left, region.Top), New Size(0.1 * g.Size.Width, region.Height)))
            Call ver.Plot(g, New Rectangle(New Point(region.Left, region.Top), New Size(region.Width, 0.1 * g.Size.Height)))

            For i As Integer = 0 To labelOrders.Length - 1
                For j As Integer = 0 To labelOrders.Length - 1

                Next
            Next
        End Sub
    End Class
End Namespace