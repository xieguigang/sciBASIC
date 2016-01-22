Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream

Namespace KMeans

    Public Module Tree

        <Extension> Public Function TreeCluster(Of T As Entity)(source As IEnumerable(Of T)) As Entity()
            If source.Count = 2 Then
                Dim array = source.ToArray
                For i As Integer = 0 To array.Length - 1
                    Dim id As String = "." & CStr(i + 1)
                    array(i).uid &= id
                Next
                Return array
            End If

            Dim list As New List(Of Entity)
            Dim result As Cluster(Of T)() = ClusterDataSet(2, source).ToArray

            For i As Integer = 0 To result.Length - 1
                Dim cluster = result(i)
                Dim id As String = "." & CStr(i + 1)

                For Each x In cluster
                    x.uid &= id
                Next

                If cluster.NumOfEntity = 1 Then
                    Call list.Add(cluster.Item(Scan0))
                ElseIf cluster.NumOfEntity = 0 Then
                Else
                    Call Console.Write(".")
                    Call list.Add(TreeCluster(cluster.ToArray))  ' 递归聚类分解
                End If
            Next

            Return list.ToArray
        End Function

        Private Class __edgePath
            Public path As String()
            Public node As EntityLDM
        End Class

        <Extension> Public Function TreeNET(source As IEnumerable(Of EntityLDM)) As Network.FileStream.Network
            Dim array = (From x As EntityLDM In source
                         Let path As String() = x.Cluster.Split("."c)
                         Select New __edgePath With {
                             .node = x,
                             .path = path}).ToArray
            Dim nodes = array.ToArray(Function(x) New FileStream.Node With {.Identifier = x.node.Name, .NodeType = "Entity"}).ToList
            Dim root As New FileStream.Node With {
                .Identifier = "ROOT",
                .NodeType = "Virtual"
            }
            Call nodes.Add(root)

            Dim edges = __buildNET(array, root, Scan0, nodes)

            Return New FileStream.Network With {
                .Edges = edges,
                .Nodes = nodes.ToArray
            }
        End Function

        ''' <summary>
        ''' 从某一个分支点下来
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="depth"></param>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Private Function __buildNET(array As __edgePath(), parent As FileStream.Node, depth As Integer, ByRef nodes As List(Of FileStream.Node)) As NetworkNode()
            Dim [next] As Integer = depth + 1  ' 下一层节点的深度
            Dim Gp = (From x In array Let cur = x.path(depth) Select cur, x Group By cur Into Group).ToArray
            Dim edges As New List(Of NetworkNode)

            For Each part In Gp
                Dim parts = part.Group.ToArray(Function(x) x.x)

                If parts.Length = 1 Then ' 叶节点
                    Dim leaf = parts.First
                    Call edges.Add(New NetworkNode With {.FromNode = parent.Identifier, .ToNode = leaf.node.Name, .InteractionType = "Leaf"})
                Else                    ' 继续递归
                    Dim uid As String = $"[{part.cur}]" & parts.First.path.Take(depth).JoinBy(".")
                    Dim virtual As New FileStream.Node With {
                        .Identifier = uid,
                        .NodeType = "Virtual"
                    }
                    Call nodes.Add(virtual)
                    Call edges.Add(New NetworkNode With {.FromNode = parent.Identifier, .ToNode = uid, .InteractionType = "Path"})
                    Call edges.Add(__buildNET(parts, virtual, [next], nodes))
                End If
            Next

            Return edges.ToArray
        End Function
    End Module
End Namespace