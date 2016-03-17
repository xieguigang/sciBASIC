Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace ComponentModel.DataStructures.BinaryTree

    Public Module ClusterParts

        Public Const PATH As String = "Path"
        Public Const LEAF_NODE As String = "Leaf"
        Public Const LEAF_X As String = "Leaf-X"
        Public Const ROOT As String = "ROOT"

        ''' <summary>
        ''' {最开始的节点，实体列表}
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("Cluster.Parts")>
        <Extension>
        Public Function ClusterParts(Of T)(tree As BinaryTree(Of T)) As Dictionary(Of String, String())
            Dim ROOT = tree.DirectFind(BinaryTree.ClusterParts.ROOT)
            Dim hash As Dictionary(Of String, String()) = New Dictionary(Of String, String())
            For Each x In ROOT.GetEnumerator
                Call x.__addCluster(hash)
            Next
            Return hash
        End Function

        <Extension> Private Function __addCluster(Of T)(node As TreeNode(Of T), ByRef hash As Dictionary(Of String, String())) As Dictionary(Of String, String())
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

        <Extension> Private Sub __continuteCluster(Of T)(node As TreeNode(Of T), ByRef hash As Dictionary(Of String, String()), ByRef list As List(Of String))
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
        <Extension> Private Function __hashLeaf(Of T)(node As TreeNode(Of T)) As Boolean
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