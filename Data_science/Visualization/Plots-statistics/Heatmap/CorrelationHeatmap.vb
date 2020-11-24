
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

        End Sub
    End Class
End Namespace