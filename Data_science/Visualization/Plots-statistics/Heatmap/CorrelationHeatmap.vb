#Region "Microsoft.VisualBasic::954e419c9ac9c0bf110dc57a1b67e4e7, Data_science\Visualization\Plots-statistics\Heatmap\CorrelationHeatmap.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 79
    '    Code Lines: 65 (82.28%)
    ' Comment Lines: 2 (2.53%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 12 (15.19%)
    '     File Size: 3.31 KB


    '     Class CorrelationHeatmap
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

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
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim region As Rectangle = canvas.PlotRegion(css)
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
