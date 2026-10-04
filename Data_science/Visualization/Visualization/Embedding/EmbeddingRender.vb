#Region "Microsoft.VisualBasic::afc87cf68bc5bcb8a2c2f06abc8a0092, Data_science\Visualization\Visualization\Embedding\EmbeddingRender.vb"

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

    '   Total Lines: 61
    '    Code Lines: 45 (73.77%)
    ' Comment Lines: 3 (4.92%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (21.31%)
    '     File Size: 2.02 KB


    ' Class EmbeddingRender
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetClusterColors, getClusterLabel
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq

#If NET48 Then
Imports SolidBrush = System.Drawing.SolidBrush
Imports Brushes = System.Drawing.Brushes
#Else
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
#End If

Public MustInherit Class EmbeddingRender : Inherits Plot

    Protected ReadOnly labels As String()

    ''' <summary>
    ''' [label => clusterid]
    ''' </summary>
    Protected ReadOnly clusters As Dictionary(Of String, String)
    Protected ReadOnly umap As IDataEmbedding

    ReadOnly colorSet As String

    Protected Sub New(umap As IDataEmbedding, labels$(), clusters As Dictionary(Of String, String), colorSet$, theme As Theme)
        MyBase.New(theme)

        Me.clusters = clusters
        Me.colorSet = colorSet
        Me.umap = umap
        Me.labels = labels.ToArray
    End Sub

    Protected Function getClusterLabel(i As Integer) As String
        If clusters.IsNullOrEmpty OrElse Not clusters.ContainsKey(labels(i)) Then
            Return "n/a"
        Else
            Return clusters(labels(i))
        End If
    End Function

    Protected Function GetClusterColors() As Dictionary(Of String, SolidBrush)
        Dim map As New Dictionary(Of String, SolidBrush)

        If Not clusters.IsNullOrEmpty Then
            Dim clusterLabels As String() = clusters.Values.Distinct.ToArray
            Dim colors As Color() = Designer.GetColors(colorSet, clusterLabels.Length)

            For i As Integer = 0 To clusterLabels.Length - 1
                map(clusterLabels(i)) = New SolidBrush(colors(i))
            Next
        End If

        map("n/a") = Brushes.Gray

        Return map
    End Function

    ''' <summary>
    ''' 把降维结果按聚类分组拆成 DataPlot 的数据系列。
    ''' 二维 embedding 的绘制统一交给 <see cref="ScatterPlot"/> 完成，这里只负责分组与配色。
    ''' </summary>
    Protected Function Build2DSeries() As List(Of Series)
        Dim embeddings As PointF() = umap.GetPoint2D
        Dim serials As New List(Of Series)()

        If clusters.IsNullOrEmpty Then
            serials.Add(ToSeries("ungroups", Color.Gray, embeddings))
            Return serials
        End If

        Dim colorMap = GetClusterColors()
        Dim maps As New Dictionary(Of String, List(Of PointF))()

        For Each clusterId In colorMap
            maps(clusterId.Key) = New List(Of PointF)()
        Next

        For i As Integer = 0 To embeddings.Length - 1
            maps(getClusterLabel(i)).Add(embeddings(i))
        Next

        For Each group In maps.Where(Function(a) a.Value.Count > 0).ToArray()
            serials.Add(ToSeries(group.Key, colorMap(group.Key).Color, group.Value.ToArray()))
        Next

        Return serials
    End Function

    Private Function ToSeries(name As String, color As Color, points As PointF()) As Series
        Return New Series With {
            .Name = name,
            .Color = color,
            .MarkerShape = MarkerShape.Circle,
            .PointSize = CSng(theme.pointSize),
            .Visible = True,
            .X = points.Select(Function(p) CDbl(p.X)).ToArray(),
            .Y = points.Select(Function(p) CDbl(p.Y)).ToArray()
        }
    End Function
End Class
