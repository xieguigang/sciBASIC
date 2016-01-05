Namespace Dijkstra.PQDijkstra

    Public MustInherit Class PQDijkstraProvider

        Dim dijkstra As DijkstraFast

        ''' <summary>
        ''' get costs. If there is no connection, then cost is maximum.(获取)
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="finish"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected MustOverride Function getInternodeTraversalCost(start As Integer, finish As Integer) As Single
        ''' <summary>
        ''' 获取与目标节点直接相邻的所有的节点的编号
        ''' </summary>
        ''' <param name="startingNode"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected MustOverride Function GetNearbyNodes(startingNode As Integer) As IEnumerable(Of Integer)

        ''' <summary>
        ''' 网络之中的节点的总数目
        ''' </summary>
        ''' <param name="totalNodes"></param>
        ''' <remarks></remarks>
        Sub New(totalNodes As Integer)
            dijkstra = New DijkstraFast(totalNodes, New DijkstraFast.InternodeTraversalCost(AddressOf getInternodeTraversalCost), New DijkstraFast.NearbyNodesHint(AddressOf GetNearbyNodes))
        End Sub

        Public Function Compute(start As Integer, ends As Integer) As Integer()
            Dim minPath As Integer() = dijkstra.GetMinimumPath(start, ends)
            Return minPath
        End Function
    End Class

    Public Class Example : Inherits PQDijkstraProvider

        Private totalNodes As Integer = 100000
        Private cost As Single(,)
        Private dijkstra As DijkstraFast
        Private rv As Random

        Sub New(totalNodes As Integer)
            Call MyBase.New(totalNodes)

            cost = New Single(totalNodes - 1, 3) {}
            ' initialize the cost matrix
            rv = New Random()
            For i As Integer = 0 To totalNodes - 1
                cost(i, 0) = CSng(rv.[Next](1000)) * 0.01F
                cost(i, 1) = CSng(rv.[Next](1000)) * 0.01F
                cost(i, 2) = CSng(rv.[Next](1000)) * 0.01F
                cost(i, 3) = CSng(rv.[Next](1000)) * 0.01F
            Next
        End Sub

        ' a function to get relative position from one node to another
        Private Function GetRelativePosition(start As Integer, finish As Integer) As Integer
            If start - 1 = finish Then
                Return 0

            ElseIf start + 1 = finish Then
                Return 1

            ElseIf start + 5 = finish Then
                Return 2

            ElseIf start - 5 = finish Then
                Return 3
            End If

            Return -1
        End Function

        Protected Overrides Function getInternodeTraversalCost(start As Integer, finish As Integer) As Single
            Dim relativePosition As Integer = GetRelativePosition(start, finish)
            If relativePosition < 0 Then
                Return Single.MaxValue
            End If
            Return cost(start, relativePosition)
        End Function

        Protected Overrides Function GetNearbyNodes(startingNode As Integer) As IEnumerable(Of Integer)
            Dim nearbyNodes As New List(Of Integer)(4)

            If startingNode >= totalNodes - 5 Then
                startingNode = -1
            End If
            If startingNode <= 5 Then
                startingNode = -1
            End If

            ' in the order as defined in GetRelativePosition
            nearbyNodes.Add(startingNode - 1)
            nearbyNodes.Add(startingNode + 1)
            nearbyNodes.Add(startingNode + 5)
            nearbyNodes.Add(startingNode - 5)
            Return nearbyNodes
        End Function
    End Class
End Namespace