Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataVisualization.Network
Imports Microsoft.VisualBasic.DataVisualization.Network.FileStream
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace KMeans

    <PackageNamespace("KMeans.Tree.NET", Category:=APICategories.ResearchTools, Publisher:="smrucc@gcmodeller.org")>
    Public Module Tree

        ''' <summary>
        ''' 树形聚类
        ''' </summary>
        ''' <param name="resultSet"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("Cluster.Trees")>
        <Extension> Public Function TreeCluster(resultSet As IEnumerable(Of EntityLDM)) As EntityLDM()
            Dim mapNames As String() = resultSet.First.Properties.Keys.ToArray
            Dim ds = resultSet.ToArray(Function(x) New KMeans.Entity With {
                                       .uid = x.Name,
                                       .Properties = mapNames.ToArray(Function(s) x.Properties(s))
                                       })
            Dim tree As KMeans.Entity() = KMeans.TreeCluster(ds)
            Dim saveResult = tree.ToArray(Function(x) x.ToLDM(mapNames))

            For Each name As String In resultSet.ToArray(Function(x) x.Name)
                For Each x In saveResult
                    If InStr(x.Name, name) > 0 Then
                        x.Cluster = x.Name.Replace(name & ".", "")
                        x.Name = name
                        Exit For
                    End If
                Next
            Next

            Return saveResult
        End Function

        <ExportAPI("Cluster.Trees")>
        <Extension> Public Function TreeCluster(source As IEnumerable(Of Entity)) As Entity()
            Return TreeCluster(Of Entity)(source)
        End Function

        Public Function TreeCluster(Of T As Entity)(source As IEnumerable(Of T)) As Entity()
            Return __treeCluster(source, Scan0, source.Count / 2)
        End Function

        Private Function __treeCluster(Of T As Entity)(source As IEnumerable(Of T), depth As Integer, [stop] As Integer) As Entity()
            If source.Count = 2 Then
EXIT_:          Dim array = source.ToArray
                For i As Integer = 0 To array.Length - 1
                    Dim id As String = "." & CStr(i + 1)
                    array(i).uid &= id
                Next
                Return array
            Else
                depth += 1

                If depth >= [stop] Then
                    Dim cluster As T() = source.ToArray

                    For Each x As T In cluster
                        x.uid &= ".X"
                    Next

                    Return cluster
                End If
            End If

            Dim list As New List(Of Entity)
            Dim result As Cluster(Of T)() = ClusterDataSet(2, source).ToArray

            ' 检查数据
            Dim b0 As Boolean = False ', b20 As Boolean = False

            For Each x In result
                If x.NumOfEntity = 0 Then
                    b0 = True
                    'Else
                    '    Dim nl As Integer = 0.75 * source.First.Properties.Count
                    '    If (From c In x.ClusterMean Where c = 0R Select 1).Count >= nl AndAlso
                    '        (From c In x.ClusterSum Where c = 0R Select 1).Count >= nl Then
                    '        b20 = True
                    '    End If
                End If
            Next

            If b0 Then    ' 已经无法再分了，全都是0，则放在一个cluster里面
                'Dim cluster As T() = result.MatrixToVector
                'For Each x In cluster
                '    x.uid &= ".X"
                'Next

                'Call list.Add(cluster)
                Call Console.Write(">")
                Call list.Add(__treeCluster(result.MatrixToList, depth, [stop]))  ' 递归聚类分解
            Else
                For i As Integer = 0 To result.Length - 1
                    Dim cluster = result(i)
                    Dim id As String = "." & CStr(i + 1)

                    For Each x In cluster
                        x.uid &= id
                    Next

                    If cluster.NumOfEntity = 1 Then
                        Call list.Add(cluster.Item(Scan0))
                    ElseIf cluster.NumOfEntity = 0 Then
                        '  不可以取消这可分支，否则会死循环
                    Else
                        Call Console.Write(">")
                        Call list.Add(__treeCluster(cluster.ToArray, depth, [stop]))  ' 递归聚类分解
                    End If
                Next
            End If

            Call Console.Write("<")

            Return list.ToArray
        End Function

        Private Structure __edgePath
            Public path As String()
            Public node As EntityLDM

            Public Overrides Function ToString() As String
                Return $"[{node.Cluster}] --> {node.Name}"
            End Function
        End Structure

        <ExportAPI("Cluster.Trees.Network")>
        <Extension> Public Function TreeNET(source As IEnumerable(Of EntityLDM)) As FileStream.Network
            Dim array = (From x As EntityLDM In source
                         Let path As String() = x.Cluster.Split("."c)
                         Select New __edgePath With {
                             .node = x,
                             .path = path}).ToArray
            Dim nodes = array.ToArray(Function(x) New FileStream.Node With {
                                          .Identifier = x.node.Name,
                                          .NodeType = "Entity",
                                          .Properties = x.node.Properties _
                                                .ToDictionary(Function(xx) xx.Key,
                                                              Function(xx) Math.Round(xx.Value, 4).ToString)
                                          }).ToList
            Dim root As New FileStream.Node With {
                .Identifier = "ROOT",
                .NodeType = "ROOT"
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
        Private Function __buildNET(array As __edgePath(), parent As FileStream.Node, depth As Integer, ByRef nodes As List(Of FileStream.Node)) As NetworkEdge()
            Dim [next] As Integer = depth + 1  ' 下一层节点的深度

            If depth = array(Scan0).path.Length AndAlso
                array(Scan0).path.Last = "X"c Then
                Return array.ToArray(Function(x) New NetworkEdge With {
                                         .FromNode = parent.Identifier,
                                         .ToNode = x.node.Name,
                                         .InteractionType = "Leaf-X"})
            End If

            Dim Gp = (From x In array Let cur = x.path(depth) Select cur, x Group By cur Into Group).ToArray
            Dim edges As New List(Of NetworkEdge)

            For Each part In Gp
                Dim parts = part.Group.ToArray(Function(x) x.x)

                If parts.Length = 1 Then ' 叶节点
                    Dim leaf = parts.First
                    Call edges.Add(New NetworkEdge With {.FromNode = parent.Identifier, .ToNode = leaf.node.Name, .InteractionType = "Leaf"})
                Else                    ' 继续递归
                    Dim uid As String = $"[{part.cur}]" & parts.First.path.Take(depth).JoinBy(".")
                    Dim virtual As New FileStream.Node With {
                        .Identifier = uid,
                        .NodeType = "Virtual"
                    }
                    Call nodes.Add(virtual)
                    Call edges.Add(New NetworkEdge With {.FromNode = parent.Identifier, .ToNode = uid, .InteractionType = "Path"})
                    Call edges.Add(__buildNET(parts, virtual, [next], nodes))
                End If
            Next

            Return edges.ToArray
        End Function
    End Module
End Namespace