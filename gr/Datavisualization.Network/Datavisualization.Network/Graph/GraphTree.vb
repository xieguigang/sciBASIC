Namespace Graph

    ''' <summary>
    ''' 这是一个节点对象
    ''' </summary>
    Public Class GraphTreeNode

        ''' <summary>
        ''' 从外部链接到当前节点的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Parents As List(Of GraphTreeNode)
        ''' <summary>
        ''' 从当前的节点出发，链接到的其他的节点列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Childs As List(Of GraphTreeNode)
        Public Property Node As Node

        Public Overrides Function ToString() As String
            Return Node.ToString
        End Function

    End Class

    Public Class GraphTree

        ''' <summary>
        ''' 一个大网络可能会是由几个小的独立的网络模块所构成，也可能由独立的节点构成
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Trees As GraphTreeNode()

        Sub New(graph As NetworkGraph)
            Trees = IterateTrees(graph)
        End Sub

        Private Shared Function IterateTrees(graph As NetworkGraph) As GraphTreeNode()
            Dim trees As New List(Of GraphTreeNode)
            Dim travels As New Dictionary(Of Node, GraphTreeNode)

            For Each node As Node In graph.nodes
                Dim root As New GraphTreeNode With {
                    .Node = graph _
                        .nodes _
                        .FirstOrDefault
                }
                IterateTrees(root, graph, travels)
                trees.Add(root)
            Next

            Return trees.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="current"></param>
        ''' <param name="graph"></param>
        ''' <param name="treeTable">如果下一个节点在这个列表之中，则不会再进行递归操作了</param>
        Private Shared Sub IterateTrees(current As GraphTreeNode, graph As NetworkGraph, ByRef treeTable As Dictionary(Of Node, GraphTreeNode))
            If current Is Nothing Then
                Return
            ElseIf treeTable.ContainsKey(current.Node) Then
                Return
            Else
                Call treeTable.Add(current.Node, current)
            End If

            For Each edge As Edge In graph.edges
                Dim [next] As GraphTreeNode = Nothing

                With current
                    If edge.Source = .Node Then
                        [next] = New GraphTreeNode With {
                            .Node = edge.Target
                        }
                        [next].Parents.Add(current)
                        .Childs.Add([next])
                    ElseIf edge.Target = .Node Then
                        [next] = New GraphTreeNode With {
                            .Node = edge.Source
                        }
                        [next].Childs.Add(current)
                        .Parents.Add([next])
                    End If
                End With

                If Not [next] Is Nothing Then
                    Call IterateTrees([next], graph, treeTable)
                End If
            Next
        End Sub
    End Class
End Namespace