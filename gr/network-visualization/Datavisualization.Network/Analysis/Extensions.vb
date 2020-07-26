#Region "Microsoft.VisualBasic::0a6ceb9f4b477c99766be2864c257541, gr\network-visualization\Datavisualization.Network\Analysis\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: DecomposeGraph, isTupleEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Linq

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
        Public Function isTupleEdge(Of Node As INamedValue, IEdge As {Class, IInteraction})(edge As IEdge, g As GraphIndex(Of Node, IEdge)) As Boolean
            Dim uset = g.GetEdges(edge.source).ToArray
            Dim vset = g.GetEdges(edge.target).ToArray

            If uset.Length = 1 AndAlso vset.Length = 1 AndAlso uset(Scan0) Is vset(Scan0) AndAlso uset(Scan0) Is edge Then
                Return True
            Else
                Return False
            End If
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
        Public Iterator Function DecomposeGraph(g As NetworkGraph,
                                                Optional weakMode As Boolean = True,
                                                Optional minVertices As Integer = 5) As IEnumerable(Of NetworkGraph)

            Dim analysis As Kosaraju = Kosaraju.StronglyConnectedComponents(g)
            Dim subnetwork As NetworkGraph
            Dim nodes As Node()

            For Each part As Edge() In analysis.GetComponents.Where(Function(a) a.Length <> g.size.edges)
                subnetwork = New NetworkGraph
                nodes = part _
                    .Select(Function(a) {a.U, a.V}) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray

                If nodes.Length < minVertices Then
                    Continue For
                End If

                For Each v As Node In nodes.Select(Function(a) a.Clone)
                    Call subnetwork.AddNode(v)
                Next
                For Each edge As Edge In part.Select(Function(a) a.Clone)
                    Call subnetwork.CreateEdge(edge.U, edge.V, 0, edge.data)
                Next

                Yield subnetwork
            Next
        End Function
    End Module
End Namespace
