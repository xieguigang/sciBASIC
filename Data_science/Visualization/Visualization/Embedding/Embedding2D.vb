#Region "Microsoft.VisualBasic::4a14173d6b0fd2796c68804c5bb93840, Data_science\Visualization\Visualization\Embedding\Embedding2D.vb"

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
    '    Code Lines: 68 (86.08%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (13.92%)
    '     File Size: 2.93 KB


    ' Class Embedding2D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

Public Class Embedding2D : Inherits EmbeddingRender

    ReadOnly showConvexHull As Boolean

    Public Sub New(umap As IDataEmbedding, labels$(), clusters As Dictionary(Of String, String), colorSet$, showConvexHull As Boolean, theme As Theme)
        MyBase.New(umap, labels, clusters, colorSet, theme)

        Me.showConvexHull = showConvexHull
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        ' 二维embedding的绘制改由 DataPlot 的散点图引擎完成：
        ' 老的 ChartPlots.Scatter2D 已经不再使用，这里只把分组好的数据喂进去。
        Dim serials = Build2DSeries()

        Using scatter As New ScatterPlot(g)
            scatter.ShowLegend = True
            scatter.ShowConvexHull = showConvexHull
            scatter.Plot(serials)
        End Using
    End Sub
End Class
