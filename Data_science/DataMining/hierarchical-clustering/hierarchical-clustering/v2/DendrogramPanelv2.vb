Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.DendrogramVisualize
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Public Class DendrogramPanelv2 : Inherits Plot

    Friend ReadOnly hist As Cluster
    Friend ReadOnly layout As Layouts

    Public Sub New(hist As Cluster, theme As Theme)
        MyBase.New(theme)

        Me.hist = hist
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim plotRegion As Rectangle = canvas.PlotRegion
        ' 每一个样本点都平分一段长度
        Dim unitWidth As Double = plotRegion.Height / hist.Leafs

    End Sub
End Class
