Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.BinaryTree

Namespace TreeAPI

    <PackageNamespace("TREE.Cluster")>
    Public Module Operations

        Public Const PATH As String = "Path"
        Public Const LEAF As String = "Leaf"
        Public Const LEAF_X As String = "Leaf-X"
        Public Const ROOT As String = "ROOT"

        <ExportAPI("Tree.Build")>
        <Extension>
        Public Function BuildTree(net As IEnumerable(Of FileStream.NetworkEdge)) As BinaryTree(Of NodeTypes)
            Dim ROOTs = net.GetConnections(ROOT)
            Dim tree As New BinaryTree(Of NodeTypes)(ROOT, NodeTypes.ROOT)
            Dim netList = net.ToList

            For Each node In ROOTs
                Dim Xnext As String = node.GetConnectedNode(ROOT)
                Call netList.Remove(node)
                Call tree.Add(ROOT, New TreeNode(Of NodeTypes)(Xnext, NodeTypes.Path))
                Call __buildTREE(tree, Xnext, netList)
            Next

            Return tree
        End Function

        Private Sub __buildTREE(ByRef tree As BinaryTree(Of NodeTypes), node As String, ByRef netList As List(Of FileStream.NetworkEdge))
            Dim nexts = (From x In netList.GetNextConnects(node) Select x Group x By x.InteractionType Into Group)

            For Each part In nexts
                Dim type = __getTypes(part.InteractionType)
                Dim nextNodes = part.Group.ToArray

                If type <> NodeTypes.Path Then
                    For Each x In nextNodes
                        Call netList.Remove(x)
                    Next

                    If tree.DirectFind(node) Is Nothing Then
                        Call tree.insert(node, NodeTypes.Path)
                    End If
                End If

                If type = NodeTypes.Leaf Then
                    Dim left As Boolean = True
                    Dim Leaf As New Leaf(node)

                    Call tree.Add(node, Leaf)

                    For Each nxode In nextNodes
                        Dim nodeChild As New TreeNode(Of NodeTypes)(nxode.ToNode, NodeTypes.Leaf)
                        Call tree.Add(Leaf.Name, nodeChild, left)
                        left = Not left
                    Next
                ElseIf type = NodeTypes.LeafX Then
                    Dim Xnode As New LeafX(node) With {.LeafX = nextNodes}
                    Call tree.Add(node, Xnode, True)  ' Leaf-X 只有一个，默认为左边
                Else ' 这个是Path，则继续建树
                    For Each nxode As FileStream.NetworkEdge In nextNodes
                        Call netList.Remove(nxode)
                        Call tree.Add(node, New TreeNode(Of NodeTypes)(nxode.ToNode, __getTypes(nxode.InteractionType)))
                        Call __buildTREE(tree, nxode.ToNode, netList)
                    Next
                End If
            Next
        End Sub

        Private ReadOnly __getTypes As Dictionary(Of String, NodeTypes) =
            New Dictionary(Of String, NodeTypes) From {
 _
            {PATH, NodeTypes.Path},
            {LEAF, NodeTypes.Leaf},
            {LEAF_X, NodeTypes.LeafX},
            {ROOT, NodeTypes.ROOT}
        }

        <ExportAPI("GetType")>
        Public Function [GetType](name As String) As NodeTypes
            If __getTypes.ContainsKey(name) Then
                Return __getTypes(name)
            Else
                Return NodeTypes.ROOT
            End If
        End Function

        ''' <summary>
        ''' {最开始的节点，实体列表}
        ''' </summary>
        ''' <param name="net"></param>
        ''' <returns></returns>
        <ExportAPI("Cluster.Parts")>
        Public Function ClusterParts(net As IEnumerable(Of FileStream.NetworkEdge)) As Dictionary(Of String, String())
            Dim tree As BinaryTree(Of NodeTypes) = net.BuildTree
            Dim ROOT = tree.DirectFind(Operations.ROOT)
            Dim hash As Dictionary(Of String, String()) = New Dictionary(Of String, String())
            For Each x In ROOT.GetEnumerator
                Call x.__addCluster(hash)
            Next
            Return hash
        End Function

        <Extension> Private Function __addCluster(node As TreeNode(Of NodeTypes), ByRef hash As Dictionary(Of String, String())) As Dictionary(Of String, String())
            Dim list As New List(Of String)

            For Each x In node.GetEnumerator
                If x.__hashLeaf Then
                    Dim leafs As New List(Of String)
                    Call x.__continuteCluster(hash, leafs)
                    Call list.AddRange(leafs)
                Else
                    Call x.__addCluster(hash)
                End If
            Next

            If list.Count > 0 Then
                Call hash.Add(node.Name, list.Distinct.ToArray)
            End If

            Return hash
        End Function

        <Extension> Private Sub __continuteCluster(node As TreeNode(Of NodeTypes), ByRef hash As Dictionary(Of String, String()), ByRef list As List(Of String))
            For Each x In node.GetEnumerator
                If TypeOf x Is Leaf OrElse TypeOf x Is LeafX Then
                    Dim leafs As String() = DirectCast(x, TreeNode).GetEntities
                    Call list.AddRange(leafs)
                ElseIf x.__hashLeaf Then
                    Call x.__continuteCluster(hash, list)
                Else
                    Call x.__addCluster(hash)
                End If
            Next
        End Sub

        ''' <summary>
        ''' 最远只允许隔着一层Path，这些就可以看作为一个cluster
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        <Extension> Private Function __hashLeaf(node As TreeNode(Of NodeTypes)) As Boolean
            For Each x In node.GetEnumerator
                If TypeOf x Is Leaf OrElse TypeOf x Is LeafX Then
                    Return True
                End If
                'For Each y In x.GetEnumerator
                '    If TypeOf y Is Leaf OrElse TypeOf x Is LeafX Then
                '        Return True
                '    End If
                'Next
            Next
            Return False
        End Function
    End Module
End Namespace