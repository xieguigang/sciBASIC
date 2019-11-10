Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph.Abstract

Namespace Analysis.Model

    Public Class GraphIndex(Of Node As INamedValue, Edge As IInteraction)

        ''' <summary>
        ''' 应用于按照节点的<see cref="Graph.Node.Label"/>为键名进行节点对象的快速查找
        ''' </summary>
        Dim _nodeSet As Dictionary(Of String, Node)
        Dim _adjacencySet As Dictionary(Of String, AdjacencySet(Of Edge))

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

        Public Sub Delete(node As Node)
            If _nodeSet.ContainsKey(node.Key) Then
                Call _nodeSet.Remove(node.Key)
            End If
        End Sub

        ''' <summary>
        ''' Delete a graph edge connection from current network graph model
        ''' </summary>
        ''' <param name="edge"></param>
        Public Sub RemoveEdge(edge As Edge)
            Dim u_adjacencySet As AdjacencySet(Of Edge) = _adjacencySet(edge.source)

            Call u_adjacencySet.Remove(edge.target)

            If u_adjacencySet.Count = 0 Then
                Call _adjacencySet.Remove(edge.source)
            End If
        End Sub

        Public Sub Clear()

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
        ''' <returns></returns>
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
        ''' <param name="iNode"></param>
        ''' <returns></returns>
        Public Function GetEdges(iNode As Node) As IEnumerable(Of Edge)
            If Not _adjacencySet.ContainsKey(iNode.Key) Then
                Return {}
            Else
                Return _adjacencySet(iNode.Key).EnumerateAllEdges
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