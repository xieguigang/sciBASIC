Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Language

Public Class GraphRouter

    Dim matrix As Integer(,)
    Dim nodes As Dictionary(Of Node, Integer)
    Dim nodeSet As Node()

    Public Class Route : Inherits Dijkstra.RoutePathway

        Public Overrides ReadOnly Property Count As Integer
            Get
                Return PathwayNodes.TryCount
            End Get
        End Property

        Public Property PathwayNodes As Node()

        Public Sub New(id As String)
            MyBase.New(id)
        End Sub
    End Class

    Public Iterator Function FindPath(start As Node) As IEnumerable(Of Route)
        Dim i As Integer = nodes.TryGetValue(start, [default]:=-1)

        If i < 0 Then
            Return
        End If

        For Each route As Dijkstra.DijkstraAlgoritm.Node In Dijkstra.DijkstraAlgoritm.DistanceFinder(matrix, nodes.Count, startIndex:=i)
            Yield New Route(nodeSet(route.Index).label) With {
                .Cost = route.TotalDistance,
                .PathwayNodes = route.Path _
                    .Select(Function(idx) nodeSet(idx)) _
                    .ToArray()
            }
        Next
    End Function

    ''' <summary>
    ''' 将自定义的Graph对象转换为Dijkstra算法所需的二维整数矩阵。
    ''' </summary>
    ''' <param name="networkGraph">自定义的网络图对象</param>
    ''' <returns>邻接矩阵</returns>
    Public Shared Function ConvertToMatrix(networkGraph As NetworkGraph, Optional undirected As Boolean = False) As GraphRouter
        ' 1. 获取节点总数
        Dim nodeCount As Integer = networkGraph.vertex.Count

        If nodeCount = 0 Then
            Return New GraphRouter With {
                .matrix = New Integer(0, 0) {},
                .nodes = New Dictionary(Of Node, Integer),
                .nodeSet = {}
            }
        End If

        ' 2. 初始化矩阵，所有位置默认为 0 (表示无连接)
        Dim matrix(nodeCount - 1, nodeCount - 1) As Integer
        ' 3. 建立 Node 对象到矩阵索引的映射 (Dictionary)
        ' 这样我们可以根据 Node 对象快速找到它在数组中的位置 (0, 1, 2...)
        Dim nodeIndexMap As New Dictionary(Of Node, Integer)()
        Dim i As i32 = 0
        Dim nodeSet As New List(Of Node)

        For Each v As Node In networkGraph.vertex
            nodeIndexMap(v) = ++i
            nodeSet.Add(v)
        Next

        ' 4. 遍历所有边并填充矩阵
        For Each edge As Edge In networkGraph.graphEdges
            ' 检查边的两个端点是否都在我们的节点列表中
            If nodeIndexMap.ContainsKey(edge.U) AndAlso nodeIndexMap.ContainsKey(edge.V) Then
                Dim uIndex As Integer = nodeIndexMap(edge.U)
                Dim vIndex As Integer = nodeIndexMap(edge.V)

                ' 将权重填入矩阵
                ' 注意：这里假设图是有向图。如果是无向图（双向道路），
                If undirected Then
                    matrix(vIndex, uIndex) = edge.weight
                End If

                matrix(uIndex, vIndex) = edge.weight
            End If
        Next

        Return New GraphRouter With {
            .matrix = matrix,
            .nodes = nodeIndexMap,
            .nodeSet = nodeSet.ToArray()
        }
    End Function
End Class
