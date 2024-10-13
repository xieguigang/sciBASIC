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
Imports Microsoft.VisualBasic.DataMining.ComponentModel
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
End Class
