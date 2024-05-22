#Region "Microsoft.VisualBasic::2ae81d5ac94f4ea9d6e64d2aa43d6a83, gr\network-visualization\Datavisualization.Network\Analysis\Model\GraphIndex.vb"

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

    '   Total Lines: 130
    '    Code Lines: 82 (63.08%)
    ' Comment Lines: 28 (21.54%)
    '    - Xml Docs: 82.14%
    ' 
    '   Blank Lines: 20 (15.38%)
    '     File Size: 4.98 KB


    '     Class GraphIndex
    ' 
    '         Function: AddEdge, CreateNodeAdjacencySet, edges, (+2 Overloads) GetEdges, nodes
    ' 
    '         Sub: Clear, Delete, RemoveEdge
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract
Imports Microsoft.VisualBasic.Linq

Namespace Analysis.Model

    ''' <summary>
    ''' The network graph element index
    ''' </summary>
    ''' <typeparam name="Node">The network node element</typeparam>
    ''' <typeparam name="Edge">The network edge element</typeparam>
    Public Class GraphIndex(Of Node As INamedValue, Edge As IInteraction)

        ''' <summary>
        ''' 应用于按照节点的<see cref="Graph.Node.Label"/>为键名进行节点对象的快速查找
        ''' </summary>
        Dim _nodeSet As New Dictionary(Of String, Node)
        Dim _adjacencySet As New Dictionary(Of String, AdjacencySet(Of Edge))

        Default Public Property NodeAccess(label As String) As Node
            Get
                Return _nodeSet.TryGetValue(label)
            End Get
            Set(value As Node)
                _nodeSet(label) = value
            End Set
        End Property

        Public Function nodes(nodeSet As IEnumerable(Of Node)) As GraphIndex(Of Node, Edge)
            _nodeSet = nodeSet.ToDictionary(Function(n) n.Key)
            Return Me
        End Function

        Public Function edges(edgeSet As IEnumerable(Of Edge)) As GraphIndex(Of Node, Edge)
            For Each edge As Edge In edgeSet.SafeQuery
                Call AddEdge(edge)
            Next

            Return Me
        End Function

        Public Sub Delete(node As Node)
            If _nodeSet.ContainsKey(node.Key) Then
                Call _nodeSet.Remove(node.Key)
            End If
            If _adjacencySet.ContainsKey(node.Key) Then
                Call _adjacencySet.Remove(node.Key)
            End If
        End Sub

        ''' <summary>
        ''' Delete a graph edge connection from current network graph model
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub RemoveEdge(edge As Edge)
            If _adjacencySet.ContainsKey(edge.source) Then
                Dim u_adjacencySet As AdjacencySet(Of Edge) = _adjacencySet(edge.source)

                Call u_adjacencySet.Remove(edge.target)

                If u_adjacencySet.Count = 0 Then
                    Call _adjacencySet.Remove(edge.source)
                End If
            End If
        End Sub

        Public Sub Clear()
            Call _nodeSet.Clear()
            Call _adjacencySet.Clear()
        End Sub

        Public Function CreateNodeAdjacencySet(node As INamedValue) As AdjacencySet(Of Edge)
            _adjacencySet.Add(node.Key, New AdjacencySet(Of Edge))
            Return _adjacencySet(node.Key)
        End Function

        ''' <summary>
        ''' 获取目标两个节点之间的所有的重复的边连接
        ''' </summary>
        ''' <param name="u"></param>
        ''' <param name="v"></param>
        ''' <returns>get a set of the directed edges</returns>
        Public Function GetEdges(u As Node, v As Node) As IEnumerable(Of Edge)
            If u Is Nothing OrElse v Is Nothing OrElse u.Key Is Nothing OrElse v.Key Is Nothing Then
                Return Nothing
            ElseIf Not _adjacencySet.ContainsKey(u.Key) Then
                Return Nothing
            Else
                Return _adjacencySet(u.Key).EnumerateAllEdges(v)
            End If
        End Function

        ''' <summary>
        ''' 获取得到与目标节点所有相连接的节点
        ''' </summary>
        ''' <param name="nodeKey"></param>
        ''' <returns></returns>
        Public Function GetEdges(nodeKey As String) As IEnumerable(Of Edge)
            If Not _adjacencySet.ContainsKey(nodeKey) Then
                Return {}
            Else
                Return _adjacencySet(nodeKey).EnumerateAllEdges
            End If
        End Function

        Public Function AddEdge(edge As Edge) As (U As AdjacencySet(Of Edge), V As AdjacencySet(Of Edge))
            Dim U, V As AdjacencySet(Of Edge)

            ' 20191106
            ' 添加edge的时候需要将u和v都添加一次
            ' 否则_adjacencySet之中将只会出现U节点的数据
            ' V节点可能不存在
            ' 数据不完整
            If Not _adjacencySet.ContainsKey(edge.source) Then
                _adjacencySet(edge.source) = New AdjacencySet(Of Edge) With {.U = edge.source}
            End If
            If Not _adjacencySet.ContainsKey(edge.target) Then
                _adjacencySet(edge.target) = New AdjacencySet(Of Edge) With {.U = edge.target}
            End If

            U = _adjacencySet(edge.source)
            V = _adjacencySet(edge.target)

            Call _adjacencySet(edge.source).Add(edge)
            Call _adjacencySet(edge.target).Add(edge)

            Return (U, V)
        End Function
    End Class
End Namespace
