#Region "Microsoft.VisualBasic::31bd01c239928e3434a7546a58d692ea, Data_science\Visualization\Visualization\Embedding\Embedding2D.vb"

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

    '   Total Lines: 80
    '    Code Lines: 69 (86.25%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (13.75%)
    '     File Size: 2.99 KB


    ' Class Embedding2D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: PlotInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Public Class Embedding2D ： Inherits EmbeddingRender

    ReadOnly showConvexHull As Boolean

    Public Sub New(umap As IDataEmbedding, labels$(), clusters As Dictionary(Of String, String), colorSet$, showConvexHull As Boolean, theme As Theme)
        MyBase.New(umap, labels, clusters, colorSet, theme)

        Me.showConvexHull = showConvexHull
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim embeddings As PointF() = umap.GetPoint2D
        Dim serials As SerialData()
        Dim hullConvexList As String() = Nothing

        If clusters.IsNullOrEmpty Then
            serials = {
                New SerialData With {
                    .color = Color.Gray,
                    .pointSize = theme.pointSize,
                    .shape = LegendStyles.Circle,
                    .title = "ungroups",
                    .pts = embeddings _
                        .Select(Function(p) New PointData(p)) _
                        .ToArray
                }
            }
            hullConvexList = Nothing
        Else
            Dim maps As New Dictionary(Of String, List(Of PointData))
            Dim color = GetClusterColors()

            For Each group In color
                maps(group.Key) = New List(Of PointData)
            Next

            For i As Integer = 0 To embeddings.Length - 1
                maps(getClusterLabel(i)).Add(New PointData(embeddings(i)))
            Next

            serials = maps _
                .Where(Function(c) c.Value.Count > 0) _
                .Select(Function(a)
                            Return New SerialData With {
                                .color = color(a.Key).Color,
                                .pointSize = theme.pointSize,
                                .pts = a.Value.ToArray,
                                .shape = LegendStyles.Circle,
                                .title = a.Key
                            }
                        End Function) _
                .ToArray

            If showConvexHull Then
                hullConvexList = maps _
                    .Keys _
                    .Where(Function(a) a <> "n/a") _
                    .ToArray
            End If
        End If

        Call New Plots.Scatter2D(
            data:=serials,
            theme:=theme,
            scatterReorder:=False,
            fillPie:=True,
            ablines:=Nothing,
            hullConvexList:=hullConvexList
        ).Plot(g, canvas.PlotRegion)
    End Sub
End Class
