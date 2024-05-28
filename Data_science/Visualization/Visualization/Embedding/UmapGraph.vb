#Region "Microsoft.VisualBasic::11803501733baeefb8d82d65b2a5a4f8, Data_science\Visualization\Visualization\Embedding\UMAPGraph.vb"

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

    '   Total Lines: 72
    '    Code Lines: 57 (79.17%)
    ' Comment Lines: 3 (4.17%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (16.67%)
    '     File Size: 2.74 KB


    ' Module UMAPGraph
    ' 
    '     Function: BuildGraph, (+2 Overloads) CreateGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Language
Imports std = System.Math

''' <summary>
''' create network model based on umap result for data visualization
''' </summary>
Public Module UMAPGraph

    <Extension>
    Public Function CreateGraph(umap As UMAPProject, Optional threshold As Double = 0) As NetworkGraph
        Return BuildGraph(umap.graph, umap.embedding, umap.labels.UniqueNames, umap.labels, umap.clusters, threshold)
    End Function

    Private Function BuildGraph(matrix As Double()(), embedding As Double()(), uid As String(), labels As String(), clusters As String(), threshold As Double) As NetworkGraph
        Dim g As New NetworkGraph
        Dim points As PointF() = embedding _
            .Select(Function(v) New PointF(v(0), v(1))) _
            .ToArray
        Dim data As NodeData = Nothing
        Dim index As i32 = Scan0

        If labels Is Nothing Then
            labels = uid
        End If

        Dim getLabel As Func(Of String) = Function() labels(index)
        Dim has_clusters As Boolean = Not clusters.IsNullOrEmpty

        For Each label As String In uid
            data = New NodeData With {
                .label = getLabel(),
                .origID = getLabel()
            }

            If has_clusters Then
                data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = clusters(index)
            End If
            If Not points Is Nothing Then
                data.initialPostion = New FDGVector2(points(++index))
            Else
                index += 1
            End If

            Call g.CreateNode(label, data)
        Next

        For i As Integer = 0 To matrix.Length - 1
            For j As Integer = 0 To matrix(i).Length - 1
                If i <> j AndAlso std.Abs(matrix(i)(j)) > threshold Then
                    Call g.CreateEdge(uid(i), uid(j), weight:=matrix(i)(j))
                End If
            Next
        Next

        Return g
    End Function

    <Extension>
    Public Function CreateGraph(umap As Umap, uid As String(),
                                Optional labels As String() = Nothing,
                                Optional threshold As Double = 0) As NetworkGraph

        Return BuildGraph(umap.GetGraph.ToArray, umap.GetEmbedding, uid, labels, Nothing, threshold)
    End Function
End Module
