#Region "Microsoft.VisualBasic::4bd01c13ffa42d748e38e899adfcaca0, ..\visualbasic_App\Datavisualization\Datavisualization.Network\Datavisualization.Network\FindPath\PathFinder.vb"

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

#Const DEBUG = 1

''' <summary>
''' 这个似乎是有方向的
''' </summary>
''' <typeparam name="NetworkNode"></typeparam>
Public Class PathFinder(Of NetworkNode As FileStream.NetworkEdge)

    Dim Network As NetworkNode()

    Sub New(Network As NetworkNode())
        Me.Network = Network
    End Sub

    Public Function FindAllPath(NodeA As String, NodeB As String) As KeyValuePair(Of Integer, NetworkNode())()
        Dim StartNodes = ListNodes(NodeA, Network)      '从NodeA出发
        Dim ChunkBuffer = ExistsNode(NodeA, NodeB, StartNodes)
        If Not ChunkBuffer.IsNullOrEmpty Then
            Return New KeyValuePair(Of Integer, NetworkNode())() {New KeyValuePair(Of Integer, NetworkNode())(0, ChunkBuffer)}
        End If

        Dim LQuery = (From NodeItem In StartNodes
                      Let PathResult = MoveNextStep(NodeItem, ShadowCopy(Network), ends:=NodeB, starts:=NodeItem.GetConnectedNode(NodeA)).ToList
                      Let Path = AssemblePath(PathResult, NodeItem)
                      Let value = New With {.Length = Path.Count, .Path = Path}
                      Select value
                      Order By value.Length Ascending).ToArray

        Return (From item In LQuery Select New KeyValuePair(Of Integer, NetworkNode())(item.Length, item.Path)).ToArray
    End Function

    Public Function FindShortestPath(NodeA As String, NodeB As String) As KeyValuePair(Of Integer, NetworkNode())()
        Dim Result = FindAllPath(NodeA, NodeB)

        If Result.IsNullOrEmpty Then
            Return New KeyValuePair(Of Integer, NetworkNode())() {}
        Else
            Dim ShortestPathLength = (From item In Result Select item.Key Distinct Order By Key Ascending).First
            Return (From item In Result Where item.Key = ShortestPathLength Select item).ToArray  '获取最短的路径：进过Linq查询排序后第一个是最短的路径，可能会有多条最短的路径
        End If
    End Function

    Private Shared Function AssemblePath(PathResult As Generic.IEnumerable(Of NetworkNode), StartNode As NetworkNode) As NetworkNode()
        If PathResult.IsNullOrEmpty Then '找不到路径
            Return New NetworkNode() {}
        End If
        Dim ChunkList = PathResult.ToList
        Call ChunkList.Add(StartNode)
        Return ChunkList.ToArray
    End Function

    Private Shared Function ShadowCopy(Network As Generic.IEnumerable(Of NetworkNode)) As List(Of NetworkNode)
        Dim LQuery = (From Node In Network Select Node).ToList
        Return LQuery
    End Function

    ''' <summary>
    ''' 进行递归查询
    ''' </summary>
    ''' <param name="Node"></param>
    ''' <param name="ends"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function MoveNextStep(Node As NetworkNode, Network As List(Of NetworkNode), starts As String, ends As String) As NetworkNode()
        Call Network.Remove(Node)

        Dim StartNodes = ListNodes(starts, Network)      '从NodeA出发

        Dim ChunkBuffer = ExistsNode(starts, ends, StartNodes)
        If Not ChunkBuffer.IsNullOrEmpty Then
            Return ChunkBuffer
        End If

        Dim LQuery = (From NodeItem In StartNodes
                      Let PathResult = MoveNextStep(NodeItem, ShadowCopy(Network), ends:=ends, starts:=NodeItem.GetConnectedNode(starts))
                      Let Path = AssemblePath(PathResult, NodeItem)
                      Let value = New With {.Length = Path.Count, .Path = Path}
                      Select value
                      Order By value.Length Ascending).ToArray '获取最短的路径

        If LQuery.IsNullOrEmpty Then
            Return New NetworkNode() {}
        Else
            Return LQuery.First.Path '获取最短的路径：进过Linq查询排序后第一个是最短的路径
        End If
    End Function

    Public Shared Function ExistsNode(NodeA As String, NodeB As String, Network As Generic.IEnumerable(Of NetworkNode)) As NetworkNode()
        Dim LQuery = (From Node In Network.AsParallel
                      Where (String.Equals(NodeA, Node.FromNode) AndAlso String.Equals(Node.ToNode, NodeB)) OrElse
                      (String.Equals(NodeB, Node.FromNode) AndAlso String.Equals(Node.ToNode, NodeA)) Select Node).ToArray
        Return LQuery
    End Function

    Public Shared Function ListNodes(NodeId As String, Network As Generic.IEnumerable(Of NetworkNode)) As NetworkNode()
        Dim LQuery = (From Node As NetworkNode In Network.AsParallel Where String.Equals(NodeId, Node.FromNode) OrElse String.Equals(Node.ToNode, NodeId) Select Node).ToArray
        Return LQuery
    End Function
End Class
