
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Namespace Heatmap

    Public Class CorrelationHeatmap : Inherits Plot

        Dim cor As CorrelationData
        Dim hist As Cluster
        Dim levels As Integer
        Dim treeHeight As Double

        Public Sub New(cor As CorrelationData, theme As Theme,
                       Optional levels As Integer = 20,
                       Optional treeHeight As Double = 0.1)

            MyBase.New(theme)

            Me.cor = cor
            Me.hist = New DefaultClusteringAlgorithm().performClustering(
                distances:=cor.GetMatrix,
                clusterNames:=cor.data.keys,
                linkageStrategy:=New AverageLinkageStrategy
            )
            Me.levels = levels
            Me.treeHeight = treeHeight
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            ' left
            Dim hor As New DendrogramPanelV2(hist, theme, showRuler:=False, showLeafLabels:=False)
            ' top
            Dim ver As New Horizon(hist, theme, showRuler:=False, showLeafLabels:=False)
            Dim region As Rectangle = canvas.PlotRegion
            Dim labelOrders As String() = hist.OrderLeafs
            Dim deltaW As Integer = treeHeight * region.Width
            Dim deltaH As Integer = treeHeight * region.Height
            Dim rectSize As New SizeF With {
                .Width = (region.Width - deltaW) / labelOrders.Length,
                .Height = (region.Height - deltaH) / labelOrders.Length
            }

            Call hor.Plot(g, New Rectangle(New Point(region.Left, region.Top + deltaH - rectSize.Height / 2), New Size(deltaW, region.Height - deltaH)))
            Call ver.Plot(g, New Rectangle(New Point(region.Left + deltaW - rectSize.Width / 2, region.Top), New Size(region.Width - deltaW, deltaH)))

            cor = cor _
                .SetLevels(levels) _
                .SetKeyOrders(labelOrders)

            Dim rect As RectangleF
            Dim left As Integer = region.Left + deltaW
            Dim top As Integer = region.Top + deltaH
            Dim colors As SolidBrush() = Designer _
                .GetColors(theme.colorSet, cor.levelRange.Max) _
                .Select(Function(cl) New SolidBrush(cl)) _
                .ToArray
            Dim level As Integer
            Dim position As PointF

            For i As Integer = 0 To labelOrders.Length - 1
                For j As Integer = 0 To labelOrders.Length - 1
                    position = New PointF With {
                        .X = left + i * rectSize.Width,
                        .Y = top + j * rectSize.Height
                    }
                    rect = New RectangleF(position, rectSize)
                    level = cor.GetLevel(i, j)

                    g.FillRectangle(colors(level), rect)
                Next
            Next
        End Sub
    End Class
End Namespace