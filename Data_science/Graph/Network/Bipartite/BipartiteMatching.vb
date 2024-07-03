#Region "Microsoft.VisualBasic::960c20c5941b9310b82d05ac8ac0f8c6, Data_science\Graph\Network\Bipartite\BipartiteMatching.vb"

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

    '   Total Lines: 262
    '    Code Lines: 153 (58.40%)
    ' Comment Lines: 66 (25.19%)
    '    - Xml Docs: 84.85%
    ' 
    '   Blank Lines: 43 (16.41%)
    '     File Size: 10.43 KB


    ' Class BipartiteMatching
    ' 
    '     Properties: flow, matches
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: CreateMatches, existsAugmentingPath, fordFulkersonMaxFlow
    ' 
    '     Sub: addEdge, connectSinkToRightHalf, connectSourceToLeftHalf, depthFirstSearch
    '     Class Edge
    ' 
    '         Properties: Capacity, Flow
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getOtherEndNode, residualCapacityTo, ToString
    ' 
    '         Sub: increaseFlowTo
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Public Class BipartiteMatching

    Friend Class Edge

        ''' <summary>
        ''' an edge is composed of 2 vertices
        ''' </summary>
        Friend fromVertex As Integer
        Friend toVertex As Integer
        ''' <summary>
        ''' edges also have a capacity &amp; a flow
        ''' </summary>
        Friend m_capacity As Integer
        Friend m_flow As Integer

        Public Sub New(fromVertex As Integer, toVertex As Integer, Optional capacity As Integer = 1)
            Me.fromVertex = fromVertex
            Me.toVertex = toVertex
            Me.m_capacity = capacity
        End Sub

        ''' <summary>
        ''' Given an end-node, Returns the other end-node (completes the edge)
        ''' </summary>
        ''' <param name="vertex"></param>
        ''' <returns></returns>
        Public Overridable Function getOtherEndNode(vertex As Integer) As Integer
            If vertex = fromVertex Then
                Return toVertex
            End If
            Return fromVertex
        End Function

        Public Overridable ReadOnly Property Capacity As Integer
            Get
                Return m_capacity
            End Get
        End Property

        Public Overridable ReadOnly Property Flow As Integer
            Get
                Return m_flow
            End Get
        End Property

        Public Overridable Function residualCapacityTo(vertex As Integer) As Integer
            If vertex = fromVertex Then
                Return m_flow
            End If
            Return m_capacity - m_flow
        End Function

        Public Overridable Sub increaseFlowTo(vertex As Integer, changeInFlow As Integer)
            If vertex = fromVertex Then
                m_flow = m_flow - changeInFlow
            Else
                m_flow = m_flow + changeInFlow
            End If
        End Sub

        ''' <summary>
        ''' Prints edge using Array indexes, not human readable ID's like "S" or "T"
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return "(" & fromVertex.ToString() & " --> " & toVertex.ToString() & ")"
        End Function
    End Class

    ''' <summary>
    ''' Graph is represented as an ArrayList of Edges
    ''' </summary>
    Private graph As List(Of List(Of Edge))

    ''' <summary>
    ''' convert between array indexes (starting from 0) & human readable vertex names
    ''' </summary>
    Private getStringVertexIdFromArrayIndex As List(Of String)
    ''' <summary>
    ''' How many vertices are in the graph
    ''' </summary>
    Private vertexCount As Integer

    ''' <summary>
    ''' These fields are updated by fordFulkersonMaxFlow and when finding augmentation paths
    ''' </summary>
    Private edgeTo As Edge()
    ''' <summary>
    ''' array of all vertices, updated each time an augmentation path is found
    ''' </summary>
    Private isVertexMarked As Boolean()

    Public ReadOnly Property flow As Integer
    Public ReadOnly Property matches As (S As String, T As String)()

    ''' <summary>
    ''' Constructor initializes graph edge list with number of vertexes, string equivalents for array indexes & adds empty ArrayLists to the graph for how many vertices ther are
    ''' </summary>
    ''' <param name="vertexCount"></param>
    ''' <param name="getStringVertexIdFromArrayIndex"></param>
    Public Sub New(vertexCount As Integer, getStringVertexIdFromArrayIndex As List(Of String))
        Dim i = 0

        Me.vertexCount = vertexCount
        Me.getStringVertexIdFromArrayIndex = getStringVertexIdFromArrayIndex

        ' Populate graph with empty ArrayLists for each vertex
        graph = New List(Of List(Of Edge))(vertexCount)

        While i < vertexCount
            graph.Add(New List(Of Edge)())
            i = i + 1
        End While
    End Sub

    Public Overridable Sub addEdge(fromVertex As Integer, toVertex As Integer, Optional capacity As Integer = 1)
        Dim newEdge As Edge = New Edge(fromVertex, toVertex, capacity) 'create new edge between 2 vertices
        graph(fromVertex).Add(newEdge) 'Undirected bipartie graph, so add edge in both directions
        graph(toVertex).Add(newEdge)
    End Sub

    ''' <summary>
    ''' Adds edges from the source to all vertices in the left half
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="leftHalfVertices"></param>
    Public Overridable Sub connectSourceToLeftHalf(source As Integer, leftHalfVertices As Integer())
        For Each vertexIndex In leftHalfVertices
            ' System.out.println("addEdge(source, vertexIndex) = ("+source+", "+vertexIndex+")");
            addEdge(source, vertexIndex)
        Next
    End Sub

    ''' <summary>
    ''' Adds edges from all vertices in right half to sink
    ''' </summary>
    ''' <param name="sink"></param>
    ''' <param name="rightHalfVertices"></param>
    Public Overridable Sub connectSinkToRightHalf(sink As Integer, rightHalfVertices As Integer())
        For Each vertexIndex In rightHalfVertices
            ' System.out.println("addEdge(vertexIndex, sink) = ("+vertexIndex+", "+sink+")");
            addEdge(vertexIndex, sink)
        Next
    End Sub

    ''' <summary>
    ''' Finds max flow / min cut of a graph
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="sink"></param>
    Public Overridable Function fordFulkersonMaxFlow(source As Integer, sink As Integer) As BipartiteMatching
        Dim matches As New List(Of (String, String))

        edgeTo = New Edge(vertexCount - 1) {}
        While existsAugmentingPath(source, sink)
            Dim flowIncrease = 1 'default value is 1 since it's a bipartite matching problem with capacities = 1
            Dim S As String = getStringVertexIdFromArrayIndex(edgeTo(sink).getOtherEndNode(sink))
            ' Loop over The path from source to sink. (Update max flow & print the other matched vertex)
            Dim i = sink
            Dim T As String

            While i <> source
                'Loop stops when i reaches the source, so print out the vertex in the path that comes right before the source
                If edgeTo(i).getOtherEndNode(i) = source Then
                    T = getStringVertexIdFromArrayIndex(i)
                    matches.Add((S, T))
                End If
                flowIncrease = std.Min(flowIncrease, edgeTo(i).residualCapacityTo(i))
                i = edgeTo(i).getOtherEndNode(i)
            End While

            'Update Residual Capacities
            i = sink

            While i <> source
                edgeTo(i).increaseFlowTo(i, flowIncrease)
                i = edgeTo(i).getOtherEndNode(i)
            End While

            _flow += flowIncrease
        End While

        _matches = matches.ToArray

        Return Me
    End Function

    ''' <summary>
    ''' Calls dfs to find an augmentation path & check if it reached the sink
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="sink"></param>
    ''' <returns></returns>
    Public Overridable Function existsAugmentingPath(source As Integer, sink As Integer) As Boolean
        isVertexMarked = New Boolean(vertexCount - 1) {} 'recreate array of visited nodes each time searching for a path
        isVertexMarked(source) = True 'visit the source

        ' System.out.print("Augmenting Path : S ");
        depthFirstSearch(source, sink) 'attempts to find path from source to sink & updates isVertexMarked
        ' System.out.print("T  ");

        Return isVertexMarked(sink) 'if it reached the sink, then a path was found
    End Function

    Public Overridable Sub depthFirstSearch(v As Integer, sink As Integer)
        ' No point in finding a path if the starting vertex is already at the sink
        If v = sink Then
            Return
        End If

        For Each edge As Edge In graph(v) 'loop over all edges in the graph
            Dim otherEndNode = edge.getOtherEndNode(v)
            If Not isVertexMarked(otherEndNode) AndAlso edge.residualCapacityTo(otherEndNode) > 0 Then 'if otherEndNode is unvisited AND if the residual capacity exists at the otherEndNode
                ' System.out.print( getStringVertexIdFromArrayIndex.get(otherEndNode) +" ");
                edgeTo(otherEndNode) = edge 'update next link in edge chain
                isVertexMarked(otherEndNode) = True 'visit the node
                depthFirstSearch(otherEndNode, sink) 'recursively continue exploring
            End If
        Next
    End Sub

    Public Shared Function CreateMatches(links As IEnumerable(Of (s As String, t As String))) As BipartiteMatching
        Dim names As New Dictionary(Of String, Integer)
        Dim leftHalfVertices As New List(Of Integer)
        Dim rightHalfVertices As New List(Of Integer)
        Dim edges As New List(Of (Integer, Integer))

        For Each link As (s$, t$) In links
            If Not names.ContainsKey(link.s) Then
                Call names.Add(link.s, names.Count)
            End If
            If Not names.ContainsKey(link.t) Then
                Call names.Add(link.t, names.Count)
            End If

            Call leftHalfVertices.Add(names(link.s))
            Call rightHalfVertices.Add(names(link.t))
            Call edges.Add((leftHalfVertices.Last, rightHalfVertices.Last))
        Next

        Const sourceName As String = NameOf(leftHalfVertices)
        Const sinkName As String = NameOf(rightHalfVertices)

        Call names.Add(sourceName, names.Count)
        Call names.Add(sinkName, names.Count)

        Dim source = names(sourceName)
        Dim sink = names(sinkName)
        Dim graph1BipartiteMatcher As New BipartiteMatching(names.Count, names.Keys.AsList)

        For Each link In edges
            Call graph1BipartiteMatcher.addEdge(link.Item1, link.Item2)
        Next

        graph1BipartiteMatcher.connectSourceToLeftHalf(source, leftHalfVertices.ToArray)
        graph1BipartiteMatcher.connectSinkToRightHalf(sink, rightHalfVertices.ToArray)

        Return graph1BipartiteMatcher.fordFulkersonMaxFlow(source, sink)
    End Function
End Class
