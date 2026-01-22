#Region "Microsoft.VisualBasic::4f8efa488285c26467bb3375a49b23f3, gr\network-visualization\Datavisualization.Network\Graph\Model\Graph.vb"

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

    '   Total Lines: 667
    '    Code Lines: 378 (56.67%)
    ' Comment Lines: 205 (30.73%)
    '    - Xml Docs: 71.71%
    ' 
    '   Blank Lines: 84 (12.59%)
    '     File Size: 25.77 KB


    '     Class NetworkGraph
    ' 
    '         Properties: connectedNodes, pinnedNodes
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: (+4 Overloads) AddEdge, AddNode, AddVertex, (+2 Overloads) Clone, Copy
    '                   (+2 Overloads) CreateEdge, createEdgeInternal, (+2 Overloads) CreateNode, GetConnectedGraph, GetConnectedVertex
    '                   GetEdge, (+2 Overloads) GetEdges, (+2 Overloads) GetElementByID, GetElementsByClassName, GetElementsByName
    '                   StyleSelectorGetElementById, ToString, Union
    ' 
    '         Sub: Clear, (+2 Overloads) CreateEdges, (+2 Overloads) CreateNodes, DetachNode, FilterEdges
    '              FilterNodes, Merge, RemoveEdge, (+2 Overloads) RemoveNode, RemovesIsolatedNodes
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
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Graph

    ''' <summary>
    ''' The network graph object model
    ''' </summary>
    Public Class NetworkGraph : Inherits NetworkGraph(Of Node, Edge)
        Implements ICloneable
        Implements IStyleSelector(Of Node)

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
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return graphEdges _
                    .Select(Function(d) d.Iterate2Nodes) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray
            End Get
        End Property

        <IgnoreDataMember>
        Public ReadOnly Property pinnedNodes As Node()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return vertex _
                    .Where(Function(v) v.pinned) _
                    .ToArray
            End Get
        End Property

        Dim _nextNodeId As Integer = 0
        Dim _nextEdgeId As Integer = 0
        Dim _index As GraphIndex(Of Node, Edge)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New()
            Call Me.New({}, {})
        End Sub

        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of Edge), Optional ignoresBrokenLinks As Boolean = False)
            Call MyBase.New({}, {})

            _index = New GraphIndex(Of Node, Edge)

            For Each node As Node In nodes
                Call AddNode(node)
            Next

            For Each edge As Edge In edges
                If ignoresBrokenLinks AndAlso edge.U Is Nothing OrElse edge.V Is Nothing Then
                    Call $"[{edge}] link is broken!".Warning
                Else
                    Call AddEdge(edge)
                End If
            Next

            For Each node As Node In vertex
                If node.adjacencies Is Nothing Then
                    node.adjacencies = _index.CreateNodeAdjacencySet(node)
                End If
                If node.directedVertex Is Nothing Then
                    node.directedVertex = New DirectedVertex(node.label)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Empties the Graph
        ''' </summary>
        Public Sub Clear()
            Call vertices.Clear()
            Call clearEdges()
            Call _index.Clear()
        End Sub

        ''' <summary>
        ''' 添加节点然后返回这个新添加的节点，如果节点不存在的话，
        ''' 则会自动更新<see cref="Node.ID"/>之后添加进入图之中
        ''' </summary>
        ''' <param name="node"></param>
        ''' <param name="assignId">
        ''' make update and assign the <see cref="Node.ID"/> hashcode value?
        ''' </param>
        ''' <returns></returns>
        Public Function AddNode(node As Node, Optional assignId As Boolean = True) As Node
            If Not vertices.ContainsKey(node.label) Then
                If assignId Then
                    ' 20201223 ID必须要在哈希表添加之前进行赋值
                    ' 编号必须从零开始
                    If buffer.Count = 0 Then
                        node.ID = 1
                    Else
                        ' the buffer dictionary key is the node ID collection
                        node.ID = buffer.Keys.Max + 1
                    End If
                End If

                buffer.Add(node.ID, node)
                vertices.Add(node)
            End If

            _index(node.label) = node
            _index(node.label).directedVertex = New DirectedVertex(node.label)
            _index(node.label).adjacencies = _index.CreateNodeAdjacencySet(node)

            Return node
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function AddEdge(i%, j%) As Edge
            Return CreateEdge(GetElementByID(i), GetElementByID(j))
        End Function

        Public Function GetElementByID(id As Integer) As Node
            Return vertex.Where(Function(n) n.ID = id).FirstOrDefault
        End Function

        ''' <summary>
        ''' 根据node节点的label来查找
        ''' </summary>
        ''' <param name="labelID"><see cref="Node.label"/></param>
        ''' <param name="dataLabel"></param>
        ''' <returns>
        ''' 查找失败会返回空值
        ''' </returns>
        Public Function GetElementByID(labelID$, Optional dataLabel As Boolean = False) As Node
            If Not dataLabel Then
                ' do not search via node data label
                ' use the unique id indexer directly
                If vertices.ContainsKey(labelID) Then
                    Return vertices(labelID)
                Else
                    Return Nothing
                End If
            ElseIf vertices.ContainsKey(labelID) Then
                Return vertices(labelID)
            Else
                ' try to search via node data label string matches as a candidate options
                Return vertex.AsParallel _
                    .Where(Function(n)
                               Return n.data.label = labelID
                           End Function) _
                    .FirstOrDefault
            End If
        End Function

#Region "css selector supports"

        Private Function StyleSelectorGetElementById(id As String) As Node Implements IStyleSelector(Of Node).GetElementById
            Return GetElementByID(id, dataLabel:=False)
        End Function

        ''' <summary>
        ''' get node of given node type
        ''' </summary>
        ''' <param name="classname">the node type</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function check of the property with name <see cref="NamesOf.REFLECTION_ID_MAPPING_NODETYPE"/>
        ''' </remarks>
        Public Iterator Function GetElementsByClassName(classname As String) As IEnumerable(Of Node) Implements IStyleSelector(Of Node).GetElementsByClassName
            For Each v As Node In vertex
                If classname = v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) Then
                    Yield v
                End If
            Next
        End Function

        Public Function GetElementsByName(name As String) As Node() Implements IStyleSelector(Of Node).GetElementsByName
            Return vertex _
                .Where(Function(node)
                           Return name = node.data.label
                       End Function) _
                .ToArray
        End Function
#End Region

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function AddEdge(u As String, v As String, Optional weight As Double = 0) As NetworkGraph(Of Node, Edge)
            Return AddEdge(u, v, weight, Nothing)
        End Function

        Public Overloads Function AddEdge(u As String, v As String, weight As Double, <Out> ByRef getNewEdge As Edge) As NetworkGraph(Of Node, Edge)
            getNewEdge = New EdgeData With {
                .bends = {},
                .label = $"{u}->{v}"
            }.DoCall(Function(data)
                         ' 在利用这个函数创建edge的时候，
                         ' 会将创建出来的新edge添加进入当前的这个图对象之中
                         ' 所以不需要再次调用addedge方法了
                         Return CreateEdge(GetElementByID(u), GetElementByID(v), weight, data)
                     End Function)

            Return Me
        End Function

        Public Overloads Function AddEdge(edge As Edge) As Edge
            Dim tuple = _index.AddEdge(edge)

            Call Insert(edge)

            ' gr.addEdge(edge)
            ' tail.addOutgoingEdge(edge)
            ' head.addIncomingEdge(edge)

            edge.U.adjacencies = tuple.U
            edge.V.adjacencies = tuple.V
            edge.U.directedVertex.addEdge(edge)
            edge.V.directedVertex.addEdge(edge)

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
                If Not vertices.ContainsKey(listTrav.aId) Then
                    Continue For
                ElseIf Not vertices.ContainsKey(listTrav.bId) Then
                    Continue For
                Else
                    u = _index(listTrav.aId)
                    v = _index(listTrav.bId)

                    createEdgeInternal(u, v, listTrav.data, 0)
                End If
            Next
        End Sub

        Public Sub CreateEdges(linkList As IEnumerable(Of KeyValuePair(Of String, String)))
            Dim u, v As Node

            For Each listTrav As KeyValuePair(Of String, String) In linkList
                If Not vertices.ContainsKey(listTrav.Key) Then
                    Continue For
                ElseIf Not vertices.ContainsKey(listTrav.Value) Then
                    Continue For
                Else
                    u = _index(listTrav.Key)
                    v = _index(listTrav.Value)

                    createEdgeInternal(u, v, Nothing, 0)
                End If
            Next
        End Sub

        Public Function CreateNode(data As NodeData) As Node
            Dim tNewNode As New Node(_nextNodeId.ToString(), data) With {.ID = _nextNodeId}
            _nextNodeId += 1
            AddNode(tNewNode, assignId:=False)
            Return tNewNode
        End Function

        Public Overrides Function AddVertex(label As String) As Node
            Return CreateNode(label)
        End Function

        ''' <summary>
        ''' 使用节点的标签创建一个新的节点对象，将这个节点对象添加进入网络模型之后将新创建的节点对象返回给用户
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 使用这个函数所构建的节点对象的<see cref="Node.ID"/>是自增的，<paramref name="label"/>则会赋值给<see cref="Node.Label"/>属性
        ''' </remarks>
        Public Function CreateNode(label As String, Optional data As NodeData = Nothing) As Node
            If data Is Nothing Then
                data = New NodeData With {.label = label}
            End If

            Dim tNewNode As New Node(label, data) With {
                .ID = _nextNodeId
            }
            _nextNodeId += 1

            ' the id of the new node already been assigned
            ' via the nextNodeId, no needs for assign it again
            Call AddNode(tNewNode, assignId:=False)

            Return tNewNode
        End Function

        Private Function createEdgeInternal(u As Node, v As Node, data As EdgeData, weight#) As Edge
            Dim tNewEdge As New Edge(_nextEdgeId.ToString(), u, v, data) With {
                .weight = weight
            }
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
        Public Overloads Function CreateEdge(u As Node, v As Node, Optional weight# = 0, Optional data As EdgeData = Nothing) As Edge
            If u Is Nothing OrElse v Is Nothing Then
                Return Nothing
            Else
                Return createEdgeInternal(u, v, data, weight)
            End If
        End Function

        ''' <summary>
        ''' 这个会自动添加新创建的边对象，因为这个函数的含义是在图之中创建一条新的边连接
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="target"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Public Overloads Function CreateEdge(source As String, target As String, Optional weight# = 0, Optional data As EdgeData = Nothing) As Edge
            If Not vertices.ContainsKey(source) Then
                Return Nothing
            End If
            If Not vertices.ContainsKey(target) Then
                Return Nothing
            End If

            Dim u As Node = _index(source)
            Dim v As Node = _index(target)

            If data Is Nothing Then
                data = New EdgeData
            End If

            Return createEdgeInternal(u, v, data, weight)
        End Function

        Public Overloads Function GetConnectedVertex(label As String) As Node()
            Dim node As Node = GetElementByID(label)
            Dim edges As Edge() = GetEdges(node).ToArray
            Dim connectedNodes As Node() = edges _
                .Select(Function(e) {e.U, e.V}) _
                .IteratesALL _
                .Where(Function(n)
                           Return Not n Is node
                       End Function) _
                .ToArray

            Return connectedNodes
        End Function

        ''' <summary>
        ''' 获取目标两个节点之间的所有的重复的边连接
        ''' </summary>
        ''' <param name="u"></param>
        ''' <param name="v"></param>
        ''' <returns>get a set of the directed edges</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetEdges(u As Node, v As Node) As IEnumerable(Of Edge)
            If u Is Nothing OrElse v Is Nothing Then
                Return {}
            Else
                Return _index.GetEdges(u, v).SafeQuery
            End If
        End Function

        ''' <summary>
        ''' 获取得到与目标节点所有相连接的节点
        ''' </summary>
        ''' <param name="iNode"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetEdges(iNode As Node) As IEnumerable(Of Edge)
            Return _index.GetEdges(iNode.label)
        End Function

        ''' <summary>
        ''' removes a target node from graph object via a given <see cref="Node.label"/>.
        ''' </summary>
        ''' <param name="labelId"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub RemoveNode(labelId As String)
            Call RemoveNode(GetElementByID(labelId))
        End Sub

        ''' <summary>
        ''' 应该使用这个方法来安全的删除节点
        ''' </summary>
        ''' <remarks>
        ''' 这个函数会移除:
        ''' 
        ''' 1. 目标节点从内部索引中删除
        ''' 2. 删除与之相关的边连接
        ''' </remarks>
        ''' <param name="node"></param>
        Public Sub RemoveNode(node As Node)
            If node Is Nothing Then
                ' node not found when call GetElementByID from
                ' removeNode(label string) function
                Return
            End If

            Call _index.Delete(node)
            Call vertices.Remove(node)
            Call buffer.Remove(CUInt(node.ID))
            Call DetachNode(node)
        End Sub

        ''' <summary>
        ''' 将目标节点相关联的边从图中删除
        ''' </summary>
        ''' <param name="iNode"></param>
        Public Sub DetachNode(iNode As Node)
            For Each e As Edge In graphEdges.ToArray
                If e.U.label = iNode.label OrElse e.V.label = iNode.label Then
                    Call RemoveEdge(e)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Delete a graph edge connection from current network graph model
        ''' </summary>
        ''' <param name="edge"></param>
        ''' <remarks>
        ''' this method just break the edge connection, the edge node 
        ''' will be keeps in the graph.
        ''' </remarks>
        Public Sub RemoveEdge(edge As Edge)
            Call _index.RemoveEdge(edge)
            Call Delete(edge)
        End Sub

        ''' <summary>
        ''' Find edge by label data
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        Public Overloads Function GetEdge(label As String) As Edge
            Dim retEdge As Edge = graphEdges.FirstOrDefault(Function(e) e.data.label = label)
            Return retEdge
        End Function

        ''' <summary>
        ''' merge another graph into current graph object
        ''' </summary>
        ''' <param name="another"></param>
        Public Sub Merge(another As NetworkGraph)
            Dim mergeNode As Node
            Dim fromNode, toNode As Node

            For Each n As Node In another.vertex
                mergeNode = New Node(_nextNodeId.ToString(), n.data) With {.ID = _nextNodeId}
                AddNode(mergeNode, assignId:=False)
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="another"></param>
        ''' <param name="assignId">
        ''' assign the new node id to the union graph vertex when
        ''' insert target node into the union graph object.
        ''' </param>
        ''' <returns></returns>
        Public Function Union(another As NetworkGraph, Optional assignId As Boolean = True) As NetworkGraph
            Dim g As New NetworkGraph

            For Each v As Node In vertex
                Call g.AddNode(v.Clone, assignId:=assignId)
            Next
            For Each v As Node In another.vertex
                If g.GetElementByID(v.label) Is Nothing Then
                    Call g.AddNode(v.Clone, assignId:=assignId)
                Else
                    ' union the node data?
                    ' just do nothing, currently
                End If
            Next

            For Each edge As Edge In graphEdges.JoinIterates(another.graphEdges)
                If Not g.GetEdges(g.GetElementByID(edge.U.label), g.GetElementByID(edge.V.label)).Any Then
                    Call g.AddEdge(edge.Clone)
                End If
            Next

            Return g
        End Function

        ''' <summary>
        ''' removes the nodes which is not matched with the given condition <paramref name="match"/>.
        ''' </summary>
        ''' <param name="match"></param>
        Public Sub FilterNodes(match As Predicate(Of Node))
            For Each n As Node In vertex
                If Not match(n) Then
                    RemoveNode(n)
                End If
            Next
        End Sub

        ''' <summary>
        ''' removes the edges which is not matched with the given condition <paramref name="match"/>.
        ''' </summary>
        ''' <param name="match"></param>
        Public Sub FilterEdges(match As Predicate(Of Edge))
            For Each e As Edge In graphEdges
                If Not match(e) Then
                    RemoveEdge(e)
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Dim communities As String() = vertex _
                .Select(Function(v) v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)) _
                .Where(Function(groupId) Not groupId.StringEmpty) _
                .Distinct _
                .ToArray

            If communities.Length = 0 Then
                Return $"Network graph have {vertices.Count} nodes and {graphEdges.Count} edges."
            Else
                Return $"Network graph [{vertices.Count} nodes, {graphEdges.Count} edges] has {communities.Length} community class ({communities.JoinBy(", ")})."
            End If
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
            Return Clone(vertex)
        End Function

        Private Function Clone(vertex As IEnumerable(Of Node)) As Object
            Dim g As New NetworkGraph

            For Each v As Node In vertex
                Dim cloneNode = g.CreateNode(v.label, v.data.Clone)

                cloneNode.degree = v.degree
                cloneNode.pinned = v.pinned
                cloneNode.visited = v.visited
            Next

            For Each edge As Edge In graphEdges
                g.CreateEdge(
                    u:=g.GetElementByID(edge.U.label),
                    v:=g.GetElementByID(edge.V.label),
                    weight:=edge.weight,
                    data:=edge.data.Clone
                )
            Next

            Return g
        End Function

        Public Function GetConnectedGraph() As NetworkGraph
            Return Clone(connectedNodes)
        End Function

        ''' <summary>
        ''' Perform a network graph model deep clone
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 1. 经过克隆之后，节点和边对象已经完全切断了和之前的副本的所有引用关联
        ''' 2. the node id may be have some gap or shift after delete some nodes,
        ''' this situation will makes the cluster analysis failure, so this graph
        ''' copy operation will fix this problem
        ''' </remarks>
        Public Function Copy() As NetworkGraph
            Return DirectCast(Clone(), NetworkGraph)
        End Function

        ''' <summary>
        ''' The degress data of each node should be computed at first, 
        ''' before calling this method for make the graph object 
        ''' cleanup.
        ''' </summary>
        Public Sub RemovesIsolatedNodes()
            For Each v As Node In vertex.ToArray
                If v.degree.In = 0 AndAlso v.degree.Out = 0 Then
                    Call RemoveNode(v)
                End If
            Next
        End Sub
    End Class
End Namespace
