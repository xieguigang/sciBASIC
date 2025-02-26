#Region "Microsoft.VisualBasic::aaf3f83872684f4277ab8afa8d4e5ec0, gr\network-visualization\Network.IO.Extensions\Cytoscape\CytoscapeTableLoader.vb"

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

    '   Total Lines: 110
    '    Code Lines: 90 (81.82%)
    ' Comment Lines: 6 (5.45%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (12.73%)
    '     File Size: 4.88 KB


    '     Module CytoscapeTableLoader
    ' 
    '         Function: (+2 Overloads) CytoscapeExportAsGraph, CytoscapeExportAsTable, CytoscapeNetworkFromEdgeTable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

Namespace FileStream.Cytoscape

    Public Module CytoscapeTableLoader

        ''' <summary>
        ''' Load cytoscape exports as network graph model.
        ''' </summary>
        ''' <param name="edgesTable">``edges.csv``</param>
        ''' <param name="nodesTable">``nodes.csv``</param>
        ''' <returns></returns>
        Public Function CytoscapeExportAsGraph(edgesTable As String, nodesTable As String) As NetworkGraph
            Dim edges As Edges() = edgesTable.LoadCsv(Of Edges)
            Dim nodes As Nodes() = nodesTable.LoadCsv(Of Nodes)

            Return CytoscapeExportAsGraph(edges, nodes)
        End Function

        Public Function CytoscapeExportAsTable(edgesTable As String, nodesTable As String) As NetworkTables
            Dim edges As NetworkEdge() = edgesTable.LoadCsv(Of Edges) _
                .Select(Function(edge)
                            Dim interaction = edge.GetConnectNodes

                            Return New NetworkEdge With {
                                .fromNode = interaction(Scan0),
                                .toNode = interaction(1),
                                .interaction = edge.interaction,
                                .value = edge.EdgeBetweenness,
                                .Properties = edge.data
                            }
                        End Function) _
                .ToArray
            Dim nodes As Node() = nodesTable.LoadCsv(Of Nodes) _
                .Select(Function(node)
                            Return New Node With {
                                .ID = node.name,
                                .NodeType = "node",
                                .Properties = node.ToTable
                            }
                        End Function) _
                .ToArray

            Return New NetworkTables(edges, nodes)
        End Function

        <Extension>
        Public Function CytoscapeNetworkFromEdgeTable(edgesData As IEnumerable(Of Edges)) As NetworkGraph
            Dim edges = edgesData.ToArray
            Dim nodes = edges _
                .Select(Function(e) e.GetConnectNodes) _
                .IteratesALL _
                .Distinct _
                .Select(Function(id)
                            Return New Nodes With {
                                .name = id
                            }
                        End Function) _
                .ToArray

            Dim graph As NetworkGraph = CytoscapeExportAsGraph(edges, nodes)
            Return graph
        End Function

        Public Function CytoscapeExportAsGraph(edges As Edges(), nodes As Nodes()) As NetworkGraph
            Dim colors As Color() = AllDotNetPrefixColors
            Dim randColor As Func(Of Color) =
                Function()
                    Dim index = RandomSingle() * (colors.Length - 1)
                    Dim color As Color = colors(index)

                    Return Color.FromArgb(
                        baseColor:=color,
                        alpha:=225
                    )
                End Function

            Dim gNodes As List(Of Graph.Node) =
 _
                LinqAPI.MakeList(Of Graph.Node) <= From n As Nodes
                                                   In nodes
                                                   Let r = If(n.Degree <= 4, 4, n.Degree) * 5
                                                   Let nd As NodeData = New NodeData With {
                                                       .size = {r},
                                                       .color = New SolidBrush(randColor())
                                                   }
                                                   Select New Graph.Node(n.name, nd)

            Dim nodesTable As New Dictionary(Of Graph.Node)(gNodes)
            Dim gEdges As List(Of Graph.Edge) =
 _
                LinqAPI.MakeList(Of Edge) <= From edge As Edges
                                             In edges
                                             Let geNodes As Graph.Node() = edge.GetNodes(nodesTable).ToArray
                                             Let u = geNodes(0)
                                             Let v = geNodes(1)
                                             Select New Edge(edge.SUID, u, v, New EdgeData)

            Return New NetworkGraph(gNodes, gEdges)
        End Function
    End Module
End Namespace
