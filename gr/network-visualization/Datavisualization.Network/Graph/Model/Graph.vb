#Region "Microsoft.VisualBasic::808105be5df77e41e59a5f8ee7550b39, gr\network-visualization\Datavisualization.Network\Graph\Model\Graph.vb"

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

    '     Class NetworkGraph
    ' 
    '         Properties: connectedNodes
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) AddEdge, AddNode, Clone, ComputeIfNotExists, Copy
    '                   (+2 Overloads) CreateEdge, createEdgeInternal, (+2 Overloads) CreateNode, GetEdge, (+2 Overloads) GetEdges
    '                   GetElementByID, GetNode, ToString
    ' 
    '         Sub: AddGraphListener, Clear, (+2 Overloads) CreateEdges, (+2 Overloads) CreateNodes, DetachNode
    '              FilterEdges, FilterNodes, Merge, notify, RemoveEdge
    '              RemoveNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file Graph.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date August 08, 2013
'@brief Graph Interface
'@version 1.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the Graph Class.
'
'

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Graph

    ''' <summary>
    ''' The network graph object model, corresponding network csv table data model is <see cref="FileStream.NetworkTables"/> 
    ''' </summary>
    Public Class NetworkGraph : Inherits GraphTheory.Network.NetworkGraph(Of Node, Edge)
        Implements ICloneable

        ''' <summary>
        ''' Returns the set of all Nodes that have emanating Edges.
        ''' This therefore returns all Nodes that will be visible in the drawing.
        ''' (这个属性之中是没有任何孤立的节点的)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 对于<see cref="vertex"/>属性而言，其则是所有的节点的集合，
        ''' 包括当前的这个<see cref="connectedNodes"/>和孤立点的总集合
        ''' </remarks>
        Public ReadOnly Property connectedNodes() As Node()
            Get
                Return graphEdges _
                    .Select(Function(d) d.Iterate2Nodes) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' <see cref="Node.Label"/>为键名
        ''' </summary>
        Dim _nodeSet As Dictionary(Of String, Node)
        Dim _adjacencySet As Dictionary(Of String, AdjacencySet)

        Dim _nextNodeId As Integer = 0
        Dim _nextEdgeId As Integer = 0
        Dim _eventListeners As List(Of IGraphEventListener)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New()
            Call Me.New({}, {})
        End Sub

        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of Edge))
            Call MyBase.New({}, {})

            _nodeSet = New Dictionary(Of String, Node)()
            _eventListeners = New List(Of IGraphEventListener)
            _adjacencySet = New Dictionary(Of String, AdjacencySet)

            For Each node As Node In nodes
                Call AddNode(node)
            Next

            For Each edge As Edge In edges
                Call AddEdge(edge)
            Next

            For Each node As Node In vertex
                If node.adjacencies Is Nothing Then
                    _adjacencySet.Add(node.label, New AdjacencySet)
                    node.adjacencies = _adjacencySet(node.label)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Empties the Graph
        ''' </summary>
        Public Sub Clear()
            Call vertices.Clear()
            Call edges.Clear()
            Call _adjacencySet.Clear()
        End Sub

        ''' <summary>
        ''' 添加节点然后返回这个新添加的节点，如果节点不存在的话，
        ''' 则会自动更新<see cref="Node.ID"/>之后添加进入图之中
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function AddNode(node As Node) As Node
            If Not _nodeSet.ContainsKey(node.label) Then
                vertices.Add(node)
                node.ID = buffer.GetAvailablePos
                buffer += node
            End If

            _nodeSet(node.label) = node
            notify()
            Return node
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function AddEdge(i%, j%) As Edge
            Return CreateEdge(GetElementByID(i), GetElementByID(j))
        End Function

        Public Function GetElementByID(id As Integer) As Node
            Return vertex.Where(Function(n) n.ID = id).FirstOrDefault
        End Function

        Public Overloads Function AddEdge(edge As Edge) As Edge
            If Not edges.ContainsKey(edge.ID) Then
                Call edges.Add(edge.ID, edge)
            End If

            If Not (_adjacencySet.ContainsKey(edge.U.label)) Then
                _adjacencySet(edge.U.label) = New AdjacencySet With {.U = edge.U.label}
                edge.U.adjacencies = _adjacencySet(edge.U.label)
            End If

            Call _adjacencySet(edge.U.label).Add(edge)
            Call notify()

            Return edge
        End Function

        Public Sub CreateNodes(iDataList As List(Of NodeData))
            For listTrav As Integer = 0 To iDataList.Count - 1
                CreateNode(iDataList(listTrav))
            Next
        End Sub

        Public Sub CreateNodes(nameList As List(Of String))
            For listTrav As Integer = 0 To nameList.Count - 1
                CreateNode(nameList(listTrav))
            Next
        End Sub

        Public Sub CreateEdges(dataList As IEnumerable(Of (aId$, bId$, data As EdgeData)))
            Dim u, v As Node

            For Each listTrav In dataList
                If Not _nodeSet.ContainsKey(listTrav.aId) Then
                    Continue For
                ElseIf Not _nodeSet.ContainsKey(listTrav.bId) Then
                    Continue For
                Else
                    u = _nodeSet(listTrav.aId)
                    v = _nodeSet(listTrav.bId)

                    createEdgeInternal(u, v, listTrav.data)
                End If
            Next
        End Sub

        Public Sub CreateEdges(linkList As IEnumerable(Of KeyValuePair(Of String, String)))
            Dim u, v As Node

            For Each listTrav As KeyValuePair(Of String, String) In linkList
                If Not _nodeSet.ContainsKey(listTrav.Key) Then
                    Continue For
                ElseIf Not _nodeSet.ContainsKey(listTrav.Value) Then
                    Continue For
                Else
                    u = _nodeSet(listTrav.Key)
                    v = _nodeSet(listTrav.Value)

                    createEdgeInternal(u, v, Nothing)
                End If
            Next
        End Sub

        Public Function CreateNode(data As NodeData) As Node
            Dim tNewNode As New Node(_nextNodeId.ToString(), data)
            _nextNodeId += 1
            AddNode(tNewNode)
            Return tNewNode
        End Function

        ''' <summary>
        ''' 使用节点的标签创建一个新的节点对象，将这个节点对象添加进入网络模型之后将新创建的节点对象返回给用户
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 使用这个函数所构建的节点对象的<see cref="Node.ID"/>是自增的，<paramref name="label"/>则会赋值给<see cref="Node.Label"/>属性
        ''' </remarks>
        Public Function CreateNode(label As String) As Node
            Dim data As New NodeData With {.label = label}
            Dim tNewNode As New Node(label, data) With {
                .ID = _nextNodeId
            }
            _nextNodeId += 1
            AddNode(tNewNode)
            Return tNewNode
        End Function

        Private Function createEdgeInternal(u As Node, v As Node, data As EdgeData) As Edge
            Dim tNewEdge As New Edge(_nextEdgeId.ToString(), u, v, data)
            _nextEdgeId += 1
            AddEdge(tNewEdge)
            Return tNewEdge
        End Function

        ''' <summary>
        ''' 使用两个节点对象创建一条边连接之后，将所创建的边连接对象添加进入当前的图模型之中，最后将边对象返回给用户
        ''' </summary>
        ''' <param name="u"></param>
        ''' <param name="v"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Overloads Function CreateEdge(u As Node, v As Node, Optional data As EdgeData = Nothing) As Edge
            If u Is Nothing OrElse v Is Nothing Then
                Return Nothing
            Else
                Return createEdgeInternal(u, v, data)
            End If
        End Function

        ''' <summary>
        ''' 这个会自动添加新创建的边对象，因为这个函数的含义是在图之中创建一条新的边连接
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="target"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Overloads Function CreateEdge(source As String, target As String, Optional data As EdgeData = Nothing) As Edge
            If Not _nodeSet.ContainsKey(source) Then
                Return Nothing
            End If
            If Not _nodeSet.ContainsKey(target) Then
                Return Nothing
            End If

            Dim u As Node = _nodeSet(source)
            Dim v As Node = _nodeSet(target)

            Return createEdgeInternal(u, v, data)
        End Function

        Public Function GetEdges(u As Node, v As Node) As IEnumerable(Of Edge)
            If u Is Nothing OrElse v Is Nothing Then
                Return Nothing
            ElseIf Not _adjacencySet.ContainsKey(u.label) Then
                Return Nothing
            Else
                Return _adjacencySet(u.label).EnumerateAllEdges(v)
            End If
        End Function

        Public Function GetEdges(iNode As Node) As IEnumerable(Of Edge)
            If Not _adjacencySet.ContainsKey(iNode.label) Then
                Return {}
            Else
                Return _adjacencySet(iNode.label).EnumerateAllEdges
            End If
        End Function

        Public Sub RemoveNode(node As Node)
            If _nodeSet.ContainsKey(node.label) Then
                _nodeSet.Remove(node.label)
            End If

            Call vertices.Remove(node)
            Call DetachNode(node)
        End Sub

        ''' <summary>
        ''' 将目标节点相关联的边从图中删除
        ''' </summary>
        ''' <param name="iNode"></param>
        Public Sub DetachNode(iNode As Node)
            Call graphEdges _
                .ToArray _
                .DoEach(Sub(e As Edge)
                            If e.U.label = iNode.label OrElse e.V.label = iNode.label Then
                                Call RemoveEdge(e)
                            End If
                        End Sub)
            notify()
        End Sub

        ''' <summary>
        ''' Delete a graph edge connection from current network graph model
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub RemoveEdge(edge As Edge)
            Dim u_adjacencySet As AdjacencySet = _adjacencySet(edge.U.label)

            Call edges.Remove(edge.ID)
            Call u_adjacencySet.Remove(edge.V)

            If u_adjacencySet.Count = 0 Then
                Call _adjacencySet.Remove(edge.U.label)
            End If

            Call notify()
        End Sub

        ''' <summary>
        ''' 根据node节点的label来查找
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetNode(label As String) As Node
            Return vertex _
                .Where(Function(n) n.label = label OrElse n.data.label = label) _
                .FirstOrDefault
        End Function

        ''' <summary>
        ''' Find edge by label data
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        Public Function GetEdge(label As String) As Edge
            Dim retEdge As Edge = graphEdges.FirstOrDefault(Function(e) e.data.label = label)
            Return retEdge
        End Function

        Public Sub Merge(another As NetworkGraph)
            Dim mergeNode As Node
            Dim fromNode, toNode As Node

            For Each n As Node In another.vertex
                mergeNode = New Node(_nextNodeId.ToString(), n.data)
                AddNode(mergeNode)
                _nextNodeId += 1
                mergeNode.data.origID = n.label
            Next

            For Each e As Edge In another.graphEdges
                fromNode = vertex.FirstOrDefault(Function(n) e.U.label = n.data.origID)
                toNode = vertex.FirstOrDefault(Function(n) e.V.label = n.data.origID)

                Call AddEdge(New Edge(_nextEdgeId.ToString(), fromNode, toNode, e.data))

                _nextEdgeId += 1
            Next
        End Sub

        Public Sub FilterNodes(match As Predicate(Of Node))
            For Each n As Node In vertex
                If Not match(n) Then
                    RemoveNode(n)
                End If
            Next
        End Sub

        Public Sub FilterEdges(match As Predicate(Of Edge))
            For Each e As Edge In graphEdges
                If Not match(e) Then
                    RemoveEdge(e)
                End If
            Next
        End Sub

        Public Sub AddGraphListener(iListener As IGraphEventListener)
            _eventListeners.Add(iListener)
        End Sub

        Private Sub notify(<CallerMemberName> Optional event$ = Nothing)
            For Each listener As IGraphEventListener In _eventListeners
                Call listener.GraphChanged(Me, [event])
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"Network graph have {vertices.Count} nodes and {graphEdges.Count} edges."
        End Function

        ''' <summary>
        ''' 应用于网络之中的节点对象的克隆
        ''' </summary>
        ''' <param name="vertices"></param>
        ''' <param name="U"></param>
        ''' <returns></returns>
        Private Shared Function ComputeIfNotExists(vertices As Dictionary(Of Node), U As Node) As Node
            If Not vertices.Have(U) Then
                U = New Node With {
                    .data = New NodeData(U.data),
                    .degree = U.degree,
                    .ID = U.ID,
                    .label = U.label,
                    .pinned = U.pinned
                }
                vertices.Add(U)
            End If

            Return vertices(DirectCast(U, INamedValue).Key)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' graphEdges和edges这两个元素集合应该都是等长的
        ''' 在这个函数之中会将节点以及边连接的值都进行复制
        ''' 因为克隆之后的操作可能会涉及对边或者节点对象的修改操作
        ''' </remarks>
        Private Function Clone() As Object Implements ICloneable.Clone
            Dim vertices As New Dictionary(Of Node)
            Dim edges As New List(Of Edge)

            For Each edge As Edge In graphEdges
                Dim U = ComputeIfNotExists(vertices, edge.U)
                Dim V = ComputeIfNotExists(vertices, edge.V)

                edges += New Edge With {
                    .data = New EdgeData(edge.data),
                    .U = U,
                    .V = V,
                    .ID = edge.ID,
                    .isDirected = edge.isDirected,
                    .weight = edge.weight
                }
            Next

            ' 可能存在有孤立的节点
            ' 这个也需要添加加进来
            For Each node As Node In Me.vertex
                Call ComputeIfNotExists(vertices, node)
            Next

            Dim copy As New NetworkGraph(vertices.Values, edges)

            For Each node In copy.vertex

            Next

            Return copy
        End Function

        ''' <summary>
        ''' Perform a network graph model deep clone
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 经过克隆之后，节点和边对象已经完全切断了和之前的副本的所有引用关联
        ''' </remarks>
        Public Function Copy() As NetworkGraph
            Return DirectCast(Clone(), NetworkGraph)
        End Function
    End Class
End Namespace
