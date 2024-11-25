#Region "Microsoft.VisualBasic::8a2105d4076304178537746b9a4569a8, gr\network-visualization\Datavisualization.Network\Analysis\Extensions.vb"

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

    '   Total Lines: 158
    '    Code Lines: 106 (67.09%)
    ' Comment Lines: 28 (17.72%)
    '    - Xml Docs: 82.14%
    ' 
    '   Blank Lines: 24 (15.19%)
    '     File Size: 6.71 KB


    '     Module Extensions
    ' 
    '         Function: (+3 Overloads) DecomposeGraph, DecomposeGraphByGroup, getEdgeSet, isTupleEdge, IteratesSubNetworks
    ' 
    ' 
    '     Class NodeReader
    ' 
    '         Function: getMetadata, hasMetadata
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Namespace Analysis

    <HideModuleName> Public Module Extensions

        ' a-b is tuple
        ' a-b-c is not

        ''' <summary>
        ''' 判断边的两个节点是否只有当前的边连接而再无其他的任何边连接了
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="edge"></param>
        ''' <returns></returns>
        <Extension>
        Public Function isTupleEdge(Of Node As INamedValue, IEdge As {Class, SparseGraph.IInteraction})(edge As IEdge, g As GraphIndex(Of Node, IEdge)) As Boolean
            Dim uset = g.GetEdges(edge.source).ToArray
            Dim vset = g.GetEdges(edge.target).ToArray

            If uset.Length = 1 AndAlso vset.Length = 1 AndAlso uset(Scan0) Is vset(Scan0) AndAlso uset(Scan0) Is edge Then
                Return True
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Iterator Function DecomposeGraphByGroup(g As NetworkGraph, Optional minVertices As Integer = 5) As IEnumerable(Of NetworkGraph)
            Dim nodeGroups = g.vertex.GroupBy(Function(v) v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)).ToArray
            Dim getEdgeGoups =
                Iterator Function() As IEnumerable(Of Edge())
                    For Each nodeSet In nodeGroups
                        Yield g.getEdgeSet(nodeSet)
                    Next
                End Function

            For Each group In getEdgeGoups().DecomposeGraph(minVertices)
                Yield group
            Next
        End Function

        <Extension>
        Public Function getEdgeSet(g As NetworkGraph, nodeSet As IEnumerable(Of Node)) As Edge()
            ' find a subset of edges from a
            ' given node set of the network.
            Dim id As Index(Of String) = nodeSet _
                .Select(Function(v) v.label) _
                .Indexing
            Dim edgeSet = g.graphEdges _
                .Where(Function(url)
                           Return url.U.label Like id OrElse url.V.label Like id
                       End Function) _
                .ToArray

            Return edgeSet
        End Function

        <Extension>
        Public Function DecomposeGraph(components As Edge(), minVertices As Integer) As NetworkGraph
            Dim subnetwork As New NetworkGraph
            Dim nodes = components _
                .Select(Function(a) {a.U, a.V}) _
                .IteratesALL _
                .Distinct _
                .ToArray

            If nodes.Length < minVertices Then
                Return Nothing
            End If

            For Each v As Node In nodes.Select(Function(a) a.Clone)
                Call subnetwork.AddNode(v)
            Next
            For Each edge As Edge In components.Select(Function(a) a.Clone)
                Call subnetwork.CreateEdge(edge.U, edge.V, 0, edge.data)
            Next

            Return subnetwork
        End Function

        <Extension>
        Public Iterator Function DecomposeGraph(components As IEnumerable(Of Edge()), minVertices As Integer) As IEnumerable(Of NetworkGraph)
            Dim subnetwork As NetworkGraph

            For Each part As Edge() In components
                subnetwork = part.DecomposeGraph(minVertices)

                If Not subnetwork Is Nothing Then
                    Yield subnetwork
                End If
            Next
        End Function

        ''' <summary>
        ''' Decompose a graph into components, Creates a separate graph for each component of a graph.
        ''' 
        ''' 与<see cref="GraphTheory.Network.IteratesSubNetworks"/>所不同的是，IteratesSubNetworks是分离出独立的子网络
        ''' 而这个函数则是根据连接强度进行子网络的分割
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="minVertices"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DecomposeGraph(g As NetworkGraph,
                                       Optional weakMode As Boolean = True,
                                       Optional minVertices As Integer = 5) As IEnumerable(Of NetworkGraph)

            Dim analysis As Kosaraju = Kosaraju.StronglyConnectedComponents(g)
            Dim components = analysis _
                .GetComponents _
                .Where(Function(a) a.Length <> g.size.edges) _
                .DecomposeGraph(minVertices)

            Return components
        End Function

        ''' <summary>
        ''' 枚举出所输入的网络数据模型之中的所有互不相连的子网络
        ''' </summary>
        ''' <param name="network"></param>
        ''' <param name="edgeCut">
        ''' all of the edge weight less than this 
        ''' cutff value will be ignored.
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function IteratesSubNetworks(network As NetworkGraph,
                                            Optional singleNodeAsGraph As Boolean = False,
                                            Optional edgeCut As Double = -1,
                                            Optional breakKeys As String() = Nothing) As IEnumerable(Of NetworkGraph)

            Return New SubNetworkComponents(Of Node, Edge, NetworkGraph)(network, singleNodeAsGraph, edgeCut, breakKeys, New NodeReader)
        End Function
    End Module

    Friend Class NodeReader : Implements NodeMetaDataAccessor(Of Node)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasMetadata(v As Node, key As String) As Boolean Implements NodeMetaDataAccessor(Of Node).hasMetadata
            Return v.data.HasProperty(key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getMetadata(v As Node, key As String) As String Implements NodeMetaDataAccessor(Of Node).getMetadata
            Return v.data(key)
        End Function
    End Class
End Namespace
