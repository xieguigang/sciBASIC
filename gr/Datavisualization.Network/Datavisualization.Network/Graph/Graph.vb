#Region "Microsoft.VisualBasic::0aa9da80a0746d2a39f9465f5587167d, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\LDM\Graph\Graph.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Interfaces
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Graph

    Public Class NetworkGraph : Inherits ClassObject
        Implements IGraph

        Public Sub New()
            m_nodeSet = New Dictionary(Of String, Node)()
            nodes = New List(Of Node)()
            edges = New List(Of Edge)()
            m_eventListeners = New List(Of IGraphEventListener)()
            m_adjacencySet = New Dictionary(Of String, Dictionary(Of String, List(Of Edge)))()
        End Sub

        Public Sub Clear() Implements IGraph.Clear
            nodes.Clear()
            edges.Clear()
            m_adjacencySet.Clear()
        End Sub

        Public Function AddNode(iNode As Node) As Node Implements IGraph.AddNode
            If Not m_nodeSet.ContainsKey(iNode.ID) Then
                nodes.Add(iNode)
            End If

            m_nodeSet(iNode.ID) = iNode
            notify()
            Return iNode
        End Function

        Public Function AddEdge(iEdge As Edge) As Edge Implements IGraph.AddEdge
            If Not edges.Contains(iEdge) Then
                edges.Add(iEdge)
            End If


            If Not (m_adjacencySet.ContainsKey(iEdge.Source.ID)) Then
                m_adjacencySet(iEdge.Source.ID) = New Dictionary(Of String, List(Of Edge))()
            End If
            If Not (m_adjacencySet(iEdge.Source.ID).ContainsKey(iEdge.Target.ID)) Then
                m_adjacencySet(iEdge.Source.ID)(iEdge.Target.ID) = New List(Of Edge)()
            End If


            If Not m_adjacencySet(iEdge.Source.ID)(iEdge.Target.ID).Contains(iEdge) Then
                m_adjacencySet(iEdge.Source.ID)(iEdge.Target.ID).Add(iEdge)
            End If

            notify()
            Return iEdge
        End Function

        Public Sub CreateNodes(iDataList As List(Of NodeData)) Implements IGraph.CreateNodes
            For listTrav As Integer = 0 To iDataList.Count - 1
                CreateNode(iDataList(listTrav))
            Next
        End Sub

        Public Sub CreateNodes(iNameList As List(Of String)) Implements IGraph.CreateNodes
            For listTrav As Integer = 0 To iNameList.Count - 1
                CreateNode(iNameList(listTrav))
            Next
        End Sub

        Public Sub CreateEdges(iDataList As List(Of Triple(Of String, String, EdgeData))) Implements IGraph.CreateEdges
            For listTrav As Integer = 0 To iDataList.Count - 1
                If Not m_nodeSet.ContainsKey(iDataList(listTrav).first) Then
                    Return
                End If
                If Not m_nodeSet.ContainsKey(iDataList(listTrav).second) Then
                    Return
                End If
                Dim node1 As Node = m_nodeSet(iDataList(listTrav).first)
                Dim node2 As Node = m_nodeSet(iDataList(listTrav).second)
                CreateEdge(node1, node2, iDataList(listTrav).third)
            Next
        End Sub

        Public Sub CreateEdges(iDataList As List(Of KeyValuePair(Of String, String))) Implements IGraph.CreateEdges
            For listTrav As Integer = 0 To iDataList.Count - 1
                If Not m_nodeSet.ContainsKey(iDataList(listTrav).Key) Then
                    Return
                End If
                If Not m_nodeSet.ContainsKey(iDataList(listTrav).Value) Then
                    Return
                End If
                Dim node1 As Node = m_nodeSet(iDataList(listTrav).Key)
                Dim node2 As Node = m_nodeSet(iDataList(listTrav).Value)
                CreateEdge(node1, node2)
            Next
        End Sub

        Public Function CreateNode(data As NodeData) As Node Implements IGraph.CreateNode
            Dim tNewNode As New Node(m_nextNodeId.ToString(), data)
            m_nextNodeId += 1
            AddNode(tNewNode)
            Return tNewNode
        End Function

        Public Function CreateNode(label As String) As Node Implements IGraph.CreateNode
            Dim data As New NodeData()
            data.label = label
            Dim tNewNode As New Node(m_nextNodeId.ToString(), data)
            m_nextNodeId += 1
            AddNode(tNewNode)
            Return tNewNode
        End Function

        Public Function CreateEdge(iSource As Node, iTarget As Node, Optional iData As EdgeData = Nothing) As Edge Implements IGraph.CreateEdge
            If iSource Is Nothing OrElse iTarget Is Nothing Then
                Return Nothing
            End If

            Dim tNewEdge As New Edge(m_nextEdgeId.ToString(), iSource, iTarget, iData)
            m_nextEdgeId += 1
            AddEdge(tNewEdge)
            Return tNewEdge
        End Function

        Public Function CreateEdge(iSource As String, iTarget As String, Optional iData As EdgeData = Nothing) As Edge Implements IGraph.CreateEdge
            If Not m_nodeSet.ContainsKey(iSource) Then
                Return Nothing
            End If
            If Not m_nodeSet.ContainsKey(iTarget) Then
                Return Nothing
            End If
            Dim node1 As Node = m_nodeSet(iSource)
            Dim node2 As Node = m_nodeSet(iTarget)
            Return CreateEdge(node1, node2, iData)
        End Function


        Public Function GetEdges(iNode1 As Node, iNode2 As Node) As List(Of Edge) Implements IGraph.GetEdges
            If m_adjacencySet.ContainsKey(iNode1.ID) AndAlso m_adjacencySet(iNode1.ID).ContainsKey(iNode2.ID) Then
                Return m_adjacencySet(iNode1.ID)(iNode2.ID)
            End If
            Return Nothing
        End Function

        Public Function GetEdges(iNode As Node) As List(Of Edge)
            Dim retEdgeList As New List(Of Edge)()
            If m_adjacencySet.ContainsKey(iNode.ID) Then
                For Each keyPair As KeyValuePair(Of String, List(Of Edge)) In m_adjacencySet(iNode.ID)
                    For Each e As Edge In keyPair.Value
                        retEdgeList.Add(e)
                    Next
                Next
            End If

            For Each keyValuePair As KeyValuePair(Of String, Dictionary(Of String, List(Of Edge))) In m_adjacencySet
                If keyValuePair.Key <> iNode.ID Then
                    For Each keyPair As KeyValuePair(Of String, List(Of Edge)) In m_adjacencySet(keyValuePair.Key)
                        For Each e As Edge In keyPair.Value
                            retEdgeList.Add(e)
                        Next

                    Next
                End If
            Next
            Return retEdgeList
        End Function

        Public Sub RemoveNode(iNode As Node) Implements IGraph.RemoveNode
            If m_nodeSet.ContainsKey(iNode.ID) Then
                m_nodeSet.Remove(iNode.ID)
            End If
            nodes.Remove(iNode)
            DetachNode(iNode)
        End Sub

        Public Sub DetachNode(iNode As Node) Implements IGraph.DetachNode
            edges.ForEach(Sub(e As Edge)
                              If e.Source.ID = iNode.ID OrElse e.Target.ID = iNode.ID Then
                                  Call RemoveEdge(e)
                              End If
                          End Sub)
            notify()
        End Sub

        Public Sub RemoveEdge(iEdge As Edge) Implements IGraph.RemoveEdge
            edges.Remove(iEdge)

            For Each x As KeyValuePair(Of String, Dictionary(Of String, List(Of Edge))) In m_adjacencySet
                For Each y As KeyValuePair(Of String, List(Of Edge)) In x.Value
                    Dim tEdges As List(Of Edge) = y.Value
                    tEdges.Remove(iEdge)
                    If tEdges.Count = 0 Then
                        m_adjacencySet(x.Key).Remove(y.Key)
                        Exit For
                    End If
                Next
                If x.Value.Count = 0 Then
                    m_adjacencySet.Remove(x.Key)
                    Exit For
                End If
            Next

            Call notify()
        End Sub

        ''' <summary>
        ''' 根据node节点的label来查找
        ''' </summary>
        ''' <param name="label"></param>
        ''' <returns></returns>
        Public Function GetNode(label As String) As Node
            Dim retNode As Node = Nothing
            nodes.ForEach(Sub(n As Node)
                              If n.Data.label = label Then
                                  retNode = n
                              End If
                          End Sub)
            Return retNode
        End Function

        Public Function GetEdge(label As String) As Edge
            Dim retEdge As Edge = Nothing
            edges.ForEach(Sub(e As Edge)
                              If e.Data.label = label Then

                                  retEdge = e
                              End If
                          End Sub)
            Return retEdge
        End Function

        Public Sub Merge(iMergeGraph As NetworkGraph) Implements IGraph.Merge
            For Each n As Node In iMergeGraph.nodes
                Dim mergeNode As New Node(m_nextNodeId.ToString(), n.Data)
                AddNode(mergeNode)
                m_nextNodeId += 1
                mergeNode.Data.origID = n.ID
            Next

            For Each e As Edge In iMergeGraph.edges
                Dim fromNode As Node = nodes.Find(Function(n) e.Source.ID = n.Data.origID)
                Dim toNode As Node = nodes.Find(Function(n) e.Target.ID = n.Data.origID)

                Dim tNewEdge As Edge = AddEdge(New Edge(m_nextEdgeId.ToString(), fromNode, toNode, e.Data))
                m_nextEdgeId += 1
            Next
        End Sub

        Public Sub FilterNodes(match As Predicate(Of Node)) Implements IGraph.FilterNodes
            For Each n As Node In nodes
                If Not match(n) Then
                    RemoveNode(n)
                End If
            Next
        End Sub

        Public Sub FilterEdges(match As Predicate(Of Edge)) Implements IGraph.FilterEdges
            For Each e As Edge In edges
                If Not match(e) Then
                    RemoveEdge(e)
                End If
            Next
        End Sub

        Public Sub AddGraphListener(iListener As IGraphEventListener) Implements IGraph.AddGraphListener
            m_eventListeners.Add(iListener)
        End Sub

        Private Sub notify()
            For Each listener As IGraphEventListener In m_eventListeners
                listener.GraphChanged()
            Next
        End Sub

#Region "Network data source"

        ''' <summary>
        ''' 这个属性与<see cref="connectedNodes()"/>属性之间的区别就是这个属性之中还包含着孤立的没有任何连接的节点
        ''' </summary>
        ''' <returns></returns>
        Public Property nodes() As List(Of Node) Implements IGraph.nodes
        Public Property edges() As List(Of Edge) Implements IGraph.edges
#End Region

        ''' <summary>
        ''' Returns the set of all Nodes that have emanating Edges.
        ''' This therefore returns all Nodes that will be visible in the drawing.
        ''' (这个属性之中是没有任何孤立的节点的)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property connectedNodes() As Node()
            Get
                Return edges _
                    .Select(Function(d) {d.Source, d.Target}) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray
            End Get
        End Property

        ''' <summary>
        ''' <see cref="Node.ID"/>为键名
        ''' </summary>
        Private m_nodeSet As Dictionary(Of String, Node)
        Private m_adjacencySet As Dictionary(Of String, Dictionary(Of String, List(Of Edge)))

        Private m_nextNodeId As Integer = 0
        Private m_nextEdgeId As Integer = 0
        Private m_eventListeners As List(Of IGraphEventListener)
    End Class
End Namespace
